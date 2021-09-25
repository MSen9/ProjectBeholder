using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        WorldController.Instance.World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);
        WorldController.Instance.World.RegisterTileChanged(OnTileTypeChanged);
    }
    bool soundPlayed = false;
    // Update is called once per frame
    void Update()
    {
        soundPlayed = false;
    }

    void OnInstalledObjectCreated(InstalledObject obj)
    {
        //FIXME
        
        if (soundPlayed == false) { 
            AudioClip ac = Resources.Load<AudioClip>("Sounds/WallPlace");
            if (ac == null)
            {
                //load default on create sound.
                //ac = Resources.Load<AudioClip>("Sounds/WallPlace");
                return;
            }
            //AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
            soundPlayed = true;
        }

    }
    void OnTileTypeChanged(Tile tile_data)
    {
        if (soundPlayed == false)
        {
            AudioClip ac = Resources.Load<AudioClip>("Sounds/TilePlace");
            if (ac == null)
            {
                //load default on create sound.
                //ac = Resources.Load<AudioClip>("Sounds/WallPlace");
                return;
            }
            //AudioSource.PlayClipAtPoint(ac, new Vector3(tile_data.X, tile_data.Y, Camera.main.transform.position.z));
            soundPlayed = true;
        }
        
    }
}
