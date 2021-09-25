using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MouseOverInfo : MonoBehaviour
{
    //every frame this script check to see which tile is under the mouse then updates the GetComponent<Tet>().text param
    //of
    // Start is called before the first frame update
    
    MouseController mouseController;
    public GameObject tileType;
    public GameObject roomID;
    public GameObject instObjType;
    

    Text tileTypeText;
    Text roomIdText;
    Text instObjText;
    
    void Start()
    {
        tileTypeText = tileType.GetComponent<Text>();
        roomIdText = roomID.GetComponent<Text>();
        instObjText = instObjType.GetComponent<Text>();
      
        if (tileTypeText == null || roomIdText == null || instObjText == null)
        {
            Debug.LogError("MouseOverTileTypeText: No 'text' component on this go");
            this.enabled = false;
            return;
        }
        mouseController = GameObject.FindObjectOfType<MouseController>();
        if(mouseController == null)
        {
            Debug.LogError("No mouse controller.");
            this.enabled = false;
            return;
        }
    }


    // Update is called once per frame
    void Update()
    {
        Tile t = mouseController.GetMouseOverTile();
        if (t != null) { 
            tileTypeText.text = "Tile Type: " + t.TileType.ToString();
            roomIdText.text = "Room ID: " + World.current.rooms.IndexOf(t.room);
            if(t.installedObject != null) { 
                instObjText.text = "InstObj type: " +  t.installedObject.Name;
            } else
            {
                instObjText.text = "InstObj type: null";
            }
        }
    }
}
