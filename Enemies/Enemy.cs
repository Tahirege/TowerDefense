using SkiaSharp;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public abstract class Enemy : GameObject
    {
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public float Speed { get; set; }
        public float SlowTimer { get; set; }
        public float SlowFactor { get; set; } = 1f;
        public int PathIndex { get; set; }

        public int Reward { get; set; }
        public float PathProgress { get; set; }
        public bool ReachedEnd { get; set; }
        public string EnemyName { get; set; } = "Enemy";
        public SKColor EnemyColor { get; set; } = SKColors.Red;
        public float Size { get; set; } = 20f;
        public float CurrentSpeed => Speed * (SlowTimer > 0 ? SlowFactor : 1f);

        public Hero? TargetHero { get; set; }
        public float AttackRange { get; set; } = 35f;
        public int AttackDamage { get; set; } = 5;
        public float AttackCooldown { get; set; } = 1.0f;
        public float AttackTimer { get; set; } = 0f;
        public List<(float x, float y)> Path { get; set; } = new();

        public Enemy(float x, float y, int hp, float speed, int reward)
            : base(x, y)
        {
            MaxHp = hp; Hp = hp; Speed = speed; Reward = reward;
        }

        public void SetPath(List<(float x, float y)> path)
        {
            Path = path;
            PathIndex = 0;
            if (path.Count > 0) { X = path[0].x; Y = path[0].y; }
        }

        public override void Update(float dt)
        {
            if (!IsAlive || ReachedEnd) return;
            if (SlowTimer > 0) SlowTimer -= dt;
            
            if (TargetHero != null && TargetHero.Health > 0)
            {
                float dx = TargetHero.X - X, dy = TargetHero.Y - Y;
                if (MathF.Sqrt(dx*dx + dy*dy) <= AttackRange)
                {
                    if (AttackTimer <= 0) { TargetHero.TakeDamage(AttackDamage); AttackTimer = AttackCooldown; }
                }
            }
            if (AttackTimer > 0) AttackTimer -= dt;
            MoveAlongPath(dt);
        }

        public virtual void MoveAlongPath(float dt)
        {
            if (PathIndex >= Path.Count - 1) { ReachedEnd = true; Destroy(); return; }
            var (tx, ty) = Path[PathIndex + 1];
            float dx = tx - X, dy = ty - Y;
            float dist = MathF.Sqrt(dx*dx + dy*dy), move = CurrentSpeed * dt;
            if (dist <= move) { X = tx; Y = ty; PathIndex++; PathProgress = (float)PathIndex / Path.Count; }
            else { X += dx / dist * move; Y += dy / dist * move; }
        }

        public virtual void TakeDamage(int dmg)
        {
            Hp -= dmg;
            if (Hp <= 0) { Hp = 0; Destroy(); }
        }

        public void Heal(int amount) => Hp = Math.Min(MaxHp, Hp + amount);

        public void ApplySlow(float factor, float duration) { SlowFactor = factor; SlowTimer = duration; }

        public override void Draw(SKCanvas canvas)
        {
            EnemyRenderer.Draw(canvas, this);
        }
    }
}
