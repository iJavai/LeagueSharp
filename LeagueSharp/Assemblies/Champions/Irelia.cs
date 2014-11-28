using System;
using System.Collections.Generic;
using System.Linq;
using Assemblies.Utilitys;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies.Champions {
    internal class Irelia : Champion {
        public int NumberR;
        public float PredictedArrivalTime;
        public bool QCastedMinion;
        public Obj_AI_Base SelectedMinion;

        public Irelia() {
            loadMenu();
            loadSpells();

            Game.OnGameUpdate += onUpdate;
            Drawing.OnDraw += onDraw;
            xSLxOrbwalker.BeforeAttack += beforeAttack;
            Game.PrintChat("[Assemblies] - Irelia Loaded.");
        }

        private void beforeAttack(xSLxOrbwalker.BeforeAttackEventArgs args) {
            if (args.Unit.IsMe) {
                if (isMenuEnabled(menu, "useWC") && W.IsReady() && args.Target.IsValidTarget(Q.Range) &&
                    !args.Target.IsMinion)
                    W.Cast(true);
            }
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);

            R.SetSkillshot(0.15f, 80f, 1500f, false, SkillshotType.SkillshotLine);

            //TODO set skillshots
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use E in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("gapcloseQ", "Use Q to gapclose").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("OStunE", "Only Use E to stun").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useEH", "Use E in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useRH", "Use R in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("harassSlider", "hp to harass R").SetValue(new Slider(30)));

            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQL", "Use Q in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useWL", "Use W in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useEL", "Use E in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useRL", "Use R in laneclear").SetValue(true));

            menu.AddSubMenu(new Menu("Killsteal Options", "killsteal"));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useQKS", "Use Q in killsteal").SetValue(true));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useWKS", "Use W in killsteal").SetValue(true));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useEKS", "Use E in killsteal").SetValue(true));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useRKS", "Use R in killsteal").SetValue(true));

            menu.AddSubMenu(new Menu("Flee Options", "flee"));
            menu.SubMenu("flee").AddItem(new MenuItem("useQF", "Use Q to flee").SetValue(true));
            menu.SubMenu("flee").AddItem(new MenuItem("useRF", "Use R to lower minions in flee").SetValue(false));

            menu.AddSubMenu(new Menu("Drawing Options", "drawing"));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q Range").SetValue(true));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawW", "Draw W Range").SetValue(true));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw E Range").SetValue(true));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw R Range").SetValue(true));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            //TODO idk ?

            menu.AddItem(new MenuItem("creds", "Made by iJabba & DZ191"));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;
            Obj_AI_Hero target = SimpleTs.GetTarget(isMenuEnabled(menu, "gapcloseQ") ? Q.Range*3 : Q.Range,
                SimpleTs.DamageType.Physical);

            /*foreach (BuffInstance buff in player.Buffs) {
                Game.PrintChat(buff.Name);   
            }*/

            switch (xSLxOrbwalker.CurrentMode) {
                case xSLxOrbwalker.Mode.Combo:
                    doCombo(target);
                    break;
                case xSLxOrbwalker.Mode.LaneClear:
                    laneclear();
                    break;
            }
        }

        private void doCombo(Obj_AI_Hero target) {
            //xSLxOrbwalker.ForcedTarget = target; TODO not needed m8
            if (isMenuEnabled(menu, "gapcloseQ") && player.Distance(target) > Q.Range) {
                List<Obj_AI_Base> jumpMinions = MinionManager.GetMinions(player.Position, Q.Range);
                foreach (Obj_AI_Base minion in jumpMinions) {
                    if (Q.IsReady() && Q.GetDamage(minion) >= minion.Health &&
                        minion.Distance(target.Position) < Q.Range && minion.IsValidTarget(Q.Range*3)) {
                        Q.Cast(minion, true);
                    }
                }
            }
            //TODO: note, removed the else statment, because not rly needed it may cause problems when target is actually in Q Range and doesn't need to be gapclosed.
            if (isMenuEnabled(menu, "useQC") && player.Distance(target) <= Q.Range) {
                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                    Q.Cast(target, true);
            }

            /*if (isMenuEnabled(menu, "useWC") && W.IsReady()) {
                W.Cast(true);
            }* TODO before attack mate/ */
            if (isMenuEnabled(menu, "OStunE")) {
                if (canStun(target) && E.IsReady() && player.Distance(target) <= E.Range) {
                    E.Cast(target, true);
                }
            }
            else {
                if (E.IsReady() && player.Distance(target) <= E.Range) {
                    E.Cast(target, true);
                }
            }

            //TODO find buff name for keeping count of charges? == IreliaTranscendentBlades
            if (R.IsReady() && player.Distance(target) <= R.Range) {
                if (isMenuEnabled(menu, "useRC")) {
                    PredictionOutput rPrediction = R.GetPrediction(target, true);
                    if (rPrediction.Hitchance >= HitChance.Medium)
                        R.Cast(rPrediction.UnitPosition, true);
                }
            }
        }


        private void SuperDuperOpChaseMode(Obj_AI_Hero target) {
            if (!SelectedMinion.IsValid || !R.IsReady()) {
                SelectedMinion = null;
                NumberR = 0;
            }
            if (SelectedMinion.IsValidTarget(R.Range) && R.IsReady() && NumberR > 0) {
                if (SelectedMinion != null) {
                    R.Cast(SelectedMinion.Position);
                    NumberR -= 1;
                    PredictedArrivalTime = Game.Time + (player.Distance(SelectedMinion)/R.Speed);
                }
            }
            if (NumberR == 0 && Q.IsReady() && SelectedMinion.IsValidTarget(Q.Range) && !QCastedMinion &&
                Game.Time > PredictedArrivalTime) {
                Q.Cast(SelectedMinion);
                QCastedMinion = true;
                PredictedArrivalTime = Game.Time;
            }
            if (QCastedMinion && Q.IsReady() && target.IsValidTarget(Q.Range)) {
                Q.Cast(target);
                QCastedMinion = false;
                return;
            }
            if (SelectedMinion == null) {
                List<Obj_AI_Base> MinionList = MinionManager.GetMinions(player.Position, Q.Range).ToList();
                var List2 = MinionList.Where(min => min.Distance(target) <= Q.Range).ToList();
                if (!List2.Any()) return;
                IOrderedEnumerable<Obj_AI_Base> List3 = List2.OrderBy(m => m.Distance(target));
                Obj_AI_Base minion = List3.First();
                int rCount = getNumberOfR(minion);
                if (rCount == 0 && Q.IsReady() && minion.IsValidTarget(Q.Range)) {
                    Q.Cast(minion);
                    QCastedMinion = true;
                }
                else {
                    SelectedMinion = minion;
                    NumberR = rCount;
                }
            }
        }

        private int getNumberOfR(Obj_AI_Base target) {
            int rCount = 0;
            float targetHealth = target.Health;
            while (targetHealth >= Q.GetDamage(target)) {
                rCount += 1;
                targetHealth -= Q.GetDamage(target);
            }
            return rCount;
        }

        private int getUltStacks() {
            foreach (BuffInstance buff in player.Buffs.Where(buff => buff.Name == "IreliaTranscendentBlades")) {
                return buff.Count; // might need to be buff.count - 1
            }
            return 4; // idk might work :^)
        }

        private void onDraw(EventArgs args) {}

        private bool canStun(Obj_AI_Hero target) {
            return target.Health/target.MaxHealth*100 > player.Health/player.MaxHealth*100;
        }

        private void laneclear() {
            List<Obj_AI_Base> minions = MinionManager.GetMinions(player.Position, Q.Range);
            foreach (Obj_AI_Base minion in minions) {
                if (isMenuEnabled(menu, "useWL") && W.IsReady()) {
                    W.Cast(true);
                }
                if (minion.Distance(player) <= Q.Range && Q.GetDamage(minion) >= minion.Health &&
                    isMenuEnabled(menu, "useQL")) {
                    Q.Cast(minion, true);
                }
                if (minion.Distance(player) <= E.Range && isMenuEnabled(menu, "useEL") && E.IsReady()) {
                    E.Cast(minion, true);
                }
                if (R.IsReady() && isMenuEnabled(menu, "useRL") && minion.Distance(player) <= R.Range) {
                    R.Cast(minion, true);
                }
            }
        }
    }
}