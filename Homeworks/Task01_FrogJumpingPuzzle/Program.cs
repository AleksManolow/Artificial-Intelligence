using System.Drawing;

namespace Task01_FrogJumpingPuzzle
{
	public class Program
	{
		static void Main(string[] args)
		{
			int n = int.Parse(Console.ReadLine());

			char[] board = GenerateBoard(n);

			List<string> path = new List<string>();

			DFS(board, n, path);

			Console.WriteLine(string.Join(Environment.NewLine, path.AsEnumerable().Reverse().ToList()));

			return;
		}
		static char[] GenerateBoard(int n)
		{
			char[] board = new char[(n * 2) + 1];
			for (int i = 0; i < n; i++)
			{
				board[i] = '>';
				board[board.Length - i - 1] = '<';
			}
			board[n] = '_';
			return board;
		}
		static bool IsGoalState(char[] board, int zeroState)
		{
			for (int i = 0; i < board.Length / 2; i++)
			{
				if (board[i] != '<') return false;
				if (board[board.Length - i - 1] != '>') return false;
			}
			return board[zeroState] == '_';
		}
		static List<(char[], int)> Moves(char[] board, int zeroState)
		{
			List<(char[], int)> moves = new List<(char[], int)>();

			if (zeroState > 0 && board[zeroState - 1] == '>') 
			{
				moves.Add(MakeMove(board, zeroState, zeroState - 1));
			}
			if (zeroState > 1 && board[zeroState - 2] == '>') 
			{
				moves.Add(MakeMove(board, zeroState, zeroState - 2));
			}

			if (zeroState < board.Length - 1 && board[zeroState + 1] == '<')
			{
				moves.Add(MakeMove(board, zeroState, zeroState + 1));
			}
			if (zeroState < board.Length - 2 && board[zeroState + 2] == '<')
			{
				moves.Add(MakeMove(board, zeroState, zeroState + 2));
			}

			return moves;
		}
		static (char[], int) MakeMove(char[] board, int zeroIndex, int targetIndex)
		{
			char[] newBoard = (char[])board.Clone();
			newBoard[zeroIndex] = newBoard[targetIndex];
			newBoard[targetIndex] = '_';
			return (newBoard, targetIndex);
		}
		static bool DFS(char[] board, int zeroState, List<string> path)
		{
			if(IsGoalState(board, zeroState))
				return true;

			List<(char[], int)> moves = Moves(board, zeroState);
			foreach (var move in moves)
			{
				if (DFS(move.Item1, move.Item2, path))
				{
					path.Add(new string(move.Item1));
					return true;
				}
			}

			return false;
		}
		
	}
}
