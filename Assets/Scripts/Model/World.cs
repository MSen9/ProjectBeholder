using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

public class World : IXmlSerializable
{
    Tile[,] tiles;
    public List<Character> characters;
    public List<InstalledObject> instObjects;
    public List<Room> rooms;
    public LooseObjManager looseObjManager;

    //The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;
    public Dictionary<string, InstalledObject> installedObjectPrototypes;
    public Dictionary<string, Job> instObjJobPrototype;
    int width;

    Action<InstalledObject> cbIntalledObjectCreated;
    Action<Character> cbCharacterCreated;
    Action<Tile> cbTileChanged;


    static public World current { get; protected set; }
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
    public World(int width = 200, int height = 50)
    {
        SetupWorld(width, height);

    }
    public Room GetOutsideRoom()
    {
        return rooms[0];
    }


    public void AddRoom(Room r)
    {

        rooms.Add(r);
    }

    public void CheckEmptyRooms()
    {
        List < Room > delRooms = new List<Room>();
        for (int i = 1; i < rooms.Count; i++)
        {
            if (rooms[i].hasTiles() == false)
            {
                delRooms.Add(rooms[i]);
                
            }
        }
        foreach(Room r in delRooms)
        {
            DeleteRoom(r);
        }
    }
    public void DeleteRoom(Room r)
    {
        if (r == GetOutsideRoom())
        {
            Debug.LogError("Tried to delete outside room");
            return;
        }
        //removes room from list, re-assigs tiles to outside
        rooms.Remove(r);
        //r.UnAssignAllTiles();
    }
    void SetupWorld(int width, int height)
    {

        //set the current world to be this world
        //Todo: do we need to do any cleanup of the old world?
        current = this;
        jobQueue = new JobQueue();
        this.width = width;
        this.height = height;
        tiles = new Tile[width, height];
        rooms = new List<Room>();
        rooms.Add(new Room());
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
                
                tiles[x, y].AddTileTypeUpdate(OnTileChanged);
                tiles[x, y].room = GetOutsideRoom(); //always in outside, default room
                GetOutsideRoom().AssignTile(tiles[x, y]);
                tiles[x, y].TileType = TileType.Floor;
            }
        }
        Debug.Log("World created with " + width + "," + height);
        CreateInstalledObjectPrototypes();

        characters = new List<Character>();
        instObjects = new List<InstalledObject>();
        looseObjManager = new LooseObjManager();

    }
    public World()
    {

    }


    public void Update(float deltaTime)
    {
        foreach (Character c in characters)
        {
            c.Update(deltaTime);
        }

        foreach (InstalledObject inst in instObjects)
        {
            inst.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);
        characters.Add(c);
        if (cbCharacterCreated != null)
        {
            cbCharacterCreated(c);
        }


        return c;
    }
    
    public void SetInstObjJobPrototype(Job j, InstalledObject instObj)
    {
        instObjJobPrototype[instObj.objectType] = j;
    }


    void LoadInstObjLua()
    {

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Lua");
        filePath = System.IO.Path.Combine(filePath, "InstalledObjects.lua");
        string myLuaCode = System.IO.File.ReadAllText(filePath);
        //instantiate singleton
        //new InstObjActions(myLuaCode);
    }
    void CreateInstalledObjectPrototypes()
    {
        //LoadInstObjLua();
        installedObjectPrototypes = new Dictionary<string, InstalledObject>();
        instObjJobPrototype = new Dictionary<string, Job>();

        //read INST OBJ prototype XML file here
        //opening the file ourselves
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "InstalledObjects.xml");
        string instObjXmlText = System.IO.File.ReadAllText(filePath );
        XmlTextReader reader = new XmlTextReader(new StringReader(instObjXmlText));
        if (reader.ReadToDescendant("InstalledObjects"))
        {
            if (reader.ReadToDescendant("InstalledObject"))
            {
                do
                {
                    InstalledObject instObj = new InstalledObject();
                    instObj.ReadXmlPrototype(reader);
                    installedObjectPrototypes.Add(instObj.objectType, instObj);
                } while (reader.ReadToNextSibling("InstalledObject"));
            } else
            {
                Debug.LogError("The inst obj definiton file has no InstObj elements");
            }
            
        }


        //adds relevant actions to applicable objects
        //TODO: Have this loaded in from the XML, allow for c# modding
        installedObjectPrototypes["Door"].RegisterIsEnterableAction(InstObjActions.Door_IsEnterable);
        installedObjectPrototypes["Door"].RegisterUpdateAction(InstObjActions.Door_UpdateAction);
        installedObjectPrototypes["MiningBase"].RegisterUpdateAction(InstObjActions.MiningBase_UpdateAction);
        //This bit will come from parsing a LUA file later
        //We will still need to check inst obj prototypes later
        //installedObjectPrototypes["Door"].RegisterUpdateAction(InstObjActions.Door_UpdateAction);
        //installedObjectPrototypes["Door"].IsEnterable = InstObjActions.Door_IsEnterable;
    }
    /*
    void CreateObjectPrototypes()
    {

        installedObjectPrototypes = new Dictionary<string, InstalledObject>();
        instObjJobPrototype = new Dictionary<string, Job>();
        InstalledObject wallPrototype = new InstalledObject(
            "Wall",
            0,
            1,
            1,
            true,
            true
            );
        installedObjectPrototypes.Add("Wall", wallPrototype);
        wallPrototype.Name = "Dumb Wall >:)";
        //instObjJobPrototype.Add("Wall", new Job(null, "Wall", InstObjActions.JobComplete_InstalledObject, 1f, new LooseObject[] { new LooseObject("Bars", 0, 5)}));
        InstalledObject doorPrototype = new InstalledObject(
            "Door",
            1, //pathfinding cost
            1,
            1,
            false,
            true
            );
        installedObjectPrototypes.Add("Door", doorPrototype);

        installedObjectPrototypes["Door"].SetParameter("openness", 0f);
        installedObjectPrototypes["Door"].SetParameter("is_opening", false);
        installedObjectPrototypes["Door"].RegisterUpdateAction(InstObjActions.Door_UpdateAction);
        installedObjectPrototypes["Door"].IsEnterable = InstObjActions.Door_IsEnterable;

        InstalledObject sPile = new InstalledObject(
            "Stockpile",
            1,
            1,
            1,
            false,
            false
            );
        installedObjectPrototypes.Add("Stockpile", sPile);
        instObjJobPrototype.Add("Stockpile", new Job(null, "Stockpile",
            InstObjActions.JobComplete_InstalledObject, -1f, null));

        InstalledObject generator = new InstalledObject(
            "Generator",
            3,
            2,
            2,
            false,
            false
            );
        installedObjectPrototypes.Add("Generator", generator);
        //instObjJobPrototype.Add("Stockpile", new Job(null, "Stockpile",
        //    InstObjActions.JobComplete_InstalledObject, -1f, null));

        InstalledObject miningPrototype = new InstalledObject(
            "MiningBase",
            3, //pathfinding cost
            3,
            2,
            false,
            false
            );
        installedObjectPrototypes.Add("MiningBase", miningPrototype);
        miningPrototype.jobSpotOffset = new Vector2(1, -1);
        installedObjectPrototypes["MiningBase"].RegisterUpdateAction(InstObjActions.MiningBase_UpdateAction);

    }
    */

    public Tile GetTileAt(int x, int y)
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


    public InstalledObject PlaceInstalledObject(string objectType, Tile t, bool doFloodFill = true)
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
        instObj.RegisterRemovedCB(OnInstObjRemoved);

        //do we need to recalculate our rooms?
        if (instObj.roomEnclosure && doFloodFill)
        {
            Room.DoRoomFloodFill(instObj.tile);
        }
        if (cbIntalledObjectCreated != null)
        {
            cbIntalledObjectCreated(instObj);
            InvalidateTileGraph();
        }


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
        if (installedObjectPrototypes.ContainsKey(objectType) == false)
        {
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

        writer.WriteStartElement("Rooms");
        foreach (Room r in rooms)
        {
            writer.WriteStartElement("Room");

            r.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();
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
        foreach (InstalledObject instObj in instObjects)
        {
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
                case "Rooms":
                    ReadXml_Rooms(reader);
                    break;
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
        //for testing: create investiory items

        //LooseObjManager.Add(looseObj);
    }

    public void ReadXml_Tiles(XmlReader reader)
    {
        //reader.ReadToDescendant("Tile");

        if (reader.ReadToDescendant("Tile"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                tiles[x, y].ReadXml(reader);
            } while (reader.ReadToNextSibling("Tile"));
        }

    }
    public void ReadXml_InstObjs(XmlReader reader)
    {


        //reader.ReadToDescendant("Tile");
        if (reader.ReadToDescendant("InstObj"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                //tiles[x, y].ReadXml(reader);
                InstalledObject instObj = PlaceInstalledObject(reader.GetAttribute("objectType"), tiles[x, y]);
                if (instObj != null)
                {
                    instObj.ReadXml(reader);
                }
            } while (reader.ReadToNextSibling("InstObj"));
        }

    }

    public void ReadXml_Characters(XmlReader reader)
    {


        //reader.ReadToDescendant("Tile");
        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                //tiles[x, y].ReadXml(reader);
                Character c = CreateCharacter(tiles[x, y]);
                if (c != null)
                {
                    c.ReadXml(reader);
                }
            } while (reader.ReadToNextSibling("Character"));
        }

    }

    public void OnInstObjRemoved(InstalledObject instObj)
    {
        instObjects.Remove(instObj);
    }

    public void ReadXml_Rooms(XmlReader reader)
    {
        if (reader.ReadToDescendant("Room"))
        {
            do
            {
                //tiles[x, y].ReadXml(reader);
                Room r = new Room();
                AddRoom(r);
                if (r != null)
                {
                    r.ReadXml(reader);
                }
            } while (reader.ReadToNextSibling("Room"));
        }
    }
}