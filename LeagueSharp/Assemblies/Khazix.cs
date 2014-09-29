using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    internal class Khazix : Champion {

        public Khazix() {
            loadMenu();
            loadSpells();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Game.PrintChat("Loaded Assembly - "+player.ChampionName);
        }

        private void loadMenu() {}

        private void loadSpells() {}

        private void Game_OnGameUpdate(EventArgs args) {
            
        }

        private void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target) {}

        private void Drawing_OnDraw(EventArgs args) {}
    }
}