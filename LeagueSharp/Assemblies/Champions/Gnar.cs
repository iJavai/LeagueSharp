using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies.Champions {
    internal class Gnar : Champion {
        private Spell eMega;
        private Spell qMega;

        public Gnar() {
            loadMenu();
            loadSpells();

            Game.OnGameUpdate += onUpdate;
            Drawing.OnDraw += onDraw;
            Game.PrintChat("[Assemblies] - Irelia Loaded.");
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 1100f);
            Q.SetSkillshot(0.066f, 60f, 1400f, true, SkillshotType.SkillshotLine);

            qMega = new Spell(SpellSlot.Q, 1100f);
            qMega.SetSkillshot(0.60f, 90f, 2100f, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 525f);
            W.SetSkillshot(0.25f, 80f, 1200f, false, SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E, 475f);
            E.SetSkillshot(0.695f, 150f, 2000f, false, SkillshotType.SkillshotCircle);

            eMega = new Spell(SpellSlot.E, 475f);
            eMega.SetSkillshot(0.695f, 350f, 2000f, false, SkillshotType.SkillshotCircle);

            R = new Spell(SpellSlot.R, 1f);
            R.SetSkillshot(0.066f, 400f, 1400f, false, SkillshotType.SkillshotCircle);
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use E in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));

            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));

            menu.AddSubMenu(new Menu("Killsteal Options", "killsteal"));

            menu.AddSubMenu(new Menu("Flee Options", "flee"));

            menu.AddSubMenu(new Menu("Drawing Options", "drawing"));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;

            Console.WriteLine(player.Spellbook.GetSpell(SpellSlot.Q).SData);
        }

        private void onDraw(EventArgs args) {}
    }
}