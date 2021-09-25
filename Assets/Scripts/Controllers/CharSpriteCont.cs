using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSpriteCont : MonoBehaviour
{

    Dictionary<Character, GameObject> characterGameObjectMap;
    string CHARACTER_SPRITE_FOLDER = "Characters_";
    //Dictionary<string, Sprite> characterSprites;
    // Start is called before the first frame update

    World world
    {
        get { return WorldController.Instance.World; }
    }
    // Start is called before the first frame update
    void Start()
    {
        characterGameObjectMap = new Dictionary<Character, GameObject>();
        /*
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Characters/");
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            characterSprites[s.name] = s;
        }
        */
        world.RegisterCharacterCreated(OnCharacterCreated);

       //check for pre-existing
       foreach(Character c in world.characters)
        {
            OnCharacterCreated(c);
        }
    }

    public void OnCharacterCreated(Character character)
    {
        //created a visual gameobject linked to this data.
        GameObject char_go = new GameObject();

        //Tile tile_data = World.getTileAt(x, y);

        //add our tile-go pair to our dictionary

        characterGameObjectMap.Add(character, char_go);

        char_go.name = "Character";
        char_go.transform.position = new Vector3(character.X, character.Y);
        SpriteRenderer char_sr = char_go.AddComponent<SpriteRenderer>();
        char_go.transform.SetParent(this.transform, true);

        
        char_sr.sprite = SpriteManager.current.sprites[CHARACTER_SPRITE_FOLDER+"BaseMan"]; //FIXME: add a more advanced way of getting new character sprites
        char_sr.sortingLayerName = "Characters";

        /*
        BoxCollider2D bCollider = char_go.AddComponent<BoxCollider2D>();
        Vector2 spriteSize = char_sr.sprite.bounds.size;
        bCollider.size = spriteSize;
        */
        //bCollider.center = new Vector2(spriteSize.x / 2, 0);
        character.RegisterOnChangedCB(OnCharacterChanged);
    }
    
    void OnCharacterChanged(Character c)
    {

        //make sure the furniture's graphics are correct


        if (characterGameObjectMap.ContainsKey(c) == false)
        {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map");
            return;
        }

        GameObject char_go = characterGameObjectMap[c];
        char_go.transform.position = new Vector3(c.X, c.Y,char_go.transform.position.z);
    }
    
    public List<Character> GetCharsUnderMouse(Tile t, float mouseX, float mouseY)
    {
        List<Character> chars = new List<Character>();
        foreach(Character c in characterGameObjectMap.Keys)
        {
            if (t == c.CurrTile || t.IsNeighbor(c.CurrTile, true))
            {
                //mouse could be over it
                GameObject char_go = characterGameObjectMap[c];
                SpriteRenderer char_sr = char_go.GetComponent<SpriteRenderer>();
                Vector2 spriteSize = char_sr.sprite.bounds.size;
                //assume central pivot for now
                float xDist = Mathf.Abs(char_go.transform.position.x - mouseX);
                float yDist = Mathf.Abs(char_go.transform.position.y - mouseY);
                if(xDist <= spriteSize.x/2 && yDist <= spriteSize.y / 2)
                {
                    chars.Add(c);
                    Debug.Log("Clicked over a character");
                }
            }
        }
        return chars;
    }
}
