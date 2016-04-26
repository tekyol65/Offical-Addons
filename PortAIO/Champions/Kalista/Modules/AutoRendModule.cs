using System;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using PortAIO.Champions.Kalista.Utils;
using DZLib.Modules;
using EloBuddy.SDK.Menu.Values;

namespace PortAIO.Champions.Kalista.Modules
{
    internal class AutoRendModule : IModule
    {
        public string GetName()
        {
            return "AutoRend";
        }

        public bool ShouldGetExecuted()
        {
            return SpellManager.Spell[SpellSlot.E].IsReady() &&
                   Kalista.comboMenu["com.ikalista.combo.useE"].Cast<CheckBox>().CurrentValue;
        }

        public void OnExecute()
        {
            foreach (
                var source in
                    HeroManager.Enemies.Where(
                        x => x.IsValid && x.HasRendBuff() && SpellManager.Spell[SpellSlot.E].IsInRange(x)))
            {
                if (source.IsRendKillable() || source.GetRendBuffCount() >= Kalista.comboMenu["com.ikalista.combo.stacks"].Cast<Slider>().CurrentValue)
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnLoad()
        {
            Console.WriteLine("Auto Rend Module Loaded");
        }
    }

}
