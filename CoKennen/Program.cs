using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

//TODO: Damage Calculator, Jungle Clear.

namespace CoKennen
{
    internal class Program
    {
        private static Spell.Skillshot Q;

        private static Spell.Active W;

        private static Spell.Active E;

        private static Spell.Active R;

        public static Menu FirstMenu, DrawMenu, ComboMenu, HarassMenu, LaneClearMenu, MiscMenu, KillStealMenu;

        private static AIHeroClient me => Player.Instance;

        //Loading Function

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += GameLoad;
        }

        //Damages

        public static float QDamage(Obj_AI_Base target)
        {
            if (Q.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] { 0f, 75f, 115f, 155f, 195f, 235f }[Q.Level] + 0.75f * Player.Instance.TotalMagicalDamage);
            return 0f;
        }

        public static float WDamage(Obj_AI_Base target)
        {
            if (W.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] { 0f, 65f, 95f, 125f, 155f, 185f }[W.Level] + 0.55f * Player.Instance.TotalMagicalDamage);
            return 0f;
        }

        public static float EDamage(Obj_AI_Base target)
        {
            if (E.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] { 0f, 42.5f, 62.5f, 82.5f, 102.5f, 122.5f }[E.Level] +
                    0.3f * ObjectManager.Player.TotalMagicalDamage);
            return 0f;
        }

        public static float RDamage(Obj_AI_Base target)
        {
            if (R.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] { 0f, 80f, 145f, 210f }[R.Level] + 0.4f * ObjectManager.Player.TotalMagicalDamage);
            return 0f;
        }

        //GameLoad

        private static void GameLoad(EventArgs args)
        {
            //Spells

            Q = new Spell.Skillshot(SpellSlot.Q, 1050, SkillShotType.Linear, 250, 1650, 50)
            { AllowedCollisionCount = 1 };

            W = new Spell.Active(SpellSlot.W, 750, DamageType.Magical);

            E = new Spell.Active(SpellSlot.E, uint.MaxValue, DamageType.Magical);

            R = new Spell.Active(SpellSlot.R, 550, DamageType.Magical);


            //Drawing Exection

            EloBuddy.Drawing.OnDraw += Drawing;

            //Game Tick Counter

            Game.OnTick += OnTick;

            //Chat Print

            {
                if (me.ChampionName != "Kennen") return;
                Chat.Print("Welcome to CoKennen, Go Break The Game.");
            }

            //Main Menu

            FirstMenu = MainMenu.AddMenu("CoKennen", "coocle");
            FirstMenu.AddGroupLabel("CoKennen");
            FirstMenu.AddLabel(
                "this is my first addon, if there are any bugs please report to me on elobuddy or discord" +
                "Discord: Coocle#0510" +
                "Elobuddy: coocle");

            // Combo Menu (Finished)

            ComboMenu = FirstMenu.AddSubMenu("Combo", "combomenuid8913");

            ComboMenu.AddGroupLabel("Combo Menu");

            ComboMenu.Add("Q", new CheckBox("Use Q", true));

            ComboMenu.Add("proitize", new CheckBox("Proitize Enemy Champion that has the " +
                                                   "Passive Buff"));

            ComboMenu.Add("W", new CheckBox("Use W", true));

            ComboMenu.Add("SliderWCombo", new Slider("How Many Enemy's in Range to Hit??", 2, 1, 5));

            ComboMenu.Add("E", new CheckBox("Use E", true));

            ComboMenu.Add("R", new CheckBox("Use R", false));

            ComboMenu.Add("UseFlashInCombo", new Slider("Use Flash", 3, 1, 5));

            //Harass Menu (Finished)

            HarassMenu = FirstMenu.AddSubMenu("Harass", "HarassmenuId7941");

            HarassMenu.AddGroupLabel("Harass Menu");

            HarassMenu.Add("harassQ", new CheckBox("Use Q", true));

            HarassMenu.Add("harassW", new CheckBox("Use W", false));

            HarassMenu.Add("WEnemyCount", new Slider("Use W if there are {0}", 1, 1, 5));

            HarassMenu.Add("HarassEnergyManager", new Slider("Stop using harass if energy below {0}", 100, 0, 200));

            //Laneclear Menu (finished)

            LaneClearMenu = FirstMenu.AddSubMenu("Lane Clear", "LaneClear");

            LaneClearMenu.AddGroupLabel("Lane Clear Menu");

            LaneClearMenu.Add("LaneClearQ", new CheckBox("Use Q", true));

            LaneClearMenu.Add("LaneClearW", new CheckBox("Use W", true));

            LaneClearMenu.Add("EnergyManger", new Slider("Stop using skills for at {0}", 100, 0, 200));

            LaneClearMenu.Add("LaneClearE", new CheckBox("Use E", false));

            LaneClearMenu.Add("LaneClearR", new CheckBox("Use R", false));

            LaneClearMenu.Add("LaneClearCountManager", new Slider("Use R if minions Exceed {0}", 15, 0, 40));

            //Killsteal Menu

            KillStealMenu = FirstMenu.AddSubMenu("Kill Steal", "KillSteal");

            KillStealMenu.AddGroupLabel("Kill Steal Menu");

            KillStealMenu.Add("KillStealQ", new CheckBox("Use Q to killsteal?", true));

            KillStealMenu.Add("KillStealW", new CheckBox("Use W to killsteal?", true));

            //Draw Menu (Finished)

            DrawMenu = FirstMenu.AddSubMenu("Drawings", "drawmenuid412341");

            DrawMenu.AddGroupLabel("Ranges");

            DrawMenu.Add("drawQrange", new CheckBox("Draw Q", true));

            DrawMenu.Add("drawWrange", new CheckBox("Draw W", false));

            DrawMenu.Add("drawRrange", new CheckBox("Draw R", true));

            //Misc (Finished)

            MiscMenu = FirstMenu.AddSubMenu("Misc", "Miscmenuid9834");

            MiscMenu.AddGroupLabel("Misc Menu");

            MiscMenu.Add("MiscGapcloser", new CheckBox("Use E as gapclose?", true));

            MiscMenu.Add("LasthitMisc", new CheckBox(" Use Q to last hit?", true));

            MiscMenu.Add("LashitMiscW", new CheckBox("Use W to last hit?", false));
        }

        //Drawing Call Method

        private static void Drawing(EventArgs args)
        {
            Drawing();
        }

        //Game Tick Function

        private static void OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
        }

        //Combo Function

        private static void Combo()
        {
            var mark =
                EntityManager.Heroes.Enemies.Where(x => x.IsValid && Q.IsInRange(x) && x.HasBuff("kennenmarkofstorm"));

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            var focus = ComboMenu["proitize"].Cast<CheckBox>().CurrentValue
                ? mark != null ? mark.OrderBy(x => TargetSelector.GetPriority(x)).LastOrDefault() : null
                : null;

            if (target == null)
            {
                return;
            }

            //Q Cast

            if (ComboMenu["Q"].Cast<CheckBox>().CurrentValue)
            {
                var Qpred = Q.GetPrediction(target);

                if (target.IsValidTarget(Q.Range) && Q.IsReady() && Qpred.HitChance >= HitChance.High && focus == null)
                {
                    Q.Cast(target);
                }

                if (focus != null)
                {
                    var QMarkpred = Q.GetPrediction(focus);
                    if (Qpred.CollisionObjects.Count() == 0)
                    {
                        Q.Cast(Qpred.CastPosition);
                    }
                }


                //W Cast

                if (ComboMenu["W"].Cast<CheckBox>().CurrentValue && W.IsReady())
                {
                    var Target =
                        EntityManager.Heroes.Enemies.Count(
                            x => x.IsValid && W.IsInRange(x) && x.HasBuff("kennenmarkofstorm"));

                    if (Target >= ComboMenu["SliderWCombo"].Cast<Slider>().CurrentValue)
                    {
                        W.Cast();
                    }
                }

                //E Cast

                if (ComboMenu["E"].Cast<CheckBox>().CurrentValue)
                {
                    if (target.IsValidTarget(E.Range) && E.IsReady())
                    {
                        E.Cast();
                    }
                }

                //R Cast

                if (ComboMenu["R"].Cast<CheckBox>().CurrentValue)
                {
                    var Count = EntityManager.Heroes.Enemies.Count(x => x.IsValid && R.IsInRange(x));
                    if (Count >= ComboMenu["UseFlashInCombo"].Cast<Slider>().CurrentValue)
                    {
                        R.Cast();
                    }
                }
            }
        }

        //Harass Function

        private static void Harass()
        {
            if (me.Mana <= LaneClearMenu["HarassEnergyManager"].Cast<Slider>().CurrentValue) return;
            if (HarassMenu["harassQ"].Cast<CheckBox>().CurrentValue)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (target != null && target.IsValidTarget())
                {
                    var pred = Q.GetPrediction(target);
                    if (pred.CollisionObjects.Count() == 0)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }
            if (W.IsReady())
            {
                var Target =
                    EntityManager.Heroes.Enemies.Count(
                        x => x.IsValid && W.IsInRange(x) && x.HasBuff("kennenmarkofstorm"));
                if (Target >= HarassMenu["WEnemyCount"].Cast<Slider>().CurrentValue)
                {
                    W.Cast();
                }
            }
        }

        //LaneClear Function 

        private static void LaneClear()
        {
            var minion = Orbwalker.LaneClearMinionsList.FirstOrDefault();
            if (minion != null) return;
            if (LaneClearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue &&
                me.Mana > LaneClearMenu["EnergyManager"].Cast<Slider>().CurrentValue && Q.IsReady())
            {
                Q.Cast(minion);
            }

            if (LaneClearMenu["LaneClearW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                W.Cast();
            }

            if (LaneClearMenu["LaneClearE"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                E.Cast();
            }

            var rminions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, me.Position,
                R.Range);
            if (LaneClearMenu["LaneClearR"].Cast<CheckBox>().CurrentValue && R.IsReady() &&
                rminions.Count() >= LaneClearMenu["LaneClearCountManager"].Cast<Slider>().CurrentValue
                && R.IsReady())
            {
                R.Cast();
            }
        }

        //Misc Function

        public static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (target == null || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;
            if (args.RemainingHealth <= QDamage(target) && MiscMenu["LasthitMisc"].Cast<CheckBox>().CurrentValue)
            {
                Q.Cast(target);
            }

            if (args.RemainingHealth <= WDamage(target) && MiscMenu["LasthitMiscW"].Cast<CheckBox>().CurrentValue)
            {
                W.Cast();
            }
        }

        public static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs args)
        {
            if (E.IsReady()
                && sender != null
                && sender.IsEnemy
                && sender.IsValid
                && (sender.IsAttackingPlayer || Player.Instance.Distance(args.End) < 100)
                && MiscMenu["MiscGapcloser"].Cast<CheckBox>().CurrentValue)
            {
                E.Cast(sender);
            }
        }


        //KillSteal Function

        private static void KillSteal()
        {
            if (KillStealMenu["KillStealQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                var target =
                    TargetSelector.GetTarget(EntityManager.Heroes.Enemies.Where(t => t != null && t.IsValidTarget()
                                                                                     && Q.IsInRange(t)
                                                                                     && t.Health <= QDamage(t)),
                        DamageType.Magical);

                if (target != null)
                {
                    var pred = Q.GetPrediction(target);
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }
            if (KillStealMenu["KillStealW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                var target = TargetSelector.GetTarget(EntityManager.Heroes.Enemies.Where(t => t != null
                                                                                              && t.IsValidTarget()
                                                                                              &&
                                                                                              t.HasBuff(
                                                                                                  "kennenmarkofstorm")
                                                                                              && W.IsInRange(t)
                                                                                              && t.Health <= WDamage(t)),
                    DamageType.Magical);

                if (target != null)
                {
                    W.Cast();
                }
            }
        }

        //Drawing Function/Method

        private static void Drawing()
        {
            if (DrawMenu["drawQrange"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(Q.IsLearned ? Color.Aquamarine : Color.Zero, Q.Range, me.Position);
            }

            if (DrawMenu["drawWrange"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(W.IsLearned ? Color.Green : Color.Zero, W.Range, me.Position);
            }

            if (DrawMenu["drawRrange"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(R.IsLearned ? Color.AliceBlue : Color.Zero, R.Range, me.Position);
            }
        }
    }
}