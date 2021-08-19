using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveManager 
{
    public static string directory = "/SaveData/";
    public static string fileName = "MyData.txt";
    public static void save(World world)
    {
        string dir = Application.persistentDataPath + directory;
        if(Directory.Exists(dir) == false)
        {
            Directory.CreateDirectory(dir);
        }

        string json = JsonUtility.ToJson(world);
        
        File.WriteAllText(dir + fileName, json);
    }

    public static World Load()
    {
        string fullPath = Application.persistentDataPath + directory + fileName;
        World world = new World(100, 100);
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            world = JsonUtility.FromJson<World>(json);
        } else
        {
            Debug.Log("Save file does not exist");
        }
        return world;
    }
}
