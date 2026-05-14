using SkiaSharp;
using TowerDefense.Core;
using TowerDefense.Enemies;

namespace TowerDefense.Shots
{
    public abstract class Shot : GameObject
    {
        public Enemy Target { get; }
        public int Damage { get; }
        public float Speed { get; }
        public SKColor Color { get; set; } = SKColors.Yellow;
        public bool IsHit { get; set; }

        protected Shot(float x, float y, Enemy target, int damage, float speed) : base(x, y)
        {
            Target = target;
            Damage = damage;
            Speed = speed;
        }

        public override void Update(float dt)
        {
            if (IsHit || !IsAlive) return;
            if (Target == null || !Target.IsAlive) { Destroy(); return; }

            float dx = Target.X - X, dy = Target.Y - Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist < Speed * dt + 5f)
            {
                OnHit(Target);
                IsHit = true;
                Destroy();
                return;
            }

            X += dx / dist * Speed * dt;
            Y += dy / dist * Speed * dt;
        }

        public abstract void OnHit(Enemy target);

        public override void Draw(SKCanvas canvas)
        {
            using var paint = new SKPaint { Color = Color, IsAntialias = true };
            canvas.DrawCircle(X, Y, 4, paint);
        }
    }

    public class Arrow : Shot
    {
        public Arrow(float x, float y, Enemy t, int dmg) : base(x, y, t, dmg, 300f) => Color = new SKColor(70, 130, 180);
        public override void OnHit(Enemy t) => t.TakeDamage(Damage);
        public override void Draw(SKCanvas c)
        {
            float dx = Target.X - X, dy = Target.Y - Y, d = MathF.Sqrt(dx * dx + dy * dy);
            if (d < 1) return;
            using var p = new SKPaint { Color = Color, IsAntialias = true, StrokeWidth = 2.5f, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(X, Y, X + dx / d * 14, Y + dy / d * 14, p);
        }
    }

    public class Ball : Shot
    {
        public float SplashRadius { get; }
        public static event Action<float, float, float>? OnExplosion;
        public Ball(float x, float y, Enemy t, int dmg, float splash) : base(x, y, t, dmg, 180f) 
        { 
            SplashRadius = splash; 
            Color = new SKColor(200, 100, 50); 
        }
        public override void OnHit(Enemy t) => OnExplosion?.Invoke(t.X, t.Y, SplashRadius);
        public override void Draw(SKCanvas c)
        {
            using var fill = new SKPaint { Color = Color, IsAntialias = true };
            using var stroke = new SKPaint { Color = SKColors.Black, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
            c.DrawCircle(X, Y, 7, fill);
            c.DrawCircle(X, Y, 7, stroke);
        }
    }

    public class Ice : Shot
    {
        public float SlowAmount { get; }
        public Ice(float x, float y, Enemy t, int dmg, float slow) : base(x, y, t, dmg, 220f)
        { 
            SlowAmount = slow; 
            Color = new SKColor(150, 220, 255); 
        }
        public override void OnHit(Enemy t) { t.TakeDamage(Damage); t.ApplySlow(SlowAmount, 2.5f); }
    }

    public class Laser : Shot
    {
        public Laser(float x, float y, Enemy t, int dmg) : base(x, y, t, dmg, 9999f) => Color = new SKColor(255, 50, 180);
        public override void OnHit(Enemy t) => t.TakeDamage(Damage);
        public override void Draw(SKCanvas c) { }
    }

    public class Sniper : Shot
    {
        public List<(float x, float y, float alpha)> Trail = new();
        public Sniper(float x, float y, Enemy t, int dmg) : base(x, y, t, dmg, 600f) => Color = new SKColor(200, 255, 150);
        public override void OnHit(Enemy t) => t.TakeDamage(Damage);
        public override void Update(float dt)
        {
            Trail.Add((X, Y, 1f));
            for (int i = Trail.Count - 1; i >= 0; i--)
            {
                var (tx, ty, a) = Trail[i];
                if (a <= 0) { Trail.RemoveAt(i); continue; }
                Trail[i] = (tx, ty, a - dt * 3f);
            }
            base.Update(dt);
        }
        public override void Draw(SKCanvas c)
        {
            foreach (var (tx, ty, a) in Trail)
            {
                using var p = new SKPaint { Color = Color.WithAlpha((byte)(a * 200)), IsAntialias = true };
                c.DrawCircle(tx, ty, 2f, p);
            }
            using var head = new SKPaint { Color = SKColors.White, IsAntialias = true };
            c.DrawCircle(X, Y, 4f, head);
        }
    }
}
