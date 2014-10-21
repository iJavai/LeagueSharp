using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;

namespace Assemblies {
    internal class Champion : ChampionUtils {
        protected readonly Obj_AI_Hero player = ObjectManager.Player;
        protected Spell E;
        protected Spell Q;
        protected Spell R;
        protected Spell W;
        protected Menu menu;
        protected Orbwalking.Orbwalker orbwalker;
        private WardJumper wardJumper;

        public Champion() {
            addBasicMenu();
            wardJumper = new WardJumper();
            if (wardJumper.isCompatibleChampion(player))
                wardJumper.AddToMenu(menu);
        }

        private void addBasicMenu() {
            menu = new Menu("Assemblies - " + player.ChampionName, "Assemblies - " + player.ChampionName,
                true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            //Orbwalker submenu
            var orbwalkerMenu = new Menu("LX-Orbwalker", "orbwalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            menu.AddSubMenu(orbwalkerMenu);

            menu.AddToMainMenu();
        }
    }
}