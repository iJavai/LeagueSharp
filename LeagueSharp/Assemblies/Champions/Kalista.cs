using System;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;

namespace Assemblies.Champions {
    internal class Kalista : Champion {
        public Kalista() {
            if (player.ChampionName != "Kalista") {
                return;
            }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;
            Game.PrintChat("[Assemblies] - Kalista Loaded.");

            var wc = new WebClient { Proxy = null };

            wc.DownloadString("http://league.square7.ch/put.php?name=iKalista");
            string amount = wc.DownloadString("http://league.square7.ch/get.php?name=iKalista");
            Game.PrintChat("[Assemblies] - iKalista has been loaded " + Convert.ToInt32(amount) + " times by LeagueSharp Users.");
        }

        private static int GetSpearCount {
            get {
                int xBuffCount = 0;
                foreach (
                    BuffInstance buff in
                        from enemy in
                            ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy && enemy.IsValidTarget(975))
                        from buff in enemy.Buffs
                        where buff.Name.Contains("kalistaexpungemarker")
                        select buff) {
                    xBuffCount = buff.Count;
                }
                return xBuffCount;
            }
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 1450);
            W = new Spell(SpellSlot.W, 5500);
            E = new Spell(SpellSlot.E, 975);
            R = new Spell(SpellSlot.R, 1350);

            Q.SetSkillshot(0.12f, 40, 1800, true, SkillshotType.SkillshotLine);
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use E in combo").SetValue(true));
            //menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useEH", "Use E in harass").SetValue(true));

            menu.AddSubMenu(new Menu("Laneclear Options", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQLC", "Use Q in laneclear").SetValue(true));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("eStacks", "Cast E on stacks").SetValue(new Slider(2, 1, 10)));
            menu.SubMenu("misc").AddItem(new MenuItem("eKill", "Use e to Kill enemies").SetValue(true));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;

            Obj_AI_Hero target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);

            if (isMenuEnabled(menu, "eKill"))
                killsteal(target);

            switch (LXOrbwalker.CurrentMode) {
                case LXOrbwalker.Mode.Combo:
                    if (isMenuEnabled(menu, "useQC"))
                        castQ(target);
                    if (isMenuEnabled(menu, "useEC")) {
                        if (GetSpearCount >= menu.Item("eStacks").GetValue<Slider>().Value &&
                            player.Distance(target) <= E.Range) {
                            E.Cast(true);
                        }
                    }
                    break;
                case LXOrbwalker.Mode.Harass:
                    if (isMenuEnabled(menu, "useQH"))
                        castQ(target);
                    if (isMenuEnabled(menu, "useEH")) {
                        if (GetSpearCount >= menu.Item("eStacks").GetValue<Slider>().Value &&
                            player.Distance(target) <= E.Range) {
                            E.Cast(true);
                        }
                    }
                    break;
                    
            }
        }

        private void onDraw(EventArgs args) {}

        private bool hasCollision() {
            return false;
        }


        private void castQ(Obj_AI_Hero target) {
            if (target.IsValidTarget(Q.Range) && player.Distance(target) <= Q.Range) {
                if (Q.IsReady() && Q.GetPrediction(target).Hitchance >= HitChance.Medium) {
                    Q.Cast(target, true);
                }
                else if (Q.GetPrediction(target).Hitchance == HitChance.Collision) {
                    var collisionObjects = Q.GetPrediction(target).CollisionObjects;
                    foreach (Obj_AI_Base collision in collisionObjects.Where(collision => collision.IsMinion)) {
                        if (Q.IsReady() && Q.IsKillable(collision))
                            Q.Cast(collision.Position, true);
                    }
                }
            }
        }

        private void killsteal(Obj_AI_Hero target) {
            if (target.IsValidTarget(E.Range) && E.IsReady() &&
                player.GetSpellDamage(target, SpellSlot.E) - 10 > target.Health) {
                E.Cast(true);
            }
        }

    }
}