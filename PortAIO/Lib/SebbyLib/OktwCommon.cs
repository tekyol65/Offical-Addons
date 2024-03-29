﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using SharpDX;

namespace SebbyLib
{
    public class OktwCommon
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }

        private static int LastAATick = Utils.GameTimeTickCount;
        public static bool YasuoInGame = false;
        public static bool Thunderlord = false;

        public static bool
            blockMove = false,
            blockAttack = false,
            blockSpells = false;

        private static List<UnitIncomingDamage> IncomingDamageList = new List<UnitIncomingDamage>();
        private static List<AIHeroClient> ChampionList = new List<AIHeroClient>();
        private static YasuoWall yasuoWall = new YasuoWall();

        static OktwCommon()
        {
            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {
                ChampionList.Add(hero);
                if (hero.IsEnemy && hero.ChampionName == "Yasuo")
                    YasuoInGame = true;
            }
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            EloBuddy.Player.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnDoCast;
            Game.OnWndProc += Game_OnWndProc;
        }

        public static void debug(string msg)
        {
            if (true)
            {
                Console.WriteLine(msg);
            }
            if (false)
            {
                Chat.Print(msg);
            }
        }

        public static double GetIncomingDamage(AIHeroClient target, float time = 0.5f, bool skillshots = true)
        {
            double totalDamage = 0;

            foreach (var damage in IncomingDamageList.Where(damage => damage.TargetNetworkId == target.NetworkId && Game.Time - time < damage.Time))
            {
                if (skillshots)
                {
                    totalDamage += damage.Damage;
                }
                else
                {
                    if (!damage.Skillshot)
                        totalDamage += damage.Damage;
                }
            }

            return totalDamage;
        }

        public static bool CanHarras()
        {
            if (!Player.UnderTurret(true) && Orbwalking.CanMove(50) && !ShouldWait())
                return true;
            else
                return false;
        }
        public static bool ShouldWait()
        {
            var attackCalc = (int)(Player.AttackDelay * 1000 * 2);
            return
                Cache.GetMinions(Player.Position, 0).Any(
                    minion => HealthPrediction.LaneClearHealthPrediction(minion, attackCalc, 30) <= Player.LSGetAutoAttackDamage(minion));
        }


        public static float GetEchoLudenDamage(AIHeroClient target)
        {
            float totalDamage = 0;

            if (Player.HasBuff("itemmagicshankcharge"))
            {
                if (Player.GetBuff("itemmagicshankcharge").Count == 100)
                {
                    totalDamage += (float)Player.CalcDamage(target, DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
                }
            }
            return totalDamage;
        }

        public static bool IsSpellHeroCollision(AIHeroClient t, Spell QWER, int extraWith = 50)
        {
            foreach (var hero in HeroManager.Enemies.FindAll(hero => hero.LSIsValidTarget(QWER.Range + QWER.Width, true, QWER.RangeCheckFrom) && t.NetworkId != hero.NetworkId))
            {
                var prediction = QWER.GetPrediction(hero);
                var powCalc = Math.Pow((QWER.Width + extraWith + hero.BoundingRadius), 2);
                if (prediction.UnitPosition.LSTo2D().LSDistance(QWER.From.LSTo2D(), QWER.GetPrediction(t).CastPosition.LSTo2D(), true, true) <= powCalc)
                {
                    return true;
                }
                else if (prediction.UnitPosition.LSTo2D().LSDistance(QWER.From.LSTo2D(), t.ServerPosition.LSTo2D(), true, true) <= powCalc)
                {
                    return true;
                }

            }
            return false;
        }

        public static bool CanHitSkillShot(Obj_AI_Base target, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target == null && target.LSIsValidTarget(float.MaxValue,false))
            {

                var pred = Prediction.Prediction.GetPrediction(target, 0.25f).CastPosition;
                if (pred == null)
                    return false;

                if (args.SData.LineWidth > 0)
                {
                    var powCalc = Math.Pow(args.SData.LineWidth + target.BoundingRadius, 2);
                    if (pred.LSTo2D().LSDistance(args.End.LSTo2D(), args.Start.LSTo2D(), true, true) <= powCalc || target.ServerPosition.LSTo2D().LSDistance(args.End.LSTo2D(), args.Start.LSTo2D(), true, true) <= powCalc)
                    {
                        return true;
                    } 
                }
                else if (target.LSDistance(args.End) < 50 + target.BoundingRadius || pred.LSDistance(args.End) < 50 + target.BoundingRadius)
                {
                    return true;
                }  
            }
            return false;
        }

        public static float GetKsDamage(AIHeroClient t, Spell QWER)
        {
            var totalDmg = QWER.GetDamage(t);
            totalDmg -= t.HPRegenRate;

            if (totalDmg > t.Health)
            {
                if (Player.HasBuff("summonerexhaust"))
                    totalDmg = totalDmg * 0.6f;

                if (t.HasBuff("ferocioushowl"))
                    totalDmg = totalDmg * 0.7f;

                if (t.ChampionName == "Blitzcrank" && !t.HasBuff("BlitzcrankManaBarrierCD") && !t.HasBuff("ManaBarrier"))
                {
                    totalDmg -= t.Mana / 2f;
                }
            }
            //if (Thunderlord && !Player.HasBuff( "masterylordsdecreecooldown"))
            //totalDmg += (float)Player.CalcDamage(t, Damage.DamageType.Magical, 10 * Player.Level + 0.1 * Player.FlatMagicDamageMod + 0.3 * Player.FlatPhysicalDamageMod);
            totalDmg += (float)GetIncomingDamage(t);
            return totalDmg;
        }

        public static bool ValidUlt(AIHeroClient target)
        {
            if (target.HasBuffOfType(BuffType.PhysicalImmunity) || target.HasBuffOfType(BuffType.SpellImmunity)
                || target.IsZombie || target.IsInvulnerable || target.HasBuffOfType(BuffType.Invulnerability) || target.HasBuff("kindredrnodeathbuff")
                || target.HasBuffOfType(BuffType.SpellShield) || target.Health - GetIncomingDamage(target) < 1)
                return false;
            else
                return true;
        }

        public static bool CanMove(AIHeroClient target)
        {
            if (target.MoveSpeed < 50 || target.IsStunned || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuff("Recall") ||
                target.HasBuffOfType(BuffType.Knockback) || target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Suppression) || (target.IsChannelingImportantSpell() && !target.IsMoving))
            {
                return false;
            }
            else
                return true;
        }

        public static int GetBuffCount(Obj_AI_Base target, string buffName)
        {
            foreach (var buff in target.Buffs.Where(buff => buff.Name.ToLower() == buffName.ToLower()))
            {
                if (buff.Count == 0)
                    return 1;
                else
                    return buff.Count;
            }
            return 0;
        }

        public static int CountEnemyMinions(Obj_AI_Base target, float range)
        {
            var allMinions = Cache.GetMinions(target.Position, range);
            if (allMinions != null)
                return allMinions.Count;
            else
                return 0;
        }

        public static float GetPassiveTime(Obj_AI_Base target, string buffName)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Name.ToLower() == buffName.ToLower())
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault() - Game.Time;
        }

        public static bool CollisionYasuo(Vector3 from, Vector3 to)
        {
            if (!YasuoInGame)
                return false;

            if (Game.Time - yasuoWall.CastTime > 4)
                return false;

            var level = yasuoWall.WallLvl;
            var wallWidth = (350 + 50 * level);
            var wallDirection = (yasuoWall.CastPosition.LSTo2D() - yasuoWall.YasuoPosition.LSTo2D()).LSNormalized().LSPerpendicular();
            var wallStart = yasuoWall.CastPosition.LSTo2D() + wallWidth / 2f * wallDirection;
            var wallEnd = wallStart - wallWidth * wallDirection;

            if (wallStart.LSIntersection(wallEnd, to.LSTo2D(), from.LSTo2D()).Intersects)
            {
                return true;
            }
            return false;
        }

        public static void DrawTriangleOKTW(float radius, Vector3 position, System.Drawing.Color color, float bold = 1)
        {
            var positionV2 = Drawing.WorldToScreen(position);
            Vector2 a = new Vector2(positionV2.X + radius, positionV2.Y + radius / 2);
            Vector2 b = new Vector2(positionV2.X - radius, positionV2.Y + radius / 2);
            Vector2 c = new Vector2(positionV2.X, positionV2.Y - radius);
            Drawing.DrawLine(a[0], a[1], b[0], b[1], bold, color);
            Drawing.DrawLine(b[0], b[1], c[0], c[1], bold, color);
            Drawing.DrawLine(c[0], c[1], a[0], a[1], bold, color);
        }

        public static void DrawLineRectangle(Vector3 start2, Vector3 end2, int radius, float width, System.Drawing.Color color)
        {
            Vector2 start = start2.LSTo2D();
            Vector2 end = end2.LSTo2D();
            var dir = (end - start).LSNormalized();
            var pDir = dir.LSPerpendicular();

            var rightStartPos = start + pDir * radius;
            var leftStartPos = start - pDir * radius;
            var rightEndPos = end + pDir * radius;
            var leftEndPos = end - pDir * radius;

            var rStartPos = Drawing.WorldToScreen(new Vector3(rightStartPos.X, rightStartPos.Y, ObjectManager.Player.Position.Z));
            var lStartPos = Drawing.WorldToScreen(new Vector3(leftStartPos.X, leftStartPos.Y, ObjectManager.Player.Position.Z));
            var rEndPos = Drawing.WorldToScreen(new Vector3(rightEndPos.X, rightEndPos.Y, ObjectManager.Player.Position.Z));
            var lEndPos = Drawing.WorldToScreen(new Vector3(leftEndPos.X, leftEndPos.Y, ObjectManager.Player.Position.Z));

            Drawing.DrawLine(rStartPos, rEndPos, width, color);
            Drawing.DrawLine(lStartPos, lEndPos, width, color);
            Drawing.DrawLine(rStartPos, lStartPos, width, color);
            Drawing.DrawLine(lEndPos, rEndPos, width, color);
        }

        public static List<Vector3> CirclePoints(float CircleLineSegmentN, float radius, Vector3 position)
        {
            List<Vector3> points = new List<Vector3>();
            for (var i = 1; i <= CircleLineSegmentN; i++)
            {
                var angle = i * 2 * Math.PI / CircleLineSegmentN;
                var point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z);
                points.Add(point);
            }
            return points;
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 123 && blockMove)
            {
                blockMove = false;
                blockAttack = false;
                Orbwalking.Attack = true;
                Orbwalking.Move = true;
                EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target != null && args.SData != null)
            {
                if (args.Target.Type == GameObjectType.AIHeroClient && !sender.IsMelee && args.Target.Team != sender.Team)
                {
                    IncomingDamageList.Add(new UnitIncomingDamage { Damage = sender.LSGetSpellDamage((Obj_AI_Base)args.Target, args.SData.Name), TargetNetworkId = args.Target.NetworkId, Time = Game.Time, Skillshot = false });
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            float time = Game.Time - 2;
            IncomingDamageList.RemoveAll(damage => time < damage.Time);
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData == null)
            {
                return;
            }
            /////////////////  HP prediction
            var targed = args.Target as Obj_AI_Base;
            
            if (targed != null)
            {
                if (targed.Type == GameObjectType.AIHeroClient && targed.Team != sender.Team && sender.IsMelee)
                {
                    IncomingDamageList.Add(new UnitIncomingDamage { Damage = sender.LSGetSpellDamage(targed, args.SData.Name), TargetNetworkId = args.Target.NetworkId, Time = Game.Time, Skillshot = false });
                }
            }
            else
            {
                foreach (var champion in ChampionList.Where(champion => !champion.IsDead && champion.IsVisible && champion.Team != sender.Team && champion.LSDistance(sender) < 2000))
                {
                    if (CanHitSkillShot(champion,args))
                    {
                        IncomingDamageList.Add(new UnitIncomingDamage { Damage = sender.LSGetSpellDamage(champion, args.SData.Name), TargetNetworkId = champion.NetworkId, Time = Game.Time, Skillshot = true });
                    }
                }

                if (!YasuoInGame)
                    return;

                if (!sender.IsEnemy || sender.IsMinion || args.SData.IsAutoAttack() || sender.Type != GameObjectType.AIHeroClient)
                    return;

                if (args.SData.Name == "YasuoWMovingWall")
                {
                    yasuoWall.CastTime = Game.Time;
                    yasuoWall.CastPosition = sender.Position.LSExtend(args.End, 400);
                    yasuoWall.YasuoPosition = sender.Position;
                    yasuoWall.WallLvl = sender.Spellbook.Spells[1].Level;
                }
            }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (blockSpells)
            {
                args.Process = false;
            }
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (blockMove && !args.IsAttackMove)
            {
                args.Process = false;
            }
            if (blockAttack && args.IsAttackMove)
            {
                args.Process = false;
            }
        }

    }

    class UnitIncomingDamage
    {
        public int TargetNetworkId { get; set; }
        public float Time { get; set; }
        public double Damage { get; set; }
        public bool Skillshot { get; set; }
    }

    class YasuoWall
    {
        public Vector3 YasuoPosition { get; set; }
        public float CastTime { get; set; }
        public Vector3 CastPosition { get; set; }
        public float WallLvl { get; set; }

        public YasuoWall()
        {
            CastTime = 0;
        }
    }
}
