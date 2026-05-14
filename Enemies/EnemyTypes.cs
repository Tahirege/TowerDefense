using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefense.Enemies
{
    public class BasicEnemy : Enemy
    {
        public BasicEnemy(float x, float y) : base(x, y, hp: 80, speed: 60f, reward: 10)
        { EnemyName = "Soldier"; EnemyColor = new SKColor(210, 70, 70); Size = 20f; }
    }

    public class FastEnemy : Enemy
    {
        public FastEnemy(float x, float y) : base(x, y, hp: 40, speed: 130f, reward: 15)
        { EnemyName = "Cavalry"; EnemyColor = new SKColor(255, 165, 0); Size = 16f; }
    }

    public class BossEnemy : Enemy
    {
        public float Pulse;
        public BossEnemy(float x, float y) : base(x, y, hp: 600, speed: 30f, reward: 100)
        { EnemyName = "BOSS"; EnemyColor = new SKColor(160, 0, 220); Size = 36f; }

        public override void Update(float dt) { base.Update(dt); Pulse += dt; }

        public override void TakeDamage(int dmg)
        { if (Hp > MaxHp / 2) dmg = (int)(dmg * 0.7f); base.TakeDamage(dmg); }
    }

    public class ArmoredEnemy : Enemy
    {
        public ArmoredEnemy(float x, float y) : base(x, y, hp: 200, speed: 45f, reward: 30)
        { EnemyName = "Armored"; EnemyColor = new SKColor(130, 130, 150); Size = 24f; }

        public override void TakeDamage(int dmg) => base.TakeDamage(Math.Max(1, dmg - 10));
    }

    public class HealerEnemy : Enemy
    {
        public float HealTimer;
        public float GlowPulse;

        public HealerEnemy(float x, float y) : base(x, y, hp: 120, speed: 50f, reward: 40)
        { EnemyName = "Healer"; EnemyColor = new SKColor(60, 210, 110); Size = 22f; }

        public List<Enemy>? NearbyEnemies { get; set; }

        public override void Update(float dt)
        {
            base.Update(dt);
            GlowPulse += dt;
            HealTimer += dt;
            if (HealTimer >= 2f)
            {
                HealTimer = 0f;
                if (NearbyEnemies != null)
                    foreach (var e in NearbyEnemies.Where(e => e.IsAlive && e != this))
                    {
                        float dx = e.X - X, dy = e.Y - Y;
                        if (MathF.Sqrt(dx*dx + dy*dy) < 80f) e.Heal(15);
                    }
            }
        }
    }

    public class FlyingEnemy : Enemy
    {
        public float WingTimer;
        public List<(float x, float y)>? DirectPath;

        public FlyingEnemy(float x, float y) : base(x, y, hp: 60, speed: 90f, reward: 25)
        { EnemyName = "Eagle"; EnemyColor = new SKColor(80, 170, 255); Size = 18f; }

        public void SetDirectPath(float endX, float endY)
        {
            int steps = 40;
            var path = new List<(float, float)>();
            for (int i = 0; i <= steps; i++)
                path.Add((X + (endX - X) * i / steps, Y + (endY - Y) * i / steps));
            DirectPath = path;
            SetPath(DirectPath);
        }

        public override void Update(float dt) { base.Update(dt); WingTimer += dt; }
    }

    public class BomberEnemy : Enemy
    {
        public float FuseTimer = 0f;
        public float ExplosionRadius = 90f;
        public int   ExplosionDamage = 40;
        public bool  HasExploded;

        public (float x, float y)? CurrentTowerTarget { get; set; }
        public const float ExplodeRange = 36f;

        public Action<float, float, float, int>? OnBomberExplode { get; set; }

        public BomberEnemy(float x, float y) : base(x, y, hp: 90, speed: 60f, reward: 35)
        {
            EnemyName  = "Bomber";
            EnemyColor = new SKColor(80, 160, 50);
            Size = 22f;
        }

        public override void Update(float dt)
        {
            FuseTimer += dt;
            base.Update(dt);
            if (!IsAlive && !HasExploded && !ReachedEnd)
            {
                HasExploded = true;
                OnBomberExplode?.Invoke(X, Y, ExplosionRadius, ExplosionDamage);
            }
        }

        public override void MoveAlongPath(float dt)
        {
            if (!CurrentTowerTarget.HasValue) { base.MoveAlongPath(dt); return; }
            var (tx, ty) = CurrentTowerTarget.Value;
            float dx = tx - X, dy = ty - Y;
            float dist = MathF.Sqrt(dx*dx + dy*dy);
            if (dist <= ExplodeRange)
            {
                HasExploded = true;
                OnBomberExplode?.Invoke(X, Y, ExplosionRadius, ExplosionDamage);
                Destroy();
                return;
            }
            float speed = CurrentSpeed * 1.25f;
            X += (dx / dist) * speed * dt;
            Y += (dy / dist) * speed * dt;
        }
    }

    public class AggroEnemy : Enemy
    {
        public Managers.MapManager? Map { get; set; }
        public Action<float, float, float, int>? OnAggroExplode { get; set; }
        public float ExplosionRadius = 80f;
        public int   ExplosionDamage = 30;
        public bool  HasExploded;

        public AggroEnemy(float x, float y) : base(x, y, 120, 45f, 25)
        {
            EnemyName = "Savaşçı";
            EnemyColor = SKColors.DarkRed;
            Size = 22f;
            AttackRange = 40f;
            AttackDamage = 15;
            AttackCooldown = 0.8f;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (!IsAlive && !HasExploded && !ReachedEnd)
            {
                HasExploded = true;
                OnAggroExplode?.Invoke(X, Y, ExplosionRadius, ExplosionDamage);
            }
        }
    }
}
