//TODO idea, use EvadeSpellDatabase or .dll to have an option to use ultimate to dodge dangeruous spells like Grag ult when evade can't dodge, so it doesn't waste ur R ? 
//TODO - reply here.
//TODO - when hes played more we will finish this tbh, i doubt he can carry solo q anyway too team orientated..

/* 
 * In combo it should Cast R then Items (Bork/Hydra/etc) after that everything is variable. 
 * If the enemy dashes/blinks away use W-E-Double Q. If not Zed should try to save his W shadow 
 * in case the enemy is saving his Escape for your double Q. If the enemy doesnt try to get away 
 * at all Zed should just either save his W or throw it in last second to get the double Q for his Death Mark Proc.
 * Also dodging important spells with Death Mark and Shadow Swaps should be an option confirguable spell by spell 
 * and integrated into Evade. With Shadow Swaps it should check if a specific number of enemys is around before switching 
 * and also check how far away/how close the shadow is from your target (assuming you are holding combo key down) and a check 
 * if the spell would kill you if you dont dodge it etc etc I could continue talking about such features for, well, forever.
 */

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;

namespace Assemblies {
    internal class Zed : Champion {
        private Obj_AI_Minion wShadow,rShadow;

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
            //TODO Can this detect R ?
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
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use E in combo").SetValue(true));
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
            Game.PrintChat("Zed by iJava,DZ191 and DETUKS Loaded.");
        }

        private void onUpdate(EventArgs args) {
            wShadow = findShadows(RWEnum.W);

            switch (LXOrbwalker.CurrentMode) {
                case LXOrbwalker.Mode.Combo:
                    //Shouldn't +W.Range be added ?
                    Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range*2, SimpleTs.DamageType.Physical);
                    if (Q.IsReady() && target.Distance(wShadow) < Q.Range) {
                        Q.UpdateSourcePosition(wShadow.Position, wShadow.Position);
                        Q.Cast(SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical), true);
                    }
                    //Console.WriteLine(findShadow("W").Position);
                    break;
            }
        }

        private Obj_AI_Minion findShadows(RWEnum RW) {
            switch (RW) {
                case RWEnum.W:
                    wShadow =
                        ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(obj => obj.Name == "Shadow" && obj.IsAlly);
                    if (wShadow != null)
                        return wShadow;
                    break;
                case RWEnum.R:
                    //TODO detuks is gonna handle shadows i think :^)
                    //TODO Done by DZ191. Think this should work.
                    var WShadow = findShadows(RWEnum.W);
                    rShadow =
                        ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(obj => obj.Name == "Shadow" && obj.IsAlly && obj!=WShadow);
                    if (rShadow != null)
                        return rShadow;
                    break;
            }
            return null;
        }

        private void onCreateObject(GameObject sender, EventArgs args)
        {
            //TODO Dunno,alternative method taht is called when shadows are created.
            var spell = (Obj_SpellMissile)sender;
            var unit = spell.SpellCaster;
            var name = spell.SData.Name;
            if (unit.IsMe)
            {
                switch (name)
                {
                    case "ZedShadowDashMissile":

                        break;
                    case "ZedUltMissile":

                        break;
                    default:
                        break;
                }
            }
        }

        private enum RWEnum {
            R,
            W
        };


        private struct ZedShadow {
            private Vector3 shadowPosition;
            private Obj_AI_Base sender;

        }

    }
}