using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace BaseUlt {
    internal class Program {
        private static Menu _menu;
        private static bool _compatibleChamp;
        private static IEnumerable<Obj_AI_Hero> _ownTeam;
        private static IEnumerable<Obj_AI_Hero> _enemyTeam;
        private static Vector3 _enemySpawnPos;
        private static List<PlayerInfo> _playerInfo = new List<PlayerInfo>();
        private static int _ultCasted;
        private static Spell _ult;

        public static Utility.Map.MapType Map;
        public static Dictionary<int, int> RecallT = new Dictionary<int, int>();

        private static readonly Dictionary<String, UltData> UltInfo = new Dictionary<string, UltData> {
            {
                "Jinx",
                new UltData {
                    ManaCost = 100f,
                    DamageMultiplicator = 1f,
                    Width = 140f,
                    Delay = 600f/1000f,
                    Speed = 1700f,
                    Range = 20000f
                }
            }, {
                "Ashe",
                new UltData {
                    ManaCost = 100f,
                    DamageMultiplicator = 1f,
                    Width = 130f,
                    Delay = 250f/1000f,
                    Speed = 1600f,
                    Range = 20000f
                }
            }, {
                "Draven",
                new UltData {
                    ManaCost = 120f,
                    DamageMultiplicator = 0.7f,
                    Width = 160f,
                    Delay = 400f/1000f,
                    Speed = 2000f,
                    Range = 20000f
                }
            }, {
                "Ezreal",
                new UltData {
                    ManaCost = 100f,
                    DamageMultiplicator = 0.7f,
                    Width = 160f,
                    Delay = 1000f/1000f,
                    Speed = 2000f,
                    Range = 20000f
                }
            }
        };

        private static void Main(string[] args) {
            Game.OnGameStart += Game_OnGameStart;

            if (Game.Mode == GameMode.Running)
                Game_OnGameStart(new EventArgs());
        }

        private static void Game_OnGameStart(EventArgs args) {
            (_menu = new Menu("BaseUlt2", "BaseUlt", true)).AddToMainMenu();
            _menu.AddItem(new MenuItem("showRecalls", "Show Recalls").SetValue(true));
            _menu.AddItem(new MenuItem("baseUlt", "Base Ult").SetValue(true));
            _menu.AddItem(new MenuItem("extraDelay", "Extra Delay").SetValue(new Slider(0, -2000, 2000)));
            _menu.AddItem(
                new MenuItem("panicKey", "Panic key (hold for disable)").SetValue(new KeyBind(32, KeyBindType.Press)));
            //32 == space
            _menu.AddItem(
                new MenuItem("regardlessKey", "No timelimit (hold)").SetValue(new KeyBind(17, KeyBindType.Press)));
            //17 == ctrl
            _menu.AddItem(new MenuItem("debugMode", "Debug (developer only)").SetValue(false).DontSave());

            Menu teamUlt = _menu.AddSubMenu(new Menu("Team Baseult Friends", "TeamUlt"));

            List<Obj_AI_Hero> champions = ObjectManager.Get<Obj_AI_Hero>().ToList();

            _ownTeam = champions.Where(x => x.IsAlly);
            _enemyTeam = champions.Where(x => x.IsEnemy);

            _compatibleChamp = Helper.IsCompatibleChamp(ObjectManager.Player.ChampionName);

            if (_compatibleChamp)
                foreach (Obj_AI_Hero champ in _ownTeam.Where(x => !x.IsMe && Helper.IsCompatibleChamp(x.ChampionName)))
                    teamUlt.AddItem(
                        new MenuItem(champ.ChampionName, champ.ChampionName + " friend with Baseult?").SetValue(false)
                            .DontSave());

            _enemySpawnPos =
                ObjectManager.Get<GameObject>()
                    .First(x => x.Type == GameObjectType.obj_SpawnPoint && x.Team != ObjectManager.Player.Team)
                    .Position;

            Map = Utility.Map.GetMap()._MapType;

            _playerInfo = _enemyTeam.Select(x => new PlayerInfo(x)).ToList();
            _playerInfo.Add(new PlayerInfo(ObjectManager.Player));

            _ult = new Spell(SpellSlot.R, 20000f);

            Game.OnGameProcessPacket += Game_OnGameProcessPacket;
            Drawing.OnDraw += Drawing_OnDraw;

            if (_compatibleChamp)
                Game.OnGameUpdate += Game_OnGameUpdate;

            Game.PrintChat(
                "<font color=\"#1eff00\">BaseUlt2 -</font> <font color=\"#00BFFF\">Loaded (compatible champ: " +
                (_compatibleChamp ? "Yes" : "No") + ")</font>");
        }

        private static void Game_OnGameUpdate(EventArgs args) {
            int time = Environment.TickCount;

            foreach (PlayerInfo playerInfo in _playerInfo.Where(x => x.Champ.IsVisible))
                playerInfo.LastSeen = time;

            if (!_menu.Item("baseUlt").GetValue<bool>()) return;

            foreach (PlayerInfo playerInfo in _playerInfo.Where(x =>
                x.Champ.IsValid &&
                !x.Champ.IsDead &&
                x.Champ.IsEnemy &&
                x.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted).OrderBy(x => x.GetRecallEnd())) {
                if (_ultCasted == 0 || Environment.TickCount - _ultCasted > 20000)
                    //DONT change Environment.TickCount; check for draven ult return
                    HandleRecallShot(playerInfo);
            }
        }

        private static float GetUltManaCost(Obj_AI_Hero source) //remove later when fixed
        {
            float manaCost = UltInfo[source.ChampionName].ManaCost;

            if (source.ChampionName == "Karthus") {
                if (source.Level >= 11)
                    manaCost += 25;

                if (source.Level >= 16)
                    manaCost += 25;
            }

            return manaCost;
        }

        private static void HandleRecallShot(PlayerInfo playerInfo) {
            bool shoot = false;

            foreach (Obj_AI_Hero champ in _ownTeam.Where(x =>
                x.IsValid && (x.IsMe || Helper.GetSafeMenuItem<bool>(_menu.Item(x.ChampionName))) &&
                !x.IsDead && !x.IsStunned &&
                (x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready ||
                 (x.Spellbook.GetSpell(SpellSlot.R).Level > 0 &&
                  x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Surpressed &&
                  x.Mana >= GetUltManaCost(x)))))
                //use when fixed: champ.Spellbook.GetSpell(SpellSlot.R) = Ready or champ.Spellbook.GetSpell(SpellSlot.R).ManaCost)
            {
                if (champ.ChampionName != "Ezreal" && champ.ChampionName != "Karthus" &&
                    Helper.IsCollidingWithChamps(champ, _enemySpawnPos, UltInfo[champ.ChampionName].Width))
                    continue;

                //increase timeneeded if it should arrive earlier, decrease if later
                float timeneeded =
                    Helper.GetSpellTravelTime(champ, UltInfo[champ.ChampionName].Speed,
                        UltInfo[champ.ChampionName].Delay, _enemySpawnPos) -
                    (_menu.Item("extraDelay").GetValue<Slider>().Value + 65);

                if (timeneeded - playerInfo.GetRecallCountdown() > 60)
                    continue;

                playerInfo.IncomingDamage[champ.NetworkId] = (float) Helper.GetUltDamage(champ, playerInfo.Champ)*
                                                             UltInfo[champ.ChampionName].DamageMultiplicator;

                if (playerInfo.GetRecallCountdown() <= timeneeded)
                    if (champ.IsMe)
                        shoot = true;
            }

            float totalUltDamage = playerInfo.IncomingDamage.Values.Sum();

            float targetHealth = Helper.GetTargetHealth(playerInfo);

            if (!shoot || _menu.Item("panicKey").GetValue<KeyBind>().Active) {
                if (_menu.Item("debugMode").GetValue<bool>())
                    Game.PrintChat("!SHOOT/PANICKEY {0} (Health: {1} TOTAL-UltDamage: {2})",
                        playerInfo.Champ.ChampionName, targetHealth, totalUltDamage);

                return;
            }

            playerInfo.IncomingDamage.Clear(); //wrong placement?

            int time = Environment.TickCount;

            if (time - playerInfo.LastSeen > 20000 && !_menu.Item("regardlessKey").GetValue<KeyBind>().Active) {
                if (totalUltDamage < playerInfo.Champ.MaxHealth) {
                    if (_menu.Item("debugMode").GetValue<bool>())
                        Game.PrintChat("DONT SHOOT, TOO LONG NO VISION {0} (Health: {1} TOTAL-UltDamage: {2})",
                            playerInfo.Champ.ChampionName, targetHealth, totalUltDamage);

                    return;
                }
            }
            else if (totalUltDamage < targetHealth) {
                if (_menu.Item("debugMode").GetValue<bool>())
                    Game.PrintChat("DONT SHOOT {0} (Health: {1} TOTAL-UltDamage: {2})", playerInfo.Champ.ChampionName,
                        targetHealth, totalUltDamage);

                return;
            }

            if (_menu.Item("debugMode").GetValue<bool>())
                Game.PrintChat("SHOOT {0} (Health: {1} TOTAL-UltDamage: {2})", playerInfo.Champ.ChampionName,
                    targetHealth, totalUltDamage);

            Game.PrintChat("Should shoot {0}: Target Health: {1} UltDamage: {2}", playerInfo.Champ.ChampionName,
                targetHealth, totalUltDamage);

            _ult.Cast(_enemySpawnPos, true);
            _ultCasted = time;
        }

        private static void Drawing_OnDraw(EventArgs args) {
            if (!_menu.Item("showRecalls").GetValue<bool>()) return;

            int index = -1;

            foreach (PlayerInfo playerInfo in _playerInfo.Where(x =>
                (x.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted ||
                 x.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart) &&
                x.Champ.IsValid &&
                !x.Champ.IsDead &&
                x.GetRecallCountdown() > 0 &&
                (x.Champ.IsEnemy || _menu.Item("debugMode").GetValue<bool>())).OrderBy(x => x.GetRecallEnd())) {
                index++;

                //draw progress bar
                //Draw circle on minimap.

                Drawing.DrawText(Drawing.Width*0.73f, Drawing.Height*0.88f + (index*15f), Color.Red,
                    playerInfo.ToString());
            }
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args) {
            if (args.PacketData[0] == Packet.S2C.Recall.Header) {
                Packet.S2C.Recall.Struct newRecall = Helper.RecallDecode(args.PacketData);

                PlayerInfo playerInfo =
                    _playerInfo.Find(x => x.Champ.NetworkId == newRecall.UnitNetworkId).UpdateRecall(newRecall);
                //Packet.S2C.Recall.Decoded(args.PacketData)

                if (_menu.Item("debugMode").GetValue<bool>())
                    Game.PrintChat(playerInfo.Champ.ChampionName + ": " + playerInfo.Recall.Status + " duration: " +
                                   playerInfo.Recall.Duration + " guessed health: " + Helper.GetTargetHealth(playerInfo) +
                                   " lastseen: " + playerInfo.LastSeen + " health: " + playerInfo.Champ.Health +
                                   " own-ultdamage: " +
                                   (float) Helper.GetUltDamage(ObjectManager.Player, playerInfo.Champ)*
                                   UltInfo[ObjectManager.Player.ChampionName].DamageMultiplicator);
            }
        }

        private struct UltData {
            public float DamageMultiplicator;
            public float Delay;
            public float ManaCost;
            public float Range;
            public float Speed;
            public float Width;
        }
    }
}