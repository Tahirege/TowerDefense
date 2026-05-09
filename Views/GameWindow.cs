using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Animation;
using Avalonia.Media.Transformation;
using TowerDefense.Audio;
using TowerDefense.Exceptions;
using TowerDefense.Managers;
using TowerDefense.Maps;
using Avalonia.Controls.Presenters;

namespace TowerDefense.Views
{
    public class GameWindow : Window
    {
        private GameManager        _game   = null!;
        private ScoreManager       _score  = null!;

        private readonly GameCanvas _canvas;

        // ── HUD stat labels ───────────────────────────────────────
        private readonly TextBlock _lblGold  = StatLbl("💰 200",   "#FFD700");
        private readonly TextBlock _lblLives = StatLbl("❤️ 20",    "#FF6B6B");
        private readonly TextBlock _lblWave  = StatLbl("🌊 0/0",  "#74B9FF");
        private readonly TextBlock _lblKills = StatLbl("💀 0",     "#FFA07A");

        // ── Tower build buttons ───────────────────────────────────
        private readonly Button _btnArrow  = TowerBtn("🏹", "Arrow",  "100g", "#4A90D9", "#2C5F8A");
        private readonly Button _btnCannon = TowerBtn("💣", "Cannon", "200g", "#C0522A", "#7A2A10");
        private readonly Button _btnIce    = TowerBtn("❄️", "Ice",    "150g", "#5BC8F0", "#2A7A9A");
        private readonly Button _btnLaser  = TowerBtn("⚡", "Laser",  "250g", "#E040E0", "#8A1080");
        private readonly Button _btnBomb   = TowerBtn("💥", "Bomb",   "300g", "#808090", "#404050");
        private readonly Button _btnSniper = TowerBtn("🎯", "Sniper", "350g", "#4A8A4A", "#204020");

        // ── Action buttons ────────────────────────────────────────
        private readonly Button _btnUpgrade = ActionBtn("⬆", "Upgrade",  "#27AE60", "#1A7A40");
        private readonly Button _btnSell    = ActionBtn("💰", "Sell",     "#D4A017", "#8A6008");
        private readonly Button _btnWave    = ActionBtn("▶", "Send Wave","#2980B9", "#1A5080");
        private readonly Button _btnPause   = ActionBtn("⏸", "Pause",    "#717D7E", "#404A4B");

        private readonly ListBox _lstBoard  = MakeList();

        // ── Toast ─────────────────────────────────────────────────
        private readonly TextBlock _toastText = new() { FontSize = 13, Foreground = Brushes.White,
            TextAlignment = TextAlignment.Center, Margin = new Thickness(16, 8) };
        private readonly Border _toastBorder;
        private DispatcherTimer? _toastTimer;

        private const int SideW = 260;
        private const int TopH  = 48;

        public GameWindow(string initialMapId = "classic")
        {
            Title     = "🛡️ Tower Defense";
            CanResize = true;
            WindowState = WindowState.Normal;
            Background = new SolidColorBrush(Color.Parse("#0B0E14"));

            int mapW = MapManager.Cols * MapManager.CellSize;
            int mapH = MapManager.Rows * MapManager.CellSize;
            double targetW = mapW + SideW;
            double targetH = mapH + TopH + 30;

            AddStyles();

            // Set a reasonable default window size; Viewbox will handle the scaling
            Width  = 1024; 
            Height = 720;

            _canvas = new GameCanvas { Width = mapW, Height = mapH };
            _canvas.OnMessage      += msg => Dispatcher.UIThread.Post(() => ShowToast(msg));
            _canvas.OnStateChanged += UpdateUI;

            _toastBorder = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#CC1A1A2E")),
                CornerRadius = new CornerRadius(10), IsVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 20), ZIndex = 99,
                Child  = _toastText
            };

            WireButtons();
            var mainGrid = BuildLayout();
            mainGrid.Width  = targetW;
            mainGrid.Height = targetH;
            Content = new Viewbox { Stretch = Stretch.Uniform, Child = mainGrid };
            SwitchMap(initialMapId);
        }

        private void SwitchMap(string mapId)
        {
            if (_game != null)
            {
                _game.OnMessage     -= OnGameMsg;
                _game.OnGameOver    -= OnGameOver;
                _game.OnVictory     -= OnVictory;
            }

            var map = new MapManager(mapId);
            _score  = new ScoreManager("Player");
            _game = new GameManager(map, _score, new AudioManager());
            _game.OnMessage     += OnGameMsg;
            _game.OnGameOver    += OnGameOver;
            _game.OnVictory     += OnVictory;

            _canvas.SetGame(_game);
            _btnPause.Content = MakeBtnContent("⏸", "Pause");
            RefreshBoard(); UpdateUI();
        }

        private void OnGameMsg(string msg)       => Dispatcher.UIThread.Post(() => ShowToast(msg));
        private void OnGameOver() => Dispatcher.UIThread.Post(() =>
        {
            RefreshBoard();
            ShowToast($"💀 GAME OVER — Score: {_score.Score} | Wave: {_game.CurrentWave}", 6);
        });
        private void OnVictory() => Dispatcher.UIThread.Post(() =>
        {
            RefreshBoard();
            ShowToast($"🏆 VICTORY! All waves cleared! Score: {_score.Score}", 6);
        });

        private void WireButtons()
        {
            _btnArrow.Click  += (_, _) => { _canvas.SelectTowerType("Arrow");  HighBtn(_btnArrow); };
            _btnCannon.Click += (_, _) => { _canvas.SelectTowerType("Cannon"); HighBtn(_btnCannon); };
            _btnIce.Click    += (_, _) => { _canvas.SelectTowerType("Ice");    HighBtn(_btnIce); };
            _btnLaser.Click  += (_, _) => { _canvas.SelectTowerType("Laser");  HighBtn(_btnLaser); };
            _btnBomb.Click   += (_, _) => { _canvas.SelectTowerType("Bomb");   HighBtn(_btnBomb); };
            _btnSniper.Click += (_, _) => { _canvas.SelectTowerType("Sniper"); HighBtn(_btnSniper); };

            _btnUpgrade.Click += (_, _) =>
            {
                var t = _canvas.SelectedTower;
                if (t == null) { ShowToast("Select a tower first!"); return; }
                try { _game.UpgradeTower(t); ShowToast($"⬆ {t.TowerName} → Lv.{t.Level}!"); }
                catch (GameException ex) { ShowToast($"❌ {ex.Message}"); }
            };

            _btnSell.Click += (_, _) =>
            {
                var t = _canvas.SelectedTower;
                if (t == null) { ShowToast("Select a tower first!"); return; }
                ShowToast($"💰 Sold {t.TowerName} for {t.SellValue}g");
                _game.SellTower(t); _canvas.CancelSelection();
            };


            _btnWave.Click  += (_, _) => { try { _game.SpawnWave(); } catch (GameException ex) { ShowToast($"❌ {ex.Message}"); } };
            _btnPause.Click += (_, _) =>
            {
                _game.TogglePause();
                _btnPause.Content = MakeBtnContent(_game.State == GameState.Paused ? "▶" : "⏸",
                                                   _game.State == GameState.Paused ? "Resume" : "Pause");
            };
        }

        private Grid BuildLayout()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition(TopH, GridUnitType.Pixel));
            grid.RowDefinitions.Add(new RowDefinition(1,    GridUnitType.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(1,     GridUnitType.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(SideW, GridUnitType.Pixel));

            // ── Top HUD bar ───────────────────────────────────────
            var topBar = new Border
            {
                Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops =
                    {
                        new GradientStop(Color.Parse("#1A1F26"), 0),
                        new GradientStop(Color.Parse("#11151C"), 1)
                    }
                },
                BorderBrush = new SolidColorBrush(Color.Parse("#30363D")),
                BorderThickness = new Thickness(0, 0, 0, 2),
                Height = TopH,
                Child  = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(12, 0),
                    Spacing = 12,
                    Children = { 
                        StatGroup("💰", _lblGold,  "#FFD700"), 
                        StatGroup("❤️", _lblLives, "#FF6B6B"), 
                        StatGroup("🌊", _lblWave,  "#74B9FF"), 
                        StatGroup("💀", _lblKills, "#FFA07A") 
                    }
                }
            };
            Grid.SetRow(topBar, 0); Grid.SetColumnSpan(topBar, 2);
            grid.Children.Add(topBar);

            Grid.SetRow(_canvas, 1); Grid.SetColumn(_canvas, 0);
            grid.Children.Add(_canvas);

            var side = BuildSidePanel();
            Grid.SetRow(side, 1); Grid.SetColumn(side, 1);
            grid.Children.Add(side);

            Grid.SetRow(_toastBorder, 1);  Grid.SetColumn(_toastBorder, 0);
            grid.Children.Add(_toastBorder);

            return grid;
        }

        private Control BuildSidePanel()
        {
            var towers  = new TabItem { Header = MakeTabHeader("🏰", "Build"),  Content = BuildTowerTab() };
            var maps    = new TabItem { Header = MakeTabHeader("🗺", "Maps"),   Content = BuildMapTab() };

            return new Border
            {
                Width = SideW,
                Background = new SolidColorBrush(Color.Parse("#11151C")),
                BorderBrush = new SolidColorBrush(Color.Parse("#30363D")),
                BorderThickness = new Thickness(1, 0, 0, 0),
                Child = new TabControl
                {
                    Items = { towers, maps },
                    Background = Brushes.Transparent,
                    Padding = new Thickness(0)
                }
            };
        }

        private Control BuildTowerTab()
        {
            var p = new StackPanel { Spacing = 2, Margin = new Thickness(6, 4) };

            p.Children.Add(SectionLabel("BUILD TOWERS"));
            foreach (var b in new[] { _btnArrow, _btnCannon, _btnIce, _btnLaser, _btnBomb, _btnSniper })
                p.Children.Add(b);

            p.Children.Add(SectionLabel("SELECTED TOWER"));
            var towerActions = new Grid();
            towerActions.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            towerActions.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(_btnUpgrade, 0);
            Grid.SetColumn(_btnSell, 1);
            towerActions.Children.Add(_btnUpgrade);
            towerActions.Children.Add(_btnSell);
            p.Children.Add(towerActions);

            p.Children.Add(SectionLabel("GAME"));
            var gameRow = new Grid();
            gameRow.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            gameRow.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(_btnWave, 0); Grid.SetColumn(_btnPause, 1);
            gameRow.Children.Add(_btnWave); gameRow.Children.Add(_btnPause);
            p.Children.Add(gameRow);

            return p;
        }

        private Control BuildMapTab()
        {
            var p = new StackPanel { Spacing = 5, Margin = new Thickness(6) };
            p.Children.Add(SectionLabel("SELECT MAP"));
            foreach (var map in MapLibrary.All)
            {
                string id = map.Id, name = map.Name;
                var inner = new StackPanel { Spacing = 2 };
                inner.Children.Add(new TextBlock { Text = map.Name, FontSize = 12, FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.Parse("#E8BD44")) });
                inner.Children.Add(new TextBlock { Text = map.Description, FontSize = 10,
                    Foreground = new SolidColorBrush(Color.Parse("#8B949E")), TextWrapping = TextWrapping.Wrap });
                inner.Children.Add(new TextBlock
                {
                    Text = $"⚡ x{map.EnemySpeedMultiplier:F1}  🌊 {map.MaxWaves} waves",
                    FontSize = 10, Foreground = new SolidColorBrush(Color.Parse("#58A6FF"))
                });
                var btn = new Button
                {
                    Content = inner,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 2),
                    Padding = new Thickness(8, 6),
                    CornerRadius = new CornerRadius(6),
                    BorderThickness = new Thickness(1)
                };
                btn.Classes.Add("map-btn");
                btn.Click += (_, _) => { SwitchMap(id); ShowToast($"🗺 {name} loaded!"); };
                p.Children.Add(btn);
            }
            return p;
        }

        private Control BuildScoreTab()
        {
            _lstBoard.Background  = new SolidColorBrush(Color.Parse("#0D1117"));
            _lstBoard.Foreground  = new SolidColorBrush(Color.Parse("#A8E063"));
            _lstBoard.FontFamily  = new FontFamily("Courier New");
            _lstBoard.FontSize    = 11;
            return _lstBoard;
        }

        // ── UI updates ────────────────────────────────────────────
        private void UpdateUI()
        {
            Dispatcher.UIThread.Post(() =>
            {
                _lblGold.Text  = $"{_game.Gold}";
                _lblLives.Text = $"{_game.Lives}";
                _lblWave.Text  = $"{_game.CurrentWave}/{_game.MaxWaves}";
                _lblKills.Text = $"{_game.TotalKills}";
                _btnWave.IsEnabled = !_game.IsWaveInProgress && _game.State == GameState.Playing;
            });
        }

        private void ShowToast(string msg, double seconds = 2.5)
        {
            _toastText.Text        = msg;
            _toastBorder.IsVisible = true;
            _toastTimer?.Stop();
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            _toastTimer.Tick += (_, _) => { _toastBorder.IsVisible = false; _toastTimer?.Stop(); };
            _toastTimer.Start();
        }

        private void RefreshBoard() =>
            _lstBoard.ItemsSource = _score.Leaderboard
                .Select(r => $"{r.PlayerName,-8} {r.Score,6}  W{r.Wave}  {r.Date:MM/dd}").ToList();

        private void HighBtn(Button active)
        {
            foreach (var b in new[] { _btnArrow, _btnCannon, _btnIce, _btnLaser, _btnBomb, _btnSniper })
            {
                b.BorderBrush = new SolidColorBrush(Color.Parse("#30363D"));
                b.BorderThickness = new Thickness(1);
            }
            active.BorderBrush = Brushes.White;
            active.BorderThickness = new Thickness(2);
        }

        // ── Static factory helpers ────────────────────────────────
        private static TextBlock StatLbl(string text, string color) => new()
        {
            Text = text,
            FontSize = 14, FontWeight = FontWeight.ExtraBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0)
        };

        private static Control StatGroup(string icon, TextBlock lbl, string color)
        {
            return new Border
            {
                Background = new SolidColorBrush(Color.Parse("#2D333B")) { Opacity = 0.5 },
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 4),
                Child = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Children = {
                        new TextBlock { Text = icon, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.Parse(color)) },
                        lbl
                    }
                }
            };
        }

        private static Control MakeBtnContent(string icon, string label) =>
            new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 0,
                Children =
                {
                    new TextBlock { Text = icon,  FontSize = 16, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = label, FontSize = 10, FontWeight = FontWeight.Bold, TextAlignment = TextAlignment.Center }
                }
            };

        private static Control MakeTabHeader(string icon, string label) =>
            new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 62, // Slightly wider to fit label
                Margin = new Thickness(0, 4),
                Spacing = 1,
                Children =
                {
                    new TextBlock { Text = icon,  FontSize = 16, TextAlignment = TextAlignment.Center },
                    new TextBlock { Text = label, FontSize = 10, FontWeight = FontWeight.Bold, TextAlignment = TextAlignment.Center,
                        Foreground = Brushes.White }
                }
            };

        private static TextBlock SectionLabel(string text) => new()
        {
            Text = text, FontSize = 9, FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#58A6FF")),
            Margin = new Thickness(4, 8, 0, 4),
            LetterSpacing = 1.1
        };

        private static Button TowerBtn(string icon, string name, string cost, string accentColor, string darkColor) =>
            new()
            {
                Height = 44,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 3),
                Padding = new Thickness(12, 6),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                Classes = { "tower-btn" },
                Content = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    Children =
                    {
                        Placed(new TextBlock { Text = icon, FontSize = 22, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,10,0) }, 0),
                        Placed(new StackPanel { 
                            VerticalAlignment = VerticalAlignment.Center,
                            Children = {
                                new TextBlock { Text = name, FontSize = 13, FontWeight = FontWeight.Bold },
                                new TextBlock { Text = "Tower", FontSize = 9 }
                            }
                        }, 1),
                        Placed(new Border {
                            Background = new SolidColorBrush(Color.Parse("#2D333B")),
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(6, 2),
                            Child = new TextBlock { Text = cost, FontSize = 11, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(Color.Parse("#E8BD44")) }
                        }, 2)
                    }
                }
            };

        private static Button ActionBtn(string icon, string label, string bgColor, string darkBg) =>
            new()
            {
                Height = 36,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(2, 4),
                Padding = new Thickness(4, 2),
                CornerRadius = new CornerRadius(8),
                Classes = { "action-btn" },
                Content = MakeBtnContent(icon, label)
            };

        private static Control Placed(Control c, int col)
        { Grid.SetColumn(c, col); return c; }

        private static ListBox MakeList() => new()
        {
            Background = new SolidColorBrush(Color.Parse("#0D1117")),
            Foreground = Brushes.White, FontSize = 11
        };

        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            if (_game == null) return;
            switch (e.Key)
            {
                case Avalonia.Input.Key.W: _game.PlayerHero.MovingUp = true; break;
                case Avalonia.Input.Key.S: _game.PlayerHero.MovingDown = true; break;
                case Avalonia.Input.Key.A: _game.PlayerHero.MovingLeft = true; break;
                case Avalonia.Input.Key.D: _game.PlayerHero.MovingRight = true; break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
        {
            if (_game == null) return;
            switch (e.Key)
            {
                case Avalonia.Input.Key.W: _game.PlayerHero.MovingUp = false; break;
                case Avalonia.Input.Key.S: _game.PlayerHero.MovingDown = false; break;
                case Avalonia.Input.Key.A: _game.PlayerHero.MovingLeft = false; break;
                case Avalonia.Input.Key.D: _game.PlayerHero.MovingRight = false; break;
            }
            base.OnKeyUp(e);
        }
        private void AddStyles()
        {
            // Smooth transitions
            var buttonTransitions = new Transitions
            {
                new TransformOperationsTransition
                {
                    Property = Button.RenderTransformProperty,
                    Duration = TimeSpan.FromMilliseconds(120)
                },
                new BrushTransition
                {
                    Property = Button.BackgroundProperty,
                    Duration = TimeSpan.FromMilliseconds(120)
                }
            };

            // Base button style
            Styles.Add(new Style(s => s.OfType<Button>())
            {
                Setters = {
                    new Setter(Button.TransitionsProperty, buttonTransitions),
                    new Setter(Button.RenderTransformProperty, TransformOperations.Parse("scale(1)")),
                    new Setter(Button.BorderBrushProperty, new SolidColorBrush(Color.Parse("#30363D"))),
                    new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                    new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#1C2128"))),
                    new Setter(Button.ForegroundProperty, Brushes.White)
                }
            });

            // Specific button types
            Styles.Add(new Style(s => s.OfType<Button>().Class("action-btn"))
            {
                Setters = {
                    new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#27AE60"))),
                    new Setter(Button.BorderBrushProperty, new SolidColorBrush(Color.Parse("#1A7A40"))),
                    new Setter(Button.BorderThicknessProperty, new Thickness(1, 1, 1, 3))
                }
            });

            // Map button specific
            Styles.Add(new Style(s => s.OfType<Button>().Class("map-btn"))
            {
                Setters = {
                    new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#1C2128")))
                }
            });

            // Premium Hover Effect - Applies to ALL buttons
            // 1. Scale and Cursor
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true))
            {
                Setters = {
                    new Setter(Button.RenderTransformProperty, TransformOperations.Parse("scale(1.04)")),
                    new Setter(Button.ZIndexProperty, 10),
                    new Setter(Button.CursorProperty, new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)),
                    new Setter(Button.ForegroundProperty, new SolidColorBrush(Color.Parse("#0B0E14")))
                }
            });

            // 2. Background - Targeting ContentPresenter directly to override FluentTheme defaults
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true).Descendant().OfType<ContentPresenter>())
            {
                Setters = {
                    new Setter(ContentPresenter.BackgroundProperty, Brushes.White),
                    new Setter(ContentPresenter.BorderBrushProperty, Brushes.White)
                }
            });

            // 3. Transparent background for internal borders (like cost tags) on hover
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true).Descendant().OfType<Border>())
            {
                Setters = { new Setter(Border.BackgroundProperty, Brushes.Transparent) }
            });

            // Change all text inside hovered buttons to dark
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true).Descendant().OfType<TextBlock>())
            {
                Setters = { new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(Color.Parse("#0B0E14"))) }
            });
        }
    }
}
