﻿using System;
using System.Linq;
using System.Collections.Generic;
using EloBuddy;
using LeagueSharp.Common;
using SPrediction;
using SharpDX;
using Color = System.Drawing.Color;
using Collision = LeagueSharp.Common.Collision;
using Spell = LeagueSharp.Common.Spell;
using Utility = LeagueSharp.Common.Utility;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK;
using Kalista.Lib;

namespace Kalista
{
    class Program
    {
        static AIHeroClient Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static Menu Menu,draw, combo, harass, laneclear, jungleclear, misc;
        static float getManaPer { get { return Player.Mana / Player.MaxMana * 100; } }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        #region menu items
        public static bool comboUseQ { get { return Menu["comboUseQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool comboUseE { get { return Menu["comboUseE"].Cast<CheckBox>().CurrentValue; } }
        public static bool harassUseQ { get { return Menu["harassUseQ"].Cast<CheckBox>().CurrentValue; } }
        public static int harassMana { get { return Menu["harassMana"].Cast<Slider>().CurrentValue; } }
        public static bool laneclearUseQ { get { return Menu["laneclearUseQ"].Cast<CheckBox>().CurrentValue; } }
        public static int laneclearQnum { get { return Menu["laneclearQnum"].Cast<Slider>().CurrentValue; } }
        public static bool laneclearUseE { get { return Menu["laneclearUseE"].Cast<CheckBox>().CurrentValue; } }
        public static int laneclearEnum { get { return Menu["laneclearEnum"].Cast<Slider>().CurrentValue; } }
        public static int laneclearMana { get { return Menu["laneclearMana"].Cast<Slider>().CurrentValue; } }
        public static bool jungleclearUseQ { get { return Menu["jungleclearUseQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool jungleclearUseE { get { return Menu["jungleclearUseE"].Cast<CheckBox>().CurrentValue; } }
        public static int jungleclearMana { get { return Menu["jungleclearMana"].Cast<Slider>().CurrentValue; } }
        public static bool killsteal { get { return Menu["killsteal"].Cast<CheckBox>().CurrentValue; } }
        public static bool mobsteal { get { return Menu["mobsteal"].Cast<CheckBox>().CurrentValue; } }
        public static bool lasthitassist { get { return Menu["lasthitassist"].Cast<CheckBox>().CurrentValue; } }
        public static bool soulboundsaver { get { return Menu["soulboundsaver"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoEHarass1 { get { return Menu["autoEHarass1"].Cast<CheckBox>().CurrentValue; } }
        public static bool drawingQ { get { return Menu["drawingQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool drawingW { get { return Menu["drawingW"].Cast<CheckBox>().CurrentValue; } }
        public static bool drawingE { get { return Menu["drawingE"].Cast<CheckBox>().CurrentValue; } }
        public static bool drawingR { get { return Menu["drawingR"].Cast<CheckBox>().CurrentValue; } }
        public static bool Draw_EDamage { get { return Menu["Draw_EDamage"].Cast<CheckBox>().CurrentValue; } }
        public static bool Draw_Fill { get { return Menu["Draw_Fill"].Cast<CheckBox>().CurrentValue; } }
        #endregion

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Hero != Champion.Jinx)
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1150f) { MinHitChance = HitChance.High };
            W = new Spell(SpellSlot.W, 5000f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1500f);

            Q.SetSkillshot(0.25f, 40f, 1200f, true, SkillshotType.SkillshotLine);

            Menu = MainMenu.AddMenu("SS Kalista", "Kalista");
            Menu.AddLabel("Ported from SharpShooter - Pikachu7");

            combo = Menu.AddSubMenu("Combo", "combo");
            combo.Add("comboUseQ", new CheckBox("Use Q"));
            combo.Add("comboUseE", new CheckBox("Use E"));

            harass = Menu.AddSubMenu("Harass", "harass");
            harass.Add("harassUseQ", new CheckBox("Use Q"));
            harass.Add("harassMana", new Slider("if Mana % >", 50));
            
            laneclear = Menu.AddSubMenu("Lane Clear", "laneclear");
            laneclear.Add("laneclearUseQ", new CheckBox("Use Q"));
            laneclear.Add("laneclearQnum", new Slider("Cast Q If Can Kill Minion Number >=", 3, 1, 5));
            laneclear.Add("laneclearUseE", new CheckBox("Use E"));
            laneclear.Add("laneclearEnum", new Slider("Cast E If Can Kill Minion Number >=", 2, 1, 5));
            laneclear.Add("laneclearMana", new Slider("if Mana % >", 60));

            jungleclear = Menu.AddSubMenu("Jungle Clear", "jungleclear");
            jungleclear.Add("jungleclearUseQ", new CheckBox("Use Q"));
            jungleclear.Add("jungleclearUseE", new CheckBox("Use E"));
            jungleclear.Add("jungleclearMana", new Slider("if Mana % >", 20));

            misc = Menu.AddSubMenu("Misc", "misc");
            misc.Add("killsteal", new CheckBox("Use Killsteal (With E)"));
            misc.Add("mobsteal", new CheckBox("Use Mobsteal (With E)"));
            misc.Add("lasthitassist", new CheckBox("Use Lasthit Assist (With E)"));
            misc.Add("soulboundsaver", new CheckBox("Use Soulbound Saver (With R)"));
            misc.Add("autoEHarass1", new CheckBox("Auto Lasthit a Minion For Harass (With E)", false));

            draw = Menu.AddSubMenu("Drawings", "draw");
            draw.Add("drawingQ", new CheckBox("Q Range"));
            draw.Add("drawingW", new CheckBox("W Range",false));
            draw.Add("drawingE", new CheckBox("E Range"));
            draw.Add("drawingR", new CheckBox("R Range",false));
            var drawDamageMenu = draw.Add("Draw_EDamage", new CheckBox("Draw (E) Damage"));
            var drawFill = draw.Add("Draw_Fill", new CheckBox("Draw (E) Damage Fill"));

            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = Draw_EDamage;
            DamageIndicator.Fill = Draw_Fill;
            DamageIndicator.FillColor = Color.FromArgb(90, 255, 169, 4);

            drawDamageMenu.OnValueChange += DrawDamageMenu_OnValueChange;
            drawFill.OnValueChange += DrawFill_OnValueChange;

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion; 
        }

        private static void Orbwalker_OnUnkillableMinion(Obj_AI_Base minion, Orbwalker.UnkillableMinionArgs args)
        {
            if (!lasthitassist)
                return;
                
            if (E.CanCast((Obj_AI_Minion)minion) && minion.Health <= E.GetDamage((Obj_AI_Minion)minion))
                E.Cast();
        }

        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == E.Instance.Name)
                Utility.DelayAction.Add(250, () => Orbwalker.ResetAutoAttack());

            if (soulboundsaver && R.IsReady())
            {
                if (sender.Type == GameObjectType.AIHeroClient && sender.IsEnemy)
                {
                    var soulbound = HeroManager.Allies.FirstOrDefault(hero => hero.HasBuff("kalistacoopstrikeally") && args.Target.NetworkId == hero.NetworkId && hero.HealthPercent <= 20);

                    if (soulbound != null)
                        R.Cast();
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
                       
            if (Q.IsReady() && drawingQ)
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.FromArgb(0, 230, 255));

            if (W.IsReady() && drawingW)
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.FromArgb(0, 230, 255));

            if (E.IsReady() && drawingE)
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.FromArgb(0, 230, 255));

            if (R.IsReady() && drawingR)
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.FromArgb(0, 230, 255));
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                Combo();

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
                Harass();

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                Laneclear();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear)
            {
                Jungleclear();
            }

            Killsteal();
            Mobsteal();

            #region E harass with lasthit for anytime
            if (autoEHarass1)
            {
                var Minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy).Where(x => x.Health <= E.GetDamage(x)).OrderBy(x => x.Health).FirstOrDefault();
                var Target = HeroManager.Enemies.Where(x => E.CanCast(x) && E.GetDamage(x) >= 1 && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();

                if (E.CanCast(Minion) && E.CanCast(Target))
                    E.Cast();
            }
            #endregion
        }


        static void Killsteal()
        {
            if (!killsteal || !E.IsReady())
                return;

            var target = HeroManager.Enemies.FirstOrDefault(x => !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield) && E.CanCast(x) && (x.Health + (x.HPRegenRate / 2)) <= E.GetDamage(x));

            if (E.CanCast(target))
                E.Cast();
        }

        static void Mobsteal()
        {
            if (!mobsteal || !E.IsReady())
                return;

            var Mob = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.isKillableAndValidTarget(E.GetDamage(x), E.Range));

            if (E.CanCast(Mob))
                E.Cast();

            var Minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.Health <= E.GetDamage(x) && (x.BaseSkinName.ToLower().Contains("siege") || x.BaseSkinName.ToLower().Contains("super")));

            if (E.CanCast(Minion))
                E.Cast();
        }

        static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (E.IsReady())
                damage += E.GetDamage(enemy);

            return damage;
        }

        static List<Obj_AI_Base> Q_GetCollisionMinions(AIHeroClient source, Vector3 targetposition)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = Q.Width,
                Delay = Q.Delay,
                Speed = Q.Speed,
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return Collision.GetCollision(new List<Vector3> { targetposition }, input).OrderBy(obj => obj.Distance(source, false)).ToList();
        }

        static void Combo()
        {
            if (comboUseQ)
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                if (Q.CanCast(Qtarget) && !Player.IsDashing())
                    Q.SPredictionCast(Qtarget, Q.MinHitChance);
            }

            if (comboUseE && E.IsReady())
            {
                var eTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && E.GetDamage(x) >= 1 && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();

                if (eTarget != null && eTarget.isKillableAndValidTarget(E.GetDamage(eTarget)))
                    E.Cast();
            }
        }

        static void Harass()
        {
            if (!(getManaPer > harassMana))
                return;

            if (harassUseQ)
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                if (Q.CanCast(Qtarget) && !Player.IsDashing())
                    Q.SPredictionCast(Qtarget, Q.MinHitChance);
            }
        }

        static void Laneclear()
        {
            if (!(getManaPer > laneclearMana))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (laneclearUseQ && Q.IsReady())
            {
                //-------------------------------------------------------------------------------------------------------------------------------
                foreach (var minion in Minions.Where(x => x.Health <= Q.GetDamage(x)))
                {
                    var killcount = 0;

                    foreach (var colminion in Q_GetCollisionMinions(Player, (Vector3)Player.ServerPosition.Extend(minion.ServerPosition, Q.Range)))
                    {
                        if (colminion.Health <= Q.GetDamage(colminion))
                            killcount++;
                        else
                            break;
                    }

                    if (killcount >= laneclearQnum)
                    {
                        if (!Player.IsDashing())
                        {
                            Q.Cast(minion.ServerPosition);
                            break;
                        }
                    }
                }
                //-------------------------------------------------------------------------------------------------------------------------------
            }

            if (laneclearUseE && E.IsReady())
            {
                var minionkillcount = 0;

                foreach (var Minion in Minions.Where(x => E.CanCast(x) && x.Health <= E.GetDamage(x))) { minionkillcount++; }

                if (minionkillcount >= laneclearEnum)
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            if (!(getManaPer > jungleclearMana))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (jungleclearUseQ && Q.CanCast(Mobs[0]))
                Q.Cast(Mobs[0]);

            if (jungleclearUseE && E.isReadyPerfectly())
                if (Mobs.Any(x => x.isKillableAndValidTarget(E.GetDamage(x))))
                    E.Cast();
        }

        private static void DrawFill_OnValueChange(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            DamageIndicator.Fill = args.NewValue;
            DamageIndicator.FillColor = Color.FromArgb(90, 255, 169, 4);
        }

        private static void DrawDamageMenu_OnValueChange(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            DamageIndicator.Enabled = args.NewValue;
        }


    }
}
