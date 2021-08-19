using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;


public class World : IXmlSerializable
{
    Tile[,] tiles;
    public List<Character> characters;
    public List<InstalledObject> instObjects;

    //The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;
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
        SetupWorld(width, height);

    }

    void SetupWorld(int width, int height)
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
        instObjects = new List<InstalledObject>();
    }
    public World() : this(100, 100)
    {

    }

    public void Update(float deltaTime)
    {
        foreach (Character c in characters)
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
        if (x >= width || x < 0 || y >= height || y < 0)
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


    public InstalledObject PlaceInstalledObject(string objectType, Tile t)
    {
        //fixme: assumes 1x1 and no rotation

        if (installedObjectPrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("InstallledObjectPrototypes doesn't contain key for: " + objectType);
            return null;
        }
        //InstalledObject.PlaceObject(installedObjectPrototypes[objectType],t);
        InstalledObject instObj = InstalledObject.PlaceObject(installedObjectPrototypes[objectType], t);

        if (instObj == null)
        {
            return null;
        }

        instObjects.Add(instObj);
        if (cbIntalledObjectCreated != null)
        {
            cbIntalledObjectCreated(instObj);
        }
        InvalidateTileGraph();

        return instObj;
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
        if (cbTileChanged == null)
        {
            return;
        }
        cbTileChanged(t);

        InvalidateTileGraph();
    }
    //called whenever a change to the world
    //means that our old pathfinding info in invalid
    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }
    public bool IsInstalledObjectPlacementValid(string instType, Tile t)
    {
        return installedObjectPrototypes[instType].isValidPosition(t);
    }

    public InstalledObject GetInstObjPrototype(string objectType)
    {
        if (installedObjectPrototypes.ContainsKey(objectType) == false) {
            Debug.LogError("Inst prototype does not exist: " + objectType);
            return null;
        }
        return installedObjectPrototypes[objectType];
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        //save info here
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                writer.WriteStartElement("Tile");

                tiles[x, y].WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("InstObjs");
        foreach(InstalledObject instObj in instObjects) { 
            writer.WriteStartElement("InstObj");

            instObj.WriteXml(writer);
            writer.WriteEndElement();
            
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            writer.WriteStartElement("Character");

            c.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();
    }

    public void ReadXml(XmlReader reader)
    {
        //load info here
        Debug.Log("Read XML");
        Debug.Log(reader.Name);

        int w = int.Parse(reader.GetAttribute("Width"));
        int h = int.Parse(reader.GetAttribute("Height"));
        SetupWorld(w, h);


        while (reader.Read())
        {

            switch (reader.Name)
            {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "InstObjs":
                    ReadXml_InstObjs(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }
        }

    }

    public void ReadXml_Tiles(XmlReader reader)
    {
        //reader.ReadToDescendant("Tile");
        while (reader.Read())
        {
            if (reader.Name != "Tile")
            {
                return;
            }
            int x = int.Parse(reader.GetAttribute("X"));
            int y = int.Parse(reader.GetAttribute("Y"));
            tiles[x, y].ReadXml(reader);
        }

    }
    public void ReadXml_InstObjs(XmlReader reader)
    {


        //reader.ReadToDescendant("Tile");
        while (reader.Read())
        {
            if (reader.Name != "InstObj")
            {
                return;
            }
            int x = int.Parse(reader.GetAttribute("X"));
            int y = int.Parse(reader.GetAttribute("Y"));
            //tiles[x, y].ReadXml(reader);
            InstalledObject instObj = PlaceInstalledObject(reader.GetAttribute("objectType"), tiles[x, y]);
            if(instObj != null)
            {
                instObj.ReadXml(reader);
            }
        }

    }

    public void ReadXml_Characters(XmlReader reader)
    {


        //reader.ReadToDescendant("Tile");
        while (reader.Read())
        {
            if (reader.Name != "Character")
            {
                return;
            }
            int x = int.Parse(reader.GetAttribute("X"));
            int y = int.Parse(reader.GetAttribute("Y"));
            //tiles[x, y].ReadXml(reader);
            Character c = CreateCharacter(tiles[x, y]);
            if (c != null)
            {
                c.ReadXml(reader);
            }
        }

    }
}