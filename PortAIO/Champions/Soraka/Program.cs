namespace PortAIO.Champions.Soraka
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;

    using EloBuddy;
    using LeagueSharp.Common;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu.Values;/// <summary>
                                   ///     The sophies soraka.
                                   /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    internal class SophiesSoraka
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        public static LeagueSharp.Common.Spell E { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        public static Menu Menu, comboMenu, harassMenu, wMenu, healingMenu, rMenu, miscMenu, drawMenu;
        

        /// <summary>
        ///     Gets a value indicating whether to use packets.
        /// </summary>
        public static bool Packets
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets or sets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        public static LeagueSharp.Common.Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        public static LeagueSharp.Common.Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        public static LeagueSharp.Common.Spell W { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The on game load.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        public static void OnGameLoad()
        {
            if (ObjectManager.Player.ChampionName != "Soraka")
            {
                return;
            }

            Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 950);
            W = new LeagueSharp.Common.Spell(SpellSlot.W, 550);
            E = new LeagueSharp.Common.Spell(SpellSlot.E, 925);
            R = new LeagueSharp.Common.Spell(SpellSlot.R);

            Q.SetSkillshot(0.26f, 125, 1600, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 70f, 1750, false, SkillshotType.SkillshotCircle);

            CreateMenu();

            PrintChat("loaded.");

            Interrupter2.OnInterruptableTarget += InterrupterOnOnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Game.OnUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            Orbwalker.OnPreAttack += OrbwalkingOnBeforeAttack;
        }

        /// <summary>
        ///     Prints to the chat.
        /// </summary>
        /// <param name="msg">The message.</param>
        public static void PrintChat(string msg)
        {
            Chat.Print("<font color='#3492EB'>Sophie's Soraka:</font> <font color='#FFFFFF'>" + msg + "</font>");
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The on enemy gapcloser event.
        /// </summary>
        /// <param name="gapcloser">
        ///     The gapcloser.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender;

            if (useQGapcloser && unit.IsValidTarget(Q.Range) && Q.IsReady())
            {
                Q.Cast(unit, Packets);
            }

            if (useEGapcloser && unit.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(unit, Packets);
            }
        }

        /// <summary>
        ///     Automatics the ultimate.
        /// </summary>
        private static void AutoR()
        {
            if (!R.IsReady())
            {
                return;
            }

            if (
                ObjectManager.Get<AIHeroClient>()
                    .Where(x => x.IsAlly && x.IsValidTarget(float.MaxValue, false))
                    .Select(x => (int)x.Health / x.MaxHealth * 100)
                    .Select(
                        friendHealth =>
                        new { friendHealth, health = autoRPercent })
                    .Where(x => x.friendHealth <= x.health)
                    .Select(x => x.friendHealth)
                    .Any())
            {
                R.Cast(Packets);
            }
        }

        /// <summary>
        ///     Automatics the W heal.
        /// </summary>
        private static void AutoW()
        {
            if (!W.IsReady())
            {
                return;
            }
            
            if (ObjectManager.Player.HealthPercent < autoWHealth)
            {
                return;
            }
            
            if (DontWInFountain && ObjectManager.Player.InFountain())
            {
                return;
            }

            var healthPercent = autoWPercent;

            var canidates = ObjectManager.Get<AIHeroClient>().Where(x => x.IsValidTarget(W.Range, false) && x.IsAlly && x.HealthPercent < healthPercent);
            var wMode = HealingPriority;

            switch (wMode)
            {
                case 0:
                    canidates = canidates.OrderByDescending(x => x.TotalAttackDamage);
                    break;
                case 1:
                    canidates = canidates.OrderByDescending(x => x.TotalMagicalDamage);
                    break;
                case 2:
                    canidates = canidates.OrderBy(x => x.Health);
                    break;
                case 3:
                    canidates = canidates.OrderBy(x => x.Health).ThenBy(x => x.MaxHealth);
                    break;
            }

            var target = DontWInFountain ? canidates.FirstOrDefault(x => !x.InFountain()) : canidates.FirstOrDefault();

            if (target != null)
            {
                W.CastOnUnit(target);
            }
        }

        /// <summary>
        ///     The combo.
        /// </summary>
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
            {
                return;
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, Packets);
            }

            if (useE && E.IsReady())
            {
                E.Cast(target, Packets);
            }
        }

        

        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = MainMenu.AddMenu("Sophies's Soraka", "sSoraka");
            
            // Combo
            comboMenu = Menu.AddSubMenu("Combo", "ssCombo");
            comboMenu.Add("useQ", new CheckBox("Use Q"));
            comboMenu.Add("useE", new CheckBox("Use E"));

            // Harass
            harassMenu = Menu.AddSubMenu("Harass", "ssHarass");
            harassMenu.Add("useQHarass", new CheckBox("Use Q"));
            harassMenu.Add("useEHarass", new CheckBox("Use E"));

            // Healing
            //healingMenu = Menu.AddSubMenu("Healing", "ssHeal");

            wMenu = Menu.AddSubMenu("W Settings", "WSettings");
            wMenu.Add("autoW", new CheckBox("Use W"));
            wMenu.Add("autoWPercent", new Slider("Ally Health Percent", 50, 1));
            wMenu.Add("autoWHealth", new Slider("My Health Percent", 30, 1));
            wMenu.Add("DontWInFountain", new CheckBox("Dont W in Fountain"));
            wMenu.Add(
                "HealingPriority", new ComboBox("Healing Priority", 3, "Most AD", "Most AP", "Least Health", "Least Health (Prioritize Squishies)"));

            rMenu = Menu.AddSubMenu("R Settings", "RSettings");
            rMenu.Add("autoR", new CheckBox("Use R"));
            rMenu.Add("autoRPercent", new Slider("% Percent", 15, 1));
            

            // Drawing
            drawMenu = Menu.AddSubMenu("Drawing", "ssDrawing");
            drawMenu.Add("drawQ", new CheckBox("Draw Q"));
            drawMenu.Add("drawW", new CheckBox("Draw W"));
            drawMenu.Add("drawE", new CheckBox("Draw E"));

            // Misc
            miscMenu = Menu.AddSubMenu("Misc", "ssMisc");
            miscMenu.Add("useQGapcloser", new CheckBox("Q on Gapcloser"));
            miscMenu.Add("useEGapcloser", new CheckBox("E on Gapcloser"));
            miscMenu.Add("eInterrupt", new CheckBox("Use E to Interrupt"));
            miscMenu.Add("AttackMinions", new CheckBox("Attack Minions", false));
            miscMenu.Add("AttackChampions", new CheckBox("Attack Champions"));
        }

        #region menu items
        public static bool useQ { get { return comboMenu["useQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool useE { get { return comboMenu["useE"].Cast<CheckBox>().CurrentValue; } }
        public static bool useQHarass { get { return harassMenu["useQHarass"].Cast<CheckBox>().CurrentValue; } }
        public static bool useEHarass { get { return harassMenu["useEHarass"].Cast<CheckBox>().CurrentValue; } }
        public static bool autoW { get { return wMenu["autoW"].Cast<CheckBox>().CurrentValue; } }
        public static int autoWPercent { get { return wMenu["autoWPercent"].Cast<Slider>().CurrentValue; } }
        public static int autoWHealth { get { return wMenu["autoWHealth"].Cast<Slider>().CurrentValue; } }
        public static bool DontWInFountain { get { return wMenu["DontWInFountain"].Cast<CheckBox>().CurrentValue; } }
        public static int HealingPriority { get { return wMenu["HealingPriority"].Cast<ComboBox>().CurrentValue; } }
        public static bool autoR { get { return rMenu["autoR"].Cast<CheckBox>().CurrentValue; } }
        public static int autoRPercent { get { return rMenu["autoRPercent"].Cast<Slider>().CurrentValue; } }
        public static bool drawQ { get { return drawMenu["drawQ"].Cast<CheckBox>().CurrentValue; } }
        public static bool drawW { get { return drawMenu["drawW"].Cast<CheckBox>().CurrentValue; } }
        public static bool drawE { get { return drawMenu["drawE"].Cast<CheckBox>().CurrentValue; } }
        public static bool useQGapcloser { get { return miscMenu["useQGapcloser"].Cast<CheckBox>().CurrentValue; } }
        public static bool useEGapcloser { get { return miscMenu["useEGapcloser"].Cast<CheckBox>().CurrentValue; } }
        public static bool eInterrupt { get { return miscMenu["eInterrupt"].Cast<CheckBox>().CurrentValue; } }
        public static bool AttackMinions { get { return miscMenu["AttackMinions"].Cast<CheckBox>().CurrentValue; } }
        public static bool AttackChampions { get { return miscMenu["AttackChampions"].Cast<CheckBox>().CurrentValue; } }
        #endregion

        /// <summary>
        ///     The on draw event.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void DrawingOnOnDraw(EventArgs args)
        {
            var p = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(p, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        /// <summary>
        ///     The  on game update event.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void GameOnOnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
            }

            if (autoW)
            {
                AutoW();
            }

            if (autoR)
            {
                AutoR();
            }
        }

        /// <summary>
        ///     The harass.
        /// </summary>
        private static void Harass()
        {
            var useQ = useQHarass;
            var useE = useEHarass;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
            {
                return;
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, Packets);
            }

            if (useE && E.IsReady())
            {
                E.Cast(target, Packets);
            }
        }

        /// <summary>
        ///     The on possible to interrupt event.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void InterrupterOnOnPossibleToInterrupt(
            AIHeroClient sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            var unit = sender;
            var spell = args;

            if (eInterrupt == false || spell.DangerLevel != Interrupter2.DangerLevel.High)
            {
                return;
            }

            if (!unit.IsValidTarget(E.Range))
            {
                return;
            }

            if (!E.IsReady())
            {
                return;
            }

            E.Cast(unit, Packets);
        }

        /// <summary>
        ///     Called before the orbwalker attacks a unit.
        /// </summary>
        /// <param name="args">The <see cref="Orbwalking.BeforeAttackEventArgs" /> instance containing the event data.</param>
        private static void OrbwalkingOnBeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (args.Target.IsValid<Obj_AI_Minion>()
                && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)
                    || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                && !AttackMinions)
            {
                if (ObjectManager.Player.CountAlliesInRange(1200) != 0)
                {
                    args.Process = false;
                }
            }

            if (!args.Target.IsValid<AIHeroClient>() || AttackChampions)
            {
                return;
            }

            if (ObjectManager.Player.CountAlliesInRange(1200) != 0)
            {
                args.Process = false;
            }
        }

        #endregion
    }
}
