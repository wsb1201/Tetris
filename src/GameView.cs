using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Tetris;

public sealed class GameView : StackPanel
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

	public GameView(TetrisGame game)
	{
		Focusable = true;
		Orientation = Orientation.Vertical;

		_drawing = new DrawingSurface(game)
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MinWidth = 320,
			MinHeight = 640,
		};

		Children.Add(_scoreLabel);
		Children.Add(_drawing);

		_game = game;
		_game.ScoreUpdated += (_, score) => _scoreLabel.Text = $"Score: {score}";
		_game.GameOver += (_, score) => _scoreLabel.Text = $"Game Over! Final score: {score}";

		_timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
		_timer.Tick += delegate
		{
			_drawing.InvalidateVisual();
			if (!_game.Step())
				_timer.Stop();
		};
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

	private bool _shiftKeyDown;

	protected override void OnKeyUp(KeyEventArgs e)
	{
		base.OnKeyUp(e);
		switch (e.Key)
		{
			case Key.LeftShift:
				_shiftKeyDown = false;
				e.Handled = true;
				return;
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

			case Key.LeftShift:
				_shiftKeyDown = true;
				return;

			case Key.Escape:
				// TODO: Pause
				return;

			case Key.Up:
				if (_game.Rotate(_shiftKeyDown))
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
