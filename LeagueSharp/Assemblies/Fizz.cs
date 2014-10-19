// fizzmarinerdoombomb = R, fizzmarinerdoomslow = slow buff for R 

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

/**
 * IDEA - Flee mode over walls should be easy - ish, Cast E on first vector point then cast E2 on second vector point on the other side of the wall? :3
 *
 */

namespace Assemblies {
    internal class Fizz : Champion {
        private Spell E2;
        private bool isCalled;
        private FizzJump jumpStage; // 0 = playful, 1 = trickster :3
        private float time;

        public Fizz() {
            loadMenu();
            loadSpells();

            Game.OnGameUpdate += onUpdate;
            Obj_AI_Base.OnProcessSpellCast += onSpellCast;
            Game.PrintChat("[Assemblies] - Fizz Loaded.");
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use E in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("initR", "Initiate with R").SetValue(false));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(false));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("eDodge", "Use E to dodge spells").SetValue(false));
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 400);
            E2 = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 1200);

            E.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E2.SetSkillshot(0.5f, 400, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 250f, 1200f, false, SkillshotType.SkillshotLine);
        }

        private void onUpdate(EventArgs args) {
            if (time + 1f < Game.Time && !isCalled) {
                isCalled = true;
                jumpStage = FizzJump.PLAYFUL;
            }
            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    goFishyGo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //TODO harass
                    break;
            }
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            foreach (BuffInstance buff in target.Buffs.Where(buff => hasBuff(target, "fizzmarinerdoombomb"))) {
                Utility.DrawCircle(target.Position, 130, Color.Coral);
            }
            //Game.PrintChat(jumpStage == FizzJump.PLAYFUL ? "playful" : "trickster");
        }

        private void castEGapclose(Obj_AI_Hero target) {
            // 100% working add checks and shit ofc
            //Obj_AI_Hero target = SimpleTs.GetTarget(800, SimpleTs.DamageType.Magical); 
            if (target.IsValidTarget(800)) {
                if (E.IsReady() && player.Distance(target) > Q.Range) {
                    if (jumpStage == FizzJump.PLAYFUL && player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump")
                        E.Cast(target.ServerPosition, true);
                    //Gapclosing witrh e hopefully
                }
                if (jumpStage == FizzJump.TRICKSTER && player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo")
                    E2.Cast(target.ServerPosition, true);
            }
        }

        private void onSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            if (sender.IsMe) {
                if (args.SData.Name == "FizzJump") {
                    jumpStage = FizzJump.TRICKSTER;
                    time = Game.Time;
                    isCalled = false;
                }
            }
        }

        private void goFishyGo() {
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            PredictionOutput prediction = R.GetPrediction(target, true);

            if (target.IsValidTarget(R.Range)) {
                if (R.IsReady() && !isUnderEnemyTurret(target)) {
                    if (prediction.Hitchance >= HitChance.High && target.IsValidTarget()) {
                        R.Cast(target, true);
                    }
                }
                if (E.IsReady())
                    castEGapclose(target);
                if (W.IsReady())
                    W.Cast();
                if (Q.IsReady())
                    Q.CastOnUnit(target);
            }
        }

        private enum FizzJump {
            PLAYFUL,
            TRICKSTER
        }
    }
}