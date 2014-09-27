using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace BaseUlt {
    internal class Helper {
        public static T GetSafeMenuItem<T>(MenuItem item) {
            if (item != null)
                return item.GetValue<T>();

            return default(T);
        }

        public static float GetTargetHealth(PlayerInfo playerInfo) {
            if (playerInfo.Champ.IsVisible)
                return playerInfo.Champ.Health;

            float predictedhealth = playerInfo.Champ.Health +
                                    playerInfo.Champ.HPRegenRate*
                                    ((Environment.TickCount - playerInfo.LastSeen + playerInfo.GetRecallCountdown())/
                                     1000f);

            return predictedhealth > playerInfo.Champ.MaxHealth ? playerInfo.Champ.MaxHealth : predictedhealth;
        }

        public static float GetSpellTravelTime(Obj_AI_Hero source, float speed, float delay, Vector3 targetpos) {
            if (source.ChampionName == "Karthus")
                return delay*1000;

            float distance = Vector3.Distance(source.ServerPosition, targetpos);

            float missilespeed = speed;

            if (source.ChampionName == "Jinx" && distance > 1350)
                //1700 = missilespeed, 2200 = missilespeed after acceleration, 1350 acceleration starts, 1500 = fully acceleration
            {
                float accelerationrate = 0.3f; //= (1500f - 1350f) / (2200 - speed), 1 unit = 0.3units/second

                float acceldifference = distance - 1350f;

                if (acceldifference > 150f) //it only accelerates 150 units
                    acceldifference = 150f;

                float difference = distance - 1500f;

                missilespeed = (1350f*speed + acceldifference*(speed + accelerationrate*acceldifference) +
                                difference*2200f)/distance;
            }

            return (distance/missilespeed + delay)*1000;
        }

        public static bool IsCollidingWithChamps(Obj_AI_Hero source, Vector3 targetpos, float width) {
            var input = new PredictionInput {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;

            return Collision.GetCollision(new List<Vector3> {targetpos}, input).Any();
                //x => x.NetworkId != targetnetid, bit harder to realize with teamult
        }

        public static Packet.S2C.Recall.Struct RecallDecode(byte[] data) {
            var reader = new BinaryReader(new MemoryStream(data));
            var recall = new Packet.S2C.Recall.Struct();

            reader.ReadByte(); //PacketId
            reader.ReadInt32();
            recall.UnitNetworkId = reader.ReadInt32();
            reader.ReadBytes(66);

            recall.Status = Packet.S2C.Recall.RecallStatus.Unknown;

            bool teleport = false;

            if (BitConverter.ToString(reader.ReadBytes(6)) != "00-00-00-00-00-00") {
                if (BitConverter.ToString(reader.ReadBytes(3)) != "00-00-00") {
                    recall.Status = Packet.S2C.Recall.RecallStatus.TeleportStart;
                    teleport = true;
                }
                else
                    recall.Status = Packet.S2C.Recall.RecallStatus.RecallStarted;
            }

            reader.Close();

            var champ = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recall.UnitNetworkId);

            if (champ != null) {
                if (teleport)
                    recall.Duration = 3500;
                else
                    //use masteries to detect recall duration, because spelldata is not initialized yet when enemy has not been seen
                {
                    recall.Duration = Program.Map == Utility.Map.MapType.CrystalScar ? 4500 : 8000;

                    if (champ.Masteries.Any(x => x.Page == MasteryPage.Utility && x.Id == 65 && x.Points == 1))
                        recall.Duration -= Program.Map == Utility.Map.MapType.CrystalScar ? 500 : 1000;
                            //phasewalker mastery
                }

                int time = Environment.TickCount - Game.Ping;

                if (!Program.RecallT.ContainsKey(recall.UnitNetworkId))
                    Program.RecallT.Add(recall.UnitNetworkId, time);
                        //will result in status RecallStarted, which would be wrong if the assembly was to be loaded while somebody recalls
                else {
                    if (Program.RecallT[recall.UnitNetworkId] == 0)
                        Program.RecallT[recall.UnitNetworkId] = time;
                    else {
                        if (time - Program.RecallT[recall.UnitNetworkId] > recall.Duration - 75)
                            recall.Status = teleport
                                ? Packet.S2C.Recall.RecallStatus.TeleportEnd
                                : Packet.S2C.Recall.RecallStatus.RecallFinished;
                        else
                            recall.Status = teleport
                                ? Packet.S2C.Recall.RecallStatus.TeleportAbort
                                : Packet.S2C.Recall.RecallStatus.RecallAborted;

                        Program.RecallT[recall.UnitNetworkId] = 0; //recall aborted or finished, reset status
                    }
                }
            }

            return recall;
        }

        public static bool IsCompatibleChamp(String championName) {
            switch (championName) {
                case "Ashe":
                case "Ezreal":
                case "Draven":
                case "Jinx":
                    return true;

                default:
                    return false;
            }
        }

        public static double GetUltDamage(Obj_AI_Hero source, Obj_AI_Hero enemy) {
            switch (source.ChampionName) {
                case "Ashe":
                    return
                        CalcMagicDmg(
                            (75 + (source.Spellbook.GetSpell(SpellSlot.R).Level*175)) + (1.0*source.FlatMagicDamageMod),
                            source, enemy);
                case "Draven":
                    return
                        CalcPhysicalDmg(
                            (75 + (source.Spellbook.GetSpell(SpellSlot.R).Level*100)) +
                            (1.1*source.FlatPhysicalDamageMod), source, enemy); // way to enemy
                case "Jinx":
                    double percentage =
                        CalcPhysicalDmg(
                            ((enemy.MaxHealth - enemy.Health)/100)*
                            (20 + (5*source.Spellbook.GetSpell(SpellSlot.R).Level)), source, enemy);
                    return percentage +
                           CalcPhysicalDmg(
                               (150 + (source.Spellbook.GetSpell(SpellSlot.R).Level*100)) +
                               (1.0*source.FlatPhysicalDamageMod), source, enemy);
                case "Ezreal":
                    return CalcMagicDmg((200 + (source.Spellbook.GetSpell(SpellSlot.R).Level*150)) +
                                        (1.0*(source.FlatPhysicalDamageMod + source.BaseAttackDamage)) +
                                        (0.9*source.FlatMagicDamageMod), source, enemy);
                case "Karthus":
                    return CalcMagicDmg(
                        (100 + (source.Spellbook.GetSpell(SpellSlot.R).Level*150)) +
                        (0.6*source.FlatMagicDamageMod), source, enemy);
                default:
                    return 0;
            }
        }

        public static double CalcPhysicalDmg(double dmg, Obj_AI_Hero source, Obj_AI_Base enemy) {
            bool doubleedgedsword = false, havoc = false, arcaneblade = false, butcher = false;
            int executioner = 0;

            foreach (Mastery mastery in source.Masteries) {
                if (mastery.Page == MasteryPage.Offense) {
                    switch (mastery.Id) {
                        case 65:
                            doubleedgedsword = (mastery.Points == 1);
                            break;
                        case 146:
                            havoc = (mastery.Points == 1);
                            break;
                        case 132:
                            arcaneblade = (mastery.Points == 1);
                            break;
                        case 100:
                            executioner = mastery.Points;
                            break;
                        case 68:
                            butcher = (mastery.Points == 1);
                            break;
                    }
                }
            }

            double additionaldmg = 0;
            if (doubleedgedsword) {
                if (source.CombatType == GameObjectCombatType.Melee) {
                    additionaldmg += dmg*0.02;
                }
                else {
                    additionaldmg += dmg*0.015;
                }
            }

            if (havoc) {
                additionaldmg += dmg*0.03;
            }

            if (executioner > 0) {
                if (executioner == 1) {
                    if ((enemy.Health/enemy.MaxHealth)*100 < 20) {
                        additionaldmg += dmg*0.05;
                    }
                }
                else if (executioner == 2) {
                    if ((enemy.Health/enemy.MaxHealth)*100 < 35) {
                        additionaldmg += dmg*0.05;
                    }
                }
                else if (executioner == 3) {
                    if ((enemy.Health/enemy.MaxHealth)*100 < 50) {
                        additionaldmg += dmg*0.05;
                    }
                }
            }

            double newarmor = enemy.Armor*source.PercentArmorPenetrationMod;
            double dmgreduction = 100/(100 + newarmor - source.FlatArmorPenetrationMod);
            return (((dmg + additionaldmg)*dmgreduction));
        }

        public static double CalcMagicDmg(double dmg, Obj_AI_Hero source, Obj_AI_Base enemy) {
            bool doubleedgedsword = false, havoc = false, arcaneblade = false, butcher = false;
            int executioner = 0;

            foreach (Mastery mastery in source.Masteries) {
                if (mastery.Page == MasteryPage.Offense) {
                    switch (mastery.Id) {
                        case 65:
                            doubleedgedsword = (mastery.Points == 1);
                            break;
                        case 146:
                            havoc = (mastery.Points == 1);
                            break;
                        case 132:
                            arcaneblade = (mastery.Points == 1);
                            break;
                        case 100:
                            executioner = mastery.Points;
                            break;
                        case 68:
                            butcher = (mastery.Points == 1);
                            break;
                    }
                }
            }

            double additionaldmg = 0;
            if (doubleedgedsword) {
                if (source.CombatType == GameObjectCombatType.Melee) {
                    additionaldmg = dmg*0.02;
                }
                else {
                    additionaldmg = dmg*0.015;
                }
            }
            if (havoc) {
                additionaldmg += dmg*0.03;
            }
            if (executioner > 0) {
                if (executioner == 1) {
                    if ((enemy.Health/enemy.MaxHealth)*100 < 20) {
                        additionaldmg += dmg*0.05;
                    }
                }
                else if (executioner == 2) {
                    if ((enemy.Health/enemy.MaxHealth)*100 < 35) {
                        additionaldmg += dmg*0.05;
                    }
                }
                else if (executioner == 3) {
                    if ((enemy.Health/enemy.MaxHealth)*100 < 50) {
                        additionaldmg += dmg*0.05;
                    }
                }
            }

            double newspellblock = enemy.SpellBlock*source.PercentMagicPenetrationMod;
            double dmgreduction = 100/(100 + newspellblock - source.FlatMagicPenetrationMod);
            return (((dmg + additionaldmg)*dmgreduction));
        }
    }
}