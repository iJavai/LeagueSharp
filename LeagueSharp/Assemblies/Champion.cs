using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    class Champion : ChampionUtils {

        public readonly Obj_AI_Hero player = ObjectManager.Player;
        private Menu menu;
        private Orbwalking.Orbwalker orbwalker;

        public Champion() {
            Game.PrintChat(player.ChampionName+" loaded");
            addBasicMenu();
        }

        private void addBasicMenu() {
            menu = new Menu("Assemblies", "Assemblies - " + ObjectManager.Player.ChampionName, true);
            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);
            Menu orbwalking = menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            orbwalker = new Orbwalking.Orbwalker(orbwalking);
            menu.Item("FarmDelay").SetValue(new Slider(0, 0, 200));
            menu.AddToMainMenu();
        }
    }
}
