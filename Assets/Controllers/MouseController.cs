using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor.Events;

public class MouseController : MonoBehaviour
{
    Vector3 lastFramePos;
    public GameObject circleCursorPrefab;
    Vector3 dragStartPosition;
    List<GameObject> dragPreviewGameObjects;
    float CAMERA_ZOOM_MULT = 0.8f;
    float CAMERA_ZOOM_MAX = 10f;
    float CAMERA_ZOOM_MIN = 1f;
    Vector3 currFramePos;
    bool canDragSelect = false;

    void Start()
    {
        lastFramePos = new Vector3(0, 0, 0);
        dragPreviewGameObjects = new List<GameObject>();
        //SimplePool.Preload(circleCursorPrefab, 100);
    }

    // Update is called once per frame
    void Update()
    {
        currFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        currFramePos.z = 0;

        updateSelectDrag();
        updateScreenDrag();



    }

    void updateSelectDrag()
    {


        if (Input.GetMouseButtonDown(0))
        {
            //start drag
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                dragStartPosition = currFramePos;
                canDragSelect = true;
            } else
            {
                canDragSelect = false;
                   
                return;
            }
            
        }
        if (canDragSelect)
        {
            int start_x = Mathf.RoundToInt(dragStartPosition.x);
            int end_x = Mathf.RoundToInt(currFramePos.x);
            int start_y = Mathf.RoundToInt(dragStartPosition.y);
            int end_y = Mathf.RoundToInt(currFramePos.y);
            if (end_x < start_x)
            {
                int tmp = end_x;
                end_x = start_x;
                start_x = tmp;
            }
            if (end_y < start_y)
            {
                int tmp = end_y;
                end_y = start_y;
                start_y = tmp;
            }

            //Clean up old drag previews
            while (dragPreviewGameObjects.Count > 0)
            {
                GameObject go = dragPreviewGameObjects[0];
                dragPreviewGameObjects.RemoveAt(0);
                SimplePool.Despawn(go);
            }
            if (Input.GetMouseButton(0))
            {
                //Display a preview of the drag area


                for (int x = start_x; x <= end_x; x++)
                {
                    for (int y = start_y; y <= end_y; y++)
                    {
                        Tile t = WorldController.Instance.World.getTileAt(x, y);
                        if (t != null)
                        {
                            //Display the building hint on top of tile pos
                            GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                            go.transform.SetParent(this.transform, true);
                            dragPreviewGameObjects.Add(go);
                        }
                    }
                }
            }

            //end drag
            if (Input.GetMouseButtonUp(0))
            {
                BuildModeController bmc = GameObject.FindObjectOfType<BuildModeController>();
                for (int x = start_x; x <= end_x; x++)
                {
                    for (int y = start_y; y <= end_y; y++)
                    {
                        Tile t = WorldController.Instance.World.getTileAt(x, y);
                        if (t != null)
                        {
                            //call build mode controller do build
                            bmc.DoBuild(t);
                        }
                    }
                }
            }
        }
    }


    void updateScreenDrag()
    {
        //handle screen dragging
        
        //right of middle mouse button
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector3 diff = lastFramePos - currFramePos;
            Camera.main.transform.Translate(diff);
        }
        lastFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePos.z = 0;

        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel") * CAMERA_ZOOM_MULT;


        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, CAMERA_ZOOM_MIN, CAMERA_ZOOM_MAX);
    }
   
}
