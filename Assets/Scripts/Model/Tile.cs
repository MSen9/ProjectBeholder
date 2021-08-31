using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;


public enum TileType { Empty, Floor };
public enum Enterability { Yes, Never, Soon};

public class Tile : IXmlSerializable
{
    
    Action<Tile> cbTileChanged;
    public LooseObject looseObject;

    public Room room;
    float baseMoveCost = 1;
    public InstalledObject installedObject
    {
        get; protected set;
    }

    public Job pendingInstObjJob;

    int x;
    int y;
    TileType trueTileType;
    public TileType tileType
    {
        get
        {
            return trueTileType;
        }
        set
        {
            TileType oldType = trueTileType;
            trueTileType = value;
            
            if(cbTileChanged != null && trueTileType != oldType)
            {
                cbTileChanged(this);
            }
           
        }
    }
    public int X { get => x;}
    public int Y { get => y; }

    public float movementCost
    {
        get
        {
            if(tileType == TileType.Empty)
            {
                return 0;
            }

            if(installedObject == null)
            {
                return baseMoveCost;
            }

            return baseMoveCost * installedObject.movementCost;
        }
        
    }
    public Tile( World world, int x, int y)
    {
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


    public bool RemoveInstObj()
    {
        if (installedObject == null)
        {
            return false;
        }
        int width = installedObject.width;
        int height = installedObject.height;
        for (int x_off = X; x_off < X + width; x_off++)
        {
            for (int y_off = Y; y_off < Y + height; y_off++)
            {
                Tile t = World.current.GetTileAt(x_off, y_off);
                t.installedObject = null;
            }
        }
        return true;
    }
    public bool PlaceInstalledObject(InstalledObject objInstance)
    {

        if(objInstance == null)
        {
            RemoveInstObj();
            //just uninstalling FIXME: what if we have a multi-tile inst obj;
        }

        if (objInstance.isValidPosition(this) == false)
        {
            Debug.LogError("Trying to assign inst obj to invalid position");
            return false;
        }
        for (int x_off = X; x_off < X + objInstance.width; x_off++)
        {
            for (int y_off = Y; y_off < Y+objInstance.height; y_off++)
            {
                Tile t = World.current.GetTileAt(x_off, y_off);



                t.installedObject = objInstance;
            }
        }
      
        return true;
    }


    public bool PlaceLooseObject(LooseObject newLooseObj)
    {
        int numToMove = newLooseObj.stackSize;

        

        if(newLooseObj == null)
        {
            return false; 
        }
        if(looseObject != null)
        {
            if(newLooseObj.objectType != looseObject.objectType)
            {
                Debug.LogError("Trying to assign loose object to stack of different type");
                return false;
            }

            if(newLooseObj.stackSize + looseObject.stackSize > looseObject.maxStackSize)
            {
                numToMove = looseObject.maxStackSize - looseObject.stackSize;
            }
            looseObject.stackSize += numToMove;
            newLooseObj.stackSize -= numToMove;
            return true;
        }

        //we know our current inventory is null

        looseObject = newLooseObj.Clone();
        newLooseObj.stackSize = 0;
        looseObject.tile = this;

        return true;
    }

    
    //tells us if two tile are adjacent
    public bool IsNeighbor(Tile tile, bool checkDiag = false)
    {

        //directly adjacent
        if(Mathf.Abs(this.X - tile.X) + Mathf.Abs(this.Y-tile.Y) == 1)
        {
            return true;
        }
        
        if (checkDiag)
        {
            //check diagonal adjacency
            if (Mathf.Abs(this.X - tile.X) == 1)
            {
                if (Mathf.Abs(this.Y - tile.Y) == 1)
                {
                    return true;
                }
            }
            
        }
        return false;
    }

    public Tile[] GetNeighbors(bool diagOkay = false,bool clippingOkay = true)
    {
        Tile[] ns;
        if(diagOkay == false)
        {
            ns = new Tile[4]; //NESW
        } else
        {
            ns = new Tile[8]; //NESW - NE - SE - SW - NW
        }
        Tile t;
        t = World.current.GetTileAt(X, Y + 1);
        ns[0] = t; //could be null, no problem
        t = World.current.GetTileAt(X+1, Y);
        ns[1] = t; //could be null, no problem
        t = World.current.GetTileAt(X, Y - 1);
        ns[2] = t; //could be null, no problem
        t = World.current.GetTileAt(X-1, Y);
        ns[3] = t; //could be null, no problem

        if (diagOkay)
        {

            t = World.current.GetTileAt(X+1, Y + 1);
            ns[4] = t; //could be null, no problem
            t = World.current.GetTileAt(X + 1, Y-1);
            ns[5] = t; //could be null, no problem
            t = World.current.GetTileAt(X-1, Y - 1);
            ns[6] = t; //could be null, no problem
            t = World.current.GetTileAt(X - 1, Y+1);
            ns[7] = t; //could be null, no problem
        }

        return ns;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("Type", tileType.ToString());
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
    }

    public void ReadXml(XmlReader reader)
    {
       
        tileType = (TileType)Enum.Parse(typeof(TileType), reader.GetAttribute("Type"));
        
    }

    public Enterability TryEnter()
    {
        //this returns true if you can enter this tile right this moment.

        if(movementCost == 0)
        {
            return Enterability.Never;
        }
        //check our instObj to see if it has a special block on enterability
        if(installedObject != null && installedObject.IsEnterable != null)
        {
            return installedObject.IsEnterable(installedObject);
        }
         
        return Enterability.Yes;
    }

    public Tile North()
    {
        return World.current.GetTileAt(X, Y + 1);
    }
    public Tile South()
    {
        return World.current.GetTileAt(X, Y - 1);
    }
    public Tile East()
    {
        return World.current.GetTileAt(X+1, Y);
    }
    public Tile West()
    {
        return World.current.GetTileAt(X-1, Y + 1);
    }
}
