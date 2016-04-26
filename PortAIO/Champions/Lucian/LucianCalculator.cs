using EloBuddy;
using LeagueSharp.Common;

namespace PortAIO.Champions.Lucian
{
    class LucianCalculator
    {
        public static float LucianTotalDamage(AIHeroClient enemy)
        {
            if (LucianSpells.Q.IsReady() && LucianMenu.lucianqcombo)
            {
                return LucianSpells.Q.GetDamage(enemy);
            }
            if (LucianSpells.W.IsReady() && LucianMenu.lucianwcombo)
            {
                return LucianSpells.W.GetDamage(enemy);
            }
            if (LucianSpells.R.IsReady() && LucianMenu.lucianrcombo)
            {
                return LucianSpells.R.GetDamage(enemy);
            }
            return 0;
        }
    }
}
