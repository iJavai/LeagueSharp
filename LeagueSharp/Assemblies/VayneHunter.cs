using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Assemblies {
    internal class VayneHunter : Champion {
        public static string[] interrupt;
        public static string[] notarget;
        public static string[] gapcloser;
        public static Obj_AI_Hero tar;
        public static Dictionary<string, SpellSlot> spellData;
        public static Dictionary<Obj_AI_Hero, Vector3> dirDic, lastVecDic = new Dictionary<Obj_AI_Hero, Vector3>();
        public static Dictionary<Obj_AI_Hero, float> angleDic = new Dictionary<Obj_AI_Hero, float>();
        public static Vector3 currentVec, lastVec;
        public static bool sol = false;

        public VayneHunter() {
            loadMenu();
            loadSpells();

            Game.OnGameUpdate += OnTick;
            Orbwalking.AfterAttack += OW_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker1"));
            menu.AddSubMenu(new Menu("[Hunter] - Combo", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            menu.AddSubMenu(new Menu("[Hunter] - Mixed Mode", "Harrass"));
            menu.SubMenu("Harrass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            //menu.SubMenu("Harrass").AddItem(new MenuItem("UseEH", "Use E").SetValue(true));
            //menu.SubMenu("Harrass").AddItem(new MenuItem("UseQPH", "Use Q&Auto While they auto minions").SetValue(true));
            menu.AddSubMenu(new Menu("[Hunter] - Misc", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("AntiGP", "Use AntiGapcloser").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Interrupt Spells").SetValue(true));
            menu.SubMenu("Misc").AddItem(
                new MenuItem("ENextAuto", "Use E after next AA").SetValue(new KeyBind("T".ToCharArray()[0],
                    KeyBindType.Toggle)));
            menu.SubMenu("Misc").AddItem(new MenuItem("AdvE", "Use AdvE logic").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("SmartQ", "WIP Use Q for GapClose").SetValue(false));
            menu.SubMenu("Misc").AddItem(new MenuItem("UsePK", "Use Packets").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("AutoE", "Use Auto E (Lag)").SetValue(false));
            menu.SubMenu("Misc").AddItem(new MenuItem("PushDistance", "E Push Dist").SetValue(new Slider(425, 400, 475)));
            menu.AddSubMenu(new Menu("[Hunter] - Items", "Items"));
            menu.SubMenu("Items").AddItem(new MenuItem("Botrk", "Use BOTRK").SetValue(true));
            menu.SubMenu("Items").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
            menu.SubMenu("Items").AddItem(
                new MenuItem("OwnHPercBotrk", "Min Own H % Botrk").SetValue(new Slider(50, 1, 100)));
            menu.SubMenu("Items").AddItem(
                new MenuItem("EnHPercBotrk", "Min Enemy H % Botrk").SetValue(new Slider(20, 1, 100)));
            menu.SubMenu("Items").AddItem(new MenuItem("ItInMix", "Use Items In Mixed Mode").SetValue(false));
            menu.AddSubMenu(new Menu("[Hunter] - Mana Mng", "ManaMan"));
            menu.SubMenu("ManaMan").AddItem(
                new MenuItem("QManaC", "Min Q Mana in Combo").SetValue(new Slider(30, 1, 100)));
            menu.SubMenu("ManaMan").AddItem(
                new MenuItem("QManaM", "Min Q Mana in Mixed").SetValue(new Slider(30, 1, 100)));
            menu.SubMenu("ManaMan").AddItem(
                new MenuItem("EManaC", "Min E Mana in Combo").SetValue(new Slider(20, 1, 100)));
            menu.SubMenu("ManaMan").AddItem(
                new MenuItem("EManaM", "Min E Mana in Mixed").SetValue(new Slider(20, 1, 100)));
            //menu.AddSubMenu(new Menu("[Hunter]WIP", "ezCondemn"));
            // menu.SubMenu("ezCondemn").AddItem(new MenuItem("CheckDistance", "Condemn check Distance").SetValue(new Slider(25, 1, 200)));
            //menu.SubMenu("ezCondemn").AddItem(new MenuItem("Checks", "Num of Checks").SetValue(new Slider(3, 0, 5)));
            //menu.SubMenu("ezCondemn").AddItem(new MenuItem("MaxDistance", "Max Condemn Distance").SetValue(new Slider(1000, 0, 1500)));
            //Thank you blm95 ;)
            menu.AddSubMenu(new Menu("[Hunter]Condemn: ", "CondemnHero"));
            menu.AddSubMenu(new Menu("[Hunter]Gapcloser", "gap"));
            menu.AddSubMenu(new Menu("[Hunter]Gapcloser 2", "gap2"));
            menu.AddSubMenu(new Menu("[Hunter]Interrupts", "int"));
            GPIntmenuCreate();
            NoCondemnMenuCreate();
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 0f);
            E = new Spell(SpellSlot.E, 550f);
            R = new Spell(SpellSlot.R, 0f);
            E.SetTargetted(0.25f, 2200f);
        }

        private void OnTick(EventArgs args) {
            if (isEn("AutoE")) {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy)) {
                    if (hero.IsValid && !hero.IsDead && hero.IsVisible && player.Distance(hero) < 715f &&
                        player.Distance(hero) > 0f && menu.Item(hero.BaseSkinName).GetValue<bool>()) {
                        PredictionOutput pred = E.GetPrediction(hero);
                        int pushDist = menu.Item("PushDistance").GetValue<Slider>().Value;
                        for (int i = 0; i < pushDist; i += (int) hero.BoundingRadius) {
                            if (
                                IsWall(
                                    pred.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D
                                        ())) {
                                CastE(hero);
                                break;
                            }
                        }
                    }
                }
            }
            if (!isMode("Combo") || !isEn("UseE") || !E.IsReady()) {
                return;
            }
            if (!isEn("AdvE")) {
                foreach (
                    Obj_AI_Hero hero in
                        from hero in
                            ObjectManager.Get<Obj_AI_Hero>().Where(
                                hero => hero.IsValidTarget(550f) && menu.Item(hero.BaseSkinName).GetValue<bool>())
                        let prediction = E.GetPrediction(hero)
                        where NavMesh.GetCollisionFlags(
                            prediction.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(),
                                -menu.Item("PushDistance").GetValue<Slider>().Value).To3D())
                            .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                                prediction.UnitPosition.To2D()
                                    .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                        -(menu.Item("PushDistance").GetValue<Slider>().Value/2))
                                    .To3D())
                                .HasFlag(CollisionFlags.Wall)
                        select hero) {
                    CastE(hero);
                }
            }
            else {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy)) {
                    if (hero.IsValid && !hero.IsDead && hero.IsVisible && player.Distance(hero) < 715f &&
                        player.Distance(hero) > 0f && menu.Item(hero.BaseSkinName).GetValue<bool>()) {
                        PredictionOutput pred = E.GetPrediction(hero);
                        int pushDist = menu.Item("PushDistance").GetValue<Slider>().Value;
                        for (int i = 0; i < pushDist; i += (int) hero.BoundingRadius) {
                            if (
                                IsWall(
                                    pred.UnitPosition.To2D().Extend(ObjectManager.Player.ServerPosition.To2D(), -i).To3D
                                        ())) {
                                CastE(hero);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {
            String spellName = args.SData.Name;
            //Interrupts
            if (isEn(spellName) && sender.IsValidTarget(550f) && isEn("Interrupt")) {
                CastE((Obj_AI_Hero) sender, true);
            }
            //Targeted GapClosers
            if (isEn(spellName) && sender.IsValidTarget(550f) && isEn("AntiGP") &&
                gapcloser.Any(str => str.Contains(args.SData.Name))
                && args.Target.IsMe) {
                CastE((Obj_AI_Hero) sender, true);
            }
            //NonTargeted GP
            if (isEn(spellName) && sender.IsValidTarget(550f) && isEn("AntiGP") &&
                notarget.Any(str => str.Contains(args.SData.Name))
                && player.Distance(args.End) <= 320f) {
                CastE((Obj_AI_Hero) sender, true);
            }
        }

        private void OW_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target) {
            if (unit.IsMe) {
                var targ = (Obj_AI_Hero) target;
                if (isEnK("ENextAuto")) {
                    CastE(targ);
                    menu.Item("ENextAuto").SetValue(new KeyBind("E".ToCharArray()[0], KeyBindType.Toggle));
                }
                if (isEn("UseQ") && isMode("Combo")) {
                    if (isEn("UseR")) {
                        R.Cast();
                    }
                    CastQ(targ);
                }
                if (isEn("UseQH") && isMode("Mixed")) {
                    CastQ(targ);
                }
                if (isMode("Combo")) {
                    useItems(targ);
                }
                if (isMode("Mixed") && isEn("ItInMix")) {
                    useItems(targ);
                }
            }
        }

        private void CastQ(Obj_AI_Hero targ) {
            if (Q.IsReady()) {
                if (isEn("SmartQ") && player.Distance(targ) >= Orbwalking.GetRealAutoAttackRange(null)) {
                    if (isMode("Combo") && getPercentValue(player, true) >= menu.Item("QManaC").GetValue<Slider>().Value) {
                        const float tumbleRange = 300f;
                        bool canGapclose = player.Distance(targ) <=
                                           Orbwalking.GetRealAutoAttackRange(null) + tumbleRange;
                        if ((player.Distance(targ) >= Orbwalking.GetRealAutoAttackRange(null))) {
                            if (canGapclose) {
                                var PositionForQ = new Vector3(targ.Position.X, targ.Position.Y, targ.Position.Z);
                                Q.Cast(PositionForQ, isEn("UsePK"));
                            }
                        }
                    }
                    else if (isMode("Mixed") &&
                             getPercentValue(player, true) >= menu.Item("QManaM").GetValue<Slider>().Value) {
                        const float tumbleRange = 300f;
                        bool canGapclose = player.Distance(targ) <=
                                           Orbwalking.GetRealAutoAttackRange(null) + tumbleRange;
                        if ((player.Distance(targ) >= Orbwalking.GetRealAutoAttackRange(null))) {
                            if (canGapclose) {
                                var PositionForQ = new Vector3(targ.Position.X, targ.Position.Y, targ.Position.Z);
                                Q.Cast(PositionForQ, isEn("UsePK"));
                            }
                        }
                    }
                }
                else {
                    if (isMode("Combo") && getPercentValue(player, true) >= menu.Item("QManaC").GetValue<Slider>().Value) {
                        Q.Cast(Game.CursorPos, isEn("UsePK"));
                    }
                    else if (isMode("Mixed") &&
                             getPercentValue(player, true) >= menu.Item("QManaM").GetValue<Slider>().Value) {
                        Q.Cast(Game.CursorPos, isEn("UsePK"));
                    }
                }
            }
        }

        private void CastE(Obj_AI_Hero Target, bool forGp = false) {
            if (E.IsReady() && player.Distance(Target) < 550f) {
                if (!forGp) {
                    if (isMode("Combo") && getPercentValue(player, true) >= menu.Item("EManaC").GetValue<Slider>().Value) {
                        E.Cast(Target, isEn("UsePK"));
                    }
                    else if (isMode("Mixed") &&
                             getPercentValue(player, true) >= menu.Item("EManaM").GetValue<Slider>().Value) {
                        E.Cast(Target, isEn("UsePK"));
                    }
                }
                else {
                    E.Cast(Target, isEn("UsePK"));
                }
            }
        }

        private bool IsWall(Vector3 position) {
            CollisionFlags cFlags = NavMesh.GetCollisionFlags(position);
            return (cFlags == CollisionFlags.Wall || cFlags == CollisionFlags.Building || cFlags == CollisionFlags.Prop);
        }

        private void UpdateHeroes() {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(550f))) {
                currentVec = hero.Position;
                Vector3 direction = Vector3.Subtract(currentVec, lastVec);
                if (!(direction == new Vector3(0, 0, 0))) {
                    direction.Normalize();
                }
                float angle = Vector3.Dot(direction, direction);
                lastVecDic[hero] = currentVec;
                dirDic[hero] = direction;
                angleDic[hero] = angle;
            }
        }

        private Vector3 condemnCollisionTime(Obj_AI_Hero target) {
            Vector3 dir = dirDic[target];
            float angle = angleDic[target];
            if (!(dir == new Vector3(0, 0, 0))) {
                Vector3 windup = target.Position + dir*(target.MoveSpeed*250/1000);
                var time = (float) GetCollisionTime(windup, dir, target.MoveSpeed, player.Position, 1600f);
                if (Math.Abs(time) < 1) {
                    return new Vector3(0, 0, 0);
                }
                Vector3 returner = target.Position + dir*(target.MoveSpeed*(time + 0.25f))/2;
                return returner;
            }
            return new Vector3(0, 0, 0);
        }

        //Thanks Yomie
        private double GetCollisionTime(Vector3 position, Vector3 direction, float tSpeed, Vector3 sourcePos,
            float projSpeed) {
            Vector3 velocity = direction*tSpeed;
            float velocityX = velocity.X;
            float velocityY = velocity.Z;
            Vector3 relStart = position - sourcePos;
            float relStartX = relStart.X;
            float relStartY = relStart.Z;
            float a = velocityX*velocityX + velocityY*velocityY - projSpeed*projSpeed;
            float b = 2*velocityX*relStartX + 2*velocityY*relStartY;
            float c = relStartX*relStartX + relStartY*relStartY;
            float disc = b*b - 4*a*c;
            if (disc >= 0) {
                double t1 = -(b + Math.Sqrt(disc))/(2*a);
                double t2 = -(b - Math.Sqrt(disc))/(2*a);
                if (t1 != null && t2 != null && t1 > 0 && t2 > 0) {
                    if (t1 > t2) {
                        return t2;
                    }
                    return t1;
                }
                if (t1 != null && t1 > 0) {
                    return t1;
                }
                if (t2 != null && t2 > 0) {
                    return t2;
                }
            }
            return 0;
        }

        private void GPIntmenuCreate() {
            gapcloser = new[] {
                "AkaliShadowDance", "Headbutt", "DianaTeleport", "IreliaGatotsu", "JaxLeapStrike", "JayceToTheSkies",
                "MaokaiUnstableGrowth", "MonkeyKingNimbus", "Pantheon_LeapBash", "PoppyHeroicCharge", "QuinnE",
                "XenZhaoSweep", "blindmonkqtwo", "FizzPiercingStrike", "RengarLeap"
            };
            notarget = new[] {
                "AatroxQ", "GragasE", "GravesMove", "HecarimUlt", "JarvanIVDragonStrike", "JarvanIVCataclysm", "KhazixE",
                "khazixelong", "LeblancSlide", "LeblancSlideM", "LeonaZenithBlade", "UFSlash", "RenektonSliceAndDice",
                "SejuaniArcticAssault", "ShenShadowDash", "RocketJump", "slashCast"
            };
            interrupt = new[] {
                "KatarinaR", "GalioIdolOfDurand", "Crowstorm", "Drain", "AbsoluteZero", "ShenStandUnited", "UrgotSwap2",
                "AlZaharNetherGrasp", "FallenOne", "Pantheon_GrandSkyfall_Jump", "VarusQ", "CaitlynAceintheHole",
                "MissFortuneBulletTime", "InfiniteDuress", "LucianR"
            };
            foreach (string t in gapcloser) {
                menu.SubMenu("gap").AddItem(new MenuItem(t, t)).SetValue(true);
            }
            foreach (string t in notarget) {
                menu.SubMenu("gap2").AddItem(new MenuItem(t, t)).SetValue(true);
            }
            foreach (string t in interrupt) {
                menu.SubMenu("int").AddItem(new MenuItem(t, t)).SetValue(true);
            }
        }

        private void NoCondemnMenuCreate() {
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy)) {
                menu.SubMenu("CondemnHero").AddItem(new MenuItem(hero.BaseSkinName, hero.BaseSkinName)).SetValue(true);
            }
        }

        private void useItems(Obj_AI_Hero target) {
            float currentHealth = getPercentValue(player, false);
            if (menu.Item("Botrk").GetValue<bool>() &&
                (menu.Item("OwnHPercBotrk").GetValue<Slider>().Value <= currentHealth) &&
                ((menu.Item("EnHPercBotrk").GetValue<Slider>().Value <= getEnH(target)))) {
                useItem(3153, target);
            }
            if (menu.Item("Youmuu").GetValue<bool>()) {
                useItem(3142);
            }
        }

        private bool isEn(String opt) {
            return menu.Item(opt).GetValue<bool>();
        }

        private bool isEnK(String opt) {
            return menu.Item(opt).GetValue<KeyBind>().Active;
        }

        private bool isMode(String mode) {
            return (orbwalker.ActiveMode.ToString() == mode);
        }

        private void useItem(int id, Obj_AI_Hero target = null) {
            if (Items.HasItem(id) && Items.CanUseItem(id)) {
                Items.UseItem(id, target);
            }
        }

        private float getEnH(Obj_AI_Hero target) {
            float h = (target.Health/target.MaxHealth)*100;
            return h;
        }
    }
}