using System;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies.Champions {
    internal class Irelia : Champion {
        public Irelia() {
            loadMenu();
            loadSpells();
            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ||
                  Utility.Map.GetMap()._MapType == Utility.Map.MapType.CrystalScar
                ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            Game.OnGameUpdate += onUpdate;
            Drawing.OnDraw += onDraw;
            Game.PrintChat("[Assemblies] - Irelia Loaded.");

            var wc = new WebClient {Proxy = null};

            wc.DownloadString("http://league.square7.ch/put.php?name=iIrelia");
            string amount = wc.DownloadString("http://league.square7.ch/get.php?name=iIrelia");
            Game.PrintChat("[Assemblies] - Irelia has been loaded " + Convert.ToInt32(amount) +
                           " times by LeagueSharp Users.");
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 0);
            R = new Spell(SpellSlot.R, 0);

            //TODO set skillshots
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            //TODO all menu
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;

            switch (XSLxOrbwalker.CurrentMode) {
                case XSLxOrbwalker.Mode.Combo:
                    //TODO onCombo
                    break;
            }
        }

        private void onDraw(EventArgs args) {
            //TODO you know...
        }
    }
}