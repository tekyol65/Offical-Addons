#region

using System;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Events;

#endregion

namespace AutoLantern
{
    internal class Program
    {
        public static Menu Menu;
        public static SpellSlot LanternSlot = (SpellSlot)62;
        public static int LastLantern;

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        public static SpellDataInst LanternSpell
        {
            get { return Player.Spellbook.GetSpell(LanternSlot); }
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (!ThreshInGame())
            {
                return;
            }

            Menu = MainMenu.AddMenu("AutoLantern", "AutoLantern");
            Menu.Add("Auto", new CheckBox("Auto-Lantern at Low HP"));
            Menu.Add("Low", new Slider("Low HP Percent", 20, 10, 50));
            Menu.Add("Hotkey", new KeyBind("Hotkey", false, KeyBind.BindTypes.HoldActive, 32));
            Menu.Add("LanternReady", new CheckBox("Lantern Ready", false));
           
            Game.OnUpdate += OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender is AIHeroClient && sender.IsAlly && args.SData.Name.Equals("LanternWAlly"))
            {
                LastLantern = Utils.TickCount;
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (!IsLanternSpellActive())
            {
                Menu["LanternReady"].Cast<CheckBox>().CurrentValue = (false);
                return;
            }

            Menu["LanternReady"].Cast<CheckBox>().CurrentValue = (true);

            if (Menu["Auto"].Cast<CheckBox>().CurrentValue && IsLow() && UseLantern())
            {
                return;
            }

            if (!Menu["Hotkey"].Cast<KeyBind>().CurrentValue)
            {
                return;
            }

            UseLantern();
        }

        private static bool IsLanternSpellActive()
        {
            return LanternSpell != null && LanternSpell.Name.Equals("LanternWAlly");
        }

        private static bool UseLantern()
        {
            var lantern =
                ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(
                        o => o.IsValid && o.IsAlly && o.Name.Equals("ThreshLantern") && Player.LSDistance(o) <= 500);

            return lantern != null && lantern.IsVisible && Utils.TickCount - LastLantern > 5000 &&
                   Player.Spellbook.CastSpell(LanternSlot, lantern);
        }

        private static bool IsLow()
        {
            return Player.HealthPercent <= Menu["Low"].Cast<Slider>().CurrentValue;
        }

        private static bool ThreshInGame()
        {
            return HeroManager.Allies.Any(h => !h.IsMe && h.ChampionName.Equals("Thresh"));
        }
    }
}
