using SkiaSharp;
using System;

namespace TowerDefense.Core
{
    public static class HeroRenderer
    {
        public const float SwingDuration = 0.18f;

        public static void Draw(SKCanvas canvas, Hero hero)
        {
            float walkCycle = hero.WalkCycle;
            float walkMag = Math.Clamp(walkCycle > 0 ? 1f : 0f, 0, 1);
            float bob = MathF.Cos(walkCycle * 2f) * 2.5f * walkMag;
            if (walkMag < 0.1f) bob = 0;

            float pulse = 0.8f + 0.2f * MathF.Sin(walkCycle * 0.5f);
            float legStepLeft = MathF.Cos(walkCycle) * 4.5f;
            float legStepRight = MathF.Cos(walkCycle + MathF.PI) * 4.5f;
            float legXLeft = -4f + MathF.Sin(walkCycle) * 1.5f;
            float legXRight = 4f + MathF.Sin(walkCycle + MathF.PI) * 1.5f;

            float moveLean = (hero.MovingLeft || hero.MovingRight || hero.MovingUp || hero.MovingDown) ? 0.08f : 0f;
            float walkSway = MathF.Sin(walkCycle) * 0.05f; 
            float totalTilt = (walkSway + moveLean);

            canvas.Save();
            canvas.Scale(hero.FacingX, 1, hero.X, hero.Y);

            using (var legP = new SKPaint { Color = new SKColor(20, 20, 50), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 5.5f, StrokeCap = SKStrokeCap.Round })
            {
                canvas.DrawLine(hero.X - 4, hero.Y + 8 + bob, hero.X + legXLeft, hero.Y + 17 + bob + legStepLeft, legP);
                canvas.DrawLine(hero.X + 4, hero.Y + 8 + bob, hero.X + legXRight, hero.Y + 17 + bob + legStepRight, legP);
            }

            canvas.Save();
            canvas.RotateRadians(totalTilt, hero.X, hero.Y + bob);

            using (var plateFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(hero.X - 4, hero.Y - 6 + bob), 16,
                    new SKColor[] { new SKColor(120, 180, 255), new SKColor(40, 70, 180) },
                    null, SKShaderTileMode.Clamp)
            })
            {
                var bodyPath = new SKPath();
                bodyPath.MoveTo(hero.X - 9, hero.Y - 3 + bob);
                bodyPath.LineTo(hero.X - 10, hero.Y + 11 + bob);
                bodyPath.LineTo(hero.X + 10, hero.Y + 11 + bob);
                bodyPath.LineTo(hero.X + 9, hero.Y - 3 + bob);
                bodyPath.Close();
                canvas.DrawPath(bodyPath, plateFill);
            }

            using (var emblem = new SKPaint { Color = new SKColor(255, 220, 0, (byte)(200 * pulse)), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f })
            {
                canvas.DrawLine(hero.X, hero.Y + 1 + bob, hero.X, hero.Y + 9 + bob, emblem);
                canvas.DrawLine(hero.X - 4, hero.Y + 5 + bob, hero.X + 4, hero.Y + 5 + bob, emblem);
            }

            using (var armP = new SKPaint { Color = new SKColor(40, 70, 180), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 4.5f, StrokeCap = SKStrokeCap.Round })
            {
                float armSwing = MathF.Sin(walkCycle) * 5f;
                canvas.DrawLine(hero.X - 9, hero.Y + bob, hero.X - 15, hero.Y + 8 + armSwing + bob, armP);

                float swordArmX = hero.X + 9;
                float swordArmY = hero.Y + bob;
                float handAngle = hero.Swinging ? (hero.FacingX < 0 ? MathF.PI - hero.SwingAngle : hero.SwingAngle) : -MathF.PI / 5f;
                float handX = swordArmX + MathF.Cos(handAngle) * 11f;
                float handY = swordArmY + MathF.Sin(handAngle) * 11f;
                canvas.DrawLine(swordArmX, swordArmY, handX, handY, armP);

                float swordLen = 25f;
                float ex = handX + MathF.Cos(handAngle) * swordLen;
                float ey = handY + MathF.Sin(handAngle) * swordLen;

                if (hero.Swinging)
                {
                    float swingProgress = hero.SwingTimer / SwingDuration;
                    float fadeAlpha = 1f - swingProgress;
                    using (var trailP = new SKPaint { Color = new SKColor(150, 200, 255, (byte)(100 * fadeAlpha)), IsAntialias = true, Style = SKPaintStyle.Fill })
                    {
                        var trailPath = new SKPath();
                        trailPath.MoveTo(hero.X, hero.Y);
                        float sDeg = (handAngle - 1.3f) * 180f / MathF.PI;
                        float wDeg = 2.6f * 180f / MathF.PI;
                        trailPath.ArcTo(new SKRect(hero.X - hero.AttackRange, hero.Y - hero.AttackRange, hero.X + hero.AttackRange, hero.Y + hero.AttackRange), sDeg, wDeg, false);
                        trailPath.Close();
                        canvas.DrawPath(trailPath, trailP);
                    }
                    using (var bladeGlow = new SKPaint { Color = new SKColor(200, 230, 255, (byte)(150 * fadeAlpha)), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 10f, StrokeCap = SKStrokeCap.Round })
                    {
                        canvas.DrawLine(handX, handY, ex, ey, bladeGlow);
                    }
                }

                using (var bladeP = new SKPaint { IsAntialias = true, Shader = SKShader.CreateLinearGradient(new SKPoint(handX, handY), new SKPoint(ex, ey), new SKColor[] { SKColors.White, new SKColor(180, 210, 255) }, null, SKShaderTileMode.Clamp), Style = SKPaintStyle.Stroke, StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round })
                {
                    canvas.DrawLine(handX, handY, ex, ey, bladeP);
                }

                float px = -MathF.Sin(handAngle) * 6f, py = MathF.Cos(handAngle) * 6f;
                using (var goldP = new SKPaint { Color = new SKColor(255, 215, 0), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 4f, StrokeCap = SKStrokeCap.Round })
                {
                    canvas.DrawLine(handX - px, handY - py, handX + px, handY + py, goldP);
                    canvas.DrawCircle(handX, handY, 2.5f, goldP);
                }
            }

            using (var skinP = new SKPaint { Color = new SKColor(255, 220, 190), IsAntialias = true })
            {
                canvas.DrawCircle(hero.X, hero.Y - 13 + bob, 9, skinP);
            }

            using (var helmetFill = new SKPaint { IsAntialias = true, Shader = SKShader.CreateRadialGradient(new SKPoint(hero.X - 3, hero.Y - 20 + bob), 10, new SKColor[] { new SKColor(150, 200, 255), new SKColor(50, 90, 210) }, null, SKShaderTileMode.Clamp) })
            {
                var helmetPath = new SKPath();
                helmetPath.AddArc(new SKRect(hero.X - 10, hero.Y - 23 + bob, hero.X + 10, hero.Y - 13 + bob), 180, 180);
                helmetPath.Close();
                canvas.DrawPath(helmetPath, helmetFill);
            }

            using (var crestP = new SKPaint { Color = new SKColor(220, 40, 40), IsAntialias = true })
            {
                var crestPath = new SKPath();
                crestPath.MoveTo(hero.X - 2, hero.Y - 23 + bob);
                crestPath.LineTo(hero.X + 2, hero.Y - 23 + bob);
                crestPath.LineTo(hero.X,     hero.Y - 29 + bob);
                crestPath.Close();
                canvas.DrawPath(crestPath, crestP);
            }

            using (var visor = new SKPaint { Color = new SKColor(10, 10, 30, 220), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f })
            {
                canvas.DrawLine(hero.X - 6, hero.Y - 14 + bob, hero.X + 6, hero.Y - 14 + bob, visor);
            }

            canvas.Restore();
            canvas.Restore();
            DrawHealthBar(canvas, hero, bob);
        }

        public static void DrawHealthBar(SKCanvas canvas, Hero hero, float bob)
        {
            float w = 34, h = 5;
            float bx = hero.X - w/2, by = hero.Y - 28 + bob;
            float pct = (float)hero.Health / hero.MaxHealth;
            using (var bg = new SKPaint { Color = new SKColor(20,20,20,200), IsAntialias = true })
            {
                var bgRect = new SKRoundRect(new SKRect(bx - 1, by - 1, bx + w + 1, by + h + 1), 3, 3);
                canvas.DrawRoundRect(bgRect, bg);
            }
            var fc = pct > 0.5f ? new SKColor(50, 220, 80) : pct > 0.2f ? new SKColor(255, 180, 30) : new SKColor(255, 50, 50);
            using (var fg = new SKPaint { Color = fc, IsAntialias = true })
            {
                if (pct > 0)
                {
                    var fgRect = new SKRoundRect(new SKRect(bx, by, bx + w * pct, by + h), 2, 2);
                    canvas.DrawRoundRect(fgRect, fg);
                }
            }
        }
    }
}
