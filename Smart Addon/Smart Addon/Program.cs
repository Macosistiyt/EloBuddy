using System;
using EloBuddy;
using EloBuddy.SDK.Events;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using System.Linq;
using System.Collections.Generic;

namespace Smart_Addon
{
    class Program
    {
        public static Spell.Skillshot Q, E, R;
        public static Spell.Chargeable W;
        public static Menu menu, prediction, Combo, Harass, Clear, Automatic, Drawings;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnGalioLoad;
        }

        private static void OnGalioLoad(EventArgs args)
        {
            if (Player.Instance.ChampionName == "Galio")
                if (Player.Instance.ChampionName != "Galio") return;

            Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Circular, 250, 500, 400);
            W = new Spell.Chargeable(SpellSlot.W, 275, 450, 0);
            E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Linear, 250, 300, 150)
            {
                AllowedCollisionCount = 1
            };
            R = new Spell.Skillshot(SpellSlot.R, 4000, SkillShotType.Circular);


            menu = MainMenu.AddMenu("Galio", "Galio");
            //
            prediction = menu.AddSubMenu("-> Prediction");
            prediction.AddLabel("--> Prediction <--");
            prediction.Add("Qhit", new Slider("HitChance --> Q", 75, 1, 100));
            prediction.Add("Ehit", new Slider("HitChance --> E", 50, 1, 100));
            prediction.AddSeparator();
            prediction.AddLabel("Change combo? Not recommended");
            prediction.Add("HitBox", new ComboBox("Combos", 0, "Q -> E -> W -> R", "W Rage -> Q -> E"));
            prediction.AddSeparator();
            prediction.AddLabel("The 2nd combo is still not working trying to solve the problem");

            //
            Combo = menu.AddSubMenu("-> Combo");
            Combo.AddLabel("--> Combo < --");
            Combo.Add("Qc", new CheckBox("Q -> Combo"));
            Combo.Add("Wc", new CheckBox("W -> Combo"));
            Combo.Add("Ec", new CheckBox("E -> Combo"));
            Combo.Add("Rc", new CheckBox("R -> Combo", false));
            Combo.AddSeparator();
            Combo.AddLabel("[R] Settings");
            Combo.Add("CorretCurso", new Slider("How would you like to use Uti?", 800, 1, 2000));
            Combo.Add("UtiEnemy", new Slider("Minimum of Enemies", 2, 1, 5));

            //
            Harass = menu.AddSubMenu("-> Harass");
            Harass.AddLabel("--> Harass < --");
            Harass.Add("Hq", new CheckBox("Q -> Harass"));
            Harass.Add("He", new CheckBox("E -> Harass", false));
            Harass.AddSeparator();
            Harass.AddLabel("Harass Mana");
            Harass.Add("Hqm", new Slider("Mana -> [Q]", 50, 1, 100));
            Harass.Add("Hem", new Slider("Mana -> [E]", 70, 1, 100));
            //
            Clear = menu.AddSubMenu("-> Clear");
            Clear.AddLabel("--> LaneClear and JungleClear < --");
            Clear.Add("Lq", new CheckBox("Q -> LaneClear"));
            Clear.Add("Jq", new CheckBox("Q -> JungleClear"));
            Clear.AddSeparator();
            Clear.AddLabel("-> Clear Mana <-");
            Clear.Add("Lqm", new Slider("Mana [Q]", 50, 1, 100));
            Clear.AddLabel("Jungle");
            Clear.Add("Jqm", new Slider("Mana [Q]", 50, 1, 100));
            //
            Automatic = menu.AddSubMenu("-> Automatic");
            Automatic.AddLabel("--> Automatic < --");
            Automatic.Add("Ruti", new CheckBox("R -> Ally"));
            Automatic.Add("Re", new CheckBox("R -> Automatic"));
            //
            Drawings = menu.AddSubMenu("-> Drawing");
            Drawings.AddLabel("--> Drawing < --");
            Drawings.Add("Dq", new CheckBox("Q -> Drawing"));
            Drawings.Add("Dw", new CheckBox("W -> Drawing"));
            Drawings.Add("De", new CheckBox("E -> Drawing"));
            Drawings.Add("Dr", new CheckBox("R -> Drawing"));

            Drawing.OnDraw += OnDrawing;
            Game.OnTick += OnTick;
        }

        private static void OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combos();
            }
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Harass))
            {
                Haras();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                Lanes();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Jungles();
            }
            RSmart();
        }

        private static void Haras()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var target2 = TargetSelector.GetTarget(E.Range, DamageType.Magical);

            if (Harass["Hq"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    var QHit = Q.GetPrediction(target);
                    if (QHit.HitChancePercent >= prediction["Qhit"].Cast<Slider>().CurrentValue)
                    {
                        Q.Cast(QHit.CastPosition);
                    }
                }
                if (Harass["He"].Cast<CheckBox>().CurrentValue)
                {
                    if (target2.IsValidTarget(E.Range) && E.IsReady())
                    {
                        var EHit = E.GetPrediction(target);
                        if (EHit.HitChancePercent >= prediction["Ehit"].Cast<Slider>().CurrentValue)
                        {
                            E.Cast(EHit.CastPosition);
                        }
                    }
                }
            }
        }

        private static void RSmart()
        {
            throw new NotImplementedException();
        }

        private static void Jungles()
        {
            if (Clear["Jq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                var MQ = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, Q.Range);
                if (MQ != null)
                {
                    foreach (var MonsterQ in MQ)
                    {
                        if (Player.Instance.ManaPercent >= Clear["Jqm"].Cast<Slider>().CurrentValue)
                        {
                            Q.Cast(MonsterQ);
                        }
                    }
                }
            }
        }

        private static void Lanes()
        {
            var clearminionQ = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.Position, Q.Range);
            var clearminionE = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.Position, E.Range);

            if (Clear["Lq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                foreach (var minionQ in clearminionQ)
                {
                    if (Player.Instance.ManaPercent >= Clear["Lqm"].Cast<Slider>().CurrentValue)
                    {
                        Q.Cast(minionQ);
                    }
                }
                foreach (var minionE in clearminionE)
                {
                    if (Player.Instance.ManaPercent >= Clear["Lqm"].Cast<Slider>().CurrentValue)
                    {
                        E.Cast(minionE);
                    }
                }
            }
        }

        private static void Combos()
        {
            if (prediction["HitBox"].Cast<ComboBox>().CurrentValue == 0)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if ((target == null) || target.IsInvulnerable) return;

                if (Combo["Qc"].Cast<CheckBox>().CurrentValue)
                {
                    if (target.IsValidTarget(Q.Range) && Q.IsReady())
                    {
                        var QHit = Q.GetPrediction(target);
                        if (QHit.HitChancePercent >= prediction["Qhit"].Cast<Slider>().CurrentValue)
                        {
                            Q.Cast(QHit.CastPosition);
                        }
                    }
                }
                if (Combo["Ec"].Cast<CheckBox>().CurrentValue)
                {
                    if (target.IsValidTarget(E.Range) && E.IsReady())
                    {
                        var EHit = E.GetPrediction(target);
                        if (EHit.HitChancePercent >= prediction["Ehit"].Cast<Slider>().CurrentValue)
                        {
                            E.Cast(EHit.CastPosition);
                        }
                    }
                }

                if (Combo["Wc"].Cast<CheckBox>().CurrentValue)
                {
                    if (W.IsReady() && target.IsValidTarget(W.Range) && W.IsInRange(target) && Q.IsOnCooldown && E.IsOnCooldown)
                    {
                        W.StartCharging();
                    }
                }

                if (Combo["Rc"].Cast<CheckBox>().CurrentValue)
                {
                    var ally = EntityManager.Heroes.Allies.FirstOrDefault(hero => !hero.IsMe && !hero.IsInShopRange() && !hero.IsZombie && hero.Distance(Player.Instance) <= R.Range);
                    var predictedHealth = Prediction.Health.GetPrediction(ally, R.CastDelay + Game.Ping);
                    if (ally.IsValidTarget(Combo["CorretCurso"].Cast<Slider>().CurrentValue) && R.IsReady() && Game.CursorPos.CountEnemyChampionsInRange(Combo["CorretCurso"].Cast<Slider>().CurrentValue) >= Combo["UtiEnemy"].Cast<Slider>().CurrentValue)
                    {
                        R.Cast(ally);
                    }
                    else if (R.Cast())
                    {
                    }

                    if (prediction["HitBox"].Cast<ComboBox>().CurrentValue == 1)
                    {
                        if (Combo["Ec"].Cast<CheckBox>().CurrentValue)
                        {
                            if (target.IsValidTarget(E.Range) && E.IsReady())
                            {
                                var EHit = E.GetPrediction(target);
                                if (EHit.HitChancePercent >= prediction["Ehit"].Cast<Slider>().CurrentValue)
                                {
                                    E.Cast(EHit.CastPosition);
                                }
                            }

                            if (target.IsValidTarget(Q.Range) && Q.IsReady())
                            {
                                var QHit = Q.GetPrediction(target);
                                if (QHit.HitChancePercent >= prediction["Qhit"].Cast<Slider>().CurrentValue)
                                {
                                    Q.Cast(QHit.CastPosition);
                                }
                            }
                        }
                        if (Combo["Wc"].Cast<CheckBox>().CurrentValue)
                        {
                            if (W.IsReady() && target.IsValidTarget(W.Range) && W.IsInRange(target) && Q.IsOnCooldown && E.IsOnCooldown)
                            {
                                W.StartCharging();
                            }
                        }
                    }
                }
            }
        }
        private static void OnDrawing(EventArgs args)
        {
            if (Drawings["Dq"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                Q.DrawRange(Color.LightBlue);
            }
            if (Drawings["Dw"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                Circle.Draw(Color.LightGreen, 450 + 50, Player.Instance.Position);
            }
            if (Drawings["Dr"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                R.DrawRange(Color.DeepPink);
            }
        }
    }
}