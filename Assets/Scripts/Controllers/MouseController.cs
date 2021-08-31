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
    BuildModeController bmc;
    InstObjSpriteController iosc;
    GameObject instObjPreview;
    GameObject instDragPrevs;
    bool isDragging;

    enum MouseMode
    {
        Build,
        Select
    }
    MouseMode currMouseMode = MouseMode.Select;
  void Start()
    {
        isDragging = false;
        bmc = GameObject.FindObjectOfType<BuildModeController>();
        lastFramePos = new Vector3(0, 0, 0);
        dragPreviewGameObjects = new List<GameObject>();
        //SimplePool.Preload(circleCursorPrefab, 100);

        iosc = GameObject.FindObjectOfType<InstObjSpriteController>();
        instObjPreview = new GameObject();
        instObjPreview.transform.SetParent(gameObject.transform);
        instObjPreview.AddComponent<SpriteRenderer>();
        instObjPreview.SetActive(false);
    }


    public void SetMode_Build()
    {
        currMouseMode = MouseMode.Build;
    }
    public Vector3 GetMousePosition()
    {
        return currFramePos;
    }
    
    public Tile GetMouseOverTile()
    {
        return WorldController.Instance.world.GetTileAt(Mathf.RoundToInt(currFramePos.x), Mathf.RoundToInt(currFramePos.y));
    }
    // Update is called once per frame
    void Update()
    {
        currFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        currFramePos.z = 0;

        if(currMouseMode == MouseMode.Build)
        {
            updateSelectDrag();
        }
        
        updateScreenDrag();



    }

    
    void updateSelectDrag()
    {

        while (dragPreviewGameObjects.Count > 0)
        {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }

        if (Input.GetMouseButtonDown(1))
        {
            isDragging = false;
            currMouseMode = MouseMode.Select;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //start drag
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                dragStartPosition = currFramePos;
                isDragging = true;
                canDragSelect = true;
            } else
            {
                canDragSelect = false;
                   
                return;
            }
            
        }


        
        if(currMouseMode != MouseMode.Build)
        {

            return;
        }
        if (bmc.IsInstObjDraggable() == false || isDragging == false)
        {
            dragStartPosition = currFramePos;
        }
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

        //if (Input.GetMouseButton(0))
        //{
        //Display a preview of the drag area

        if (canDragSelect)
        {
            for (int x = start_x; x <= end_x; x++)
                {
                    for (int y = start_y; y <= end_y; y++)
                    {
                        Tile t = WorldController.Instance.World.GetTileAt(x, y);
                        if (t != null)
                        {
                            MakeSpriteGhost(t);
                            //Display the building hint on top of tile pos
                        }
                    }
                }
            //}

            //end drag
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                for (int x = start_x; x <= end_x; x++)
                {
                    for (int y = start_y; y <= end_y; y++)
                    {
                        canDragSelect = false;
                        Tile t = WorldController.Instance.World.GetTileAt(x, y);
                        if (t != null)
                        {
                            //call build mode controller do build
                            bmc.DoBuild(t);
                        }
                    }
                }
            }

            
        }

        if (bmc.buildMode == BuildMode.INST_OBJS && isDragging == false && Input.GetMouseButtonUp(0) == false)
        {
            MakeSpriteGhost(WorldController.Instance.World.GetTileAt(start_x, start_y));
        }
    }

    void MakeSpriteGhost(Tile t)
    {
        if (bmc.buildMode == BuildMode.INST_OBJS)
        {
            ShowInstObjSpriteAtTile(bmc.buildModeObjectType, t);
        }
        else
        {
            //show the generic dragging visuals
            GameObject go = SimplePool.Spawn(circleCursorPrefab, new Vector3(t.X, t.Y, 0), Quaternion.identity);
            go.transform.SetParent(this.transform, true);
            dragPreviewGameObjects.Add(go);
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
    void ShowInstObjSpriteAtTile(string instObjType, Tile t)
    {

        GameObject go = new GameObject();

        go.transform.SetParent(this.transform, true);
        dragPreviewGameObjects.Add(go);
        SpriteRenderer inst_sr = go.AddComponent<SpriteRenderer>();
        //InstalledObject proto = t.world.installedObjectPrototypes[instObjType];
        InstalledObject proto = World.current.GetInstObjPrototype(bmc.buildModeObjectType);
        go.transform.position = new Vector3(t.X + (proto.width - 1) / 2f, t.Y + (proto.height - 1) / 2f);
        inst_sr.sprite = iosc.GetSpriteForInstalledObject(instObjType);
        //inst_sr.sortingLayerName = "InstalledObject";
        if (WorldController.Instance.World.IsInstalledObjectPlacementValid(bmc.buildModeObjectType, t))
        {
            inst_sr.color = new Color(.5f, 1f, .5f, 0.8f);
        }
        else
        {
            inst_sr.color = new Color(1f, .5f, .5f, 0.8f);
        }

        inst_sr.sortingLayerName = "Jobs";
    }
}



