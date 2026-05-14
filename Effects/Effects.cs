using SkiaSharp;

namespace TowerDefense.Effects
{
    public record struct Effect(float X, float Y, float Vx, float Vy,
        float Life, float MaxLife, float Size, SKColor Color);

    public class Effects
    {
        public List<Effect> List = new();
        public Random Rng = new();

        public void Update(float dt)
        {
            for (int i = List.Count - 1; i >= 0; i--)
            {
                var p = List[i];
                p = p with { X = p.X + p.Vx * dt, Y = p.Y + p.Vy * dt,
                    Vy = p.Vy + 60f * dt, Life = p.Life - dt };
                if (p.Life <= 0) List.RemoveAt(i);
                else List[i] = p;
            }
        }

        public void Draw(SKCanvas canvas)
        {
            using var paint = new SKPaint { IsAntialias = true };
            foreach (var p in List)
            {
                float alpha = p.Life / p.MaxLife;
                paint.Color = p.Color.WithAlpha((byte)(alpha * 220));
                canvas.DrawCircle(p.X, p.Y, p.Size * alpha, paint);
            }
        }

        public void Explode(float x, float y, SKColor color, int count = 20)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (float)(Rng.NextDouble() * Math.PI * 2);
                float speed = 60f + (float)Rng.NextDouble() * 120f;
                List.Add(new Effect(x, y,
                    MathF.Cos(angle)*speed, MathF.Sin(angle)*speed - 40f,
                    0.6f + (float)Rng.NextDouble()*0.4f, 1f,
                    2f + (float)Rng.NextDouble()*3f, color));
            }
        }

        public void EnemyDeath(float x, float y, SKColor color)
        {
            Explode(x, y, color, 12);
            for (int i = 0; i < 6; i++)
            {
                float angle = (float)(Rng.NextDouble() * Math.PI * 2);
                List.Add(new Effect(x, y,
                    MathF.Cos(angle)*40f, MathF.Sin(angle)*40f - 60f,
                    0.8f, 0.8f, 3f, new SKColor(255, 215, 0)));
            }
        }

        public void HealEffect(float x, float y)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = (float)(Rng.NextDouble() * Math.PI * 2);
                List.Add(new Effect(x, y,
                    MathF.Cos(angle)*20f, MathF.Sin(angle)*20f - 50f,
                    1f, 1f, 4f, new SKColor(80, 220, 120)));
            }
        }

        public void UpgradeEffect(float x, float y)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = (float)(Rng.NextDouble() * Math.PI * 2);
                float speed = 40f + (float)Rng.NextDouble()*80f;
                List.Add(new Effect(x, y,
                    MathF.Cos(angle)*speed, MathF.Sin(angle)*speed - 80f,
                    1.2f, 1.2f, 3f, new SKColor(255, 220, 50)));
            }
        }

        public void LifeLostEffect(float x, float y)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = (float)(Rng.NextDouble() * Math.PI * 2);
                List.Add(new Effect(x, y,
                    MathF.Cos(angle)*50f, MathF.Sin(angle)*50f,
                    0.5f, 0.5f, 5f, new SKColor(255, 50, 50)));
            }
        }
    }
}
