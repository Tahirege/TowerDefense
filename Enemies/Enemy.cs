using SkiaSharp;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public abstract class Enemy : GameObject
    {
        private int _maxHp;
        private int _hp;
        private float _speed;
        protected float _slowTimer;
        protected float _slowFactor = 1f;
        protected int _pathIndex;

        public int MaxHp => _maxHp;
        public int Hp => _hp;
        public int Reward { get; }
        public float PathProgress { get; private set; }
        public bool ReachedEnd { get; private set; }
        public string EnemyName { get; protected set; } = "Enemy";
        public SKColor EnemyColor { get; protected set; } = SKColors.Red;
        public float Size { get; protected set; } = 20f;
        public float CurrentSpeed => _speed * (_slowTimer > 0 ? _slowFactor : 1f);

        public Hero? TargetHero { get; set; }
        public float AttackRange { get; protected set; } = 35f;
        public int AttackDamage { get; protected set; } = 5;
        public float AttackCooldown { get; protected set; } = 1.0f;
        protected float _attackTimer = 0f;
        protected List<(float x, float y)> _path = new();

        protected Enemy(float x, float y, int hp, float speed, int reward)
            : base(x, y)
        {
            _maxHp = hp; _hp = hp; _speed = speed; Reward = reward;
        }

        public void SetPath(List<(float x, float y)> path)
        {
            _path = path;
            _pathIndex = 0;
            if (path.Count > 0) { X = path[0].x; Y = path[0].y; }
        }

        public override void Update(float dt)
        {
            if (!IsAlive || ReachedEnd) return;
            if (_slowTimer > 0) _slowTimer -= dt;
            
            // Hero-attack logic while passing by
            if (TargetHero != null && TargetHero.Health > 0)
            {
                float dx = TargetHero.X - X;
                float dy = TargetHero.Y - Y;
                float dist = MathF.Sqrt(dx * dx + dy * dy);

                if (dist <= AttackRange)
                {
                    if (_attackTimer <= 0)
                    {
                        TargetHero.TakeDamage(AttackDamage);
                        _attackTimer = AttackCooldown;
                    }
                }
            }
            if (_attackTimer > 0) _attackTimer -= dt;

            MoveAlongPath(dt);
        }

        protected virtual void MoveAlongPath(float dt)
        {
            if (_pathIndex >= _path.Count - 1) { ReachedEnd = true; Destroy(); return; }
            var (tx, ty) = _path[_pathIndex + 1];
            float dx = tx - X, dy = ty - Y;
            float dist = MathF.Sqrt(dx*dx + dy*dy);
            float move = CurrentSpeed * dt;
            if (dist <= move) { X = tx; Y = ty; _pathIndex++; PathProgress = (float)_pathIndex / _path.Count; }
            else { X += dx / dist * move; Y += dy / dist * move; }
        }

        public virtual void TakeDamage(int dmg)
        {
            _hp -= dmg;
            if (_hp <= 0) { _hp = 0; Destroy(); }
        }

        public void Heal(int amount)
        {
            _hp = Math.Min(_maxHp, _hp + amount);
        }

        public void ApplySlow(float factor, float duration)
        { _slowFactor = factor; _slowTimer = duration; }

        public override void Draw(SKCanvas canvas)
        {
            // Ice slow aura
            if (_slowTimer > 0)
            {
                using var iceAura = new SKPaint { Color = new SKColor(100, 180, 255, 60),
                    IsAntialias = true, Style = SKPaintStyle.Fill };
                using var iceRing = new SKPaint { Color = new SKColor(150, 210, 255, 120),
                    IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
                canvas.DrawCircle(X, Y, Size/2 + 5, iceAura);
                canvas.DrawCircle(X, Y, Size/2 + 5, iceRing);
            }

            DrawBody(canvas);
            DrawHealthBar(canvas);
        }

        protected virtual void DrawBody(SKCanvas canvas)
        {
            // Shadow
            using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 60), IsAntialias = true };
            canvas.DrawOval(X, Y + Size/2 - 1, Size/2 * 0.9f, Size/6, shadow);

            using var fill = new SKPaint { Color = EnemyColor, IsAntialias = true, Style = SKPaintStyle.Fill };
            using var stroke = new SKPaint { Color = SKColors.Black.WithAlpha(150), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            canvas.DrawCircle(X, Y, Size/2, fill);
            canvas.DrawCircle(X, Y, Size/2, stroke);

            // Shine highlight
            using var shine = new SKPaint { Color = new SKColor(255, 255, 255, 70), IsAntialias = true };
            canvas.DrawCircle(X - Size/5, Y - Size/5, Size/5, shine);
        }

        private void DrawHealthBar(SKCanvas canvas)
        {
            float bw = Size + 6, bh = 5;
            float bx = X - bw/2, by = Y - Size/2 - 12;
            float fillPct = (float)_hp / _maxHp;

            // Background track
            using var bgTrack = new SKPaint { Color = new SKColor(30, 30, 30, 200), IsAntialias = true };
            var trackRect = new SKRoundRect(new SKRect(bx - 1, by - 1, bx + bw + 1, by + bh + 1), 3, 3);
            canvas.DrawRoundRect(trackRect, bgTrack);

            // Health fill
            var fillColor = fillPct > 0.6f ? new SKColor(50, 220, 80)
                          : fillPct > 0.3f ? new SKColor(255, 180, 30)
                                           : new SKColor(255, 50, 50);
            using var fg = new SKPaint { Color = fillColor, IsAntialias = true };
            if (fillPct > 0)
            {
                var fillRect = new SKRoundRect(new SKRect(bx, by, bx + bw * fillPct, by + bh), 2, 2);
                canvas.DrawRoundRect(fillRect, fg);
            }
        }
    }
}
