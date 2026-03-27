using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Empty = 0,
    Wall = 1,
    Enemy = 2,
    Player = 3
}



public class Maze : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject tilePrefab;
    private TileType[,] maze;

    [SerializeField] private GameObject playerTilePrefab;
    private GameObject playerTile;
    private Vector2Int playerPos;
    private float playerSpeed = 0.25f;
    private Vector2Int currentDirection = new Vector2Int(0, -1);
    private Vector2Int desiredDirection;

    [SerializeField] private GameObject enemyTilePrefab;
    private GameObject[] enemyTiles = new GameObject[4];
    private Vector2Int[] enemyPos = new Vector2Int[4];
    private GameObject[,] mazeGameObjects;
    private PathFinding pf;

    [SerializeField] private GameObject winTilePrefab;
    private GameObject winTile;

    private GameManager gameManager;
    private bool isPlayerMoving = false;


    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        pf = new PathFinding();
        pf.maze = this;

        maze = new TileType[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = TileType.Wall;

        mazeGameObjects = new GameObject[width, height];
        
        GenerateMaze();
        RandomHoles();
        playerPos = new Vector2Int(1, 1);
        playerTile = Instantiate(playerTilePrefab, new Vector2(playerPos.x, playerPos.y), transform.rotation, gameObject.transform);
        maze[playerPos.x, playerPos.y] = TileType.Player;

        DrawMap();

        if (!isPlayerMoving)
        {
            StartCoroutine(UpdatePlayerPosition());
        }

        winTile = Instantiate(winTilePrefab, new Vector2(width-2, height-2), transform.rotation, gameObject.transform);
        SpawnEnemy();
        StartCoroutine(EnemyMove());
    }

    void Update()
    {
        PlayerInput();
    }

    void DrawMap()
    {
        if(transform.Find("Tiles") != null)
        {
            Destroy(transform.Find("Tiles").gameObject);
        }

        GameObject tiles = new GameObject("Tiles");
        tiles.transform.SetParent(gameObject.transform);

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(maze[i, j] == TileType.Wall)
                    mazeGameObjects[i, j] = Instantiate(tilePrefab, new Vector2(i, j), transform.rotation, tiles.transform);
            }    
        }
    }

    void GenerateMaze()
    {
        Vector2Int start = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        Vector2Int current = new Vector2Int(1, 1);
        maze[current.x, current.y] = TileType.Empty;

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(current);

        while(stack.Count > 0)
        {
            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(current);
            if(unvisitedNeighbors.Count > 0)
            {
                Vector2Int previous = current;
                current = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                stack.Push(current);
                maze[current.x, current.y] = TileType.Empty;
                maze[previous.x+((current.x - previous.x)/2), previous.y+((current.y - previous.y)/2)] = TileType.Empty;
            }
            else
            {
                stack.Pop();
                if(stack.Count > 0)
                    current = stack.Peek();
            }
        }

    }


    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int current)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = {
            new Vector2Int(0, 2),  
            new Vector2Int(0, -2), 
            new Vector2Int(-2, 0), 
            new Vector2Int(2, 0),  
        };

        foreach (Vector2Int dir in directions)
        {
            if(IsInBound(dir + current))
            {
                if(maze[(dir + current).x, (dir + current).y] == TileType.Wall)
                {
                    neighbors.Add(new Vector2Int((dir + current).x, (dir + current).y));
                }
            }
            
        }

        return neighbors;
    }

    public List<Node> GetNeighbors(Vector2Int current)
    {
        List<Node> neighbors = new List<Node>();

        Vector2Int[] directions = {
            new Vector2Int(0, 1),  
            new Vector2Int(0, -1), 
            new Vector2Int(-1, 0), 
            new Vector2Int(1, 0),  
        };

        foreach (Vector2Int dir in directions)
        {
            if(IsInBound(dir + current))
            {
                if(maze[(dir + current).x, (dir + current).y] != TileType.Wall)
                {
                    Node node = new Node(new Vector2Int((dir + current).x, (dir + current).y));
                    neighbors.Add(node);
                }
            }
            
        }

        return neighbors;
    }

    IEnumerator EnemyMove()
    {
        
        while(enemyPos[0] != playerPos && enemyPos[1] != playerPos && enemyPos[2] != playerPos && enemyPos[3] != playerPos)
        {
            Vector2Int[] newPositions = new Vector2Int[enemyPos.Length];
            Vector2[] startPositions = new Vector2[enemyPos.Length];
            
            for(int i = 0; i < enemyPos.Length; i++)
            {
                startPositions[i] = enemyTiles[i].transform.position;
                maze[enemyPos[i].x, enemyPos[i].y] = TileType.Empty;
                
                
                
                newPositions[i] = pf.PathFind(enemyPos[i], playerPos)[0];
                
                
                enemyPos[i] = newPositions[i];
                maze[enemyPos[i].x, enemyPos[i].y] = TileType.Enemy;
            }
            
            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                for(int i = 0; i < enemyPos.Length; i++)
                {
                    enemyTiles[i].transform.position = Vector2.Lerp(startPositions[i], new Vector2(newPositions[i].x, newPositions[i].y), t);
                }
                
                yield return null;
            }
            
            for(int i = 0; i < enemyPos.Length; i++)
            {
                enemyTiles[i].transform.position = new Vector2(enemyPos[i].x, enemyPos[i].y);
            }
        }
        
    }

    void PlayerInput()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            desiredDirection = new Vector2Int(0, 1);
            if(!isPlayerMoving)
                StartCoroutine(UpdatePlayerPosition());
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            desiredDirection = new Vector2Int(0, -1);
            if(!isPlayerMoving)
                StartCoroutine(UpdatePlayerPosition());
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            desiredDirection = new Vector2Int(1, 0);
            if(!isPlayerMoving)
                StartCoroutine(UpdatePlayerPosition());
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            desiredDirection = new Vector2Int(-1, 0);
            if(!isPlayerMoving)
                StartCoroutine(UpdatePlayerPosition());
        }
    }

    IEnumerator UpdatePlayerPosition()
    {
        while (true)
        {
            Vector2Int newPosition = playerPos;
            Vector2 startPosition = playerTile.transform.position;

            Vector2Int pos = playerPos + desiredDirection; 
            if(maze[pos.x, pos.y] == TileType.Wall)
            {
        
                pos = playerPos + currentDirection;
            }
            else
                currentDirection = desiredDirection;


            if(maze[pos.x, pos.y] != TileType.Wall)
            {
                isPlayerMoving = true;
                startPosition = playerTile.transform.position;
                maze[playerPos.x, playerPos.y] = TileType.Empty;
                newPosition = pos;
                playerPos = newPosition;
                maze[playerPos.x, playerPos.y] = TileType.Player;
            }
            else
            {
                isPlayerMoving = false;
                yield break;
            }

            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;


                playerTile.transform.position = Vector2.Lerp(startPosition, new Vector2(newPosition.x, newPosition.y), t);


                yield return null;
            }
        }
        
    }

    

    bool IsInBound(Vector2Int tile)
    {
        return tile.x > 0 && tile.y > 0 && tile.x < width && tile.y < height;
    }

    void RandomHoles()
    {
        for(int i = 0; i < 100;)
        {
            Vector2Int rand = new Vector2Int(Random.Range(1, width-1), Random.Range(1, height-1));
            if (maze[rand.x, rand.y] == TileType.Wall)
            {
                maze[rand.x, rand.y] = TileType.Empty;
                i++;
            }
        }
    }

    Vector2Int GetRandomPosition()
    {
        while (true)
        {
            Vector2Int rand = new Vector2Int(Random.Range(1, width-1), Random.Range(1, height-1));
            if (maze[rand.x, rand.y] == TileType.Empty)
            {
                return rand;
            }
        }
    }

    public void SpawnEnemy()
    {
        for(int i = 0; i < 4;)
        {
            Vector2Int rand = new Vector2Int(Random.Range(1, width-1), Random.Range(1, height-1));
            if (maze[rand.x, rand.y] == TileType.Empty && pf.PathFind(new Vector2Int(rand.x, rand.y), playerPos).Count > 20)
            {
                enemyPos[i] = new Vector2Int(rand.x, rand.y);
                enemyTiles[i] = Instantiate(enemyTilePrefab, new Vector2(enemyPos[i].x, enemyPos[i].y), transform.rotation, gameObject.transform);
                maze[Random.Range(1, width-1), Random.Range(1, height-1)] = TileType.Empty;
                i++;
            }
        }
        
        
        
    }
}
