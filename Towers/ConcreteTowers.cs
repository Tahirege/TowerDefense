using SkiaSharp;
using TowerDefense.Enemies;
using TowerDefense.Projectiles;

namespace TowerDefense.Towers
{
    // ── Arrow Tower: fast, single target ─────────────────────────
    public class ArrowTower : Tower
    {
        public ArrowTower(float x, float y) : base(x, y, 100, 110f, 15, 1.5f)
        { TowerName = "Arrow Tower"; TowerColor = new SKColor(70, 140, 220); }

        protected override Projectile CreateProjectile(Enemy t) => new Arrow(X, Y, t, Damage);

        protected override void DrawTop(SKCanvas c, float s)
        {
            // Bow shape: arc + arrow line
            using var bowP = new SKPaint { Color = new SKColor(150, 210, 255), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f, StrokeCap = SKStrokeCap.Round };
            using var arrowP = new SKPaint { Color = SKColors.White, IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round };

            // Bow arc
            var arcRect = new SKRect(X - 6, Y - 9, X + 6, Y + 9);
            c.DrawArc(arcRect, -80, 160, false, bowP);

            // Arrow shaft
            c.DrawLine(X - 9, Y, X + 9, Y, arrowP);
            // Arrowhead
            c.DrawLine(X + 6, Y - 4, X + 9, Y, arrowP);
            c.DrawLine(X + 6, Y + 4, X + 9, Y, arrowP);
        }
    }

    // ── Cannon Tower: area damage ──────────────────────────────────
    public class CannonTower : Tower
    {
        public CannonTower(float x, float y) : base(x, y, 200, 90f, 50, 0.5f)
        { TowerName = "Cannon Tower"; TowerColor = new SKColor(200, 90, 50); }

        protected override Projectile CreateProjectile(Enemy t) => new Cannonball(X, Y, t, Damage, 40f);

        protected override void DrawTop(SKCanvas c, float s)
        {
            // Cannon barrel
            using var barrel = new SKPaint { Color = new SKColor(60, 60, 70), IsAntialias = true };
            using var barrelHL = new SKPaint { Color = new SKColor(100, 100, 120, 160), IsAntialias = true };
            using var ball = new SKPaint { Color = new SKColor(30, 30, 35), IsAntialias = true };
            using var ballHL = new SKPaint { Color = new SKColor(80, 80, 90, 120), IsAntialias = true };

            // Barrel body
            var barrelPath = new SKPath();
            barrelPath.MoveTo(X - 3, Y + 3);
            barrelPath.LineTo(X - 3, Y - 1);
            barrelPath.LineTo(X + 12, Y - 3);
            barrelPath.LineTo(X + 12, Y + 3);
            barrelPath.Close();
            c.DrawPath(barrelPath, barrel);
            // Barrel highlight
            c.DrawLine(X - 2, Y - 0.5f, X + 11, Y - 2f, barrelHL);

            // Cannonball at base
            c.DrawCircle(X - 1, Y + 1, 5, ball);
            c.DrawCircle(X - 2.5f, Y - 0.5f, 2, ballHL);
        }
    }

    // ── Ice Tower: slows enemies ───────────────────────────────────
    public class IceTower : Tower
    {
        public float SlowFactor { get; private set; } = 0.4f;
        public IceTower(float x, float y) : base(x, y, 150, 100f, 8, 0.8f)
        { TowerName = "Ice Tower"; TowerColor = new SKColor(100, 210, 255); }

        protected override Projectile CreateProjectile(Enemy t) => new IceProjectile(X, Y, t, Damage, SlowFactor);
        protected override void OnUpgrade() => SlowFactor = Math.Max(0.15f, SlowFactor - 0.08f);

        protected override void DrawTop(SKCanvas c, float s)
        {
            // Snowflake / crystal
            using var outer = new SKPaint { Color = new SKColor(180, 240, 255), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.8f, StrokeCap = SKStrokeCap.Round };
            using var inner = new SKPaint { Color = new SKColor(220, 250, 255, 200), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1f, StrokeCap = SKStrokeCap.Round };

            // 6 main arms
            for (int i = 0; i < 6; i++)
            {
                double a = i * Math.PI / 3;
                float ex = X + (float)(Math.Cos(a) * 10);
                float ey = Y + (float)(Math.Sin(a) * 10);
                c.DrawLine(X, Y, ex, ey, outer);

                // Barbs on each arm
                double bAngle1 = a + Math.PI / 4;
                double bAngle2 = a - Math.PI / 4;
                float mx = X + (float)(Math.Cos(a) * 6);
                float my = Y + (float)(Math.Sin(a) * 6);
                c.DrawLine(mx, my, mx + (float)(Math.Cos(bAngle1) * 4), my + (float)(Math.Sin(bAngle1) * 4), inner);
                c.DrawLine(mx, my, mx + (float)(Math.Cos(bAngle2) * 4), my + (float)(Math.Sin(bAngle2) * 4), inner);
            }
            // Center gem
            using var gem = new SKPaint { Color = new SKColor(200, 245, 255), IsAntialias = true };
            c.DrawCircle(X, Y, 3, gem);
        }
    }

    // ── Laser Tower: continuous damage ────────────────────────────
    public class LaserTower : Tower
    {
        private Enemy? _currentTarget;
        private float _beamPulse;

        public LaserTower(float x, float y) : base(x, y, 250, 130f, 5, 10f)
        { TowerName = "Laser Tower"; TowerColor = new SKColor(255, 60, 200); }

        protected override Projectile CreateProjectile(Enemy t) { _currentTarget = t; return new LaserBeam(X, Y, t, Damage); }

        public override void Draw(SKCanvas canvas)
        {
            _beamPulse += 0.1f;
            base.Draw(canvas);
            if (_currentTarget != null && _currentTarget.IsAlive)
            {
                float pulse = 0.7f + 0.3f * MathF.Sin(_beamPulse * 6f);
                using var outerGlow = new SKPaint { Color = new SKColor(255, 100, 230, (byte)(40 * pulse)),
                    IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 14f };
                using var midGlow = new SKPaint { Color = new SKColor(255, 50, 200, (byte)(90 * pulse)),
                    IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 6f };
                using var beam = new SKPaint { Color = new SKColor(255, 220, 255, (byte)(230 * pulse)),
                    IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
                canvas.DrawLine(X, Y, _currentTarget.X, _currentTarget.Y, outerGlow);
                canvas.DrawLine(X, Y, _currentTarget.X, _currentTarget.Y, midGlow);
                canvas.DrawLine(X, Y, _currentTarget.X, _currentTarget.Y, beam);
            }
            else _currentTarget = null;
        }

        protected override void DrawTop(SKCanvas c, float s)
        {
            // Laser emitter: concentric rings
            using var outer = new SKPaint { Color = new SKColor(255, 60, 200, 100), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            using var inner = new SKPaint { Color = new SKColor(255, 160, 240), IsAntialias = true };
            using var core  = new SKPaint { Color = SKColors.White, IsAntialias = true };
            c.DrawCircle(X, Y, 9, outer);
            c.DrawCircle(X, Y, 6, inner);
            c.DrawCircle(X, Y, 3, core);
        }
    }

    // ── Bomb Tower: huge area, slow ────────────────────────────────
    public class BombTower : Tower
    {
        private float _fuseTimer;

        public BombTower(float x, float y) : base(x, y, 300, 80f, 80, 0.3f)
        { TowerName = "Bomb Tower"; TowerColor = new SKColor(80, 80, 100); }

        public override void Update(float dt) { base.Update(dt); _fuseTimer += dt; }

        protected override Projectile CreateProjectile(Enemy t) => new Cannonball(X, Y, t, Damage, 70f);

        protected override void DrawTop(SKCanvas c, float s)
        {
            // Bomb body
            using var bombFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X - 3, Y - 3), 9,
                    new SKColor[] { new SKColor(90, 90, 110), new SKColor(20, 20, 30) },
                    null, SKShaderTileMode.Clamp)
            };
            using var bombStroke = new SKPaint { Color = new SKColor(120, 120, 140), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawCircle(X, Y + 1, 8, bombFill);
            c.DrawCircle(X, Y + 1, 8, bombStroke);

            // Fuse spark
            float sparkFlicker = MathF.Sin(_fuseTimer * 12f);
            using var fuseP = new SKPaint { Color = new SKColor(160, 140, 100), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(X, Y - 7, X + 4, Y - 12, fuseP);
            // Spark at fuse tip
            using var sparkP = new SKPaint { Color = sparkFlicker > 0 ? new SKColor(255, 220, 0) : new SKColor(255, 100, 0),
                IsAntialias = true };
            c.DrawCircle(X + 4, Y - 12, 2.5f, sparkP);

            // Shine
            using var shine = new SKPaint { Color = new SKColor(200, 200, 220, 100), IsAntialias = true };
            c.DrawCircle(X - 3, Y - 2, 3, shine);
        }
    }

    // ── Sniper Tower: very long range, high damage ─────────────────
    public class SniperTower : Tower
    {
        public SniperTower(float x, float y) : base(x, y, 350, 220f, 120, 0.4f)
        { TowerName = "Sniper Tower"; TowerColor = new SKColor(60, 140, 60); }

        protected override Projectile CreateProjectile(Enemy t) => new SniperBullet(X, Y, t, Damage);

        protected override Enemy? FindTarget(List<Enemy> enemies) =>
            enemies.Where(e => e.IsAlive && Dist(e) <= Range)
                   .OrderByDescending(e => e.Hp).FirstOrDefault();

        private float Dist(Enemy e) { float dx = e.X-X, dy = e.Y-Y; return MathF.Sqrt(dx*dx+dy*dy); }

        protected override void DrawTop(SKCanvas c, float s)
        {
            // Long rifle barrel
            using var barrel = new SKPaint { Color = new SKColor(50, 100, 50), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round };
            using var barrelHL = new SKPaint { Color = new SKColor(130, 200, 130, 150), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1f, StrokeCap = SKStrokeCap.Round };
            using var scope = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X + 4, Y - 2), 5,
                    new SKColor[] { new SKColor(80, 150, 80), new SKColor(30, 70, 30) },
                    null, SKShaderTileMode.Clamp)
            };
            using var scopeR = new SKPaint { Color = new SKColor(100, 180, 100), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f };
            using var crosshair = new SKPaint { Color = new SKColor(200, 255, 200, 160), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 0.8f };

            // Barrel
            c.DrawLine(X - 3, Y, X + 16, Y - 1, barrel);
            c.DrawLine(X - 2, Y - 1, X + 15, Y - 2, barrelHL);

            // Scope body
            c.DrawCircle(X + 4, Y - 2, 5, scope);
            c.DrawCircle(X + 4, Y - 2, 5, scopeR);

            // Scope crosshair
            c.DrawLine(X + 4, Y - 6, X + 4, Y + 2, crosshair);
            c.DrawLine(X + 1, Y - 2, X + 7, Y - 2, crosshair);
        }
    }
}
