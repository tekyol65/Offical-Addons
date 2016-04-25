using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Linq;
using ItemData = LeagueSharp.Common.Data.ItemData;
using Color = System.Drawing.Color;
using PortAIO.Lib;

namespace PortAIO.Champions.Lucian
{
    public class Program
    {
        private static Menu Menu, Combo, Misc, Harass, LC, JC, Auto, Draw, killsteal;
        private static AIHeroClient Player = ObjectManager.Player;
        private static LeagueSharp.Common.Spell Q, Q1, W, E, R;

        private static bool AAPassive;
        private static bool HEXQ => Harass["HEXQ"].Cast<CheckBox>().CurrentValue;
        private static bool KillstealQ => killsteal["KillstealQ"].Cast<CheckBox>().CurrentValue;
        private static bool CQ => Combo["CQ"].Cast<CheckBox>().CurrentValue;
        private static bool CW => Combo["CW"].Cast<CheckBox>().CurrentValue;
        private static int CE => Combo["CE"].Cast<ComboBox>().CurrentValue;
        private static bool HQ => Harass["HQ"].Cast<CheckBox>().CurrentValue;
        private static bool HW => Harass["HW"].Cast<CheckBox>().CurrentValue;
        private static int HE => Harass["HE"].Cast<ComboBox>().CurrentValue;
        private static int HMinMana => Harass["HMinMana"].Cast<Slider>().CurrentValue;
        private static bool JQ => JC["JQ"].Cast<CheckBox>().CurrentValue;
        private static bool JW => JC["JW"].Cast<CheckBox>().CurrentValue;
        private static bool JE => JC["JE"].Cast<CheckBox>().CurrentValue;
        private static bool LHQ => LC["LHQ"].Cast<CheckBox>().CurrentValue;
        private static int LQ => LC["LQ"].Cast<Slider>().CurrentValue;
        private static bool LW => LC["LW"].Cast<CheckBox>().CurrentValue;
        private static bool LE => LC["LE"].Cast<CheckBox>().CurrentValue;
        private static int LMinMana => LC["LMinMana"].Cast<Slider>().CurrentValue;
        private static bool Dind => Draw["Dind"].Cast<CheckBox>().CurrentValue;
        private static bool DEQ => Draw["DEQ"].Cast<CheckBox>().CurrentValue;
        private static bool DQ => Draw["DQ"].Cast<CheckBox>().CurrentValue;
        private static bool DW => Draw["DW"].Cast<CheckBox>().CurrentValue;
        private static bool DE => Draw["DE"].Cast<CheckBox>().CurrentValue;
        static bool AutoQ => Auto["AutoQ"].Cast<KeyBind>().CurrentValue;
        private static int MinMana => Auto["MinMana"].Cast<Slider>().CurrentValue;
        private static int HHMinMana => Misc["HHMinMana"].Cast<Slider>().CurrentValue;
        private static int Humanizer => Misc["Humanizer"].Cast<Slider>().CurrentValue;
        static bool ForceR => Combo["ForceR"].Cast<KeyBind>().CurrentValue;
        static bool LT => LC["LT"].Cast<KeyBind>().CurrentValue;

        public static void OnGameLoad()
        {
            Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 675);
            Q1 = new LeagueSharp.Common.Spell(SpellSlot.Q, 1200);
            W = new LeagueSharp.Common.Spell(SpellSlot.W, 1200, DamageType.Magical);
            E = new LeagueSharp.Common.Spell(SpellSlot.E, 475f);
            R = new LeagueSharp.Common.Spell(SpellSlot.R, 1400);

            OnMenuLoad();

            Q.SetTargetted(0.25f, 1400f);
            Q1.SetSkillshot(0.5f, 50, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.30f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.2f, 110f, 2500, true, SkillshotType.SkillshotLine);

            DamageIndicator.Initialize(getComboDamage);

            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Game.OnUpdate += Game_OnTick; ;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Obj_AI_Base.OnSpellCast += OnDoCast;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnSpellCast += OnDoCastLC;
        }

        

        private static void OnMenuLoad()
        {
            Menu = MainMenu.AddMenu("Hoola Lucian", "hoolalucian");
            
            Combo = Menu.AddSubMenu("Combo", "Combo");
            Combo.Add("CQ", new CheckBox("Use Q"));
            Combo.Add("CW", new CheckBox("Use W"));
            Combo.Add("CE", new ComboBox("Use E Mode", 0,"Side", "Cursor", "Enemy", "Never"));
            Combo.Add("ForceR", new KeyBind("Force R On Target Selector", false, KeyBind.BindTypes.HoldActive, 'T'));

            Misc = Menu.AddSubMenu("Misc", "Misc");
            Misc.Add("Humanizer", new Slider("Humanizer Delay", 5, 5, 300));
            Misc.Add("Nocolision", new CheckBox("Nocolision W"));


            Harass = Menu.AddSubMenu("Harass", "Harass");
            Harass.Add("HEXQ", new CheckBox("Use Extended Q"));
            Harass.Add("HMinMana", new Slider("Extended Q Min Mana (%)", 80));
            Harass.Add("HQ", new CheckBox("Use Q"));
            Harass.Add("HW", new CheckBox("Use W"));
            Harass.Add("HE", new ComboBox("Use E Mode", 0, "Side", "Cursor", "Enemy", "Never"));
            Harass.Add("HHMinMana", new Slider("Harass Min Mana (%)", 80));

            LC = Menu.AddSubMenu("LaneClear", "LaneClear");
            LC.Add("LT", new KeyBind("Use Spell LaneClear (Toggle)", false, KeyBind.BindTypes.PressToggle, 'J'));
            LC.Add("LHQ", new CheckBox("Use Extended Q For Harass"));
            LC.Add("LQ", new Slider("Use Q (0 = Don't)", 0, 0, 5));
            LC.Add("LW", new CheckBox("Use W", false));
            LC.Add("LE", new CheckBox("Use E", false));
            LC.Add("LMinMana", new Slider("Min Mana (%)", 80));

            JC = Menu.AddSubMenu("JungleClear", "JungleClear");
            JC.Add("JQ", new CheckBox("Use Q"));
            JC.Add("JW", new CheckBox("Use W"));
            JC.Add("JE", new CheckBox("Use E"));

            Auto = Menu.AddSubMenu("Auto", "Auto");
            Auto.Add("AutoQ", new KeyBind("Auto Extended Q (Toggle)", false, KeyBind.BindTypes.PressToggle, 'G'));
            Auto.Add("MinMana", new Slider("Min Mana (%)", 80));

            Draw = Menu.AddSubMenu("Draw", "Draw");
            Draw.Add("Dind", new CheckBox("Draw Damage Incidator"));
            Draw.Add("DEQ", new CheckBox("Draw Extended Q", false));
            Draw.Add("DQ", new CheckBox("Draw Q", false));
            Draw.Add("DW", new CheckBox("Draw W", false));
            Draw.Add("DE", new CheckBox("Draw E", false));

            killsteal = Menu.AddSubMenu("killsteal", "Killsteal");
            killsteal.Add("KillstealQ", new CheckBox("Killsteal Q"));
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spellName = args.SData.Name;
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(spellName)) return;

            if (args.Target is AIHeroClient)
            {
                var target = (Obj_AI_Base)args.Target;
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target.IsValid)
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(Humanizer, () => OnDoCastDelayed(args));
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && target.IsValid)
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(Humanizer, () => OnDoCastDelayed(args));
                }
            }
            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && args.Target.IsValid)
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(Humanizer, () => OnDoCastDelayed(args));
                }
            }
        }
        private static void OnDoCastLC(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spellName = args.SData.Name;
            if (!sender.IsMe || !Orbwalking.IsAutoAttack(spellName)) return;

            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && args.Target.IsValid)
                {
                    LeagueSharp.Common.Utility.DelayAction.Add(Humanizer, () => OnDoCastDelayedLC(args));
                }
            }
        }

        static void killsteal1()
        {
            if (KillstealQ && Q.IsReady())
            {
                var targets = HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Q.GetDamage(target) && (!target.HasBuff("kindrednodeathbuff") && !target.HasBuff("Undying Rage") && !target.HasBuff("JudicatorIntervention")))
                        Q.Cast(target);
                }
            }
        }
        private static void OnDoCastDelayedLC(GameObjectProcessSpellCastEventArgs args)
        {
            AAPassive = false;
            if (args.Target is Obj_AI_Minion && args.Target.IsValid)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.ManaPercent > LMinMana)
                {
                    var Minions = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                    if (Minions[0].IsValid && Minions.Count != 0)
                    {
                        if (!LT)
                        {
                            return;
                        }

                        if (E.IsReady() && !AAPassive && LE)
                        {
                            E.Cast(Player.Position.Extend(Game.CursorPos, 70));
                        }
                        if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && !LE)) && LQ != 0 && !AAPassive)
                        {
                            var QMinions = MinionManager.GetMinions(Q.Range);
                            var exminions = MinionManager.GetMinions(Q1.Range);
                            foreach (var Minion in QMinions)
                            {
                                var QHit = new EloBuddy.SDK.Geometry.Polygon.Rectangle(Player.Position, Player.Position.LSExtend(Minion.Position, Q1.Range), Q1.Width);
                                if (exminions.Count(x => !QHit.IsOutside(x.Position.LSTo2D())) >= LQ)
                                {
                                    Q.Cast(Minion);
                                    break;
                                }
                            }
                        }
                        if ((!E.IsReady() || (E.IsReady() && !LE)) && (!Q.IsReady() || (Q.IsReady() && LQ == 0)) && LW && W.IsReady() && !AAPassive)
                        {
                            W.Cast(Minions[0].Position);
                        }
                    }
                }
            }
        }
        public static Vector2 Deviation(Vector2 point1, Vector2 point2, double angle)
        {
            angle *= Math.PI / 180.0;
            Vector2 temp = Vector2.Subtract(point2, point1);
            Vector2 result = new Vector2(0);
            result.X = (float)(temp.X * Math.Cos(angle) - temp.Y * Math.Sin(angle)) / 4;
            result.Y = (float)(temp.X * Math.Sin(angle) + temp.Y * Math.Cos(angle)) / 4;
            result = Vector2.Add(result, point1);
            return result;
        }
        private static void OnDoCastDelayed(GameObjectProcessSpellCastEventArgs args)
        {
            AAPassive = false;
            if (args.Target is AIHeroClient)
            {
                var target = (Obj_AI_Base)args.Target;
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target.IsValid)
                {
                    if (ItemData.Youmuus_Ghostblade.GetItem().IsReady()) ItemData.Youmuus_Ghostblade.GetItem().Cast();
                    if (E.IsReady() && !AAPassive && CE == 0) E.Cast((Deviation(Player.Position.LSTo2D(), target.Position.LSTo2D(), 65).To3D()));
                    if (E.IsReady() && !AAPassive && CE == 1) E.Cast(Game.CursorPos);
                    if (E.IsReady() && !AAPassive && CE == 2) E.Cast(Player.Position.Extend(target.Position, 50));
                    if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && CE == 3)) && CQ && !AAPassive) Q.Cast(target);
                    if ((!E.IsReady() || (E.IsReady() && CE == 3)) && (!Q.IsReady() || (Q.IsReady() && !CQ)) && CW && W.IsReady() && !AAPassive) W.Cast(target.Position);
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && target.IsValid)
                {
                    if (Player.ManaPercent < HHMinMana) return;

                    if (E.IsReady() && !AAPassive && HE == 0) E.Cast((Deviation(Player.Position.LSTo2D(), target.Position.LSTo2D(), 65).To3D()));
                    if (E.IsReady() && !AAPassive && HE == 1) E.Cast(Player.Position.Extend(Game.CursorPos, 50));
                    if (E.IsReady() && !AAPassive && HE == 2) E.Cast(Player.Position.Extend(target.Position, 50));
                    if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && HE == 3)) && HQ && !AAPassive) Q.Cast(target);
                    if ((!E.IsReady() || (E.IsReady() && HE == 3)) && (!Q.IsReady() || (Q.IsReady() && !HQ)) && HW && W.IsReady() && !AAPassive) W.Cast(target.Position);
                }
            }
            if (args.Target is Obj_AI_Minion && args.Target.IsValid)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    var Mobs = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                    if (Mobs[0].IsValid && Mobs.Count != 0)
                    {
                        if (E.IsReady() && !AAPassive && JE) E.Cast(Player.Position.Extend(Game.CursorPos, 70));
                        if (Q.IsReady() && (!E.IsReady() || (E.IsReady() && !JE)) && JQ && !AAPassive) Q.Cast(Mobs[0]);
                        if ((!E.IsReady() || (E.IsReady() && !JE)) && (!Q.IsReady() || (Q.IsReady() && !JQ)) && JW && W.IsReady() && !AAPassive) W.Cast(Mobs[0].Position);
                    }
                }
            }
        }

        private static void Harass1()
        {
            if (Player.ManaPercent < HMinMana) return;

            if (Q.IsReady() && HEXQ)
            {
                var target = TargetSelector.GetTarget(Q1.Range, DamageType.Physical);
                var Minions = MinionManager.GetMinions(Q.Range);
                foreach (var Minion in Minions)
                {
                    var QHit = new EloBuddy.SDK.Geometry.Polygon.Rectangle(Player.Position, Player.Position.LSExtend(Minion.Position, Q1.Range), Q1.Width);
                    var QPred = Q1.GetPrediction(target);
                    if (!QHit.IsOutside(QPred.UnitPosition.LSTo2D()) && QPred.Hitchance == HitChance.High)
                    {
                        Q.Cast(Minion);
                        break;
                    }
                }
            }
        }
        static void LaneClear()
        {
            if (Player.ManaPercent < LMinMana) return;

            if (Q.IsReady() && LHQ)
            {
                var extarget = TargetSelector.GetTarget(Q1.Range, DamageType.Physical);
                var Minions = MinionManager.GetMinions(Q.Range);
                foreach (var Minion in Minions)
                {
                    var QHit = new LeagueSharp.Common.Geometry.Polygon.Rectangle(Player.Position, Player.Position.LSExtend(Minion.Position, Q1.Range), Q1.Width);
                    var QPred = Q1.GetPrediction(extarget);
                    if (!QHit.IsOutside(QPred.UnitPosition.LSTo2D()) && QPred.Hitchance == HitChance.High)
                    {
                        Q.Cast(Minion);
                        break;
                    }
                }
            }
        }
        static void AutoUseQ()
        {
            if (Q.IsReady() && AutoQ && Player.ManaPercent > MinMana)
            {
                var extarget = TargetSelector.GetTarget(Q1.Range, DamageType.Physical);
                var Minions = MinionManager.GetMinions(Q.Range);
                foreach (var Minion in Minions)
                {
                    var QHit = new LeagueSharp.Common.Geometry.Polygon.Rectangle(Player.Position, (Vector3)Player.Position.Extend(Minion.Position, Q1.Range), Q1.Width);
                    var QPred = Q1.GetPrediction(extarget);
                    if (!QHit.IsOutside(QPred.UnitPosition.LSTo2D()) && QPred.Hitchance == HitChance.High)
                    {
                        Q.Cast(Minion);
                        break;
                    }
                }
            }
        }

        static void UseRTarget()
        {
            if (Player.HasBuff("LucianR"))
            {
                return;
            }
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (ForceR && R.IsReady() && target.IsValid && target is AIHeroClient && !Player.HasBuff("LucianR")) R.Cast(target.Position);
        }

        private static void Game_OnTick(EventArgs args)
        {
            W.Collision = Misc["Nocolision"].Cast<CheckBox>().CurrentValue;

            AutoUseQ();

            if (ForceR)
            {
                UseRTarget();
            }

            killsteal1();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass1();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
        }

        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E) AAPassive = true;
            if (args.Slot == SpellSlot.E) Orbwalker.ResetAutoAttack();
            if (args.Slot == SpellSlot.R) ItemData.Youmuus_Ghostblade.GetItem().Cast();
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                if (E.IsReady()) damage = damage + (float)Player.GetAutoAttackDamage(enemy) * 2;
                if (W.IsReady()) damage = damage + W.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy);
                if (Q.IsReady())
                {
                    damage = damage + Q.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy);
                }
                damage = damage + (float)Player.GetAutoAttackDamage(enemy);

                return damage;
            }
            return 0;
        }

        static void OnDraw(EventArgs args)
        {
            if (DEQ) Render.Circle.DrawCircle(Player.Position, Q1.Range, Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
            if (DQ) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
            if (DW) Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.LimeGreen : Color.IndianRed);
            if (DE) Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.LimeGreen : Color.IndianRed);
        }
        static void Drawing_OnEndScene(EventArgs args)
        {
            foreach (
                var enemy in
                    ObjectManager.Get<AIHeroClient>()
                        .Where(ene => ene.IsEnemy && ene.IsValidTarget() && !ene.IsZombie))
            {
                DamageIndicator.DrawingColor = Color.YellowGreen;
                DamageIndicator.HealthbarEnabled = Dind;

            }
            
        }
    }
}
