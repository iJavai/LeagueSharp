using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    internal class Akali : Champion {
        public Akali() {
            if (player.ChampionName != "Zed") {
                return;
            }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;

            Game.PrintChat("[Assemblies] - Akali Loaded." + "swag.");
        }


        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 900);
            Q.SetSkillshot(0.235f, 50f, 1700, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 550);

            E = new Spell(SpellSlot.E, 290);

            R = new Spell(SpellSlot.R, 600);
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(false));
            menu.SubMenu("harass").AddItem(new MenuItem("useEH", "Use E in harass").SetValue(false));

            Game.PrintChat("Akali by iJava, Princer007 and DZ191 Loaded.");
        }

        private void onUpdate(EventArgs args) {
            //TODO combo.
        }

        private void onDraw(EventArgs args) {
            throw new NotImplementedException();
        }
    }
}