using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class LooseObjManager 
{
    //public List<LooseObject> looseObjects;
    public Dictionary<string, List<LooseObject>> looseObjects;
    Action<LooseObject> cbLooseObjCreated;
    public LooseObjManager()
    {
        looseObjects = new Dictionary<string, List<LooseObject>>(); 
    }


    void CleanLooseObject(LooseObject looseObj)
    {
        if (looseObj.stackSize == 0)
        {
            if (looseObjects.ContainsKey(looseObj.objectType))
            {
                looseObjects[looseObj.objectType].Remove(looseObj);
            }
            if(looseObj.tile != null)
            {
                looseObj.tile.looseObject = null;
                looseObj.tile = null;
            }

            if (looseObj.character != null)
            {
                looseObj.character.looseObject = null;
                looseObj.character = null;
            }
        }
    }

    public void RegisterLooseObjCreated(Action<LooseObject> callbackFunc)
    {
        cbLooseObjCreated += callbackFunc;
    }

    public void UnregisterLooseObjCreated(Action<LooseObject> callbackFunc)
    {
        cbLooseObjCreated -= callbackFunc;
    }

    bool tileWasEmpty;
    public bool PlaceLooseObj(Tile baseT, LooseObject newLooseObj, bool forcedPlace = false)
    {
        tileWasEmpty = false;
        Tile t = baseT;
        if(t.looseObject == null)
        {
            tileWasEmpty = true;
        }

        if(newLooseObj == null)
        {
            return false;
        }
        if (forcedPlace)
        {
            int fillTries = 0; //the amount of times an object tries to re-fill due to multiple overflows
            while(newLooseObj.stackSize > 0 && fillTries < newLooseObj.maxStackSize)
            {
                t = FindUnfilledTile(baseT, newLooseObj);
                if(t == null)
                {
                    return false;
                }
                if (t.PlaceLooseObject(newLooseObj) == false)
                {
                    Debug.LogError("Placement didn't work on supposedly valid tile");
                    return false;
                } else
                {
                    if(newLooseObj.stackSize > 0)
                    {
                        fillTries++;
                    }
                }
            }
            if(fillTries >= newLooseObj.maxStackSize)
            {
                Debug.LogError("Tried too many times to fill in a tile, likely would be stuck in an infinite loop.");
                return false;
            }
        } else
        {
            if (t.PlaceLooseObject(newLooseObj) == false)
            {
                //the tile did not accept the loose object, therefore stop
                return false;
            }
        }
        
       


        CleanLooseObject(newLooseObj);
        
        if (tileWasEmpty)
        {
            if (looseObjects.ContainsKey(t.looseObject.objectType) == false)
            {
                looseObjects[t.looseObject.objectType] = new List<LooseObject>();
            }
            looseObjects[t.looseObject.objectType].Add(t.looseObject);
            if (cbLooseObjCreated != null)
            {
                cbLooseObjCreated(t.looseObject);
            }

        }
        tileWasEmpty = false;

        
        return true;
    }

    Tile FindUnfilledTile(Tile basetile, LooseObject newLooseObj)
    {
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        List<Tile> checkedTiles = new List<Tile>();
        tilesToCheck.Enqueue(basetile);
        while (tilesToCheck.Count > 0)
        {
            Tile t = tilesToCheck.Dequeue();
            checkedTiles.Add(t);

            if (t.looseObject == null)
            {
                tileWasEmpty = true;
                return t;
            }
            else if (t.looseObject.objectType == newLooseObj.objectType && t.looseObject.stackSize < t.looseObject.maxStackSize)
            {
                tileWasEmpty = false;
                return t;
            }
            else
            {
                Tile[] ns = t.GetNeighbors();

                foreach (Tile t2 in ns)
                {
                    if (t2 != null
                        && (t2.installedObject == null || t2.installedObject.roomEnclosure == false)
                        && checkedTiles.Contains(t2) == false
                        && t2.TileType != TileType.Empty)
                    {
                        tilesToCheck.Enqueue(t2);
                    }
                }
            }
            
        }
        //no valid times
        Debug.Log("No valid tile for: " + newLooseObj.objectType + ". It has been destroyed");
        return null;
    }

    public bool PlaceLooseObj(Job job, LooseObject newLooseObj)
    {

        if(job.looseObjRequirements.ContainsKey(newLooseObj.objectType) == false)
        {
            Debug.LogError("Trying to add inventory to a job that it doesn't want.");
            return false;
        }
        job.looseObjRequirements[newLooseObj.objectType].stackSize += newLooseObj.stackSize;

        if(job.looseObjRequirements[newLooseObj.objectType].maxStackSize < job.looseObjRequirements[newLooseObj.objectType].stackSize)
        {
            newLooseObj.stackSize = job.looseObjRequirements[newLooseObj.objectType].stackSize -
                job.looseObjRequirements[newLooseObj.objectType].maxStackSize;

            job.looseObjRequirements[newLooseObj.objectType].stackSize = job.looseObjRequirements[newLooseObj.objectType].maxStackSize;
        } else
        {
            newLooseObj.stackSize = 0;
        }


        CleanLooseObject(newLooseObj);
        return true;
    }

    public bool PlaceLooseObj(Character character, LooseObject sourceLooseObj, int amount = -1)
    {
        if(amount < 0)
        {
            amount = sourceLooseObj.stackSize;
        } else
        {
            amount = Mathf.Min(amount, sourceLooseObj.stackSize);
        }

        if(character.looseObject == null)
        {
            character.looseObject = sourceLooseObj.Clone();
            character.looseObject.stackSize = 0;
            looseObjects[character.looseObject.objectType].Add(character.looseObject);
        } else if (character.looseObject.objectType != sourceLooseObj.objectType)
        {
            Debug.LogError("Character is trying to pickup a mis-matched object type");
            return false;
        }
        character.looseObject.stackSize += amount;

        if (character.looseObject.maxStackSize < character.looseObject.stackSize)
        {
            sourceLooseObj.stackSize = character.looseObject.stackSize - character.looseObject.maxStackSize;
            character.looseObject.stackSize = character.looseObject.maxStackSize;
        }
        else
        {
            sourceLooseObj.stackSize -= amount;
        }


        CleanLooseObject(sourceLooseObj);
        return true;
    }


    //gets a stack of the chosen object type
    public LooseObject GetClosestLooseObjectOfType(string objectType, Tile t, int desiredAmount)
    {

        //FIXME: we are lying right now, just getting the FIRST, not the closest, need a distance type database
        if (looseObjects.ContainsKey(objectType) == false)
        {
            Debug.LogError("No items of desired type");
            return null;
        }
        Path_AStar path = new Path_AStar(World.current, t, null, objectType);
        return path.EndTile().looseObject;
    }

    public Path_AStar GetPathToClosestLooseObject(string objectType, Tile t, int desiredAmount)
    {

        //FIXME: we are lying right now, just getting the FIRST, not the closest, need a distance type database
        if (looseObjects.ContainsKey(objectType) == false)
        {
            Debug.Log("No items of desired type: " + objectType);
            return null;
        }
        Path_AStar path = new Path_AStar(World.current, t, null, objectType);
        return path;
    }
}
