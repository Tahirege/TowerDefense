using SkiaSharp;
using System;
using System.Linq;

namespace TowerDefense.Enemies
{
    public static class EnemyRenderer
    {
        public static void Draw(SKCanvas canvas, Enemy enemy)
        {
            if (!enemy.IsAlive) return;
            if (enemy.SlowTimer > 0)
            {
                using var iceAura = new SKPaint { Color = new SKColor(100, 180, 255, 60), IsAntialias = true };
                using var iceRing = new SKPaint { Color = new SKColor(150, 210, 255, 120), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
                canvas.DrawCircle(enemy.X, enemy.Y, enemy.Size / 2 + 5, iceAura);
                canvas.DrawCircle(enemy.X, enemy.Y, enemy.Size / 2 + 5, iceRing);
            }
            DrawBody(canvas, enemy);
            DrawHealthBar(canvas, enemy);
        }

        public static void DrawBody(SKCanvas canvas, Enemy enemy)
        {
            switch (enemy)
            {
                case BasicEnemy basic:    DrawBasicEnemy(canvas, basic); break;
                case FastEnemy fast:      DrawFastEnemy(canvas, fast); break;
                case BossEnemy boss:      DrawBossEnemy(canvas, boss); break;
                case ArmoredEnemy arm:    DrawArmoredEnemy(canvas, arm); break;
                case HealerEnemy healer:  DrawHealerEnemy(canvas, healer); break;
                case FlyingEnemy flying:  DrawFlyingEnemy(canvas, flying); break;
                case BomberEnemy bomber:  DrawBomberEnemy(canvas, bomber); break;
                case AggroEnemy aggro:    DrawAggroEnemy(canvas, aggro); break;
                default:                  DrawDefaultEnemy(canvas, enemy); break;
            }
        }

        public static void DrawDefaultEnemy(SKCanvas canvas, Enemy enemy)
        {
            using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 60), IsAntialias = true };
            canvas.DrawOval(enemy.X, enemy.Y + enemy.Size / 2 - 1, enemy.Size / 2 * 0.9f, enemy.Size / 6, shadow);
            using var fill = new SKPaint { Color = enemy.EnemyColor, IsAntialias = true };
            using var stroke = new SKPaint { Color = SKColors.Black.WithAlpha(150), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            canvas.DrawCircle(enemy.X, enemy.Y, enemy.Size / 2, fill);
            canvas.DrawCircle(enemy.X, enemy.Y, enemy.Size / 2, stroke);
            using var shine = new SKPaint { Color = new SKColor(255, 255, 255, 70), IsAntialias = true };
            canvas.DrawCircle(enemy.X - enemy.Size / 5, enemy.Y - enemy.Size / 5, enemy.Size / 5, shine);
        }

        public static void DrawBasicEnemy(SKCanvas c, BasicEnemy e)
        {
            float r = e.Size / 2;
            using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 55), IsAntialias = true };
            c.DrawOval(e.X, e.Y + r + 1, r * 0.85f, r * 0.25f, shadow);
            using var body = new SKPaint { Color = e.EnemyColor, IsAntialias = true };
            c.DrawCircle(e.X, e.Y, r, body);
            using var helmet = new SKPaint { Color = new SKColor(120, 20, 20), IsAntialias = true };
            var helmetPath = new SKPath();
            helmetPath.AddArc(new SKRect(e.X - r, e.Y - r, e.X + r, e.Y + r), 180, 180);
            helmetPath.Close();
            c.DrawPath(helmetPath, helmet);
            using var brim = new SKPaint { Color = new SKColor(90, 15, 15), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f };
            c.DrawLine(e.X - r - 1, e.Y - 1, e.X + r + 1, e.Y - 1, brim);
            using var eyeW = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var eyeB = new SKPaint { Color = new SKColor(30, 0, 0), IsAntialias = true };
            c.DrawCircle(e.X - 4, e.Y + 2, 2.5f, eyeW);
            c.DrawCircle(e.X + 4, e.Y + 2, 2.5f, eyeW);
            c.DrawCircle(e.X - 4, e.Y + 2, 1.3f, eyeB);
            c.DrawCircle(e.X + 4, e.Y + 2, 1.3f, eyeB);
            using var outline = new SKPaint { Color = new SKColor(0, 0, 0, 150), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawCircle(e.X, e.Y, r, outline);
        }

        public static void DrawFastEnemy(SKCanvas c, FastEnemy e)
        {
            float r = e.Size / 2;
            using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 50), IsAntialias = true };
            c.DrawOval(e.X, e.Y + r + 2, r * 1.3f, r * 0.3f, shadow);
            using var fill = new SKPaint { Color = e.EnemyColor, IsAntialias = true };
            using var dark = new SKPaint { Color = new SKColor(180, 100, 0), IsAntialias = true };
            var path = new SKPath();
            path.MoveTo(e.X, e.Y - r * 1.2f);
            path.LineTo(e.X + r, e.Y);
            path.LineTo(e.X, e.Y + r);
            path.LineTo(e.X - r, e.Y);
            path.Close();
            c.DrawPath(path, fill);
            var pathBot = new SKPath();
            pathBot.MoveTo(e.X - r, e.Y);
            pathBot.LineTo(e.X, e.Y + r);
            pathBot.LineTo(e.X + r, e.Y);
            pathBot.Close();
            c.DrawPath(pathBot, dark);
            using var eyeW = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var eyeB = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            c.DrawCircle(e.X + 3, e.Y - 2, 2.5f, eyeW);
            c.DrawCircle(e.X + 3, e.Y - 2, 1.2f, eyeB);
            using var speedLine = new SKPaint { Color = new SKColor(255, 220, 100, 150), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f };
            c.DrawLine(e.X - r - 5, e.Y - 2, e.X - r - 1, e.Y - 2, speedLine);
            c.DrawLine(e.X - r - 4, e.Y + 2, e.X - r, e.Y + 2, speedLine);
            using var outline = new SKPaint { Color = new SKColor(0, 0, 0, 130), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.3f };
            c.DrawPath(path, outline);
        }

        public static void DrawBossEnemy(SKCanvas c, BossEnemy e)
        {
            float ps = e.Size * (1f + 0.07f * MathF.Sin(e.Pulse * 4f));
            float r = ps / 2;
            float auraA = 40 + 20 * MathF.Sin(e.Pulse * 3f);
            using var aura2 = new SKPaint { Color = new SKColor(200, 0, 255, (byte)auraA), IsAntialias = true };
            using var aura1 = new SKPaint { Color = new SKColor(140, 0, 200, (byte)(auraA + 25)), IsAntialias = true };
            c.DrawCircle(e.X, e.Y, r + 12, aura2);
            c.DrawCircle(e.X, e.Y, r + 6, aura1);
            using var bodyFill = new SKPaint { IsAntialias = true, Shader = SKShader.CreateRadialGradient(new SKPoint(e.X - r * 0.3f, e.Y - r * 0.3f), r * 1.1f, new SKColor[] { new SKColor(200, 80, 255), new SKColor(80, 0, 160) }, null, SKShaderTileMode.Clamp) };
            c.DrawCircle(e.X, e.Y, r, bodyFill);
            using var crownP = new SKPaint { Color = new SKColor(255, 215, 0), IsAntialias = true };
            var crown = new SKPath();
            float cx = e.X, cy = e.Y - r + 2;
            crown.MoveTo(cx - 10, cy + 6);
            crown.LineTo(cx - 10, cy - 2);
            crown.LineTo(cx - 6, cy + 1);
            crown.LineTo(cx, cy - 5);
            crown.LineTo(cx + 6, cy + 1);
            crown.LineTo(cx + 10, cy - 2);
            crown.LineTo(cx + 10, cy + 6);
            crown.Close();
            c.DrawPath(crown, crownP);
            using var goldRing = new SKPaint { Color = new SKColor(255, 220, 0), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f };
            c.DrawCircle(e.X, e.Y, r, goldRing);
            using var eyeGlow = new SKPaint { Color = new SKColor(255, 100, 0), IsAntialias = true };
            using var eyePupil = new SKPaint { Color = new SKColor(255, 0, 0), IsAntialias = true };
            c.DrawCircle(e.X - 7, e.Y + 3, 4.5f, eyeGlow);
            c.DrawCircle(e.X + 7, e.Y + 3, 4.5f, eyeGlow);
            c.DrawCircle(e.X - 7, e.Y + 3, 2.5f, eyePupil);
            c.DrawCircle(e.X + 7, e.Y + 3, 2.5f, eyePupil);
        }

        public static void DrawArmoredEnemy(SKCanvas c, ArmoredEnemy e)
        {
            float r = e.Size / 2;
            using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 60), IsAntialias = true };
            c.DrawOval(e.X, e.Y + r + 1, r * 0.9f, r * 0.25f, shadow);
            using var armorFill = new SKPaint { IsAntialias = true, Shader = SKShader.CreateRadialGradient(new SKPoint(e.X - r * 0.3f, e.Y - r * 0.3f), r * 1.2f, new SKColor[] { new SKColor(200, 200, 220), new SKColor(80, 80, 100) }, null, SKShaderTileMode.Clamp) };
            c.DrawCircle(e.X, e.Y, r, armorFill);
            using var plateP = new SKPaint { Color = new SKColor(60, 60, 80, 180), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawLine(e.X - r * 0.6f, e.Y - r * 0.2f, e.X + r * 0.6f, e.Y - r * 0.2f, plateP);
            c.DrawLine(e.X - r * 0.5f, e.Y + r * 0.2f, e.X + r * 0.5f, e.Y + r * 0.2f, plateP);
            using var shieldP = new SKPaint { Color = new SKColor(255, 230, 100), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(e.X, e.Y - 5, e.X, e.Y + 5, shieldP);
            c.DrawLine(e.X - 4, e.Y, e.X + 4, e.Y, shieldP);
            using var ring = new SKPaint { Color = new SKColor(180, 180, 200), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f };
            c.DrawCircle(e.X, e.Y, r, ring);
            using var shine = new SKPaint { Color = new SKColor(255, 255, 255, 80), IsAntialias = true };
            c.DrawCircle(e.X - r * 0.35f, e.Y - r * 0.35f, r * 0.3f, shine);
        }

        public static void DrawHealerEnemy(SKCanvas c, HealerEnemy e)
        {
            float r = e.Size / 2;
            float glow = 0.6f + 0.4f * MathF.Sin(e.GlowPulse * 3f);
            using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 50), IsAntialias = true };
            c.DrawOval(e.X, e.Y + r + 1, r * 0.9f, r * 0.25f, shadow);
            using var aura = new SKPaint { Color = new SKColor(60, 220, 120, (byte)(40 * glow)), IsAntialias = true };
            c.DrawCircle(e.X, e.Y, r + 7, aura);
            using var bodyFill = new SKPaint { IsAntialias = true, Shader = SKShader.CreateRadialGradient(new SKPoint(e.X - r * 0.3f, e.Y - r * 0.3f), r * 1.1f, new SKColor[] { new SKColor(100, 240, 150), new SKColor(20, 130, 70) }, null, SKShaderTileMode.Clamp) };
            c.DrawCircle(e.X, e.Y, r, bodyFill);
            using var crossP = new SKPaint { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(e.X, e.Y - 7, e.X, e.Y + 7, crossP);
            c.DrawLine(e.X - 5, e.Y, e.X + 5, e.Y, crossP);
            using var outline = new SKPaint { Color = new SKColor(10, 80, 40, 170), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawCircle(e.X, e.Y, r, outline);
            using var shine = new SKPaint { Color = new SKColor(200, 255, 220, 80), IsAntialias = true };
            c.DrawCircle(e.X - r * 0.3f, e.Y - r * 0.3f, r * 0.28f, shine);
        }

        public static void DrawFlyingEnemy(SKCanvas c, FlyingEnemy e)
        {
            float wing = MathF.Sin(e.WingTimer * 9f) * 8f;
            float r = e.Size / 2;
            using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 40), IsAntialias = true };
            c.DrawOval(e.X, e.Y + r + 8, r * 1.4f, r * 0.3f, shadow);
            using var wingFill = new SKPaint { Color = new SKColor(150, 210, 255, 200), IsAntialias = true };
            using var wingDark = new SKPaint { Color = new SKColor(50, 130, 200, 160), IsAntialias = true };
            using var featherLine = new SKPaint { Color = new SKColor(80, 160, 220, 120), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
            var leftWing = new SKPath();
            leftWing.MoveTo(e.X - 3, e.Y);
            leftWing.LineTo(e.X - 18, e.Y - wing);
            leftWing.LineTo(e.X - 14, e.Y + 5);
            leftWing.LineTo(e.X - 8, e.Y + 6);
            leftWing.Close();
            var rightWing = new SKPath();
            rightWing.MoveTo(e.X + 3, e.Y);
            rightWing.LineTo(e.X + 18, e.Y - wing);
            rightWing.LineTo(e.X + 14, e.Y + 5);
            rightWing.LineTo(e.X + 8, e.Y + 6);
            rightWing.Close();
            c.DrawPath(leftWing, wingFill);
            c.DrawPath(rightWing, wingFill);
            c.DrawPath(leftWing, wingDark);
            c.DrawPath(rightWing, wingDark);
            c.DrawLine(e.X - 3, e.Y, e.X - 14, e.Y - wing * 0.7f, featherLine);
            c.DrawLine(e.X + 3, e.Y, e.X + 14, e.Y - wing * 0.7f, featherLine);
            using var bodyFill = new SKPaint { IsAntialias = true, Shader = SKShader.CreateRadialGradient(new SKPoint(e.X - 2, e.Y - 2), r * 1.1f, new SKColor[] { new SKColor(130, 200, 255), new SKColor(30, 100, 200) }, null, SKShaderTileMode.Clamp) };
            c.DrawCircle(e.X, e.Y, r, bodyFill);
            using var beak = new SKPaint { Color = new SKColor(255, 200, 50), IsAntialias = true };
            var beakPath = new SKPath();
            beakPath.MoveTo(e.X + r - 1, e.Y);
            beakPath.LineTo(e.X + r + 5, e.Y - 1);
            beakPath.LineTo(e.X + r - 1, e.Y + 3);
            beakPath.Close();
            c.DrawPath(beakPath, beak);
            using var eyeW = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var eyeB = new SKPaint { Color = new SKColor(10, 10, 40), IsAntialias = true };
            c.DrawCircle(e.X + 3, e.Y - 2, 2.8f, eyeW);
            c.DrawCircle(e.X + 4, e.Y - 2, 1.5f, eyeB);
            using var outline = new SKPaint { Color = new SKColor(0, 0, 0, 120), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f };
            c.DrawCircle(e.X, e.Y, r, outline);
        }

        public static void DrawBomberEnemy(SKCanvas c, BomberEnemy e)
        {
            float r = e.Size / 2;
            using var shadow = new SKPaint { Color = new SKColor(0, 0, 0, 55), IsAntialias = true };
            c.DrawOval(e.X, e.Y + r + 1, r * 0.9f, r * 0.25f, shadow);
            float stripeAngle = e.FuseTimer * 60f;
            using var stripe1 = new SKPaint { Color = new SKColor(60, 130, 30), IsAntialias = true };
            using var stripe2 = new SKPaint { Color = new SKColor(30, 80, 15), IsAntialias = true };
            c.DrawCircle(e.X, e.Y, r, stripe1);
            for (int i = 0; i < 3; i++)
            {
                float sa = (i * 120f + stripeAngle) * MathF.PI / 180f;
                var stripePath = new SKPath();
                stripePath.MoveTo(e.X + MathF.Cos(sa) * r, e.Y + MathF.Sin(sa) * r);
                stripePath.ArcTo(new SKRect(e.X - r, e.Y - r, e.X + r, e.Y + r), sa * 180f / MathF.PI, 40, false);
                stripePath.LineTo(e.X, e.Y);
                stripePath.Close();
                c.DrawPath(stripePath, stripe2);
            }
            using var bodyFill = new SKPaint { IsAntialias = true, Shader = SKShader.CreateRadialGradient(new SKPoint(e.X - 3, e.Y - 3), r * 1.1f, new SKColor[] { new SKColor(110, 190, 70), new SKColor(40, 100, 20) }, null, SKShaderTileMode.Clamp) };
            c.DrawCircle(e.X, e.Y, r, bodyFill);
            using var bombOutline = new SKPaint { Color = new SKColor(20, 60, 10), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            c.DrawCircle(e.X, e.Y, r, bombOutline);
            using var warnP = new SKPaint { Color = new SKColor(255, 220, 0), IsAntialias = true };
            var warnPath = new SKPath();
            warnPath.MoveTo(e.X, e.Y - 7);
            warnPath.LineTo(e.X + 6, e.Y + 5);
            warnPath.LineTo(e.X - 6, e.Y + 5);
            warnPath.Close();
            c.DrawPath(warnPath, warnP);
            using var exclP = new SKPaint { Color = new SKColor(40, 20, 0), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(e.X, e.Y - 3, e.X, e.Y + 1, exclP);
            using var dotP = new SKPaint { Color = new SKColor(40, 20, 0), IsAntialias = true };
            c.DrawCircle(e.X, e.Y + 3.5f, 1f, dotP);
            float fuseFlicker = MathF.Sin(e.FuseTimer * 15f);
            using var fuseP = new SKPaint { Color = new SKColor(160, 130, 60), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(e.X, e.Y - r + 1, e.X + 3, e.Y - r - 7, fuseP);
            using var sparkP = new SKPaint { Color = fuseFlicker > 0 ? new SKColor(255, 230, 0) : new SKColor(255, 100, 0), IsAntialias = true };
            c.DrawCircle(e.X + 3, e.Y - r - 7, 2.5f, sparkP);
            using var shine = new SKPaint { Color = new SKColor(200, 255, 180, 80), IsAntialias = true };
            c.DrawCircle(e.X - r * 0.35f, e.Y - r * 0.35f, r * 0.28f, shine);
        }

        public static void DrawAggroEnemy(SKCanvas canvas, AggroEnemy e)
        {
            DrawDefaultEnemy(canvas, e);
            if (e.Hp < e.MaxHp * 0.3f)
            {
                using var p = new SKPaint { Color = SKColors.Orange, IsAntialias = true, TextSize = 14f, TextAlign = SKTextAlign.Center, FakeBoldText = true };
                canvas.DrawText("!", e.X, e.Y - e.Size / 2 - 2, p);
            }
        }

        public static void DrawHealthBar(SKCanvas canvas, Enemy enemy)
        {
            float bw = enemy.Size + 6, bh = 5, bx = enemy.X - bw / 2, by = enemy.Y - enemy.Size / 2 - 12, fillPct = (float)enemy.Hp / enemy.MaxHp;
            using var bg = new SKPaint { Color = new SKColor(30, 30, 30, 200), IsAntialias = true };
            canvas.DrawRoundRect(new SKRect(bx - 1, by - 1, bx + bw + 1, by + bh + 1), 3, 3, bg);
            var color = fillPct > 0.6f ? new SKColor(50, 220, 80) : fillPct > 0.3f ? new SKColor(255, 180, 30) : new SKColor(255, 50, 50);
            using var fg = new SKPaint { Color = color, IsAntialias = true };
            if (fillPct > 0) canvas.DrawRoundRect(new SKRect(bx, by, bx + bw * fillPct, by + bh), 2, 2, fg);
        }
    }
}
