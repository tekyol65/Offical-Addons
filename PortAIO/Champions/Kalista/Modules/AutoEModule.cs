using System;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using PortAIO.Champions.Kalista.Utils;
using DZLib.Modules;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace PortAIO.Champions.Kalista.Modules
{
    internal class AutoEModule : IModule
    {
        public void OnLoad()
        {
            Console.WriteLine("Auto E Module Loaded");
        }

        public string GetName()
        {
            return "AutoEHarass";
        }

        public bool ShouldGetExecuted()
        {
            return SpellManager.Spell[SpellSlot.E].IsReady() &&
                   Kalista.comboMenu["com.ikalista.combo.autoE"].Cast<CheckBox>().CurrentValue;
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                var enemy = HeroManager.Enemies.Where(hero => hero.HasRendBuff()).MinOrDefault(hero => hero.Distance(ObjectManager.Player, true));
                if (enemy?.Distance(ObjectManager.Player, true) <
                    Math.Pow(SpellManager.Spell[SpellSlot.E].Range + 200, 2))
                {
                    if (
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Any(
                                x =>
                                    SpellManager.Spell[SpellSlot.E].IsInRange(x) && x.HasRendBuff() &&
                                    x.IsRendKillable()))
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }
                }
            }
        }
    }

}
