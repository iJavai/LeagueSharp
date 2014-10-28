using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Assemblies {
    internal class WardJumper {
        private readonly Spell jumpSpell;
        private readonly Obj_AI_Hero player = ObjectManager.Player;
        private int lastPlaced;
        private Vector3 lastWardPos;
        private Menu menu;

        public WardJumper() {
            jumpSpell = getJumpSpell();
            GameObject.OnCreate += GameObject_OnCreate;
            //Game.OnGameUpdate += processJump;
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args) {
            if (Environment.TickCount < lastPlaced + 300) {
                var ward = (Obj_AI_Minion) sender;
                if (ward.Name.ToLower().Contains("ward") && ward.Distance(lastWardPos) < 500) {
                    jumpSpell.Cast(ward);
                }
            }
        }

        public void processJump() {
            foreach (
                Obj_AI_Minion ward in
                    ObjectManager.Get<Obj_AI_Minion>().Where(
                        ward =>
                            menu.Item("Wardjump").GetValue<KeyBind>().Active && jumpSpell != null &&
                            ward.Name.ToLower().Contains("ward") && ward.Distance(Game.CursorPos) < 130 &&
                            ward.Distance(ObjectManager.Player) < jumpSpell.Range)) {
                jumpSpell.Cast(ward);
            }
            if (!menu.Item("Wardjump").GetValue<KeyBind>().Active || jumpSpell == null ||
                Environment.TickCount <= lastPlaced + 3000 || !IsJumpReady()) return;

            Vector3 cursorPosition = Game.CursorPos;
            Vector3 myPosition = player.Position;

            Vector3 delta = cursorPosition - myPosition;
            delta.Normalize();

            Vector3 wardPosition = myPosition + delta*(600 - 5);
            InventorySlot inventorySlot = getWardSlot();
            if (inventorySlot == null) return;

            inventorySlot.UseItem(wardPosition);
            lastWardPos = wardPosition;
            lastPlaced = Environment.TickCount;
        }

        public void AddToMenu(Menu attachMenu) {
            menu = attachMenu;
            menu.AddSubMenu(new Menu("Ward Jumper", "wardJumper"));
            menu.SubMenu("wardJummper").AddItem(
                new MenuItem("Wardjump", "Wardjump").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            menu.AddToMainMenu();
            Game.PrintChat("Vis's WardJumper loaded.");
        }

        private InventorySlot getWardSlot() {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot))
                return null;

            SpellDataInst instance = getItemSpell(slot);
            if (instance != default(SpellDataInst) && instance.State == SpellState.Ready)
                return slot;
            return null;
        }

        private SpellDataInst getItemSpell(InventorySlot invSlot) {
            return player.Spellbook.Spells.FirstOrDefault(spell => (int) spell.Slot == invSlot.Slot + 4);
        }

        private Spell getJumpSpell() {
            switch (ObjectManager.Player.ChampionName) {
                case "Jax":
                    return new Spell(SpellSlot.Q, 700);
                case "Katarina":
                    return new Spell(SpellSlot.E, 700);
                case "LeeSin":
                    return new Spell(SpellSlot.W, 700);
            }
            return null;
        }

        public bool isCompatibleChampion(Obj_AI_Hero hero) {
            return (hero.ChampionName == "Jax" || hero.ChampionName == "Katarina" || hero.ChampionName == "LeeSin");
        }


        private bool IsJumpReady() {
            if (ObjectManager.Player.ChampionName != "LeeSin") {
                return jumpSpell.IsReady();
            }
            return jumpSpell.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "BlindMonkWOne";
        }
    }
}