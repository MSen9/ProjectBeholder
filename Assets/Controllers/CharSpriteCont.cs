using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSpriteCont : MonoBehaviour
{

    Dictionary<Character, GameObject> characterGameObjectMap;

    Dictionary<string, Sprite> characterSprites;
    // Start is called before the first frame update

    World world
    {
        get { return WorldController.Instance.World; }
    }
    // Start is called before the first frame update
    void Start()
    {
        characterGameObjectMap = new Dictionary<Character, GameObject>();
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Characters/");
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            characterSprites[s.name] = s;
        }

        world.RegisterCharacterCreated(OnCharacterCreated);

        Character c = world.CreateCharacter(world.getTileAt(world.Width / 2, world.Height / 2));
        c.SetDesination(world.getTileAt(world.Width / 2 + 4, world.Height / 2));
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
        SpriteRenderer inst_sr = char_go.AddComponent<SpriteRenderer>();
        char_go.transform.SetParent(this.transform, true);

        inst_sr.sprite = characterSprites["BaseMan"]; //FIXME
        inst_sr.sortingLayerName = "Characters";
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
    
}
