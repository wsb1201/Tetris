using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;

namespace Tetris;

internal static class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<GameApp>().UsePlatformDetect().LogToTrace();

	sealed class GameApp : Application
	{
		public GameApp() => Styles.Add(new FluentTheme());
	}

	static void AppMain(Application app, string[] args)
	{
		var tetrisLabel = new TextBlock
		{
			Text = "Tetris",
			FontSize = 48,
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		var startButton = new Button
		{
			Content = "Start Game",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 160,
		};

		int count = 0;
		var countLabel = new TextBlock
		{
			Text = "0",
			FontSize = 48,
			HorizontalAlignment = HorizontalAlignment.Center,
		};

		var incrementButton = new Button
		{
			Content = "Increment",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 160,
		};
		incrementButton.Click += delegate
		{
			count++;
			countLabel.Text = count.ToString();
		};

		var decrementButton = new Button
		{
			Content = "Decrement",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 160,
		};
		decrementButton.Click += delegate
		{
			count--;
			countLabel.Text = count.ToString();
		};

		var resetButton = new Button
		{
			Content = "Reset",
			FontSize = 14,
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		resetButton.Click += delegate
		{
			count = 0;
			countLabel.Text = "0";
		};

		var buttonRow = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Center,
			Spacing = 12,
			Children = { incrementButton, decrementButton },
		};

		var mainMenu = new StackPanel
		{
			VerticalAlignment = VerticalAlignment.Center,
			Spacing = 20,
			Children = { tetrisLabel, startButton, countLabel, buttonRow, resetButton },
		};

		var window = new Window
		{
			Title = "Tetris",
			Width = 320,
			Height = 668,
			CanResize = false,
			WindowStartupLocation = WindowStartupLocation.CenterScreen,
			Content = mainMenu,
		};

		GameView? gameView = null;

		startButton.Click += delegate
		{
			window.Content = gameView ??= new(new TetrisGame());
		};

		app.Run(window);
	}
}
