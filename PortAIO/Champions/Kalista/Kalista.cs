using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using LeagueSharp.Common;
using SPrediction;
using System.Drawing;
using EloBuddy.SDK.Menu;
using PortAIO.Champions.Kalista.Utils;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using DZLib.Modules;
using PortAIO.Champions.Kalista.Modules;
using PortAIO.Lib;

namespace PortAIO.Champions.Kalista
{
    internal class Kalista
    {
        public static Menu Menu, comboMenu, mixedMenu, laneclearMenu, jungleStealMenu, miscMenu, drawingMenu;

        public static List<string> JungleMinions = new List<string>
        {
            "SRU_Baron",
            "SRU_Blue",
            "Sru_Crab",
            "SRU_Dragon",
            "SRU_Gromp",
            "SRU_Krug",
            "SRU_Murkwolf",
            "SRU_Razorbeak",
            "SRU_Red"
        };

        public static Dictionary<string, string> JungleMinion = new Dictionary<string, string>
        {
            { "SRU_Baron", "Baron" },
            { "SRU_Dragon", "Dragon" },
        };

        /// <summary>
        ///     The Modules
        /// </summary>
        public static readonly List<IModule> Modules = new List<IModule>
        {
            new AutoRendModule(),
            new JungleStealModule(),
            new AutoEModule(),
            new AutoELeavingModule()
        };


        public static void OnLoad()
        {
            CreateMenu();
            LoadModules();
            DamageIndicator.Initialize(Helper.GetRendDamage);
            SPrediction.Prediction.Initialize(Menu);
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            Spellbook.OnCastSpell += (sender, args) =>
            {
                if (sender.Owner.IsMe && args.Slot == SpellSlot.Q && ObjectManager.Player.IsDashing())
                {
                    args.Process = false;
                }
            };

            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;
        }

        private static void Orbwalker_OnUnkillableMinion(Obj_AI_Base minion, Orbwalker.UnkillableMinionArgs args)
        {
            var killableMinion = minion as Obj_AI_Base;
            if (killableMinion == null || !SpellManager.Spell[SpellSlot.E].IsReady()
                || ObjectManager.Player.HasBuff("summonerexhaust") || !killableMinion.HasRendBuff())
            {
                return;
            }

            if (laneclearMenu["com.ikalista.laneclear.useEUnkillable"].Cast<CheckBox>().CurrentValue &&
                killableMinion.IsMobKillable())
            {
                SpellManager.Spell[SpellSlot.E].Cast();
            }
        }

        /// <summary>
        ///     This is where jeff creates his first Menu in a long time
        /// </summary>
        private static void CreateMenu()
        {
            Menu = MainMenu.AddMenu("iKalista: Reborn", "com.ikalista");

            comboMenu = Menu.AddSubMenu("iKalista: Reborn - Combo", "com.ikalista.combo");
            {
                comboMenu.Add("com.ikalista.combo.useQ", new CheckBox("Use Q"));
                //comboMenu.AddText("--", "------------------");
                comboMenu.Add("com.ikalista.combo.useE", new CheckBox("Use E"));
                comboMenu.Add("com.ikalista.combo.stacks", new Slider("Rend at X stacks", 10, 1, 20));
                comboMenu.Add("com.ikalista.combo.eLeaving", new CheckBox("Use E Leaving", true));
                comboMenu.Add("com.ikalista.combo.ePercent", new Slider("Min Percent for E Leaving", 50, 10, 100));
                comboMenu.Add("com.ikalista.combo.saveMana", new CheckBox("Save Mana for E", true));
                comboMenu.Add("com.ikalista.combo.autoE", new CheckBox("Auto E Minion > Champion", true));
                comboMenu.Add("com.ikalista.combo.orbwalkMinions", new CheckBox("Orbwalk Minions in combo", true));
                //comboMenu.AddText("---", "------------------");
                comboMenu.Add("com.ikalista.combo.saveAlly", new CheckBox("Save Ally With R", true));
                comboMenu.Add("com.ikalista.combo.balista", new CheckBox("Use Balista", true));
                comboMenu.Add("com.ikalista.combo.allyPercent", new Slider("Min Health % for Ally", 20, 10, 100));
            }

            mixedMenu = Menu.AddSubMenu("iKalista: Reborn - Mixed", "com.ikalista.mixed");
            {
                mixedMenu.Add("com.ikalista.mixed.useQ", new CheckBox("Use Q", true));
                mixedMenu.Add("com.ikalista.mixed.useE", new CheckBox("Use E", true));
                mixedMenu.Add("com.ikalista.mixed.stacks", new Slider("Rend at X stacks", 10, 1, 20));
            }

            laneclearMenu = Menu.AddSubMenu("iKalista: Reborn - Laneclear", "com.ikalista.laneclear");
            {
                laneclearMenu.Add("com.ikalista.laneclear.useQ", new CheckBox("Use Q", true));
                laneclearMenu.Add("com.ikalista.laneclear.qMinions", new Slider("Min Minions for Q", 3, 1, 10));
                laneclearMenu.Add("com.ikalista.laneclear.useE", new CheckBox("Use E", true));
                laneclearMenu.Add("com.ikalista.laneclear.eMinions", new Slider("Min Minions for E", 5, 1, 10));
                laneclearMenu.Add("com.ikalista.laneclear.useEUnkillable", new CheckBox("E Unkillable Minions", true));
            }

            jungleStealMenu = Menu.AddSubMenu("iKalista: Reborn - Jungle Steal", "com.ikalista.jungleSteal");
            {
                jungleStealMenu.Add("com.ikalista.jungleSteal.enabled", new CheckBox("Use Rend To Steal Jungle Minions", true));

                /*foreach (var minion in JungleMinions)
                {
                    jungleStealMenu.AddBool("com.ikalista.jungleSteal." + minion, minion, true);
                }*/
            }

            miscMenu = Menu.AddSubMenu("iKalista: Reborn - Misc", "com.ikalista.misc");
            {
                
            }

            drawingMenu = Menu.AddSubMenu("iKalista: Reborn - Drawing", "com.ikalista.drawing");
            {
                drawingMenu.Add("com.ikalista.drawing.spellRanges",new CheckBox("Draw Spell Ranges", true));
                drawingMenu.Add(
                    "com.ikalista.drawing.eDamage", new CheckBox("Draw E Damage"));
                drawingMenu.Add(
                    "com.ikalista.drawing.damagePercent", new CheckBox("Draw Percent Damage"));
            }
        }

        private static void LoadModules()
        {
            foreach (var module in Modules.Where(x => x.ShouldGetExecuted()))
            {
                try
                {
                    module.OnLoad();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error loading module: " + module.GetName() + " Exception: " + e);
                }
            }
        }

        /// <summary>
        ///     My names definatly jeffery.
        /// </summary>
        /// <param name="args">even more gay</param>
        private static void OnDraw(EventArgs args)
        {
            if (drawingMenu["com.ikalista.drawing.spellRanges"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var spell in SpellManager.Spell.Values)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, Color.DarkOliveGreen);
                }
            }

            DamageIndicator.DrawingColor = Color.DarkOliveGreen;
            DamageIndicator.HealthbarEnabled = drawingMenu["com.ikalista.drawing.eDamage"].Cast<CheckBox>().CurrentValue;
            DamageIndicator.PercentEnabled = drawingMenu["com.ikalista.drawing.damagePercent"].Cast<CheckBox>().CurrentValue;
        }

        /// <summary>
        ///     The on process spell function
        /// </summary>
        /// <param name="sender">
        ///     The Spell Sender
        /// </param>
        /// <param name="args">
        ///     The Arguments
        /// </param>
        private static void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
            {
                Orbwalker.ResetAutoAttack();
            }

            if (sender.Type == GameObjectType.AIHeroClient && sender.IsEnemy && args.Target != null &&
                comboMenu["com.ikalista.combo.saveAlly"].Cast<CheckBox>().CurrentValue)
            {
                var soulboundhero =
                    HeroManager.Allies.FirstOrDefault(
                        hero =>
                            hero.HasBuff("kalistacoopstrikeally") && args.Target.NetworkId == hero.NetworkId);

                if (soulboundhero != null &&
                    soulboundhero.HealthPercent < comboMenu["com.ikalista.combo.allyPercent"].Cast<Slider>().CurrentValue)
                {
                    SpellManager.Spell[SpellSlot.R].Cast();
                }
            }
        }

        /// <summary>
        ///     My Names Jeff
        /// </summary>
        /// <param name="args">gay</param>
        private static void OnUpdate(EventArgs args)
        {
            if(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                OnCombo();
            }
            else if(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                OnMixed();
            }
            else if(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                OnLaneclear();
            }
            else if(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                OnFlee();
            }

            //BALISTA
            if (comboMenu["com.ikalista.combo.balista"].Cast<CheckBox>().CurrentValue && SpellManager.Spell[SpellSlot.R].IsReady())
            {
                var soulboundhero = HeroManager.Allies.FirstOrDefault(x => x.HasBuff("kalistacoopstrikeally"));
                if (soulboundhero?.ChampionName == "Blitzcrank")
                {
                    foreach (
                        var unit in
                            HeroManager.Enemies
                                .Where(
                                    h => h.IsHPBarRendered &&
                                         h.Distance(ObjectManager.Player.ServerPosition) > 700 &&
                                         h.Distance(ObjectManager.Player.ServerPosition) < 1400)
                        )
                    {
                        if (unit.HasBuff("rocketgrab2"))
                        {
                            SpellManager.Spell[SpellSlot.R].Cast();
                        }
                    }
                }
            }

            foreach (var module in Modules.Where(x => x.ShouldGetExecuted()))
            {
                module.OnExecute();
            }
        }

        private static void OnFlee()
        {
            var bestTarget =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(x => x.IsEnemy && ObjectManager.Player.Distance(x) <= Orbwalking.GetRealAutoAttackRange(x))
                    .OrderBy(x => ObjectManager.Player.Distance(x))
                    .FirstOrDefault();

            Orbwalker.OrbwalkTo(bestTarget.Position);

            //TODO wall flee
        }

        private static void OnCombo()
        {
            if (comboMenu["com.ikalista.combo.orbwalkMinions"].Cast<CheckBox>().CurrentValue)
            {
                var targets =
                    HeroManager.Enemies.Where(
                        x =>
                            ObjectManager.Player.Distance(x) <= SpellManager.Spell[SpellSlot.E].Range * 2 &&
                            x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range * 2));

                if (targets.Count(x => ObjectManager.Player.Distance(x) < Orbwalking.GetRealAutoAttackRange(x)) == 0)
                {
                    var minion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                x =>
                                    ObjectManager.Player.Distance(x) <= Orbwalking.GetRealAutoAttackRange(x) &&
                                    x.IsEnemy)
                            .OrderBy(x => x.Health)
                            .FirstOrDefault();
                    if (minion != null)
                    {
                        Orbwalker.OrbwalkTo(minion.ServerPosition);
                    }
                }
            }

            if (!SpellManager.Spell[SpellSlot.Q].IsReady() || !comboMenu["com.ikalista.combo.useQ"].Cast<CheckBox>().CurrentValue)
                return;

            if (comboMenu["com.ikalista.combo.saveMana"].Cast<CheckBox>().CurrentValue && ObjectManager.Player.Mana < SpellManager.Spell[SpellSlot.E].ManaCost * 2)
            {
                return;
            }

            var target = TargetSelector.GetTarget(SpellManager.Spell[SpellSlot.Q].Range,
                DamageType.Physical);
            var prediction = SpellManager.Spell[SpellSlot.Q].GetSPrediction(target);
            if (prediction.HitChance >= HitChance.High &&
                target.IsValidTarget(SpellManager.Spell[SpellSlot.Q].Range) && !ObjectManager.Player.IsDashing())
            {
                SpellManager.Spell[SpellSlot.Q].Cast(prediction.CastPosition);
            }
        }

        private static void OnMixed()
        {
            if (SpellManager.Spell[SpellSlot.Q].IsReady() && mixedMenu["com.ikalista.mixed.useQ"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(SpellManager.Spell[SpellSlot.Q].Range,
                    DamageType.Physical);
                var prediction = SpellManager.Spell[SpellSlot.Q].GetSPrediction(target);
                if (prediction.HitChance >= HitChance.High &&
                    target.IsValidTarget(SpellManager.Spell[SpellSlot.Q].Range))
                {
                    SpellManager.Spell[SpellSlot.Q].Cast(prediction.CastPosition);
                }
            }

            if (SpellManager.Spell[SpellSlot.E].IsReady() && mixedMenu["com.ikalista.mixed.useE"].Cast<CheckBox>().CurrentValue)
            {
                foreach (
                    var source in
                        HeroManager.Enemies.Where(
                            x => x.IsValid && x.HasRendBuff() && SpellManager.Spell[SpellSlot.E].IsInRange(x)))
                {
                    if (source.IsRendKillable() ||
                        source.GetRendBuffCount() >= mixedMenu["com.ikalista.mixed.stacks"].Cast<Slider>().CurrentValue)
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }
                }
            }
        }

        private static void OnLaneclear()
        {
            if (laneclearMenu["com.ikalista.laneclear.useQ"].Cast<CheckBox>().CurrentValue)
            {
                var minions = MinionManager.GetMinions(SpellManager.Spell[SpellSlot.Q].Range).ToList();
                if (minions.Count < 0)
                    return;

                foreach (var minion in minions.Where(x => x.Health <= SpellManager.Spell[SpellSlot.Q].GetDamage(x)))
                {
                    var killableMinions = Helper.GetCollisionMinions(ObjectManager.Player,
                        ObjectManager.Player.ServerPosition.LSExtend(
                            minion.ServerPosition,
                            SpellManager.Spell[SpellSlot.Q].Range))
                        .Count(
                            collisionMinion =>
                                collisionMinion.Health
                                <= ObjectManager.Player.GetSpellDamage(collisionMinion, SpellSlot.Q));

                    if (killableMinions >= laneclearMenu["com.ikalista.laneclear.qMinions"].Cast<Slider>().CurrentValue)
                    {
                        SpellManager.Spell[SpellSlot.Q].Cast(minion.ServerPosition);
                    }
                }
            }
            if (laneclearMenu["com.ikalista.laneclear.useE"].Cast<CheckBox>().CurrentValue)
            {
                var minions = MinionManager.GetMinions(SpellManager.Spell[SpellSlot.E].Range).ToList();
                if (minions.Count < 0)
                    return;

                var count =
                    minions.Count(
                        x => SpellManager.Spell[SpellSlot.E].CanCast(x) && x.IsMobKillable());

                if (count >= laneclearMenu["com.ikalista.laneclear.eMinions"].Cast<Slider>().CurrentValue &&
                    !ObjectManager.Player.HasBuff("summonerexhaust"))
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }
    }
}
