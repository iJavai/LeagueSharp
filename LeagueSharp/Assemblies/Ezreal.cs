using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

/*
 * Maxric
its pretty good, only things that might be useful are being able to have it auto q for harassing, i like that later game on other ezreals
8:15:09 PM
Maxric
also it looks to me like the damage calc for his ult is off some, it has ulted and failed to kill twice now
8:15:16 PM
Maxric
hit them, but they lived with about 80 hp
8:18:24 PM
Maxric
oh, i dont think i hit minions but maybe i did
8:20:41 PM
Maxric
yeah it actually works pretty well, not sure the first couple ults but now its fine
8:20:59 PM
Maxric
only thing i can think of is like auto q and maybe w if you go the mana route or have a blue you can turn it on for
8:28:31 PM
Maxric
oh ok, maybe add some logic for R not to execute if within x range?  */

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
            menu.AddSubMenu(new Menu("Combo", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R to execute").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("usePackets", "Use Packet Casting").SetValue(true));

            menu.AddSubMenu(new Menu("Harass", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(false));

            menu.AddSubMenu(new Menu("Drawing", "drawing"));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw Q").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw R").SetValue(false));

            menu.AddSubMenu(new Menu("Misc", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("useRAOE", "Use R on >= enemies").SetValue(false));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("rAmount", "Use R if enemeies > amount").SetValue(new Slider(3, 1, 5)));
            menu.SubMenu("misc").AddItem(new MenuItem("useNE", "No R if Closer than range").SetValue(false));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("NERange", "No R Range").SetValue(new Slider(450, 450, 1400)));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;

            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (menu.Item("useWC").GetValue<bool>())
                        castW();
                    if (menu.Item("useRAOE").GetValue<bool>() &&
                        Utility.CountEnemysInRange(600) >= menu.Item("rAmount").GetValue<Slider>().Value) {
                        AOEUltimate();
                    }
                    if (menu.Item("useRC").GetValue<bool>()) {
                        Obj_AI_Hero targetPhis = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
                        //Obj_AI_Hero targetMagic = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical); //Redundant atm TODO
                        //Not finished,gotta check if R damage on target >= target.health //DONE TODO
                        // Done the Not Execute in certain range
                        if ((R.GetPrediction(targetPhis).Hitchance == HitChance.High) &&
                            (targetPhis != null && targetPhis.IsValidTarget(2000))) {
                            if (R.IsKillable(targetPhis)) // Thats extremly handy
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

        private void onAfterAttack(Obj_AI_Base unit, Obj_AI_Base target) {
            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (menu.Item("useQC").GetValue<bool>())
                        castQ();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
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

        private void AOEUltimate() { // needs testing - iJava
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
            if (target != null && target.Distance(player) >= 600)
                R.CastIfWillHit(target, menu.Item("rAmount").GetValue<Slider>().Value, true);
            // TODO set a value for 450 min range or >= maxRange..
        }

        private void castQ() { // needs testing - iJava
            Obj_AI_Hero qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (!Q.IsReady() || qTarget == null) return;

            if (qTarget.IsValidTarget(Q.Range) || Q.GetPrediction(qTarget).Hitchance >= HitChance.High) {
                // TODO choose hitchance with slider more user customizability.
                Q.Cast(qTarget, getPackets());
            }
        }

        private void castW() { // needs testing - iJava
            Obj_AI_Hero wTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (!W.IsReady() || wTarget == null) return;

            if (wTarget.IsValidTarget(W.Range) || W.GetPrediction(wTarget).Hitchance >= HitChance.High) {
                // TODO choose hitchance with slider more user customizability.
                W.Cast(wTarget, getPackets());
            }
        }
    }
}