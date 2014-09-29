using System;
using System.Runtime.Remoting;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    internal class Program {
        public static Champion champion;
        public static Menu menu;


        private static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args) {
            try {
                ObjectHandle handle = Activator.CreateInstance(null,
                    "Assemblies." + ObjectManager.Player.ChampionName);
                champion = (Champion) handle.Unwrap();
            }
            catch {
                Console.WriteLine("Fail.");
            }
        }
    }
}