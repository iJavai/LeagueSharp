using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    internal class Ezreal : Champion {
        public Ezreal() {
            if (player.ChampionName != "Ezreal") { return; }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;
            Orbwalking.AfterAttack += onAfterAttack;
            Game.PrintChat("[Assemblies] - Ezreal Loaded.");
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
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead ) return;

            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (menu.Item("useWC").GetValue<bool>())
                        castLineSkillShot(W, SimpleTs.DamageType.Magical);
                    if (menu.Item("useRAOE").GetValue<bool>())
                        AOEUltimate();
                    break;
            }
        }

        private void onAfterAttack(Obj_AI_Base unit, Obj_AI_Base target) {
            //TODO cast Q after attack?
            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (menu.Item("useQC").GetValue<bool>())
                        castLineSkillShot(Q);
                    if (menu.Item("useWC").GetValue<bool>())
                        castLineSkillShot(W);
                    if (menu.Item("useRC").GetValue<bool>())
                    {
                        var targetPhis = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
                        var targetMagic = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                        //Not finished,gotta check if R damage on target >= target.health
                        if ((R.GetPrediction(targetPhis).Hitchance < HitChance.High ) && (targetPhis != null && targetPhis.IsValid))
                        {
                            R.Cast(targetPhis, true);
                        }
                    }
                       
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    break;
            }
        }

        private void onDraw(EventArgs args) {
            //TODO draw pls DZ191
        }

        private bool AOEUltimate() {
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
            return target != null && target.Distance(player) >= 450 &&
                   R.CastIfWillHit(target, menu.Item("rAmount").GetValue<Slider>().Value, true);
            // TODO set a value for 450 min range or >= maxRange..
        }
    }
}