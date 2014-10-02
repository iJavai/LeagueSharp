using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

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

namespace Assemblies {
    internal class Ezreal : Champion {
        public Ezreal() {
            if (player.ChampionName != "Ezreal") {
                return;
            }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;
            Orbwalking.AfterAttack += onAfterAttack;
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
            //TODO either do items in here or do it in champion class for future scripts maybe something like an inbuilt activator
            //TODO also autoQ while laneclearing has been requested with option for on  / off also check ultimate apparently its leave players on low hp... idek why i did an R.isKillable check.. :/

            //DONE tried to change your check. Idk if it is working. Test please :P DZ191
            //Also added Q in laneclear. Untested either.

            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R to execute").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("usePackets", "Use Packet Casting").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(false));
            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQLC", "Use Q in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("AutoQLC", "Auto Q to farm").SetValue(false));

            menu.AddSubMenu(new Menu("Killsteal Options", "killsteal"));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useQK", "Use Q for killsteal").SetValue(true));

            menu.AddSubMenu(new Menu("Drawing Options", "drawing"));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw R").SetValue(false));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("useRAOE", "Use R on >= enemies").SetValue(false));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("rAmount", "Use R if enemeies > amount").SetValue(new Slider(3, 1, 5)));
            menu.SubMenu("misc").AddItem(new MenuItem("useNE", "No R if Closer than range").SetValue(false));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("NERange", "No R Range").SetValue(new Slider(450, 450, 1400)));
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
                        Obj_AI_Hero targetPhis = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
                        //Obj_AI_Hero targetMagic = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical); //Redundant atm TODO
                        //Not finished,gotta check if R damage on target >= target.health //DONE TODO
                        // Done the Not Execute in certain range
                        if ((R.GetPrediction(targetPhis).Hitchance == HitChance.High) &&
                            (targetPhis != null && targetPhis.IsValidTarget(2000))) {
                            if (R.GetHealthPrediction(targetPhis)<=0) // Thats extremly handy // Tried to change the method
                                if (menu.Item("useNE").GetValue<bool>()) {
                                    if (player.Distance(targetPhis) >= menu.Item("NERange").GetValue<Slider>().Value)
                                        R.Cast(targetPhis, true);
                                }
                                else {
                                    R.Cast(targetPhis, true);
                                }
                        }
                    }
                    break;
            }
        }
        private void Farm()
        {
            var minionforQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var useQ = menu.Item("useQLC").GetValue<bool>();
            var useAutoQ = menu.Item("AutoQLC").GetValue<bool>();
            var QPosition = Q.GetLineFarmLocation(minionforQ);
            if(useQ && orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Q.IsReady() && QPosition.MinionsHit>=1)
            {
                Q.Cast(QPosition.Position, getPackets());
            }
            if(useAutoQ && Q.IsReady() && QPosition.MinionsHit>=1)
            {
                Q.Cast(QPosition.Position, getPackets());
            }
        }
        private void onAfterAttack(Obj_AI_Base unit, Obj_AI_Base target) {
            switch (orbwalker.ActiveMode) {}
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
            // needs testing - iJava
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
            if (target != null && target.Distance(player) >= 600)
                R.CastIfWillHit(target, menu.Item("rAmount").GetValue<Slider>().Value, true);
            // TODO set a value for 450 min range or >= maxRange..
        }

        private void castQ() {
            // needs testing - iJava
            Obj_AI_Hero qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (!Q.IsReady() || qTarget == null) return;

            if (qTarget.IsValidTarget(Q.Range) && qTarget.IsVisible && !qTarget.IsDead && Q.GetPrediction(qTarget).Hitchance >= HitChance.High) {
                // TODO choose hitchance with slider more user customizability.
                Q.Cast(qTarget, getPackets());
            }
        }

        private void castW() {
            // needs testing - iJava
            Obj_AI_Hero wTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (!W.IsReady() || wTarget == null) return;

            if (wTarget.IsValidTarget(W.Range) || W.GetPrediction(wTarget).Hitchance >= HitChance.High) {
                // TODO choose hitchance with slider more user customizability.
                W.Cast(wTarget, getPackets());
            }
        }
    }
}