using SkiaSharp;
using System;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public class Hero : GameObject
    {
        public float Speed     = 150f;
        public int   MaxHealth = 500;
        public int   Health;

        public Managers.MapManager? Map;

        public float AttackRange    = 65f;
        public int   AttackDamage   = 50;
        public float AttackCooldown = 0.6f;
        public float AttackTimer;

        public float SwingAngle;
        public float SwingStartAngle;
        public float SwingEndAngle;
        public bool  Swinging;
        public bool  IsSwinging => Swinging;
        public float SwingTimer;
        public const float SwingDuration = 0.18f;

        public float WalkCycle;
        public float FacingX = 1f;

        public bool MovingLeft;
        public bool MovingRight;
        public bool MovingUp;
        public bool MovingDown;

        public HashSet<Enemy> HitThisSwing = new();

        public Hero(float startX, float startY) : base(startX, startY)
        {
            Health = MaxHealth;
            SwingAngle = -MathF.PI / 4f;
        }

        public void TakeDamage(int damage) => Health = Math.Max(0, Health - damage);

        public override void Update(float dt)
        {
            if (Health <= 0 || Map == null) return;
            HandleMovement(dt, Map);
            UpdateAnimations(dt);
            if (AttackTimer > 0) AttackTimer -= dt;
        }

        public void HandleMovement(float dt, TowerDefense.Managers.MapManager map)
        {
            float dx = 0, dy = 0;
            if (MovingLeft)  dx -= 1;
            if (MovingRight) dx += 1;
            if (MovingUp)    dy -= 1;
            if (MovingDown)  dy += 1;

            if (dx != 0 || dy != 0)
            {
                float len = MathF.Sqrt(dx*dx + dy*dy);
                dx /= len; dy /= len;
                if (dx != 0) FacingX = dx > 0 ? 1f : -1f;

                float nextX = Math.Clamp(X + dx * Speed * dt, 10, map.MapWidth - 10);
                float nextY = Math.Clamp(Y + dy * Speed * dt, 10, map.MapHeight - 10);

                if (map.IsWalkable(nextX, Y, 8f)) X = nextX;
                if (map.IsWalkable(X, nextY, 8f)) Y = nextY;

                WalkCycle += dt * 12f;
            }
            else
            {
                WalkCycle %= MathF.PI * 2f;
                if (WalkCycle > 0.1f) WalkCycle = Lerp(WalkCycle, MathF.PI * 2f, dt * 8f);
                else WalkCycle = 0;
            }
        }

        public void UpdateAnimations(float dt)
        {
            if (Swinging)
            {
                SwingTimer += dt;
                float t = Math.Clamp(SwingTimer / SwingDuration, 0f, 1f);
                SwingAngle = Lerp(SwingStartAngle, SwingEndAngle, t);
                if (SwingTimer >= SwingDuration)
                {
                    Swinging   = false;
                    SwingTimer = 0f;
                    HitThisSwing.Clear();
                }
            }
        }

        public bool TrySwing(float targetX, float targetY)
        {
            if (Health <= 0 || AttackTimer > 0) return false;
            AttackTimer = AttackCooldown;

            float dx = targetX - X, dy = targetY - Y;
            float targetAngle = MathF.Atan2(dy, dx);
            
            SwingStartAngle = targetAngle - 1.4f; 
            SwingEndAngle   = targetAngle + 1.4f;
            SwingAngle      = SwingStartAngle;

            Swinging    = true;
            SwingTimer  = 0f;
            HitThisSwing.Clear();
            if (dx != 0) FacingX = dx > 0 ? 1f : -1f;
            return true;
        }

        public bool TryHitEnemy(Enemy e)
        {
            if (!Swinging || HitThisSwing.Contains(e)) return false;
            float dx = e.X - X, dy = e.Y - Y;
            if (MathF.Sqrt(dx*dx + dy*dy) > AttackRange + e.Size/2) return false;

            float angle = MathF.Atan2(dy, dx);
            if (Math.Abs(AngleDiff(angle, SwingAngle)) > 0.6f) return false;

            HitThisSwing.Add(e);
            return true;
        }

        public override void Draw(SKCanvas canvas)
        {
            if (Health > 0) HeroRenderer.Draw(canvas, this);
        }

        public static float Lerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0, 1);

        public static float AngleDiff(float a, float b)
        {
            float d = a - b;
            while (d >  MathF.PI) d -= 2 * MathF.PI;
            while (d < -MathF.PI) d += 2 * MathF.PI;
            return d;
        }
    }
}
