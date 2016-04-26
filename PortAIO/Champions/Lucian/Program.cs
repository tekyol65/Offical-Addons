using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace PortAIO.Champions.Lucian
{
    public class Program
    {
        public static void OnLoad()
        {
            if (ObjectManager.Player.ChampionName != "Lucian")
            {
                return;
            }

            LucianSpells.Init();
            LucianMenu.MenuInit();
            
            Chat.Print("<font color='#99FFFF'>LCS Series - Lucian loaded! </font><font color='#99FF00'> Be Rekkles ! Its Possible. Enjoy GODSPEED Spell + Passive Usage </font>");
            Chat.Print("<font color='##FFCC00'>LCS Series totally improved LCS player style.</font>");

            Game.OnUpdate += LucianOnUpdate;
            Obj_AI_Base.OnSpellCast += LucianOnDoCast;
            Drawing.OnDraw += LucianOnDraw;
        }
        public static bool UltActive
        {
            get { return ObjectManager.Player.HasBuff("LucianR"); }
        }

        private static void ECast(AIHeroClient enemy)
        {
            var range = Orbwalking.GetRealAutoAttackRange(enemy);
            var path = LeagueSharp.Common.Geometry.CircleCircleIntersection(ObjectManager.Player.ServerPosition.LSTo2D(),
                LeagueSharp.Common.Prediction.GetPrediction(enemy, 0.25f).UnitPosition.LSTo2D(), LucianSpells.E.Range, range);

            if (path.Count() > 0)
            {
                var epos = path.MinOrDefault(x => x.LSDistance(Game.CursorPos));
                if (epos.To3D().UnderTurret(true) || epos.To3D().LSIsWall())
                {
                    return;
                }

                if (epos.To3D().LSCountEnemiesInRange(LucianSpells.E.Range - 100) > 0)
                {
                    return;
                }
                LucianSpells.E.Cast(epos);
            }
            if (path.Count() == 0)
            {
                var epos = ObjectManager.Player.ServerPosition.LSExtend(enemy.ServerPosition, -LucianSpells.E.Range);
                if (epos.UnderTurret(true) || epos.LSIsWall())
                {
                    return;
                }

                // no intersection or target to close
                LucianSpells.E.Cast(ObjectManager.Player.ServerPosition.LSExtend(enemy.ServerPosition, -LucianSpells.E.Range));
            }
        }
        private static void LucianOnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Orbwalking.IsAutoAttack(args.SData.Name) && args.Target is AIHeroClient && args.Target.IsValid)
            {
                if (LucianMenu.luciancombostarte)
                {
                    if (!LucianSpells.E.IsReady() && LucianSpells.Q.IsReady() && LucianMenu.lucianqcombo &&
                    ObjectManager.Player.LSDistance(args.Target.Position) < LucianSpells.Q.Range &&
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        LucianSpells.Q.CastOnUnit(((AIHeroClient)args.Target));
                    }

                    if (!LucianSpells.E.IsReady() && LucianSpells.W.IsReady() && LucianMenu.lucianwcombo &&
                        ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.W.Range &&
                        Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        if (LucianMenu.luciandisablewprediction)
                        {
                            LucianSpells.W.Cast(((AIHeroClient)args.Target).Position);
                        }
                        else
                        {
                            if (LucianSpells.W.GetPrediction(((AIHeroClient)args.Target)).Hitchance >= HitChance.Medium)
                            {
                                LucianSpells.W.Cast(((AIHeroClient)args.Target).Position);
                            }
                        }

                    }
                    if (LucianSpells.E.IsReady() && LucianMenu.lucianecombo &&
                        ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.Q2.Range &&
                        Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        switch (LucianMenu.lucianemode)
                        {
                            case 0:
                                ECast(((AIHeroClient)args.Target));
                                break;
                            case 1:
                                LucianSpells.E.Cast(Game.CursorPos);
                                break;
                        }

                    }
                }
                else
                {
                    if (LucianSpells.Q.IsReady() && LucianMenu.lucianqcombo &&
                    ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.Q.Range &&
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        LucianSpells.Q.CastOnUnit(((AIHeroClient)args.Target));
                    }
                    if (LucianSpells.W.IsReady() && LucianMenu.lucianwcombo &&
                        ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.W.Range &&
                        Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff")
                        && LucianSpells.W.GetPrediction(((AIHeroClient)args.Target)).Hitchance >= HitChance.Medium)
                    {
                        LucianSpells.W.Cast(((AIHeroClient)args.Target).Position);
                    }
                    if (LucianSpells.E.IsReady() && LucianMenu.lucianecombo &&
                        ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.Q2.Range &&
                        Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                    {
                        switch (LucianMenu.lucianemode)
                        {
                            case 0:
                                ECast(((AIHeroClient)args.Target));
                                break;
                            case 1:
                                LucianSpells.E.Cast(Game.CursorPos);
                                break;
                        }
                    }
                }

            }
            else if (sender.IsMe && Orbwalking.IsAutoAttack(args.SData.Name) && args.Target is Obj_AI_Minion && args.Target.IsValid && ((Obj_AI_Minion)args.Target).Team == GameObjectTeam.Neutral
                && ObjectManager.Player.ManaPercent > LucianMenu.lucianclearmana)
            {
                if (LucianSpells.Q.IsReady() && LucianMenu.lucianqjungle &&
                    ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.Q.Range &&
                    (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    LucianSpells.Q.CastOnUnit(((Obj_AI_Minion)args.Target));
                }
                if (LucianSpells.W.IsReady() && LucianMenu.lucianwjungle &&
                    ObjectManager.Player.Distance(args.Target.Position) < LucianSpells.W.Range &&
                    (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    LucianSpells.W.Cast(((Obj_AI_Minion)args.Target).Position);
                }
                if (LucianSpells.E.IsReady() && LucianMenu.lucianejungle &&
                   ((Obj_AI_Minion)args.Target).IsValidTarget(1000) &&
                    (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
                {
                    LucianSpells.E.Cast(Game.CursorPos);
                }

            }
        }
        private static void LucianOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    Clear();
                    break;
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Clear();
            }

            if (LucianMenu.luciansemimanualult)
            {
                SemiManual();
            }

            if (UltActive && LucianMenu.luciansemimanualult)
            {
                Orbwalker.DisableAttacking = true;
            }

            if (!UltActive || !LucianMenu.luciansemimanualult)
            {
                Orbwalker.DisableAttacking = false;
            }



        }
        private static void SemiManual()
        {
            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(LucianSpells.R.Range) &&
                LucianSpells.R.GetPrediction(x).CollisionObjects.Count == 0))
            {
                LucianSpells.R.Cast(enemy);
            }
        }
        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < LucianMenu.lucianharassmana)
            {
                return;
            }
            if (LucianSpells.Q.IsReady() || LucianSpells.Q2.IsReady() && LucianMenu.lucianqharass && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                HarassQCast();
            }
            if (LucianSpells.W.IsReady() && LucianMenu.lucianwharass && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(LucianSpells.W.Range) && LucianSpells.W.GetPrediction(x).Hitchance >= HitChance.Medium))
                {
                    LucianSpells.W.Cast(enemy);
                }
            }
        }
        private static void HarassQCast()
        {
            switch (LucianMenu.lucianqtype)
            {
                case 0:
                    var minions = ObjectManager.Get<Obj_AI_Minion>().Where(o => o.LSIsValidTarget(LucianSpells.Q.Range));
                    var target = ObjectManager.Get<AIHeroClient>().Where(x => x.LSIsValidTarget(LucianSpells.Q2.Range)).FirstOrDefault(x => LucianMenu.harassMenu["lucian.white" + x.ChampionName].Cast<CheckBox>().CurrentValue);
                    if (target.LSDistance(ObjectManager.Player.Position) > LucianSpells.Q.Range && target.LSCountEnemiesInRange(LucianSpells.Q2.Range) > 0)
                    {
                        foreach (var minion in minions)
                        {
                            if (LucianSpells.Q2.WillHit(target, ObjectManager.Player.ServerPosition.LSExtend(minion.Position, LucianSpells.Q2.Range), 0, HitChance.VeryHigh))
                            {
                                LucianSpells.Q2.CastOnUnit(minion);
                            }
                        }
                    }
                    break;
                case 1:
                    foreach (var enemy in HeroManager.Enemies.Where(x => x.LSIsValidTarget(LucianSpells.Q.Range)))
                    {
                        LucianSpells.Q.CastOnUnit(enemy);
                    }
                    break;
            }
        }
        private static void Clear()
        {
            if (ObjectManager.Player.ManaPercent < LucianMenu.lucianclearmana)
            {
                return;
            }
            if (LucianSpells.Q.IsReady() && LucianMenu.lucianqclear && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.ServerPosition, LucianSpells.Q.Range, MinionTypes.All,
                MinionTeam.NotAlly))
                {
                    var prediction = LeagueSharp.Common.Prediction.GetPrediction(minion, LucianSpells.Q.Delay,
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).SData.CastRadius);

                    var collision = LucianSpells.Q.GetCollision(ObjectManager.Player.Position.To2D(),
                        new List<Vector2> { prediction.UnitPosition.To2D() });

                    foreach (var cs in collision)
                    {
                        if (collision.Count >= LucianMenu.lucianqminionhitcount)
                        {
                            if (collision.Last().Distance(ObjectManager.Player) -
                                collision[0].Distance(ObjectManager.Player) <= 600
                                && collision[0].Distance(ObjectManager.Player) <= 500)
                            {
                                LucianSpells.Q.Cast(cs);
                            }
                        }
                    }

                }
            }
            if (LucianSpells.W.IsReady() && LucianMenu.lucianwclear && ObjectManager.Player.Buffs.Any(buff => buff.Name != "lucianpassivebuff"))
            {
                if (LucianSpells.W.GetCircularFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, LucianSpells.Q.Range, MinionTypes.All, MinionTeam.NotAlly)).MinionsHit >= LucianMenu.lucianwminionhitcount)
                {
                    LucianSpells.W.Cast(LucianSpells.W.GetCircularFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, LucianSpells.Q.Range, MinionTypes.All, MinionTeam.NotAlly)).Position);
                }
            }
        }
        private static void LucianOnDraw(EventArgs args)
        {
            LucianDrawing.Init();
        }
    }
}
