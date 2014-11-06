using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Assemblies {
    internal class AntiRengar {
        private readonly Spell gapcloseSpell;
        private readonly Obj_AI_Hero player = ObjectManager.Player;
        private Menu menu;
        private Obj_AI_Hero rengarObject;

        public AntiRengar() {
            gapcloseSpell = getSpell();

            GameObject.OnCreate += onCreateObj;
        }

        public void AddToMenu(ref Menu attachMenu) {
            menu = attachMenu;
            menu.AddSubMenu(new Menu("Anti Rengar", "antiRengar"));
            menu.SubMenu("antiRengar").AddItem(new MenuItem("enabled", "Enabled").SetValue(true));

            Game.PrintChat("[Assemblies] - AntiRengar Loaded!");
        }

        public bool isCompitableChampion() {
            return player.ChampionName == "Vayne" || player.ChampionName == "Tristana";
        }

        private Spell getSpell() {
            switch (player.ChampionName) {
                case "Vayne":
                    return new Spell(SpellSlot.E);
                case "Tristana":
                    return new Spell(SpellSlot.R, 550);
                case "Draven":
                    return new Spell(SpellSlot.E, 1100);
            }
            return null;
        }

        private void gapcloserRengar() {
            if (rengarObject.ChampionName == "Rengar") {
                if (rengarObject.IsValidTarget(1000) && gapcloseSpell.IsReady() && rengarObject.Distance(player) <= gapcloseSpell.Range) {
                    gapcloseSpell.Cast(rengarObject, true);
                    Utility.DelayAction.Add(50, gapcloserRengar);
                }
            }
        }

        private void onCreateObj(GameObject Obj, EventArgs args) {
            if (Obj.Name == "Rengar_LeapSound.troy" && Obj.IsEnemy) {
                foreach (
                    Obj_AI_Hero enemy in
                        ObjectManager.Get<Obj_AI_Hero>().Where(
                            hero => hero.IsValidTarget(1500) && hero.ChampionName == "Rengar")) {
                    rengarObject = enemy;
                }
            }
            if (rengarObject != null && Vector3.DistanceSquared(player.Position, rengarObject.Position) < 1000*1000 &&
                menu.Item("enabled").GetValue<bool>()) {
                gapcloserRengar();
            }
        }
    }
}