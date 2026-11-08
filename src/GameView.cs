using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Tetris;

public sealed class GameView : Canvas
{
	private readonly DispatcherTimer _timer;
	private readonly TetrisGame _game;
	private readonly DrawingSurface _drawing;
	private readonly TextBlock _scoreLabel = new()
	{
		Text = "Score: 0",
		FontSize = 14,
		Foreground = Brushes.White,
		Padding = new Thickness(4),
		HorizontalAlignment = HorizontalAlignment.Left,
	};
	private readonly TextBlock _levelLabel = new()
	{
		Text = "Level: 1",
		FontSize = 14,
		Foreground = Brushes.White,
		Padding = new Thickness(4),
		HorizontalAlignment = HorizontalAlignment.Left,
	};
	private readonly StackPanel _pauseMenu;

	public event EventHandler<int>? ExitGame;

	public GameView(TetrisGame game)
	{
		Focusable = true;
		MinWidth = 320;
		MinHeight = 640;

		_drawing = new DrawingSurface(game)
		{
			MinWidth = MinWidth,
			MinHeight = MinHeight,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
		};

		var resumeButton = new Button()
		{
			Content = "Resume",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 160,
		};

		var exitButton = new Button()
		{
			Content = "End Game",
			FontSize = 18,
			HorizontalAlignment = HorizontalAlignment.Center,
			HorizontalContentAlignment = HorizontalAlignment.Center,
			Width = 160,
		};

		Children.AddRange([
			new StackPanel
			{
				MinWidth = MinWidth,
				MinHeight = MinHeight,
				Orientation = Orientation.Vertical,
				Children =
				{
					new StackPanel
					{
						HorizontalAlignment = HorizontalAlignment.Stretch,
						Orientation = Orientation.Horizontal,
						Children = { _scoreLabel, _levelLabel },
					},
					_drawing,
				},
			},
			_pauseMenu = new StackPanel
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Orientation = Orientation.Vertical,
				IsVisible = false,
				ZIndex = 1,
				Background = Brushes.Navy,
				Margin = new Thickness(80, 280),
				Children =
				{
					new TextBlock
					{
						Text = "Paused",
						FontSize = 32,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
					},
					resumeButton,
					exitButton,
				},
			},
		]);

		resumeButton.Click += (_, __) => TogglePauseMenu();
		exitButton.Click += (_, __) => ExitGame?.Invoke(this, game.Score);

		_game = game;
		_game.ScoreUpdated += (_, score) => _scoreLabel.Text = $"Score: {score}";
		_game.LevelUpdated += (_, level) => _levelLabel.Text = $"Level: {level}";
		_levelLabel.Text = $"Level: {_game.Level}";

		_game.GameOver += (_, score) =>
		{
			_scoreLabel.Text = $"Game Over! Final score: {score}";
		};

		_timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
		_timer.Tick += delegate
		{
			_drawing.InvalidateVisual();
			if (!_game.Step())
				_timer.Stop();
		};
	}

	private void TogglePauseMenu()
	{
		if (_game.IsGameOver)
			return;
		if (_pauseMenu.IsVisible = _game.IsPaused = !_game.IsPaused)
		{
			_drawing.InvalidateVisual();
			_timer.Stop();
		}
		else
		{
			_drawing.InvalidateVisual();
			_timer.Start();
			Focus();
		}
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);
		_game._clock.Start();
		_timer.Start();
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);
		Focus();
	}

	private sealed class DrawingSurface(TetrisGame game) : Control
	{
		private double _cellSize;

		protected override void OnSizeChanged(SizeChangedEventArgs e)
		{
			base.OnSizeChanged(e);
			InvalidateVisual();
			_cellSize = double.Min(
				e.NewSize.Width / TetrisGame.Columns,
				e.NewSize.Height / TetrisGame.Rows
			);
		}

		public override void Render(DrawingContext context)
		{
			base.Render(context);
			game.Render(context, _cellSize);
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);

		e.Handled = true;
		switch (e.Key)
		{
			default:
				e.Handled = false;
				return;

			case Key.Escape:
				if (_game.IsGameOver)
					ExitGame?.Invoke(this, _game.Score);
				else
					TogglePauseMenu();
				return;

			case Key.Up:
				if (_game.Rotate())
					_drawing.InvalidateVisual();
				return;

			case Key.LeftCtrl:
			case Key.Z:
				if (_game.Rotate(reverse: true))
					_drawing.InvalidateVisual();
				return;

			case Key.Left:
				if (_game.Move(Direction.Left))
					_drawing.InvalidateVisual();
				return;

			case Key.Right:
				if (_game.Move(Direction.Right))
					_drawing.InvalidateVisual();
				return;

			case Key.Down:
				if (_game.Move(Direction.Down))
					_drawing.InvalidateVisual();
				return;

			case Key.Space:
				_game.HardDrop();
				_drawing.InvalidateVisual();
				return;
		}
	}
}
