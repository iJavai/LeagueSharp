using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

// fizzmarinerdoombomb = R, fizzmarinerdoomslow = slow buff for R 
using SharpDX;

namespace Assemblies {
    internal class Fizz : Champion {
        private Spell E2;
        private FizzJump fizzJump;

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
            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (menu.Item("initR").GetValue<bool>() && menu.Item("useRC").GetValue<bool>())
                        goFishyGo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    //TODO harass
                    break;
            }
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            foreach (BuffInstance buff in target.Buffs.Where(buff => hasBuff(target, "fizzmarinerdoombomb"))) {
                //Game.PrintChat("Rek that kid: " + target.ChampionName);
                Utility.DrawCircle(target.Position,130,System.Drawing.Color.Coral,5,30,false);
            }
        }

        private void onSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            //TODO check jump states and set
            if (sender.IsMe)
                if (args.SData.Name == "FizzJump") {
                    fizzJump.jumpStage = JumpStage.PLAYFUL; // idek this is pissing me off and half asleep
                }
        }

        private void goFishyGo() {
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            PredictionOutput prediction = R.GetPrediction(target, true);

            if (target != null && target.IsValidTarget(R.Range)) {
                if (R.IsReady() && !isUnderEnemyTurret(target)) {
                    if (prediction.Hitchance >= HitChance.High && target.IsValidTarget()) {
                        R.Cast(target, true);
                    }
                }
            }
            if (player.Distance(target) > Q.Range)
            {
                //TODO E cast
            }
            W.Cast();
            Q.Cast(target,true);
        }

        private float getDamage(Obj_AI_Hero target) {
            var damages = new[] {SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R};
            return (float) player.GetComboDamage(target, damages);
        }

        private class FizzJump {
            public float jumpTime { get; set; }
            public bool called { get; set; }
            public JumpStage jumpStage { get; set; }
        }

        private enum JumpStage {
            PLAYFUL,
            TRICKSTER
        }
    }
}