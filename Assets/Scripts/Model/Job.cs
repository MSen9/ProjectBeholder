using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Job 
{
    //This class holds info for a queued up job
    //Stuff like buidling, moving, and fighting


    public Tile tile { get; protected set; }
    float jobTime;

    //FIXME: hard-coded parameter for furniture
    public string jobObjectType
    {
        get; protected set;
    }

    System.Object jobDataObject;

    Action<Job> cbJobComplete;
    Action<Job> cbJobCanceled;

    

    public Job (Tile tile,string jobObjectType,Action<Job> cbJobComplete,float jobTime = .09f)
    {
        this.tile = tile;
        this.cbJobComplete = cbJobComplete;
        this.jobTime = jobTime;
        this.jobObjectType = jobObjectType;
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
    
}
