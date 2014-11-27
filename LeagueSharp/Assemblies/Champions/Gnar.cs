using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assemblies.Utilitys;
using LeagueSharp;

namespace Assemblies.Champions {
    class Gnar : Champion {

        public Gnar() {
            loadMenu();
            loadSpells();

            Game.OnGameUpdate += onUpdate;
            Drawing.OnDraw += onDraw;
            Game.PrintChat("[Assemblies] - Irelia Loaded.");
        }

        private void loadSpells() {
            
        }

        private void loadMenu() {
            
        }

        private void onUpdate(EventArgs args) {

        }

        private void onDraw(EventArgs args) {

        }
    }
}
