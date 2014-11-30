using System;
using System.Collections.Generic;
using System.Linq;
using Assemblies.Utilitys;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Assemblies.Champions {
    internal class Gnar : Champion {
        private Spell eMega;
        private Spell qMega;

        public Gnar() {
            loadMenu();
            loadSpells();

            Game.OnGameUpdate += onUpdate;
            Drawing.OnDraw += onDraw;
            Game.PrintChat("[Assemblies] - Gnar Loaded.");
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 1100f);
            Q.SetSkillshot(0.066f, 60f, 1400f, false, SkillshotType.SkillshotLine);

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
            menu.SubMenu("combo").AddItem(
                new MenuItem("minEnemies", "Enemies for R").SetValue(new Slider(2, 1, 5)));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useEH", "Use E in harass").SetValue(false));

            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQL", "Use Q in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useEL", "Use E in laneclear").SetValue(false));

            menu.AddSubMenu(new Menu("Killsteal Options", "killsteal"));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useQK", "Use Q in killsteal").SetValue(true));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useEK", "Use E in killsteal").SetValue(true));

            menu.AddSubMenu(new Menu("Flee Options", "flee"));
            menu.SubMenu("flee").AddItem(new MenuItem("useEF", "Use E in flee").SetValue(true));

            menu.AddSubMenu(new Menu("Drawing Options", "drawing"));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw E Range").SetValue(true));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("unitHop", "Always bounce off units for flee").SetValue(true));
            menu.SubMenu("misc").AddItem(
                new MenuItem("throwPos", "Position to throw enemies").SetValue(
                    new StringList(new[] {"Closest Wall", "Mouse Position", "Closest Turret", "Closest Ally"})));
            menu.SubMenu("misc").AddItem(new MenuItem("alwaysR", "Always Ult if killable").SetValue(true));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;

            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

            switch (xSLxOrbwalker.CurrentMode) {
                case xSLxOrbwalker.Mode.Combo:
                    doCombo(target);
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    doHarass(target);
                    break;
                case xSLxOrbwalker.Mode.Flee:
                    unitFlee();
                    break;
            }
        }

        private void doCombo(Obj_AI_Hero target) {
            //TODO le combo modes

            if (R.IsReady() && target.IsValidTarget(R.Width)) {
                if (isMenuEnabled(menu, "useRC")) {
                    castR(target);
                }
            }

            if (Q.IsReady() && target.IsValidTarget(Q.Range) &&
                Q.GetPrediction(target, true).Hitchance >= HitChance.Medium) {
                if (isMenuEnabled(menu, "useQC"))
                    Q.Cast(target, true, true);
            }
            if (qMega.IsReady() && target.IsValidTarget(qMega.Range) &&
                qMega.GetPrediction(target).Hitchance >= HitChance.Medium) {
                if (isMenuEnabled(menu, "useQC"))
                    qMega.Cast(target, true);
            }

            if (W.IsReady() && target.IsValidTarget(W.Range) && player.Distance(target) < W.Range) {
                if (isMenuEnabled(menu, "useWC"))
                    W.Cast(target, true);
            }

            if (E.IsReady() && target.IsValidTarget(E.Range)) {
                if (isMenuEnabled(menu, "useEC"))
                    E.Cast(target, true);
            }
        }

        private void doHarass(Obj_AI_Hero target) {
            if (Q.IsReady() && target.IsValidTarget(Q.Range) &&
                Q.GetPrediction(target, true).Hitchance >= HitChance.Medium) {
                if (isMenuEnabled(menu, "useQH"))
                    Q.Cast(target, true, true);
            }
            if (qMega.IsReady() && target.IsValidTarget(qMega.Range) &&
                qMega.GetPrediction(target).Hitchance >= HitChance.Medium) {
                if (isMenuEnabled(menu, "useQH"))
                    qMega.Cast(target, true);
            }

            if (W.IsReady() && target.IsValidTarget(W.Range) && player.Distance(target) < W.Range) {
                if (isMenuEnabled(menu, "useWH"))
                    W.Cast(target, true);
            }

            if (E.IsReady() && target.IsValidTarget(E.Range)) {
                if (isMenuEnabled(menu, "useEH"))
                    E.Cast(target, true);
            }
        }

        private void castR(Obj_AI_Hero target) {
            if (!R.IsReady()) return;
            int mode = menu.Item("throwPos").GetValue<StringList>().SelectedIndex;

            switch (mode) {
                case 0:
                    foreach (
                        Obj_AI_Hero collisionTarget in
                            ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Width)))
                        CastRToCollision(collisionTarget);
                    break;
                case 1:
                    //Mouse position
                    foreach (
                        Obj_AI_Hero collisionTarget in
                            ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Width)))
                        if (unitCheck(Game.CursorPos)) {
                            R.Cast(Game.CursorPos);
                        }

                    break;
                case 2:
                    //Closest Turret
                    foreach (
                        Obj_AI_Hero collisionTarget in
                            ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Width))) {
                        //975 Turret Range
                        //425 Push distance (Idk if it is correct);
                        Obj_AI_Turret Turret =
                            ObjectManager.Get<Obj_AI_Turret>().First(
                                tu => tu.IsAlly && tu.Distance(collisionTarget) <= 975 + 425 && tu.Health > 0);
                        if (Turret.IsValid && unitCheck(Turret.Position)) {
                            R.Cast(Turret.Position);
                        }
                    }
                    break;
                case 3:
                    //Closest Ally
                    foreach (
                        Obj_AI_Hero collisionTarget in
                            ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Width))) {
                        //975 Turret Range
                        //425 Push distance (Idk if it is correct);
                        Obj_AI_Hero ally =
                            ObjectManager.Get<Obj_AI_Hero>().First(
                                tu => tu.IsAlly && tu.Distance(collisionTarget) <= 425 + 65 && tu.Health > 0);
                        if (ally.IsValid && unitCheck(ally.Position)) {
                            R.Cast(ally.Position);
                        }
                    }
                    break;
            }
        }

        private bool unitCheck(Vector3 EndPosition) {
            List<Vector2> Points = GRectangle(player.Position.To2D(), EndPosition.To2D(), R.Width);
            var Poly = new Polygon(Points);
            int num = 0;
            foreach (
                Obj_AI_Hero collisionTarget in
                    ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Width))) {
                if (Poly.Contains(collisionTarget.Position.To2D())) {
                    num++;
                }
            }
            if (num < menu.Item("minEnemies").GetValue<Slider>().Value) return false;
            return true;
        }

        private void CastRToCollision(Obj_AI_Hero target) {
            Vector3 center = player.Position;
            const int points = 36;
            const int radius = 300;
            const double slice = 2*Math.PI/points;
            for (int i = 0; i < points; i++) {
                double angle = slice*i;
                var newX = (int) (center.X + radius*Math.Cos(angle));
                var newY = (int) (center.Y + radius*Math.Sin(angle));
                var position = new Vector3(newX, newY, 0);
                if (isWall(position) && unitCheck(position))
                    R.Cast(position, true);
            }
        }

        private void unitFlee() {
            if (!E.IsReady() && !eMega.IsReady()) return;

            List<Obj_AI_Base> minions = MinionManager.GetMinions(player.Position, E.Range, MinionTypes.All,
                MinionTeam.All,
                MinionOrderTypes.None);
            Obj_AI_Base bestMinion = null;

            foreach (Obj_AI_Base jumpableUnit in minions) {
                if (jumpableUnit.Distance(Game.CursorPos) <= 300 && player.Distance(jumpableUnit) <= E.Range)
                    bestMinion = jumpableUnit;
            }

            if (bestMinion != null && bestMinion.IsValid) {
                E.Cast(bestMinion, true);
            }
        }

        private void onDraw(EventArgs args) {}

        //Credits to Andreluis
        public List<Vector2> GRectangle(Vector2 startVector2, Vector2 endVector2, float radius) {
            var points = new List<Vector2>();

            Vector2 v1 = endVector2 - startVector2;
            Vector2 to1Side = Vector2.Normalize(v1).Perpendicular()*radius;

            points.Add(startVector2 + to1Side);
            points.Add(startVector2 - to1Side);
            points.Add(endVector2 - to1Side);
            points.Add(endVector2 + to1Side);
            return points;
        }
    }
}