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
using System.Drawing;
using EloBuddy.SDK.Spells;
using SharpDX.Direct3D9;

namespace CoCassiopeia
{
    class Program
    {
        private static Spell.Skillshot Q;

        private static Spell.Skillshot W;

        private static Spell.Targeted E;

        private static Spell.Skillshot R;

        public static Menu FirstMenu,
            DrawMenu,
            ComboMenu,
            HarassMenu,
            LaneClearMenu,
            MiscMenu,
            KillStealMenu,
            JungleClearMenu,
            ActivatorMenu,
            LastHitMenu,
            SkinMenu;

        private float _lastECast = 0f;

        private float lastQCast = 0f;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += GameLoad;
        }

        public static bool IsChecked(Menu obj, string value)
        {
            return obj[value].Cast<CheckBox>().CurrentValue;
        }

         public static int GetSliderValue(Menu obj, string value)
        {
            return obj[value].Cast<Slider>().CurrentValue;
        }

        public static float QDamage(Obj_AI_Base target)
        {
            if (Q.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] {0f, 75f, 120f, 165f, 210f, 240f }[Q.Level] + 0.2333f*Player.Instance.TotalMagicalDamage);
            return 0f;
        }

        public static float WDamage(Obj_AI_Base target)
        {
            if (W.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] {0f, 20f, 35f, 50f, 65f, 80f}[W.Level] + 0.15f*Player.Instance.TotalMagicalDamage);
            return 0f;
        }

        public static float EDamage(Obj_AI_Base target)
        {
            if (E.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] {0f, 10f, 40f, 70f, 100f, 130f}[E.Level] +
                    0.35f*ObjectManager.Player.TotalMagicalDamage);
            return 0f;
        }

        public static float RDamage(Obj_AI_Base target)
        {
            if (R.IsReady())
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                    new[] {0f, 150f, 250f, 350f}[R.Level] + 0.5f*ObjectManager.Player.TotalMagicalDamage);
            return 0f;
        }

        private static void OnUpdate(EventArgs args)
        {

            if (Player.Instance.IsDead) return;


        }

        private static void InitEvents()
        {
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Orbwalker.OnUnkillableMinion += Orbwalker_OnUnkillableMinion;
            Game.OnUpdate += OnUpdate;
        }

        //Game Load

        private static void GameLoad(EventArgs args)
        {



            //Spells

            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, castDelay: 400, spellWidth: 75);

            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, spellWidth: 125);

            E = new Spell.Targeted(SpellSlot.E, 700);

            R = new Spell.Skillshot(SpellSlot.R, 825, SkillShotType.Cone, spellWidth: 80);
            //Initinazling Events

            InitEvents();

            //Drawings 

            EloBuddy.Drawing.OnDraw += Drawing;

            //Chat Print

            {
                if (Player.Instance.ChampionName != "Cassiopeia") return;
                Chat.Print("Welcome to CoCassiopeia, Go Break The Game.");
            }

            //Main Menu

            FirstMenu = MainMenu.AddMenu("CoCassiopeia", "cassiocoocle");
            FirstMenu.AddGroupLabel("CoCassiopeia");
            FirstMenu.AddLabel(
                "if there are any bugs please report to me on elobuddy or discord" +
                "Discord: Coocle#0510" +
                "Elobuddy: coocle");


            //Combo Menu

            ComboMenu = FirstMenu.AddSubMenu("Combo", "ComboCassC");
            ComboMenu.AddGroupLabel("Combo Menu");
            ComboMenu.Add("UseQCombo", new CheckBox("Use Q", true));
            ComboMenu.Add("UseWCombo", new CheckBox("Use W", true));
            ComboMenu.Add("UseECombo", new CheckBox("Use E", true));
            ComboMenu.Add("UseRCombo", new CheckBox("Use R", false));
            ComboMenu.Add("UseRCount", new Slider("How Many Enemies Needed to use R?", 1, 0, 5));
            ComboMenu.Add("UseFlashCombo", new CheckBox("Use Flash + R (if you want to use this disable Use R)", false));
            ComboMenu.Add("Priortize", new CheckBox("Priortize Enemies with Poison?", false));

            //Harass Menu

            HarassMenu = FirstMenu.AddSubMenu("Harass", "HarassCassC");
            HarassMenu.AddGroupLabel("Harass Menu");
            HarassMenu.Add("UseQHarass", new CheckBox("Use Q", true));
            HarassMenu.Add("UseEHarass", new CheckBox("Use E", true));
            HarassMenu.Add("ManaManagerHarass", new Slider("Mana Manager", 60, 0, 100));

            //Killsteal Menu

            KillStealMenu = FirstMenu.AddSubMenu("Killsteal", "KillstealCassC");
            KillStealMenu.Add("UseQKillSteal", new CheckBox("KS with Q", true));
            KillStealMenu.Add("UseEKillSteal", new CheckBox("KS with E", true));
            
            //LaneClear Menu

            LaneClearMenu = FirstMenu.AddSubMenu("Lane Clear", "LaneclearCassC");
            LaneClearMenu.AddGroupLabel("Lane Clear Menu");
            LaneClearMenu.Add("UseQLaneClear", new CheckBox("Use Q", true));
            LaneClearMenu.Add("UseWLaneClear", new CheckBox("Use W", false));
            LaneClearMenu.Add("UseELaneClear", new CheckBox("Use E", false));
            LaneClearMenu.Add("ManaManagerLaneClear", new Slider("Mana Manager", 50, 0, 100));

            //JungleClear Menu

            JungleClearMenu = FirstMenu.AddSubMenu("Jungle Clear", "JungleclearCassC");
            JungleClearMenu.AddGroupLabel("Jungle Clear Menu");
            JungleClearMenu.Add("UseQJungleClear", new CheckBox("Use Q", true));
            JungleClearMenu.Add("UseWJungleClear", new CheckBox("Use W", false));
            JungleClearMenu.Add("UseEJungleClear", new CheckBox("Use E", true));
            JungleClearMenu.Add("ManaManagerJungleClear", new Slider("Mana Manager", 40, 0, 100));
            
            //Last Hit Menu

            LastHitMenu = FirstMenu.AddSubMenu("Last Hit", "LasthitCassC");
            LastHitMenu.AddGroupLabel("Last Hit Menu");
            LastHitMenu.Add("UseQLastHit", new CheckBox("Use Q", false));
            LastHitMenu.Add("UseELastHit", new CheckBox("Use E", true));
            LastHitMenu.Add("ManaManagerLastHit", new Slider("Mana Manager", 40, 0, 100));

            //Drawing Menu

            DrawMenu = FirstMenu.AddSubMenu("Drawings", "DrawingsCassC");
            DrawMenu.AddGroupLabel("Drawings Menu");
            DrawMenu.Add("DrawQ", new CheckBox("Draw Q", true));
            DrawMenu.Add("DrawW", new CheckBox("Draw W", true));
            DrawMenu.Add("DrawE", new CheckBox("Draw E", true));
            DrawMenu.Add("DrawR", new CheckBox("Draw R", true));

            //Activator

            ActivatorMenu = FirstMenu.AddSubMenu("Activator", "ActivatorCassC");
            ActivatorMenu.Add("UseSeraphsEmbrace", new CheckBox("Use Seraph's Embrace?", true));
            ActivatorMenu.Add("UseZhonyas", new CheckBox("Use Zhonyas Hourglass?", true));
            ActivatorMenu.Add("UseHPPotion", new CheckBox("Use Health Pot?", true));

            //Skin Changer

            SkinMenu = FirstMenu.AddSubMenu("Skin Changer", "SkinChangerCassC");
            SkinMenu.AddGroupLabel("Skin Changer");
            SkinMenu.Add("SkinChangerActivate", new CheckBox("Use Skin Changer?", true));
            SkinMenu.Add("SkinChangerSlider", new Slider("Skin ID", 4, 0, 7));

            //Misc Menu

            MiscMenu = FirstMenu.AddSubMenu("Misc", "MiscCassC");
            MiscMenu.Add("GapcloserW", new CheckBox("Gapclose With W? (stop dashes)", true));
            MiscMenu.Add("DamageIndicator", new CheckBox("Damage Indicator (placeholder)", true));

        }

        //Drawing Things

        private static void Drawing(EventArgs args)
        {
            Drawing();
        }

        //Combo Method

        public void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range + 400, DamageType.Magical);

            if (target == null || !target.IsValidTarget(Q.Range)) return;

            var flash = Player.Spells.FirstOrDefault(a => a.SData.Name == "summonerflash");

            if (IsChecked(ComboMenu, "UseQCombo") && Q.IsReady() && target.IsValidTarget(Q.Range) && target.HasBuffOfType(BuffType.Poison))
            {
                var predictionQ = Q.GetPrediction(target);
                if (predictionQ.HitChancePercent >= 80)
                {
                    Q.Cast(predictionQ.CastPosition);
                    lastQCast = Game.Time;
                }
            }

            if (IsChecked(ComboMenu, "UseWCombo") && W.IsReady() && target.IsValidTarget(W.Range))
            {
                if (target.HasBuffOfType(BuffType.Poison) && (lastQCast - Game.Time) < -0.43f)
                {
                    var predictionW = W.GetPrediction(target);

                    if (predictionW.HitChancePercent >= 70)
                    {
                        W.Cast(predictionW.CastPosition);
                    }
                    else
                    {
                        if (predictionW.HitChancePercent >= 70)
                        {
                            W.Cast(predictionW.CastPosition);
                        }
                    }
                }
            }

            if (IsChecked(ComboMenu, "UseECombo") && E.IsReady() && target.IsValidTarget(E.Range) &&
                target.HasBuffOfType(BuffType.Poison) || IsChecked(ComboMenu, "Priotize") && E.CanCast(target))
            {
                E.Cast(target);
            }

            if (IsChecked(ComboMenu, "UseRCombo") && R.IsReady())
            {
                if (IsChecked(ComboMenu, "UseFlashCombo") &&
                    Damage.CalculateDamageOnUnit(Player.Instance, target, DamageType.Magical,
                        QDamage(target) + WDamage(target) + EDamage(target) + RDamage(target)) > target.Health &&
                    target.IsFacing(Player.Instance))
                {
                    Player.CastSpell(flash.Slot, target.Position);
                    Core.DelayAction(() => R.Cast(target), 250);
                }

                var countFacing =
                    EntityManager.Heroes.Enemies.Count(
                        t => t.IsValidTarget(R.Range) && target.IsFacing(Player.Instance) && Player.Instance.IsFacing(target));
                if (GetSliderValue(ComboMenu, "UseRCount") <= countFacing && ProbablyFacing(target) &&
                    target.IsValidTarget(R.Range - 50))
                {
                    R.Cast(target);
                }
            }
        }

        private static bool ProbablyFacing(Obj_AI_Base target)
        {
            var predictPos = Prediction.Position.PredictUnitPosition(target, 250);

            return predictPos.Distance(Player.Instance.ServerPosition) < target.ServerPosition.Distance(Player.Instance.ServerPosition);
        }


        //Harass Method

        private static void Harass()
        {
            if (Player.Instance.Mana <= HarassMenu["ManaMangerHarass"].Cast<Slider>().CurrentValue) return;
            if (HarassMenu["UseQHarass"].Cast<CheckBox>().CurrentValue && Q.IsReady())
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

            if (HarassMenu["UseEHarass"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (target != null && target.IsValidTarget())
                {
                    E.Cast(target);
                }
            }
        }

        //Killsteal Method

        private static void KillSteal()
        {
            if (KillStealMenu["UseQKillSteal"].Cast<CheckBox>().CurrentValue && Q.IsReady())
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

            if (KillStealMenu["UseEKillSteal"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                var target = TargetSelector.GetTarget(EntityManager.Heroes.Enemies.Where(t => t != null
                                                                                              && t.IsValidTarget()
                                                                                              &&
                                                                                              t.HasBuff(
                                                                                                  "cassiopeiaqdebuff")
                                                                                              && E.IsInRange(t)
                                                                                              && t.Health <= EDamage(t)),
                    DamageType.Magical);

                if (target != null)
                {
                   E.Cast();
                }
            }
        }

        //Jungle Clear Method


        public static void JungleClear()
        {
            var monster = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsMonster && x.IsValidTarget(Q.Range)).OrderBy(x => x.Health).LastOrDefault();
            if (monster == null || !monster.IsValid) return;
            if (Orbwalker.IsAutoAttacking) return;
            Orbwalker.ForcedTarget = null;
            if (JungleClearMenu["UseQJungleClear"].Cast<CheckBox>().CurrentValue
                && Player.Instance.Mana > JungleClearMenu["ManaManagerJungleClear"].Cast<Slider>().CurrentValue
                && Q.IsReady())
            {
                Q.Cast(monster);
            }

            var wmonster = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsMonster && x.IsValidTarget(W.Range)).OrderBy(x => x.Health).LastOrDefault();
            if (wmonster == null || !wmonster.IsValid) return;
            if (Orbwalker.IsAutoAttacking) return;
            Orbwalker.ForcedTarget = null;
            if (JungleClearMenu["UseWJungleClear"].Cast<CheckBox>().CurrentValue
                && Player.Instance.ManaPercent > JungleClearMenu["ManaManagerJungleClear"].Cast<Slider>().CurrentValue
                && wmonster.HasBuff("cassiopeiaqdebuff"))
            {
                if (Player.Instance.Mana > JungleClearMenu["ManaManagerJungleClear"].Cast<Slider>().CurrentValue)
                {
                    W.Cast();
                }
            }

            if (JungleClearMenu["UseEJungleClear"].Cast<CheckBox>().CurrentValue
               && Player.Instance.Mana > JungleClearMenu["ManaManagerJungleClear"].Cast<Slider>().CurrentValue)
            {
                if (E.IsReady())
                {
                    E.Cast();
                }
            }

        }


        //Laneclear Method

        private static void LaneClear()
        {
            if (Player.Instance.Mana <= HarassMenu["ManaMangerLaneClear"].Cast<Slider>().CurrentValue) return;
            if (LaneClearMenu["UseQLaneClear"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                var minion = Orbwalker.LaneClearMinionsList.FirstOrDefault();
                if (minion != null) return;
                if (minion != null && minion.IsValidTarget())
                {
                    var pred = Q.GetPrediction(minion);
                    if (pred.CollisionObjects.Count() == 0)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }

            if (LaneClearMenu["UseWLaneClear"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                var minion = Orbwalker.LaneClearMinionsList.FirstOrDefault();
                if (minion != null) return;
                if (minion != null && minion.IsValidTarget())
                {
                    var pred = W.GetPrediction(minion);
                    if (pred.CollisionObjects.Count() == 0)
                    {
                        W.Cast(pred.CastPosition);
                    }
                }

            }

            if (LaneClearMenu["UseELaneClear"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                var minion = Orbwalker.LaneClearMinionsList.FirstOrDefault();
                if (minion != null) return;
                if (minion != null && minion.IsValidTarget())
                {
                    E.Cast(minion);
                }
            }
        }

        //Last Hit Method

        public static void Orbwalker_OnUnkillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            if (Player.Instance.Mana <= LastHitMenu["ManaMangerLastHit"].Cast<Slider>().CurrentValue) return;
            if (Q.IsReady() && LastHitMenu["UseQLastHit"].Cast<CheckBox>().CurrentValue &&
                Orbwalker.LastHitMinion.IsValid &&
                Orbwalker.LastHitMinion.Health <= Q.GetSpellDamage(Orbwalker.LastHitMinion))
            {
                Q.Cast(Orbwalker.LastHitMinion);
            }
        }

        //Activator Method


        //Draw Method

        private static void Drawing()
        {
            if (DrawMenu["DrawQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                Circle.Draw(Q.IsLearned ? Color.ForestGreen : Color.Zero, Q.Range, Player.Instance.Position);
            }

            if (DrawMenu["DrawW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                Circle.Draw(W.IsLearned? Color.Purple : Color.Zero, W.Range, Player.Instance.Position);
            }

            if (DrawMenu["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                Circle.Draw(E.IsLearned? Color.DarkBlue : Color.Zero, E.Range, Player.Instance.Position);
            }

            if (DrawMenu["DrawR"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                Circle.Draw(R.IsLearned? Color.Orange : Color.Zero, R.Range, Player.Instance.Position);
            }
        }

        //Skin Changer Method

        private static void SkinChanger()
        {
            if (SkinMenu["SkinChangerActivate"].Cast<CheckBox>().CurrentValue) Player.SetSkinId(SkinMenu["SkinManager"].Cast<Slider>().CurrentValue);
            SkinMenu["SkinChangerSlider"].Cast<Slider>().OnValueChange += (sender, vargs) =>
            {
                if (SkinMenu["SkinChangerActivate"].Cast<CheckBox>().CurrentValue) Player.SetSkinId(vargs.NewValue);
            };
            SkinMenu["SkinChangerActivate"].Cast<CheckBox>().OnValueChange += (sender, vargs) =>
            {
                if (vargs.NewValue)
                    Player.SetSkinId(SkinMenu["SkinChangerSlider"].Cast<Slider>().CurrentValue);
                else
                    Player.SetSkinId(0);
            };
        }

        //Misc Method

        public static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs args)
        {
            if (W.IsReady()
                && sender != null
                && sender.IsEnemy
                && sender.IsValid
                && sender.IsDashing()
                && (sender.IsAttackingPlayer || Player.Instance.Distance(args.End) < 800)
                && MiscMenu["GapcloserW"].Cast<CheckBox>().CurrentValue)
            {
                W.Cast(sender);
            }

        }
    }
}
