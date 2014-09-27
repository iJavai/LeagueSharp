using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace RandomChampions {
    internal class Program {
        private static readonly List<Spell> spellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        private static Orbwalking.Orbwalker orbwalker;
        private static Menu Config;

        private static Obj_AI_Minion Shadow {
            get {
                return
                    ObjectManager.Get<Obj_AI_Minion>()
                        .FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && minion.Name == "Shadow");
            }
        }

        private static ShadowCastStage ShadowStage {
            get {
                if (!_w.IsReady()) return ShadowCastStage.Cooldown;
                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedShadowDash"
                    ? ShadowCastStage.First
                    : ShadowCastStage.Second);
            }
        }

        private static void Main(string[] args) {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args) {
            #region Assign Spells

            _q = new Spell(SpellSlot.Q, 925f);
            _w = new Spell(SpellSlot.W, 600f);
            _e = new Spell(SpellSlot.E, 325f);
            _r = new Spell(SpellSlot.R, 650f);

            #endregion

            #region set skillshots or targetted

            _q.SetSkillshot(0.235f, 50f, 1700, false, SkillshotType.SkillshotLine);

            #endregion

            spellList.AddRange(new[] {_q, _w, _e, _r});

            #region target selector

            Config = new Menu(ObjectManager.Player.ChampionName, ObjectManager.Player.ChampionName, true);
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            #endregion

            #region Orbwalker

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            #endregion

            #region Combo menu

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.AddItem(new MenuItem("useQC", "Use Q").SetValue(true));
            Config.AddItem(new MenuItem("useWC", "Use W").SetValue(true));
            Config.AddItem(new MenuItem("useEC", "Use E").SetValue(true));
            Config.AddItem(new MenuItem("useRC", "Use R").SetValue(false));

            #endregion

            #region harass menu

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.AddItem(new MenuItem("useQH", "Use Q").SetValue(true));
            Config.AddItem(new MenuItem("useWH", "Use W").SetValue(true));
            Config.AddItem(new MenuItem("useEH", "Use E").SetValue(true));
            Config.AddItem(new MenuItem("useRH", "Use R").SetValue(false));

            #endregion

            #region misc menu

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.AddItem(new MenuItem("gapcloseW", "Use W to gapclose").SetValue(false));
            Config.AddItem(new MenuItem("useRDodge", "Use ult to dodge dangerous").SetValue(false));

            #endregion

            Config.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Drawing_OnDraw(EventArgs args) {
            if (Shadow != null)
                Utility.DrawCircle(Shadow.ServerPosition, Shadow.BoundingRadius*2, Color.MistyRose);
            //foreach (Vector3 vector in GetPossibleShadowPositions())
            //  Utility.DrawCircle(vector, 50f, Color.RoyalBlue);
        }

        private static void Game_OnGameUpdate(EventArgs args) {
            Obj_AI_Hero target = SimpleTs.GetTarget(_w.Range + _q.Range, SimpleTs.DamageType.Physical);

            if (target == null) return;

            switch (orbwalker.ActiveMode) {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (Config.Item("useQC").GetValue<bool>() && target.IsValidTarget(_w.Range + _q.Range))
                        CastQ(target);
                    if (Config.Item("useEC").GetValue<bool>() && target.IsValidTarget(_e.Range))
                        CastE();
                    break;
            }
        }

        private static void CastQ(Obj_AI_Base target) {
            if (!_q.IsReady()) return;
            _q.UpdateSourcePosition(ObjectManager.Player.ServerPosition, ObjectManager.Player.ServerPosition);
            if (_q.Cast(target, false, true) == Spell.CastStates.SuccessfullyCasted)
                return;
            if (Shadow != null) {
                _q.UpdateSourcePosition(Shadow.ServerPosition, Shadow.ServerPosition);
                _q.Cast(target, false, true);
            }
            if (ShadowStage == ShadowCastStage.First &&
                ObjectManager.Player.Mana >
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost) {
                foreach (
                    Vector3 castPosition in
                        GetPossibleShadowPositions()
                            .OrderBy(castPosition => castPosition.Distance(target.ServerPosition))) {
                    _q.UpdateSourcePosition(castPosition, castPosition);
                    if (_q.WillHit(target, target.ServerPosition)) {
                        if (LastCastedSpell.LastCastPacketSent.Slot != SpellSlot.W)
                            if (Config.Item("useWC").GetValue<bool>() || Config.Item("useWH").GetValue<bool>())
                                _w.Cast(castPosition);
                        Vector3 position = castPosition;
                        Utility.DelayAction.Add(250, () => {
                            _q.UpdateSourcePosition(position, position);
                            _q.Cast(target, false, true);
                        });
                        if (ShadowStage != ShadowCastStage.First)
                            return;
                    }
                }
            }
        }

        private static void CastE() {
            if (ObjectManager.Get<Obj_AI_Hero>()
                .Count(
                    hero =>
                        hero.IsValidTarget() &&
                        (hero.Distance(ObjectManager.Player.ServerPosition) <= _e.Range ||
                         (Shadow != null && hero.Distance(Shadow.ServerPosition) <= _e.Range))) > 0)
                _e.Cast();
        }

        private static IEnumerable<Vector3> GetPossibleShadowPositions() {
            var pointList = new List<Vector3>();
            for (float j = _w.Range; j >= 50; j -= 100) {
                var offset = (int) (2*Math.PI*j/100);
                for (int i = 0; i <= offset; i++) {
                    double angle = i*Math.PI*2/offset;
                    var point = new Vector3((float) (ObjectManager.Player.Position.X + j*Math.Cos(angle)),
                        (float) (ObjectManager.Player.Position.Y - j*Math.Sin(angle)),
                        ObjectManager.Player.Position.Z);
                    if (!NavMesh.GetCollisionFlags(point).HasFlag(CollisionFlags.Wall))
                        pointList.Add(point);
                }
            }
            return pointList;
        }

        private enum ShadowCastStage {
            First,
            Second,
            Cooldown
        }
    }
}