using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LooseObjSpriteController : MonoBehaviour
{
    public GameObject looseObjTextCanvas;
    string LOOSE_OBJ_SPRITE_FOLDER = "LooseObjs_";
    Dictionary<LooseObject, GameObject> looseObjGameObjectMap;

    //Dictionary<string, Sprite> looseObjSprites;
    LooseObjManager looseObjManager;
    // Start is called before the first frame update

    World world
    {
        get { return WorldController.Instance.World; }
    }
    // Start is called before the first frame update
    void Start()
    {
        looseObjManager = WorldController.Instance.world.looseObjManager;
        looseObjGameObjectMap = new Dictionary<LooseObject, GameObject>();
        /*
        looseObjSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/LooseObjs/");
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            looseObjSprites[s.name] = s;
        }
        */
        looseObjManager.RegisterLooseObjCreated(OnLooseObjectCreated);

       //check for pre-existing
       foreach(string objectType in world.looseObjManager.looseObjects.Keys)
        {
            foreach(LooseObject looseObj in world.looseObjManager.looseObjects[objectType])
            {
                OnLooseObjectCreated(looseObj);
            }
           
        }
    }

    public void OnLooseObjectCreated(LooseObject looseObj)
    {
        //created a visual gameobject linked to this data.
        GameObject looseObj_go = new GameObject();

        //Tile tile_data = World.getTileAt(x, y);

        //add our tile-go pair to our dictionary

        looseObjGameObjectMap.Add(looseObj, looseObj_go);

        looseObj_go.name = looseObj.objectType;
        looseObj_go.transform.position = new Vector3(looseObj.tile.X, looseObj.tile.Y);
        SpriteRenderer inst_sr = looseObj_go.AddComponent<SpriteRenderer>();
        looseObj_go.transform.SetParent(this.transform, true);

        inst_sr.sprite = SpriteManager.current.sprites[LOOSE_OBJ_SPRITE_FOLDER+looseObj.objectType]; 
        inst_sr.sortingLayerName = "LooseObjects";

        if(looseObj.maxStackSize > 1)
        {
            //this is a stackable object, so lets add a Inventory UI to it.
            GameObject ui_go = Instantiate(looseObjTextCanvas);
            ui_go.transform.SetParent(looseObj_go.transform);
            ui_go.transform.localPosition = Vector3.zero;
            ui_go.GetComponentInChildren<Text>().text = looseObj.stackSize.ToString();
        }
        //character.RegisterOnChangedCB(OnCharacterChanged);

        looseObj.RegisterLooseObjChanged(OnLooseObjChanged);
    }

    void OnLooseObjChanged(LooseObject looseObj)
    {


        //make sure the furniture's graphics are correct


        if (looseObjGameObjectMap.ContainsKey(looseObj) == false)
        {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map");
            return;
        }

        GameObject obj_go = looseObjGameObjectMap[looseObj];
        obj_go.transform.position = new Vector3(looseObj.tile.X, looseObj.tile.Y, obj_go.transform.position.z);
        if (looseObj.stackSize > 0)
        {
            Text text = obj_go.GetComponentInChildren<Text>();

            //FIXME: if maxstacksize changed to/from 1, then we either need to create or destroy the dest
            if (looseObj.maxStackSize > 1 && text != null)
            {
                text.text = looseObj.stackSize.ToString();
            }
        }
        else
        {
            Destroy(obj_go);
            looseObjGameObjectMap.Remove(looseObj);
            looseObj.UnregisterLooseObjChanged(OnLooseObjChanged);
                
        }
    }
}
