using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

//TODO idea, use EvadeSpellDatabase or .dll to have an option to use ultimate to dodge dangeruous spells like Grag ult when evade can't dodge, so it doesn't waste ur R ? 
//TODO - reply here.

namespace Assemblies {
    //Kappa
    internal class Zed : Champion {
        private bool ROut;
        private ZedShadow RShadow;
        private bool WOut;
        private ZedShadow WShadow;
        private HitChance customHitchance = HitChance.High;
        private List<ZedShadow> shadowList;
        private enum RWEnum{R, W};
        private bool isChampKill; //but what if champ is not kill Kappa
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

            Game.PrintChat("[Assemblies] - Zed Loaded." + "swag.");
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

            Game.PrintChat("Zed by iJava, Princer007 and DZ191 Loaded.");
        }

        private void onUpdate(EventArgs args) {
            throw new NotImplementedException();
        }

        private void onDraw(EventArgs args) {
            throw new NotImplementedException();
        }

        private void fillShadowList() {} // todo wat? :S

        private Obj_AI_Hero getDeathmarkedTarget() {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(heroes => heroes.IsEnemy)
                    .FirstOrDefault(heroes => heroes.HasBuff("zedulttargetmark", true));
                // <-- is that the actual buff name or nah? It is
        }
        private bool isTargetKilled()
        {
            return isChampKill;
        }
        private bool canGoBackW() {
            return player.Spellbook.GetSpell(SpellSlot.W).Name == "zedw2";
        }

        private bool canGoBackR() {
            return player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2";
        }

        /**
         * This would work? Ye,maybe
         */
        private bool canBackToShadow() {
            return player.Spellbook.GetSpell(SpellSlot.W).Name == "zedw2" ||
                   player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2";
        }
        private int GetCountNearPos(Vector3 pos,float range)
        {
            int count = 0;
            foreach(Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && !hero.IsDead && hero.IsValid && hero.Distance(pos)<=range))
                count++;
            return count;
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
            if (sender.Name.Contains("Zed_Base_R_buf_tell.troy"))
                isChampKill = false;
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
            if (sender.Name.Contains("Zed_Base_R_buf_tell.troy"))
                isChampKill = true;
        }
        private Obj_AI_Minion CheckForClones(RWEnum RorW)
        {
            switch(RorW){
                case RWEnum.W:
                    var obj1 = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(obj => obj.Name == "Shadow" && obj.IsAlly && obj != RShadow.shadowObj);
                    if (obj1 != null)
                        return obj1;
                    return null;
                case RWEnum.R:
                    var obj2 = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(obj => obj.Name == "Shadow" && player.Distance(obj)<50 && obj.IsAlly && obj != WShadow.shadowObj);
                    if (obj2 != null)
                        return obj2;
                    return null;
                default:
                    return null;
            }
        }
        private class ZedShadow {
            public Vector3 shadowPosition { get; set; }
            public RWEnum WR { get; set; }
            public float gameTick { get; set; }
            public GameObject sender { get; set; }
            public Obj_AI_Minion shadowObj { get; set; }
        }
    }
}