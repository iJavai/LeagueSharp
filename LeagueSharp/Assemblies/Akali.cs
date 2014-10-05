using System;
using LeagueSharp;
using LeagueSharp.Common;

//TODO auto R KS for stacks reset maybe
//AUtoShroud erm, fake recall in top lane? :3
//Flee mode using jungle camps or miniions
//Maybe different combo modes switchable using StringList ofc


namespace Assemblies {
    internal class Akali : Champion {
        public Akali() {
            if (player.ChampionName != "Akali") {
                return;
            }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;

            Game.PrintChat("[Assemblies] - Akali Loaded. Swag.");
        }


        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 800);
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

            //TODO items

            Game.PrintChat("Akali by iJava, Princer007 and DZ191 Loaded.");
        }

        private void onUpdate(EventArgs args) {
            //TODO combo.
        }

        private void onDraw(EventArgs args) {
            throw new NotImplementedException();
        }
        
        private void Combo(EventArgs args) {
            throw new NotImplementedException();
        }
        
        private void CastR(Obj_AI_Base target, bool escape) {
            throw new NotImplementedException();
        }
        
        
        
        private static bool IsPassWall(Vector3 start, Vector3 end)
        {
            double count = Vector3.Distance(start, end);
            for (uint i = 0; i <= count; i += 10)
            {
                Vector2 pos = V2E(start, end, i);
                if (IsWall(pos)) return true;
            }
            return false;
        }
        
        private static bool IsWall(Vector2 pos)
        {
            return (NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Wall || NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Building);
        }
        
    }
}
