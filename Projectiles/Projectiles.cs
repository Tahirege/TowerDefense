using SkiaSharp;
using TowerDefense.Core;
using TowerDefense.Enemies;

namespace TowerDefense.Projectiles
{
    // ── Abstract base ─────────────────────────────────────────
    public abstract class Projectile : GameObject
    {
        protected Enemy Target;
        public int Damage { get; protected set; }
        protected float Speed;
        protected SKColor ProjColor;
        private bool _hit;

        protected Projectile(float x, float y, Enemy target, int dmg, float speed)
            : base(x, y) { Target = target; Damage = dmg; Speed = speed; ProjColor = SKColors.Yellow; }

        public override void Update(float dt)
        {
            if (_hit || !IsAlive) return;
            if (!Target.IsAlive) { Destroy(); return; }
            float dx = Target.X - X, dy = Target.Y - Y;
            float dist = MathF.Sqrt(dx*dx + dy*dy);
            if (dist < Speed * dt + 5f) { OnHit(Target); _hit = true; Destroy(); return; }
            X += dx / dist * Speed * dt;
            Y += dy / dist * Speed * dt;
        }

        protected abstract void OnHit(Enemy primary);

        public override void Draw(SKCanvas canvas)
        {
            using var p = new SKPaint { Color = ProjColor, IsAntialias = true };
            canvas.DrawCircle(X, Y, 4, p);
        }
    }

    // ── Ok ────────────────────────────────────────────────────
    public class Arrow : Projectile
    {
        public Arrow(float x, float y, Enemy t, int dmg) : base(x, y, t, dmg, 300f)
        { ProjColor = new SKColor(70, 130, 180); }

        protected override void OnHit(Enemy t) => t.TakeDamage(Damage);

        public override void Draw(SKCanvas c)
        {
            float dx = Target.X - X, dy = Target.Y - Y;
            float d = MathF.Sqrt(dx*dx + dy*dy); if (d < 1) return;
            using var p = new SKPaint { Color = ProjColor, IsAntialias = true,
                StrokeWidth = 2.5f, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(X, Y, X + dx/d*14, Y + dy/d*14, p);
        }
    }

    // ── Gülle — alan hasarı ───────────────────────────────────
    public class Cannonball : Projectile
    {
        private readonly float _splash;
        public static event Action<float, float, float>? OnExplosion;

        public Cannonball(float x, float y, Enemy t, int dmg, float splash)
            : base(x, y, t, dmg, 180f) { _splash = splash; ProjColor = new SKColor(200, 100, 50); }

        protected override void OnHit(Enemy t) => OnExplosion?.Invoke(t.X, t.Y, _splash);

        public override void Draw(SKCanvas c)
        {
            using var fill   = new SKPaint { Color = ProjColor, IsAntialias = true };
            using var stroke = new SKPaint { Color = SKColors.Black, IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
            c.DrawCircle(X, Y, 7, fill);
            c.DrawCircle(X, Y, 7, stroke);
        }
    }

    // ── Buz mermisi ───────────────────────────────────────────
    public class IceProjectile : Projectile
    {
        private readonly float _slow;
        public IceProjectile(float x, float y, Enemy t, int dmg, float slow)
            : base(x, y, t, dmg, 220f) { _slow = slow; ProjColor = new SKColor(150, 220, 255); }

        protected override void OnHit(Enemy t) { t.TakeDamage(Damage); t.ApplySlow(_slow, 2.5f); }
    }

    // ── Lazer — anlık hasar ───────────────────────────────────
    public class LaserBeam : Projectile
    {
        public LaserBeam(float x, float y, Enemy t, int dmg) : base(x, y, t, dmg, 9999f)
        { ProjColor = new SKColor(255, 50, 180); }

        protected override void OnHit(Enemy t) => t.TakeDamage(Damage);
        public override void Draw(SKCanvas c) { }  // LaserTower kendisi çiziyor
    }

    // ── Sniper mermisi — iz bırakır ───────────────────────────
    public class SniperBullet : Projectile
    {
        private readonly List<(float x, float y, float a)> _trail = new();

        public SniperBullet(float x, float y, Enemy t, int dmg) : base(x, y, t, dmg, 600f)
        { ProjColor = new SKColor(200, 255, 150); }

        protected override void OnHit(Enemy t) => t.TakeDamage(Damage);

        public override void Update(float dt)
        {
            _trail.Add((X, Y, 1f));
            for (int i = _trail.Count - 1; i >= 0; i--)
            {
                var (tx, ty, a) = _trail[i];
                if (a <= 0) { _trail.RemoveAt(i); continue; }
                _trail[i] = (tx, ty, a - dt * 3f);
            }
            base.Update(dt);
        }

        public override void Draw(SKCanvas c)
        {
            foreach (var (tx, ty, a) in _trail)
            {
                using var p = new SKPaint { Color = ProjColor.WithAlpha((byte)(a * 200)), IsAntialias = true };
                c.DrawCircle(tx, ty, 2f, p);
            }
            using var head = new SKPaint { Color = SKColors.White, IsAntialias = true };
            c.DrawCircle(X, Y, 4f, head);
        }
    }
}
