using System;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using PortAIO.Champions.Kalista.Utils;
using DZLib.Modules;
using EloBuddy.SDK.Menu.Values;

namespace PortAIO.Champions.Kalista.Modules
{
    class AutoELeavingModule : IModule
    {
        public void OnLoad()
        {
            Console.WriteLine("Leaving Module");
        }

        public string GetName()
        {
            return "AutoELeaving";
        }

        public bool ShouldGetExecuted()
        {
            return SpellManager.Spell[SpellSlot.E].IsReady() && Kalita.comboMenu["com.ikalista.combo.eLeaving"].Cast<CheckBox>().CurrentValue;
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            var target =
                HeroManager.Enemies
                    .FirstOrDefault(x => x.HasRendBuff() && SpellManager.Spell[SpellSlot.E].IsInRange(x));
            if (target == null) return;
            var damage = Math.Ceiling(Helper.GetRendDamage(target) * 100 / target.Health);
            if (damage >= Kalita.comboMenu["com.ikalista.combo.ePercent"].Cast<Slider>().CurrentValue && target.ServerPosition.LSDistance(ObjectManager.Player.ServerPosition, true) > Math.Pow(SpellManager.Spell[SpellSlot.E].Range * 0.8, 2))
            {
                SpellManager.Spell[SpellSlot.E].Cast();
            }
        }
    }
}
