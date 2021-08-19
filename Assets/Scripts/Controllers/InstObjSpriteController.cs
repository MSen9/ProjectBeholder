using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class InstObjSpriteController : MonoBehaviour
{

    Dictionary<InstalledObject, GameObject> installedGameObjectMap;

    Dictionary<string, Sprite> installedObjectSprites;
    // Start is called before the first frame update

    World world
    {
        get { return WorldController.Instance.World; }
    }
    void Start()
    {
        installedObjectSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/InstObjs/");
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            installedObjectSprites[s.name] = s;
        }
        installedGameObjectMap = new Dictionary<InstalledObject, GameObject>();
        world.RegisterInstalledObjectCreated(OnInstalledObjectCreated);

        //Go through any existingand call the oncrated event manually?
        foreach (InstalledObject instObj in world.instObjects)
        {
            OnInstalledObjectCreated(instObj);
        }
    }

    //float randomizeTileTimer =  2f;





    public void OnInstalledObjectCreated(InstalledObject obj)
    {
        //created a visual gameobject linked to this data.
        GameObject inst_go = new GameObject();

        //Tile tile_data = World.getTileAt(x, y);

        //add our tile-go pair to our dictionary
        installedGameObjectMap.Add(obj, inst_go);
        inst_go.name = obj.objectType + obj.tile.X + "_" + obj.tile.Y;
        inst_go.transform.position = new Vector3(obj.tile.X, obj.tile.Y);
        SpriteRenderer inst_sr = inst_go.AddComponent<SpriteRenderer>();
        inst_go.transform.SetParent(this.transform, true);

        inst_sr.sprite = GetSpriteForInstalledObject(obj); //FIXME
        inst_sr.sortingLayerName = "InstalledObject";
        obj.RegisterOnChangedCallback(OnInstalledObjectChanged);


        
    }
    void OnInstalledObjectChanged(InstalledObject obj)
    {

        //make sure the furniture's graphics are correct


        if (installedGameObjectMap.ContainsKey(obj) == false)
        {
            Debug.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map");
            return;
        }

        GameObject inst_go = installedGameObjectMap[obj];
        inst_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(obj);
    }
    public Sprite GetSpriteForInstalledObject(InstalledObject obj)
    {
        if (obj.linksToNeighbor == false) {
            return installedObjectSprites[obj.objectType];
        }

        //otherwise, the sprite name is more complicated
        string spriteName = obj.objectType + "_";
        //check for neightbors: North, East, South, West
        Tile t;
        int x = obj.tile.X;
        int y = obj.tile.Y;
        t = world.getTileAt(x, y + 1);
        if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType)
        {
            spriteName += "N";
        }
        t = world.getTileAt(x + 1, y);
        if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType)
        {
            spriteName += "E";
        }
        t = world.getTileAt(x, y - 1);
        if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType)
        {
            spriteName += "S";
        }
        t = world.getTileAt(x - 1, y);
        if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType)
        {
            spriteName += "W";
        }
        if (installedObjectSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("No spriteName of name: " + spriteName);
            return null;
        }

        return installedObjectSprites[spriteName];
    }
    public Sprite GetSpriteForInstalledObject(string objType) {
        if (installedObjectSprites.ContainsKey(objType))

        {
            return installedObjectSprites[objType];
        }
        if (installedObjectSprites.ContainsKey(objType + "_"))
        {
            return installedObjectSprites[objType+"_"];
        }

        Debug.LogError("No spriteName of name: " + objType);
        return null;
    }
}
