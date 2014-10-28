//TODO idea, use EvadeSpellDatabase or .dll to have an option to use ultimate to dodge dangeruous spells like Grag ult when evade can't dodge, so it doesn't waste ur R ? 
//TODO - reply here.
//TODO - when hes played more we will finish this tbh, i doubt he can carry solo q anyway too team orientated..

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;

namespace Assemblies {
    //Kappa

    internal class Zed : Champion {
        private Obj_AI_Minion wShadow;

        public Zed() {
            if (player.ChampionName != "Zed") {
                return;
            }
            //targetPont = player.Position;
            loadMenu();
            loadSpells();
            Game.OnGameUpdate += onUpdate;
            GameObject.OnCreate += onCreateObject;
            Obj_AI_Base.OnProcessSpellCast += onSpellCast;
            Game.PrintChat("[Assemblies] - Zed Loaded.");
        }

        private void onSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            if (sender.IsMe && args.SData.Name == "ZedShadowDash") {
                Game.PrintChat("WUSED NIGGA");
            }
            if (sender.IsMe)
                Game.PrintChat("spell casted: " + args.SData.Name); // zedw2
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 900);
            Q.SetSkillshot(0.235f, 50f, 1700, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 550);

            E = new Spell(SpellSlot.E, 290);

            R = new Spell(SpellSlot.R, 600);
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWF", "Use W to follow").SetValue(true));


            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "Use W in harass").SetValue(false));
            menu.SubMenu("harass").AddItem(new MenuItem("useEH", "Use E in harass").SetValue(false));

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("SwapHPToggle", "Swap R at % HP").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("SwapHP", "%HP").SetValue(new Slider(5, 1)));
            menu.SubMenu("misc").AddItem(new MenuItem("SwapRKill", "Swap R when target dead").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("SafeRBack", "Safe swap calculation").SetValue(true));
            Game.PrintChat("Zed by iJava and DZ191 Loaded.");
        }

        private void onUpdate(EventArgs args) {
            wShadow = findShadow("W");

            switch (LXOrbwalker.CurrentMode) {
                case LXOrbwalker.Mode.Combo:
                    Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range*2, SimpleTs.DamageType.Physical);
                    if (Q.IsReady() && target.Distance(wShadow) < Q.Range) {
                        Q.UpdateSourcePosition(wShadow.Position, wShadow.Position);
                        Q.Cast(SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical), true);
                    }
                    //Console.WriteLine(findShadow("W").Position);
                    break;
            }
        }

        private Obj_AI_Minion findShadow(string shadow) {
            if (shadow == "W") {
                Obj_AI_Minion wShadow =
                    ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(obj => obj.Name == "Shadow" && obj.IsAlly);
                if (wShadow != null) {
                    return wShadow;
                }
            }
            if (shadow == "R") {
                Obj_AI_Minion rShadow =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .FirstOrDefault(
                            obj =>
                                obj.Name == "Shadow" && player.Distance(obj) < 50 && obj.IsAlly &&
                                player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2");
                if (rShadow != null) {
                    return rShadow;
                }
            }
            return null;
        }

        private Obj_AI_Minion findShadows(RWEnum RW) {
            switch (RW) {
                case RWEnum.W:
                    break;
            }
            return null;
        }

        private void onCreateObject(GameObject sender, EventArgs args) {}

        private enum RWEnum {
            R,
            W
        };
    }
}