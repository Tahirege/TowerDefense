using SkiaSharp;

namespace TowerDefense.Enemies
{
    // ── Basic Soldier ─────────────────────────────────────────────
    public class BasicEnemy : Enemy
    {
        public BasicEnemy(float x, float y) : base(x, y, hp: 80, speed: 60f, reward: 10)
        { EnemyName = "Soldier"; EnemyColor = new SKColor(210, 70, 70); Size = 20f; }

        protected override void DrawBody(SKCanvas c)
        {
            float r = Size / 2;

            // Drop shadow
            using var shadow = new SKPaint { Color = new SKColor(0,0,0,55), IsAntialias = true };
            c.DrawOval(X, Y + r + 1, r * 0.85f, r * 0.25f, shadow);

            // Body
            using var body = new SKPaint { Color = EnemyColor, IsAntialias = true };
            c.DrawCircle(X, Y, r, body);

            // Helmet cap (draw upper-half circle using a path — no canvas clip needed)
            using var helmet = new SKPaint { Color = new SKColor(120, 20, 20), IsAntialias = true };
            var helmetPath = new SKPath();
            helmetPath.AddArc(new SKRect(X - r, Y - r, X + r, Y + r), 180, 180);
            helmetPath.Close();
            c.DrawPath(helmetPath, helmet);

            // Helmet brim
            using var brim = new SKPaint { Color = new SKColor(90, 15, 15), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f };
            c.DrawLine(X - r - 1, Y - 1, X + r + 1, Y - 1, brim);

            // Eyes
            using var eyeW = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var eyeB = new SKPaint { Color = new SKColor(30, 0, 0), IsAntialias = true };
            c.DrawCircle(X - 4, Y + 2, 2.5f, eyeW);
            c.DrawCircle(X + 4, Y + 2, 2.5f, eyeW);
            c.DrawCircle(X - 4, Y + 2, 1.3f, eyeB);
            c.DrawCircle(X + 4, Y + 2, 1.3f, eyeB);

            // Outline
            using var outline = new SKPaint { Color = new SKColor(0,0,0,150), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawCircle(X, Y, r, outline);
        }
    }

    // ── Fast Cavalry ──────────────────────────────────────────────
    public class FastEnemy : Enemy
    {
        public FastEnemy(float x, float y) : base(x, y, hp: 40, speed: 130f, reward: 15)
        { EnemyName = "Cavalry"; EnemyColor = new SKColor(255, 165, 0); Size = 16f; }

        protected override void DrawBody(SKCanvas c)
        {
            float r = Size / 2;

            using var shadow = new SKPaint { Color = new SKColor(0,0,0,50), IsAntialias = true };
            c.DrawOval(X, Y + r + 2, r * 1.3f, r * 0.3f, shadow);

            // Horse body (diamond/rhombus shape)
            using var fill = new SKPaint { Color = EnemyColor, IsAntialias = true };
            using var dark = new SKPaint { Color = new SKColor(180, 100, 0), IsAntialias = true };
            var path = new SKPath();
            path.MoveTo(X,       Y - r * 1.2f);   // top
            path.LineTo(X + r,   Y);               // right
            path.LineTo(X,       Y + r);            // bottom
            path.LineTo(X - r,   Y);               // left
            path.Close();
            c.DrawPath(path, fill);

            // Dark shading bottom half
            var pathBot = new SKPath();
            pathBot.MoveTo(X - r, Y);
            pathBot.LineTo(X, Y + r);
            pathBot.LineTo(X + r, Y);
            pathBot.Close();
            c.DrawPath(pathBot, dark);

            // Eye
            using var eyeW = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var eyeB = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            c.DrawCircle(X + 3, Y - 2, 2.5f, eyeW);
            c.DrawCircle(X + 3, Y - 2, 1.2f, eyeB);

            // Speed lines
            using var speedLine = new SKPaint { Color = new SKColor(255, 220, 100, 150), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f };
            c.DrawLine(X - r - 5, Y - 2, X - r - 1, Y - 2, speedLine);
            c.DrawLine(X - r - 4, Y + 2, X - r, Y + 2, speedLine);

            using var outline = new SKPaint { Color = new SKColor(0,0,0,130), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.3f };
            c.DrawPath(path, outline);
        }
    }

    // ── Boss ──────────────────────────────────────────────────────
    public class BossEnemy : Enemy
    {
        private float _pulse;
        public BossEnemy(float x, float y) : base(x, y, hp: 600, speed: 30f, reward: 100)
        { EnemyName = "BOSS"; EnemyColor = new SKColor(160, 0, 220); Size = 36f; }

        public override void Update(float dt) { base.Update(dt); _pulse += dt; }

        protected override void DrawBody(SKCanvas c)
        {
            float ps = Size * (1f + 0.07f * MathF.Sin(_pulse * 4f));
            float r = ps / 2;

            // Outer aura rings
            float auraA = (byte)(40 + 20 * MathF.Sin(_pulse * 3f));
            using var aura2 = new SKPaint { Color = new SKColor(200, 0, 255, (byte)auraA), IsAntialias = true };
            using var aura1 = new SKPaint { Color = new SKColor(140, 0, 200, (byte)(auraA + 25)), IsAntialias = true };
            c.DrawCircle(X, Y, r + 12, aura2);
            c.DrawCircle(X, Y, r + 6, aura1);

            // Body gradient-like
            using var bodyFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X - r * 0.3f, Y - r * 0.3f), r * 1.1f,
                    new SKColor[] { new SKColor(200, 80, 255), new SKColor(80, 0, 160) },
                    null, SKShaderTileMode.Clamp)
            };
            c.DrawCircle(X, Y, r, bodyFill);

            // Crown
            using var crownP = new SKPaint { Color = new SKColor(255, 215, 0), IsAntialias = true };
            var crown = new SKPath();
            float cx = X, cy = Y - r + 2;
            crown.MoveTo(cx - 10, cy + 6);
            crown.LineTo(cx - 10, cy - 2);
            crown.LineTo(cx - 6,  cy + 1);
            crown.LineTo(cx,      cy - 5);
            crown.LineTo(cx + 6,  cy + 1);
            crown.LineTo(cx + 10, cy - 2);
            crown.LineTo(cx + 10, cy + 6);
            crown.Close();
            c.DrawPath(crown, crownP);

            // Gold outline
            using var goldRing = new SKPaint { Color = new SKColor(255, 220, 0), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f };
            c.DrawCircle(X, Y, r, goldRing);

            // Eyes
            using var eyeGlow = new SKPaint { Color = new SKColor(255, 100, 0), IsAntialias = true };
            using var eyePupil = new SKPaint { Color = new SKColor(255, 0, 0), IsAntialias = true };
            c.DrawCircle(X - 7, Y + 3, 4.5f, eyeGlow);
            c.DrawCircle(X + 7, Y + 3, 4.5f, eyeGlow);
            c.DrawCircle(X - 7, Y + 3, 2.5f, eyePupil);
            c.DrawCircle(X + 7, Y + 3, 2.5f, eyePupil);

        }

        public override void TakeDamage(int dmg)
        { if (Hp > MaxHp / 2) dmg = (int)(dmg * 0.7f); base.TakeDamage(dmg); }
    }

    // ── Armored Enemy ─────────────────────────────────────────────
    public class ArmoredEnemy : Enemy
    {
        public ArmoredEnemy(float x, float y) : base(x, y, hp: 200, speed: 45f, reward: 30)
        { EnemyName = "Armored"; EnemyColor = new SKColor(130, 130, 150); Size = 24f; }

        public override void TakeDamage(int dmg) => base.TakeDamage(Math.Max(1, dmg - 10));

        protected override void DrawBody(SKCanvas c)
        {
            float r = Size / 2;

            using var shadow = new SKPaint { Color = new SKColor(0,0,0,60), IsAntialias = true };
            c.DrawOval(X, Y + r + 1, r * 0.9f, r * 0.25f, shadow);

            // Armor body
            using var armorFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X - r * 0.3f, Y - r * 0.3f), r * 1.2f,
                    new SKColor[] { new SKColor(200, 200, 220), new SKColor(80, 80, 100) },
                    null, SKShaderTileMode.Clamp)
            };
            c.DrawCircle(X, Y, r, armorFill);

            // Armor plate lines
            using var plateP = new SKPaint { Color = new SKColor(60, 60, 80, 180), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawLine(X - r * 0.6f, Y - r * 0.2f, X + r * 0.6f, Y - r * 0.2f, plateP);
            c.DrawLine(X - r * 0.5f, Y + r * 0.2f, X + r * 0.5f, Y + r * 0.2f, plateP);

            // Shield icon (center cross shape)
            using var shieldP = new SKPaint { Color = new SKColor(255, 230, 100), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(X, Y - 5, X, Y + 5, shieldP);
            c.DrawLine(X - 4, Y, X + 4, Y, shieldP);

            // Outer ring
            using var ring = new SKPaint { Color = new SKColor(180, 180, 200), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f };
            c.DrawCircle(X, Y, r, ring);

            // Shine
            using var shine = new SKPaint { Color = new SKColor(255, 255, 255, 80), IsAntialias = true };
            c.DrawCircle(X - r * 0.35f, Y - r * 0.35f, r * 0.3f, shine);
        }
    }

    // ── Healer Enemy ──────────────────────────────────────────────
    public class HealerEnemy : Enemy
    {
        private float _healTimer;
        private float _glowPulse;

        public HealerEnemy(float x, float y) : base(x, y, hp: 120, speed: 50f, reward: 40)
        { EnemyName = "Healer"; EnemyColor = new SKColor(60, 210, 110); Size = 22f; }

        public List<Enemy>? NearbyEnemies { get; set; }

        public override void Update(float dt)
        {
            base.Update(dt);
            _glowPulse += dt;
            _healTimer += dt;
            if (_healTimer >= 2f)
            {
                _healTimer = 0f;
                if (NearbyEnemies != null)
                    foreach (var e in NearbyEnemies.Where(e => e.IsAlive && e != this))
                    {
                        float dx = e.X - X, dy = e.Y - Y;
                        if (MathF.Sqrt(dx*dx + dy*dy) < 80f) e.Heal(15);
                    }
            }
        }

        protected override void DrawBody(SKCanvas c)
        {
            float r = Size / 2;
            float glow = 0.6f + 0.4f * MathF.Sin(_glowPulse * 3f);

            using var shadow = new SKPaint { Color = new SKColor(0,0,0,50), IsAntialias = true };
            c.DrawOval(X, Y + r + 1, r * 0.9f, r * 0.25f, shadow);

            // Healing aura
            using var aura = new SKPaint { Color = new SKColor(60, 220, 120, (byte)(40 * glow)), IsAntialias = true };
            c.DrawCircle(X, Y, r + 7, aura);

            // Body
            using var bodyFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X - r * 0.3f, Y - r * 0.3f), r * 1.1f,
                    new SKColor[] { new SKColor(100, 240, 150), new SKColor(20, 130, 70) },
                    null, SKShaderTileMode.Clamp)
            };
            c.DrawCircle(X, Y, r, bodyFill);

            // Cross symbol
            using var crossP = new SKPaint { Color = SKColors.White, IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(X, Y - 7, X, Y + 7, crossP);
            c.DrawLine(X - 5, Y, X + 5, Y, crossP);

            using var outline = new SKPaint { Color = new SKColor(10, 80, 40, 170), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawCircle(X, Y, r, outline);

            using var shine = new SKPaint { Color = new SKColor(200, 255, 220, 80), IsAntialias = true };
            c.DrawCircle(X - r * 0.3f, Y - r * 0.3f, r * 0.28f, shine);
        }
    }

    // ── Flying Enemy ──────────────────────────────────────────────
    public class FlyingEnemy : Enemy
    {
        private float _wingTimer;
        private List<(float x, float y)>? _directPath;

        public FlyingEnemy(float x, float y) : base(x, y, hp: 60, speed: 90f, reward: 25)
        { EnemyName = "Eagle"; EnemyColor = new SKColor(80, 170, 255); Size = 18f; }

        public void SetDirectPath(float endX, float endY)
        {
            int steps = 40;
            var path = new List<(float, float)>();
            for (int i = 0; i <= steps; i++)
                path.Add((X + (endX - X) * i / steps, Y + (endY - Y) * i / steps));
            _directPath = path;
            SetPath(_directPath);
        }

        public override void Update(float dt) { base.Update(dt); _wingTimer += dt; }

        protected override void DrawBody(SKCanvas c)
        {
            float wing = MathF.Sin(_wingTimer * 9f) * 8f;
            float r = Size / 2;

            // Shadow on ground
            using var shadow = new SKPaint { Color = new SKColor(0,0,0,40), IsAntialias = true };
            c.DrawOval(X, Y + r + 8, r * 1.4f, r * 0.3f, shadow);

            // Wings
            using var wingFill = new SKPaint { Color = new SKColor(150, 210, 255, 200), IsAntialias = true };
            using var wingDark = new SKPaint { Color = new SKColor(50, 130, 200, 160), IsAntialias = true };
            using var featherLine = new SKPaint { Color = new SKColor(80, 160, 220, 120), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1f };

            var leftWing = new SKPath();
            leftWing.MoveTo(X - 3, Y);
            leftWing.LineTo(X - 18, Y - wing);
            leftWing.LineTo(X - 14, Y + 5);
            leftWing.LineTo(X - 8,  Y + 6);
            leftWing.Close();

            var rightWing = new SKPath();
            rightWing.MoveTo(X + 3, Y);
            rightWing.LineTo(X + 18, Y - wing);
            rightWing.LineTo(X + 14, Y + 5);
            rightWing.LineTo(X + 8,  Y + 6);
            rightWing.Close();

            c.DrawPath(leftWing,  wingFill);
            c.DrawPath(rightWing, wingFill);
            c.DrawPath(leftWing,  wingDark);
            c.DrawPath(rightWing, wingDark);

            // Feather lines on wings
            c.DrawLine(X - 3, Y, X - 14, Y - wing * 0.7f, featherLine);
            c.DrawLine(X + 3, Y, X + 14, Y - wing * 0.7f, featherLine);

            // Bird body
            using var bodyFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X - 2, Y - 2), r * 1.1f,
                    new SKColor[] { new SKColor(130, 200, 255), new SKColor(30, 100, 200) },
                    null, SKShaderTileMode.Clamp)
            };
            c.DrawCircle(X, Y, r, bodyFill);

            // Beak
            using var beak = new SKPaint { Color = new SKColor(255, 200, 50), IsAntialias = true };
            var beakPath = new SKPath();
            beakPath.MoveTo(X + r - 1, Y);
            beakPath.LineTo(X + r + 5, Y - 1);
            beakPath.LineTo(X + r - 1, Y + 3);
            beakPath.Close();
            c.DrawPath(beakPath, beak);

            // Eye
            using var eyeW = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var eyeB = new SKPaint { Color = new SKColor(10, 10, 40), IsAntialias = true };
            c.DrawCircle(X + 3, Y - 2, 2.8f, eyeW);
            c.DrawCircle(X + 4, Y - 2, 1.5f, eyeB);

            using var outline = new SKPaint { Color = new SKColor(0,0,0,120), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f };
            c.DrawCircle(X, Y, r, outline);
        }
    }

    // ── Bomber Enemy: targets nearest tower, explodes on contact ──
    public class BomberEnemy : Enemy
    {
        private float _fuseTimer = 0f;
        public float ExplosionRadius { get; } = 90f;
        public int   ExplosionDamage { get; } = 40;
        public bool  HasExploded     { get; private set; } = false;

        // Set by GameManager each frame — position of nearest tower
        public (float x, float y)? CurrentTowerTarget { get; set; }
        private const float ExplodeRange = 36f;

        // Callback to GameManager explosion handler
        public Action<float, float, float, int>? OnBomberExplode { get; set; }

        public BomberEnemy(float x, float y) : base(x, y, hp: 90, speed: 60f, reward: 35)
        {
            EnemyName  = "Bomber";
            EnemyColor = new SKColor(80, 160, 50);
            Size = 22f;
        }

        public override void Update(float dt)
        {
            _fuseTimer += dt;
            base.Update(dt);

            // If killed by damage (not self-destruct, not reaching end), explode
            if (!IsAlive && !HasExploded && !ReachedEnd)
            {
                HasExploded = true;
                OnBomberExplode?.Invoke(X, Y, ExplosionRadius, ExplosionDamage);
            }
        }

        protected override void MoveAlongPath(float dt)
        {
            if (!CurrentTowerTarget.HasValue)
            {
                base.MoveAlongPath(dt);
                return;
            }

            var (tx, ty) = CurrentTowerTarget.Value;
            float dx = tx - X, dy = ty - Y;
            float dist = MathF.Sqrt(dx*dx + dy*dy);

            if (dist <= ExplodeRange)
            {
                // Reached tower — self-destruct!
                HasExploded = true;
                OnBomberExplode?.Invoke(X, Y, ExplosionRadius, ExplosionDamage);
                Destroy();
                return;
            }

            // Rush toward tower with 25% speed boost
            float speed = CurrentSpeed * 1.25f;
            X += (dx / dist) * speed * dt;
            Y += (dy / dist) * speed * dt;
        }

        protected override void DrawBody(SKCanvas c)
        {
            float r = Size / 2;

            // Ground shadow
            using var shadow = new SKPaint { Color = new SKColor(0,0,0,55), IsAntialias = true };
            c.DrawOval(X, Y + r + 1, r * 0.9f, r * 0.25f, shadow);

            // Danger stripes background (alternating dark/bright green)
            float stripeAngle = _fuseTimer * 60f;
            using var stripe1 = new SKPaint { Color = new SKColor(60, 130, 30), IsAntialias = true };
            using var stripe2 = new SKPaint { Color = new SKColor(30, 80, 15), IsAntialias = true };
            c.DrawCircle(X, Y, r, stripe1);
            // Draw 3 dark stripes over the base
            for (int i = 0; i < 3; i++)
            {
                float sa = (i * 120f + stripeAngle) * MathF.PI / 180f;
                var stripePath = new SKPath();
                stripePath.MoveTo(X + MathF.Cos(sa) * r, Y + MathF.Sin(sa) * r);
                stripePath.ArcTo(new SKRect(X - r, Y - r, X + r, Y + r), sa * 180f / MathF.PI, 40, false);
                stripePath.LineTo(X, Y);
                stripePath.Close();
                c.DrawPath(stripePath, stripe2);
            }

            // Main body gradient
            using var bodyFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X - 3, Y - 3), r * 1.1f,
                    new SKColor[] { new SKColor(110, 190, 70), new SKColor(40, 100, 20) },
                    null, SKShaderTileMode.Clamp)
            };
            c.DrawCircle(X, Y, r, bodyFill);

            // Bomb outline
            using var bombOutline = new SKPaint { Color = new SKColor(20, 60, 10), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            c.DrawCircle(X, Y, r, bombOutline);

            // Warning symbol ⚠ (triangle + exclamation)
            using var warnP = new SKPaint { Color = new SKColor(255, 220, 0), IsAntialias = true };
            var warnPath = new SKPath();
            warnPath.MoveTo(X,       Y - 7);
            warnPath.LineTo(X + 6,   Y + 5);
            warnPath.LineTo(X - 6,   Y + 5);
            warnPath.Close();
            c.DrawPath(warnPath, warnP);
            using var exclP = new SKPaint { Color = new SKColor(40, 20, 0), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(X, Y - 3, X, Y + 1, exclP);
            using var dotP = new SKPaint { Color = new SKColor(40, 20, 0), IsAntialias = true };
            c.DrawCircle(X, Y + 3.5f, 1f, dotP);

            // Fuse (animated spark)
            float fuseFlicker = MathF.Sin(_fuseTimer * 15f);
            using var fuseP = new SKPaint { Color = new SKColor(160, 130, 60), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(X, Y - r + 1, X + 3, Y - r - 7, fuseP);
            using var sparkP = new SKPaint
            {
                Color = fuseFlicker > 0 ? new SKColor(255, 230, 0) : new SKColor(255, 100, 0),
                IsAntialias = true
            };
            c.DrawCircle(X + 3, Y - r - 7, 2.5f, sparkP);

            // Shine
            using var shine = new SKPaint { Color = new SKColor(200, 255, 180, 80), IsAntialias = true };
            c.DrawCircle(X - r * 0.35f, Y - r * 0.35f, r * 0.28f, shine);
        }
    }
}
