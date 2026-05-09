using SkiaSharp;
using System;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public class AggroEnemy : Enemy
    {
        public Managers.MapManager? Map { get; set; }
        
        // Callback to GameManager explosion handler
        public Action<float, float, float, int>? OnAggroExplode { get; set; }
        public float ExplosionRadius { get; } = 80f;
        public int   ExplosionDamage { get; } = 30;
        private bool _hasExploded = false;

        public AggroEnemy(float x, float y) : base(x, y, 120, 45f, 25)
        {
            EnemyName = "Savaşçı";
            EnemyColor = SKColors.DarkRed;
            Size = 22f;
            
            // Stats for passing-by attack (now in base class)
            AttackRange = 40f;
            AttackDamage = 15;
            AttackCooldown = 0.8f;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            
            // Explode on death (if not reached end)
            if (!IsAlive && !_hasExploded && !ReachedEnd)
            {
                _hasExploded = true;
                OnAggroExplode?.Invoke(X, Y, ExplosionRadius, ExplosionDamage);
            }
        }

        // We removed the MoveAlongPath override that implemented the follow logic.
        // It will now use the base MoveAlongPath which follows the designated path.

        protected override void DrawBody(SKCanvas canvas)
        {
            base.DrawBody(canvas);
            
            // Draw a small explosion warning icon if HP is low
            if (Hp < MaxHp * 0.3f)
            {
                using var p = new SKPaint { Color = SKColors.Orange, IsAntialias = true, TextSize = 14f, TextAlign = SKTextAlign.Center, FakeBoldText = true };
                canvas.DrawText("!", X, Y - Size/2 - 2, p);
            }
        }
    }
}
