//TODO auto R KS for stacks reset maybe
//AUtoShroud erm, fake recall in top lane? :3
//Flee mode using jungle camps or miniions
//Maybe different combo modes switchable using StringList ofc
/* 
Combo:
Use Q -> R -> E -> AA -> W, not AA when in shroud but if killable
If not in Q range
Use R -> Q -> E -> AA -> W, not AA when in shroud but if killable

*/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

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

            menu.AddSubMenu(new Menu("Miscellaneous", "misc"));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("escape", "Escape key").SetValue(new KeyBind('G', KeyBindType.Press)));
            menu.SubMenu("misc").AddItem(new MenuItem("RCounter", "Do not escape if R<").SetValue(new Slider(1, 1, 3)));
            //TODO items

            Game.PrintChat("Akali by iJava, Princer007 and DZ191 Loaded.");
        }

        private void onUpdate(EventArgs args) {
            if (menu.SubMenu("misc").Item("escape").GetValue<KeyBind>().Active) Escape();
        }

        private void onDraw(EventArgs args) {
            if (menu.SubMenu("misc").Item("escape").GetValue<KeyBind>().Active)
                Utility.DrawCircle(Game.CursorPos, 150, W.IsReady() ? Color.Blue : Color.Red, 2);
        }

        private void Combo(EventArgs args) {
            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    //TODO princers combo when i get homerino
                    
                    break;
            }
        }

        private void castQ() {
            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null || !target.IsValidTarget(Q.Range)) return;
            if (Q.IsReady())
                Q.Cast(target, true);
        }

        private void castE() {
            Obj_AI_Hero target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target == null || !target.IsValidTarget(E.Range)) return;
            if (E.IsReady()) {
                E.Cast(target, true);
            } // TODO: AutoAttack this cunt
        }

        private void castR() {
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (target == null || !target.IsValidTarget(R.Range)) return;
            if (R.IsReady()) {
                R.Cast(target, true);
            }
        }

        private void castREscape() {
            Obj_AI_Base target = MinionManager.GetMinions(player.Position, 800, MinionTypes.All, MinionTeam.NotAlly)[0];
            foreach (
                Obj_AI_Base minion in
                    MinionManager.GetMinions(player.Position, 800, MinionTypes.All, MinionTeam.NotAlly))
                if (player.Distance(target) < player.Distance(minion))
                    target = minion;
            if (R.IsReady() && R.InRange(target.Position) && target.Distance(Game.CursorPos) < 150)
                R.Cast(target, true);
        }

        private void Escape() {
            Vector3 cursorPos = Game.CursorPos;
            Vector2 pos = V2E(player.Position, cursorPos, R.Range);
            Vector2 pass = V2E(player.Position, cursorPos, 120);
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pass.X, pass.Y)).Send();
            if (menu.SubMenu("misc").Item("RCounter").GetValue<Slider>().Value > ultiCount()) return;
            if (!IsWall(pos) && IsPassWall(player.Position, pos.To3D()))
                if (W.IsReady()) W.Cast(V2E(player.Position, cursorPos, W.Range));
            castREscape();
        }

        private static bool IsPassWall(Vector3 start, Vector3 end) {
            double count = Vector3.Distance(start, end);
            for (uint i = 0; i <= count; i += 10) {
                Vector2 pos = V2E(start, end, i);
                if (IsWall(pos)) return true;
            }
            return false;
        }

        private int ultiCount() {
            return (from buff in player.Buffs where buff.Name == "AkaliShadowDance" select buff.Count).FirstOrDefault();
        }

        private static bool IsWall(Vector2 pos) {
            return (NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Wall ||
                    NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Building);
        }

        private static Vector2 V2E(Vector3 from, Vector3 direction, float distance) {
            return from.To2D() + distance*Vector3.Normalize(direction - from).To2D();
        }
    }
}