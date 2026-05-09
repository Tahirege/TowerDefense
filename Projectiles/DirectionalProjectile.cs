using SkiaSharp;
using System;
using TowerDefense.Enemies;
using TowerDefense.Core;

namespace TowerDefense.Projectiles
{
    public class DirectionalProjectile : Projectile
    {
        private float _dx;
        private float _dy;
        private float _maxDistance = 600f;
        private float _traveled = 0f;

        public DirectionalProjectile(float startX, float startY, float targetX, float targetY, float speed, int damage)
            : base(startX, startY, null!, damage, speed)
        {
            // Yönü hesapla
            float dx = targetX - startX;
            float dy = targetY - startY;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len > 0)
            {
                _dx = dx / len;
                _dy = dy / len;
            }
            else
            {
                _dx = 1; _dy = 0;
            }
        }

        public override void Update(float dt)
        {
            if (!IsAlive) return;

            float moveDist = Speed * dt;
            X += _dx * moveDist;
            Y += _dy * moveDist;
            _traveled += moveDist;

            if (_traveled > _maxDistance || X < 0 || Y < 0 || X > 2000 || Y > 2000)
            {
                IsAlive = false;
            }
        }

        // Çarpışma kontrolünü GameManager'da yapacağız
        public void DrawHeroProj(SKCanvas canvas)
        {
            if (!IsAlive) return;
            using var paint = new SKPaint { Color = SKColors.Cyan, IsAntialias = true };
            canvas.DrawCircle(X, Y, 4, paint);
        }
        
        public void DrawTowerProj(SKCanvas canvas, SKColor color)
        {
            if (!IsAlive) return;
            using var paint = new SKPaint { Color = color, IsAntialias = true };
            canvas.DrawCircle(X, Y, 6, paint);
        }

        protected override void OnHit(Enemy target)
        {
            // Collision is handled by GameManager directly
            // Do nothing here
        }
    }
}
