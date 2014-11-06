using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;

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
            Game.PrintChat("[Assemblies] - Ezreal Loaded.");
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 1200);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 1050);
            W.SetSkillshot(0.25f, 80f, 2000f, false, SkillshotType.SkillshotLine);

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
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQLCH", "Harass while laneclearing").SetValue(false));


            menu.AddSubMenu(new Menu("Lasthit Options", "lastHit"));
            menu.SubMenu("lastHit").AddItem(new MenuItem("lastHitq", "Use Q Last Hit").SetValue(false));
            menu.SubMenu("lastHit").AddItem(new MenuItem("autoLastHit", "Auto Last Hit Q").SetValue(false));

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
            laneClear();
            lastHit();
            switch (LXOrbwalker.CurrentMode) {
                case LXOrbwalker.Mode.Combo:
                    if (menu.Item("useQC").GetValue<bool>())
                        castQ();
                    if (menu.Item("useWC").GetValue<bool>())
                        castW();
                    if (menu.Item("useRC").GetValue<bool>()) {
                        Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                        if (getUnitsInPath(player, target, R)) {
                            PredictionOutput prediction = R.GetPrediction(target, true);
                            if (target.IsValidTarget(R.Range) && R.IsReady() && prediction.Hitchance >= HitChance.High) {
                                sendSimplePing(target.Position);
                                R.Cast(target, getPackets(), true);
                            }
                        }
                    }
                    break;
                case LXOrbwalker.Mode.Harass:
                    if (menu.Item("useQH").GetValue<bool>())
                        castQ();
                    if (menu.Item("useWH").GetValue<bool>())
                        castW();
                    break;
            }
        }

        private void lastHit() {
            //TODO - get minions around you
            //Check if minion is killable with Q && isInRange
            //Also check if orbwalking mode == lasthit
            var autoQ = menu.Item("autoLastHit").GetValue<bool>();
            var lastHitNormal = menu.Item("lastHitq").GetValue<bool>();

            foreach (
                Obj_AI_Base minion in
                    MinionManager.GetMinions(player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly)) {
                if (autoQ && Q.IsReady() && Q.IsKillable(minion))
                    Q.Cast(minion.Position, getPackets());
                if (lastHitNormal && Q.IsReady() && LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Lasthit &&
                    Q.IsKillable(minion))
                    Q.Cast(minion.Position, getPackets());
            }
        }

        private void laneClear() {
            List<Obj_AI_Base> minionforQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            var useQ = menu.Item("useQLC").GetValue<bool>();
            var useAutoQ = menu.Item("AutoQLC").GetValue<bool>();
            MinionManager.FarmLocation qPosition = Q.GetLineFarmLocation(minionforQ);
            if (useQ && LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear && Q.IsReady() &&
                qPosition.MinionsHit >= 1) {
                Q.Cast(qPosition.Position, getPackets());
            }
            if (useAutoQ && Q.IsReady() && qPosition.MinionsHit >= 1) {
                Q.Cast(qPosition.Position, getPackets());
            }
            if (menu.Item("useQLCH").GetValue<bool>() && LXOrbwalker.CurrentMode == LXOrbwalker.Mode.LaneClear) {
                castQ();
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
            if (menu.Item("drawQ").GetValue<bool>()) {
                Utility.DrawCircle(player.Position, Q.Range, Color.Cyan);
            }
            if (menu.Item("drawW").GetValue<bool>()) {
                Utility.DrawCircle(player.Position, W.Range, Color.Crimson);
            }
            if (menu.Item("drawR").GetValue<bool>()) {
                Utility.DrawCircle(player.Position, R.Range, Color.Purple, 5, 30, true);
            }
        }

        private void castQ() {
            Obj_AI_Hero qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            if (!Q.IsReady() || qTarget == null || player.Distance(qTarget) > 1200) return;

            if (qTarget.IsValidTarget(Q.Range) && qTarget.IsVisible && !qTarget.IsDead &&
                Q.GetPrediction(qTarget).Hitchance >= getHitchance()) {
                Q.Cast(qTarget, getPackets());
            }
        }

        private void castW() {
            Obj_AI_Hero wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Physical);
            if (!W.IsReady() || wTarget == null) return;
            if (wTarget.IsValidTarget(W.Range) || W.GetPrediction(wTarget).Hitchance >= getHitchance()) {
                W.Cast(wTarget, getPackets());
            }
        }
    }
}