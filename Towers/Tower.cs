using SkiaSharp;
using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.Exceptions;
using TowerDefense.Shots;

namespace TowerDefense.Towers
{
    public abstract class Tower : GameObject, IUpgradeable
    {
        public float Range { get; set; }
        public int Damage { get; set; }
        public float FireRate { get; set; }
        public float Cooldown { get; set; }
        public int Level { get; set; } = 1;
        public int UpgradeCost { get; set; }
        public int SellValue { get; set; }
        public int Cost { get; }
        public string TowerName { get; set; } = "Tower";
        public SKColor TowerColor { get; set; } = SKColors.Gray;
        public bool IsSelected { get; set; }
        public int MaxHealth { get; set; }
        public int Health { get; set; }

        public Tower(float x, float y, int cost, float range, int damage, float fireRate)
            : base(x, y)
        {
            Cost = cost; Range = range; Damage = damage;
            FireRate = fireRate; UpgradeCost = cost; SellValue = cost / 2;
            MaxHealth = 80 + cost / 5; Health = MaxHealth;
        }

        public void TakeDamage(int dmg)
        {
            Health -= dmg;
            if (Health <= 0) { Health = 0; Destroy(); }
        }

        public abstract Shot CreateShot(Enemy target);

        public override void Update(float dt) { if (Cooldown > 0) Cooldown -= dt; }

        public override void Draw(SKCanvas canvas) => TowerRenderer.Draw(this, canvas);

        public virtual Enemy? FindTarget(List<Enemy> enemies)
        {
            return enemies.Where(e => e.IsAlive)
                   .Select(e => new { e, d = MathF.Sqrt(MathF.Pow(e.X - X, 2) + MathF.Pow(e.Y - Y, 2)) })
                   .Where(x => x.d <= Range)
                   .OrderBy(x => x.d)
                   .Select(x => x.e)
                   .FirstOrDefault();
        }

        public Shot? TryShoot(List<Enemy> enemies)
        {
            if (Cooldown > 0) return null;
            var target = FindTarget(enemies);
            if (target == null) return null;
            Cooldown = 1f / FireRate;
            return CreateShot(target);
        }

        public void Upgrade()
        {
            if (Level >= 3) throw new GameException("Max level!");
            Level++;
            Damage = (int)(Damage * 1.4f);
            Range *= 1.15f;
            FireRate *= 1.2f;
            SellValue += UpgradeCost / 2;
            UpgradeCost = (int)(UpgradeCost * 1.5f);
            MaxHealth = (int)(MaxHealth * 1.25f);
            Health = MaxHealth;
            OnUpgrade();
        }

        public virtual void OnUpgrade() { }
        public void Sell() => Destroy();
    }
}
