using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WorldController : MonoBehaviour
{
    

   
    public static WorldController Instance { get; protected set; }
    public World World { get; protected set; }

    // Start is called before the first frame update
    void OnEnable()
    {
        if(Instance != null) {
            Debug.LogError("There should never be two world controllers.");
        }
        Instance = this;
        //create empty world
        World = new World();
        
        //instantiate dictionary that tracks which gameObject is rendering which tile data

        
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
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
        return World.getTileAt(x, y);
    }


   
    
}
