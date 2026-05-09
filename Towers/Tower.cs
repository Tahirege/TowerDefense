using SkiaSharp;
using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.Exceptions;
using TowerDefense.Projectiles;

namespace TowerDefense.Towers
{
    public abstract class Tower : GameObject, IUpgradeable
    {
        private float _range;
        private int   _damage;
        private float _fireRate;
        private float _cooldown;
        private int   _level = 1;
        private int   _upgradeCost;
        private int   _sellValue;

        // ── Stats ─────────────────────────────────────────────────
        public float Range     { get => _range;    protected set => _range    = value; }
        public int   Damage    { get => _damage;   protected set => _damage   = value; }
        public float FireRate  { get => _fireRate; protected set => _fireRate = value; }
        public int   Cost      { get; }
        public int   Level     => _level;
        public int   UpgradeCost => _upgradeCost;
        public int   SellValue   => _sellValue;
        public string  TowerName  { get; protected set; } = "Tower";
        public SKColor TowerColor { get; protected set; } = SKColors.Gray;
        public bool    IsSelected { get; set; }
        public ITargetingStrategy Strategy { get; set; } = new FirstTargeting();

        // ── Health ────────────────────────────────────────────────
        private int _maxHealth;
        private int _health;
        public int MaxHealth => _maxHealth;
        public int Health    => _health;

        protected Tower(float x, float y, int cost, float range, int damage, float fireRate)
            : base(x, y)
        {
            Cost = cost; _range = range; _damage = damage;
            _fireRate = fireRate; _upgradeCost = cost; _sellValue = cost / 2;

            // Health scales with cost (cheap = 80, expensive = 200)
            _maxHealth = 80 + cost / 5;
            _health    = _maxHealth;
        }

        public void TakeDamage(int dmg)
        {
            _health -= dmg;
            if (_health <= 0) { _health = 0; Destroy(); }
        }

        protected abstract Projectile CreateProjectile(Enemy target);

        public override void Update(float dt) { if (_cooldown > 0) _cooldown -= dt; }

        public override void Draw(SKCanvas canvas)
        {
            const float s = 34f;
            const float r = 6f;

            // Range ring when selected
            if (IsSelected)
            {
                using var rc = new SKPaint { Color = TowerColor.WithAlpha(40), IsAntialias = true };
                using var rl = new SKPaint { Color = TowerColor.WithAlpha(160), IsAntialias = true,
                    Style = SKPaintStyle.Stroke, StrokeWidth = 2.2f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 8, 4 }, 0) };
                canvas.DrawCircle(X, Y, _range, rc);
                canvas.DrawCircle(X, Y, _range, rl);
            }

            var rect = new SKRoundRect(new SKRect(X - s/2, Y - s/2, X + s/2, Y + s/2), r, r);

            // Shadow
            using (var shadow = new SKPaint { Color = new SKColor(0,0,0,80), IsAntialias = true })
            {
                var sr = new SKRoundRect(new SKRect(X - s/2+2, Y - s/2+3, X + s/2+2, Y + s/2+3), r, r);
                canvas.DrawRoundRect(sr, shadow);
            }

            // Base fill — colour shifts to red when damaged
            float healthPct = (float)_health / _maxHealth;
            var baseFill = healthPct > 0.5f
                ? new SKColor(30, 35, 45)
                : new SKColor((byte)(30 + (1f - healthPct) * 80), 20, 20);

            using (var fill = new SKPaint { Color = baseFill, IsAntialias = true })
                canvas.DrawRoundRect(rect, fill);

            // Accent border — dims with damage
            byte borderAlpha = (byte)(255 * healthPct + 80 * (1f - healthPct));
            using (var border = new SKPaint { Color = TowerColor.WithAlpha(borderAlpha), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f })
                canvas.DrawRoundRect(rect, border);

            // Inner highlight
            using (var shine = new SKPaint { Color = TowerColor.WithAlpha(40), IsAntialias = true })
            {
                var shineRect = new SKRoundRect(new SKRect(X - s/2+3, Y - s/2+3, X + s/2-3, Y), 3, 3);
                canvas.DrawRoundRect(shineRect, shine);
            }

            // Level badge
            using var badgeFill = new SKPaint { Color = TowerColor.WithAlpha(200), IsAntialias = true };
            canvas.DrawCircle(X - s/2 + 8, Y - s/2 + 8, 7, badgeFill);
            using var lvlText = new SKPaint { Color = SKColors.White, IsAntialias = true,
                TextSize = 9f, TextAlign = SKTextAlign.Center, FakeBoldText = true };
            canvas.DrawText($"{_level}", X - s/2 + 8, Y - s/2 + 12, lvlText);

            DrawTop(canvas, s);
            DrawHealthBar(canvas, s);
        }

        private void DrawHealthBar(SKCanvas canvas, float s)
        {
            if (_health >= _maxHealth) return;
            
            float bw = s, bh = 4;
            float bx = X - bw/2, by = Y - s/2 - 8;
            float fillPct = (float)_health / _maxHealth;

            using var bgTrack = new SKPaint { Color = new SKColor(30, 30, 30, 180), IsAntialias = true };
            canvas.DrawRect(bx, by, bw, bh, bgTrack);

            var fillColor = fillPct > 0.6f ? new SKColor(50, 220, 80)
                          : fillPct > 0.3f ? new SKColor(255, 180, 30)
                                           : new SKColor(255, 50, 50);
            using var fg = new SKPaint { Color = fillColor, IsAntialias = true };
            if (fillPct > 0)
                canvas.DrawRect(bx, by, bw * fillPct, bh, fg);
        }

        protected virtual void DrawTop(SKCanvas canvas, float s) { }

        protected virtual Enemy? FindTarget(List<Enemy> enemies) =>
            Strategy.SelectTarget(enemies, X, Y, _range);


        public Projectile? TryShoot(List<Enemy> enemies)
        {
            if (_cooldown > 0) return null;
            var target = FindTarget(enemies);
            if (target == null) return null;
            _cooldown = 1f / _fireRate;
            return CreateProjectile(target);
        }

        private float Dist(Enemy e) { float dx = e.X-X, dy = e.Y-Y; return MathF.Sqrt(dx*dx+dy*dy); }

        public void Upgrade()
        {
            if (_level >= 3) throw new GameException("Max level!");
            _level++;
            _damage   = (int)(_damage   * 1.4f);
            _range    *= 1.15f;
            _fireRate *= 1.2f;
            _sellValue  += _upgradeCost / 2;
            _upgradeCost = (int)(_upgradeCost * 1.5f);
            // Heal on upgrade
            _maxHealth = (int)(_maxHealth * 1.25f);
            _health    = _maxHealth;
            OnUpgrade();
        }

        protected virtual void OnUpgrade() { }
        public void Sell() => Destroy();
    }
}
