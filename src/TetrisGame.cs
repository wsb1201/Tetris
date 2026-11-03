using System.Diagnostics;
using Avalonia;
using Avalonia.Media;

namespace Tetris;

public class TetrisGame
{
	internal readonly Stopwatch _clock = new();

	public int Score { get; private set; }
	public int Level { get; private set; } = 1;

	public const int Columns = 10;
	public const int Rows = 20;
	private readonly TetrominoType[,] _playfield = new TetrominoType[Rows, Columns];

	private Tetromino? _currentPiece;

	private readonly IEnumerator<TetrominoType> _sequence = SequenceGenerator(Random.Shared)
		.GetEnumerator();

	public event EventHandler<int>? GameOver;

	public event EventHandler<int>? ScoreUpdated;

	private void IncrementScore(int additional)
	{
		if (additional > 0)
		{
			Score += additional;
			ScoreUpdated?.Invoke(this, Score);
		}
	}

	public void Render(DrawingContext context, double CellSize)
	{
		void DrawCellExplicit(int x, int y, IBrush? color, IBrush? penColor)
		{
			var rect = new Rect(x * CellSize + 1, y * CellSize + 1, CellSize - 2, CellSize - 2);
			context.DrawRectangle(color, new Pen(penColor, 1), rect);
		}
		void DrawCell(int x, int y, IBrush? color) =>
			DrawCellExplicit(x, y, color, Brushes.DimGray);

		for (int row = 0; row < Rows; row++)
		for (int col = 0; col < Columns; col++)
		{
			if (_playfield[row, col] is not 0 and TetrominoType i)
				DrawCell(col, row, Tetromino.GetColor(i));
			else
				DrawCellExplicit(col, row, Brushes.Black, Brushes.White);
		}

		if (_currentPiece != null)
		{
			var ghostY = 0;
			while (!Collides(_currentPiece, 0, ++ghostY)) { }
			if (--ghostY > 0)
				foreach (var (x, y) in _currentPiece.GetShape(0, ghostY))
					DrawCellExplicit(x, y, Brushes.Black, _currentPiece.Color);

			foreach (var (x, y) in _currentPiece.GetShape())
				DrawCell(x, y, _currentPiece.Color);
		}
	}

	public bool Move(Direction direction)
	{
		if (
			!_clock.IsRunning
			|| _currentPiece is null
			|| (_currentPiece.PlacementLock != null && _currentPiece.RemainingLockMoves < 1)
		)
			return false;

		switch (direction)
		{
			case Direction.Up:
			case Direction.Left when Collides(_currentPiece, -1, 0):
			case Direction.Right when Collides(_currentPiece, +1, 0):
			case Direction.Down when Collides(_currentPiece, 0, +1):
				return false;

			case Direction.Left:
				_currentPiece.X -= 1;
				break;
			case Direction.Right:
				_currentPiece.X += 1;
				break;
			case Direction.Down:
				IncrementScore(1);
				_currentPiece.Y += 1;
				break;
		}

		if (_currentPiece.PlacementLock != null)
		{
			_currentPiece.RemainingLockMoves -= 1;
			_currentPiece.PlacementLock.Clock.Restart();
		}

		return true;
	}

	public bool Rotate(bool reverse = false)
	{
		if (
			!_clock.IsRunning
			|| _currentPiece is null
			|| _currentPiece.Type is TetrominoType.O
			|| (_currentPiece.PlacementLock != null && _currentPiece.RemainingLockMoves < 1)
		)
			return false;

		var rotated = (Direction)(((int)_currentPiece.Facing + (reverse ? -1 : 1) + 4) % 4);

		if (!Collides(_currentPiece, 0, 0, rotated))
		{
			_currentPiece.Facing = rotated;
			if (_currentPiece.PlacementLock != null)
			{
				_currentPiece.RemainingLockMoves -= 1;
				_currentPiece.PlacementLock.Clock.Restart();
			}
			return true;
		}

		(int x, int y)[] tryRotations =
			_currentPiece.Type is TetrominoType.I
				? (_currentPiece.Facing, rotated) switch
				{
					(Direction.Up, Direction.Right) or (Direction.Left, Direction.Down) => (
						[(-2, 0), (+1, 0), (-2, -1), (+1, +2)]
					),
					(Direction.Right, Direction.Up) or (Direction.Down, Direction.Left) => (
						[(+2, 0), (-1, 0), (+2, +1), (-1, -2)]
					),
					(Direction.Right, Direction.Down) or (Direction.Up, Direction.Left) => (
						[(-1, 0), (+2, 0), (-1, +2), (+2, -1)]
					),
					(Direction.Down, Direction.Right) or (Direction.Left, Direction.Up) => (
						[(+1, 0), (-2, 0), (+1, -2), (-2, +1)]
					),
					_ => throw new UnreachableException(),
				}
				: (_currentPiece.Facing, rotated) switch
				{
					(Direction.Up or Direction.Down, Direction.Right) => (
						[(-1, 0), (-1, +1), (0, -2), (-1, -2)]
					),
					(Direction.Right, Direction.Up or Direction.Down) => (
						[(+1, 0), (+1, -1), (0, +2), (+1, +2)]
					),
					(Direction.Up or Direction.Down, Direction.Left) => (
						[(+1, 0), (+1, +1), (0, -2), (+1, -2)]
					),
					(Direction.Left, Direction.Up or Direction.Down) => (
						[(-1, 0), (-1, -1), (0, +2), (-1, +2)]
					),
					_ => throw new UnreachableException(),
				};

		var (dx, dy) = Array.Find(
			tryRotations,
			(offset) => !Collides(_currentPiece, offset.x, offset.y, rotated)
		);
		if ((dx, dy) is (0, 0))
			return false;

		_currentPiece.X += dx;
		_currentPiece.Y += dy;
		_currentPiece.Facing = rotated;

		if (_currentPiece.PlacementLock != null)
		{
			_currentPiece.RemainingLockMoves -= 1;
			_currentPiece.PlacementLock.Clock.Restart();
		}

		return true;
	}

	public void HardDrop()
	{
		if (!_clock.IsRunning || _currentPiece is null)
			return;

		var score = 0;
		while (!Collides(_currentPiece, 0, +1))
		{
			score += 2;
			_currentPiece.Y++;
		}

		IncrementScore(score);
		LockTetromino();
		NextTetromino();
	}

	private bool Collides(Tetromino piece) => Collides(piece, 0, 0);

	private bool Collides(Tetromino piece, int dx, int dy) => Collides(piece, dx, dy, piece.Facing);

	private bool Collides(Tetromino piece, int dx, int dy, Direction facing) =>
		piece
			.GetShape(dx, dy, facing)
			.Any(pos =>
				pos is (< 0, _) or (>= Columns, _) or (_, >= Rows)
				|| (pos.y is >= 0 && _playfield[pos.y, pos.x] is not 0)
			);

	public bool Step()
	{
		if (!_clock.IsRunning)
			return false;
		if (_currentPiece == null)
			return NextTetromino();

		if (!Collides(_currentPiece, 0, +1))
		{
			if (_currentPiece.PlacementLock != null)
			{
				_currentPiece.PlacementLock = null;
				_clock.Restart();
			}
			if (_clock.Elapsed >= TimeSpan.FromMilliseconds(500))
			{
				_clock.Restart();
				_currentPiece.Y++;
			}
			return true;
		}

		_currentPiece.PlacementLock ??= new();
		if (_currentPiece.PlacementLock.RemainingTime > TimeSpan.Zero)
			return true;

		LockTetromino();
		return NextTetromino();
	}

	public bool NextTetromino()
	{
		if (!_clock.IsRunning || _currentPiece != null)
			return false;

		_sequence.MoveNext();
		_currentPiece = new(_sequence.Current) { X = Columns / 2 - 2, RemainingLockMoves = 15 };

		if (!Collides(_currentPiece))
		{
			_clock.Restart();
			return true;
		}

		_currentPiece = null;
		_clock.Reset();
		GameOver?.Invoke(this, Score);
		return false;
	}

	private void LockTetromino()
	{
		if (_currentPiece == null)
			return;

		foreach (var (x, y) in _currentPiece.GetShape())
			if (y is >= 0 and < Rows && x is >= 0 and < Columns)
				_playfield[y, x] = _currentPiece.Type;

		_currentPiece = null;
		int rowsCleared = 0;

		for (int row = Rows - 1; row >= 0; row--)
		{
			bool fullRow = true;
			for (int col = 0; col < Columns; col++)
				if (_playfield[row, col] == 0)
					fullRow = false;

			if (!fullRow)
				continue;

			for (int y = row; y > 0; y--)
			for (int col = 0; col < Columns; col++)
				_playfield[y, col] = _playfield[y - 1, col];

			for (int col = 0; col < Columns; col++)
				_playfield[0, col] = 0;

			rowsCleared++;
			row++; // Check this row again after shifting.
		}

		switch (rowsCleared)
		{
			default:
				throw new UnreachableException();
			case 0:
				break;
			case 1: // Single
				IncrementScore(100 * Level);
				break;
			case 2: // Double
				IncrementScore(300 * Level);
				break;
			case 3: // Triple
				IncrementScore(500 * Level);
				break;
			case 4: // Tetris
				IncrementScore(800 * Level);
				break;
		}
	}

	private static IEnumerable<TetrominoType> SequenceGenerator(Random random)
	{
		TetrominoType[] nextSequence = [.. "IJLOSZT"u8.ToArray().Select(i => (TetrominoType)i)];
		Queue<TetrominoType> queue = new(nextSequence.Length);

		random.Shuffle(nextSequence);

		while (true)
		{
			if (!queue.Any())
			{
				queue = new(nextSequence);
				random.Shuffle(nextSequence);
			}
			yield return queue.Dequeue();
		}
	}
}
