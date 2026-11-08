using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Tetris;

public sealed class MainMenu : Viewbox
{
	public event EventHandler<int>? StartGame;

	public MainMenu()
	{
		VerticalAlignment = VerticalAlignment.Center;
		Margin = new Thickness(20);

		var tetrisLabel = new TextBlock
		{
			Text = "Tetris",
			FontSize = 48,
			HorizontalAlignment = HorizontalAlignment.Center,
		};

		int level = 1;

		var startGameButton = new Button
		{
			Content = "Start Game",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 160,
		};

		startGameButton.Click += (_, __) => StartGame?.Invoke(this, level);

		var countLabel = new TextBlock
		{
			Text = "Level: 1",
			FontSize = 24,
			HorizontalAlignment = HorizontalAlignment.Center,
		};

		var incrementButton = new Button
		{
			Content = "+",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 80,
		};
		incrementButton.Click += delegate
		{
			if (level < 20)
				countLabel.Text = $"Level: {++level}";
		};

		var decrementButton = new Button
		{
			Content = "-",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 80,
		};
		decrementButton.Click += delegate
		{
			if (level > 1)
				countLabel.Text = $"Level: {--level}";
		};

		var buttonRow = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Center,
			Spacing = 12,
			Children = { decrementButton, incrementButton },
		};

		var controlsInfoButton = new Button()
		{
			Content = "Controls",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 160,
		};

		var mainMenuContent = new StackPanel
		{
			Spacing = 20,
			Children = { tetrisLabel, startGameButton, controlsInfoButton, countLabel, buttonRow },
		};

		var controlsInfoContent = new ControlsInfo();
		controlsInfoButton.Click += (_, __) => Child = controlsInfoContent;
		controlsInfoContent.ReturnButton.Click += (_, __) => Child = mainMenuContent;

		Child = mainMenuContent;
	}
}

public sealed class ControlsInfo : StackPanel
{
	public readonly Button ReturnButton = new()
	{
		Content = "<- Return",
		FontSize = 18,
		HorizontalAlignment = HorizontalAlignment.Center,
		HorizontalContentAlignment = HorizontalAlignment.Center,
		Width = 160,
	};

	public ControlsInfo()
	{
		VerticalAlignment = VerticalAlignment.Center;
		Spacing = 20;

		var headerLabel = new TextBlock
		{
			Text = "Controls",
			FontSize = 48,
			HorizontalAlignment = HorizontalAlignment.Center,
		};

		var textBox = new TextBlock
		{
			Text = TextContent,
			TextAlignment = TextAlignment.Center,
			TextWrapping = TextWrapping.Wrap,
			FontSize = 24,
		};

		Children.AddRange([headerLabel, textBox, ReturnButton]);
	}

	const string TextContent =
		@"
Left and right arrow keys: Piece shifting
Up arrow key: Rotating 90 degrees clockwise
Down arrow key: Non-locking soft drop
Space bar: Locking hard drop
Z key / Left Control key: Rotating 90 degrees counterclockwise
";
}
