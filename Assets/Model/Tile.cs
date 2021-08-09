using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile
{
    public enum TileType { Empty , Floor};

    TileType type = TileType.Empty;
    Action<Tile> tileTypeUpdate;
    LooseObject looseObject;
    InstalledObject installedObject;

    World world;
    int x;
    int y;

    public TileType Type
    {
        get
        {
            return type;
        }
        set
        {
            type = value;
            tileTypeUpdate(this);
        }
    }
    public int X { get => x;}
    public int Y { get => y; }

    public Tile( World world, int x, int y)
    {
        this.world = world;
        this.x = x;
        this.y = y;
    }

    public void SetTileTypeUpdate(Action<Tile> callback)
    {
        tileTypeUpdate = callback;
    }
}
