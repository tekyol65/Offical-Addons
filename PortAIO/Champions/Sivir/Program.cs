using System;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using SharpDX;
using SebbyLib;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;

namespace PortAIO.Champions.Sivir
{
    class Program
    {
        public static int tickIndex = 0;
        private static Menu Config,draw,farm,harass,shield;
        public static LeagueSharp.Common.Spell E, Q, Qc, W, R;
        public static float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;

        public static List<AIHeroClient> Enemies = new List<AIHeroClient>(), Allies = new List<AIHeroClient>();
        public static AIHeroClient Player { get { return ObjectManager.Player; } }


        public static Core.MissileReturn missileManager;

        #region menu items
        public static bool notif { get { return draw["notif"].Cast<CheckBox>().CurrentValue; } }
        public static bool noti { get { return draw["noti"].Cast<CheckBox>().CurrentValue; } }
        public static bool qRange { get { return draw["qRange"].Cast<CheckBox>().CurrentValue; } }
        public static bool onlyRdy { get { return draw["onlyRdy"].Cast<CheckBox>().CurrentValue; } }
        public static bool farmQ { get { return farm["farmQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool farmW { get { return farm["farmW"].Cast<CheckBox>().CurrentValue; } }
        public static int Mana { get { return farm["Mana"].Cast<Slider>().CurrentValue; } }
        public static int LCminions { get { return farm["LCminions"].Cast<Slider>().CurrentValue; } }
        public static bool jungleQ { get { return farm["jungleQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool jungleW { get { return farm["jungleW"].Cast<CheckBox>().CurrentValue; } }
        public static bool harasW { get { return harass["harasW"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoR { get { return Config["autoR"].Cast<CheckBox>().CurrentValue; } }
        public static bool manaDisable { get { return Config["manaDisable"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoE { get { return shield["autoE"].Cast<CheckBox>().CurrentValue; } }
        public static bool AGC { get { return shield["AGC"].Cast<CheckBox>().CurrentValue; } }
        public static int Edmg { get { return shield["Edmg"].Cast<Slider>().CurrentValue; } }        
        #endregion


        public static void LoadOKTW()
        {
            Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 1200f);
            Qc = new LeagueSharp.Common.Spell(SpellSlot.Q, 1200f);
            W = new LeagueSharp.Common.Spell(SpellSlot.W, float.MaxValue);
            E = new LeagueSharp.Common.Spell(SpellSlot.E, float.MaxValue);
            R = new LeagueSharp.Common.Spell(SpellSlot.R, 25000f);

            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);
            Qc.SetSkillshot(0.25f, 90f, 1350f, true, SkillshotType.SkillshotLine);

            missileManager = new Core.MissileReturn("SivirQMissile", "SivirQMissileReturn", Q);

            Config = MainMenu.AddMenu("Sivir OKTW", "sivir");

            draw = Config.AddSubMenu("Draw", "draw");
            draw.Add("notif", new CheckBox("Notification (timers)"));
            draw.Add("noti", new CheckBox("Show KS notification"));
            draw.Add("qRange", new CheckBox("Q range"));
            draw.Add("onlyRdy", new CheckBox("Draw only ready spells"));

            farm = Config.AddSubMenu("Farm", "Farm");
            farm.Add("farmQ", new CheckBox("Lane clear Q"));
            farm.Add("farmW", new CheckBox("Lane clear W"));
            farm.Add("Mana", new Slider("LaneClear Mana", 80));
            farm.Add("LCminions", new Slider("LaneClear minimum minions", 5, 0, 10));
            farm.Add("jungleQ", new CheckBox("Jungle clear Q"));
            farm.Add("jungleW", new CheckBox("Jungle clear W"));

            harass = Config.AddSubMenu("Harass", "Harass");
            harass.Add("harasW", new CheckBox("Harras W"));
            harass.AddLabel(" ::Champions");
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.Team != Player.Team))
                harass.Add("haras" + enemy.ChampionName, new CheckBox(enemy.ChampionName));

            shield = Config.AddSubMenu("Shield", "sheild");
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy))
            {
                shield.AddGroupLabel(enemy.ChampionName);
                for (int i = 0; i < 4; i++)
                {
                    var spell = enemy.Spellbook.Spells[i];
                    if (spell.SData.TargettingType != SpellDataTargetType.Self && spell.SData.TargettingType != SpellDataTargetType.SelfAndUnit)
                    {
                        if (spell.SData.TargettingType == SpellDataTargetType.Unit)
                        {
                            
                            shield.Add("spell" + spell.SData.Name, new CheckBox(spell.SData.Name));
                        }
                        else
                        {
                            shield.Add("spell" + spell.SData.Name, new CheckBox(spell.SData.Name, false));
                        }
                    }
                }
            }

            Config.Add("autoR", new CheckBox("Auto R"));
            Config.Add("manaDisable", new CheckBox("Mana Disable", false));

            shield.AddSeparator();
            shield.Add("autoE", new CheckBox("Auto E"));
            shield.Add("AGC", new CheckBox("AntiGapcloserE"));
            shield.Add("Edmg", new Slider("Block under % hp", 90));

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPostAttack += Orbwalker_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        public static void Orbwalker_AfterAttack(AttackableUnit target, EventArgs args)
        {
            if (W.IsReady() && target.IsValidTarget())
            {
                if (target is AIHeroClient)
                {
                    var t = target as AIHeroClient;
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Player.Mana > RMANA + WMANA)
                    {
                        W.Cast();
                    }
                    else if (harasW && !Player.UnderTurret(true) && Player.Mana > RMANA + WMANA + QMANA && harass["haras" + t.ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        W.Cast();
                    }
                }
                else
                {
                    var t = TargetSelector.GetTarget(900, DamageType.Physical);
                    if (t.IsValidTarget() && harasW && harass["haras" + t.ChampionName].Cast<CheckBox>().CurrentValue && !Player.UnderTurret(true) && Player.Mana > RMANA + WMANA + QMANA && t.Distance(target.Position) < 500)
                    {
                        W.Cast();
                    }

                    if (target is Obj_AI_Minion && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) && farmW && Player.ManaPercent > Mana && !Player.UnderTurret(true))
                    {
                        var minions = Cache.GetMinions(target.Position, 500);
                        if (minions.Count >= LCminions)
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!E.IsReady() || args.SData.IsAutoAttack() || Player.HealthPercent > Edmg || !autoE
                || !sender.IsEnemy || sender.IsMinion || !sender.IsValid<AIHeroClient>() || args.SData.Name.ToLower() == "tormentedsoil")
                return;

            if (shield["spell" + args.SData.Name] != null && !shield["spell" + args.SData.Name].Cast<CheckBox>().CurrentValue)
                return;

            if (args.Target != null)
            {
                if (args.Target.IsMe)
                    E.Cast();
            }
            else if (OktwCommon.CanHitSkillShot(Player, args))
            {
                E.Cast();
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var Target = gapcloser.Sender;
            if (AGC && E.IsReady() && Target.IsValidTarget(5000))
                E.Cast();
            return;
        }

        public static bool LagFree(int offset)
        {
            if (tickIndex == offset)
                return true;
            else
                return false;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            tickIndex++;

            if (tickIndex > 4)
                tickIndex = 0;

            if (Program.LagFree(0))
            {
                SetMana();
            }

            if (Program.LagFree(1) && Q.IsReady())
            {
                LogicQ();
            }

            if (Program.LagFree(2) && R.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && autoR)
            {
                LogicR();
            }

            if (Program.LagFree(3) && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)))
            {
                Jungle();
            }
        }

        public static void CastSpell(LeagueSharp.Common.Spell QWER, Obj_AI_Base target)
        {
            SebbyLib.Prediction.SkillshotType CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotLine;
            bool aoe2 = false;

            if (QWER.Type == SkillshotType.SkillshotCircle)
            {
                CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotCircle;
                aoe2 = true;
            }

            if (QWER.Width > 80 && !QWER.Collision)
                aoe2 = true;

            var predInput2 = new SebbyLib.Prediction.PredictionInput
            {
                Aoe = aoe2,
                Collision = QWER.Collision,
                Speed = QWER.Speed,
                Delay = QWER.Delay,
                Range = QWER.Range,
                From = Player.ServerPosition,
                Radius = QWER.Width,
                Unit = target,
                Type = CoreType2
            };
            var poutput2 = SebbyLib.Prediction.Prediction.GetPrediction(predInput2);

            //var poutput2 = QWER.GetPrediction(target);

            if (QWER.Speed != float.MaxValue && OktwCommon.CollisionYasuo(Player.ServerPosition, poutput2.CastPosition))
                return;



            if (poutput2.Hitchance >= SebbyLib.Prediction.HitChance.High)
                QWER.Cast(poutput2.CastPosition);



        }
        

        private static void LogicQ()
        {
            var t = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (t.IsValidTarget())
            {
                missileManager.Target = t;
                var qDmg = OktwCommon.GetKsDamage(t, Q) * 1.9;
                if (SebbyLib.Orbwalking.InAutoAttackRange(t))
                    qDmg = qDmg + Player.GetAutoAttackDamage(t) * 3;
                if (qDmg > t.Health)
                    Q.Cast(t, true);
                else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Player.Mana > RMANA + QMANA)
                    Program.CastSpell(Q, t);
                else if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) && harass["haras" + t.ChampionName].Cast<CheckBox>().CurrentValue && !Player.UnderTurret(true))
                {
                    if (Player.Mana > Player.MaxMana * 0.9)
                        Program.CastSpell(Q, t);
                    else if (ObjectManager.Player.Mana > RMANA + WMANA + QMANA + QMANA)
                        Program.CastSpell(Qc, t);
                    else if (Player.Mana > RMANA + WMANA + QMANA + QMANA)
                    {
                        Q.CastIfWillHit(t, 2, true);
                        if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)))
                            Program.CastSpell(Q, t);
                    }
                }
                if (Player.Mana > RMANA + WMANA)
                {
                    foreach (var enemy in Program.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && !OktwCommon.CanMove(enemy)))
                        Q.Cast(enemy);
                }
            }
            else if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) && Player.ManaPercent > Mana && farmQ && Player.Mana > RMANA + QMANA)
            {
                var minionList = Cache.GetMinions(Player.ServerPosition, Q.Range);
                var farmPosition = Q.GetLineFarmLocation(minionList, Q.Width);
                if (farmPosition.MinionsHit >= LCminions)
                    Q.Cast(farmPosition.Position);
            }
        }

        private static void LogicR()
        {
            var t = TargetSelector.GetTarget(800, DamageType.Physical);
            if (Player.CountEnemiesInRange(800f) > 2)
                R.Cast();
            else if (t.IsValidTarget() && Orbwalker.ForcedTarget == null && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Player.GetAutoAttackDamage(t) * 2 > t.Health && !Q.IsReady() && t.CountEnemiesInRange(800) < 3)
                R.Cast();
        }

        private static void Jungle()
        {
            if (Player.Mana > RMANA + WMANA + RMANA)
            {
                var mobs = Cache.GetMinions(ObjectManager.Player.ServerPosition, 600, MinionTeam.Neutral);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    if (W.IsReady() && jungleW)
                    {
                        W.Cast();
                        return;
                    }
                    if (Q.IsReady() && jungleQ)
                    {
                        Q.Cast(mob);
                        return;
                    }
                }
            }
        }

        private static void SetMana()
        {
            if ((manaDisable && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) || Player.HealthPercent < 20)
            {
                QMANA = 0;
                WMANA = 0;
                EMANA = 0;
                RMANA = 0;
                return;
            }

            QMANA = Q.Instance.SData.Mana;
            WMANA = W.Instance.SData.Mana;
            EMANA = E.Instance.SData.Mana;

            if (!R.IsReady())
                RMANA = QMANA - Player.PARRegenRate * Q.Instance.Cooldown;
            else
                RMANA = R.Instance.SData.Mana;
        }

        public static void drawText2(string msg, Vector3 Hero, int high, System.Drawing.Color color)
        {
            var wts = Drawing.WorldToScreen(Hero);
            Drawing.DrawText(wts[0] - (msg.Length) * 5, wts[1] - high, color, msg);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (notif)
            {
                if (Player.HasBuff("sivirwmarker"))
                {
                    var color = System.Drawing.Color.Yellow;
                    var buffTime = OktwCommon.GetPassiveTime(Player, "sivirwmarker");
                    if (buffTime < 1)
                        color = System.Drawing.Color.Red;
                    drawText2("W:  " + String.Format("{0:0.0}", buffTime), Player.Position, 175, color);
                }
                if (Player.HasBuff("SivirE"))
                {
                    var color = System.Drawing.Color.Aqua;
                    var buffTime = OktwCommon.GetPassiveTime(Player, "SivirE");
                    if (buffTime < 1)
                        color = System.Drawing.Color.Red;
                    drawText2("E:  " + String.Format("{0:0.0}", buffTime), Player.Position, 200, color);
                }
                if (Player.HasBuff("SivirR"))
                {
                    var color = System.Drawing.Color.GreenYellow;
                    var buffTime = OktwCommon.GetPassiveTime(Player, "SivirR");
                    if (buffTime < 1)
                        color = System.Drawing.Color.Red;
                    drawText2("R:  " + String.Format("{0:0.0}", buffTime), Player.Position, 225, color);
                }
            }

            if (qRange)
            {
                if (onlyRdy)
                {
                    if (Q.IsReady())
                        LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
                }
                else
                    LeagueSharp.Common.Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Cyan, 1, 1);
            }

            if (noti)
            {
                var target = TargetSelector.GetTarget(1500, DamageType.Physical);
                if (target.IsValidTarget())
                {
                    if (Q.GetDamage(target) * 2 > target.Health)
                    {
                        Render.Circle.DrawCircle(target.ServerPosition, 200, System.Drawing.Color.Red);
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.4f, System.Drawing.Color.Red, "Q kill: " + target.ChampionName + " have: " + target.Health + "hp");
                    }
                }
            }
        }
    }
}
