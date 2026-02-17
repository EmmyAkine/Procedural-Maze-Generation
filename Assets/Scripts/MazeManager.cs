using UnityEngine;
public class MazeManager : MonoBehaviour
{
    [SerializeField] private LineRenderer mazeLineRendererPrefab;
    [SerializeField] private Transform mazeParentHolder;
    private Grid grid;


    private void Start()
    {
        grid = new Grid(30, 30, 1.8f);
        CarveOutStartAndEndPoint();
        VisualizeMaze();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            RefreshMaze();
        }

    }

    private void VisualizeMaze()
    {
        foreach (Transform child in mazeParentHolder.transform)
        {
            Destroy(child.gameObject); // Clear old lines
        }

        if (mazeLineRendererPrefab == null)
        {
            Debug.LogError("MazeLineRenderer is not assigned in the Inspector!");
            return;
        }


        for (int x = 0; x < grid.Rows; x++)
        {
            for (int y = 0; y < grid.Columns; y++)
            {
                Cell currentCell = grid.GetCell(x, y);
                if (currentCell == null) continue;

                // Position P0: Bottom-Left Corner of the cell
                Vector3 p0 = grid.GetWorldPosition(x, y);
                // Position P1: Bottom-Right Corner
                Vector3 p1 = grid.GetWorldPosition(x + 1, y);
                // Position P2: Top-Left Corner
                Vector3 p2 = grid.GetWorldPosition(x, y + 1);
                // Position P3: Top-Right Corner
                Vector3 p3 = grid.GetWorldPosition(x + 1, y + 1);



                // Draw North Wall (P2 to P3)
                if (currentCell.north)
                {
                    CreateLine(p2, p3);
                }

                // Draw East Wall (P1 to P3)
                if (currentCell.east)
                {
                    CreateLine(p1, p3);
                }

                //Draw West Wall
                if (x == 0 && currentCell.west)
                {
                    CreateLine(p0, p2);
                }

                //Draw South Wall
                if (y == 0 && currentCell.south)
                {
                    CreateLine(p0, p1);
                }
            }

        }

    }

    private void CreateLine(Vector3 start, Vector3 end)
    {
        LineRenderer lineRenderer = Instantiate(mazeLineRendererPrefab, mazeParentHolder.transform);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void RefreshMaze()
    {
        grid = new Grid(grid.Rows, grid.Columns, grid.CellSize);

        CarveOutStartAndEndPoint();

        VisualizeMaze();
    }

    private void CarveOutStartAndEndPoint()
    {
        //Start Point
        Cell startCell = grid.GetCell(0, 0);
        if (startCell != null)
        {
            startCell.south = false;
        }


        //End Point
        int endX = grid.Rows - 1;
        int endY = grid.Columns - 1;
        Cell endCell = grid.GetCell(endX, endY);
        if (endCell != null) 
        { 
            endCell.north = false;
        }
    }

}

