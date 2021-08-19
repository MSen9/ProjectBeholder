using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;


public enum TileType { Empty, Floor };
public class Tile : IXmlSerializable
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

    public float movementCost
    {
        get
        {
            if(Type == TileType.Empty)
            {
                return 0;
            }

            if(installedObject == null)
            {
                return 1;
            }

            return 1 * installedObject.movementCost;
        }
        
    }
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
        t = world.getTileAt(X, Y + 1);
        ns[0] = t; //could be null, no problem
        t = world.getTileAt(X+1, Y);
        ns[1] = t; //could be null, no problem
        t = world.getTileAt(X, Y - 1);
        ns[2] = t; //could be null, no problem
        t = world.getTileAt(X-1, Y);
        ns[3] = t; //could be null, no problem

        if (diagOkay)
        {

            t = world.getTileAt(X+1, Y + 1);
            ns[4] = t; //could be null, no problem
            t = world.getTileAt(X + 1, Y-1);
            ns[5] = t; //could be null, no problem
            t = world.getTileAt(X-1, Y - 1);
            ns[6] = t; //could be null, no problem
            t = world.getTileAt(X - 1, Y+1);
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
        writer.WriteAttributeString("Type", Type.ToString());
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
    }

    public void ReadXml(XmlReader reader)
    {
       
        Type = (TileType)Enum.Parse(typeof(TileType), reader.GetAttribute("Type"));
        
    }
}
