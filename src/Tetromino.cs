using System.Diagnostics;
using Avalonia.Media;

namespace Tetris;

public class Tetromino(TetrominoType type)
{
	public readonly TetrominoType Type = type;
	public readonly IBrush Color = GetColor(type);
	public int X { get; set; }
	public int Y { get; set; }

	public Direction Facing = Direction.Up;
	public int RemainingLockMoves;
	public LockTimer? PlacementLock;

	public class LockTimer
	{
		public readonly Stopwatch Clock = Stopwatch.StartNew();
		public TimeSpan RemainingTime => TimeSpan.FromMilliseconds(500) - Clock.Elapsed;
	}

	public static IBrush GetColor(TetrominoType type) =>
		type switch
		{
			TetrominoType.I => Brushes.Cyan,
			TetrominoType.J => Brushes.DeepSkyBlue,
			TetrominoType.L => Brushes.Orange,
			TetrominoType.O => Brushes.Yellow,
			TetrominoType.S => Brushes.Lime,
			TetrominoType.Z => Brushes.Red,
			TetrominoType.T => Brushes.Magenta,
			_ => throw new ArgumentOutOfRangeException(),
		};

	public IEnumerable<(int x, int y)> GetShape() => GetShape(0, 0);

	public IEnumerable<(int x, int y)> GetShape(int dx, int dy) => GetShape(dx, dy, Facing);

	public IEnumerable<(int x, int y)> GetShape(int dx, int dy, Direction facing)
	{
		var (shape, weight) = ((byte, byte))(
			Type switch
			{
				TetrominoType.I => (0b0000_1111, 0),
				TetrominoType.J => (0b1000_1110, 1),
				TetrominoType.L => (0b0010_1110, 1),
				TetrominoType.O => (0b1100_1100, 2),
				TetrominoType.S => (0b0110_1100, 1),
				TetrominoType.Z => (0b1100_0110, 1),
				TetrominoType.T => (0b0100_1110, 1),
				_ => throw new ArgumentOutOfRangeException(),
			}
		);

		dx += X;
		dy += Y;

		Func<int, int, (int, int)> rotateFunc = facing switch
		{
			Direction.Up => (x, y) => (x + dx, y + dy),
			Direction.Right => (x, y) => (3 - y + dx - weight, x + dy),
			Direction.Down => (x, y) => (3 - x + dx - weight, 3 - y + dy - weight),
			Direction.Left => (x, y) => (y + dx, 3 - x + dy - weight),
			_ => throw new UnreachableException(),
		};

		for (int row = 0; row < 4; row++)
		for (int col = 0; col < 4; col++)
		{
			int i = (4 * row) + col;
			if ((byte)((shape >> (7 - i)) & 1) != 0)
				yield return rotateFunc(col, row);
		}
	}
}

public enum TetrominoType : byte
{
	I = (byte)'I',
	J = (byte)'J',
	L = (byte)'L',
	O = (byte)'O',
	S = (byte)'S',
	Z = (byte)'Z',
	T = (byte)'T',
}
