//TODO auto R KS for stacks reset maybe
//AUtoShroud erm, fake recall in top lane? :3
//Flee mode using jungle camps or miniions
//Maybe different combo modes switchable using StringList ofc
/* 
Combo:
Use Q -> R -> E -> AA -> W, not AA when in shroud but if killable
If not in Q range
Use R -> Q -> E -> AA -> W, not AA when in shroud but if killable

 * This code need refactoring ASAP!
*/

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Igniter;
using Color = System.Drawing.Color;

namespace Assemblies
{
    internal class Akali : Champion
    {
        private Obj_AI_Hero rektmate = default(Obj_AI_Hero);
        public Akali()
        {
            if (player.ChampionName != "Akali")
            {
                return;
            }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;

            Game.PrintChat("[Assemblies] - Akali Loaded. Swag.");
        }


        private void loadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 800);
        }

        private void loadMenu()
        {
            menu.AddSubMenu(new Menu("Combo Options", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "Use Q in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "Use W in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useEC", "Use E in combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "Use R in combo").SetValue(true));

            menu.AddSubMenu(new Menu("Harass Options", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "Use Q in harass").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useEH", "Use E in harass").SetValue(false));

            menu.AddSubMenu(new Menu("Lane Clear", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQL", "Use Q in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useEL", "Use E in laneclear").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("hitCounter", "Use E if will hit min").SetValue(new Slider(3, 1, 6)));

            menu.AddSubMenu(new Menu("Miscellaneous", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("escape", "Escape key").SetValue(new KeyBind('G', KeyBindType.Press)));
            menu.SubMenu("misc").AddItem(new MenuItem("RCounter", "Do not escape if R<").SetValue(new Slider(1, 1, 3)));

            //TODO items

            Game.PrintChat("Akali by iJava, Princer007 and DZ191 Loaded.");
        }

        private void onUpdate(EventArgs args)
        {
            Combo();
            if (menu.SubMenu("misc").Item("escape").GetValue<KeyBind>().Active) Escape();
        }

        private void onDraw(EventArgs args)
        {
            if (menu.SubMenu("misc").Item("escape").GetValue<KeyBind>().Active)
                Utility.DrawCircle(Game.CursorPos, 200, W.IsReady() ? Color.Blue : Color.Red, 3);
        }

        private void Combo()
        {
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    RapeTime();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (menu.SubMenu("laneclear").Item("useQL").GetValue<bool>())
                        castQ(false);
                    if (menu.SubMenu("laneclear").Item("useEL").GetValue<bool>())
                        castE(false);
                    break;
            }
        }

        private void castQ(bool mode)
        {
            if (!Q.IsReady()) return;
            if (mode)
            {
                Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
                if (target == null || !target.IsValidTarget(Q.Range)) return;
                Q.Cast(target, true);
            }
            else
            {
                foreach (Obj_AI_Base minion in MinionManager.GetMinions(player.Position, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health))
                    if (HealthPrediction.GetHealthPrediction(minion, (int)(E.Delay + (minion.Distance(player) / E.Speed))*999) < player.GetSpellDamage(minion, SpellSlot.Q) &&
                        HealthPrediction.GetHealthPrediction(minion, (int)(E.Delay + (minion.Distance(player) / E.Speed))*999) > 0 &&
                        player.Distance(minion) > Orbwalking.GetRealAutoAttackRange(player))
                        Q.Cast(minion);
            }
        }

        private void castE(bool mode)
        {
            if (!E.IsReady()) return;
            if (mode)
            {
                Obj_AI_Hero target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
                if (target == null || !target.IsValidTarget(E.Range)) return;
                //TODO E if target has Q Buff for more dmg
                //TODO AA this moofuka if E is on CD (c) Princer007 <- done ( -_-)
                if (hasBuff(target, "AkaliMota") && !E.IsReady() && Orbwalking.GetRealAutoAttackRange(player) >= player.Distance(target))
                    orbwalker.ForceTarget(target);
                 else
                    E.Cast(target, true);
            }
            else
            {   //Minions in E range                                                                            >= Value in menu
                if (MinionManager.GetMinions(player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy).Count >= menu.SubMenu("laneclear").Item("hitCounter").GetValue<Slider>().Value) E.Cast();
            }
        }

        private void castR()
        {
            if (!R.IsReady()) return;
            Obj_AI_Hero target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (target == null || !target.IsValidTarget(R.Range) && target.Distance(player) < Orbwalking.GetRealAutoAttackRange(player)) return;
            R.Cast(target, true);
        }

        private void RapeTime()
        {
            Obj_AI_Hero possibleVictim = SimpleTs.GetTarget(R.Range * 2 + Orbwalking.GetRealAutoAttackRange(player), SimpleTs.DamageType.Magical);
            if (IsRapeble(possibleVictim)) rektmate = possibleVictim;
            else rektmate = default(Obj_AI_Hero);
            if (rektmate != default(Obj_AI_Hero))
            {
                if (player.Distance(rektmate) < 1600 && player.Distance(rektmate) > Orbwalking.GetRealAutoAttackRange(player))
                    castREscape(rektmate.Position);
                else if (player.Distance(rektmate) < Orbwalking.GetRealAutoAttackRange(player))
                    RaperinoCasterino(rektmate);
            }
            else
            {
                orbwalker.SetAttacks(!R.IsReady() && !Q.IsReady() && !E.IsReady());
                if (menu.Item("useQC").GetValue<bool>())
                    castQ(true);
                if (menu.Item("useEC").GetValue<bool>())
                    castE(true);
                if (menu.Item("useRC").GetValue<bool>())
                    castR();
            }
        }

        private void RaperinoCasterino(Obj_AI_Hero victim)
        {
            orbwalker.SetAttacks(!R.IsReady() && !Q.IsReady() && !E.IsReady() && player.Distance(victim) < 800f);
            orbwalker.ForceTarget(victim);
            foreach (var item in player.InventoryItems)
                switch ((int)item.Id)
                {
                    case 3144:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            player.Spellbook.CastSpell((SpellSlot)item.Slot);
                        break;
                    case 3146:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            player.Spellbook.CastSpell((SpellSlot)item.Slot);
                        break;
                    case 3128:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            player.Spellbook.CastSpell((SpellSlot)item.Slot);
                        break;
                }
            if (Q.IsReady() && Q.InRange(victim.Position)) Q.Cast(victim);
            if (E.IsReady() && E.InRange(victim.Position)) E.Cast();
            if (W.IsReady() && W.InRange(victim.Position)) W.Cast(V2E(player.Position, victim.Position, player.Distance(victim)+W.Width-20));
            if (R.IsReady() && R.InRange(victim.Position)) R.Cast(victim);
            new Igniter.Ignite().Cast(victim);
        }

        private bool IsRapeble(Obj_AI_Hero victim)
        {
            double comboDamage = 0d;
            if (Q.IsReady()) comboDamage += player.GetSpellDamage(victim, SpellSlot.Q);
            if (W.IsReady()) comboDamage += player.GetSpellDamage(victim, SpellSlot.W);
            comboDamage += player.GetAutoAttackDamage(victim, true);
            comboDamage += player.CalcDamage(victim, Damage.DamageType.Magical, CalcPassiveDmg());
            comboDamage += player.CalcDamage(victim, Damage.DamageType.Magical, CalcItemsDmg(victim));
            foreach (var item in player.InventoryItems)
                if((int)item.Id == 3128)
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            comboDamage *= 1.15;
            if (R.IsReady() && ultiCount() > 2) comboDamage += player.GetSpellDamage(victim, SpellSlot.R);
            //MAAAAAAAAAAAAAD. I'm fucked up. Gotta get cup of hot tea :3
            return victim.Health < comboDamage;
        }

        private double CalcPassiveDmg()
        {
            return (0.06 + 0.01 * (player.FlatMagicDamageMod / 6)) * (player.FlatPhysicalDamageMod + player.BaseAttackDamage);
        }

        private double CalcItemsDmg(Obj_AI_Hero victim)
        {
            double result = 0d;
            foreach (var item in player.InventoryItems)
                switch ((int)item.Id)
                {
                    case 3100: // LichBane
                        result += player.BaseAttackDamage * 0.75 + player.FlatMagicDamageMod * 0.5;
                        break;
                    case 3057://Sheen
                        result += player.BaseAttackDamage;
                        break;
                    case 3144:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            result += 100d;
                        break;
                    case 3146:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            result += 150d+player.FlatMagicDamageMod*0.4;
                        break;
                    case 3128:
                        if (player.Spellbook.CanUseSpell((SpellSlot)item.Slot) == SpellState.Ready)
                            result += victim.MaxHealth*0.2;
                        break;
                }

            return result;
        }

        private void Escape()
        {
            Vector3 cursorPos = Game.CursorPos;
            Vector2 pos = V2E(player.Position, cursorPos, R.Range);
            Vector2 pass = V2E(player.Position, cursorPos, 120);
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(pass.X, pass.Y)).Send();
            if (menu.SubMenu("misc").Item("RCounter").GetValue<Slider>().Value > ultiCount()) return;
            if (!IsWall(pos) && IsPassWall(player.Position, pos.To3D()) && MinionManager.GetMinions(cursorPos, 800, MinionTypes.All, MinionTeam.NotAlly).Count < 1)
                if (W.IsReady()) W.Cast(V2E(player.Position, cursorPos, W.Range));
            castREscape(cursorPos, true);
        }

        private void castREscape(Vector3 position, bool mouseJump = false)
        {
            Obj_AI_Base target = MinionManager.GetMinions(player.Position, 800, MinionTypes.All, MinionTeam.NotAlly)[0];
            foreach (Obj_AI_Base minion in ObjectManager.Get<Obj_AI_Base>())
                if (minion.IsValidTarget(R.Range, true) && player.Distance(target) < player.Distance(minion) && minion.Distance(position) < target.Distance(position))
                    if (mouseJump)
                    {
                        if (minion.Distance(position) < 200)
                            target = minion;
                    }
                    else
                    {
                        target = minion;
                    }
            if (R.IsReady() && R.InRange(target.Position))
                if (mouseJump)
                {
                    if (target.Distance(position) < 200)
                        R.Cast(target, true);
                }
                else
                    R.Cast(target, true);
        }

        private static bool IsPassWall(Vector3 start, Vector3 end)
        {
            double count = Vector3.Distance(start, end);
            for (uint i = 0; i <= count; i += 10)
            {
                Vector2 pos = V2E(start, end, i);
                if (IsWall(pos)) return true;
            }
            return false;
        }

        private int ultiCount()
        {
            return (from buff in player.Buffs where buff.Name == "AkaliShadowDance" select buff.Count).FirstOrDefault();
        }

        private static bool IsWall(Vector2 pos)
        {
            return (NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Wall ||
                    NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Building);
        }

        private static Vector2 V2E(Vector3 from, Vector3 direction, float distance)
        {
            return from.To2D() + distance * Vector3.Normalize(direction - from).To2D();
        }
    }
}
