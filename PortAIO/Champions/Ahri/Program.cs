using System;
using EloBuddy.SDK.Events;

namespace PortAIO.Champions.Ahri
{
    class Program
    {
        public static Helper Helper;
        
        internal static void Load()
        {
            Helper = new Helper();
            new Ahri();
        }
    }
}
