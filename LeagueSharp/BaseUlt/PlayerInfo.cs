using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace BaseUlt {
    internal class PlayerInfo {
        public readonly Obj_AI_Hero Champ;
        public readonly Dictionary<int, float> IncomingDamage;
        public int LastSeen;
        public Packet.S2C.Recall.Struct Recall;

        public PlayerInfo(Obj_AI_Hero champ) {
            Champ = champ;
            Recall = new Packet.S2C.Recall.Struct(champ.NetworkId, Packet.S2C.Recall.RecallStatus.Unknown, Packet.S2C.Recall.ObjectType.Player, 0);
            IncomingDamage = new Dictionary<int, float>();
        }

        public PlayerInfo UpdateRecall(Packet.S2C.Recall.Struct newRecall) {
            Recall = newRecall;
            return this;
        }

        public int GetRecallStart() {
            switch ((int)Recall.Status) {
                case (int)Packet.S2C.Recall.RecallStatus.RecallStarted:
                case (int)Packet.S2C.Recall.RecallStatus.TeleportStart:
                    return Program.RecallT[Recall.UnitNetworkId];

                default:
                    return 0;
            }
        }

        public int GetRecallEnd() {
            return GetRecallStart() + Recall.Duration;
        }

        public int GetRecallCountdown() {
            int countdown = GetRecallEnd() - Environment.TickCount;
            return countdown < 0 ? 0 : countdown;
        }

        public override string ToString() {
            string drawtext = Champ.ChampionName + ": " + Recall.Status; //change to better string

            float countdown = GetRecallCountdown() / 1000f;

            if (countdown > 0)
                drawtext += " (" + countdown.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s)";

            return drawtext;
        }
    }
}