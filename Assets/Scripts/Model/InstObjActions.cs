using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstObjActions 
{
    public static void Door_UpdateAction(InstalledObject instObj, object dt)
    {
        float deltaTime = (float)dt;
        //Debug.Log("Door Update");
        if ((bool)instObj.GetParameter("is_opening"))
        {
            //open door by a certain amount
            instObj.ChangeParameter("openness",deltaTime);
            if((float)instObj.GetParameter("openness") >= 1)
            {
                instObj.SetParameter("is_opening",false);
            }
        } else
        {
            instObj.ChangeParameter("openness", -1 * deltaTime);

        }
        instObj.SetParameter("openness",Mathf.Clamp01((float)instObj.GetParameter("openness")));
        instObj.cbOnChanged(instObj);
    }

   
    public static Enterability Door_IsEnterable(InstalledObject instObj)
    {
        instObj.SetParameter("is_opening", true);
        if((float)instObj.GetParameter("openness") >= 1)
        {
            return Enterability.Yes;
        }

        return Enterability.Soon;
    }

    public static void JobComplete_InstalledObject(Job theJob)
    {
        WorldController.Instance.world.PlaceInstalledObject(theJob.jobObjectType, theJob.tile);

        theJob.tile.pendingInstObjJob = null;
    }

    public static void MiningBase_UpdateAction(InstalledObject instObj, object dt)
    {
        //need to insure that we have a job the queue asking for either: (if we are empty) that ANY loose object be
        //brought to us or (if have stuff) more of the same stuff is brought to it.

        if (instObj.JobCount() > 0)
        {
            //already have a job
            return;
        }

        Job j = new Job(
            instObj.GetJobSpotTile(), null, MiningBase_JobComplete, 1f, null
            ) ;
        instObj.AddJob(j);
        
    }

    public static void MiningBase_JobComplete(Job j)
    {
        WorldController.Instance.world.looseObjManager.PlaceLooseObj(j.tile, new LooseObject("Bars", 201, 50), true);
        j.workedInstObj.RemoveJob(j);

    }

}
