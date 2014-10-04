using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Assemblies {
    //Kappa
    internal class Zed : Champion {
        private bool ROut;
        private ZedShadow RShadow;
        private bool WOut;
        private ZedShadow WShadow;
        private HitChance customHitchance = HitChance.High;
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

            Game.PrintChat("[Assemblies] - Zed Loaded." + "Huehuehue.");
        }


        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 900);
            Q.SetSkillshot(0.235f, 50f, 1700, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 550);

            E = new Spell(SpellSlot.E, 290);

            R = new Spell(SpellSlot.R, 600);
        }

        private void loadMenu() {
            throw new NotImplementedException();
        }

        private void onUpdate(EventArgs args) {
            throw new NotImplementedException();
        }

        private void onDraw(EventArgs args) {
            throw new NotImplementedException();
        }

        private void fillShadowList() {}

        private Obj_AI_Hero getDeathmarkedTarget() {
            return ObjectManager.Get<Obj_AI_Hero>().Where(heroes => heroes.IsEnemy).FirstOrDefault(heroes => heroes.HasBuff("zedulttargetmark", true));
        }

        private bool canGoBackW() {
            return player.Spellbook.GetSpell(SpellSlot.W).Name == "zedw2";
        }

        private bool canGoBackR() {
            return player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2";
        }

        private void onDeleteObject(GameObject sender, EventArgs args) {
            GameObject theObject = sender;
            //Untested. No clue if this works. :3 -Dz191
            /*
             * I mean,it should
             * But there it this weird thing with Azir where he can push Zed shadows with his ult
             * so this might need to be perfected. Maybe using NetworkdId ? 
           */
            if (theObject.IsValid && theObject.Position.Distance(WShadow.shadowPosition) < 50) {
                WShadow = null;
                WOut = false;
            }
            if (theObject.IsValid && theObject.Position.Distance(RShadow.shadowPosition) < 50) {
                RShadow = null;
                ROut = false;
            }
        }

        private void onProcessSpell(GameObject sender, EventArgs args) {
            var theSpell = (Obj_SpellMissile) sender;

            if (sender.IsMe && theSpell.SData.Name == "ZedUltMissile") {
                RShadow = new ZedShadow {
                    shadowPosition = player.ServerPosition,
                    WR = "R",
                    gameTick = Game.Time,
                    sender = sender
                };
                ROut = true;
            }
            if (sender.IsMe && theSpell.SData.Name == "ZedShadowDashMissile") {
                WShadow = new ZedShadow {
                    shadowPosition = theSpell.EndPosition,
                    WR = "W",
                    gameTick = Game.Time,
                    sender = sender
                };
                WOut = true;
            }
        }

        private class ZedShadow {
            public Vector3 shadowPosition { get; set; }
            public String WR { get; set; }
            public float gameTick { get; set; }
            public GameObject sender { get; set; }
        }
    }
}