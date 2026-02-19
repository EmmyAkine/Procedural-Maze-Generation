using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class MazeManager : MonoBehaviour
{
    [SerializeField] private LineRenderer mazeLineRendererPrefab;
    [SerializeField] private Transform mazeParentHolder;

    [SerializeField] private bool animateGeneration = true;
    [SerializeField] private float animationSpeed = 0.001f;
    [SerializeField] private SpriteRenderer cellHighlightPrefab;

    [SerializeField] private Color currentCellColor = Color.green;
    [SerializeField] private Color visitedCellColor = Color.yellow;
    [SerializeField] private Color backtrackColor = Color.red;

    private Grid grid;
    private Dictionary<Vector2Int, SpriteRenderer> cellVisualizers;
    private bool isGenerating = false;

    private void Start()
    {
        cellVisualizers = new Dictionary<Vector2Int, SpriteRenderer>();
        StartGeneration();

    }

    private void OncCellReturned(int x, int y)
    {
        if (cellHighlightPrefab != null)
        {
            HighlightCell(x, y, Color.cyan, 0.2f);  // Brief cyan flash when returning
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isGenerating)
        {
            RefreshMaze();
        }
        // Toggle animation with spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animateGeneration = !animateGeneration;
            Debug.Log($"Animation: {(animateGeneration ? "ON" : "OFF")}");
        }

        // Speed controls
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
        {
            animationSpeed = Mathf.Max(0.001f, animationSpeed * 0.5f);
            Debug.Log($"Animation Speed: {animationSpeed}s");
        }
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            animationSpeed = Mathf.Min(1f, animationSpeed * 2f);
            Debug.Log($"Animation Speed: {animationSpeed}s");
        }

    }

    private void StartGeneration()
    {
        grid = new Grid(25, 25, 2.0f);

        // Draw initial grid with all walls
        DrawInitialGrid();

        if (animateGeneration)
        {
            StartCoroutine(GenerateWithVisualization());
        }
        else
        {
            grid.GenerateMaze(0, 0);
            CarveOutStartAndEndPoint();
            VisualizeMaze();
        }
    }

    private IEnumerator GenerateWithVisualization()
    {
        isGenerating = true;

        // Subscribe to grid events
        grid.OnCellVisited += OnCellVisited;
        grid.OnWallRemoved += OnWallRemoved;
        grid.OnBacktrack += OnBacktrack;
        grid.OnCellReturned += OncCellReturned;

        // Start animated generation
        yield return grid.GenerateMazeAnimated(0, 0, animationSpeed);


        // Unsubscribe from events
        grid.OnCellVisited -= OnCellVisited;
        grid.OnWallRemoved -= OnWallRemoved;
        grid.OnBacktrack -= OnBacktrack;
        grid.OnCellReturned -= OncCellReturned;

   

        // Clear cell visualizers
        ClearCellVisualizers();

        // Carve start and end
        CarveOutStartAndEndPoint();

        // Final maze visualization
        VisualizeMaze();

        isGenerating = false;
    }

    private void DrawInitialGrid()
    {
        // Clear old visualization
        foreach (Transform child in mazeParentHolder.transform)
        {
            Destroy(child.gameObject);
        }

        if (mazeLineRendererPrefab == null) return;

        // Draw complete grid with all walls
        for (int x = 0; x < grid.Rows; x++)
        {
            for (int y = 0; y < grid.Columns; y++)
            {
                Vector3 p0 = grid.GetWorldPosition(x, y);
                Vector3 p1 = grid.GetWorldPosition(x + 1, y);
                Vector3 p2 = grid.GetWorldPosition(x, y + 1);
                Vector3 p3 = grid.GetWorldPosition(x + 1, y + 1);

                // Draw all four walls initially
                CreateLine(p2, p3); // North
                CreateLine(p1, p3); // East

                if (x == 0) CreateLine(p0, p2); // West edge
                if (y == 0) CreateLine(p0, p1); // South edge
            }
        }
    }

    private void OnCellVisited(int x, int y)
    {
        if (cellHighlightPrefab != null)
        {
            HighlightCell(x, y, currentCellColor);
        }
    }

    private void OnWallRemoved(int fromX, int fromY, int toX, int toY)
    {
        // Update visualization - redraw the maze
        VisualizeMaze();

        // Highlight the path
        if (cellHighlightPrefab != null)
        {
            HighlightCell(fromX, fromY, visitedCellColor);
            HighlightCell(toX, toY, currentCellColor);
        }
    }

    private void OnBacktrack(int x, int y)
    {
        if (cellHighlightPrefab != null)
        {
            HighlightCell(x, y, backtrackColor, 0.1f);
        }
    }

    private void HighlightCell(int x, int y, Color color, float duration = -1)
    {
        Vector2Int key = new Vector2Int(x, y);

        if (!cellVisualizers.ContainsKey(key) && cellHighlightPrefab != null)
        {
            Vector3 cellCenter = grid.GetWorldPosition(x, y) +
                               new Vector3(grid.CellSize * 0.5f, grid.CellSize * 0.5f, 0);

            SpriteRenderer highlight = Instantiate(cellHighlightPrefab, cellCenter, Quaternion.identity, mazeParentHolder);
            highlight.transform.localScale = Vector3.one * grid.CellSize * 0.3f;
            highlight.color = color;
            cellVisualizers[key] = highlight;

            if (duration > 0)
            {
                Destroy(highlight.gameObject, duration);
            }
        }
        else if (cellVisualizers.ContainsKey(key))
        {
            cellVisualizers[key].color = color;
        }
    }

    private void ClearCellVisualizers()
    {
        foreach (var visualizer in cellVisualizers.Values)
        {
            if (visualizer != null)
            {
                Destroy(visualizer.gameObject);
            }
        }
        cellVisualizers.Clear();
    }
    private void VisualizeMaze()
    {
        foreach (Transform child in mazeParentHolder.transform)
        {
            if (child.GetComponent<SpriteRenderer>() == null)
            {
                Destroy(child.gameObject); // Clear old lines   
            }
        }

        if (mazeLineRendererPrefab == null)
        {
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
        ClearCellVisualizers();
        StartGeneration();
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

