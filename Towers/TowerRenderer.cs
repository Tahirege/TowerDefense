using SkiaSharp;
using System;
using System.Linq;
using TowerDefense.Enemies;
using TowerDefense.Shots;

namespace TowerDefense.Towers
{
    public static class TowerRenderer
    {
        public static void Draw(Tower tower, SKCanvas canvas)
        {
            const float s = 34f, r = 6f;
            if (tower.IsSelected)
            {
                using var rc = new SKPaint { Color = tower.TowerColor.WithAlpha(40), IsAntialias = true };
                using var rl = new SKPaint { Color = tower.TowerColor.WithAlpha(160), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.2f, PathEffect = SKPathEffect.CreateDash(new float[] { 8, 4 }, 0) };
                canvas.DrawCircle(tower.X, tower.Y, tower.Range, rc);
                canvas.DrawCircle(tower.X, tower.Y, tower.Range, rl);
            }
            using (var shadow = new SKPaint { Color = new SKColor(0,0,0,80), IsAntialias = true })
                canvas.DrawRoundRect(new SKRect(tower.X - s/2+2, tower.Y - s/2+3, tower.X + s/2+2, tower.Y + s/2+3), r, r, shadow);

            float healthPct = (float)tower.Health / tower.MaxHealth;
            var baseFill = healthPct > 0.5f ? new SKColor(30, 35, 45) : new SKColor((byte)(30 + (1f - healthPct) * 80), 20, 20);
            using (var fill = new SKPaint { Color = baseFill, IsAntialias = true }) 
                canvas.DrawRoundRect(new SKRect(tower.X - s/2, tower.Y - s/2, tower.X + s/2, tower.Y + s/2), r, r, fill);
            
            using (var border = new SKPaint { Color = tower.TowerColor.WithAlpha((byte)(255 * healthPct + 80 * (1f - healthPct))), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f })
                canvas.DrawRoundRect(new SKRect(tower.X - s/2, tower.Y - s/2, tower.X + s/2, tower.Y + s/2), r, r, border);

            using (var shine = new SKPaint { Color = tower.TowerColor.WithAlpha(40), IsAntialias = true })
                canvas.DrawRoundRect(new SKRect(tower.X - s/2+3, tower.Y - s/2+3, tower.X + s/2-3, tower.Y), 3, 3, shine);

            using var badgeFill = new SKPaint { Color = tower.TowerColor.WithAlpha(200), IsAntialias = true };
            canvas.DrawCircle(tower.X - s/2 + 8, tower.Y - s/2 + 8, 7, badgeFill);
            using var lvlText = new SKPaint { Color = SKColors.White, IsAntialias = true, TextSize = 9f, TextAlign = SKTextAlign.Center, FakeBoldText = true };
            canvas.DrawText($"{tower.Level}", tower.X - s/2 + 8, tower.Y - s/2 + 12, lvlText);

            DrawTop(tower, canvas, s);
            DrawHealthBar(tower, canvas, s);
        }

        public static void DrawTop(Tower t, SKCanvas c, float s)
        {
            if (t is ArrowTower) DrawArrowTop(t, c);
            else if (t is CannonTower) DrawCannonTop(t, c);
            else if (t is IceTower) DrawIceTop(t, c);
            else if (t is LaserTower lt) DrawLaserTop(lt, c);
            else if (t is BombTower bt) DrawBombTop(bt, c);
            else if (t is SniperTower) DrawSniperTop(t, c);
        }

        public static void DrawArrowTop(Tower t, SKCanvas c)
        {
            using var bowP = new SKPaint { Color = new SKColor(150, 210, 255), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f, StrokeCap = SKStrokeCap.Round };
            using var arrowP = new SKPaint { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round };
            var arcRect = new SKRect(t.X - 6, t.Y - 9, t.X + 6, t.Y + 9);
            c.DrawArc(arcRect, -80, 160, false, bowP);
            c.DrawLine(t.X - 9, t.Y, t.X + 9, t.Y, arrowP);
            c.DrawLine(t.X + 6, t.Y - 4, t.X + 9, t.Y, arrowP);
            c.DrawLine(t.X + 6, t.Y + 4, t.X + 9, t.Y, arrowP);
        }

        public static void DrawCannonTop(Tower t, SKCanvas c)
        {
            using var barrel = new SKPaint { Color = new SKColor(60, 60, 70), IsAntialias = true };
            using var barrelHL = new SKPaint { Color = new SKColor(100, 100, 120, 160), IsAntialias = true };
            using var ball = new SKPaint { Color = new SKColor(30, 30, 35), IsAntialias = true };
            using var ballHL = new SKPaint { Color = new SKColor(80, 80, 90, 120), IsAntialias = true };
            var barrelPath = new SKPath();
            barrelPath.MoveTo(t.X - 3, t.Y + 3);
            barrelPath.LineTo(t.X - 3, t.Y - 1);
            barrelPath.LineTo(t.X + 12, t.Y - 3);
            barrelPath.LineTo(t.X + 12, t.Y + 3);
            barrelPath.Close();
            c.DrawPath(barrelPath, barrel);
            c.DrawLine(t.X - 2, t.Y - 0.5f, t.X + 11, t.Y - 2f, barrelHL);
            c.DrawCircle(t.X - 1, t.Y + 1, 5, ball);
            c.DrawCircle(t.X - 2.5f, t.Y - 0.5f, 2, ballHL);
        }

        public static void DrawIceTop(Tower t, SKCanvas c)
        {
            using var outer = new SKPaint { Color = new SKColor(180, 240, 255), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.8f, StrokeCap = SKStrokeCap.Round };
            using var inner = new SKPaint { Color = new SKColor(220, 250, 255, 200), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, StrokeCap = SKStrokeCap.Round };
            for (int i = 0; i < 6; i++)
            {
                double a = i * Math.PI / 3;
                float ex = t.X + (float)(Math.Cos(a) * 10);
                float ey = t.Y + (float)(Math.Sin(a) * 10);
                c.DrawLine(t.X, t.Y, ex, ey, outer);
                double bAngle1 = a + Math.PI / 4, bAngle2 = a - Math.PI / 4;
                float mx = t.X + (float)(Math.Cos(a) * 6), my = t.Y + (float)(Math.Sin(a) * 6);
                c.DrawLine(mx, my, mx + (float)(Math.Cos(bAngle1) * 4), my + (float)(Math.Sin(bAngle1) * 4), inner);
                c.DrawLine(mx, my, mx + (float)(Math.Cos(bAngle2) * 4), my + (float)(Math.Sin(bAngle2) * 4), inner);
            }
            using var gem = new SKPaint { Color = new SKColor(200, 245, 255), IsAntialias = true };
            c.DrawCircle(t.X, t.Y, 3, gem);
        }

        public static void DrawLaserTop(LaserTower lt, SKCanvas c)
        {
            if (lt.CurrentTarget != null && lt.CurrentTarget.IsAlive)
            {
                float pulse = 0.7f + 0.3f * MathF.Sin(lt.BeamPulse * 6f);
                using var outerGlow = new SKPaint { Color = new SKColor(255, 100, 230, (byte)(40 * pulse)), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 14f };
                using var midGlow = new SKPaint { Color = new SKColor(255, 50, 200, (byte)(90 * pulse)), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 6f };
                using var beam = new SKPaint { Color = new SKColor(255, 220, 255, (byte)(230 * pulse)), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
                c.DrawLine(lt.X, lt.Y, lt.CurrentTarget.X, lt.CurrentTarget.Y, outerGlow);
                c.DrawLine(lt.X, lt.Y, lt.CurrentTarget.X, lt.CurrentTarget.Y, midGlow);
                c.DrawLine(lt.X, lt.Y, lt.CurrentTarget.X, lt.CurrentTarget.Y, beam);
            }
            using var outer = new SKPaint { Color = new SKColor(255, 60, 200, 100), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            using var inner = new SKPaint { Color = new SKColor(255, 160, 240), IsAntialias = true };
            using var core  = new SKPaint { Color = SKColors.White, IsAntialias = true };
            c.DrawCircle(lt.X, lt.Y, 9, outer);
            c.DrawCircle(lt.X, lt.Y, 6, inner);
            c.DrawCircle(lt.X, lt.Y, 3, core);
        }

        public static void DrawBombTop(BombTower bt, SKCanvas c)
        {
            using var bombFill = new SKPaint { IsAntialias = true, Shader = SKShader.CreateRadialGradient(new SKPoint(bt.X - 3, bt.Y - 3), 9, new SKColor[] { new SKColor(90, 90, 110), new SKColor(20, 20, 30) }, null, SKShaderTileMode.Clamp) };
            using var bombStroke = new SKPaint { Color = new SKColor(120, 120, 140), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f };
            c.DrawCircle(bt.X, bt.Y + 1, 8, bombFill);
            c.DrawCircle(bt.X, bt.Y + 1, 8, bombStroke);
            float sparkFlicker = MathF.Sin(bt.FuseTimer * 12f);
            using var fuseP = new SKPaint { Color = new SKColor(160, 140, 100), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, StrokeCap = SKStrokeCap.Round };
            c.DrawLine(bt.X, bt.Y - 7, bt.X + 4, bt.Y - 12, fuseP);
            using var sparkP = new SKPaint { Color = sparkFlicker > 0 ? new SKColor(255, 220, 0) : new SKColor(255, 100, 0), IsAntialias = true };
            c.DrawCircle(bt.X + 4, bt.Y - 12, 2.5f, sparkP);
            using var shine = new SKPaint { Color = new SKColor(200, 200, 220, 100), IsAntialias = true };
            c.DrawCircle(bt.X - 3, bt.Y - 2, 3, shine);
        }

        public static void DrawSniperTop(Tower t, SKCanvas c)
        {
            using var barrel = new SKPaint { Color = new SKColor(50, 100, 50), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round };
            using var barrelHL = new SKPaint { Color = new SKColor(130, 200, 130, 150), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, StrokeCap = SKStrokeCap.Round };
            using var scope = new SKPaint { IsAntialias = true, Shader = SKShader.CreateRadialGradient(new SKPoint(t.X + 4, t.Y - 2), 5, new SKColor[] { new SKColor(80, 150, 80), new SKColor(30, 70, 30) }, null, SKShaderTileMode.Clamp) };
            using var scopeR = new SKPaint { Color = new SKColor(100, 180, 100), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f };
            using var crosshair = new SKPaint { Color = new SKColor(200, 255, 200, 160), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0.8f };
            c.DrawLine(t.X - 3, t.Y, t.X + 16, t.Y - 1, barrel);
            c.DrawLine(t.X - 2, t.Y - 1, t.X + 15, t.Y - 2, barrelHL);
            c.DrawCircle(t.X + 4, t.Y - 2, 5, scope);
            c.DrawCircle(t.X + 4, t.Y - 2, 5, scopeR);
            c.DrawLine(t.X + 4, t.Y - 6, t.X + 4, t.Y + 2, crosshair);
            c.DrawLine(t.X + 1, t.Y - 2, t.X + 7, t.Y - 2, crosshair);
        }

        public static void DrawHealthBar(Tower tower, SKCanvas canvas, float s)
        {
            if (tower.Health >= tower.MaxHealth) return;
            float bw = s, bh = 4, bx = tower.X - bw/2, by = tower.Y - s/2 - 8, fillPct = (float)tower.Health / tower.MaxHealth;
            using var bg = new SKPaint { Color = new SKColor(30, 30, 30, 180), IsAntialias = true };
            canvas.DrawRect(bx, by, bw, bh, bg);
            var color = fillPct > 0.6f ? new SKColor(50, 220, 80) : fillPct > 0.3f ? new SKColor(255, 180, 30) : new SKColor(255, 50, 50);
            using var fg = new SKPaint { Color = color, IsAntialias = true };
            if (fillPct > 0) canvas.DrawRect(bx, by, bw * fillPct, bh, fg);
        }
    }
}
