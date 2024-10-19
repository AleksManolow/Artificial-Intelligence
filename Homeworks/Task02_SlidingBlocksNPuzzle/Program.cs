using System.Diagnostics;

namespace Task02_SlidingBlocksNPuzzle
{
	class Program
	{
		static void Main(string[] args)
		{
			int N = int.Parse(Console.ReadLine());
			int k = int.Parse(Console.ReadLine());
			int size = (int)Math.Sqrt(N + 1);
			int[,] board = new int[size, size];
			for (int i = 0; i < size; i++)
			{
				var input = Console.ReadLine().Split();
				for (int j = 0; j < size; j++)
				{
					board[i, j] = int.Parse(input[j]);
				}
			}

			Board puzzle = new Board(k, board);
			if (!puzzle.IsSolvable())
			{
				Console.WriteLine("-1");
				return;
			}

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			List<string> solution = IDAStar(puzzle);

			stopwatch.Stop();

			if (solution == null)
			{
				Console.WriteLine("-1");
			}
			else
			{
				Console.WriteLine(solution.Count);
				foreach (var step in solution)
				{
					Console.WriteLine(step);
				}
			}

			Console.WriteLine($"Time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
		}

		static List<string> IDAStar(Board start)
		{
			int threshold = start.Manhattan();
			while (true)
			{
				(int newThreshold, List<string> path) = Search(start, 0, threshold, new List<string>());
				if (path != null) return path;
				if (newThreshold == int.MaxValue) return null;
				threshold = newThreshold;
			}
		}

		static (int, List<string>) Search(Board board, int g, int threshold, List<string> path)
		{
			int f = g + board.Manhattan();
			if (f > threshold) return (f, null);
			if (board.IsGoal()) return (f, path);

			int min = int.MaxValue;

			foreach (var (neighbor, move) in board.NeighborsWithMoves())
			{
				if (path.Count > 0 && OppositeMove(path[path.Count - 1]) == move)
					continue;

				path.Add(move);
				(int newThreshold, List<string> result) = Search(neighbor, g + 1, threshold, path);
				if (result != null) return (newThreshold, result);
				path.RemoveAt(path.Count - 1);

				if (newThreshold < min) min = newThreshold;
			}

			return (min, null);
		}

		static string OppositeMove(string move)
		{
			return move switch
			{
				"up" => "down",
				"down" => "up",
				"left" => "right",
				"right" => "left",
				_ => throw new ArgumentException("Invalid move")
			};
		}
	}
}
