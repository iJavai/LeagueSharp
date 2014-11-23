using System;
using System.Net;
using Assemblies.Utilitys;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies.Champions {
    internal class Irelia : Champion {
        public Irelia() {
            loadMenu();
            loadSpells();

            Game.OnGameUpdate += onUpdate;
            Drawing.OnDraw += onDraw;
            Game.PrintChat("[Assemblies] - Irelia Loaded.");
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W, xSLxOrbwalker.GetAutoAttackRange());
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);

            R.SetSkillshot(0.15f, 80f, 1500f, false, SkillshotType.SkillshotLine); // fix new prediction

            //TODO set skillshots
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));

            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));

            menu.AddSubMenu(new Menu("Killsteal Options", "killsteal"));

            menu.AddSubMenu(new Menu("Flee Options", "flee"));

            menu.AddSubMenu(new Menu("Drawing Options", "drawing"));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));

            menu.AddItem(new MenuItem("creds", "Made by iJabba & DZ191"));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;

            switch (xSLxOrbwalker.CurrentMode) {
                case xSLxOrbwalker.Mode.Combo:
                    //TODO onCombo
                    break;
            }
        }

        private void onDraw(EventArgs args) {} 

        private bool canStun(Obj_AI_Hero target) {
            return getPercentValue(target, false) > getPercentValue(player, false);
        }
    }
}