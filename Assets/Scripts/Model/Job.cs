using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Job 
{
    //This class holds info for a queued up job
    //Stuff like buidling, moving, and fighting


    public Tile tile;
    public float jobTime
    {
        get;
        protected set;
    }

    //FIXME: hard-coded parameter for furniture
    public string jobObjectType
    {
        get; protected set;
    }

    System.Object jobDataObject;

    Action<Job> cbJobComplete;
    Action<Job> cbJobCanceled;

    public Dictionary<string, LooseObject> looseObjRequirements;

    public InstalledObject instObjPrototype;

    public InstalledObject workedInstObj;

    public Job (Tile tile,string jobObjectType,Action<Job> cbJobComplete,float jobTime, LooseObject[] looseObjRequirements)
    {
        this.tile = tile;
        this.cbJobComplete = cbJobComplete;
        this.jobTime = jobTime;
        this.jobObjectType = jobObjectType;

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
        this.cbJobComplete += other.cbJobComplete;
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
    public void RegisterJobCompleteCB(Action<Job> cb)
    {
        cbJobComplete += cb;
    }
    public void RegisterJobCancelledCB(Action<Job> cb)
    {
        cbJobCanceled += cb;
    }
    public void UnregisterJobCompleteCB(Action<Job> cb)
    {
        cbJobComplete -= cb;
    }
    public void UnregisterJobCancelledCB(Action<Job> cb)
    {
        cbJobCanceled -= cb;
    }
    public void DoWork(float workTime)
    {
        jobTime -= workTime;

        if(jobTime <= 0)
        {
            if(cbJobComplete != null)
            {
                cbJobComplete(this);
            }
            
        }
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
