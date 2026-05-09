using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SkiaSharp;
using System.Diagnostics;
using TowerDefense.Exceptions;
using TowerDefense.Managers;
using TowerDefense.Towers;

namespace TowerDefense.Views
{
    public class GameCanvas : Control
    {
        // ── Game reference (değiştirilebilir) ─────────────────
        private GameManager? _game;
        private MapManager?  _map;

        // ── Interaction state ─────────────────────────────────
        private string? _selectedType;
        private Tower?  _selectedTower;
        private float   _snapX, _snapY;
        private bool    _canBuild;

        private readonly Dictionary<string, float> _towerRanges = new()
        {
            { "Arrow", 110f },
            { "Cannon", 90f },
            { "Ice", 100f },
            { "Laser", 130f },
            { "Bomb", 80f },
            { "Sniper", 220f }
        };

        // ── Game loop ─────────────────────────────────────────
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private double _lastMs;
        private readonly DispatcherTimer _timer;

        // ── Events ────────────────────────────────────────────
        public event Action<string>? OnMessage;
        public event Action?         OnStateChanged;

        public Tower? SelectedTower => _selectedTower;

        public GameCanvas()
        {
            Focusable = true;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        // ── Harita değişince yeni game ver ────────────────────
        public void SetGame(GameManager game)
        {
            _game          = game;
            _map           = game.Map;
            _selectedType  = null;
            _selectedTower = null;
            _lastMs        = _sw.Elapsed.TotalMilliseconds;
            InvalidateVisual();
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            if (_game == null) return;
            try
            {
                double now = _sw.Elapsed.TotalMilliseconds;
                float  dt  = Math.Min((float)((now - _lastMs) / 1000.0), 0.05f);
                _lastMs = now;
                _game.Update(dt);
                InvalidateVisual();
                OnStateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Game loop error: {ex.Message}");
            }
        }

        // ── Render ────────────────────────────────────────────
        public override void Render(DrawingContext context)
        {
            if (_game == null) return;

            int w = (int)Bounds.Width;
            int h = (int)Bounds.Height;
            if (w <= 0 || h <= 0) return;

            using var bmp    = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(new SKColor(60, 110, 45));

            _game.Draw(canvas);
            DrawPreview(canvas);
            DrawSelection(canvas);
            canvas.Flush();

            // SKBitmap → Avalonia WriteableBitmap
            var avBmp = new WriteableBitmap(
                new PixelSize(w, h), new Vector(96, 96),
                PixelFormat.Bgra8888, AlphaFormat.Premul);

            using (var buf = avBmp.Lock())
            {
                unsafe
                {
                    Buffer.MemoryCopy(
                        (void*)bmp.GetPixels(),
                        (void*)buf.Address,
                        (long)buf.RowBytes * buf.Size.Height,
                        (long)bmp.RowBytes * bmp.Height);
                }
            }
            context.DrawImage(avBmp, new Rect(Bounds.Size));
        }

        private void DrawPreview(SKCanvas c)
        {
            if (_selectedType == null || _map == null) return;
            byte r = _canBuild ? (byte)0   : (byte)255;
            byte g = _canBuild ? (byte)255 : (byte)0;

            // Range preview
            if (_towerRanges.TryGetValue(_selectedType, out float range))
            {
                using var rc = new SKPaint { Color = new SKColor(r, g, 0, 30), IsAntialias = true };
                using var rl = new SKPaint { Color = new SKColor(r, g, 0, 150), IsAntialias = true,
                    Style = SKPaintStyle.Stroke, StrokeWidth = 2.0f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 8, 4 }, 0) };
                c.DrawCircle(_snapX, _snapY, range, rc);
                c.DrawCircle(_snapX, _snapY, range, rl);
            }

            using var fill   = new SKPaint { Color = new SKColor(r, g, 0, 50),  IsAntialias = true };
            using var stroke = new SKPaint { Color = new SKColor(r, g, 0, 220), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            c.DrawRect(_snapX - 18, _snapY - 18, 36, 36, fill);
            c.DrawRect(_snapX - 18, _snapY - 18, 36, 36, stroke);
        }

        private void DrawSelection(SKCanvas c)
        {
            if (_selectedTower == null || !_selectedTower.IsAlive) return;
            using var p = new SKPaint { Color = SKColors.Yellow, Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.5f, IsAntialias = true };
            c.DrawRect(_selectedTower.X - 20, _selectedTower.Y - 20, 40, 40, p);
        }

        // ── Mouse ─────────────────────────────────────────────
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (_map == null) return;
            var pt = e.GetPosition(this);
            (_snapX, _snapY) = _map.Snap((float)pt.X, (float)pt.Y);
            _canBuild = _map.IsBuildable(_snapX, _snapY);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (_game == null || _map == null) return;
            var pt = e.GetCurrentPoint(this);
            float mx = (float)pt.Position.X;
            float my = (float)pt.Position.Y;

            if (pt.Properties.IsLeftButtonPressed)
            {
                if (_selectedType != null)
                {
                    TryPlace();
                }
                else
                {
                    if (!TrySelect(mx, my))
                    {
                        if (_game.PlayerHero.Health > 0)
                            _game.HeroMeleeAttack(mx, my);
                    }
                }
            }
            else if (pt.Properties.IsRightButtonPressed)
            {
                CancelSelection();
            }
        }

        // Key handlers moved to GameWindow for global capture

        // ── Public API ────────────────────────────────────────
        public void SelectTowerType(string type)
        {
            _selectedType  = type;
            _selectedTower = null;
            if (_game != null)
                foreach (var t in _game.Towers) t.IsSelected = false;
        }

        public void CancelSelection()
        {
            _selectedType  = null;
            _selectedTower = null;
            if (_game != null)
                foreach (var t in _game.Towers) t.IsSelected = false;
        }

        // ── Private ───────────────────────────────────────────
        private void TryPlace()
        {
            if (_game == null) return;
            try
            {
                Tower tower = _selectedType switch
                {
                    "Arrow"  => new ArrowTower(_snapX, _snapY),
                    "Cannon" => new CannonTower(_snapX, _snapY),
                    "Ice"    => new IceTower(_snapX, _snapY),
                    "Laser"  => new LaserTower(_snapX, _snapY),
                    "Bomb"   => new BombTower(_snapX, _snapY),
                    "Sniper" => new SniperTower(_snapX, _snapY),
                    _ => throw new GameException("Bilinmeyen kule tipi")
                };
                _game.PlaceTower(tower);
                OnMessage?.Invoke($"✅ {tower.TowerName} yerleştirildi!");
            }
            catch (InsufficientGoldException ex) { OnMessage?.Invoke($"❌ {ex.Message}"); }
            catch (TowerPlacementException   ex) { OnMessage?.Invoke($"❌ {ex.Message}"); }
            catch (GameException             ex) { OnMessage?.Invoke($"❌ {ex.Message}"); }
        }

        private bool TrySelect(float mx, float my)
        {
            if (_game == null) return false;
            foreach (var t in _game.Towers) t.IsSelected = false;
            _selectedTower = _game.Towers.FirstOrDefault(t =>
            {
                float dx = t.X - mx, dy = t.Y - my;
                return MathF.Sqrt(dx*dx + dy*dy) < 24;
            });
            if (_selectedTower != null)
            {
                _selectedTower.IsSelected = true;
                OnMessage?.Invoke($"🏰 {_selectedTower.TowerName} | Lv.{_selectedTower.Level} | " +
                                  $"Yükselt:{_selectedTower.UpgradeCost}💰 | Sat:{_selectedTower.SellValue}💰");
                return true;
            }
            return false;
        }
    }
}
