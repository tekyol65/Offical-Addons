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
    internal class JungleStealModule : IModule
    {
        public void OnLoad()
        {
            Console.WriteLine("Jungle Steal Module Loaded");
        }

        public string GetName()
        {
            return "JungleSteal";
        }

        public bool ShouldGetExecuted()
        {
            return SpellManager.Spell[SpellSlot.E].IsReady() &&
                   Kalita.modulesMenu["com.ikalista.modules." + GetName().ToLowerInvariant()].Cast<CheckBox>().CurrentValue;
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            var minion =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        x =>
                            SpellManager.Spell[SpellSlot.E].IsInRange(x) &&
                            x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range));

            if (minion == null || minion.CharData.BaseSkinName.Contains("Mini") ||
                !minion.CharData.BaseSkinName.Contains("SRU_"))
                return;
            if (!Kalita.JungleMinions.Contains(minion.CharData.BaseSkinName) ||
                !Kalita.jungleStealMenu[minion.CharData.BaseSkinName].Cast<CheckBox>().CurrentValue) return;
            if (minion.IsMobKillable())
            {
                SpellManager.Spell[SpellSlot.E].Cast();
            }
        }
    }

}
