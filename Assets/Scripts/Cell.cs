using UnityEngine;

public class Cell
{
    public bool north { get; set; } = true;
    public bool south { get; set; } = true;
    public bool east { get; set; } = true;
    public bool west { get; set; } = true;
    public bool visited { get; set; } = false;



    public int x { get; private set; }
    public int y { get; private set; }



    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        // All walls are initialized to true (closed) and Visited to false
    }

}
