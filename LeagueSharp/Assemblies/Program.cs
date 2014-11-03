using System;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    internal static class Program {
        private static Champion champion;
        private static Version Version;

        private static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Version = Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static void Game_OnGameLoad(EventArgs args) {
            checkVersion();
            try {
                switch (ObjectManager.Player.ChampionName) {
                    case "Ezreal":
                        champion = new Ezreal();
                        break;
                    case "Vayne":
                        champion = new VayneHunter();
                        break;
                    case "Fizz":
                        champion = new Fizz();
                        break;
                    default:
                        champion = new Champion();
                        break;
                }
            }
            catch {
                Console.WriteLine("Fail.");
            }
        }

        private static void checkVersion() {
            Task.Factory.StartNew(() => {
                try {
                    using (var client = new WebClient()) {
                        string rawVersion =
                            client.DownloadString(
                                "https://raw.githubusercontent.com/iJavai/LeagueSharp/master/LeagueSharp/Assemblies/Properties/AssemblyInfo.cs");
                        Match match =
                            new Regex(@"\[assembly\: AssemblyVersion\(""(\d{1,})\.(\d{1,})\.(\d{1,})\.(\d{1,})""\)\]")
                                .Match
                                (rawVersion);
                        if (match.Success) {
                            var gitVersion =
                                new Version(string.Format("{0}.{1}.{2}.{3}", match.Groups[1], match.Groups[2],
                                    match.Groups[3],
                                    match.Groups[4]));
                            if (gitVersion > Version) {
                                Game.PrintChat("<font color='#15C3AC'>Assemblies:</font> <font color='#FF0000'>" +
                                               "OUTDATED - Please Update to Version: " + gitVersion + "</font>");
                                Game.PrintChat("<font color='#15C3AC'>Assemblies:</font> <font color='#FF0000'>" +
                                               "OUTDATED - Please Update to Version: " + gitVersion + "</font>");
                                Game.PrintChat("<font color='#15C3AC'>Assemblies:</font> <font color='#FF0000'>" +
                                               "OUTDATED - Please Update to Version: " + gitVersion + "</font>");
                            }
                        }
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
            });
        }
    }
}