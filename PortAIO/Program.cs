﻿using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using LeagueSharp.Common;
using SharpDX;
using System;

namespace PortAIO
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Initialize;
        }
        
        static LeagueSharp.Common.Render.Sprite Intro;

        private static void Initialize(System.EventArgs args)
        {
            switch (ObjectManager.Player.ChampionName.ToLower())
            {
                case "kalista":
                    Champions.Kalista.Program.Init();
                    break;
                case "ahri":
                    Champions.Ahri.Program.Load();
                    break;
                case "vayne":
                    Intro = new Render.Sprite(LoadImg("logo"), new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
                    Intro.Add(0);
                    Intro.OnDraw();
                    Champions.Vayne.Program.OnLoad();
                    LeagueSharp.Common.Utility.DelayAction.Add(7000, () => Intro.Remove());
                    break;
                default:
                    return;
            }
        }

        private static System.Drawing.Bitmap LoadImg(string imgName)
        {
            var bitmap = Properties.Resources.ResourceManager.GetObject(imgName) as System.Drawing.Bitmap;
            if (bitmap == null)
            {
                Console.WriteLine(imgName + ".png not found.");
            }
            return bitmap;
        }
    }
}
