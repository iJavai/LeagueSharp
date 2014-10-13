using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

//TODO idea, use EvadeSpellDatabase or .dll to have an option to use ultimate to dodge dangeruous spells like Grag ult when evade can't dodge, so it doesn't waste ur R ? 
//TODO - reply here.
//TODO - when hes played more we will finish this tbh, i doubt he can carry solo q anyway too team orientated..

namespace Assemblies {
    //Kappa
    internal class Zed : Champion {
        private static Vector3 PositionBeforeR = Vector3.Zero;
        private bool ROut;
        private ZedShadow RShadow;
        private bool WOut;
        private ZedShadow WShadow;
        private HitChance customHitchance = HitChance.High;
        private Obj_AI_Hero deathMarkTarget;

        private bool isChampKill; //but what if champ is not kill Kappa
        private List<ZedShadow> shadowList;

        public Zed() {
            if (player.ChampionName != "Zed") {
                return;
            }
            //targetPont = player.Position;
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;
            GameObject.OnCreate += onProcessSpell;
            GameObject.OnDelete += onDeleteObject;

            Game.PrintChat("[Assemblies] - Zed Loaded.");
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
            if (R.IsReady()) {
                DoTheRDance();
            } // this should work right TODO DZ191? 
            else {
                doNormalCombo();
            }
        }

        private void onDraw(EventArgs args) {
            throw new NotImplementedException();
        }

        private void doNormalCombo() {
            //TODO Q,W,E combo including shadows
        }

        private void fillShadowList() {} // todo wat? :S

        private Obj_AI_Hero getDeathmarkedTarget() {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(heroes => heroes.IsEnemy)
                    .FirstOrDefault(heroes => heroes.HasBuff("zedulttargetmark", true));
            // <-- is that the actual buff name or nah? It is
        }

        private bool isTargetKilled() {
            return isChampKill;
        }

        private bool canGoBackW() {
            return player.Spellbook.GetSpell(SpellSlot.W).Name == "zedw2";
        }

        private bool canGoBackR() {
            return player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2";
        }

        private void DoTheRDance() {
            //Added a very basic line combo.
            Obj_AI_Hero ComboTarget;
            if (getDeathmarkedTarget() == null) {
                PositionBeforeR = Vector3.Zero;
                var ts = new TargetSelector(R.Range, TargetSelector.TargetingMode.LessCast);
                ComboTarget = ts.Target;
                if (!ComboTarget.IsValidTarget()) return;
                PositionBeforeR = player.ServerPosition;
                R.Cast(ComboTarget);
            }
            else {
                getDeathmarkedTarget();
            }
            safetySwap();
            ComboTarget = getDeathmarkedTarget();
            orbwalker.ForceTarget(ComboTarget);
            Vector3 tgPos = ComboTarget.ServerPosition;
            Vector2 bestShadowPos = getBestShadowPos(PositionBeforeR, tgPos);
            if (bestShadowPos == Vector2.Zero) return;
            W.Cast(bestShadowPos);
            ZedShadow WSh = WShadow;
            ZedShadow RSh = RShadow;
            if (WSh == null || RShadow == null) {
                Game.PrintChat("Something went wrong");
                return;
            }
            PredictionOutput customQPredictionW = Prediction.GetPrediction(new PredictionInput {
                Unit = ComboTarget,
                Delay = Q.Delay,
                Radius = Q.Width,
                From = WSh.shadowPosition,
                Range = Q.Range,
                Collision = false,
                Type = Q.Type,
                RangeCheckFrom = player.ServerPosition,
                Aoe = false
            });
            PredictionOutput customQPredictionR = Prediction.GetPrediction(new PredictionInput {
                Unit = ComboTarget,
                Delay = Q.Delay,
                Radius = Q.Width,
                From = RSh.shadowPosition,
                Range = Q.Range,
                Collision = false,
                Type = Q.Type,
                RangeCheckFrom = player.ServerPosition,
                Aoe = false
            });
            bool isPlayerERangeW = getEnemiesInRange(WSh.shadowPosition, E.Range).Contains(ComboTarget);
            bool isPlayerERangeR = getEnemiesInRange(RSh.shadowPosition, E.Range).Contains(ComboTarget);
            if (customQPredictionW.Hitchance >= customHitchance || customQPredictionR.Hitchance >= customHitchance)
                Q.Cast(ComboTarget);
            if (isPlayerERangeR || isPlayerERangeW)
                E.Cast();
            if (isChampKill && canBackToShadow() && isMenuEnabled(menu, "SwapRKill")) {
                if (isMenuEnabled(menu, "SafeRBack") && safeBack(RSh))
                    R.Cast();
                else
                    R.Cast();
            }
        }

        private void safetySwap() {
            float currentHealthPercent = getPercentValue(player, false);
            if (currentHealthPercent <= menu.Item("SwapHP").GetValue<Slider>().Value) {
                if (safeBack(RShadow) && canBackToShadow()) {
                    R.Cast();
                }
            }
        }

        private Vector2 getBestShadowPos(Vector3 from, Vector3 targetPos) {
            Vector2 predictPos = V2E(from, targetPos - from, W.Range);
            if (IsWall(predictPos) || IsPassWall(targetPos, predictPos.To3D())) {
                return Vector2.Zero;
            }
            return predictPos;
        }

        private bool safeBack(ZedShadow shadow) {
            Vector3 shadowPos = shadow.shadowPosition;
            Vector3 playerPos = player.ServerPosition;
            int nearShadowPos = getEnemiesInRange(shadowPos, 500f).Count;
            int nearPlayerPos = getEnemiesInRange(playerPos, 500f).Count;
            if (nearPlayerPos > nearShadowPos)
                return true;
            return false;
        }

        //Credits to princer007
        private static bool IsPassWall(Vector3 start, Vector3 end) {
            double count = Vector3.Distance(start, end);
            for (uint i = 0; i <= count; i += 10) {
                Vector2 pos = V2E(start, end, i);
                if (IsWall(pos)) return true;
            }
            return false;
        }

        private static bool IsWall(Vector2 pos) {
            return (NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Wall ||
                    NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Building);
        }

        private static Vector2 V2E(Vector3 from, Vector3 direction, float distance) {
            return from.To2D() + distance*Vector3.Normalize(direction - from).To2D();
        }

        //End of credits
        private bool canBackToShadow() {
            return player.Spellbook.GetSpell(SpellSlot.W).Name == "zedw2" ||
                   player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2";
        }

        private void onDeleteObject(GameObject sender, EventArgs args) {
            GameObject theObject = sender;

            if (theObject.IsValid && theObject == WShadow.shadowObj) {
                WShadow = null;
                WOut = false;
            }
            if (theObject.IsValid && theObject == RShadow.shadowObj) {
                RShadow = null;
                ROut = false;
            }
            if (sender.Name.Contains("Zed_Base_R_buf_tell.troy")) {
                isChampKill = false;
                deathMarkTarget = null;
            }
        }

        private void onProcessSpell(GameObject sender, EventArgs args) {
            var theSpell = (Obj_SpellMissile) sender;

            if (sender.IsMe && theSpell.SData.Name == "ZedUltMissile") {
                RShadow = new ZedShadow {
                    shadowPosition = player.ServerPosition,
                    WR = RWEnum.R,
                    gameTick = Environment.TickCount,
                    sender = sender,
                    shadowObj = CheckForClones(RWEnum.R)
                };
                ROut = true;
            }
            if (sender.IsMe && theSpell.SData.Name == "ZedShadowDashMissile") {
                WShadow = new ZedShadow {
                    shadowPosition = CheckForClones(RWEnum.W).ServerPosition,
                    WR = RWEnum.W,
                    gameTick = Environment.TickCount,
                    sender = sender,
                    shadowObj = CheckForClones(RWEnum.W)
                };
                WOut = true;
            }
            if (sender.Name.Contains("Zed_Base_R_buf_tell.troy")) {
                isChampKill = true;
                deathMarkTarget = null;
            }
        }

        private Obj_AI_Minion CheckForClones(RWEnum RorW) {
            switch (RorW) {
                case RWEnum.W:
                    Obj_AI_Minion obj1 =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .FirstOrDefault(obj => obj.Name == "Shadow" && obj.IsAlly && obj != RShadow.shadowObj);
                    if (obj1 != null)
                        return obj1;
                    return null;
                case RWEnum.R:
                    Obj_AI_Minion obj2 =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .FirstOrDefault(
                                obj =>
                                    obj.Name == "Shadow" && player.Distance(obj) < 50 && obj.IsAlly &&
                                    obj != WShadow.shadowObj);
                    if (obj2 != null)
                        return obj2;
                    return null;
                default:
                    return null;
            }
        }

        private enum RWEnum {
            R,
            W
        };

        private class ZedShadow {
            public Vector3 shadowPosition { get; set; }
            public RWEnum WR { get; set; }
            public float gameTick { get; set; }
            public GameObject sender { get; set; }
            public Obj_AI_Minion shadowObj { get; set; }
        }
    }
}