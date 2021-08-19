using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Edge<T> 
{
    public float cost; // cost to traverse this edge (i.e. cost to enter the tile)
    public Path_Node<T> node;
}
