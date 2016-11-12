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
            JungleClearMenu,
            ActivatorMenu,
            LastHitMenu,
            SkinMenu;

        private static int skinId = 1;

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
            Game.OnUpdate += OnUpdate;
        }

        public void InitVariables()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Circular, castDelay: 400, spellWidth: 75);
            W = new Spell.Skillshot(SpellSlot.W, 850, SkillShotType.Circular, spellWidth: 125);
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Skillshot(SpellSlot.R, 825, SkillShotType.Cone, spellWidth: 80);
            

            Orbwalker.OnPostAttack += OnAfterAttack;
            Gapcloser.OnGapcloser += OnGapCloser;
            Interrupter.OnInterruptableSpell += OnPossibleToInterrupt;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

            Game.OnUpdate += OnGameUpdate;
            EloBuddy.Drawing.OnDraw += Drawing;
        }

        //Game Load

        private static void GameLoad(EventArgs args)
        {
            //Initinazling Events

            InitEvents();

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
            ComboMenu.Add("DisableAA", new CheckBox("Disable AA While using Combo?", true));
            ComboMenu.Add("UseWIfCantQ", new CheckBox("Use W if Q can not land", false));

            //Harass Menu

            HarassMenu = FirstMenu.AddSubMenu("Harass", "HarassCassC");
            HarassMenu.AddGroupLabel("Harass Menu");
            HarassMenu.Add("UseQHarass", new CheckBox("Use Q", true));
            HarassMenu.Add("UseWHarass", new CheckBox("Use W", false));
            HarassMenu.Add("UseEHarass", new CheckBox("Use E", true));
            HarassMenu.Add("DisableAAHarass", new CheckBox("Disable AA While using Harass?", false));
            HarassMenu.Add("ManaManagerHarass", new Slider("Mana Manager", 60, 0, 100));
            
            //LaneClear Menu

            LaneClearMenu = FirstMenu.AddSubMenu("Lane Clear", "LaneclearCassC");
            LaneClearMenu.AddGroupLabel("Lane Clear Menu");
            LaneClearMenu.Add("UseQLaneClear", new CheckBox("Use Q", true));
            LaneClearMenu.Add("UseWLaneClear", new CheckBox("Use W", false));
            LaneClearMenu.Add("UseELaneClear", new CheckBox("Use E", false));
            LaneClearMenu.Add("UseEOnlyIfKillable", new CheckBox("Use E only if minion is killable?", true));
            LaneClearMenu.Add("UseEOnlyIfPoisoned", new CheckBox("Only E if poisoned", false));
            LaneClearMenu.Add("ManaManagerLaneClear", new Slider("Mana Manager", 50, 0, 100));

            //JungleClear Menu

            JungleClearMenu = FirstMenu.AddSubMenu("Jungle Clear", "JungleclearCassC");
            JungleClearMenu.AddGroupLabel("Jungle Clear Menu");
            JungleClearMenu.Add("UseQJungleClear", new CheckBox("Use Q", true));
            JungleClearMenu.Add("UseWJungleClear", new CheckBox("Use W", false));
            JungleClearMenu.Add("UseEJungleClear", new CheckBox("Use E", true));
            JungleClearMenu.Add("UseEJungleClearOnlyIfKillable", new CheckBox("Use E only if killable", false));
            JungleClearMenu.Add("UseEJungleClearOnlyIfPoisoned", new CheckBox("Only E if poisoned", true));
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
            DrawMenu.Add("ActivateDraw", new CheckBox("Use Drawings?", true));
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
            MiscMenu.Add("KillStealActivate", new CheckBox("Use Killsteal?", true));
            MiscMenu.Add("UseWAntiGapCloser", new CheckBox("Anti Gapcloser W", true));
            MiscMenu.Add("UseRAntiGapCloser", new CheckBox("Anti Gapcloser R", false));
            MiscMenu.Add("BlockRIfMissAnti", new CheckBox("Block R if Miss", false));
            MiscMenu.Add("MinHPToUseAntiGapCloser", new Slider("Miniumum HP to Anti Gapcloser R", 40, 0, 100));
            MiscMenu.Add("InteruptDangerSpells", new CheckBox("Use R To Interrupt Dangerous Spells?", false));
            MiscMenu.Add("DamageIndicator", new CheckBox("Damage Indicator (placeholder)", true));

        }

        //Drawing Things

        public void Drawing(EventArgs args)
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
                        PossibleDamage(target)) > target.Health &&
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

        public void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (Player.Instance.Mana <= HarassMenu["ManaMangerHarass"].Cast<Slider>().CurrentValue) return;
            if (target == null || !target.IsValidTarget(Q.Range)) return;

            if (IsChecked(HarassMenu, "UseQHarass") && Q.IsReady() && !target.IsValidTarget() &&
                target.HasBuffOfType(BuffType.Poison))
            {
                var Qpred = Q.GetPrediction(target);

                if (Qpred.HitChancePercent >= 75)
                {
                    Q.Cast(Qpred.CastPosition);
                } 
            }

            if (IsChecked(HarassMenu, "UseWHarass") && W.IsReady() && target.IsValidTarget())
            {
                if (!target.HasBuffOfType(BuffType.Poison) && !Q.IsReady())
                {
                    var predictionW = W.GetPrediction(target);

                    if (predictionW.HitChancePercent >= 70)
                    {
                        W.Cast(predictionW.CastPosition);
                    }
                }
            }

            if (IsChecked(HarassMenu, "UseEHarass") && E.IsReady() && target.IsValidTarget() && E.CanCast(target) && target.HasBuffOfType(BuffType.Poison))
            {
                E.Cast(target);
            }

        }

        //Killsteal Method

        private static void KillSteal()
        {
           
        }

        //Jungle Clear Method

        public void JungleClear()
        {
            if (Player.Instance.Mana < JungleClearMenu["ManaManagerJungleClear"].Cast<Slider>().CurrentValue) return;
            var minions = EntityManager.MinionsAndMonsters.Monsters;

            if (minions == null || !minions.Any(m => m.IsValidTarget(900))) return;
            var MJungleClear =
               (EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, Q.Range));
            var BestpositionQ = Q.GetBestCircularCastPosition(MJungleClear, 80);
            var BestpositionW = W.GetBestCircularCastPosition(MJungleClear, 70);

            if (IsChecked(JungleClearMenu, "UseQJungleClear") && Q.IsReady() && BestpositionQ.HitNumber >= 0)
            {
                Q.Cast(BestpositionQ.CastPosition);
            }

            if (IsChecked(JungleClearMenu, "UseWJungleClear") && W.IsReady() && BestpositionW.HitNumber >= 2)
            {
                W.Cast(BestpositionW.CastPosition);
            }

            if (IsChecked(JungleClearMenu, "UseEJungleClear") && E.IsReady())
            {
                if (IsChecked(JungleClearMenu, "UseEJungleClearOnlyIfKillable"))
                {
                    var minion =
                        EntityManager.MinionsAndMonsters.EnemyMinions.First(
                            t =>
                                t.IsValidTarget(E.Range) && Player.Instance.GetSpellDamage(t, SpellSlot.E) > t.Health &&
                                !IsChecked(JungleClearMenu, "UseEJungleClearOnlyIfPoisoned") ||
                                t.HasBuffOfType(BuffType.Poison));
                    if (minion != null)
                        E.Cast(minion);
                }
                else
                {
                    var minion =
                        EntityManager.MinionsAndMonsters.EnemyMinions.First(
                            t =>
                                t.IsValidTarget(E.Range) &&
                                (IsChecked(JungleClearMenu, "UseEJungleClearOnlyIfPoisoned") || t.HasBuffOfType(BuffType.Poison)));

                    if (minion != null)
                        E.Cast(minion);
                }
            }
        }

        //Flee Method

        public void OnFlee()
        {

        }

        //Game Update

        public void OnGameUpdate(EventArgs args)
        {
            if (IsChecked(SkinMenu, "SkinChangerActivate") == false)
            {
                SkinMenu["SkinChangerSlider"].Cast<Slider>().IsVisible = false;
            }

            if (skinId != GetSliderValue(SkinMenu, "SkinChangerSlider"))
            {
                skinId = GetSliderValue(SkinMenu, "SkinChangerSlider");
                Player.SetSkinId(skinId);
            }

            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    if (IsChecked(ComboMenu, "DisableAA"))
                        Orbwalker.DisableAttacking = true;
                    Combo();
                    break;
                case Orbwalker.ActiveModes.Flee:
                    OnFlee();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    if (IsChecked(HarassMenu, "DisableAAHarass"))
                        Orbwalker.DisableAttacking = true;
                    Harass();
                    break;
            }

            if (Orbwalker.DisableAttacking && (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Combo && Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.Harass))
                Orbwalker.DisableAttacking = false;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                LaneClear();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                JungleClear();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                LastHit();

            if (IsChecked(MiscMenu, "KillStealActivate"))
                KillSteal();
        }

        //Laneclear Method

       public void LaneClear()
        {
            if (Player.Instance.Mana <= LaneClearMenu["ManaMangerLaneClear"].Cast<Slider>().CurrentValue) return;
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions;
            if (minions == null || !minions.Any()) return;
            var M =
               (EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.Position, Q.Range));
           var BestpositionQ = Q.GetBestCircularCastPosition(M, 80);
            var BestpositionW = W.GetBestCircularCastPosition(M, 70);

            if (IsChecked(LaneClearMenu, "UseQLaneClear") && Q.IsReady() && BestpositionQ.HitNumber >= 0)
            {
                Q.Cast(BestpositionQ.CastPosition);
            }

            if (IsChecked(LaneClearMenu, "UseWLaneClear") && W.IsReady() && BestpositionW.HitNumber >= 2)
            {
                W.Cast(BestpositionW.CastPosition);
            }

            if (IsChecked(LaneClearMenu, "UseELaneClear") && E.IsReady())
            {
                if (IsChecked(LaneClearMenu, "UseEOnlyIfKillable"))
                {
                    var minion =
                        EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(t => t.IsValidTarget(E.Range) &&
                                                                                          Player.Instance.GetSpellDamage
                                                                                              (t, SpellSlot.E) >
                                                                                          t.Health &&
                                                                                          (!IsChecked(LaneClearMenu,
                                                                                              "UseEOnlyIfPoisoned") ||
                                                                                           t.HasBuffOfType(
                                                                                               BuffType.Poison)));
                    if (minion != null)
                        E.Cast(minion);
                }
                else
                {
                    var minion =
                        EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(
                            t =>
                                t.IsValidTarget(E.Range) &&
                                (IsChecked(LaneClearMenu, "UseEOnlyIfPoisoned") || t.HasBuffOfType(BuffType.Poison)));

                    if (minion != null)
                        E.Cast(minion);
                }
            }
        }

        //Last Hit Method

        public void LastHit()
        {
            if (Player.Instance.Mana <= LastHitMenu["ManaMangerLastHit"].Cast<Slider>().CurrentValue) return;

            var minions =
                EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                    m => m.IsValidTarget(E.Range) && Player.Instance.GetSpellDamage(m, SpellSlot.E) > m.Health);

            var target = minions.FirstOrDefault();

            if (IsChecked(LastHitMenu, "UseQLastHit") && Q.IsReady() && Q.IsInRange(target) &&
                !target.HasBuffOfType(BuffType.Poison))
            {
                Q.Cast(target.ServerPosition);
                lastQCast = Game.Time;
            }

            if (IsChecked(LastHitMenu, "UseWLastHit") && W.IsReady() && W.IsInRange(target) &&
                !target.HasBuffOfType(BuffType.Poison))
            {
                if (IsChecked(ComboMenu, "UseWIfCantQ"))
                {
                    if ((target.HasBuffOfType(BuffType.Poison) && !Q.IsReady() &&
                         (lastQCast - Game.Time) < -0.43f))
                    {
                        var predictionW = W.GetPrediction(target);

                        if (predictionW.HitChancePercent >= 70)
                        {
                            W.Cast(predictionW.CastPosition);
                        }
                    }

                }

            }
            else
            {
                var predictionW = W.GetPrediction(target);

                if (predictionW.HitChancePercent >= 70)
                {
                    W.Cast(predictionW.CastPosition);
                }
            }

            if (IsChecked(LastHitMenu, "UseELastHit") && E.IsReady() && E.IsInRange(target) &&
                target.HasBuffOfType(BuffType.Poison))
            {
                E.Cast(target);
            }
        }

        //Draw Method

        public void Drawing()
        {
            if (DrawMenu["ActivateDraw"].Cast<CheckBox>().CurrentValue) return;

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

        //Just have this here

        public void OnAfterAttack(AttackableUnit target, EventArgs args)
        {

        }

        public void OnPossibleToInterrupt(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs interruptableSpellEventArgs)
        {
            if (!sender.IsEnemy) return;

            if (IsChecked(MiscMenu, "InteruptDangerSpells") &&
                interruptableSpellEventArgs.DangerLevel >= DangerLevel.High && R.IsReady() && R.IsInRange(sender))
            {
                R.Cast(sender);
            }
        }

        public void OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!sender.IsEnemy) return;

            if ((e.End.Distance(Player.Instance) < 50 || e.Sender.IsAttackingPlayer) &&
                IsChecked(MiscMenu, "UseRAntiGapCloser") &&
                Player.Instance.HealthPercent < GetSliderValue(MiscMenu, "MinHPToUseAntiGapCloser") && R.IsReady() &&
                R.IsInRange(sender))
            {
                R.Cast(sender);
            }
            else if ((e.End.Distance(Player.Instance) < 50 || e.Sender.IsAttackingPlayer) &&
                     IsChecked(MiscMenu, "UseWAntiGapCloser") && W.IsReady() && W.IsInRange(sender))
            {
                W.Cast(e.End);
            }
        }

        public void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;

            if (args.SData.Name == "CassiopeiaTwinFang")
            {
                _lastECast = Game.Time;
            }

            if (args.SData.Name == "CassiopeiaPetrofyingGaze" && IsChecked(MiscMenu, "MinHPToUseAntiGapCloser"))
            {
                if (EntityManager.Heroes.Enemies.Count(t => t.IsValidTarget(R.Range) && t.IsFacing(Player.Instance)) < 1)
                {
                    args.Process = false;
                }
            }
        }

        public void GameObjectOnCreate(GameObject sender, EventArgs args)
        {

        }

        public void GameObjectOnDelete(GameObject sender, EventArgs args)
        {

        }

        private static void Killsteal()
        {
            if (E.IsReady() &&
                EntityManager.Heroes.Enemies.Any(
                    t => t.IsValidTarget(E.Range) && t.Health < Player.Instance.GetSpellDamage(t, SpellSlot.E)))
            {
                E.Cast(EntityManager.Heroes.Enemies.FirstOrDefault(t => t.IsValidTarget(E.Range) && t.Health < Player.Instance.GetSpellDamage(t, SpellSlot.E)));
            }

            if (Q.IsReady() &&
                EntityManager.Heroes.Enemies.Any(
                    t => t.IsValidTarget(Q.Range) && t.Health < Player.Instance.GetSpellDamage(t, SpellSlot.Q)))
            {
                var predQ = Q.GetPrediction(EntityManager.Heroes.Enemies.FirstOrDefault(t => t.IsValidTarget(Q.Range) && t.Health < Player.Instance.GetSpellDamage(t, SpellSlot.Q)));

                if (predQ.HitChancePercent >= 70)
                {
                    Q.Cast(predQ.CastPosition);
                }
            }
        }

        private static float PossibleDamage(Obj_AI_Base target)
        {
            var damage = 0f;
            if (R.IsReady())
                damage += Player.Instance.GetSpellDamage(target, SpellSlot.R);
            if (E.IsReady())
                damage += Player.Instance.GetSpellDamage(target, SpellSlot.E);
            if (W.IsReady())
                damage += Player.Instance.GetSpellDamage(target, SpellSlot.W);
            if (Q.IsReady())
                damage += Player.Instance.GetSpellDamage(target, SpellSlot.Q);

            return damage;
        }
    }
}
