using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System;

public class SpriteManager : MonoBehaviour
{

    //Sprite manager isn't responsible for actually creating GameObjects.
    //That is going to be the job of the individaul
    //This class loads all sprites from disk and keeps them organized
    public Dictionary<string, Sprite> sprites;
    Dictionary<string, int[]> neighborCoords;
    public static SpriteManager current;
    int BASE_PIXELS_PER_UNIT = 32;
    void OnEnable()
    {
        current = this;
        /* Documents the base offset for the sprites
         <Sprite name="Wall_N" x="320" y="0" w="32" h="32"/>
          <Sprite name="Wall_NE" x="0" y="128" w="32" h="32"/>
          <Sprite name="Wall_NS" x="0" y="96" w="32" h="32"/>
          <Sprite name="Wall_NW" x="64" y="128" w="32" h="32"/>
          <Sprite name="Wall_NES" x="128" y="32" w="32" h="32"/>
          <Sprite name="Wall_NEW" x="192" y="128" w="32" h="32"/>
          <Sprite name="Wall_NSW" x="320" y="160" w="32" h="32"/>
          <Sprite name="Wall_NESW" x="320" y="32" w="32" h="32"/>
          <Sprite name="Wall_E" x="362" y="32" w="32" h="32"/>
          <Sprite name="Wall_ES" x="0" y="64" w="32" h="32"/>
          <Sprite name="Wall_EW" x="64" y="32" w="32" h="32"/>
          <Sprite name="Wall_ESW" x="192" y="192" w="32" h="32"/>
          <Sprite name="Wall_S" x="320" y="64" w="32" h="32"/>
          <Sprite name="Wall_SW" x="64" y="64" w="32" h="32"/>
          <Sprite name="Wall_W" x="288" y="32" w="32" h="32"/>
        */
        neighborCoords = new Dictionary<string, int[]>()
        {
            {"_",new int[]{0,0} },
            {"_N",new int[]{10,2} },
            {"_NE",new int[]{0,4} },
            {"_NS",new int[]{0,3} },
            {"_NW",new int[]{2,4} },
            {"_NES",new int[]{4,1} },
            {"_NEW",new int[]{6,4} },
            {"_NSW",new int[]{10,5} },
            {"_NESW",new int[]{10,1} },
            {"_E",new int[]{9,1} },
            {"_ES",new int[]{0,2} },
            {"_EW",new int[]{1,2} },
            {"_ESW",new int[]{6,6} },
            {"_S",new int[]{10,0} },
            {"_SW",new int[]{2,2} },
            {"_W",new int[]{11,1} }
        };
        LoadSprites();
    }


    void LoadSprites()
    {
        sprites = new Dictionary<string, Sprite>();
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Sprites");
        //filePath = System.IO.Path.Combine(filePath, "Selection.png");
        LoadSpritesFromDirectory(filePath);
    }

    void LoadSpritesFromDirectory(string filePath)
    {
        string[] subDirs = Directory.GetDirectories(filePath);

        //First, we're going to see if we have any more sub-directories.
        //if so--call LoadSpritesFromDirectory on that.
        foreach(string sd in subDirs)
        {
            LoadSpritesFromDirectory(sd);
        }


        string[] filesInDir = Directory.GetFiles(filePath);
        foreach(string fn in filesInDir)
        {
            //is this an image file?
            //Unity's LoadImage seems to support only png or jpg
            //Just try to load the image and catch it if it fails
            if(fn.Contains(".png") || fn.Contains(".jpg")){
                if(fn.Contains(".meta") == false)
                {
                    LoadImage(fn);
                }
            }
            

        }

    }

    void LoadImage(string filePath)
    {
        //load the file into a textrue
        byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
        Texture2D imageTexture = new Texture2D(0, 0);
        if(imageTexture.LoadImage(imageBytes))
        {
            //image loaded so look for matching XML
            string baseSpriteName = Path.GetFileNameWithoutExtension(filePath);
            string basePath = Path.GetDirectoryName(filePath);

            
            
            string xmlPath = System.IO.Path.Combine(basePath, baseSpriteName + ".xml");

            //update the base sprite name to include the folder it comes from, this can help stop issues where there may be a loose object and character named "bar"
            string[] fileFolders = basePath.Split('\\');
            string lastFolder = fileFolders[fileFolders.Length - 1];
            baseSpriteName = lastFolder + "_" + baseSpriteName;
            try
            {
                string xmlText = System.IO.File.ReadAllText(xmlPath);
                XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));
                //take in xml params
                if (reader.ReadToDescendant("Sprites") && reader.ReadToDescendant("Sprite"))
                {
                    do
                    {
                        if (reader.GetAttribute("name") == null)
                        {
                            Debug.LogError("No sprite name exists for path: " + filePath);
                            return;
                        }
                        string name = reader.GetAttribute("name");

                        int x = 0;
                        int y = 0;
                        int w = 32;
                        int h = 32;
                        int pixelsPerUnit = 32;
                        

                        if (reader.GetAttribute("x") != null)
                        {
                            x = Convert.ToInt32(reader.GetAttribute("x"));
                        }
                        if (reader.GetAttribute("y") != null)
                        {
                            y = Convert.ToInt32(reader.GetAttribute("y"));
                        }
                        if (reader.GetAttribute("w") != null)
                        {
                            w = Convert.ToInt32(reader.GetAttribute("w"));
                        }
                        if (reader.GetAttribute("h") != null)
                        {
                            h = Convert.ToInt32(reader.GetAttribute("h"));
                        }
                        if (reader.GetAttribute("pixelsPerUnit") != null)
                        {
                            pixelsPerUnit = Convert.ToInt32(reader.GetAttribute("pixelsPerUnit"));
                        }
                        if (reader.GetAttribute("neighborLinks") != null)
                        {
                            LoadNeighborSprites(baseSpriteName, imageTexture, new Rect(x, y, w, h), pixelsPerUnit);

                        }
                        else
                        {

                            //just get sprite
                            LoadSingleSprite(baseSpriteName, imageTexture, new Rect(x, y, w, h), pixelsPerUnit);
                        }
                    } while (reader.ReadToNextSibling("Sprite"));

                }
            }
            catch
            {
                //likely means no file exists for the given name, just use defualt values
                LoadSingleSprite(baseSpriteName, imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), BASE_PIXELS_PER_UNIT);
            }
            //attempt to load/pase the XML file
            
        }
       
    }
    void LoadSingleSprite(string baseSpriteName, Texture2D imageTexture, Rect coords, int pixelsPerUnit)
    {
      
        Vector2 pivotPoint = new Vector2(0.5f, 0.5f); //Ranges from 0..1  -- so 0.5f is center
        Sprite s = Sprite.Create(imageTexture, coords, pivotPoint, pixelsPerUnit);
        sprites[baseSpriteName] = s;
    }
    //loads sprites that link to one-another
    void LoadNeighborSprites(string baseSpriteName, Texture2D imageTexture, Rect coords, int pixelsPerUnit)
    {
        string indivSpriteName;
        Rect indivCoords;
        foreach (KeyValuePair<string,int[]> entry in neighborCoords)
        {
            //create a sprite for each coord
            indivSpriteName = baseSpriteName + entry.Key;
            indivCoords = new Rect(coords.x + pixelsPerUnit * entry.Value[0], imageTexture.height - (coords.y + pixelsPerUnit * (entry.Value[1])+coords.height ), coords.width, coords.height);
            LoadSingleSprite(indivSpriteName, imageTexture, indivCoords, pixelsPerUnit);
        }
    }

    public Sprite GetSprite(string spriteName)
    {
        if(sprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("No sprite with name: " + spriteName);
            return null;
        }
        return sprites[spriteName];
    }
}
