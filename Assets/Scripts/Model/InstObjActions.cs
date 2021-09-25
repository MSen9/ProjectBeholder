using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class InstObjActions 
{
    static InstObjActions _Instance;
    /*
    Script myLuaScript;
    public InstObjActions(string rawLuaCode)
    {
        //tell the lua interpreter to accept all the marked classes
        UserData.RegisterAssembly();
        _Instance = this;
        myLuaScript = new Script();
        //need to add globals to instantiate a new object of a type
        myLuaScript.Globals["LooseObject"] = typeof(LooseObject);
        myLuaScript.Globals["Job"] = typeof(Job);
        myLuaScript.Globals["World"] = typeof(World);
        myLuaScript.DoString(rawLuaCode);
       
    }

    */


    /*
    public static void CallUpdateFuncs(List<string> functionNames, InstalledObject instObj, float deltaTime)
    {
       foreach(string fn in functionNames)
        {

            object func = _Instance.myLuaScript.Globals[fn];
            if(func == null)
            {
                Debug.LogError(fn + " is not a lua update function");
            }

            DynValue result = _Instance.myLuaScript.Call(func, instObj, deltaTime);
            if(result.Type == DataType.String)
            {
                Debug.Log(result.String);
            }
        }
    }
    
    public static Enterability CallEnterabilityFuncs(List<string> functionNames, InstalledObject instObj)
    {
        Enterability overallEnterability = Enterability.Yes;
        foreach (string fn in functionNames)
        {

            object func = _Instance.myLuaScript.Globals[fn];
            if (func == null)
            {
                Debug.LogError(fn + " is not a lua enterability function");
            }

            DynValue result = _Instance.myLuaScript.Call(func, instObj);
            if (result.Type != DataType.Number)
            {
                Debug.LogError("Result for enterability was not a number");
            } else
            {
                Enterability currFeedback = (Enterability)result.Number;
                //Enterability.Yes is default and can't override soon or never so don't do anything in that case
                //Enterability.Soon overrides Enterability.Yes
                //Enterability.Never overides all others so it is returned immediately
                if (currFeedback == Enterability.Soon)
                {
                    overallEnterability = Enterability.Soon;
                } else if (currFeedback == Enterability.Never)
                {
                    overallEnterability = Enterability.Never;
                    return overallEnterability;
                }
            }

        }
        return overallEnterability;

    }

    */
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
        instObj.SetParameter("openness",Mathf.Clamp01(instObj.GetParameterFloat("openness")));
        instObj.cbOnChanged(instObj);
    }

   
    public static Enterability Door_IsEnterable(InstalledObject instObj)
    {
        instObj.SetParameter("is_opening", true);
        if(instObj.GetParameterFloat("openness") >= 1)
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
            instObj.GetJobSpotTile(), null, MiningBase_JobComplete, 1f, null,false
            ) ;
        instObj.AddJob(j);
        //j.RegisterJobStoppedCB(MiningBase_JobStopped);
        
    }

    public static void MiningBase_JobComplete(Job j)
    {
        WorldController.Instance.world.looseObjManager.PlaceLooseObj(j.tile, new LooseObject("Bars", 13, 50), true);
        j.workedInstObj.RemoveJob(j);
    }
    public static void MiningBase_JobStopped(Job j)
    {
        

    }
    
}
