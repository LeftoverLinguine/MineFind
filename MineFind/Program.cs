﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MineFind
{
    public class GameFailedException: Exception { }

    public enum PointOptions
    {
        Uninitialized = 0,
        BombCovered,
        NoBombCovered,
        NoBombUncovered
    }

    public struct Board
    {
        public PointOptions[,] PointStatuses { get; set; }
        public int[,] NumberOfSurroundingBombs { get; set; }
    }

    public class Program
    {
        private static readonly (int x, int y)[] SurroundingPointVectors = new(int x, int y)[8]
        {
            (-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 1), (1, 0), (1, -1), (0, -1)
        };

        public static void Main(string[] args)
        {
            var continuePlaying = true;

            while(continuePlaying)
            {
                try
                {
                    Console.WriteLine();
                    Console.Write("Enter the number of rows for the board and press ENTER: ");
                    if(!int.TryParse(Console.ReadLine(), out var x))
                    {
                        Console.WriteLine("\nInvalid number was entered.");
                        throw new GameFailedException();
                    }

                    Console.Write("Enter the number of columns for the board and press ENTER: ");
                    if(!int.TryParse(Console.ReadLine(), out var y))
                    {
                        Console.WriteLine("\nInvalid number was entered.");
                        throw new GameFailedException();
                    }

                    Console.Write("Enter the number of bombs for the board and press ENTER: ");
                    if(!int.TryParse(Console.ReadLine(), out var numberOfBombs))
                    {
                        Console.WriteLine("\nInvalid numberOfBombs number was entered.");
                        throw new GameFailedException();
                    }

                    if(numberOfBombs > (x * y))
                    {
                        Console.WriteLine("Too many bombs for this board size.");
                        throw new GameFailedException();
                    }

                    var board = new Board
                    {
                        PointStatuses = new PointOptions[x, y],
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
                            Console.WriteLine("Invalid move.");
                        }
                        else
                        {
                            switch(board.PointStatuses[xMove, yMove])
                            {
                                case PointOptions.NoBombUncovered:
                                    /* This is a no-op, since it was already revealed to not have a bomb. */
                                    break;
                                case PointOptions.BombCovered:
                                    /* Game over */
                                    bombHit = true;
                                    break;
                                case PointOptions.NoBombCovered:
                                    UncoverNoBombSpots(board, xMove, yMove);
                                    break;
                            }

                            gameWon = IsGameWon(board);
                        }
                    }

                    if(bombHit)
                        Console.WriteLine("You lose!!!");
                    else
                        Console.WriteLine("You win!!!");

                    DrawBoard(board, true);
                }
                catch(GameFailedException)
                {
                }
                finally
                {
                    Console.Write("Play again? (y/n)");
                    continuePlaying = Console.ReadLine().ToLower() == "y";
                }
            }
        }

        private static bool IsGameWon(Board board)
        {
            var xDimension = board.PointStatuses.GetLength(0);
            var yDimension = board.PointStatuses.GetLength(1);

            for(var x = 0; x < xDimension; ++x)
            {
                for(var y = 0; y < yDimension; ++y)
                {
                    if(board.PointStatuses[x, y] == PointOptions.NoBombCovered)
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
            var maxX = board.PointStatuses.GetLength(0);
            var maxY = board.PointStatuses.GetLength(1);
            var noBombPositionsToCheck = new Queue<(int x, int y)>();

            noBombPositionsToCheck.Enqueue((x, y));
            board.PointStatuses[x, y] = PointOptions.NoBombUncovered;

            while(noBombPositionsToCheck.Count > 0)
            {
                (x, y) = noBombPositionsToCheck.Dequeue();

                foreach(var (xDistance, yDistance) in SurroundingPointVectors)
                {
                    if(((y + yDistance) >= 0) && ((x + xDistance) >= 0) 
                        && ((y + yDistance) < maxY) && ((x + xDistance) < maxX)
                        && (board.PointStatuses[x + xDistance, y + yDistance] == PointOptions.NoBombCovered))
                    {
                        board.PointStatuses[x + xDistance, y + yDistance] = PointOptions.NoBombUncovered;
                        if(board.NumberOfSurroundingBombs[x + xDistance, y + yDistance] == 0)
                            noBombPositionsToCheck.Enqueue((x + xDistance, y + yDistance));
                    }
                }
            }
        }

        private static void DrawBoard(Board board, bool uncoverEverything = false)
        {
            var maxX = board.PointStatuses.GetLength(0);
            var maxY = board.PointStatuses.GetLength(1);

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
                    switch(board.PointStatuses[x, y])
                    {
                        case PointOptions.Uninitialized:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write("X");
                            break;
                        case PointOptions.BombCovered:
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
                        case PointOptions.NoBombCovered:
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
                        case PointOptions.NoBombUncovered:
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
            var maxX = board.PointStatuses.GetLength(0);
            var maxY = board.PointStatuses.GetLength(1);

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

                if(board.PointStatuses[x, y] == PointOptions.Uninitialized)
                {
                    board.PointStatuses[x, y] = PointOptions.BombCovered;
                    --numberOfBombs;
                }
            }

            for(x = 0; x < maxX; ++x)
            {
                for(y = 0; y < maxY; ++y)
                {
                    if(board.PointStatuses[x, y] == PointOptions.Uninitialized)
                        board.PointStatuses[x, y] = PointOptions.NoBombCovered;

                    foreach(var (xDistance, yDistance) in SurroundingPointVectors)
                    {
                        if(((y + yDistance) >= 0) && ((x + xDistance) >= 0)
                            && ((y + yDistance) < maxY) && ((x + xDistance) < maxX)
                            && (board.PointStatuses[x + xDistance, y + yDistance] == PointOptions.BombCovered))
                        {
                            ++board.NumberOfSurroundingBombs[x, y];
                        }
                    }
                }
            }
        }
    }
}