using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenu : MonoBehaviour
{
    public GameObject baseInstObjButton;
    // Start is called before the first frame update
    void Start()
    {
        int buttCount = 0;
        //for each inst obj prototype in our world, create an instance of our button
        BuildModeController bmc = GameObject.FindObjectOfType<BuildModeController>();
        foreach(string s in World.current.installedObjectPrototypes.Keys)
        {
            
            GameObject go = Instantiate(baseInstObjButton);
            go.transform.SetParent(this.transform);

            go.name = "Button - Build " + s;

            go.transform.GetComponentInChildren<Text>().text = s;



            Button b = go.GetComponent<Button>();
            string objectId = s;
            b.onClick.AddListener(delegate { bmc.SetMode_BuildObject(objectId); });
        }

        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, 30 * buttCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
