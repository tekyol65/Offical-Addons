using EloBuddy;
using EloBuddy.SDK.Menu;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK;
using Color = System.Drawing.Color;
using PortAIO.Lib;

namespace PortAIO.Champions.Ahri
{
    internal class Ahri
    {
        static Menu _menu, comboMenu, harassMenu, farmMenu, drawMenu, miscMenu;

        private LeagueSharp.Common.Spell _spellQ, _spellW, _spellE, _spellR;

        static float IntroTimer = Game.Time;
        static Render.Sprite Intro;

        const float _spellQSpeed = 2600;
        const float _spellQSpeedMin = 400;
        const float _spellQFarmSpeed = 1600;
        const float _spellQAcceleration = -3200;

        #region menu items
        public static bool comboQ { get { return comboMenu["comboQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool comboW { get { return comboMenu["comboQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool comboE { get { return comboMenu["comboE"].Cast<CheckBox>().CurrentValue; } }
        public static bool comboR { get { return comboMenu["comboR"].Cast<CheckBox>().CurrentValue; } }
        public static bool comboROnlyUserInitiate { get { return comboMenu["comboROnlyUserInitiate"].Cast<CheckBox>().CurrentValue; } }
        public static bool harassQ { get { return harassMenu["harassQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool harassE { get { return harassMenu["harassE"].Cast<CheckBox>().CurrentValue; } }
        public static int harassPercent { get { return harassMenu["harassPercent"].Cast<Slider>().CurrentValue; } }
        public static bool farmQ { get { return farmMenu["farmQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool farmW { get { return farmMenu["farmW"].Cast<CheckBox>().CurrentValue; } }
        public static int farmPercent { get { return farmMenu["farmPercent"].Cast<Slider>().CurrentValue; } }
        public static int farmStartAtLevel { get { return farmMenu["farmStartAtLevel"].Cast<Slider>().CurrentValue; } }
        public static bool drawQE { get { return drawMenu["drawQE"].Cast<CheckBox>().CurrentValue; } }
        public static bool drawW { get { return drawMenu["drawW"].Cast<CheckBox>().CurrentValue; } }
        public static bool DamageAfterCombo { get { return drawMenu["DamageAfterCombo"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoE { get { return miscMenu["autoE"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoEI { get { return miscMenu["autoEI"].Cast<CheckBox>().CurrentValue; } }
        #endregion

        public Ahri()
        {
            if (ObjectManager.Player.ChampionName != "Ahri")
                return;

            _menu = MainMenu.AddMenu("AhriSharp", "AhriSharp");
            
            comboMenu = _menu.AddSubMenu("Combo", "Combo");
            comboMenu.Add("comboQ", new CheckBox("Use Q"));
            comboMenu.Add("comboW", new CheckBox("Use W"));
            comboMenu.Add("comboE", new CheckBox("Use E"));
            comboMenu.Add("comboR", new CheckBox("Use R"));
            comboMenu.Add("comboROnlyUserInitiate", new CheckBox("Use R only if user initiated", false));

            harassMenu = _menu.AddSubMenu("Harass", "Harass");
            harassMenu.Add("harassQ", new CheckBox("Use Q"));
            harassMenu.Add("harassE", new CheckBox("Use E"));
            harassMenu.Add("harassPercent", new Slider("Skills until Mana %", 20));

            farmMenu = _menu.AddSubMenu("Lane Clear", "LaneClear");
            farmMenu.Add("farmQ", new CheckBox("Use Q"));
            farmMenu.Add("farmW", new CheckBox("Use W", false));
            farmMenu.Add("farmPercent", new Slider("Skills until Mana %", 20));
            farmMenu.Add("farmStartAtLevel", new Slider("Only AA until Level", 8, 1, 18));

            drawMenu = _menu.AddSubMenu("Drawing", "Drawing");
            drawMenu.Add("drawQE", new CheckBox("Draw Q, E range"));
            //Color.FromArgb(125, 0, 255, 0)
            drawMenu.Add("drawW", new CheckBox("Draw W range", false));
            //Color.FromArgb(125, 0, 0, 255)
            drawMenu.Add("DamageAfterCombo", new CheckBox("Draw Combo Damage"));

            miscMenu = _menu.AddSubMenu("Misc", "Misc");
            miscMenu.Add("autoE", new CheckBox("Auto E on gapclosing targets"));
            miscMenu.Add("autoEI", new CheckBox("Auto E to interrupt"));

            DamageIndicator.Initialize(GetComboDamage);

            Intro = new Render.Sprite(LoadImg("logo"), new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
            Intro.Add(0);
            Intro.OnDraw();

            LeagueSharp.Common.Utility.DelayAction.Add(7000, () => Intro.Remove());

            _spellQ = new LeagueSharp.Common.Spell(SpellSlot.Q, 1000);
            _spellW = new LeagueSharp.Common.Spell(SpellSlot.W, 795 - 95);
            _spellE = new LeagueSharp.Common.Spell(SpellSlot.E, 1000);
            _spellR = new LeagueSharp.Common.Spell(SpellSlot.R, 1000 - 100);

            _spellQ.SetSkillshot(0.25f, 50, 1600f, false, SkillshotType.SkillshotLine);
            _spellW.SetSkillshot(0.70f, _spellW.Range, float.MaxValue, false, SkillshotType.SkillshotCircle);
            _spellE.SetSkillshot(0.25f, 60, 1550f, true, SkillshotType.SkillshotLine);

            LeagueSharp.Common.AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            LeagueSharp.Common.Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnEndScene += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;

        }

        private static System.Drawing.Bitmap LoadImg(string imgName)
        {
            var bitmap = Properties.Resources.ResourceManager.GetObject(imgName) as System.Drawing.Bitmap;
            if (bitmap == null)
            {
                Console.WriteLine(imgName + ".png not found.");
            }
            return bitmap;
        }

        void AntiGapcloser_OnEnemyGapcloser(LeagueSharp.Common.ActiveGapcloser gapcloser)
        {
            if (!autoE) return;
            if (ObjectManager.Player.LSDistance(gapcloser.Sender, true) < _spellE.Range * _spellE.Range)
            {
                _spellE.Cast(gapcloser.Sender);
            }
        }

        void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!autoEI) return;

            if (ObjectManager.Player.Distance(sender, true) < _spellE.Range * _spellE.Range)
            {
                _spellE.Cast(sender);
            }
        }

        void Game_OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                default:
                    break;
            }

            if(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                LaneClear();
            }
        }

        void Harass()
        {
            if (harassE && ObjectManager.Player.ManaPercent >= harassPercent)
                CastE();

            if (harassQ && ObjectManager.Player.ManaPercent >= harassPercent)
                CastQ();
        }

        void LaneClear()
        {
            _spellQ.Speed = _spellQFarmSpeed;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellQ.Range, MinionTypes.All, MinionTeam.NotAlly);
            bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            if ((farmQ && ObjectManager.Player.ManaPercent >= farmPercent && ObjectManager.Player.Level >= farmStartAtLevel) || jungleMobs)
            {
                MinionManager.FarmLocation farmLocation = _spellQ.GetLineFarmLocation(minions);

                if (farmLocation.Position.IsValid())
                {
                    if (farmLocation.MinionsHit >= 2 || jungleMobs)
                    {
                        CastQ(farmLocation.Position);
                    }
                }
            }

            minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellW.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minions.Count() > 0)
            {
                jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

                if ((farmW && ObjectManager.Player.ManaPercent >= farmPercent && ObjectManager.Player.Level >= farmStartAtLevel) || jungleMobs)
                    CastW(true);
            }
        }

        bool CastE()
        {
            if (!_spellE.IsReady())
            {
                return false;
            }

            var target = TargetSelector.GetTarget(_spellE.Range, DamageType.Magical);

            if (target != null)
            {
                return _spellE.Cast(target) == LeagueSharp.Common.Spell.CastStates.SuccessfullyCasted;
            }

            return false;
        }

        void CastQ()
        {
            if (!_spellQ.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(_spellQ.Range, DamageType.Magical);

            if (target != null)
            {
                var predictedPos = LeagueSharp.Common.Prediction.GetPrediction(target, _spellQ.Delay * 1.5f); //correct pos currently not possible with spell acceleration
                if (predictedPos.Hitchance >= HitChance.High)
                {
                    _spellQ.Speed = GetDynamicQSpeed(ObjectManager.Player.Distance(predictedPos.UnitPosition));
                    if (_spellQ.Speed > 0f)
                    {
                        _spellQ.Cast(target);
                    }
                }
            }
        }

        void CastQ(Vector2 pos)
        {
            if (!_spellQ.IsReady())
                return;

            _spellQ.Cast(pos);
        }

        void CastW(bool ignoreTargetCheck = false)
        {
            if (!_spellW.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(_spellW.Range, DamageType.Magical);

            if (target != null || ignoreTargetCheck)
            {
                _spellW.CastOnUnit(ObjectManager.Player);
            }
        }

        void Combo()
        {
            if (comboE)
            {
                if (CastE())
                {
                    return;
                }
            }

            if (comboQ)
            {
                CastQ();
            }


            if (comboW)
            {
                CastW();
            }


            if (comboR && _spellR.IsReady())
            {
                if (OkToUlt())
                {
                    _spellR.Cast(Game.CursorPos);
                }
            }
        }

        List<SpellSlot> GetSpellCombo()
        {
            var spellCombo = new List<SpellSlot>();

            if (_spellQ.IsReady())
                spellCombo.Add(SpellSlot.Q);
            if (_spellW.IsReady())
                spellCombo.Add(SpellSlot.W);
            if (_spellE.IsReady())
                spellCombo.Add(SpellSlot.E);
            if (_spellR.IsReady())
                spellCombo.Add(SpellSlot.R);
            return spellCombo;
        }

        float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = (float)ObjectManager.Player.GetComboDamage(target, GetSpellCombo());

            return (float)(comboDamage + ObjectManager.Player.GetAutoAttackDamage(target));
        }

        bool OkToUlt()
        {
            if (Program.Helper.EnemyTeam.Any(x => x.Distance(ObjectManager.Player) < 500)) //any enemies around me?
                return true;

            Vector3 mousePos = Game.CursorPos;

            var enemiesNearMouse = Program.Helper.EnemyTeam.Where(x => x.Distance(ObjectManager.Player) < _spellR.Range && x.Distance(mousePos) < 650);

            if (enemiesNearMouse.Count() > 0)
            {
                if (IsRActive()) //R already active
                    return true;

                bool enoughMana = ObjectManager.Player.Mana > ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.Mana + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).SData.Mana + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).SData.Mana;

                if (comboROnlyUserInitiate || !(_spellQ.IsReady() && _spellE.IsReady()) || !enoughMana) //dont initiate if user doesnt want to, also dont initiate if Q and E isnt ready or not enough mana for QER combo
                    return false;

                var friendsNearMouse = Program.Helper.OwnTeam.Where(x => x.IsMe || x.Distance(mousePos) < 650); //me and friends near mouse (already in fight)

                if (enemiesNearMouse.Count() == 1) //x vs 1 enemy
                {
                    AIHeroClient enemy = enemiesNearMouse.FirstOrDefault();

                    bool underTower = LeagueSharp.Common.Utility.UnderTurret(enemy);

                    return GetComboDamage(enemy) / enemy.Health >= (underTower ? 1.25f : 1); //if enemy under tower, only initiate if combo damage is >125% of enemy health
                }
                else //fight if enemies low health or 2 friends vs 3 enemies and 3 friends vs 3 enemies, but not 2vs4
                {
                    int lowHealthEnemies = enemiesNearMouse.Count(x => x.Health / x.MaxHealth <= 0.1); //dont count low health enemies

                    float totalEnemyHealth = enemiesNearMouse.Sum(x => x.Health);

                    return friendsNearMouse.Count() - (enemiesNearMouse.Count() - lowHealthEnemies) >= -1 || ObjectManager.Player.Health / totalEnemyHealth >= 0.8;
                }
            }

            return false;
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                if (drawQE)
                    LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, Color.FromArgb(125, 0, 255, 0));

                if (drawW)
                    LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, _spellW.Range, Color.FromArgb(125, 0, 0, 255));

                DamageIndicator.HealthbarEnabled = DamageAfterCombo;
            }
        }

        float GetDynamicQSpeed(float distance)
        {
            var a = 0.5f * _spellQAcceleration;
            var b = _spellQSpeed;
            var c = -distance;

            if (b * b - 4 * a * c <= 0f)
            {
                return 0;
            }

            var t = (float)(-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            return distance / t;
        }

        bool IsRActive()
        {
            return ObjectManager.Player.HasBuff("AhriTumble");
        }

        int GetRStacks()
        {
            return ObjectManager.Player.GetBuffCount("AhriTumble");
        }
    }
}
