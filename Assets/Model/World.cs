using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World
{
    Tile[,] tiles;
    List<Character> characters;
    Dictionary<string, InstalledObject> installedObjectPrototypes;
    int width;

    Action<InstalledObject> cbIntalledObjectCreated;
    Action<Character> cbCharacterCreated;
    Action<Tile> cbTileChanged;
    //TODO: Most likely will be replaced with dedictaed class for managing job quues that might also be semi-static
    //or self initializing, for now this is just a public member of world
    public JobQueue jobQueue;
    public int Width
    {
        get
        {
            return width;
        }
    }
    int height;
    public int Height
    {
        get
        {
            return height;
        }
    }
    public World(int width = 100, int height = 100)
    {
        jobQueue = new JobQueue();
        this.width = width;
        this.height = height;
        tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].AddTileTypeUpdate(OnTileChanged);
            }
        }
        Debug.Log("World created with " + width + "," + height);
        CreateObjectPrototypes();

        characters = new List<Character>();
        
    }

    public void Update(float deltaTime)
    {
        foreach(Character c in characters)
        {
            c.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);
        characters.Add(c);
        if (cbCharacterCreated != null) { 
            cbCharacterCreated(c);
        }

        return c;
    }
    void CreateObjectPrototypes()
    {

        installedObjectPrototypes = new Dictionary<string, InstalledObject>();

        InstalledObject wallPrototype = InstalledObject.CreatePrototype(
            "Wall",
            0,
            1,
            1,
            true
            );
        installedObjectPrototypes.Add("Wall", wallPrototype);
    }

  
    public Tile getTileAt(int x, int y)
    {
        /*
        if(tiles[x,y] == null)
        {
            tiles[x,y] = new Tile(this, x, y)

        }
        */
        if(x > width || x < 0 || y > height || y < 0)
        {

            //Debug.LogError("Tile ("+x+","+y+") is out of range");
            return null;
        }
        return tiles[x, y];

    }
    /*
    public void RandomizeTiles()
    {

        for (int x = 0; x<width; x++)
		{
            for (int y = 0; y < height; y++)
            {
                if (UnityEngine.Random.Range(0,2) == 0)
                {

                    tiles[x, y].Type = TileType.Empty;
                } else
                {
                    tiles[x, y].Type = TileType.Floor;
                }
            }
		}
    }
    */
    

    public void PlaceInstalledObject(string objectType, Tile t)
    {
        //fixme: assumes 1x1 and no rotation

        if(installedObjectPrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("InstallledObjectPrototypes doesn't contain key for: " + objectType);
            return;
        }
        //InstalledObject.PlaceObject(installedObjectPrototypes[objectType],t);
        InstalledObject instObj = InstalledObject.PlaceObject(installedObjectPrototypes[objectType], t);

        if (instObj == null)
        {
            return;
        }
        if(cbIntalledObjectCreated != null)
        {
            cbIntalledObjectCreated(instObj);
        }
    }

    public void RegisterInstalledObjectCreated(Action<InstalledObject> callbackFunc)
    {
        cbIntalledObjectCreated += callbackFunc;
    }

    public void UnregisterInstalledObjectCreated(Action<InstalledObject> callbackFunc)
    {
        cbIntalledObjectCreated -= callbackFunc;
    }
    public void RegisterCharacterCreated(Action<Character> callbackFunc)
    {
        cbCharacterCreated += callbackFunc;
    }

    public void UnregisterCharacterCreated(Action<Character> callbackFunc)
    {
        cbCharacterCreated -= callbackFunc;
    }
    public void RegisterTileChanged(Action<Tile> callbackFunc)
    {
        cbTileChanged += callbackFunc;
    }

    public void UnregisterTileChanged(Action<Tile> callbackFunc)
    {
        cbTileChanged -= callbackFunc;
    }

    public void OnTileChanged(Tile t)
    {
        if(cbTileChanged == null)
        {
            return;
        }
        cbTileChanged(t);
    }

    public bool IsInstalledObjectPlacementValid(string instType, Tile t)
    {
        return installedObjectPrototypes[instType].isValidPosition(t);
    }

    public InstalledObject GetInstObjPrototype(string objectType)
    {
        if(installedObjectPrototypes.ContainsKey(objectType) == false){
            Debug.LogError("Inst prototype does not exist: "+objectType);
            return null;
        }
        return installedObjectPrototypes[objectType];
    }
}
