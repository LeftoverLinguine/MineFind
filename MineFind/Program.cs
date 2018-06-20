using System;
using System.Linq;

namespace MineFind
{
    public enum BoardOptions
    {
        Uninitialized = 0,
        BombCovered,
        NoBombCovered,
        NoBombUncovered
    }

    public struct Board
    {
        public BoardOptions[,] BoardOptions { get; set; }
        public int[,] NumberOfSurroundingBombs { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var continuePlaying = true;

            while(continuePlaying)
            {
                Console.WriteLine();
                Console.Write("Enter an x dimension for the board and press ENTER: ");
                if(!int.TryParse(Console.ReadLine(), out var x))
                {
                    Console.WriteLine("\nInvalid x number was entered.");
                    Console.ReadKey();
                    return;
                }

                Console.Write("Enter a y dimension for the board and press ENTER: ");
                if(!int.TryParse(Console.ReadLine(), out var y))
                {
                    Console.WriteLine("\nInvalid y number was entered.");
                    Console.ReadKey();
                    return;
                }

                Console.Write("Enter the number of bombs for the board and press ENTER: ");
                if(!int.TryParse(Console.ReadLine(), out var numberOfBombs))
                {
                    Console.WriteLine("\nInvalid numberOfBombs number was entered.");
                    Console.ReadKey();
                    return;
                }

                if(numberOfBombs > (x * y))
                {
                    Console.WriteLine("Too many bombs for this board size.");
                    Console.ReadKey();
                    return;
                }

                var board = new Board
                {
                    BoardOptions = new BoardOptions[x, y],
                    NumberOfSurroundingBombs = new int[x, y]
                };

                InitializeBoard(board, numberOfBombs);

                var bombHit = false;
                var gameWon = false;

                while(!bombHit && !gameWon)
                {
                    DrawBoard(board);
                    Console.Write("Enter a position to uncover (row then column separated by a space: ");
                    var move = Console.ReadLine().Split(' ');
                    if((move.Length != 2) || !int.TryParse(move[0], out var xMove) || !int.TryParse(move[1], out var yMove))
                    {
                        Console.WriteLine("Invalid move. GAME OVER!!!");
                        Console.ReadKey();
                        return;
                    }

                    switch(board.BoardOptions[xMove, yMove])
                    {
                        case BoardOptions.NoBombUncovered:
                            /* This is a no-op, since it was already revealed to not have a bomb. */
                            break;
                        case BoardOptions.BombCovered:
                            /* Game over */
                            bombHit = true;
                            break;
                        case BoardOptions.NoBombCovered:
                            UncoverNoBombSpots(board, xMove, yMove);
                            break;
                    }

                    gameWon = IsGameWon(board);
                }

                if(bombHit)
                    Console.WriteLine("You lose!!!");
                else
                    Console.WriteLine("You win!!!");

                DrawBoard(board, true);

                Console.Write("Continue? (y/n)");
                continuePlaying = Console.ReadLine().ToLower() == "y";
            }
        }

        private static bool IsGameWon(Board board)
        {
            var xDimension = board.BoardOptions.GetLength(0);
            var yDimension = board.BoardOptions.GetLength(1);

            for(var x = 0; x < xDimension; ++x)
            {
                for(var y = 0; y < yDimension; ++y)
                {
                    if(board.BoardOptions[x, y] == BoardOptions.NoBombCovered)
                        return false;
                }
            }

            return true;
        }

        private static void UncoverNoBombSpots(Board board, int x, int y)
        {
            /* 
             *  Uncover all of the covered no bomb spots starting at (x, y), assuming that (x, y) is a NoBombCovered spot. 
             */
            board.BoardOptions[x, y] = BoardOptions.NoBombUncovered;

            var maxX = board.BoardOptions.GetLength(0);
            var maxY = board.BoardOptions.GetLength(1);

            if(((y - 1) >= 0) && ((x - 1) >= 0) && (board.BoardOptions[x - 1, y - 1] == BoardOptions.NoBombCovered))
            {
                if(board.NumberOfSurroundingBombs[x - 1, y - 1] == 0)
                    UncoverNoBombSpots(board, x - 1, y - 1);
                else
                    board.BoardOptions[x - 1, y - 1] = BoardOptions.NoBombUncovered;
            }
            if(((y - 1) >= 0) && (board.BoardOptions[x, y - 1] == BoardOptions.NoBombCovered))
            {
                if(board.NumberOfSurroundingBombs[x, y - 1] == 0)
                    UncoverNoBombSpots(board, x, y - 1);
                else
                    board.BoardOptions[x, y - 1] = BoardOptions.NoBombUncovered;
            }
            if(((y - 1) >= 0) && ((x + 1) < maxX) && (board.BoardOptions[x + 1, y - 1] == BoardOptions.NoBombCovered))
            {
                if(board.NumberOfSurroundingBombs[x + 1, y - 1] == 0)
                    UncoverNoBombSpots(board, x + 1, y - 1);
                else
                    board.BoardOptions[x + 1, y - 1] = BoardOptions.NoBombUncovered;
            }
            if(((x + 1) < maxX) && (board.BoardOptions[x + 1, y] == BoardOptions.NoBombCovered))
            {
                if(board.NumberOfSurroundingBombs[x + 1, y] == 0)
                    UncoverNoBombSpots(board, x + 1, y);
                else
                    board.BoardOptions[x + 1, y] = BoardOptions.NoBombUncovered;
            }
            if(((y + 1) < maxY) && ((x + 1) < maxX) && (board.BoardOptions[x + 1, y + 1] == BoardOptions.NoBombCovered))
            {
                if(board.NumberOfSurroundingBombs[x + 1, y + 1] == 0)
                    UncoverNoBombSpots(board, x + 1, y + 1);
                else
                    board.BoardOptions[x + 1, y + 1] = BoardOptions.NoBombUncovered;
            }
            if(((y + 1) < maxY) && (board.BoardOptions[x, y + 1] == BoardOptions.NoBombCovered))
            {
                if(board.NumberOfSurroundingBombs[x, y + 1] == 0)
                    UncoverNoBombSpots(board, x, y + 1);
                else
                    board.BoardOptions[x, y + 1] = BoardOptions.NoBombUncovered;
            }
            if(((y + 1) < maxY) && ((x - 1) >= 0) && (board.BoardOptions[x - 1, y + 1] == BoardOptions.NoBombCovered))
            {
                if(board.NumberOfSurroundingBombs[x - 1, y + 1] == 0)
                    UncoverNoBombSpots(board, x - 1, y + 1);
                else
                    board.BoardOptions[x - 1, y + 1] = BoardOptions.NoBombUncovered;
            }
            if(((x - 1) >= 0) && (board.BoardOptions[x - 1, y] == BoardOptions.NoBombCovered))
            {
                if(board.NumberOfSurroundingBombs[x - 1, y] == 0)
                    UncoverNoBombSpots(board, x - 1, y);
                else
                    board.BoardOptions[x - 1, y] = BoardOptions.NoBombUncovered;
            }
        }

        private static void DrawBoard(Board board, bool uncoverEverything = false)
        {
            var maxX = board.BoardOptions.GetLength(0);
            var maxY = board.BoardOptions.GetLength(1);

            Console.Write(" ");
            for(var y = 0; y < maxY; ++y)
            {
                Console.Write(y);
            }

            Console.WriteLine();

            for(var x = 0; x < maxX; ++x)
            {
                Console.ResetColor();
                Console.Write(x);
                for(var y = 0; y < maxY; ++y)
                {
                    switch(board.BoardOptions[x, y])
                    {
                        case BoardOptions.Uninitialized:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write("X");
                            break;
                        case BoardOptions.BombCovered:
                            if(uncoverEverything)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.Write("*");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write("X");
                            }
                            break;
                        case BoardOptions.NoBombCovered:
                            if(uncoverEverything)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.Write(board.NumberOfSurroundingBombs[x, y] + "");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write("X");
                            }
                            break;
                        case BoardOptions.NoBombUncovered:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(board.NumberOfSurroundingBombs[x, y] + "");
                            break;
                    }
                }

                Console.WriteLine();
            }

            Console.ResetColor();
        }

        private static void InitializeBoard(Board board, int numberOfBombs)
        {
            var x = 0;
            var y = 0;
            var maxX = board.BoardOptions.GetLength(0);
            var maxY = board.BoardOptions.GetLength(1);

            var randomCoordinateIx = -1;
            var random = new Random(Guid.NewGuid().GetHashCode());
            var coordinates = Enumerable.Range(0, maxX)
                .Join(
                    Enumerable.Range(0, maxY),
                    yj => 1,
                    xj => 1,
                    (xj, yj) => (xj, yj))
                .ToList();

            while(numberOfBombs > 0)
            {
                randomCoordinateIx = random.Next(0, coordinates.Count);
                (x, y) = coordinates[randomCoordinateIx];
                coordinates.RemoveAt(randomCoordinateIx);

                if(board.BoardOptions[x, y] == BoardOptions.Uninitialized)
                {
                    board.BoardOptions[x, y] = BoardOptions.BombCovered;
                    --numberOfBombs;
                }
            }

            for(x = 0; x < maxX; ++x)
            {
                for(y = 0; y < maxY; ++y)
                {
                    if(board.BoardOptions[x, y] == BoardOptions.Uninitialized)
                    {
                        board.BoardOptions[x, y] = BoardOptions.NoBombCovered;
                    }

                    if(((y - 1) >= 0) && ((x - 1) >= 0) && (board.BoardOptions[x - 1, y - 1] == BoardOptions.BombCovered))
                        ++board.NumberOfSurroundingBombs[x, y];
                    if(((y - 1) >= 0) && (board.BoardOptions[x, y - 1] == BoardOptions.BombCovered))
                        ++board.NumberOfSurroundingBombs[x, y];
                    if(((y - 1) >= 0) && ((x + 1) < maxX) && (board.BoardOptions[x + 1, y - 1] == BoardOptions.BombCovered))
                        ++board.NumberOfSurroundingBombs[x, y];
                    if(((x + 1) < maxX) && (board.BoardOptions[x + 1, y] == BoardOptions.BombCovered))
                        ++board.NumberOfSurroundingBombs[x, y];
                    if(((y + 1) < maxY) && ((x + 1) < maxX) && (board.BoardOptions[x + 1, y + 1] == BoardOptions.BombCovered))
                        ++board.NumberOfSurroundingBombs[x, y];
                    if(((y + 1) < maxY) && (board.BoardOptions[x, y + 1] == BoardOptions.BombCovered))
                        ++board.NumberOfSurroundingBombs[x, y];
                    if(((y + 1) < maxY) && ((x - 1) >= 0) && (board.BoardOptions[x - 1, y + 1] == BoardOptions.BombCovered))
                        ++board.NumberOfSurroundingBombs[x, y];
                    if(((x - 1) >= 0) && (board.BoardOptions[x - 1, y] == BoardOptions.BombCovered))
                        ++board.NumberOfSurroundingBombs[x, y];
                }
            }
        }
    }
}