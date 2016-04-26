using System.Drawing;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Menu;
using PortAIO.Lib;

namespace PortAIO.Champions.Lucian
{
    class LucianMenu
    {
        public static Menu Config, comboMenu, harassMenu, clearMenu, jungleMenu, killStealMenu, miscMenu, drawMenu;

        #region menu items
        public static bool lucianqcombo { get { return comboMenu["lucian.q.combo"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianecombo { get { return comboMenu["lucian.e.combo"].Cast<CheckBox>().CurrentValue; } }
        public static int lucianemode { get { return comboMenu["lucian.e.mode"].Cast<ComboBox>().CurrentValue; } }
        public static bool lucianwcombo { get { return comboMenu["lucian.w.combo"].Cast<CheckBox>().CurrentValue; } }
        public static bool luciandisablewprediction { get { return comboMenu["lucian.disable.w.prediction"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianrcombo { get { return comboMenu["lucian.r.combo"].Cast<CheckBox>().CurrentValue; } }
        public static bool luciancombostarte { get { return comboMenu["lucian.combo.start.e"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianqharass { get { return harassMenu["lucian.q.harass"].Cast<CheckBox>().CurrentValue; } }
        public static int lucianqtype { get { return harassMenu["lucian.q.type"].Cast<ComboBox>().CurrentValue; } }
        public static bool lucianwharass { get { return harassMenu["lucian.w.harass"].Cast<CheckBox>().CurrentValue; } }
        public static int lucianharassmana { get { return harassMenu["lucian.harass.mana"].Cast<Slider>().CurrentValue; } }
        public static bool lucianqclear { get { return clearMenu["lucian.q.clear"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianwclear { get { return clearMenu["lucian.w.clear"].Cast<CheckBox>().CurrentValue; } }
        public static int lucianqminionhitcount { get { return clearMenu["lucian.q.minion.hit.count"].Cast<Slider>().CurrentValue; } }
        public static int lucianwminionhitcount { get { return clearMenu["lucian.w.minion.hit.count"].Cast<Slider>().CurrentValue; } }
        public static int lucianclearmana { get { return clearMenu["lucian.clear.mana"].Cast<Slider>().CurrentValue; } }
        public static bool lucianqjungle { get { return jungleMenu["lucian.q.jungle"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianwjungle { get { return jungleMenu["lucian.w.jungle"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianejungle { get { return jungleMenu["lucian.e.jungle"].Cast<CheckBox>().CurrentValue; } }
        public static int lucianjunglemana { get { return jungleMenu["lucian.jungle.mana"].Cast<Slider>().CurrentValue; } }
        public static bool lucianqks { get { return killStealMenu["lucian.q.ks"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianwks { get { return killStealMenu["lucian.w.ks"].Cast<CheckBox>().CurrentValue; } }
        public static int lucianegapclosex { get { return miscMenu["lucian.e.gapclosex"].Cast<ComboBox>().CurrentValue; } }
        public static bool lucianqdraw { get { return drawMenu["lucian.q.draw"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianq2draw { get { return drawMenu["lucian.q2.draw"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianwdraw { get { return drawMenu["lucian.w.draw"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianedraw { get { return drawMenu["lucian.e.draw"].Cast<CheckBox>().CurrentValue; } }
        public static bool lucianrdraw { get { return drawMenu["lucian.r.draw"].Cast<CheckBox>().CurrentValue; } }
        public static bool RushDrawEDamage { get { return drawMenu["RushDrawEDamage"].Cast<CheckBox>().CurrentValue; } }
        public static bool luciansemimanualult { get { return Config["lucian.semi.manual.ult"].Cast<KeyBind>().CurrentValue; } }
        #endregion


        public static void MenuInit()
        {
            Config = MainMenu.AddMenu("LCS Series: Lucian", "LCS Series: Lucian");
            comboMenu = Config.AddSubMenu(":: Combo Settings", ":: Combo Settings");
            {
                comboMenu.Add("lucian.q.combo", new CheckBox("Use Q"));
                comboMenu.Add("lucian.e.combo", new CheckBox("Use E"));
                comboMenu.Add("lucian.e.mode", new ComboBox("E Type", 0, "Safe", "Cursor Position"));
                comboMenu.Add("lucian.w.combo", new CheckBox("Use W"));
                comboMenu.Add("lucian.disable.w.prediction", new CheckBox("Disable W Prediction"));
                comboMenu.Add("lucian.r.combo", new CheckBox("Use R"));
                comboMenu.Add("lucian.combo.start.e", new CheckBox("Start Combo With E"));
            }

            harassMenu = Config.AddSubMenu(":: Harass Settings", ":: Harass Settings");
            
            {
                harassMenu.Add("lucian.q.harass", new CheckBox("Use Q"));
                harassMenu.Add("lucian.q.type", new ComboBox("Harass Type", 0, "Extended", "Normal"));
                harassMenu.Add("lucian.w.harass", new CheckBox("Use W"));
                harassMenu.Add("lucian.harass.mana", new Slider("Min. Mana", 50, 1, 99));
                harassMenu.AddGroupLabel(":: Q Whitelist (Extended)");
                {
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValid))
                    {
                        harassMenu.Add("lucian.white" + enemy.ChampionName, new CheckBox("(Q) " + enemy.ChampionName));
                    }
                }
            }
            clearMenu = Config.AddSubMenu(":: Clear Settings", ":: Clear Settings");
            {
                clearMenu.Add("lucian.q.clear", new CheckBox("Use Q"));
                clearMenu.Add("lucian.w.clear", new CheckBox("Use W"));
                clearMenu.Add("lucian.q.minion.hit.count", new Slider("(Q) Min. Minion Hit", 3, 1, 5));
                clearMenu.Add("lucian.w.minion.hit.count", new Slider("(W) Min. Minion Hit", 3, 1, 5));
                clearMenu.Add("lucian.clear.mana", new Slider("Min. Mana", 50, 1, 99));
            }

            jungleMenu = Config.AddSubMenu(":: Jungle Settings", ":: Jungle Settings");
            {
                jungleMenu.Add("lucian.q.jungle", new CheckBox("Use Q"));
                jungleMenu.Add("lucian.w.jungle", new CheckBox("Use W"));
                jungleMenu.Add("lucian.e.jungle", new CheckBox("Use E"));
                jungleMenu.Add("lucian.jungle.mana", new Slider("Min. Mana", 50, 1, 99));
            }

            killStealMenu = Config.AddSubMenu(":: KillSteal Settings", ":: KillSteal Settings");
            {
                killStealMenu.Add("lucian.q.ks", new CheckBox("Use Q"));
                killStealMenu.Add("lucian.w.ks", new CheckBox("Use W"));
            }

            miscMenu = Config.AddSubMenu(":: Miscellaneous", ":: Miscellaneous");
            {
                miscMenu.AddGroupLabel("Anti-Gapclose Settings");
                {
                    miscMenu.Add("lucian.e.gapclosex", new ComboBox("(E) Anti-Gapclose", 1, "On", "Off"));
                    miscMenu.AddGroupLabel("Custom Anti-Gapcloser");
                    foreach (var gapclose in AntiGapcloseSpell.GapcloseableSpells.Where(x => ObjectManager.Get<AIHeroClient>().Any(y => y.ChampionName == x.ChampionName && y.IsEnemy)))
                    {
                        miscMenu.Add("gapclose." + gapclose.ChampionName, new CheckBox("Anti-Gapclose: " + gapclose.ChampionName + " - Spell: " + gapclose.Slot));
                        miscMenu.Add("gapclose.slider." + gapclose.SpellName, new Slider("" + gapclose.ChampionName + " - Spell: " + gapclose.Slot + " Priorty", gapclose.DangerLevel, 1, 5));
                    }
                }
            }

            drawMenu = Config.AddSubMenu(":: Draw Settings", ":: Draw Settings");
            {
                drawMenu.AddGroupLabel(":: Skill Draws");
                {
                    drawMenu.Add("lucian.q.draw", new CheckBox("Q Range"));
                    drawMenu.Add("lucian.q2.draw", new CheckBox("Q (Extended) Range"));
                    drawMenu.Add("lucian.w.draw", new CheckBox("W Range"));
                    drawMenu.Add("lucian.e.draw", new CheckBox("E Range"));
                    drawMenu.Add("lucian.r.draw", new CheckBox("R Range"));
                    //skillDraw.AddItem(new MenuItem("lucian.lock.noti", "Lock Notification").SetValue(true));
                }
            }
            //Config.AddItem(new MenuItem("lucian.ult.lock", "(R) Lock Target").SetValue(true));
            Config.Add("lucian.semi.manual.ult", new KeyBind("Semi-Manual (R)!", false, KeyBind.BindTypes.HoldActive, 'A'));
            //var drawDamageMenu = new MenuItem("RushDrawEDamage", "Combo Damage").SetValue(true);
            //var drawFill = new MenuItem("RushDrawEDamageFill", "Combo Damage Fill").SetValue(new Circle(true, Color.Gold));

            drawMenu.AddGroupLabel(":: Damage Draws");
            drawMenu.Add("RushDrawEDamage", new CheckBox("Combo Damage"));

            DamageIndicator.Initialize(LucianCalculator.LucianTotalDamage);
            
        }
    }
}
