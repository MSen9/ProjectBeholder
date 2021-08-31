using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TileSpriteController : MonoBehaviour
{
    Dictionary<Tile, GameObject> tileGameObjectMap;
    public Sprite floorSprite;
    public Sprite emptyFloorSprite;
    // Start is called before the first frame update

    World world
    {
        get { return WorldController.Instance.World; } 
    }
    void Start()
    {
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        int w = world.Width;
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                GameObject tile_go = new GameObject();

                Tile tile_data = world.GetTileAt(x, y);

                //add our tile-go pair to our dictionary
                tileGameObjectMap.Add(tile_data, tile_go);
                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y);
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();
                tile_sr.sprite = emptyFloorSprite;


                tile_go.transform.SetParent(this.transform, true);

                OnTileChanged(tile_data);
            }
        }
        //instantiate dictionary that tracks which gameObject is rendering which tile data
        //create a GameObject for each tiles
        world.RegisterTileChanged(OnTileChanged);

    }

    //float randomizeTileTimer =  2f;


   
    //EXAMPLE, NOT USED
    void DestroyAllGameObjects()
    {
        //might be called on floor change
        //used to destroy all visual gObs
        while(tileGameObjectMap.Count > 0)
        {
            Tile tile_data = tileGameObjectMap.Keys.First();
            GameObject tile_go = tileGameObjectMap[tile_data];
            tileGameObjectMap.Remove(tile_data);

            tile_data.RemoveTileTypeUpdate(OnTileChanged);

            //Destory the visual GameObject
            Destroy(tile_go);
        }
    }
    void OnTileChanged(Tile tile_data)
    {

        if (!tileGameObjectMap.ContainsKey(tile_data))
        {
            Debug.LogError("tileGameObjectMap Doesn't contain the tile_data. May not be in dicitonary");
            return;
        }
        GameObject tile_go = tileGameObjectMap[tile_data];
        if (tile_go == null)
        {
            Debug.LogError("tile_go not defined in dictionary");
            return;
        }


        if (tile_data.tileType ==TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        } else if (tile_data.tileType == TileType.Empty)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = emptyFloorSprite;
        } else
        {
            Debug.LogError("OnTileTypeChanged- Unrecognized tile type");

        }
    }

    
    
}
