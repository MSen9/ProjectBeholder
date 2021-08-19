using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_TileGraph
{
    //This class constructs a simple path-finding compatible graph of our world
    //each tile is a node and each WALKABLE neighbor from a tile is linked via an edge connection

    
    public Dictionary<Tile, Path_Node<Tile>> nodes;
    public Path_TileGraph(World world)
    {
        //loop through all tiles of the worlds
        //for each tile, create a node
        //Do we create nodes for tiles that are unwalkable: Walls, empty floor: NO
        nodes = new Dictionary<Tile, Path_Node<Tile>>();
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile t = world.getTileAt(x, y);
                //if(t.movementCost > 0)
               // {
                    Path_Node<Tile> n = new Path_Node<Tile>();
                    n.data = t;
                    nodes.Add(t, n);
                //}
            }
        }

        //loop through a second time to create the edges

        foreach(Tile t in nodes.Keys)
        {
            Path_Node<Tile> n = nodes[t];

            List<Path_Edge<Tile>> edges = new List<Path_Edge<Tile>>();
            //get a list of neightbors for this tile
            Tile[] neighbors = t.GetNeighbors(true); // NOTE some of the array spots could be null

            //if neighbor is walkable create an edge to the relevant node;
            for (int i = 0; i < neighbors.Length; i++)
            {
                if(neighbors[i] != null && neighbors[i].movementCost > 0)
                {
                    //neighbor exists and is walkable, so create the edge
                    //make sure we aren't clipping a diagonal or squeeze innapropriately
                    if (i >= 4 && isClippingCorner(t, neighbors[i]))
                    {

                        continue;
                    }
                    Path_Edge<Tile> e = new Path_Edge<Tile>();
                    e.cost = neighbors[i].movementCost;
                    e.node = nodes[neighbors[i]];

                    //add the edge to our temporary (and growable!) list
                    edges.Add(e);
                }
            }
            n.edges = edges.ToArray();
        }

        Debug.Log("Nodes: " + nodes.Count);
    }

    bool isClippingCorner(Tile curr, Tile neighbor)
    {
        //if the moverment from te curr to neigh is diagonal then check to make sure we aren't clipping
        int dX = curr.X - neighbor.X;
        int dY = curr.Y - neighbor.Y;
           
        if(curr.world.getTileAt(curr.X - dX,curr.Y).movementCost == 0 ||
            curr.world.getTileAt(curr.X, curr.Y - dY).movementCost == 0)
        {
            //On the of the directions in the way is blocked and diag cannot be walked through
            return true;
        }
        return false;
    }
}
