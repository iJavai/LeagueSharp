using System;
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
            Obj_AI_Hero.OnProcessSpellCast += onSpellCast;
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
                case xSLxOrbwalker.Mode.Flee:
                    unitFlee();
                    break;
            }
        }

        private void doCombo(Obj_AI_Hero target) {
            //TODO le combo modes
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

            if (R.IsReady() && target.IsValidTarget(R.Width)) {
                if (isMenuEnabled(menu, "useRC")) {
                    castR(target);
                }
            }
        }

        private void castR(Obj_AI_Hero target) {
            if (target == null || !target.IsValidTarget(R.Width)) return;

            Obj_AI_Turret closestTower =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(tur => tur.IsAlly)
                    .OrderBy(tur => tur.Distance(player.Position))
                    .First();
            Obj_AI_Hero allyHero =
                ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly).OrderBy(
                    hero => hero.Distance(player.Position)).First();
            if (player.GetSpellDamage(target, SpellSlot.R) - 10 > target.Health && menu.Item("alwaysR").GetValue<bool>())
                R.Cast(target, true);

            switch (menu.Item("throwPos").GetValue<StringList>().SelectedIndex) {
                case 0: // wall
                    foreach (
                        Obj_AI_Hero collisionTarget in
                            ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Width)))
                        CastRToCollision(collisionTarget);
                    break;

                case 1: // mouse
                    if (R.IsReady())
                        R.Cast(Game.CursorPos.To2D(), true);
                    break;

                case 2: // closest tower
                    //TODO: check if will land under turret
                    if (closestTower.Distance(target) <= 800 && R.IsReady())
                        R.Cast(closestTower.Position, true);
                    break;

                case 3: // closest ally
                    if (allyHero.IsValid && R.IsReady())
                        R.Cast(allyHero.Position, true);
                    break;
            }
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
                var p = new Vector3(newX, newY, 0);
                if (isWall(p))
                    R.Cast(p, true);
            }
        }

        private void unitFlee() {
            if (!E.IsReady() && !eMega.IsReady()) return;

            /*foreach (
                Obj_AI_Base minion in
                    MinionManager.GetMinions(player.Position, E.Range, MinionTypes.All, MinionTeam.All,
                        MinionOrderTypes.None).Where(minion => minion != null && minion.IsValid &&
                                                               Vector3.DistanceSquared(player.Position, minion.Position) <=
                                                               E.Range*E.Range)) {
                E.Cast(minion, true);
            }*/
        }

        private void onDraw(EventArgs args) {}
    }
}