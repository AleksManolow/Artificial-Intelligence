using System;
using System.Drawing;

namespace Task02_SlidingBlocksNPuzzle
{
	class Board
    {
		private readonly int[,] tiles;
		private readonly int n;
		private readonly int finalValueZero;
		private Point finalBlankPos;
		private Point currBlankPos;

		public Board(int k, int[,] blocks)
        {
			n = blocks.GetLength(0);

			tiles = new int[n, n];
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
					tiles[i, j] = blocks[i,j];

			//Value Final Blank
			finalValueZero = k;

			//Find Final Blank Pos
			if (finalValueZero == -1)
			{
				finalBlankPos = new Point(n - 1, n - 1);
				finalValueZero = n * n - 1;
			}
			else
			{
				finalBlankPos = new Point(finalValueZero / n, finalValueZero % n);
			}

			//Find Curr Blank Pos
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					if (tiles[i, j] == 0)
					{
						currBlankPos = new Point(i, j);
					}
				}
			}
		}

		public int Size => n;

		public override string ToString()
		{
			string result = "";
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					result += tiles[i, j] + " ";
				}
				result += "\n";
			}
			return result;
		}

		public int Hamming()
		{
			int hammingDistance = 0;
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					int expectedValue = i * n + j + 1;
					//Add this if because for the final position of zero, no value should be considered as long as it is non zero 
					if (i == finalBlankPos.X && j == finalBlankPos.Y && tiles[i, j] != 0)
					{
						hammingDistance++;
						continue;
					}

					if (finalValueZero + 1 <= expectedValue)
						expectedValue--;

					if (tiles[i, j] != 0 && tiles[i, j] != expectedValue)
						hammingDistance++;
				}
			}
			return hammingDistance;
		}

		public int Manhattan()
		{
			int manhattanDistance = 0;
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					int value = tiles[i, j];
					if (value != 0)
					{
						int targetRow;
						int targetCol;
						if (finalValueZero + 1 <= value)
						{
							targetRow = value / n;
							targetCol = value % n;
						}
						else
						{
							targetRow = (value - 1) / n;
							targetCol = (value - 1) % n;
						}
						
						manhattanDistance += Math.Abs(i - targetRow) + Math.Abs(j - targetCol);
					}
				}
			}
			return manhattanDistance;
		}

		public bool IsGoal()
		{
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					// if the number is greater than or equal to finalValueZero + 1 fix cuurTitle numeration
					if (tiles[i, j] != 0 && (finalValueZero + 1 <= tiles[i, j] ? tiles[i, j] != i * n + j : tiles[i, j] != i * n + j + 1))
						return false;
				}
			}
			return true;
		}

		public List<(Board, string)> NeighborsWithMoves()
		{
			List<(Board, string)> neighbors = new List<(Board, string)>();
			int[] dx = { -1, 1, 0, 0 };
			int[] dy = { 0, 0, -1, 1 };
			string[] directions = { "down", "up", "right", "left" };

			for (int i = 0; i < 4; i++)
			{
				int newRow = currBlankPos.X + dx[i];
				int newCol = currBlankPos.Y + dy[i];
				if (newRow >= 0 && newRow < n && newCol >= 0 && newCol < n)
				{
					int[,] newTiles = CloneTiles();
					newTiles[currBlankPos.X, currBlankPos.Y] = newTiles[newRow, newCol];
					newTiles[newRow, newCol] = 0;
					neighbors.Add((new Board(finalValueZero, newTiles), directions[i]));
				}
			}

			return neighbors;
		}

		public bool IsSolvable()
		{
			int inversions = CountInversions();

			if (n % 2 != 0) 
			{
				return inversions % 2 == 0;
			}
			else 
			{
				int blankRowFromBottom = n - currBlankPos.X;

				return (inversions + blankRowFromBottom) % 2 != 0;
			}
		}

		private int[,] CloneTiles()
		{
			int[,] copy = new int[n, n];
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					copy[i, j] = tiles[i, j];
				}
			}
			return copy;
		}

		private int CountInversions()
		{
			List<int> oneDArray = new List<int>();

			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					if (tiles[i, j] != 0)
					{
						oneDArray.Add(tiles[i, j]);
					}
				}
			}

			// Number of inversions
			int inversions = 0;
			for (int i = 0; i < oneDArray.Count - 1; i++)
			{
				for (int j = i + 1; j < oneDArray.Count; j++)
				{
					if (oneDArray[i] > oneDArray[j])
					{
						inversions++;
					}
				}
			}

			return inversions;
		}
	}
}
