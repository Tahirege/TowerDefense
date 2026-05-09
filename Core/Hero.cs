using SkiaSharp;
using System;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public class Hero
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Speed     { get; } = 150f;
        public int   MaxHealth { get; } = 500;
        public int   Health    { get; private set; }

        // ── Melee sword ───────────────────────────────────────────
        public float AttackRange    { get; } = 65f;   // melee reach
        public int   AttackDamage   { get; } = 50;
        public float AttackCooldown { get; } = 0.6f;
        private float _attackTimer  = 0f;

        // Swing animation
        private float _swingAngle      = 0f;
        private float _swingStartAngle = 0f;
        private float _swingEndAngle   = 0f;
        private bool  _swinging        = false;
        public  bool  IsSwinging       => _swinging;
        private float _swingTimer      = 0f;
        private const float SwingDuration = 0.18f;

        // Walk animation
        private float _walkCycle = 0f;
        private float _facingX   = 1f; // +1 = right, -1 = left

        // Input
        public bool MovingLeft  { get; set; }
        public bool MovingRight { get; set; }
        public bool MovingUp    { get; set; }
        public bool MovingDown  { get; set; }

        // Enemies hit this swing (cleared each swing)
        private readonly HashSet<Enemy> _hitThisSwing = new();

        public Hero(float startX, float startY)
        {
            X = startX; Y = startY;
            Health = MaxHealth;
            _swingAngle = -MathF.PI / 4f;
        }

        public void TakeDamage(int damage)
        {
            Health = Math.Max(0, Health - damage);
        }

        /// <summary>Called by GameManager each frame to move/update swing animation.</summary>
        public void Update(float dt, TowerDefense.Managers.MapManager map)
        {
            if (Health <= 0) return;

            // ── Movement ─────────────────────────────────────────
            float dx = 0, dy = 0;
            if (MovingLeft)  dx -= 1;
            if (MovingRight) dx += 1;
            if (MovingUp)    dy -= 1;
            if (MovingDown)  dy += 1;

            if (dx != 0 || dy != 0)
            {
                float len = MathF.Sqrt(dx*dx + dy*dy);
                dx /= len; dy /= len;
                if (dx != 0) _facingX = dx > 0 ? 1f : -1f;

                // Proposed movement
                float nextX = X + dx * Speed * dt;
                float nextY = Y + dy * Speed * dt;

                // Boundary clamp first
                nextX = Math.Clamp(nextX, 10, map.MapWidth - 10);
                nextY = Math.Clamp(nextY, 10, map.MapHeight - 10);

                // Check collision sliding independently (allows sliding against walls)
                if (map.IsWalkable(nextX, Y, 8f)) X = nextX;
                if (map.IsWalkable(X, nextY, 8f)) Y = nextY;

                _walkCycle += dt * 12f;
            }
            else
            {
                // Smoothly return to neutral pose
                float targetCycle = MathF.PI * 2f; // Return to a point where Sin is 0
                _walkCycle %= MathF.PI * 2f;
                if (_walkCycle > 0.1f) _walkCycle = Lerp(_walkCycle, targetCycle, dt * 8f);
                else _walkCycle = 0;
            }

            // ── Cooldown ─────────────────────────────────────────
            if (_attackTimer > 0) _attackTimer -= dt;

            // ── Swing animation ───────────────────────────────────
            if (_swinging)
            {
                _swingTimer += dt;
                float t = Math.Clamp(_swingTimer / SwingDuration, 0f, 1f);
                _swingAngle = Lerp(_swingStartAngle, _swingEndAngle, t);
                if (_swingTimer >= SwingDuration)
                {
                    _swinging   = false;
                    _swingTimer = 0f;
                    _hitThisSwing.Clear();
                }
            }
        }

        /// <summary>
        /// Attempt a melee swing toward (targetX, targetY).
        /// Returns list of enemies within melee range that should take damage.
        /// Call this from GameManager.HeroMeleeAttack.
        /// </summary>
        public bool TrySwing(float targetX, float targetY)
        {
            if (Health <= 0 || _attackTimer > 0) return false;
            _attackTimer = AttackCooldown;

            float dx = targetX - X, dy = targetY - Y;
            float targetAngle = MathF.Atan2(dy, dx);
            
            // Swing from -80 to +80 degrees relative to target
            _swingStartAngle = targetAngle - 1.4f; 
            _swingEndAngle   = targetAngle + 1.4f;
            _swingAngle      = _swingStartAngle;

            _swinging    = true;
            _swingTimer  = 0f;
            _hitThisSwing.Clear();
            if (dx != 0) _facingX = dx > 0 ? 1f : -1f;
            return true;
        }

        /// <summary>Returns true if enemy is inside the current swing arc and hasn't been hit yet this swing.</summary>
        public bool TryHitEnemy(Enemy e)
        {
            if (!_swinging || _hitThisSwing.Contains(e)) return false;
            float dx = e.X - X, dy = e.Y - Y;
            float dist = MathF.Sqrt(dx*dx + dy*dy);
            if (dist > AttackRange + e.Size/2) return false;

            // Arc check — ±35° of current blade position
            float angle = MathF.Atan2(dy, dx);
            float diff  = Math.Abs(AngleDiff(angle, _swingAngle));
            if (diff > 0.6f) return false; // ~35 degrees

            _hitThisSwing.Add(e);
            return true;
        }

        // ── Draw ──────────────────────────────────────────────────
        public void Draw(SKCanvas canvas)
        {
            if (Health <= 0) return;

            // ── Animation Variables ──────────────────────────────
            float walkMag = Math.Clamp(_walkCycle > 0 ? 1f : 0f, 0, 1);
            
            // Bobbing: Up on every step (two steps per cycle)
            float bob = MathF.Cos(_walkCycle * 2f) * 2.5f * walkMag;
            if (walkMag < 0.1f) bob = 0; // Stationary

            float pulse = 0.8f + 0.2f * MathF.Sin(_walkCycle * 0.5f);
            
            // Vertical leg "stepping" (Forward/Backward look)
            float legStepLeft = MathF.Cos(_walkCycle) * 4.5f;
            float legStepRight = MathF.Cos(_walkCycle + MathF.PI) * 4.5f;
            
            // Slight horizontal leg separation during walk
            float legXLeft = -4f + MathF.Sin(_walkCycle) * 1.5f;
            float legXRight = 4f + MathF.Sin(_walkCycle + MathF.PI) * 1.5f;

            // Body tilt and movement lean
            float moveLean = (MovingLeft || MovingRight || MovingUp || MovingDown) ? 0.08f : 0f;
            float walkSway = MathF.Sin(_walkCycle) * 0.05f; 
            float totalTilt = (walkSway + moveLean);

            // ── Save Canvas for Flipping ─────────────────────────
            canvas.Save();
            // Flip the entire character based on facing direction
            canvas.Scale(_facingX, 1, X, Y);



            // ── Legs (Vertical Stepping) ─────────────────────────
            using var legP = new SKPaint { Color = new SKColor(20, 20, 50), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 5.5f, StrokeCap = SKStrokeCap.Round };
            
            // Left leg
            canvas.DrawLine(X - 4, Y + 8 + bob, X + legXLeft, Y + 17 + bob + legStepLeft, legP);
            // Right leg
            canvas.DrawLine(X + 4, Y + 8 + bob, X + legXRight, Y + 17 + bob + legStepRight, legP);

            // ── Save Canvas for Upper Body Tilt ──────────────────
            canvas.Save();
            canvas.RotateRadians(totalTilt, X, Y + bob);

            // ── Body Armor (Pauldrons & Plates) ───────────────────
            using var plateFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X - 4, Y - 6 + bob), 16,
                    new SKColor[] { new SKColor(120, 180, 255), new SKColor(40, 70, 180) },
                    null, SKShaderTileMode.Clamp)
            };
            
            // Main body
            var bodyPath = new SKPath();
            bodyPath.MoveTo(X - 9, Y - 3 + bob);
            bodyPath.LineTo(X - 10, Y + 11 + bob);
            bodyPath.LineTo(X + 10, Y + 11 + bob);
            bodyPath.LineTo(X + 9, Y - 3 + bob);
            bodyPath.Close();
            canvas.DrawPath(bodyPath, plateFill);


            // Armor cross emblem
            using var emblem = new SKPaint { Color = new SKColor(255, 220, 0, (byte)(200 * pulse)), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            canvas.DrawLine(X, Y + 1 + bob, X, Y + 9 + bob, emblem);
            canvas.DrawLine(X - 4, Y + 5 + bob, X + 4, Y + 5 + bob, emblem);


            using var armP = new SKPaint { Color = new SKColor(40, 70, 180), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 4.5f, StrokeCap = SKStrokeCap.Round };

            // ── Off-arm ──────────────────────────────────────────
            float armSwing = MathF.Sin(_walkCycle) * 5f;
            canvas.DrawLine(X - 9, Y + bob, X - 15, Y + 8 + armSwing + bob, armP);

            // ── Sword Arm & Master Sword ─────────────────────────
            float swordArmX = X + 9;
            float swordArmY = Y + bob;
            float handAngle = _swinging ? (_facingX < 0 ? MathF.PI - _swingAngle : _swingAngle) : -MathF.PI / 5f;
            float handX = swordArmX + MathF.Cos(handAngle) * 11f;
            float handY = swordArmY + MathF.Sin(handAngle) * 11f;

            canvas.DrawLine(swordArmX, swordArmY, handX, handY, armP);

            float swordLen = 25f;
            float ex = handX + MathF.Cos(handAngle) * swordLen;
            float ey = handY + MathF.Sin(handAngle) * swordLen;

            if (_swinging)
            {
                float swingProgress = _swingTimer / SwingDuration;
                float fadeAlpha = 1f - swingProgress;
                
                // Swing trail
                using var trailP = new SKPaint { Color = new SKColor(150, 200, 255, (byte)(100 * fadeAlpha)), IsAntialias = true, Style = SKPaintStyle.Fill };
                var trailPath = new SKPath();
                trailPath.MoveTo(X, Y);
                float sDeg = (handAngle - 1.3f) * 180f / MathF.PI;
                float wDeg = 2.6f * 180f / MathF.PI;
                trailPath.ArcTo(new SKRect(X - AttackRange, Y - AttackRange, X + AttackRange, Y + AttackRange), sDeg, wDeg, false);
                trailPath.Close();
                canvas.DrawPath(trailPath, trailP);

                // Blade glow
                using var bladeGlow = new SKPaint { Color = new SKColor(200, 230, 255, (byte)(150 * fadeAlpha)),
                    IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 10f, StrokeCap = SKStrokeCap.Round };
                canvas.DrawLine(handX, handY, ex, ey, bladeGlow);
            }

            // Blade (Metallic)
            using var bladeP = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateLinearGradient(
                    new SKPoint(handX, handY), new SKPoint(ex, ey),
                    new SKColor[] { SKColors.White, new SKColor(180, 210, 255) }, null, SKShaderTileMode.Clamp),
                Style = SKPaintStyle.Stroke, StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round
            };
            canvas.DrawLine(handX, handY, ex, ey, bladeP);

            // Guard & Hilt
            float px = -MathF.Sin(handAngle) * 6f, py = MathF.Cos(handAngle) * 6f;
            using var goldP = new SKPaint { Color = new SKColor(255, 215, 0), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 4f, StrokeCap = SKStrokeCap.Round };
            canvas.DrawLine(handX - px, handY - py, handX + px, handY + py, goldP);
            canvas.DrawCircle(handX, handY, 2.5f, goldP);

            // ── Head & Helmet ────────────────────────────────────
            using var skinP = new SKPaint { Color = new SKColor(255, 220, 190), IsAntialias = true };
            canvas.DrawCircle(X, Y - 13 + bob, 9, skinP);

            using var helmetFill = new SKPaint
            {
                IsAntialias = true,
                Shader = SKShader.CreateRadialGradient(
                    new SKPoint(X - 3, Y - 20 + bob), 10,
                    new SKColor[] { new SKColor(150, 200, 255), new SKColor(50, 90, 210) },
                    null, SKShaderTileMode.Clamp)
            };
            var helmetPath = new SKPath();
            helmetPath.AddArc(new SKRect(X - 10, Y - 23 + bob, X + 10, Y - 13 + bob), 180, 180);
            helmetPath.Close();
            canvas.DrawPath(helmetPath, helmetFill);

            // Helmet Crest (Red)
            using var crestP = new SKPaint { Color = new SKColor(220, 40, 40), IsAntialias = true };
            var crestPath = new SKPath();
            crestPath.MoveTo(X - 2, Y - 23 + bob);
            crestPath.LineTo(X + 2, Y - 23 + bob);
            crestPath.LineTo(X,     Y - 29 + bob);
            crestPath.Close();
            canvas.DrawPath(crestPath, crestP);

            // Visor
            using var visor = new SKPaint { Color = new SKColor(10, 10, 30, 220), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2.5f };
            canvas.DrawLine(X - 6, Y - 14 + bob, X + 6, Y - 14 + bob, visor);

            // ── Restore All Canvas Transforms ────────────────────
            canvas.Restore(); // Restore Tilt
            canvas.Restore(); // Restore Flip

            DrawHealthBar(canvas, bob);
        }

        private void DrawHealthBar(SKCanvas canvas, float bob)
        {
            float w = 34, h = 5;
            float bx = X - w/2, by = Y - 28 + bob;
            float pct = (float)Health / MaxHealth;

            using var bg = new SKPaint { Color = new SKColor(20,20,20,200), IsAntialias = true };
            var bgRect = new SKRoundRect(new SKRect(bx - 1, by - 1, bx + w + 1, by + h + 1), 3, 3);
            canvas.DrawRoundRect(bgRect, bg);

            var fc = pct > 0.5f ? new SKColor(50, 220, 80)
                   : pct > 0.2f ? new SKColor(255, 180, 30)
                                : new SKColor(255, 50, 50);
            using var fg = new SKPaint { Color = fc, IsAntialias = true };
            if (pct > 0)
            {
                var fgRect = new SKRoundRect(new SKRect(bx, by, bx + w * pct, by + h), 2, 2);
                canvas.DrawRoundRect(fgRect, fg);
            }
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * Math.Clamp(t, 0, 1);

        private static float AngleDiff(float a, float b)
        {
            float d = a - b;
            while (d >  MathF.PI) d -= 2 * MathF.PI;
            while (d < -MathF.PI) d += 2 * MathF.PI;
            return d;
        }
    }
}
