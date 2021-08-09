using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public Sprite floorSprite;
    World world;
    // Start is called before the first frame update
    void Start()
    {
        //create empty world
        world = new World();
        //world.RandomizeTiles();
        //create a GameObject for each tile
        int w = world.Width;
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                GameObject tile_go = new GameObject();
                tile_go.name = "Tile_" + x + "_" + y;
                Tile tile_data = world.getTileAt(x, y);
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y);
                SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();
                /*
                if(tile_data.Type == Tile.TileType.Floor)
                {
                    tile_sr.sprite = floorSprite;
                }
                */
               
            }
        }
        world.RandomizeTiles();
    }

    float randomizeTileTimer =  2f;


    // Update is called once per frame
    void Update()
    {
        randomizeTileTimer -= Time.deltaTime;
        if (randomizeTileTimer < 0)
        {
            world.RandomizeTiles();
            randomizeTileTimer = 2f;
        }
        
    }

    void OnTileTypeChanged(Tile tile_data, GameObject tile_go)
    {
        if (tile_data.Type == Tile.TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        } else if (tile_data.Type == Tile.TileType.Empty)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = null;
        } else
        {
            Debug.LogError("OnTileTypeChanged- Unrecognized tile type");

        }
    }
}
