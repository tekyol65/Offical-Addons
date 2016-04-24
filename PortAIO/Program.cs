using EloBuddy;
using EloBuddy.SDK.Events;

namespace PortAIO
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Initialize;
        }

        private static void Initialize(System.EventArgs args)
        {
            switch (ObjectManager.Player.ChampionName.ToLower())
            {
                case "kalista": // Sharpshooter Kalista
                    Champions.Kalista.Program.Init();
                    break;
                case "ahri":
                    Champions.Ahri.Program.Load(); // Beaving Ahri
                    break;            
                default:
                    return;
            }
        }
    }
}
