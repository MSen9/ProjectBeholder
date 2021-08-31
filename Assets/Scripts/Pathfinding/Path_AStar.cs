using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
public class Path_AStar
{
    //Try D* Lite if this is in need of optimization
    Stack<Tile> path;
    public Path_AStar(World world, Tile tileStart, Tile tileEnd)
    {
        path = new Stack<Tile>();
        if (world.tileGraph == null)
        {
            world.tileGraph = new Path_TileGraph(world);
        }
        Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;
        //check to see if we have a valid tile grpah
        
        if (nodes.ContainsKey(tileStart) == false)
        {
            Debug.LogError("Path_AStar: Invalid start for path");

            //right now we're going to manually add the start tile to the list of valid nodes.
        }
        if (nodes.ContainsKey(tileEnd) == false)
        {
            Debug.LogError("Path_AStar: Invalid end for path");
        }



        //A dictionary of all valid, walkable nodes
        


        Path_Node<Tile> start = nodes[tileStart];
        Path_Node<Tile> end = nodes[tileEnd];
        //Make sure our start/end tiles are in the list of nodes
        
        List<Path_Node<Tile>> ClosedSet = new List<Path_Node<Tile>>();
        /*
        List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();
        OpenSet.Add(start);
        */
        SimplePriorityQueue < Path_Node < Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>>();
        OpenSet.Enqueue(start, 0);

        Dictionary<Path_Node<Tile>, Path_Node<Tile>> cameFrom = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        Dictionary<Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float>();
        foreach(Path_Node<Tile> n in nodes.Values)
        {
            g_score[n] = Mathf.Infinity;
        }
        g_score[start] = 0;

        Dictionary<Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float>();
        foreach (Path_Node<Tile> n in nodes.Values)
        {
            f_score[n] = Mathf.Infinity;
        }
        g_score[nodes[tileStart]] = heuristicCostEstimate(start,end);

        while(OpenSet.Count > 0)
        {
            Path_Node<Tile> current = OpenSet.Dequeue();

            if(current == end)
            {
                //TODO: Return reconsturct path
                reconstructPath(cameFrom, current);
                return;
            }

            ClosedSet.Add(current);

            foreach(Path_Edge<Tile> neighborEdge in current.edges)
            {
                Path_Node<Tile> neighbor = neighborEdge.node;
                if (ClosedSet.Contains(neighbor))
                {
                    continue; // ignore this already conpleted neightbpor
                }
                float movement_cost_to_neighbor = neighbor.data.movementCost * dist_between(current, neighbor);

                float tentative_g_score = g_score[current] + movement_cost_to_neighbor;
                if (OpenSet.Contains(neighbor) && tentative_g_score >= g_score[neighbor])
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                g_score[neighbor] = tentative_g_score;
                f_score[neighbor] = g_score[neighbor] + heuristicCostEstimate(neighbor, end);
                if(OpenSet.Contains(neighbor) == false)
                {
                    //f_score[neighbor] = g_score[neighbor];
                    OpenSet.Enqueue(neighbor,f_score[neighbor]);
                }
            } //foreach neighbor
        } // while

        //This means we burned through our entire open set withour reaching a current = goal state
        //No path from start to goal

        return;
    }


    float heuristicCostEstimate(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        return Mathf.Sqrt(Mathf.Pow(a.data.X - b.data.X, 2) + Mathf.Pow(a.data.Y - b.data.Y, 2));
    }

    float dist_between(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        //check for direct neighbor
        if(Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1){
            return 1f;
        }
        //check for 
        if(Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1){
            return 1.414213f;
        }

        //return real value
        return heuristicCostEstimate(a, b);
    }

    
    public Tile GetNextTile()
    {
        return path.Pop();
    }

    void reconstructPath(Dictionary<Path_Node<Tile>, Path_Node<Tile>> cameFrom, Path_Node<Tile> current)
    {
        //at this point current is the goal so we walk backwards through the map
        Stack <Tile> total_Path = new Stack<Tile>();
        total_Path.Push(current.data);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            total_Path.Push(current.data);
        }


        //At this point total path is a stack that is runnign backwards form the END tile to the start tile
        path = total_Path;
        return;
    }

    public int Length()
    {
        if (path == null)
            return 0;
        return path.Count;
    }
}
