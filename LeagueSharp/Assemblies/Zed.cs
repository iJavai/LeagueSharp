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
using System.Drawing.Text;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;

namespace Assemblies {
    internal class Zed : Champion {
        private bool isKillable;
        private Obj_AI_Minion rShadow;
        private bool rShadowCreated;
        private bool rShadowFound;
        private Vector3 rShadowPosition;
        private int rShadowTick;
        private Obj_AI_Minion wShadow;

        private bool wShadowCreated;
        private bool wShadowFound;
        private int wShadowTick;
        private HitChance customHitChance = HitChance.High;

        public Zed() {
            if (player.ChampionName != "Zed") {
                return;
            }
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

            menu.AddSubMenu(new Menu("Use ultimate on", "ultOn"));
            HeroMenuCreate();

            menu.AddSubMenu(new Menu("Misc Options", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("SwapHPToggle", "Swap R at % HP").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("SwapHP", "%HP").SetValue(new Slider(5, 1)));
            menu.SubMenu("misc").AddItem(new MenuItem("SwapRKill", "Swap R when target dead").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("SafeRBack", "Safe swap calculation").SetValue(true));
            Game.PrintChat("Zed by iJava,DZ191 and DETUKS Loaded.");
        }

        private void onUpdate(EventArgs args) {
            if (wShadowCreated && !wShadowFound)
                findShadow(RWEnum.W);
            if (rShadowCreated && !rShadowFound)
                findShadow(RWEnum.R);

            if (wShadow != null && (wShadowTick < Environment.TickCount - 4000)) {
                wShadow = null;
                wShadowCreated = false;
                wShadowFound = false;
            }
            if (rShadow != null && (rShadowTick < Environment.TickCount - 6000)) {
                rShadow = null;
                rShadowCreated = false;
                rShadowFound = false;
                isKillable = false;
            }

            switch (LXOrbwalker.CurrentMode) {
                case LXOrbwalker.Mode.Combo:
                    deathMarkCombo();
                    //Console.WriteLine(findShadow("W").Position);
                    break;
            }
        }

        private void HeroMenuCreate()
        {
            foreach (var Enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                menu.SubMenu("ultOn").AddItem(new MenuItem("use" + Enemy.ChampionName, Enemy.ChampionName));
            }
        }
        private void deathMarkCombo() {
            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            Vector3 wPositionToCast = target.Position + Vector3.Normalize(target.Position - player.Position)*150;
            //WRange - 100 so its close to target
            if (R.IsReady() && isMenuEnabled(menu, "use" + target.ChampionName))
            {
                //TODO death mark combo mate.
                if (player.Distance(target) < R.Range) { //Maybe add an option to gapclose with W to target
                    R.Cast(target, true);
                    findShadow(RWEnum.R);
                    //TODO use items here.
                    if (W.IsReady() && wShadow == null)
                    {
                        W.Cast(wPositionToCast, true);
                        findShadow(RWEnum.W);
                    }
                    if (wShadow != null && rShadow != null) {
                        if (target.Distance(wShadow) < Q.Range || target.Distance(rShadow) < Q.Range)
                        {
                            var CustomQPredictionW = Prediction.GetPrediction(new PredictionInput
                            {
                                Unit = target,
                                Delay = Q.Delay,
                                Radius = Q.Width,
                                From = wShadow.Position,
                                Range = Q.Range,
                                Collision = false,
                                Type = Q.Type,
                                RangeCheckFrom = player.ServerPosition,
                                Aoe = false
                            });
                            var CustomQPredictionR = Prediction.GetPrediction(new PredictionInput
                            {
                                Unit = target,
                                Delay = Q.Delay,
                                Radius = Q.Width,
                                From = rShadow.Position,
                                Range = Q.Range,
                                Collision = false,
                                Type = Q.Type,
                                RangeCheckFrom = player.ServerPosition,
                                Aoe = false
                            });
                            if (CustomQPredictionR.Hitchance >= customHitChance)//Q From R
                                Q.Cast(CustomQPredictionR.CastPosition, true);
                            if (CustomQPredictionW.Hitchance >= customHitChance)//Q from W
                                Q.Cast(CustomQPredictionW.CastPosition, true);
                            if (Q.GetPrediction(target).Hitchance >= customHitChance) //Normal Q
                                Q.Cast(Q.GetPrediction(target).CastPosition, true);
                        }         
                        if (target.Distance(wShadow) <= E.Range || target.Distance(rShadow) <= E.Range ||
                            target.Distance(player) <= E.Range)
                            E.CastOnUnit(player, true); // THIS WONT CAST IDK why js
                        foreach (
                            Obj_AI_Hero enemy in
                                ObjectManager.Get<Obj_AI_Hero>().Where(
                                    hero => hero.IsValidTarget() && hasBuff(hero, "zedulttargetmark"))) {
                            LXOrbwalker.ForcedTarget = enemy;
                        }
                        /*if (canGoToShadow(RWEnum.R)) {
                            R.Cast(player, true);
                        }*/
                    }
                }
            }
        }

        private bool canGoToShadow(RWEnum RW) {
            switch (RW) {
                case RWEnum.W:
                    if (player.Spellbook.GetSpell(SpellSlot.W).Name == "zedw2")
                        return true;
                    break;
                case RWEnum.R:
                    if (player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2")
                        return true;
                    break;
            }
            return false;
        }

        private bool isMarkKillable() {
            return isKillable;
        }

        private void findShadow(RWEnum shadowName) {
            Obj_AI_Minion shadow;
            if (shadowName == RWEnum.W) {
                shadow =
                    ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(
                        hero => (hero.Name == "Shadow" && hero.IsAlly && (hero != rShadow)));
                if (shadow != null) {
                    wShadow = shadow;
                    wShadowFound = true;
                    wShadowTick = Environment.TickCount;
                }
            }
            if (shadowName != RWEnum.R)
                return;
            shadow =
                ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(
                    hero =>
                        ((hero.ServerPosition.Distance(rShadowPosition)) < 50) && hero.Name == "Shadow" && hero.IsAlly &&
                        hero != wShadow);
            if (shadow == null)
                return;
            rShadow = shadow;
            rShadowFound = true;
            rShadowTick = Environment.TickCount;
        }

        /* private Obj_AI_Minion findShadows(RWEnum RW) {
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
                    Obj_AI_Minion WShadow = findShadows(RWEnum.W);
                    rShadow =
                        ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(
                            obj => obj.Name == "Shadow" && obj.IsAlly && obj != WShadow);
                    if (rShadow != null)
                        return rShadow;
                    break;
            }
            return null;
        }*/

        private void onCreateObject(GameObject sender, EventArgs args) {
            //TODO Dunno,alternative method taht is called when shadows are created.
            var spell = (Obj_SpellMissile) sender;
            Obj_AI_Base unit = spell.SpellCaster;
            string name = spell.SData.Name;
            if (!unit.IsMe) return;
            switch (name) {
                case "ZedShadowDashMissile":
                    wShadowCreated = true;
                    break;
                case "ZedUltMissile":
                    rShadowCreated = true;
                    rShadowPosition = player.Position;
                    break;
            }
            if (sender.Name.Contains("Zed_Base_R_buf_tell.troy"))
                Game.PrintChat("Killable mate get outta here <3");
        }
    }

    internal enum RWEnum {
        R,
        W
    };

    internal struct ZedShadow {
        private Obj_AI_Base sender;
        private Vector3 shadowPosition;
    }
}