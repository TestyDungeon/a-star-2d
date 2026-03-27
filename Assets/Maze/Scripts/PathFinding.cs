using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int pos;
    public int gCost;
    public int hCost;
    public int fCost { get { return gCost + hCost; } }
    public Node parent;

    public Node(Vector2Int pos_)
    {
        pos = pos_;
    }
}

public class PathFinding 
{
    public Maze maze;
    

    public List<Vector2Int> PathFind(Vector2Int start, Vector2Int target)
    {
        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        Node startNode = new Node(start);
        startNode.hCost = CalculateHeuristic(start, target);
        open.Add(startNode);

        while(true)
        {
            if(open.Count == 0)
            {
                Debug.Log("no path found");
                return null;
            }
            Node current = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                if (open[i].fCost < current.fCost)
                    current = open[i];
            }
            open.Remove(current);
            closed.Add(current);

            if(current.pos == target)
            {
                return TraceBack(current);
            }
            

            foreach(Node neighbor in maze.GetNeighbors(current.pos))
            {
                if(closed.Find(n => n.pos == neighbor.pos) != null)
                    continue;

                int tentativeGCost = current.gCost + 1;
                
                Node neighborNode = open.Find(n => n.pos == neighbor.pos);
                
                if (neighborNode == null || tentativeGCost < neighborNode.gCost)
                {
                    if (neighborNode == null)
                    {
                        neighborNode = new Node(neighbor.pos);
                        
                        open.Add(neighborNode);
                    }
                    
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateHeuristic(neighbor.pos, target);
                    neighborNode.parent = current;
                }

            }
        } 
    }

    int CalculateHeuristic(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    List<Vector2Int> TraceBack(Node node)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        while(node.parent != null)
        {
            path.Add(node.pos);
            node = node.parent;
        }
        path.Reverse();
        return path;
    }
       
}
