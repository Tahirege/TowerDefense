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
        public GameManager? Game;
        public MapManager?  Map;

        public string? SelectedType;
        public Tower?  SelectedTowerObj;
        public float   SnapX, SnapY;
        public bool    CanBuild;

        public Dictionary<string, float> TowerRanges = new()
        {
            { "Arrow", 110f },
            { "Cannon", 90f },
            { "Ice", 100f },
            { "Laser", 130f },
            { "Bomb", 80f },
            { "Sniper", 220f }
        };

        public Stopwatch Sw = Stopwatch.StartNew();
        public double LastMs;
        public DispatcherTimer Timer;

        public event Action<string>? OnMessage;
        public event Action?         OnStateChanged;

        public Tower? SelectedTower => SelectedTowerObj;

        public GameCanvas()
        {
            Focusable = true;
            Timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            Timer.Tick += TimerTick;
            Timer.Start();
        }

        public void SetGame(GameManager game)
        {
            Game          = game;
            Map           = game.Map;
            SelectedType  = null;
            SelectedTowerObj = null;
            LastMs        = Sw.Elapsed.TotalMilliseconds;
            InvalidateVisual();
        }

        public void TimerTick(object? sender, EventArgs e)
        {
            if (Game == null) return;
            try
            {
                double now = Sw.Elapsed.TotalMilliseconds;
                float  dt  = Math.Min((float)((now - LastMs) / 1000.0), 0.05f);
                LastMs = now;
                Game.Update(dt);
                InvalidateVisual();
                OnStateChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Game loop error: {ex.Message}");
            }
        }

        public override void Render(DrawingContext context)
        {
            if (Game == null) return;
            int w = (int)Bounds.Width;
            int h = (int)Bounds.Height;
            if (w <= 0 || h <= 0) return;

            using var bmp    = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(new SKColor(60, 110, 45));

            Game.Draw(canvas);
            DrawPreview(canvas);
            DrawSelection(canvas);
            canvas.Flush();

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

        public void DrawPreview(SKCanvas c)
        {
            if (SelectedType == null || Map == null) return;
            byte r = CanBuild ? (byte)0   : (byte)255;
            byte g = CanBuild ? (byte)255 : (byte)0;

            if (TowerRanges.TryGetValue(SelectedType, out float range))
            {
                using var rc = new SKPaint { Color = new SKColor(r, g, 0, 30), IsAntialias = true };
                using var rl = new SKPaint { Color = new SKColor(r, g, 0, 150), IsAntialias = true,
                    Style = SKPaintStyle.Stroke, StrokeWidth = 2.0f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 8, 4 }, 0) };
                c.DrawCircle(SnapX, SnapY, range, rc);
                c.DrawCircle(SnapX, SnapY, range, rl);
            }

            using var fill   = new SKPaint { Color = new SKColor(r, g, 0, 50),  IsAntialias = true };
            using var stroke = new SKPaint { Color = new SKColor(r, g, 0, 220), IsAntialias = true,
                Style = SKPaintStyle.Stroke, StrokeWidth = 2f };
            c.DrawRect(SnapX - 18, SnapY - 18, 36, 36, fill);
            c.DrawRect(SnapX - 18, SnapY - 18, 36, 36, stroke);
        }

        public void DrawSelection(SKCanvas c)
        {
            if (SelectedTowerObj == null || !SelectedTowerObj.IsAlive) return;
            using var p = new SKPaint { Color = SKColors.Yellow, Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.5f, IsAntialias = true };
            c.DrawRect(SelectedTowerObj.X - 20, SelectedTowerObj.Y - 20, 40, 40, p);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (Map == null) return;
            var pt = e.GetPosition(this);
            (SnapX, SnapY) = Map.Snap((float)pt.X, (float)pt.Y);
            CanBuild = Map.IsBuildable(SnapX, SnapY);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (Game == null || Map == null) return;
            var pt = e.GetCurrentPoint(this);
            float mx = (float)pt.Position.X;
            float my = (float)pt.Position.Y;

            if (pt.Properties.IsLeftButtonPressed)
            {
                if (SelectedType != null) TryPlace();
                else if (!TrySelect(mx, my))
                {
                    if (Game.PlayerHero.Health > 0) Game.HeroMeleeAttack(mx, my);
                }
            }
            else if (pt.Properties.IsRightButtonPressed) CancelSelection();
        }

        public void SelectTowerType(string type)
        {
            SelectedType  = type;
            SelectedTowerObj = null;
            if (Game != null)
                foreach (var t in Game.Towers) t.IsSelected = false;
        }

        public void CancelSelection()
        {
            SelectedType  = null;
            SelectedTowerObj = null;
            if (Game != null)
                foreach (var t in Game.Towers) t.IsSelected = false;
        }

        public void TryPlace()
        {
            if (Game == null) return;
            try
            {
                Tower tower = SelectedType switch
                {
                    "Arrow"  => new ArrowTower(SnapX, SnapY),
                    "Cannon" => new CannonTower(SnapX, SnapY),
                    "Ice"    => new IceTower(SnapX, SnapY),
                    "Laser"  => new LaserTower(SnapX, SnapY),
                    "Bomb"   => new BombTower(SnapX, SnapY),
                    "Sniper" => new SniperTower(SnapX, SnapY),
                    _ => throw new GameException("Bilinmeyen kule tipi")
                };
                Game.PlaceTower(tower);
                OnMessage?.Invoke($"✅ {tower.TowerName} yerleştirildi!");
            }
            catch (InsufficientGoldException ex) { OnMessage?.Invoke($"❌ {ex.Message}"); }
            catch (TowerPlacementException   ex) { OnMessage?.Invoke($"❌ {ex.Message}"); }
            catch (GameException             ex) { OnMessage?.Invoke($"❌ {ex.Message}"); }
        }

        public bool TrySelect(float mx, float my)
        {
            if (Game == null) return false;
            foreach (var t in Game.Towers) t.IsSelected = false;
            SelectedTowerObj = Game.Towers.FirstOrDefault(t =>
            {
                float dx = t.X - mx, dy = t.Y - my;
                return MathF.Sqrt(dx*dx + dy*dy) < 24;
            });
            if (SelectedTowerObj != null)
            {
                SelectedTowerObj.IsSelected = true;
                OnMessage?.Invoke($"🏰 {SelectedTowerObj.TowerName} | Lv.{SelectedTowerObj.Level} | " +
                                  $"Yükselt:{SelectedTowerObj.UpgradeCost}💰 | Sat:{SelectedTowerObj.SellValue}💰");
                return true;
            }
            return false;
        }
    }
}
