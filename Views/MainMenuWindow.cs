using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Avalonia.Media.Imaging;
using TowerDefense.Maps;
using Avalonia.Controls.Primitives;
using Avalonia.Animation;
using Avalonia.Media.Transformation;
using System.Linq;
using Avalonia.Controls.Presenters;

namespace TowerDefense.Views
{
    public class MainMenuWindow : Window
    {
        public string SelectedMapId = "classic";
        public ListBox MapList = null!;

        public MainMenuWindow()
        {
            Title = "🛡️ Tower Defense - Main Menu";
            Width = 800;
            Height = 600;
            WindowState = WindowState.Normal;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.Parse("#0f172a"));
            AddStyles();
            var mainGrid = BuildUI();
            Content = new Viewbox { Stretch = Stretch.Uniform, Child = mainGrid };
        }

        public Grid BuildUI()
        {
            var grid = new Grid { Width = 800, Height = 700 };
            var bgBorder = new Border
            {
                Background = new LinearGradientBrush { StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative), EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative), GradientStops = new GradientStops { new GradientStop(Color.Parse("#1e293b"), 0), new GradientStop(Color.Parse("#0f172a"), 1) } }
            };
            grid.Children.Add(bgBorder);
            var mainPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Spacing = 12 };
            var title = new TextBlock { Text = "TOWER DEFENSE", FontSize = 42, FontWeight = FontWeight.Black, Foreground = new SolidColorBrush(Color.Parse("#38bdf8")), TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 10), LetterSpacing = 2 };
            mainPanel.Children.Add(title);
            var subtitle = new TextBlock { Text = "Select a map:", FontSize = 18, Foreground = Brushes.White, TextAlignment = TextAlignment.Center };
            mainPanel.Children.Add(subtitle);
            MapList = new ListBox { Width = 500, Height = 260, Background = new SolidColorBrush(Color.Parse("#1e293b")), Foreground = Brushes.White, CornerRadius = new CornerRadius(8), Margin = new Thickness(0, 5, 0, 15), [ScrollViewer.VerticalScrollBarVisibilityProperty] = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };
            foreach (var map in MapLibrary.All)
            {
                var mapItem = new ListBoxItem { Content = new StackPanel { Spacing = 5, Children = { new TextBlock { Text = map.Name, FontSize = 20, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#facc15")) }, new TextBlock { Text = map.Description, FontSize = 14, Foreground = Brushes.LightGray } } }, Tag = map.Id };
                MapList.Items.Add(mapItem);
            }
            MapList.SelectedIndex = 0;
            MapList.SelectionChanged += (s, e) => { if (MapList.SelectedItem is ListBoxItem item && item.Tag is string id) SelectedMapId = id; };
            mainPanel.Children.Add(MapList);
            var startBtn = new Button { Content = "START GAME 🚀", FontSize = 24, FontWeight = FontWeight.Bold, HorizontalAlignment = HorizontalAlignment.Center, Padding = new Thickness(40, 15), CornerRadius = new CornerRadius(12) };
            startBtn.Click += StartGame;
            mainPanel.Children.Add(startBtn);
            grid.Children.Add(mainPanel);
            return grid;
        }

        public void StartGame(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var gameWin = new GameWindow(SelectedMapId);
            gameWin.Show();
            this.Close();
        }

        public void AddStyles()
        {
            var buttonTransitions = new Transitions { new TransformOperationsTransition { Property = Button.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(150) }, new BrushTransition { Property = Button.BackgroundProperty, Duration = TimeSpan.FromMilliseconds(150) } };
            Styles.Add(new Style(s => s.OfType<Button>()) { Setters = { new Setter(Button.TransitionsProperty, buttonTransitions), new Setter(Button.RenderTransformProperty, TransformOperations.Parse("scale(1)")), new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.Parse("#22c55e"))) } });
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true)) { Setters = { new Setter(Button.RenderTransformProperty, TransformOperations.Parse("scale(1.05)")), new Setter(Button.CursorProperty, new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)) } });
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true).Descendant().OfType<ContentPresenter>()) { Setters = { new Setter(ContentPresenter.BackgroundProperty, Brushes.White), new Setter(ContentPresenter.BorderBrushProperty, Brushes.White), new Setter(ContentPresenter.BorderThicknessProperty, new Thickness(2)) } });
            Styles.Add(new Style(s => s.OfType<Button>().PropertyEquals(Button.IsPointerOverProperty, true).Descendant().OfType<TextBlock>()) { Setters = { new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(Color.Parse("#0f172a"))) } });
        }
    }
}
