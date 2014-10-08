using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    internal class Champion : ChampionUtils {
        protected readonly Obj_AI_Hero player = ObjectManager.Player;
        protected Spell E;
        protected Spell Q;
        protected Spell R;
        protected Spell W;
        protected Menu menu;
        protected Orbwalking.Orbwalker orbwalker;

        public Champion() {
            addBasicMenu();
        }

        private void addBasicMenu() {
            menu = new Menu("Assemblies - " + player.ChampionName, "Assemblies - " + ObjectManager.Player.ChampionName,
                true);
            var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);
            Menu orbwalking = menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            orbwalker = new Orbwalking.Orbwalker(orbwalking);
            //menu.Item("FarmDelay").SetValue(new Slider(0, 0, 200));

            menu.AddToMainMenu();
        }
    }
}