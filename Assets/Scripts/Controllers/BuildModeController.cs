using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor.Events;


public enum BuildMode
{
    FLOOR,
    INST_OBJS,
    DECONSTRUCT

}
public class BuildModeController : MonoBehaviour
{
    public BuildMode buildMode = BuildMode.FLOOR;
    // Start is called before the first frame update
    TileType buildModeTile = TileType.Floor;
    public string buildModeObjectType;
    MouseController mouseController;

    Tile lastTile;
    Tile currTile;
    void Start()
    {
        mouseController = GameObject.FindObjectOfType<MouseController>();
        
    }

    // Update is called once per frame

    /*
    void Update()
    {
        if (buildModeIsObjects && buildModeObjectType != null && buildModeObjectType != "")
        {
            //Show a transparent preview of the object that is Color-coded on whether you can place it there or not;
            currTile = mouseController.GetMouseOverTile();
            if (currTile != null) { 
                ShowInstObjSpriteAtCoord(buildModeObjectType, currTile);
            }
        } else
        {
            instObjPreview.SetActive(false);
        }
    }
    */
    public bool IsInstObjDraggable()
    {
        if (buildMode== BuildMode.FLOOR || buildMode == BuildMode.DECONSTRUCT)
        {
            return true;
        }
        InstalledObject proto = WorldController.Instance.World.installedObjectPrototypes[buildModeObjectType];
        return proto.linksToNeighbor;

        
    }


    public void SetMode_BuildFloor()
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = TileType.Floor;
        mouseController.SetMode_Build();
    }
    public void SetMode_DestroyFloor()
    {
        buildMode = BuildMode.FLOOR;
        buildModeTile = TileType.Empty;
        mouseController.SetMode_Build();
    }

    public void SetMode_BuildObject(string objectType)
    {
        buildMode = BuildMode.INST_OBJS;
        buildModeObjectType = objectType;
        mouseController.SetMode_Build();
    }

    public void SetMode_Deconstruct()
    {
        buildMode = BuildMode.DECONSTRUCT;
        mouseController.SetMode_Build();
    }

    public void PathfindingTest()
    {
        Path_TileGraph tg = new Path_TileGraph(WorldController.Instance.World);
        return;
    }

    public void DoBuild(Tile t)
    {
        if (buildMode == BuildMode.INST_OBJS)
        {
            //create the installedObject and asign in to the tile
            //FIXME: THis instalty build the funriture

            //can we build the installed object in the selected tile?
            //run the valid placement function
            string installedObjType = buildModeObjectType;
            if (WorldController.Instance.World.IsInstalledObjectPlacementValid(installedObjType, t) &&
                t.pendingInstObjJob == null)
            {
                Job j;

                if (WorldController.Instance.world.instObjJobPrototype.ContainsKey(installedObjType))
                {

                    //make a clone of the job prototype
                    j = WorldController.Instance.world.instObjJobPrototype[installedObjType].Clone();
                    //assign the correct tile
                    j.tile = t;
                } else
                {
                    //Debug.LogError("There is no inst obj job prototype for: " + installedObjType);
                    j = new Job(t, installedObjType, InstObjActions.JobComplete_InstalledObject, 0.1f, null);
                }
                //add the job to the queue
                j.instObjPrototype = WorldController.Instance.world.installedObjectPrototypes[installedObjType];
                //FXIME: don't like manually and explicitely setting flags
                t.pendingInstObjJob = j;

                j.RegisterJobCancelledCB((thejob) => { thejob.tile.pendingInstObjJob = null; });
                WorldController.Instance.World.jobQueue.Enqueue(j);

                Debug.Log("Job Queue size: " + WorldController.Instance.World.jobQueue.Count());
            }




        }
        else if(buildMode == BuildMode.FLOOR)
        {
            t.tileType = buildModeTile;

        } else if(buildMode == BuildMode.DECONSTRUCT)
        {
            if(t.installedObject != null)
            {
                t.installedObject.Deconstruct();
            }

            //TODO: Destroy
        } 
    }

    
}
