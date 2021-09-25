using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour
{

    public static string directory = "/SaveData/";
    public static string fileName = "MyData.txt";

    public static WorldController Instance { get; protected set; }
    public World World { get; protected set; }
    public World world { get
        {
            return World;
        }
}
    static bool loadWorld = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        if(Instance != null) {
            Debug.LogError("There should never be two world controllers.");
        }
        Instance = this;
        
        //create empty world
        if (loadWorld)
        {
            CreateWorldFromSave();
        }
        else
        {
            CreateEmptyWorld(200,50);
            //world generation code
            MapGeneration();
        }
        loadWorld = false;
    }

    void Update()
    {
        //TODO: add pause/unpaunce, speed controls, etc...
        World.Update(Time.deltaTime);
    }

    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.RoundToInt(coord.x);
        int y = Mathf.RoundToInt(coord.y);

        //gameObject.FindObjectOfType<WorldController>();
        return World.GetTileAt(x, y);
    }
    public void NewWorld()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveWorld()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();

        serializer.Serialize(writer,World);

        Debug.Log(writer.ToString());

        string dir = Application.persistentDataPath + directory;
        if (Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }


        File.WriteAllText(dir + fileName, writer.ToString());
        PlayerPrefs.SetString("SaveGame00", writer.ToString());
        writer.Close();
        
   
        
    
       
    }

    public void LoadWorld()
    {
        
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }
    void CreateEmptyWorld(int width, int height)
    {
        World = new World(width,height);

        //instantiate dictionary that tracks which gameObject is rendering which tile data
        Character c = world.CreateCharacter(world.GetTileAt(10, world.Height / 2));
        world.CreateCharacter(world.GetTileAt(6, world.Height / 2));
        world.CreateCharacter(world.GetTileAt(8, world.Height / 2));
        Camera.main.transform.position = new Vector3(12, World.Height / 2, Camera.main.transform.position.z);
    }

    void MapGeneration()
    {
        //function for creating the world
        //For now: create a wavey line of rock walls about ~20 tiles right the start, then make everything else pure rock
        int START_ROCK = 20;
        for (int x = 20; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                World.PlaceInstalledObject("RockWall", World.GetTileAt(x, y), false);
            }
        }
        int ROCK_MAX = 20;
        int ROCK_MIN = 14;
        int rockOffset = 19;
        for (int y = 0; y < World.Height; y++)
        {
            for (int x = START_ROCK-1; x >= rockOffset; x--)
            {
                World.PlaceInstalledObject("RockWall", World.GetTileAt(x, y), false);
            }
            float moveOdds = UnityEngine.Random.Range(0.0f, 1.0f);

            if(moveOdds > .75)
            {
                if(rockOffset > ROCK_MIN)
                {
                    rockOffset--;
                }
            } else if(moveOdds < .25)
            {
                if (rockOffset < ROCK_MAX)
                {
                    rockOffset++;
                }
            }
        }
    }

    void CreateWorldFromSave()
    {
        //create world from our save file data
        
        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        
        World = (World)serializer.Deserialize(reader);
        reader.Close();

        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }
   
    
}
