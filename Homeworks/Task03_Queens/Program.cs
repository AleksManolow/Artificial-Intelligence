using System.Diagnostics;
using System.Text;

namespace Task03_Queens
{
	public class Program
	{
		static int[] InitQueens(int n)
		{
			Random rand = new Random();

			int[] queens = new int[n];
			for (int col = 0; col < n; col++)
			{
				queens[col] = rand.Next(n);
			}
				
			return queens;
		}

		static void InitializeConflicts(int[] queens, int[] rowConflicts, int[] diag1Conflicts, int[] diag2Conflicts)
		{
			int n = queens.Length;
			for (int col = 0; col < n; col++)
			{
				int row = queens[col];
				rowConflicts[row]++;
				diag1Conflicts[col + row]++;
				diag2Conflicts[col - row + n - 1]++;
			}
		}

		static int GetColWithQueenWithMaxConflicts(int[] queens, int[] rowConflicts, int[] diag1Conflicts, int[] diag2Conflicts)
		{
			Random rand = new Random();

			int maxConflicts = -1;
			int maxConflictCol = -1;
			int n = queens.Length;

			for (int col = 0; col < n; col++)
			{
				int row = queens[col];
				int conflicts = rowConflicts[row] + diag1Conflicts[col + row] + diag2Conflicts[col - row + n - 1] - 3;
				if (conflicts > maxConflicts)
				{
					maxConflicts = conflicts;
					maxConflictCol = col;
				}
				else if (conflicts == maxConflicts && rand.Next(2) == 0)
				{
					maxConflictCol = col;
				}
			}
			return maxConflictCol;
		}

		static int GetRowWithMinConflicts(int col, int[] queens, int[] rowConflicts, int[] diag1Conflicts, int[] diag2Conflicts)
		{
			Random rand = new Random();

			int n = queens.Length;
			int minConflicts = n;
			int bestRow = queens[col];

			for (int row = 0; row < n; row++)
			{
				int conflicts = rowConflicts[row] + diag1Conflicts[col + row] + diag2Conflicts[col - row + n - 1];
				if (conflicts < minConflicts)
				{
					minConflicts = conflicts;
					bestRow = row;
				}
				else if (conflicts == minConflicts && rand.Next(2) == 0)
				{
					bestRow = row;
				}
			}
			return bestRow;
		}

		static int[] SolveNQueens(int n, int maxSteps)
		{
			int[] queens = InitQueens(n);
			int[] rowConflicts = new int[n];
			int[] diag1Conflicts = new int[2 * n - 1];
			int[] diag2Conflicts = new int[2 * n - 1];

			InitializeConflicts(queens, rowConflicts, diag1Conflicts, diag2Conflicts);

			int iter = 0;

			while (iter++ <= maxSteps)
			{
				int col = GetColWithQueenWithMaxConflicts(queens, rowConflicts, diag1Conflicts, diag2Conflicts);

				if (rowConflicts[queens[col]] + diag1Conflicts[col + queens[col]] + diag2Conflicts[col - queens[col] + n - 1] - 3 == 0)
					return queens;

				int row = GetRowWithMinConflicts(col, queens, rowConflicts, diag1Conflicts, diag2Conflicts);

				if (row != queens[col])
				{
					
					rowConflicts[queens[col]]--;
					diag1Conflicts[col + queens[col]]--;
					diag2Conflicts[col - queens[col] + n - 1]--;

					queens[col] = row;

					rowConflicts[row]++;
					diag1Conflicts[col + row]++;
					diag2Conflicts[col - row + n - 1]++;
				}
			}

			return SolveNQueens(n, maxSteps);
		}

		static void PrintSolution(int[] queens)
		{
			for (int row = 0; row < queens.Length; row++)
			{
				for (int col = 0; col < queens.Length; col++)
				{

					Console.Write(queens[col] == row ? "*" : "_");
				}
				Console.WriteLine();
			}
		}

		static void Main()
		{
			int n = int.Parse(Console.ReadLine());
			int maxSteps = n * 2;

			Stopwatch stopwatch = Stopwatch.StartNew();
			int[] solution = SolveNQueens(n, maxSteps);
			stopwatch.Stop();

			Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:F2}");

			if (n < 100)
			{
				/*StringBuilder strSolution = new StringBuilder();
				strSolution.Append("[");
				strSolution.Append(string.Join(", ", solution));
				strSolution.Append(']');
				Console.WriteLine(strSolution.ToString());*/

				PrintSolution(solution);
			}
		}
	}
}
