using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ChampionUtils {
    internal class LeagueUtils {
        private int lastPingTime;
        private Vector2 pingLocation;

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
        ///     sends a local ping packet to the client
        /// </summary>
        /// <param name="position"> the position to send the ping. </param>
        public void sendPingPacket(Vector2 position) {
            if (Environment.TickCount - lastPingTime < 30*1000)
                return;
            lastPingTime = Environment.TickCount;
            pingLocation = position;
            sendSimplePing();
            Utility.DelayAction.Add(150, sendSimplePing);
            Utility.DelayAction.Add(300, sendSimplePing);
            Utility.DelayAction.Add(400, sendSimplePing);
            Utility.DelayAction.Add(800, sendSimplePing);
        }

        /// <summary>
        ///     Sends the actual ping.
        /// </summary>
        private void sendSimplePing() {
            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pingLocation.X, pingLocation.Y, 0, 0,
                Packet.PingType.FallbackSound)).Process();
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
        ///     sends a movement packet to a given position
        /// </summary>
        public void sendMovementPacket(Vector2 position) {
            Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(position.X, position.Y)).Send();
        }

        /// <summary>
        ///     Casts a basic line skillshot towards target if hitchance is high
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="damageType"></param>
        /// <returns> target </returns>
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

        /// <summary>
        ///     Casts a basic circle skillshot towards target if hitchance is high
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="damageType"></param>
        /// <param name="extrarange"></param>
        /// <returns></returns>
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
    }
}