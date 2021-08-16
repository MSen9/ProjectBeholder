using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//installed objects are things like wlals, doors and furniture
public class InstalledObject 
{
    //This represents the BASE tile of object, but in practice, large objects may actually require multiple times
    public Tile tile
    {
        get; protected set;
    }
    //This "objecttype" will be queries by the visual system 
    public string objectType
    {
        get; protected set;
    }

    public bool linksToNeighbor
    {
        get; protected set;
    }

    //used for walking over beds and such
    //SPECIAL: oif movement cost is 0, then this time is impasssible (wall)
    float movementCost = 1f; //this is a multiplier. So a value of 2 here means you move twice as slowly
    
    //TODO: Implement larger objects
    //TODO: Implement object rotation

    int width;
    int height;

    Action<InstalledObject> cbOnChanged;
    public Func<Tile,bool> funcPositionValidation;
    protected InstalledObject()
    {

    }
    //used by object factory to create the proto-typical object
    static public InstalledObject CreatePrototype(string objectType, float movementCost, int width=1, int height=1,
        bool linksToNeighbor = false)
    {
        InstalledObject obj = new InstalledObject();
        obj.objectType = objectType;
        obj.movementCost = movementCost;
        obj.width = width;
        obj.height = height;
        obj.linksToNeighbor = linksToNeighbor;

        obj.funcPositionValidation = obj.__IsValidPosition;
        return obj;
    }

    static public InstalledObject PlaceObject(InstalledObject proto, Tile tile)
    {
        if(proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position validity function returned false");
            return null;
        }

        //we know our placement destination is valid;

        InstalledObject obj = new InstalledObject();
        obj.objectType = proto.objectType;
        obj.movementCost = proto.movementCost;
        obj.width = proto.width;
        obj.height = proto.height;
        obj.linksToNeighbor = proto.linksToNeighbor;

        obj.tile = tile;

        //FIXME: This assumes we are 1x1
        if (tile.PlaceObject(obj) == false) {
            //for some reason, we weren't able to place our object inthis tile
            //probably it was already occupied

            //do not return ur newly instantiated object, it will be garbage collected
            return null;
        }

        if (obj.linksToNeighbor)
        {
            int x = obj.tile.X;
            int y = obj.tile.Y;
            Tile t = tile.world.getTileAt(x, y + 1);
            if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType)
            {
                //we have a northern neighbor with the same object type as us so tell it that it has changed
                t.installedObject.cbOnChanged(t.installedObject);
            }
            t = tile.world.getTileAt(x + 1, y);
            if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType)
            {
                t.installedObject.cbOnChanged(t.installedObject);
            }
            t = tile.world.getTileAt(x, y - 1);
            if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType)
            {
                t.installedObject.cbOnChanged(t.installedObject);
            }
            t = tile.world.getTileAt(x - 1, y);
            if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType)
            {
                t.installedObject.cbOnChanged(t.installedObject);
            }
        }
        return obj;
    }

    public void RegisterOnChangedCallback(Action<InstalledObject> cbInst)
    {
        cbOnChanged += cbInst;
    }

    public void UnregisterOnChangedCallback(Action<InstalledObject> cbInst)
    {
        cbOnChanged -= cbInst;
    }

    public bool __IsValidPosition(Tile t)
    {
        //check for a floor, check if it already has furniture
        if (t.Type != TileType.Floor)
        {
            return false;
        }

        if(t.installedObject != null)
        {

            return false;
        }
        return true;
    }
    public bool isValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    //FIXME: shouldn't be called directly, shouldn't be public functions
    public bool __IsValidPosition_Door(Tile t)
    {
        if(__IsValidPosition(t) == false)
        {
            return false;
        }
        //make sure we have a pair of E/W walls or N/S walls

        return true;
    }
}
