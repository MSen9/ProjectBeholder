using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public CanvasGroup canvasGroup;
    MouseController mouseController;
    public GameObject selected;
    Text selectedText;
    void Start()
    {
        selectedText = selected.GetComponent<Text>();
        mouseController = GameObject.FindObjectOfType<MouseController>();
        if (selectedText == null)
        {
            Debug.LogError("MouseOverTileTypeText: No 'text' component on this go");
            this.enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mouseController.mySelection != null)
        {
            object currSelection = mouseController.mySelection.stuffInTile[mouseController.mySelection.subSelection];
            selectedText.text = "Selecting: " + currSelection.GetType().ToString();
            //make it invisible
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

        }
        else
        {
            selectedText.text = "Selecting: Nothing";
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
