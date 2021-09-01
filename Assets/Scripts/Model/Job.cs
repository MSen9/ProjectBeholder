using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Job 
{
    //This class holds info for a queued up job
    //Stuff like buidling, moving, and fighting


    public Tile tile;

    float jobTimeRequired;
    public float jobTime
    {
        get;
        protected set;
    }

    protected bool jobRepeats = false;

    //FIXME: hard-coded parameter for furniture
    public string jobObjectType
    {
        get; protected set;
    }

    System.Object jobDataObject;

    Action<Job> cbJobCompleted; //finished work cycle, things should get build
    Action<Job> cbJobStopped; //Job has been stopped, either non-repeating or cancelled
    Action<Job> cbJobWorked;

    public Dictionary<string, LooseObject> looseObjRequirements;

    public InstalledObject instObjPrototype;

    public InstalledObject workedInstObj;

    public Job (Tile tile,string jobObjectType,Action<Job> cbJobCompleted,float jobTime, LooseObject[] looseObjRequirements, bool jobRepeats = false)
    {
        this.tile = tile;
        this.cbJobCompleted = cbJobCompleted;
        
        this.jobTimeRequired = this.jobTime = jobTime;

        this.jobObjectType = jobObjectType;
        this.jobRepeats = jobRepeats;
        this.looseObjRequirements = new Dictionary<string, LooseObject>();
        if(looseObjRequirements != null) { 
            foreach(LooseObject looseObj in looseObjRequirements)
            {
                this.looseObjRequirements[looseObj.objectType] = looseObj.Clone();
            }
        }

    }

    protected Job(Job other)
    {
        this.tile = other.tile;
        this.cbJobCompleted += other.cbJobCompleted;
        this.jobTime = other.jobTime;
        this.jobObjectType = other.jobObjectType;

        this.looseObjRequirements = new Dictionary<string, LooseObject>();
        if (looseObjRequirements != null)
        {
            foreach (LooseObject looseObj in other.looseObjRequirements.Values)
            {
                this.looseObjRequirements[looseObj.objectType] = looseObj.Clone();
            }
        }
    }
    public Job Clone()
    {
        return new Job(this);
    }
    public void RegisterJobCompletedCB(Action<Job> cb)
    {
        cbJobCompleted += cb;
    }
    public void UnregisterJobCompletedCB(Action<Job> cb)
    {
        cbJobCompleted -= cb;
    }
    public void RegisterJobStoppedCB(Action<Job> cb)
    {
        cbJobStopped += cb;
    }
    public void UnregisterJobStoppedCB(Action<Job> cb)
    {
        cbJobStopped -= cb;
    }
    public void RegisterJobWorkedCB(Action<Job> cb)
    {
        cbJobWorked += cb;
    }
    public void UnregisterJobWorkedCB(Action<Job> cb)
    {
        cbJobWorked -= cb;
    }
    public void DoWork(float workTime)
    {

        if(HasAllMaterials() == false)
        {
            Debug.LogError("Missing materials for current job");
            return;
        }
        jobTime -= workTime;

        if(jobTime <= 0)
        {
            if(cbJobCompleted != null)
            {
                cbJobCompleted(this);
            }
            
            if(jobRepeats == false)
            {
                if(cbJobStopped != null)
                {
                    cbJobStopped(this);
                }
            } else
            {
                //this is a repeating job
                jobTime += jobTimeRequired;
            }
        }
    }

    public void CancelJob()
    {
        if(cbJobStopped != null)
        {
            cbJobStopped(this);
        }
        World.current.jobQueue.Remove(this);
    }
    public bool HasAllMaterials()
    {
        foreach (LooseObject looseObj in looseObjRequirements.Values)
        {
            if (looseObj.maxStackSize > looseObj.stackSize)
            {
                return false;
            }
            
        }
        return true;
    }

    public int DesiresLooseObjType(LooseObject looseObj)
    {
        if (looseObjRequirements.ContainsKey(looseObj.objectType) == false)
        {
            return 0;
        }

        if(looseObjRequirements[looseObj.objectType].stackSize >= looseObjRequirements[looseObj.objectType].maxStackSize)
        {
            //we already have all that we need!
            return 0;
        }

        return looseObjRequirements[looseObj.objectType].maxStackSize - looseObjRequirements[looseObj.objectType].stackSize;
    }

    public LooseObject GetFirstDesiredLooseObj()
    {
        foreach(LooseObject looseObj in looseObjRequirements.Values)
        {
            if(looseObj.maxStackSize > looseObj.stackSize)
            {
                return looseObj;
            }
        }


        return null;
    }

}
