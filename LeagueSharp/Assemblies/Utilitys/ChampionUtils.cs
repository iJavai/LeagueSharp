using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Assemblies.Utilitys {
    internal class ChampionUtils {
        /// <summary>
        ///     Uses a given spell at a teleporting location to immobilize the enemy
        /// </summary>
        /// <param name="spell"> the given spell I.E Caitlyn W</param>
        public void useSpellOnTeleport(Spell spell) {
            Obj_AI_Hero player = ObjectManager.Player;
            if (!spell.IsReady()) return;
            foreach (Obj_AI_Hero targetPosition in ObjectManager.Get<Obj_AI_Hero>().Where(
                obj =>
                    obj.Distance(player) < spell.Range && obj.Team != player.Team &&
                    obj.HasBuff("teleport_target", true))) {
                spell.Cast(targetPosition.ServerPosition);
            }
        }

        protected bool isWall(Vector3 pos) {
            return (NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Wall ||
                    NavMesh.GetCollisionFlags(pos.X, pos.Y) == CollisionFlags.Building);
        }

        /// <summary>
        ///     Gets the nearest enemy from your position
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>the nearest enemy ^.^</returns>
        protected Obj_AI_Hero getNearestEnemy(Obj_AI_Hero unit) {
            return ObjectManager.Get<Obj_AI_Hero>()
                .Where(x => x.IsEnemy && x.IsValid)
                .OrderBy(x => unit.ServerPosition.Distance(x.ServerPosition))
                .FirstOrDefault();
        }

        /// <summary>
        ///     Returns if the Menu item is enabled. For convenience.
        /// </summary>
        /// <param name="menu">The menu object</param>
        /// <param name="item">Item's name</param>
        /// <returns>The value</returns>
        protected bool isMenuEnabled(Menu menu, String item) {
            return menu.Item(item).GetValue<bool>();
        }

        protected bool isMenuEnabled(Menu menu , IEnumerable<string> items) {
            return items.Select(item => menu.Item(item).GetValue<bool>()).FirstOrDefault();
        }

        /// <summary>
        ///     Sends a simple ping to a given position.
        /// </summary>
        /// <param name="pos">the position to send the ping. </param>
        protected void sendSimplePing(Vector3 pos) {
            Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(pos.X, pos.Y))
                .Process();
        }

        /// <summary>
        ///     gets the percentage value of either health or mana.
        /// </summary>
        /// <param name="unit"> mainTarget to check percentage for I.E unit </param>
        /// <param name="mana"> if you want to use mana make this true</param>
        /// <returns></returns>
        protected float getPercentValue(Obj_AI_Hero unit, bool mana) {
            return mana ? (int) (unit.Mana/unit.MaxMana)*100 : (int) (unit.Health/unit.MaxHealth)*100;
        }

        /// <summary>
        ///     gets minions and champs in a spells path.
        /// </summary>
        /// <param name="player"> the unit </param>
        /// <param name="target"> the target </param>
        /// <param name="spell"> the spell to do the calculations for </param>
        /// <returns>
        ///     if a target is killable with given spell, taking into account damage reduction from minions and champs it
        ///     passes through also takes into account health regeneration rate, returns true / false.
        /// </returns>
        protected bool getUnitsInPath(Obj_AI_Hero player, Obj_AI_Hero target, Spell spell) {
            float distance = player.Distance(target);
            List<Obj_AI_Base> minionList = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spell.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            int numberOfMinions = (from Obj_AI_Minion minion in minionList
                let skillshotPosition =
                    V2E(player.Position,
                        V2E(player.Position, target.Position,
                            Vector3.Distance(player.Position, target.Position) - spell.Width + 1).To3D(),
                        Vector3.Distance(player.Position, minion.Position))
                where skillshotPosition.Distance(minion) < spell.Width
                select minion).Count();
            int numberOfChamps = (from minion in ObjectManager.Get<Obj_AI_Hero>()
                let skillshotPosition =
                    V2E(player.Position,
                        V2E(player.Position, target.Position,
                            Vector3.Distance(player.Position, target.Position) - spell.Width + 1).To3D(),
                        Vector3.Distance(player.Position, minion.Position))
                where skillshotPosition.Distance(minion) < spell.Width && minion.IsEnemy
                select minion).Count();
            int totalUnits = numberOfChamps + numberOfMinions - 1;
            // total number of champions and minions the projectile will pass through.
            if (totalUnits == -1) return false;
            //if total higher or equal to 7 then damage reduction = 0.3 else if total == 0 then damage reduction = 1.0 else damage reduction = 1 - total / 10 // TODO make this useable for similar champs.
            double damageReduction = 0;
            switch (ObjectManager.Player.ChampionName) {
                case "Ezreal":
                    damageReduction = ((totalUnits >= 7)) ? 0.3 : (totalUnits == 0) ? 1.0 : (1 - ((totalUnits)/10));
                    break;
            }
            // the damage reduction calculations minus percentage for each unit it passes through!
            return spell.GetDamage(target)*damageReduction >= (target.Health + (distance/2000)*target.HPRegenRate);
            // - 15 is a safeguard for certain kill.
        }

        /// <summary>
        ///     Checks if a unit has the given buff.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="buffName"></param>
        /// <returns>true if buff is on unit / mainTarget</returns>
        protected bool hasBuff(Obj_AI_Hero target, string buffName) {
            return
                target.Buffs.Any(buff => String.Equals(buff.Name, buffName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        ///     Checks if an allies health is under given percentage and is in range
        /// </summary>
        /// <param name="percentage"> the health percent I.E 50 </param>
        /// <param name="range"> the range</param>
        /// <returns></returns>
        public bool getAllyHealthPercentage(int percentage, float range) {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(ally => ally.IsAlly)
                    .Any(
                        ally =>
                            Vector3.Distance(ObjectManager.Player.Position, ally.Position) < range &&
                            getPercentValue(ObjectManager.Player, false) < percentage);
        }

        /// <summary>
        ///     Checks if a unit is under an enemy turret then returns a bool value.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>true / false</returns>
        protected bool isUnderEnemyTurret(Obj_AI_Base unit) {
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
        protected void sendMovementPacket(Vector2 position) {
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
        private Vector2 V2E(Vector3 from, Vector3 direction, float distance) {
            return from.To2D() + distance*Vector3.Normalize(direction - from).To2D();
        }

        protected List<Obj_AI_Hero> getEnemiesInRange(Vector3 pos, float range) {
            return ObjectManager.Get<Obj_AI_Hero>().Where(player => player.IsValid && player.IsEnemy).ToList();
        }

        protected float getDistanceSqr(Obj_AI_Hero source, Obj_AI_Hero target) {
            return Vector2.DistanceSquared(source.ServerPosition.To2D(), target.ServerPosition.To2D());
        }

        protected int countEnemiesNearPosition(Vector3 pos, float range) {
            return
                ObjectManager.Get<Obj_AI_Hero>().Count(
                    hero => hero.IsEnemy && !hero.IsDead && hero.IsValid && hero.Distance(pos) <= range);
        }
    }
}
