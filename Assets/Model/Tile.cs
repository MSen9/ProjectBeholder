using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum TileType { Empty, Floor };
public class Tile
{
    

    TileType type = TileType.Empty;
    Action<Tile> cbTileChanged;
    LooseObject looseObject;
    public InstalledObject installedObject
    {
        get; protected set;
    }

    public Job pendingInstObjJob;
    public World world {
        get; protected set;
    }
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
            TileType oldType = type;
            type = value;
            
            if(cbTileChanged != null && type != oldType)
            {
                cbTileChanged(this);
            }
           
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

    public void AddTileTypeUpdate(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }
    public void RemoveTileTypeUpdate(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }

    public bool PlaceObject(InstalledObject objInstance)
    {
        if(objInstance == null)
        {
            //we are uninstalling whatever was here before
            installedObject = null;
            return true;
        }

        if(installedObject != null)
        {
            Debug.LogError("Trying to assign an installed object to a tile that already has one");
            return false;
        }

        installedObject = objInstance;
        return true;
    }

    //tells us if two tile are adjacent
    public bool IsNeighbor(Tile tile, bool checkDiag = false)
    {


        if(this.X == tile.X && (this.Y == tile.Y+1 || this.Y == tile.Y-1))
        {
            return true;
        }
        if (this.Y == tile.Y && (this.X == tile.X + 1 || this.X == tile.X - 1))
        {
            return true;
        }

        if (checkDiag)
        {
            if ((this.X == tile.X+1 || this.X == tile.X-1) && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1))
            {
                return true;
            }
        }
        return false;
    }
}
