// fizzmarinerdoombomb = R, fizzmarinerdoomslow = slow buff for R 

/**
 * IDEA - Flee mode over walls should be easy - ish, Cast E on first vector point then cast E2 on second vector point on the other side of the wall? :3
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;
using Color = System.Drawing.Color;

namespace Assemblies {
    internal class Fizz : Champion {
        private Spell E2;
        private bool isCalled;
        private FizzJump jumpStage;
        private Dictionary<Vector3, Vector3> positions;
        private float time;

        public Fizz() {
            loadMenu();
            loadSpells();
            addFleeSpots();

            Game.OnGameUpdate += onUpdate;
            Obj_AI_Base.OnProcessSpellCast += onSpellCast;
            LXOrbwalker.BeforeAttack += onBeforeAttack;
            Drawing.OnDraw += onDraw;
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

            menu.AddSubMenu(new Menu("Steal Options", "steal"));
            menu.SubMenu("steal").AddItem(
                new MenuItem("stealKey", "Steal Boss").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

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
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            switch (LXOrbwalker.CurrentMode) {
                case LXOrbwalker.Mode.Combo:
                    goFishyGo(target);
                    break;
                case LXOrbwalker.Mode.Harass:
                    //TODO harass
                    harassMode(target);
                    break;
                case LXOrbwalker.Mode.Flee:
                    fleeMode();
                    qFlee();
                    break;
            }
            if (menu.Item("stealKey").GetValue<KeyBind>().Active) {
                dragonStealerino();
            }
        }

        private void goFishyGo(Obj_AI_Hero target) {
            //TODO rework pl0x

            if (target.IsValidTarget(R.Range)) {
                if (E.IsReady()) // TODO this combo is a pile of shit atm, just basic as fk
                    castEGapclose(target); // Gapcloses with E First if not withing QRange
                if (R.IsReady() && !isUnderEnemyTurret(target)) {
                    // then fires R ofc
                    if (R.GetPrediction(target, true).Hitchance >= HitChance.VeryHigh) {
                        R.Cast(target, true);
                    }
                }
                if (Q.IsReady())
                    Q.CastOnUnit(target); // then Q's
            }
            foreach (BuffInstance buff in target.Buffs.Where(buff => hasBuff(target, "fizzmarinerdoombomb"))) {
                Utility.DrawCircle(target.Position, R.Range, Color.Coral);
            }
        }

        private void dragonStealerino() {
            var originalPosition = new Vector2(8567, 4231);
            var stealPosition = new Vector2(8949, 4207);
            SpellSlot smite = player.GetSpellSlot("SummonerSmite");
            Obj_AI_Base minion =
                MinionManager.GetMinions(player.Position, 1500, MinionTypes.All, MinionTeam.NotAlly).FirstOrDefault(
                    i => i.Name == "Worm12.1.1" || i.Name == "Dragon6.1.1");

            if (E.IsReady() && player.Distance(originalPosition) > 10 && jumpStage == FizzJump.PLAYFUL) {
                sendMovementPacket(originalPosition);
            }

            if (E.IsReady() && player.Distance(originalPosition) < 10 && jumpStage == FizzJump.PLAYFUL) {
                E.Cast(stealPosition, true);
            }

            if (E2.IsReady() && player.Distance(stealPosition) < 10 && jumpStage == FizzJump.TRICKSTER &&
                player.SummonerSpellbook.CanUseSpell(smite) == SpellState.Cooldown) {
                E2.Cast(originalPosition, true);
            }

            if (minion != null && minion.Distance(player) <= 625) {
                if (smite != SpellSlot.Unknown && player.SummonerSpellbook.CanUseSpell(smite) == SpellState.Ready &&
                    player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite) >= minion.Health) {
                    player.SummonerSpellbook.CastSpell(smite, minion);
                }
            }
        }

        private void harassMode(Obj_AI_Hero objAiHero) {
            Obj_AI_Hero target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);

            if (target != null && target.IsValidTarget(E.Range)) {
                if (player.Distance(target) < Q.Range) {
                    Q.Cast(target);
                }
                else {
                    castEGapclose(target);
                }
            }
        }

        private void castEGapclose(Obj_AI_Hero target) {
            if (target.IsValidTarget(800)) {
                if (E.IsReady() && player.Distance(target) > Q.Range) {
                    if (jumpStage == FizzJump.PLAYFUL && player.Spellbook.GetSpell(SpellSlot.E).Name == "FizzJump") {
                        E.Cast(target.ServerPosition, true);
                    }
                }
                if (jumpStage == FizzJump.TRICKSTER && player.Spellbook.GetSpell(SpellSlot.E).Name == "fizzjumptwo") {
                    E2.Cast(target.ServerPosition, true);
                }
            }
        }

        private void onBeforeAttack(LXOrbwalker.BeforeAttackEventArgs args) {
            if (!args.Unit.IsMe) return;
            switch (LXOrbwalker.CurrentMode) {
                case LXOrbwalker.Mode.Combo:
                    if (W.IsReady() && args.Target.Distance(args.Unit) < W.Range && !args.Target.IsMinion &&
                        args.Target.IsValidTarget(W.Range))
                        W.Cast(args.Unit, true);
                    break;
                case LXOrbwalker.Mode.Harass:
                    if (W.IsReady() && args.Target.Distance(args.Unit) < W.Range && !args.Target.IsMinion &&
                        args.Target.IsValidTarget(W.Range))
                        W.Cast(args.Unit, true);
                    break;
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

        private void qFlee() {
            List<Obj_AI_Base> minions = MinionManager.GetMinions(player.Position, Q.Range, MinionTypes.All,
                MinionTeam.Enemy,
                MinionOrderTypes.None); // minions to loop through
            sendMovementPacket(Game.CursorPos.To2D());
            foreach (
                Obj_AI_Base minion in
                    minions.Where(
                        minion => minion.IsValidTarget(Q.Range) && minion.Distance(Game.CursorPos.To2D()) < Q.Range &&
                                  Q.InRange(minion.Position))) {
                Q.Cast(minion, true); // todo make sure this works i guess? idk
            }
        }

        private void fleeMode() {
            sendMovementPacket(Game.CursorPos.To2D());
            foreach (var entry in positions) {
                if (player.Distance(entry.Key) <= E.Range || player.Distance(entry.Value) <= E.Range) {
                    Vector3 closest = entry.Key;
                    Vector3 furthest = entry.Value;
                    if (player.Distance(entry.Key) < player.Distance(entry.Value)) {
                        closest = entry.Key;
                        furthest = entry.Value;
                    }
                    if (player.Distance(entry.Key) > player.Distance(entry.Value)) {
                        closest = entry.Value;
                        furthest = entry.Key;
                    }
                    sendMovementPacket(new Vector2(closest.X, closest.Y));
                    E.Cast(closest, true);
                    E2.Cast(furthest, true);
                }
            }
        }

        private void onDraw(EventArgs args) {
            foreach (
                var entry in
                    positions.Where(
                        entry => player.Distance(entry.Key) <= 1500f && player.Distance(entry.Value) <= 1500f)) {
                Drawing.DrawCircle(entry.Key, 75f, Color.Cyan);
                Drawing.DrawCircle(entry.Value, 75f, Color.Cyan);
            }
        }

        private void addFleeSpots() {
            //TODO flee spots
        }

        private enum FizzJump {
            PLAYFUL,
            TRICKSTER
        }
    }
}