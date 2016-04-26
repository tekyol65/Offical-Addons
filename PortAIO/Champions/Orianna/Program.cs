using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.Emit;
using System.Security.AccessControl;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using EloBuddy.SDK.Notifications;
using PortAIO.Lib;

namespace PortAIO.Champions.Orianna
{
    internal class Program
    {       

        public static LeagueSharp.Common.Spell Q;
        public static LeagueSharp.Common.Spell W;
        public static LeagueSharp.Common.Spell E;
        public static LeagueSharp.Common.Spell R;

        public static bool QIsReady;
        public static bool WIsReady;
        public static bool EIsReady;
        public static bool RIsReady;

        public static Menu Config,combo,misc,harass,farm,jungle,draw;

        private static AIHeroClient Player;

        private static Dictionary<string, string> InitiatorsList = new Dictionary<string, string>
        {
            {"aatroxq", "Aatrox"},
            {"akalishadowdance", "Akali"},
            {"headbutt", "Alistar"},
            {"bandagetoss", "Amumu"},
            {"dianateleport", "Diana"},
            {"ekkoe", "ekko"},
            {"elisespidereinitial", "Elise"},
            {"crowstorm", "FiddleSticks"},
            {"fioraq", "Fiora"},
            {"gnare", "Gnar"},
            {"gnarbige", "Gnar"},
            {"gragase", "Gragas"},
            {"hecarimult", "Hecarim"},
            {"ireliagatotsu", "Irelia"},
            {"jarvanivdragonstrike", "JarvanIV"},
            {"jaxleapstrike", "Jax"},
            {"riftwalk", "Kassadin"},
            {"katarinae", "Katarina"},
            {"kennenlightningrush", "Kennen"},
            {"khazixe", "KhaZix"},
            {"khazixelong", "KhaZix"},
            {"blindmonkqtwo", "LeeSin"},
            {"leonazenithblademissle", "Leona"},
            {"lissandrae", "Lissandra"},
            {"ufslash", "Malphite"},
            {"maokaiunstablegrowth", "Maokai"},
            {"monkeykingnimbus", "MonkeyKing"},
            {"monkeykingspintowin", "MonkeyKing"},
            {"summonerflash", "MonkeyKing"},
            {"nocturneparanoia", "Nocturne"},
            {"olafragnarok", "Olaf"},
            {"poppyheroiccharge", "Poppy"},
            {"renektonsliceanddice", "Renekton"},
            {"rengarr", "Rengar"},
            {"reksaieburrowed", "RekSai"},
            {"sejuaniarcticassault", "Sejuani"},
            {"shenshadowdash", "Shen"},
            {"shyvanatransformcast", "Shyvana"},
            {"shyvanatransformleap", "Shyvana"},
            {"sionr", "Sion"},
            {"taloncutthroat", "Talon"},
            {"threshqleap", "Thresh"},
            {"slashcast", "Tryndamere"},
            {"udyrbearstance", "Udyr"},
            {"urgotswap2", "Urgot"},
            {"viq", "Vi"},
            {"vir", "Vi"},
            {"volibearq", "Volibear"},
            {"infiniteduress", "Warwick"},
            {"yasuorknockupcombow", "Yasuo"},
            {"zace", "Zac"}
        };

        #region menu items
        public static bool UseQCombo { get { return combo["UseQCombo"].Cast<CheckBox>().CurrentValue; } }
        public static bool UseWCombo { get { return combo["UseWCombo"].Cast<CheckBox>().CurrentValue; } }
        public static bool UseECombo { get { return combo["UseECombo"].Cast<CheckBox>().CurrentValue; } }
        public static bool UseRCombo { get { return combo["UseRCombo"].Cast<CheckBox>().CurrentValue; } }
        public static int UseRNCombo { get { return combo["UseRNCombo"].Cast<ComboBox>().CurrentValue; } }
        public static int UseRImportant { get { return combo["UseRImportant"].Cast<Slider>().CurrentValue; } }
        public static int AutoW { get { return misc["AutoW"].Cast<ComboBox>().CurrentValue; } }
        public static int AutoR { get { return misc["AutoR"].Cast<ComboBox>().CurrentValue; } }
        public static bool AutoEInitiators { get { return misc["AutoEInitiators"].Cast<CheckBox>().CurrentValue; } }
        public static bool InterruptSpells { get { return misc["UseRImportant"].Cast<CheckBox>().CurrentValue; } }
        public static bool BlockR { get { return misc["BlockR"].Cast<CheckBox>().CurrentValue; } }
        public static bool UseQHarass { get { return harass["UseQHarass"].Cast<CheckBox>().CurrentValue; } }
        public static bool UseWHarass { get { return harass["UseWHarass"].Cast<CheckBox>().CurrentValue; } }
        public static int HarassManaCheck { get { return harass["HarassManaCheck"].Cast<Slider>().CurrentValue; } }
        public static bool HarassActiveT { get { return harass["HarassActiveT"].Cast<KeyBind>().CurrentValue; } }
        public static int UseQFarm { get { return farm["UseQFarm"].Cast<ComboBox>().CurrentValue; } }
        public static int UseWFarm { get { return farm["UseWFarm"].Cast<ComboBox>().CurrentValue; } }
        public static int UseEFarm { get { return farm["UseEFarm"].Cast<ComboBox>().CurrentValue; } }
        public static int LaneClearManaCheck { get { return farm["LaneClearManaCheck"].Cast<Slider>().CurrentValue; } }
        public static bool UseQJFarm { get { return jungle["UseQJFarm"].Cast<CheckBox>().CurrentValue; } }
        public static bool UseWJFarm { get { return jungle["UseWJFarm"].Cast<CheckBox>().CurrentValue; } }
        public static bool UseEJFarm { get { return jungle["UseEJFarm"].Cast<CheckBox>().CurrentValue; } }
        public static bool DamageAfterR { get { return draw["DamageAfterR"].Cast<CheckBox>().CurrentValue; } }
        public static bool QRange { get { return draw["QRange"].Cast<CheckBox>().CurrentValue; } }
        public static bool WRange { get { return draw["WRange"].Cast<CheckBox>().CurrentValue; } }
        public static bool ERange { get { return draw["ERange"].Cast<CheckBox>().CurrentValue; } }
        public static bool RRange { get { return draw["RRange"].Cast<CheckBox>().CurrentValue; } }
        public static bool QOnBallRange { get { return draw["QOnBallRange"].Cast<CheckBox>().CurrentValue; } }
        #endregion

        public static void Load()
        {
            Player = ObjectManager.Player;

            Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 825);
            W = new LeagueSharp.Common.Spell(SpellSlot.W, 245);
            E = new LeagueSharp.Common.Spell(SpellSlot.E, 1095);
            R = new LeagueSharp.Common.Spell(SpellSlot.R, 380);

            Q.SetSkillshot(0f, 130f, 1400f, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 240f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1700f, true, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.6f, 375f, float.MaxValue, false, SkillshotType.SkillshotCircle);


            Config = MainMenu.AddMenu("Orianna#", "orianna");
            #region Combo
            combo = Config.AddSubMenu("Combo", "Combo");
            combo.Add("UseQCombo", new CheckBox("Use Q"));
            combo.Add("UseWCombo", new CheckBox("Use W"));
            combo.Add("UseECombo", new CheckBox("Use E"));
            combo.Add("UseRCombo", new CheckBox("Use R"));
            combo.Add("UseRNCombo", new ComboBox("Use R on at least", 0, "1 target", "2 target", "3 target", "4 target", "5 target"));
            combo.Add("UseRImportant", new Slider("-> Or if hero priority >=", 5, 1, 5)); // 5 for e.g adc's
            #endregion

            misc = Config.AddSubMenu("Misc", "misc");
            #region Misc
            misc.Add("AutoW", new ComboBox("Auto W if it'll hit", 2, "No", ">=1 target", ">=2 target", ">=3 target", ">=4 target", ">=5 target"));
            misc.Add("AutoR", new ComboBox("Auto R if it'll hit", 3, "No", ">=1 target", ">=2 target", ">=3 target", ">=4 target", ">=5 target"));
            misc.Add("AutoEInitiators", new CheckBox("Auto E initiators"));
            misc.AddGroupLabel("Initiator's List");
            HeroManager.Allies.ForEach(
                delegate (AIHeroClient hero)
                {
                    InitiatorsList.ToList().ForEach(
                        delegate (KeyValuePair<string, string> pair)
                        {
                            if (string.Equals(hero.ChampionName, pair.Value, StringComparison.InvariantCultureIgnoreCase))
                            {
                                misc.Add(pair.Key, new CheckBox(pair.Value + " - " + pair.Key));
                            }
                        });
                });
            misc.AddSeparator();
            misc.Add("InterruptSpells", new CheckBox("Interrupt spells using R"));
            misc.Add("BlockR", new CheckBox("Block R if it won't hit", false));
            #endregion

            harass = Config.AddSubMenu("Harass", "harass");
            #region Harass
            //Harass menu:
            harass.Add("UseQHarass", new CheckBox("Use Q"));
            harass.Add("UseWHarass", new CheckBox("Use W", false));
            harass.Add("HarassManaCheck", new Slider("Don't harass if mana < %", 0, 0, 100));
            harass.Add("HarassActiveT", new KeyBind("Harass (toggle)!", false, KeyBind.BindTypes.HoldActive, 'Y'));
            #endregion

            farm = Config.AddSubMenu("Farm", "farm");
            #region Farming
            //Farming menu:
            farm.Add("EnabledFarm", new CheckBox("Enable! (On/Off: Mouse Scroll)"));
            farm.Add(
                    "UseQFarm", new ComboBox("Use Q", 2, "Freeze", "LaneClear", "Both", "No"));
            farm.Add(
                    "UseWFarm", new ComboBox("Use W", 1, "Freeze", "LaneClear", "Both", "No"));
            farm.Add(
                    "UseEFarm", new ComboBox("Use E", 1, "Freeze", "LaneClear", "Both", "No"));
            farm.Add("LaneClearManaCheck", new Slider("Don't LaneClear if mana < %", 0, 0, 100));

            jungle = Config.AddSubMenu("Jungle Farm", "junglefarm");
            //JungleFarm menu:
            jungle.Add("UseQJFarm", new CheckBox("Use Q"));
            jungle.Add("UseWJFarm", new CheckBox("Use W"));
            jungle.Add("UseEJFarm", new CheckBox("Use E"));
            #endregion
            draw = Config.AddSubMenu("Drawings", "draw");
            #region Drawings
            //Damage after combo:
            draw.Add("DamageAfterR", new CheckBox("Draw damage after combo"));


            //Drawings menu:
            draw.Add(
                    "QRange", new CheckBox("Q range"));
            draw.Add(
                    "WRange", new CheckBox("W range", false));
            draw.Add(
                    "ERange", new CheckBox("E range", false));
            draw.Add(
                    "RRange", new CheckBox("R range", false));
            draw.Add(
                    "QOnBallRange", new CheckBox("Draw ball position"));

            #endregion

            DamageIndicator.Initialize(GetComboDamage);

            AIHeroClient.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += Drawing_OnDraw;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != 0x20a)
                return;

            farm["EnabledFarm"].Cast<CheckBox>().CurrentValue = !farm["EnabledFarm"].Cast<CheckBox>().CurrentValue;
        }


        static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!InterruptSpells)
            {
                return;
            }

            if (args.DangerLevel <= Interrupter2.DangerLevel.Medium)
            {
                return;
            }

            if (sender.IsAlly)
            {
                return;
            }

            if (RIsReady)
            {
                Q.Cast(sender, true);
                if (BallManager.BallPosition.Distance(sender.ServerPosition, true) < R.Range * R.Range)
                {
                    R.Cast(Player.ServerPosition, true);
                }
            }
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!(sender is AIHeroClient))
            {
                return;
            }

            if (!AutoEInitiators)
            {
                return;
            }

            var spellName = args.SData.Name.ToLower();
            if (!InitiatorsList.ContainsKey(spellName))
            {
                return;
            }

            var item = misc[spellName];
            if (item == null || !item.Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            if (!EIsReady)
            {
                return;
            }

            if (sender.IsAlly && Player.Distance(sender, true) < E.Range * E.Range)
            {
                E.CastOnUnit(sender);
            }
        }

        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!BlockR)
            {
                return;
            }

            if (args.Slot == SpellSlot.R && GetHits(R).Item1 == 0)
            {
                args.Process = false;
            }
        }

        private static void Farm(bool laneClear)
        {
            if (!farm["EnabledFarm"].Cast<CheckBox>().CurrentValue)
                return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + W.Width,
                MinionTypes.All);
            var rangedMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + W.Width,
                MinionTypes.Ranged);

            var useQi = UseQFarm;
            var useWi = UseWFarm;
            var useEi = UseEFarm;

            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && QIsReady)
            {
                if (useW)
                {
                    var qLocation = Q.GetCircularFarmLocation(allMinions, W.Range);
                    var q2Location = Q.GetCircularFarmLocation(rangedMinions, W.Range);
                    var bestLocation = (qLocation.MinionsHit > q2Location.MinionsHit + 1) ? qLocation : q2Location;

                    if (bestLocation.MinionsHit > 0)
                    {
                        Q.Cast(bestLocation.Position, true);
                        return;
                    }
                }
                else
                {
                    foreach (var minion in allMinions.FindAll(m => !Orbwalking.InAutoAttackRange(m)))
                    {
                        if (HealthPrediction.GetHealthPrediction(minion, Math.Max((int)(minion.ServerPosition.Distance(BallManager.BallPosition) / Q.Speed * 1000) - 100, 0)) < 50)
                        {
                            Q.Cast(minion.ServerPosition, true);
                            return;
                        }
                    }
                }
            }

            if (useW && WIsReady)
            {
                var n = 0;
                var d = 0;
                foreach (var m in allMinions)
                {
                    if (m.Distance(BallManager.BallPosition) <= W.Range)
                    {
                        n++;
                        if (W.GetDamage(m) > m.Health)
                        {
                            d++;
                        }
                    }
                }
                if (n >= 3 || d >= 2)
                {
                    W.Cast(Player.ServerPosition, true);
                    return;
                }
            }

            if (useE && EIsReady)
            {
                if (W.CountHits(allMinions, Player.ServerPosition) >= 3)
                {
                    E.CastOnUnit(Player, true);
                    return;
                }
            }
        }

        private static void JungleFarm()
        {
            var useQ = UseQJFarm;
            var useW = UseWJFarm;
            var useE = UseEJFarm;

            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useW && WIsReady && W.WillHit(mob.ServerPosition, BallManager.BallPosition))
                {
                    W.Cast(Player.ServerPosition, true);
                }
                else if (useQ && QIsReady)
                {
                    Q.Cast(mob, true);
                }
                else if (useE && EIsReady && (!WIsReady || !useW))
                {
                    var closestAlly = HeroManager.Allies
                        .Where(h => h.IsValidTarget(E.Range, false))
                        .MinOrDefault(h => h.Distance(mob));
                    if (closestAlly != null)
                    {
                        E.CastOnUnit(closestAlly, true);
                    }
                }
            }
        }

        public static Tuple<int, Vector3> GetBestQLocation(AIHeroClient mainTarget)
        {
            var points = new List<Vector2>();
            var qPrediction = Q.GetPrediction(mainTarget);
            if (qPrediction.Hitchance < HitChance.VeryHigh)
            {
                return new Tuple<int, Vector3>(1, Vector3.Zero);
            }
            points.Add(qPrediction.UnitPosition.To2D());

            foreach (var enemy in HeroManager.Enemies.Where(h => h.IsValidTarget(Q.Range + R.Range)))
            {
                var prediction = Q.GetPrediction(enemy);
                if (prediction.Hitchance >= HitChance.High)
                {
                    points.Add(prediction.UnitPosition.To2D());
                }
            }

            for (int j = 0; j < 5; j++)
            {
                var mecResult = MEC.GetMec(points);

                if (mecResult.Radius < (R.Range - 75) && points.Count >= 3 && RIsReady)
                {
                    return new Tuple<int, Vector3>(3, mecResult.Center.To3D());
                }

                if (mecResult.Radius < (W.Range - 75) && points.Count >= 2 && WIsReady)
                {
                    return new Tuple<int, Vector3>(2, mecResult.Center.To3D());
                }

                if (points.Count == 1)
                {
                    return new Tuple<int, Vector3>(1, mecResult.Center.To3D());
                }

                if (mecResult.Radius < Q.Width && points.Count == 2)
                {
                    return new Tuple<int, Vector3>(2, mecResult.Center.To3D());
                }

                float maxdist = -1;
                var maxdistindex = 1;
                for (var i = 1; i < points.Count; i++)
                {
                    var distance = Vector2.DistanceSquared(points[i], points[0]);
                    if (distance > maxdist || maxdist.CompareTo(-1) == 0)
                    {
                        maxdistindex = i;
                        maxdist = distance;
                    }
                }
                points.RemoveAt(maxdistindex);
            }

            return new Tuple<int, Vector3>(1, points[0].To3D());
        }

        static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range + Q.Width, DamageType.Magical);

            if (target == null)
            {
                return;
            }
            var useQ = UseQCombo;
            var useW = UseWCombo;
            var useE = UseECombo;
            var useR = UseRCombo;

            var minRTargets = UseRNCombo + 1;

            if (useW && WIsReady)
            {
                CastW(1);
            }

            if (LeagueSharp.Common.Utility.LSCountEnemiesInRange((int)(Q.Range + R.Width)) <= 1)
            {
                if (useR && GetComboDamage(target) > target.Health && RIsReady)
                {
                    CastR(minRTargets, true);
                }

                if (useQ && QIsReady)
                {
                    CastQ(target);
                }

                if (useE)
                {
                    foreach (var ally in HeroManager.Allies.Where(h => h.IsValidTarget(E.Range, false)))
                    {
                        if (ally.Position.CountEnemiesInRange(300) >= 1)
                        {
                            E.CastOnUnit(ally, true);
                        }

                        CastE(ally, 1);
                    }

                    CastE(Player, 1);
                }
            }
            else
            {
                if (useR && RIsReady)
                {
                    if (BallManager.BallPosition.CountEnemiesInRange(800) > 1)
                    {
                        var rCheck = GetHits(R);
                        var pk = 0;
                        var k = 0;
                        if (rCheck.Item1 >= 2)
                        {
                            foreach (var hero in rCheck.Item2)
                            {
                                if ((hero.Health - GetComboDamage(hero)) < 0.4 * hero.MaxHealth || GetComboDamage(hero) >= 0.4 * hero.MaxHealth)
                                {
                                    pk++;
                                }

                                if ((hero.Health - GetComboDamage(hero)) < 0)
                                {
                                    k++;
                                }
                            }

                            if (rCheck.Item1 >= BallManager.BallPosition.CountEnemiesInRange(800) || pk >= 2 ||
                                k >= 1)
                            {
                                if (rCheck.Item1 >= minRTargets)
                                {
                                    R.Cast(Player.ServerPosition, true);
                                }
                            }
                        }
                    }

                    else if (GetComboDamage(target) > target.Health)
                    {
                        CastR(minRTargets, true);
                    }
                }

                if (useQ && QIsReady)
                {
                    var qLoc = GetBestQLocation(target);
                    if (qLoc.Item1 > 1)
                    {
                        Q.Cast(qLoc.Item2, true);
                    }
                    else
                    {
                        CastQ(target);
                    }
                }

                if (useE && EIsReady)
                {
                    if (BallManager.BallPosition.CountEnemiesInRange(800) <= 2)
                    {
                        CastE(Player, 1);
                    }
                    else
                    {
                        CastE(Player, 2);
                    }

                    foreach (var ally in HeroManager.Allies.Where(h => h.IsValidTarget(E.Range, false)))
                    {
                        if (ally.Position.CountEnemiesInRange(300) >= 2)
                        {
                            E.CastOnUnit(ally, true);
                        }
                    }
                }
            }
        }

        static void Harass()
        {
            if (Player.ManaPercent < HarassManaCheck)
                return;

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target != null)
            {
                if (UseQHarass && QIsReady)
                {
                    CastQ(target);
                    return;
                }

                if (UseWHarass && WIsReady)
                {
                    CastW(1);
                }
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            QIsReady = (Player.Spellbook.CanUseSpell(Q.Slot) == SpellState.Ready || Player.Spellbook.CanUseSpell(Q.Slot) == SpellState.Surpressed);
            WIsReady = (Player.Spellbook.CanUseSpell(W.Slot) == SpellState.Ready || Player.Spellbook.CanUseSpell(W.Slot) == SpellState.Surpressed);
            EIsReady = (Player.Spellbook.CanUseSpell(E.Slot) == SpellState.Ready || Player.Spellbook.CanUseSpell(E.Slot) == SpellState.Surpressed);
            RIsReady = (Player.Spellbook.CanUseSpell(R.Slot) == SpellState.Ready || Player.Spellbook.CanUseSpell(R.Slot) == SpellState.Surpressed);

            if (Player.IsDead)
            {
                return;
            }

            if (BallManager.BallPosition == Vector3.Zero)
            {
                return;
            }

            Q.From = BallManager.BallPosition;
            Q.RangeCheckFrom = Player.ServerPosition;
            W.From = BallManager.BallPosition;
            W.RangeCheckFrom = BallManager.BallPosition;
            E.From = BallManager.BallPosition;
            R.From = BallManager.BallPosition;
            R.RangeCheckFrom = BallManager.BallPosition;

            var autoWminTargets = AutoW;
            if (autoWminTargets > 0)
            {
                CastW(autoWminTargets);
            }

            var autoRminTargets = AutoR;
            if (autoRminTargets > 0)
            {
                CastR(autoRminTargets);
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            else
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) ||
                    (HarassActiveT && !Player.HasBuff("Recall")))
                    Harass();

                var lc = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear);
                if (lc || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                    Farm(lc && (Player.Mana * 100 / Player.MaxMana >= LaneClearManaCheck));

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                    JungleFarm();
            }
        }

        public static float GetComboDamage(AIHeroClient target)
        {
            var result = 0f;
            if (QIsReady)
            {
                result += 2 * Q.GetDamage(target);
            }

            if (WIsReady)
            {
                result += W.GetDamage(target);
            }

            if (RIsReady)
            {
                result += R.GetDamage(target);
            }

            result += 2 * (float)Player.GetAutoAttackDamage(target);

            return result;
        }

        public static Tuple<int, List<AIHeroClient>> GetHits(LeagueSharp.Common.Spell spell)
        {
            var hits = new List<AIHeroClient>();
            var range = spell.Range * spell.Range;
            foreach (var enemy in HeroManager.Enemies.Where(h => h.IsValidTarget() && BallManager.BallPosition.Distance(h.ServerPosition, true) < range))
            {
                if (spell.WillHit(enemy, BallManager.BallPosition) && BallManager.BallPosition.Distance(enemy.ServerPosition, true) < spell.Width * spell.Width)
                {
                    hits.Add(enemy);
                }
            }
            return new Tuple<int, List<AIHeroClient>>(hits.Count, hits);
        }

        public static Tuple<int, List<AIHeroClient>> GetEHits(Vector3 to)
        {
            var hits = new List<AIHeroClient>();
            var oldERange = E.Range;
            E.Range = 10000; //avoid the range check
            foreach (var enemy in HeroManager.Enemies.Where(h => h.IsValidTarget(2000)))
            {
                if (E.WillHit(enemy, to))
                {
                    hits.Add(enemy);
                }
            }
            E.Range = oldERange;
            return new Tuple<int, List<AIHeroClient>>(hits.Count, hits);
        }

        public static bool CastQ(AIHeroClient target)
        {
            var qPrediction = Q.GetPrediction(target);

            if (qPrediction.Hitchance < HitChance.VeryHigh)
            {
                return false;
            }

            if (EIsReady)
            {
                var directTravelTime = BallManager.BallPosition.Distance(qPrediction.CastPosition) / Q.Speed;
                var bestEQTravelTime = float.MaxValue;

                AIHeroClient eqTarget = null;

                foreach (var ally in HeroManager.Allies.Where(h => h.IsValidTarget(E.Range, false)))
                {
                    var t = BallManager.BallPosition.Distance(ally.ServerPosition) / E.Speed + ally.Distance(qPrediction.CastPosition) / Q.Speed;
                    if (t < bestEQTravelTime)
                    {
                        eqTarget = ally;
                        bestEQTravelTime = t;
                    }
                }

                if (eqTarget != null && bestEQTravelTime < directTravelTime * 1.3f && (BallManager.BallPosition.Distance(eqTarget.ServerPosition, true) > 10000))
                {
                    E.CastOnUnit(eqTarget, true);
                    return true;
                }
            }

            if (!target.IsFacing(Player) && target.Path.Count() >= 1) // target is running
            {
                var targetBehind = Q.GetPrediction(target).CastPosition +
                                   Vector3.Normalize(target.ServerPosition - BallManager.BallPosition) * target.MoveSpeed / 2;
                Q.Cast(targetBehind, true);
                return true;
            }

            Q.Cast(qPrediction.CastPosition, true);
            return true;
        }

        public static bool CastW(int minTargets)
        {
            var hits = GetHits(W);
            if (hits.Item1 >= minTargets)
            {
                W.Cast(Player.ServerPosition, true);
                return true;
            }
            return false;
        }

        public static bool CastE(AIHeroClient target, int minTargets)
        {
            if (GetEHits(target.ServerPosition).Item1 >= minTargets)
            {
                E.CastOnUnit(target, true);
                return true;
            }
            return false;
        }

        public static bool CastR(int minTargets, bool prioriy = false)
        {
            if (GetHits(R).Item1 >= minTargets || prioriy && GetHits(R)
                    .Item2.Any(
                        hero =>
                            TargetSelector.GetPriority(hero) >=
                            UseRImportant))
            {
                R.Cast(Player.ServerPosition, true);
                return true;
            }

            return false;
        }

        public static int GetNumberOfMinionsHitByE(AIHeroClient target)
        {
            var minions = MinionManager.GetMinions(BallManager.BallPosition, 2000);
            return E.CountHits(minions, target.ServerPosition);
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            
            if (QRange)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.FromArgb(150, Color.DodgerBlue));
            }

            
            if (WRange)
            {
                Render.Circle.DrawCircle(BallManager.BallPosition, W.Range, Color.FromArgb(150, Color.DodgerBlue));
            }

            
            if (ERange)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.FromArgb(150, Color.DodgerBlue));
            }

            
            if (RRange)
            {
                Render.Circle.DrawCircle(BallManager.BallPosition, R.Range, Color.FromArgb(150, Color.DodgerBlue));
            }

            
            if (QOnBallRange)
            {
                Render.Circle.DrawCircle(BallManager.BallPosition, Q.Width, Color.FromArgb(150, Color.DodgerBlue), 5, true);
            }

            DamageIndicator.DrawingColor = Color.GreenYellow;
            DamageIndicator.HealthbarEnabled = DamageAfterR;
        }

    }
}
