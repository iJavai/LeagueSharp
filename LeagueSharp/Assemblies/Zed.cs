using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using SharpDX;

//TODO idea, use EvadeSpellDatabase or .dll to have an option to use ultimate to dodge dangeruous spells like Grag ult when evade can't dodge, so it doesn't waste ur R ? 
//TODO - reply here.
//TODO - when hes played more we will finish this tbh, i doubt he can carry solo q anyway too team orientated..
//zedulttargetmark
//return player.Spellbook.GetSpell(SpellSlot.W).Name == "zedw2";
//return player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2";

namespace Assemblies {
    internal class Zed : Champion {

        public Zed() {
            if (player.ChampionName != "Zed") {
                return;
            }
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
           
        }

        private void onDraw(EventArgs args) {
            
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

        private void onDeleteObject(GameObject sender, EventArgs args) {
           
        }

        private void onProcessSpell(GameObject sender, EventArgs args) {
            
        }
    }
}