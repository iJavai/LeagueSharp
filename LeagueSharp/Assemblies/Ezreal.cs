/*
 *
 *  ijava needs laneclear options, and harras over farm or farm over harras
    ijava cause its harras over farm all the time  
 * 
 * ijava its laneclear and its autoattack harrasing all the time not last hitting when its possible to harras, i have no other scripts injected //TODO dz191 check it out man :/
 * 
 * 
 * 
 * ijava and the logic is a bit fucked should check if its possible to kill or not for example casting ultimate while leaving the hp for one more skillshot (q) not having the mana
nedo
ijava but skillshots are kinda not missed 
 * 
 * ijava in current stage marksman is a bit better, while i think marksman is not using packets 
 * 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    internal class Ezreal : Champion {
        private HitChance customHitchance = HitChance.High;

        public Ezreal() {
            if (player.ChampionName != "Ezreal") {
                return;
            }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;
            Game.PrintChat("[Assemblies] - Ezreal Loaded." + "Happys a fag.");
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 1200);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 1050);
            W.SetSkillshot(0.25f, 80f, 2000f, false, SkillshotType.SkillshotLine);

            //DONT do e, its too situational.

            R = new Spell(SpellSlot.R, 3000);
            R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(false));

            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQLC", "Use Q in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("AutoQLC", "Auto Q to farm").SetValue(false));

            menu.AddSubMenu(new Menu("Killsteal Options", "killsteal"));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useQK", "Use Q for killsteal").SetValue(true));

            menu.AddSubMenu(new Menu("Hitchance Options", "hitchance"));
            menu.SubMenu("hitchance")
                .AddItem(
                    new MenuItem("hitchanceSetting", "Hitchance").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));

            menu.AddSubMenu(new Menu("Drawing Options", "drawing"));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw R").SetValue(false));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use packet Casting").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("useRAOE", "Use R on >= enemies").SetValue(false));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("rAmount", "Use R if enemeies > amount").SetValue(new Slider(3, 1, 5)));
            menu.SubMenu("misc").AddItem(new MenuItem("useNE", "No R if Closer than range").SetValue(false));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("NERange", "No R Range").SetValue(new Slider(450, 450, 1400)));

            Game.PrintChat("Ezreal by iJava & DZ191 Loaded. Special thanks to princer007");
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;

            if (menu.Item("useQK").GetValue<bool>()) {
                if (Q.IsKillable(SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical)))
                    castQ();
            }
            Farm();
            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (menu.Item("useQC").GetValue<bool>())
                        castQ();
                    if (menu.Item("useWC").GetValue<bool>())
                        castW();
                    if (menu.Item("useRAOE").GetValue<bool>() &&
                        Utility.CountEnemysInRange(600, player) >= menu.Item("rAmount").GetValue<Slider>().Value) {
                        AOEUltimate();
                    }
                    if (menu.Item("useRC").GetValue<bool>()) {
                        Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                        if (CustomRCalculation(target)) {
                            //TODO i think i did this right, if not then I'm a retard and feel free to test and change.
                            PredictionOutput prediction = R.GetPrediction(target, true);
                            if (target.IsValidTarget(R.Range) && R.IsReady() && prediction.Hitchance >= getHitchance()) {
                                R.Cast(target, getPackets(), true);
                            }
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (menu.Item("useQH").GetValue<bool>())
                        castQ();
                    if (menu.Item("useWH").GetValue<bool>())
                        castW();
                    break;
            }
        }

        private void Farm() {
            List<Obj_AI_Base> minionforQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            var useQ = menu.Item("useQLC").GetValue<bool>();
            var useAutoQ = menu.Item("AutoQLC").GetValue<bool>();
            MinionManager.FarmLocation qPosition = Q.GetLineFarmLocation(minionforQ);
            if (useQ && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Q.IsReady() &&
                qPosition.MinionsHit >= 1) {
                Q.Cast(qPosition.Position, getPackets());
            }
            if (useAutoQ && Q.IsReady() && qPosition.MinionsHit >= 1) {
                Q.Cast(qPosition.Position, getPackets());
            }
        }

        private HitChance getHitchance() {
            switch (menu.Item("hitchanceSetting").GetValue<StringList>().SelectedIndex) {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        private bool getPackets() {
            return menu.Item("usePackets").GetValue<bool>();
        }

        private void onDraw(EventArgs args) {
            //TODO draw pls DZ191 HURRY UP DZ191 I DOONT LIKE DOING THE BORING PARTS D: //DONE // fixed  iJava
            if (menu.Item("drawQ").GetValue<bool>()) {
                Drawing.DrawCircle(player.Position, Q.Range, Color.Purple);
            }
            if (menu.Item("drawW").GetValue<bool>()) {
                Drawing.DrawCircle(player.Position, W.Range, Color.Purple);
            }
            if (menu.Item("drawR").GetValue<bool>()) {
                Drawing.DrawCircle(player.Position, R.Range, Color.Purple);
            }
        }

        private void AOEUltimate() {
            // needs testing - iJava // not tested.
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
            if (target != null && target.Distance(player) >= 600)
                R.CastIfWillHit(target, menu.Item("rAmount").GetValue<Slider>().Value, true);
            // TODO set a value for 450 min range or >= maxRange..
        }

        private void castQ() {
            // needs testing - iJava //DONE working
            Obj_AI_Hero qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (!Q.IsReady() || qTarget == null || player.Distance(qTarget) > Q.Range - 10) return;

            if (qTarget.IsValidTarget(Q.Range) && qTarget.IsVisible && !qTarget.IsDead &&
                Q.GetPrediction(qTarget).Hitchance >= getHitchance()) {
                // TODO choose hitchance with slider more user customizability.
                Q.Cast(qTarget, getPackets());
            }
        }

        private void castW() {
            // needs testing - iJava //DONE working
            Obj_AI_Hero wTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (!W.IsReady() || wTarget == null) return;
            if (wTarget.IsValidTarget(W.Range) || W.GetPrediction(wTarget).Hitchance >= getHitchance()) {
                // TODO choose hitchance with slider more user customizability.
                W.Cast(wTarget, getPackets());
            }
        }

        //This should take into account minion and champs on the path
        //Not sure if this is working.
        private bool CustomRCalculation(Obj_AI_Hero target) {
            //So I got this weird idea. -DZ191
            float distance = player.Distance(target);
            //Vector3 RVector = player.Position - target.Position;
            List<Obj_AI_Base> minionListR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, R.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            double coeff = 1;
            int numberOfMinions = minionListR.Count(minion => R.WillHit(minion.Position, player.Position));
            int numberOfChamps =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsEnemy && hero.IsValid && !hero.IsDead && player.Distance(hero) <= R.Range)
                    .Count(hero => R.WillHit(hero.Position, player.Position));
            int total = numberOfChamps + numberOfMinions;
            if ((total - 1) >= 7) {
                coeff = 0.3;
            }
            else if (total > 1) {
                coeff = 1 - ((total - 1)/10);
            }
            //2000 being the EZ R projectile speed.
            //Factoring in The Regen. Thanks AcidRain.
            //princer007 Is a demigod <3 
            //Thanks princer007 - iJava appreciated your help :)
            if (R.GetDamage(target)*coeff >= (target.Health + (distance/2000)*target.HPRegenRate)) {
                return true;
            }
            return false;
        }
    }
}