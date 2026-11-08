using Avalonia;
using Avalonia.Controls;
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
		var mainMenu = new MainMenu();

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

		mainMenu.StartGame += (_, level) =>
		{
			window.Content = gameView ??= new(new TetrisGame(level));
			gameView.ExitGame += (_, __) =>
			{
				window.Content = mainMenu;
				gameView = null;
			};
		};

		app.Run(window);
	}
}
