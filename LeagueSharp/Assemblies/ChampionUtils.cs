using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Assemblies {
    internal class ChampionUtils {
        public void trapTeleportingTarget(Spell spell) {
            // Credits MADK
            Obj_AI_Hero player = ObjectManager.Player;
            if (!spell.IsReady()) return;
            foreach (
                Obj_AI_Base targetPosition in
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(
                            obj =>
                                obj.Distance(player) < spell.Range && obj.Team != player.Team &&
                                obj.HasBuff("teleport_target", true))) {
                spell.Cast(targetPosition.Position);
            }
        }

        public float getPercentValue(Obj_AI_Hero player, bool mana) {
            return mana ? (int) (player.Mana/player.MaxMana)*100 : (int) (player.Health/player.MaxHealth)*100;
        }

        public bool hasBuff(Obj_AI_Hero target, string buffName) {
            return
                target.Buffs.Any(buff => String.Equals(buff.Name, buffName, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool getAllyHealth(int percentage, float range) {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => ally.IsAlly)
                    .Any(
                        ally =>
                            Vector3.Distance(ObjectManager.Player.Position, ally.Position) < range &&
                            ((ally.Health/ally.MaxHealth)*100) < percentage);
        }

        public void sendMovementPacket(Vector2 point) {
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(point.X, point.Y)).Send();
        }

        public Obj_AI_Hero castLineSkillShot(Spell spell,
            SimpleTs.DamageType damageType = SimpleTs.DamageType.Physical) {
            if (!spell.IsReady())
                return null;
            Obj_AI_Hero target = SimpleTs.GetTarget(spell.Range, damageType);
            if (target == null)
                return null;
            if (!target.IsValidTarget(spell.Range) || spell.GetPrediction(target).Hitchance < HitChance.High)
                return null;
            spell.Cast(target, true);
            return target;
        }

        public Obj_AI_Base castCircleSkillShot(Spell spell,
            SimpleTs.DamageType damageType = SimpleTs.DamageType.Physical, float extrarange = 0) {
            if (!spell.IsReady())
                return null;
            Obj_AI_Hero target = SimpleTs.GetTarget(spell.Range + extrarange, damageType);
            if (target == null)
                return null;
            if (target.IsValidTarget(spell.Range + extrarange) &&
                spell.GetPrediction(target).Hitchance >= HitChance.High)
                spell.Cast(target, true);
            return target;
        }

        public Vector3 getReversePosition(Vector3 myPos, Vector3 enemyPos) {
            float x = myPos.X - enemyPos.X;
            float y = myPos.Y - enemyPos.Y;
            return new Vector3(myPos.X + x, myPos.Y + y, myPos.Z);
        }
    }
}