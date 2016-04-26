using System;
using System.Drawing;
using EloBuddy;
using LeagueSharp.Common;
using PortAIO.Lib;

namespace PortAIO.Champions.Lucian
{
    class LucianDrawing
    {

        public static void Init()
        {

            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (LucianMenu.lucianqdraw && LucianSpells.Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.Q.Range, Color.Gold);
            }
            if (LucianMenu.lucianq2draw && LucianSpells.Q2.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.Q2.Range, Color.Gold);
            }
            if (LucianMenu.lucianwdraw && LucianSpells.W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.W.Range, Color.Gold);
            }
            if (LucianMenu.lucianedraw && LucianSpells.E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.E.Range, Color.Gold);
            }
            if (LucianMenu.lucianrdraw && LucianSpells.R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.R.Range, Color.Gold);
            }

            DamageIndicator.HealthbarEnabled = LucianMenu.RushDrawEDamage;
        }
    }
}
