using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies {
    internal class SkinManager {
        private readonly List<string> Skins = new List<string>();
        private bool Initialize = true;
        private Menu Menu;
        private int SelectedSkin;

        public void AddToMenu(ref Menu menu) {
            Menu = menu;
            if (Skins.Count <= 0) return;
            Menu.AddSubMenu(new Menu("Skin Changer", "Skin Changer"));
            Menu.SubMenu("Skin Changer")
                .AddItem(
                    new MenuItem("Skin_" + ObjectManager.Player.ChampionName + "_enabled", "Enable skin changer")
                        .SetValue(true));
            Menu.SubMenu("Skin Changer")
                .AddItem(
                    new MenuItem("Skin_" + ObjectManager.Player.ChampionName + "_select", "Skins").SetValue(
                        new StringList(Skins.ToArray())));
            SelectedSkin =
                Menu.Item("Skin_" + ObjectManager.Player.ChampionName + "_select")
                    .GetValue<StringList>()
                    .SelectedIndex;
        }

        public void Add(string skin) {
            Skins.Add(skin);
        }

        public void Update() {
            if (Menu.Item("Skin_" + ObjectManager.Player.ChampionName + "_enabled").GetValue<bool>()) {
                int skin =
                    Menu.Item("Skin_" + ObjectManager.Player.ChampionName + "_select")
                        .GetValue<StringList>()
                        .SelectedIndex;
                if (Initialize || skin != SelectedSkin) {
                    GenerateSkinPacket(skin);
                    SelectedSkin = skin;
                    Initialize = false;
                }
            }
        }

        private static void GenerateSkinPacket(int skinNumber) {
            int netID = ObjectManager.Player.NetworkId;
            GamePacket model =
                Packet.S2C.UpdateModel.Encoded(new Packet.S2C.UpdateModel.Struct(ObjectManager.Player.NetworkId,
                    skinNumber, ObjectManager.Player.ChampionName));
            model.Process();
        }
    }
}