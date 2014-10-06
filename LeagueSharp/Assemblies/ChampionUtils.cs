using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Assemblies {
    internal class ChampionUtils {
        /// <summary>
        ///     Uses a given spell at a teleporting location to immobilize the enemy
        /// </summary>
        /// <param name="spell"> the given spell I.E Caitlyn W</param>
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

        /// <summary>
        ///     Gets the nearest enemy from your position
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>the nearest enemy ^.^</returns>
        public Obj_AI_Hero getNearestEnemy(Obj_AI_Base unit) {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        /// <summary>
        ///     Sends a simple ping to a given position.
        /// </summary>
        /// <param name="pos">the position to send the ping. </param>
        public void sendSimplePing(Vector3 pos) {
            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos.X, pos.Y, 0, 0, Packet.PingType.NormalSound))
                .Process();
        }


        /// <summary>
        ///     gets the percentage value of either health or mana.
        /// </summary>
        /// <param name="player"> target to check percentage for I.E player </param>
        /// <param name="mana"> if you want to use mana make this true</param>
        /// <returns></returns>
        public float getPercentValue(Obj_AI_Hero player, bool mana) {
            return mana ? (int) (player.Mana/player.MaxMana)*100 : (int) (player.Health/player.MaxHealth)*100;
        }

        /// <summary>
        ///     Checks if a player has the given buff.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="buffName"></param>
        /// <returns>true if buff is on player / target</returns>
        public bool hasBuff(Obj_AI_Hero target, string buffName) {
            return
                target.Buffs.Any(buff => String.Equals(buff.Name, buffName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        ///     Checks if an allies health is under given percentage and is in range
        /// </summary>
        /// <param name="percentage"> the health percent I.E 50 </param>
        /// <param name="range"> the range</param>
        /// <returns></returns>
        public bool getAllyHealth(int percentage, float range) {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => ally.IsAlly)
                    .Any(
                        ally =>
                            Vector3.Distance(ObjectManager.Player.Position, ally.Position) < range &&
                            ((ally.Health/ally.MaxHealth)*100) < percentage);
        }

        /// <summary>
        ///     Checks if a unit is under an enemy turret then returns a bool value.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>true / false</returns>
        public bool IsUnderEnemyTurret(Obj_AI_Base unit) {
            IEnumerable<Obj_AI_Turret> turrets;
            if (unit.IsEnemy) {
                turrets = ObjectManager.Get<Obj_AI_Turret>()
                    .Where(
                        x =>
                            x.IsAlly && x.IsValid && !x.IsDead &&
                            unit.ServerPosition.Distance(x.ServerPosition) < x.AttackRange);
            }
            else {
                turrets = ObjectManager.Get<Obj_AI_Turret>()
                    .Where(
                        x =>
                            x.IsEnemy && x.IsValid && !x.IsDead &&
                            unit.ServerPosition.Distance(x.ServerPosition) < x.AttackRange);
            }
            return (turrets.Any());
        }


        /// <summary>
        ///     sends a movement packet to a given position
        /// </summary>
        public void sendMovementPacket(Vector2 position) {
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(position.X, position.Y)).Send();
        }

        /// <summary>
        ///     Gets the reverse position of you and your enemy.
        /// </summary>
        /// <param name="myPos"></param>
        /// <param name="enemyPos"></param>
        /// <returns>Vector 3 position reversed.</returns>
        public Vector3 getReversePosition(Vector3 myPos, Vector3 enemyPos) {
            float x = myPos.X - enemyPos.X;
            float y = myPos.Y - enemyPos.Y;
            return new Vector3(myPos.X + x, myPos.Y + y, myPos.Z);
        }

        /// <summary>
        ///     Extends a vector using the params from, direction, distance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        protected static Vector2 V2E(Vector3 from, Vector3 direction, float distance) {
            return from.To2D() + distance*Vector3.Normalize(direction - from).To2D();
        }
    }
}