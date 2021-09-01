using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

public class Room : IXmlSerializable
{
    //atmospheric stats
    List<Tile> tiles;
    public Room()
    {
        tiles = new List<Tile>();
    }

    public void AssignTile(Tile t)
    {
        if (tiles.Contains(t))
        {
            return;
            
        }
        if (t.room != null)
        {
            //belongs to some other room now.
            t.room.tiles.Remove(t);
        }
        t.room = this;
        tiles.Add(t);

        
    }

    public void UnAssignAllTiles()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].room = World.current.GetOutsideRoom();
        }
        tiles = new List<Tile>();
    }
    public static void DoRoomFloodFill(Tile sourceTile)
    {
        //The furniture that may be splitting two room or may be final enclosing piece 
        //Is this piece of furniture was added to an eisting room
        World world = World.current;
        Room oldRoom = sourceTile.room;
        if(oldRoom != null)
        {
            //try building a new room starting from the north
            foreach(Tile t in sourceTile.GetNeighbors())
            {
                SpecificFloodFill(t, oldRoom);
            }

            sourceTile.room = null;
            oldRoom.tiles.Remove(sourceTile);

            oldRoom.tiles = new List<Tile>(); // we know all tiles not point to another room
            if(oldRoom != world.GetOutsideRoom())
            {
                //at this point, old room shouldn't have anymore tiles in it.
                if(oldRoom.tiles.Count > 0)
                {
                    Debug.LogError("oldroom: still has tiles assigned to it, this is WRONG");
                }
                world.DeleteRoom(oldRoom);
                //instObj.tile.room.UnAssignAllTiles();
            }
        } else
        {
            //old room is null so source tile was probably a wall that was destroyed
            SpecificFloodFill(sourceTile, null,true);
        }

        //check for empty room, destroying all that are empty
        world.CheckEmptyRooms();
    }

    public bool hasTiles()
    {
        if(tiles.Count == 0)
        {
            return false;
        }

        return true;
    }
    protected static void SpecificFloodFill(Tile tile, Room oldRoom, bool desconstructCause = false)
    {
        if(tile == null)
        {
            //trying to flood fill off map so return without doing anything
            return;
        }

        if(tile.room != oldRoom)
        {
            return;
        }

        if (tile.installedObject != null && tile.installedObject.roomEnclosure)
        {
            return;
        }

        if (tile.tileType == TileType.Empty)
        {
            return;
        }

            //if we get to this point we need to create a new room
        Room newRoom = new Room();
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(tile);
        bool outside = false;
        while (tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();

            
            if(t.room != newRoom)
            {
                
                newRoom.AssignTile(t);
                Tile[] ns = t.GetNeighbors();
                foreach(Tile t2 in ns)
                {
                    if(t2 == null || t2.tileType == TileType.Empty)
                    {
                        //hit open space,
                        if (desconstructCause)
                        {
                            outside = true;
                        } else
                        {
                            newRoom.UnAssignAllTiles();
                            return;
                        }
                        
                        
                    }
                    else if (t2 != null && (t2.installedObject == null || t2.installedObject.roomEnclosure == false) && t2.room != newRoom)
                    {
                        tilesToCheck.Enqueue(t2);
                    }
                }
                
            }
        }

        if (outside)
        {
            newRoom.UnAssignAllTiles();
            return;
        }
        //currentTile belongs to this room

        //copy oldroom data to newRoomd

        World.current.AddRoom(newRoom);

        
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        //write specific attributes here (i.e room type, containment, etc.)
    }

    public void ReadXml(XmlReader reader)
    {
       //read back in those specific attributes
    }

}
