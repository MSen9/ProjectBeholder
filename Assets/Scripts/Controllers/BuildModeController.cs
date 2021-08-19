using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor.Events;

public class BuildModeController : MonoBehaviour
{

    bool buildModeIsObjects = false;
    // Start is called before the first frame update

    TileType buildModeTile = TileType.Floor;
    string buildModeObjectType;
    void Start()
    {
       
    }

    // Update is called once per frame

    


    
    public void SetMode_BuildFloor()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Floor;

    }
    public void SetMode_DestroyFloor()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Empty;

    }

    public void SetMode_BuildObject(string objectType)
    {
        buildModeIsObjects = true;
        buildModeObjectType = objectType;
    }

    public void PathfindingTest()
    {
        Path_TileGraph tg = new Path_TileGraph(WorldController.Instance.World);
        return;
    }

    public void DoBuild(Tile t)
    {
        if (buildModeIsObjects == true)
        {
            //create the installedObject and asign in to the tile
            //FIXME: THis instalty build the funriture

            //can we build the installed object in the selected tile?
            //run the valid placement function
            string installedObjType = buildModeObjectType;
            if (WorldController.Instance.World.IsInstalledObjectPlacementValid(installedObjType, t) &&
                t.pendingInstObjJob == null)
            {
                Job j = new Job(t, installedObjType, (theJob) => {
                    WorldController.Instance.World.PlaceInstalledObject(installedObjType, theJob.tile);
                    t.pendingInstObjJob = null;
                });

                //add the job to the queue

                //FXIME: don't like manually and explicitely setting flags
                t.pendingInstObjJob = j;

                j.RegisterJobCancelledCB((thejob) => { thejob.tile.pendingInstObjJob = null; });
                WorldController.Instance.World.jobQueue.Enqueue(j);

                Debug.Log("Job Queue size: " + WorldController.Instance.World.jobQueue.Count());
            }




        }
        else
        {
            t.Type = buildModeTile;

        }
    }
}
