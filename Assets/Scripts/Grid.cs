using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int rows;
    private int column;
    private float cellSize;
    private Cell[,] cellArray;

    // Public getters for the visualization script to use
    public int Rows => rows;
    public int Columns => column;
    public float CellSize => cellSize;


    //Public struct for neighbours identification and properties.
    public struct Neighbor
    {
        public int X;
        public int Y;
        public Direction Direction; // Enum for direction
    }

    public enum Direction { North, South, East, West }


    public Grid(int rows, int column, float cellSize)
    {
        this.rows = rows;
        this.column = column;
        this.cellSize = cellSize;

        cellArray = new Cell[rows, column];

        for (int x = 0; x < cellArray.GetLength(0); x++)
        {
            for (int y = 0; y < cellArray.GetLength(1); y++)
            {

                cellArray[x, y] = new Cell(x, y);

            }
        }


        GenerateMaze(0, 0);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

    public Cell GetCell(int x, int y)
    {
        // Add boundary check to prevent IndexOutOfRangeException
        if (x >= 0 && x < rows && y >= 0 && y < column)
        {
            return cellArray[x, y];

        }
        return null;
    }


    private List<Neighbor> GetUnvisitedNeighbors(int x, int y)
    {
        // Container list to hold all the neighbour
        List<Neighbor> neighbors = new List<Neighbor>();

        //Tuples Array
        (int dx, int dy, Direction dir)[] checks = new (int, int, Direction)[]
        {
        ( 0,  1, Direction.North), // Move: x + 0, y + 1 (Up)
        ( 0, -1, Direction.South), // Move: x + 0, y - 1 (Down)
        ( 1,  0, Direction.East),  // Move: x + 1, y + 0 (Right)
        (-1,  0, Direction.West)   // Move: x - 1, y + 0 (Left)
        };

        foreach (var check in checks)
        {
            int nextX = x + check.dx;
            int nextY = y + check.dy;


            //  Boundary Check
            if (nextX >= 0 && nextX < rows && nextY >= 0 && nextY < column)
            {
                Cell neighborCell = GetCell(nextX, nextY);

                // Visited Check
                if (neighborCell != null && !neighborCell.visited)
                {
                    neighbors.Add(new Neighbor { X = nextX, Y = nextY, Direction = check.dir });
                }

            }
        }
        return neighbors;
    }


    public void GenerateMaze(int startX, int startY)
    {
        // Simple check to ensure start point is valid
        if (startX < 0 || startX >= rows || startY < 0 || startY >= column) return;

        // Start the recursive process
        CarvePath(startX, startY);

         
    }

    private void CarvePath(int x, int y)
    {
        // Mark the current cell as visited
        Cell currentCell = GetCell(x, y);
        if (currentCell == null) return;
        currentCell.visited = true;

        // Get unvisited neighbors and shuffle them for randomness
        List<Neighbor> neighbors = GetUnvisitedNeighbors(x, y);

        // Custom logic to shuffle the neighbours list
        neighbors.Shuffle();

        // Iterate through shuffled neighbors
        foreach (var neighbor in neighbors)
        {
            Cell neighborCell = GetCell(neighbor.X, neighbor.Y);

            // Check again if it was visited in the meantime by another recursive call
            if (neighborCell != null && !neighborCell.visited)
            {
                // Carve the wall
                RemoveWall(currentCell, neighborCell, neighbor.Direction);

                // Recurse or Move to the neighbor
                CarvePath(neighbor.X, neighbor.Y);
            }
            // If the neighbor is already visited, the recursive call is skipped (backtracking handled)
        }
    }

   

    private void RemoveWall(Cell current, Cell neighbor, Direction direction)
    {
    // Set the wall to false (carved/open) for both the current cell and the neighbor cell

        switch (direction)
        {
        case Direction.North:
            current.north = false;
            neighbor.south = false;
            break;
        case Direction.South:
            current.south = false;
            neighbor.north = false;
            break;
        case Direction.East:
            current.east = false;
            neighbor.west = false;
            break;
        case Direction.West:
            current.west = false;
            neighbor.east = false;
            break;
        }
    }
}

