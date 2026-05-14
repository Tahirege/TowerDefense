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
        public GameManager        GameObj   = null!;
        public ScoreManager       ScoreObj  = null!;
        public GameCanvas         Canvas;

        public TextBlock LblGold  = StatLbl("💰 200",   "#FFD700");
        public TextBlock LblLives = StatLbl("❤️ 20",    "#FF6B6B");
        public TextBlock LblWave  = StatLbl("🌊 0/0",  "#74B9FF");
        public TextBlock LblKills = StatLbl("💀 0",     "#FFA07A");
        public TextBlock LblScore = StatLbl("⭐ 0",     "#A8E063");

        public Button BtnArrow  = TowerBtn("🏹", "Arrow",  "100g", "#4A90D9", "#2C5F8A");
        public Button BtnCannon = TowerBtn("💣", "Cannon", "200g", "#C0522A", "#7A2A10");
        public Button BtnIce    = TowerBtn("❄️", "Ice",    "150g", "#5BC8F0", "#2A7A9A");
        public Button BtnLaser  = TowerBtn("⚡", "Laser",  "250g", "#E040E0", "#8A1080");
        public Button BtnBomb   = TowerBtn("💥", "Bomb",   "300g", "#808090", "#404050");
        public Button BtnSniper = TowerBtn("🎯", "Sniper", "350g", "#4A8A4A", "#204020");

        public Button BtnUpgrade = ActionBtn("⬆", "Upgrade",  "#27AE60", "#1A7A40");
        public Button BtnSell    = ActionBtn("💰", "Sell",     "#D4A017", "#8A6008");
        public Button BtnWave    = ActionBtn("▶", "Send Wave","#2980B9", "#1A5080");
        public Button BtnPause   = ActionBtn("⏸", "Pause",    "#717D7E", "#404A4B");

        public ListBox LstBoard  = MakeList();
        public TextBlock ToastText = new() { FontSize = 13, Foreground = Brushes.White, TextAlignment = TextAlignment.Center, Margin = new Thickness(16, 8) };
        public Border ToastBorder;
        public DispatcherTimer? ToastTimer;

        public const int SideW = 260;
        public const int TopH  = 48;

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
            Width  = 1024; 
            Height = 720;
            Canvas = new GameCanvas { Width = mapW, Height = mapH };
            Canvas.OnMessage      += msg => Dispatcher.UIThread.Post(() => ShowToast(msg));
            Canvas.OnStateChanged += UpdateUI;
            ToastBorder = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#CC1A1A2E")),
                CornerRadius = new CornerRadius(10), IsVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 20), ZIndex = 99,
                Child  = ToastText
            };
            WireButtons();
            var mainGrid = BuildLayout();
            mainGrid.Width  = targetW;
            mainGrid.Height = targetH;
            Content = new Viewbox { Stretch = Stretch.Uniform, Child = mainGrid };
            SwitchMap(initialMapId);
        }

        public void SwitchMap(string mapId)
        {
            if (GameObj != null)
            {
                GameObj.OnMessage     -= OnGameMsg;
                GameObj.OnGameOver    -= OnGameOver;
                GameObj.OnVictory     -= OnVictory;
            }
            var map = new MapManager(mapId);
            ScoreObj  = new ScoreManager("Player");
            GameObj = new GameManager(map, ScoreObj, new AudioManager());
            GameObj.OnMessage     += OnGameMsg;
            GameObj.OnGameOver    += OnGameOver;
            GameObj.OnVictory     += OnVictory;
            Canvas.SetGame(GameObj);
            BtnPause.Content = MakeBtnContent("⏸", "Pause");
            RefreshBoard(); UpdateUI();
        }

        public void OnGameMsg(string msg) => Dispatcher.UIThread.Post(() => ShowToast(msg));
        public void OnGameOver() => Dispatcher.UIThread.Post(() => { RefreshBoard(); ShowToast($"💀 GAME OVER — Score: {ScoreObj.Score} | Wave: {GameObj.CurrentWave}", 6); });
        public void OnVictory() => Dispatcher.UIThread.Post(() => { RefreshBoard(); ShowToast($"🏆 VICTORY! All waves cleared! Score: {ScoreObj.Score}", 6); });

        public void WireButtons()
        {
            BtnArrow.Click  += (_, _) => { Canvas.SelectTowerType("Arrow");  HighBtn(BtnArrow); };
            BtnCannon.Click += (_, _) => { Canvas.SelectTowerType("Cannon"); HighBtn(BtnCannon); };
            BtnIce.Click    += (_, _) => { Canvas.SelectTowerType("Ice");    HighBtn(BtnIce); };
            BtnLaser.Click  += (_, _) => { Canvas.SelectTowerType("Laser");  HighBtn(BtnLaser); };
            BtnBomb.Click   += (_, _) => { Canvas.SelectTowerType("Bomb");   HighBtn(BtnBomb); };
            BtnSniper.Click += (_, _) => { Canvas.SelectTowerType("Sniper"); HighBtn(BtnSniper); };

            BtnUpgrade.Click += (_, _) =>
            {
                var t = Canvas.SelectedTower;
                if (t == null) { ShowToast("Select a tower first!"); return; }
                try { GameObj.UpgradeTower(t); ShowToast($"⬆ {t.TowerName} → Lv.{t.Level}!"); }
                catch (GameException ex) { ShowToast($"❌ {ex.Message}"); }
            };
            BtnSell.Click += (_, _) =>
            {
                var t = Canvas.SelectedTower;
                if (t == null) { ShowToast("Select a tower first!"); return; }
                ShowToast($"💰 Sold {t.TowerName} for {t.SellValue}g");
                GameObj.SellTower(t); Canvas.CancelSelection();
            };
            BtnWave.Click  += (_, _) => { try { GameObj.SpawnWave(); } catch (GameException ex) { ShowToast($"❌ {ex.Message}"); } };
            BtnPause.Click += (_, _) => { GameObj.TogglePause(); BtnPause.Content = MakeBtnContent(GameObj.State == GameState.Paused ? "▶" : "⏸", GameObj.State == GameState.Paused ? "Resume" : "Pause"); };
        }

        public Grid BuildLayout()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition(TopH, GridUnitType.Pixel));
            grid.RowDefinitions.Add(new RowDefinition(1,    GridUnitType.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(1,     GridUnitType.Star));
            grid.ColumnDefinitions.Add(new ColumnDefinition(SideW, GridUnitType.Pixel));

            var topBar = new Border
            {
                Background = new LinearGradientBrush { StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative), GradientStops = { new GradientStop(Color.Parse("#1A1F26"), 0), new GradientStop(Color.Parse("#11151C"), 1) } },
                BorderBrush = new SolidColorBrush(Color.Parse("#30363D")), BorderThickness = new Thickness(0, 0, 0, 2), Height = TopH,
                Child  = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0), Spacing = 12, Children = { StatGroup("💰", LblGold, "#FFD700"), StatGroup("❤️", LblLives, "#FF6B6B"), StatGroup("🌊", LblWave, "#74B9FF"), StatGroup("💀", LblKills, "#FFA07A"), StatGroup("⭐", LblScore, "#A8E063") } }
            };
            Grid.SetRow(topBar, 0); Grid.SetColumnSpan(topBar, 2);
            grid.Children.Add(topBar);
            Grid.SetRow(Canvas, 1); Grid.SetColumn(Canvas, 0);
            grid.Children.Add(Canvas);
            var side = BuildSidePanel();
            Grid.SetRow(side, 1); Grid.SetColumn(side, 1);
            grid.Children.Add(side);
            Grid.SetRow(ToastBorder, 1);  Grid.SetColumn(ToastBorder, 0);
            grid.Children.Add(ToastBorder);
            return grid;
        }

        public Control BuildSidePanel()
        {
            var towers  = new TabItem { Header = MakeTabHeader("🏰", "Build"),  Content = BuildTowerTab() };
            var maps    = new TabItem { Header = MakeTabHeader("🗺", "Maps"),   Content = BuildMapTab() };
            var scores  = new TabItem { Header = MakeTabHeader("🏆", "Scores"), Content = BuildScoreTab() };
            return new Border { Width = SideW, Background = new SolidColorBrush(Color.Parse("#11151C")), BorderBrush = new SolidColorBrush(Color.Parse("#30363D")), BorderThickness = new Thickness(1, 0, 0, 0), Child = new TabControl { Items = { towers, maps, scores }, Background = Brushes.Transparent, Padding = new Thickness(0) } };
        }

        public Control BuildTowerTab()
        {
            var p = new StackPanel { Spacing = 2, Margin = new Thickness(6, 4) };
            p.Children.Add(SectionLabel("BUILD TOWERS"));
            foreach (var b in new[] { BtnArrow, BtnCannon, BtnIce, BtnLaser, BtnBomb, BtnSniper }) p.Children.Add(b);
            p.Children.Add(SectionLabel("SELECTED TOWER"));
            var towerActions = new Grid();
            towerActions.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            towerActions.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(BtnUpgrade, 0); Grid.SetColumn(BtnSell, 1);
            towerActions.Children.Add(BtnUpgrade); towerActions.Children.Add(BtnSell);
            p.Children.Add(towerActions);
            p.Children.Add(SectionLabel("GAME"));
            var gameRow = new Grid();
            gameRow.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            gameRow.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            Grid.SetColumn(BtnWave, 0); Grid.SetColumn(BtnPause, 1);
            gameRow.Children.Add(BtnWave); gameRow.Children.Add(BtnPause);
            p.Children.Add(gameRow);
            return p;
        }

        public Control BuildMapTab()
        {
            var p = new StackPanel { Spacing = 5, Margin = new Thickness(6) };
            p.Children.Add(SectionLabel("SELECT MAP"));
            foreach (var map in MapLibrary.All)
            {
                string id = map.Id, name = map.Name;
                var inner = new StackPanel { Spacing = 2 };
                inner.Children.Add(new TextBlock { Text = map.Name, FontSize = 12, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#E8BD44")) });
                inner.Children.Add(new TextBlock { Text = map.Description, FontSize = 10, Foreground = new SolidColorBrush(Color.Parse("#8B949E")), TextWrapping = TextWrapping.Wrap });
                inner.Children.Add(new TextBlock { Text = $"⚡ x{map.EnemySpeedMultiplier:F1}  🌊 {map.MaxWaves} waves", FontSize = 10, Foreground = new SolidColorBrush(Color.Parse("#58A6FF")) });
                var btn = new Button { Content = inner, HorizontalAlignment = HorizontalAlignment.Stretch, Foreground = Brushes.White, Margin = new Thickness(0, 2), Padding = new Thickness(8, 6), CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1) };
                btn.Classes.Add("map-btn");
                btn.Click += (_, _) => { SwitchMap(id); ShowToast($"🗺 {name} loaded!"); };
                p.Children.Add(btn);
            }
            return p;
        }

        public Control BuildScoreTab()
        {
            LstBoard.Background  = new SolidColorBrush(Color.Parse("#0D1117"));
            LstBoard.Foreground  = new SolidColorBrush(Color.Parse("#A8E063"));
            LstBoard.FontFamily  = new FontFamily("Courier New");
            LstBoard.FontSize    = 11;
            return LstBoard;
        }

        public void UpdateUI()
        {
            Dispatcher.UIThread.Post(() =>
            {
                LblGold.Text  = $"{GameObj.Gold}";
                LblLives.Text = $"{GameObj.Lives}";
                LblWave.Text  = $"{GameObj.CurrentWave}/{GameObj.MaxWaves}";
                LblKills.Text = $"{GameObj.TotalKills}";
                LblScore.Text = $"{ScoreObj.Score}";
                BtnWave.IsEnabled = !GameObj.IsWaveInProgress && GameObj.State == GameState.Playing;
            });
        }

        public void ShowToast(string msg, double seconds = 2.5)
        {
            ToastText.Text        = msg;
            ToastBorder.IsVisible = true;
            ToastTimer?.Stop();
            ToastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            ToastTimer.Tick += (_, _) => { ToastBorder.IsVisible = false; ToastTimer?.Stop(); };
            ToastTimer.Start();
        }

        public void RefreshBoard() => LstBoard.ItemsSource = ScoreObj.Leaderboard.Select(r => $"{r.PlayerName,-8} {r.Score,6}  W{r.Wave}  {r.Date:MM/dd}").ToList();

        public void HighBtn(Button active)
        {
            foreach (var b in new[] { BtnArrow, BtnCannon, BtnIce, BtnLaser, BtnBomb, BtnSniper }) { b.BorderBrush = new SolidColorBrush(Color.Parse("#30363D")); b.BorderThickness = new Thickness(1); }
            active.BorderBrush = Brushes.White;
            active.BorderThickness = new Thickness(2);
        }

        public static TextBlock StatLbl(string text, string color) => new() { Text = text, FontSize = 14, FontWeight = FontWeight.ExtraBold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0) };

        public static Control StatGroup(string icon, TextBlock lbl, string color)
        {
            return new Border { Background = new SolidColorBrush(Color.Parse("#2D333B")) { Opacity = 0.5 }, CornerRadius = new CornerRadius(6), Padding = new Thickness(8, 4), Child = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, Children = { new TextBlock { Text = icon, FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Color.Parse(color)) }, lbl } } };
        }

        public static Control MakeBtnContent(string icon, string label) => new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center, Spacing = 0, Children = { new TextBlock { Text = icon,  FontSize = 16, TextAlignment = TextAlignment.Center }, new TextBlock { Text = label, FontSize = 10, FontWeight = FontWeight.Bold, TextAlignment = TextAlignment.Center } } };

        public static Control MakeTabHeader(string icon, string label) => new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center, Width = 62, Margin = new Thickness(0, 4), Spacing = 1, Children = { new TextBlock { Text = icon,  FontSize = 16, TextAlignment = TextAlignment.Center }, new TextBlock { Text = label, FontSize = 10, FontWeight = FontWeight.Bold, TextAlignment = TextAlignment.Center, Foreground = Brushes.White } } };

        public static TextBlock SectionLabel(string text) => new() { Text = text, FontSize = 9, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#58A6FF")), Margin = new Thickness(4, 8, 0, 4), LetterSpacing = 1.1 };

        public static Button TowerBtn(string icon, string name, string cost, string accentColor, string darkColor) => new() { Height = 44, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 3), Padding = new Thickness(12, 6), CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1), Classes = { "tower-btn" }, Content = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Children = { Placed(new TextBlock { Text = icon, FontSize = 22, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,10,0) }, 0), Placed(new StackPanel { VerticalAlignment = VerticalAlignment.Center, Children = { new TextBlock { Text = name, FontSize = 13, FontWeight = FontWeight.Bold }, new TextBlock { Text = "Tower", FontSize = 9 } } }, 1), Placed(new Border { Background = new SolidColorBrush(Color.Parse("#2D333B")), CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 2), Child = new TextBlock { Text = cost, FontSize = 11, FontWeight = FontWeight.SemiBold, Foreground = new SolidColorBrush(Color.Parse("#E8BD44")) } }, 2) } } };

        public static Button ActionBtn(string icon, string label, string bgColor, string darkBg) => new() { Height = 36, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(2, 4), Padding = new Thickness(4, 2), CornerRadius = new CornerRadius(8), Classes = { "action-btn" }, Content = MakeBtnContent(icon, label) };

        public static Control Placed(Control c, int col) { Grid.SetColumn(c, col); return c; }

        public static ListBox MakeList() => new() { Background = new SolidColorBrush(Color.Parse("#0D1117")), Foreground = Brushes.White, FontSize = 11 };

        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            if (GameObj == null) return;
            switch (e.Key)
            {
                case Avalonia.Input.Key.W: GameObj.PlayerHero.MovingUp = true; break;
                case Avalonia.Input.Key.S: GameObj.PlayerHero.MovingDown = true; break;
                case Avalonia.Input.Key.A: GameObj.PlayerHero.MovingLeft = true; break;
                case Avalonia.Input.Key.D: GameObj.PlayerHero.MovingRight = true; break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
        {
            if (GameObj == null) return;
            switch (e.Key)
            {
                case Avalonia.Input.Key.W: GameObj.PlayerHero.MovingUp = false; break;
                case Avalonia.Input.Key.S: GameObj.PlayerHero.MovingDown = false; break;
                case Avalonia.Input.Key.A: GameObj.PlayerHero.MovingLeft = false; break;
                case Avalonia.Input.Key.D: GameObj.PlayerHero.MovingRight = false; break;
            }
            base.OnKeyUp(e);
        }

        public void AddStyles()
        {
            var buttonTransitions = new Transitions { new TransformOperationsTransition { Property = Button.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(120) }, new BrushTransition { Property = Button.BackgroundProperty, Duration = TimeSpan.FromMilliseconds(120) } };
            Styles.Add(new Style(s => s.OfType<Button>()) { Setters = { new Setter(Button.TransitionsProperty, buttonTransitions), new Setter(Button.RenderTransformProperty, TransformOperations.Parse("scale(1)")), new Setter(Button.BorderBrushProperty, new SolidColorBrush(Color.Parse("#30363D"))), new Setter(Button.BorderThicknessProperty, new Thickness(1)), new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#1C2128"))), new Setter(Button.ForegroundProperty, Brushes.White) } });
            Styles.Add(new Style(s => s.OfType<Button>().Class("action-btn")) { Setters = { new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#27AE60"))), new Setter(Button.BorderBrushProperty, new SolidColorBrush(Color.Parse("#1A7A40"))), new Setter(Button.BorderThicknessProperty, new Thickness(1, 1, 1, 3)) } });
            Styles.Add(new Style(s => s.OfType<Button>().Class("map-btn")) { Setters = { new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#1C2128"))) } });
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true)) { Setters = { new Setter(Button.RenderTransformProperty, TransformOperations.Parse("scale(1.04)")), new Setter(Button.ZIndexProperty, 10), new Setter(Button.CursorProperty, new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)), new Setter(Button.ForegroundProperty, new SolidColorBrush(Color.Parse("#0B0E14"))) } });
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true).Descendant().OfType<ContentPresenter>()) { Setters = { new Setter(ContentPresenter.BackgroundProperty, Brushes.White), new Setter(ContentPresenter.BorderBrushProperty, Brushes.White) } });
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true).Descendant().OfType<Border>()) { Setters = { new Setter(Border.BackgroundProperty, Brushes.Transparent) } });
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true).Descendant().OfType<TextBlock>()) { Setters = { new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(Color.Parse("#0B0E14"))) } });
        }
    }
}
