using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu.Values;

namespace PortAIO.Champions.Lucian
{
    class Helper
    {

        public static void LucianAntiGapcloser(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            if (sender.IsEnemy && spell.End.LSDistance(ObjectManager.Player.Position) < LucianSpells.E.Range && !spell.SData.IsAutoAttack() && spell.Target.IsMe)
            {
                foreach (var gapclose in AntiGapcloseSpell.GapcloseableSpells.Where(x => spell.SData.Name == ((AIHeroClient)sender).GetSpell(x.Slot).Name).OrderByDescending(c => LucianMenu.miscMenu["gapclose.slider." + spell.SData.Name]))
                {
                    if (LucianMenu.miscMenu["gapclose." + ((AIHeroClient)sender).ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        LucianSpells.E.Cast(ObjectManager.Player.Position.LSExtend(spell.End, -LucianSpells.W.Range));
                    }
                }
            }
        }


    }
}
