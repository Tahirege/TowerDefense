using SkiaSharp;
using TowerDefense.Enemies;

namespace TowerDefense.Shots
{
    public class Bullet : Shot
    {
        public float Dx;
        public float Dy;
        public float MaxDist = 800f;
        public float Traveled;
        public float Radius { get; set; } = 4f;

        public Bullet(float startX, float startY, float targetX, float targetY, float speed, int damage)
            : base(startX, startY, null!, damage, speed)
        {
            float dx = targetX - startX, dy = targetY - startY;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            
            if (len > 0) { Dx = dx / len; Dy = dy / len; }
            else { Dx = 1; Dy = 0; }
            
            Color = SKColors.Cyan;
        }

        public override void Update(float dt)
        {
            if (!IsAlive) return;

            float dist = Speed * dt;
            X += Dx * dist;
            Y += Dy * dist;
            Traveled += dist;

            if (Traveled > MaxDist || X < -50 || Y < -50 || X > 2500 || Y > 2500)
                Destroy();
        }

        public override void OnHit(Enemy target) { }

        public override void Draw(SKCanvas canvas)
        {
            if (!IsAlive) return;
            using var paint = new SKPaint { Color = Color, IsAntialias = true };
            canvas.DrawCircle(X, Y, Radius, paint);
        }
    }
}
