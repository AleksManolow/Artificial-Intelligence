namespace Task05_TicTacToe
{
	class Program
    {
        static char[,] board = {
            { ' ', ' ', ' ' },
            { ' ', ' ', ' ' },
            { ' ', ' ', ' ' }
        };
        static char player = 'X';
        static char computer = 'O';

        static void Main(string[] args)
        {
            Console.WriteLine("Choose who plays first (1 - Player or 2 - Computer): ");
            int choice = int.Parse(Console.ReadLine());

            bool isPlayerTurn = choice == 1;

            while (true)
            {
                DisplayBoard();

                if (CheckWin(player))
                {
                    Console.WriteLine("Player wins!");
                    break;
                }
                if (CheckWin(computer))
                {
                    Console.WriteLine("Computer wins!");
                    break;
                }
                if (IsBoardFull())
                {
                    Console.WriteLine("It's a draw!");
                    break;
                }

                if (isPlayerTurn)
                {
                    PlayerMove();
                }
                else
                {
                    Console.WriteLine("Computer play:");
                    ComputerMove();
                }

                isPlayerTurn = !isPlayerTurn;
            }
        }

        static void DisplayBoard()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.Write(board[i, j]);
                    if (j < 2) Console.Write("|");
                }
                Console.WriteLine();
                if (i < 2) Console.WriteLine("-----");
            }
        }

        static void PlayerMove()
        {
            int x, y;
            while (true)
            {
                Console.WriteLine("Enter move: ");
                x = int.Parse(Console.ReadLine()) - 1;
                y = int.Parse(Console.ReadLine()) - 1;
                if (x >= 0 && x < 3 && y >= 0 && y < 3 && board[x, y] == ' ')
                {
                    board[x, y] = player;
                    break;
                }
                Console.WriteLine("Invalid move, try again.");
            }
        }

        static void ComputerMove()
        {
            int bestScore = int.MinValue;
            int moveX = -1, moveY = -1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == ' ')
                    {
                        board[i, j] = computer;
                        int score = Minimax(board, 0, int.MinValue, int.MaxValue, false);
                        board[i, j] = ' ';

                        if (score > bestScore)
                        {
                            bestScore = score;
                            moveX = i;
                            moveY = j;
                        }
                    }
                }
            }

            board[moveX, moveY] = computer;
        }

        static int Minimax(char[,] board, int depth, int alpha, int beta, bool isMaximizing)
        {
            if (CheckWin(computer)) return 10 - depth;
            if (CheckWin(player)) return depth - 10;
            if (IsBoardFull()) return 0;

            if (isMaximizing)
            {
                int maxEval = int.MinValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == ' ')
                        {
                            board[i, j] = computer;
                            int eval = Minimax(board, depth + 1, alpha, beta, false);
                            board[i, j] = ' ';
                            maxEval = Math.Max(maxEval, eval);
                            alpha = Math.Max(alpha, eval);
                            if (beta <= alpha) break;
                        }
                    }
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == ' ')
                        {
                            board[i, j] = player;
                            int eval = Minimax(board, depth + 1, alpha, beta, true);
                            board[i, j] = ' ';
                            minEval = Math.Min(minEval, eval);
                            beta = Math.Min(beta, eval);
                            if (beta <= alpha) break;
                        }
                    }
                }
                return minEval;
            }
        }

        static bool CheckWin(char symbol)
        {
            for (int i = 0; i < 3; i++)
                if ((board[i, 0] == symbol && board[i, 1] == symbol && board[i, 2] == symbol) ||
                    (board[0, i] == symbol && board[1, i] == symbol && board[2, i] == symbol))
                    return true;
           
            if ((board[0, 0] == symbol && board[1, 1] == symbol && board[2, 2] == symbol) ||
                (board[0, 2] == symbol && board[1, 1] == symbol && board[2, 0] == symbol))
                return true;

            return false;
        }

        static bool IsBoardFull()
        {
            foreach (char cell in board)
                if (cell == ' ') return false;
            return true;
        }
    }
}
