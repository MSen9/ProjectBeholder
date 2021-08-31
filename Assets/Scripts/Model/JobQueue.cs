using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class JobQueue
{

    protected List<Job> jobQueue;
    Action<Job> cbJobCreated;
    public JobQueue()
    {
        jobQueue = new List<Job>();
    }

    public void Enqueue(Job j)
    {

        if(j.jobTime < 0)
        {
            //job is completed instantly
            j.DoWork(0);
            return;
        }
        jobQueue.Add(j);

        //TODO callbacks
        if (cbJobCreated != null)
        {
            cbJobCreated(j);
        }
    }
    public int Count()
    {
        return jobQueue.Count;
    }
    public Job Dequeue(){
        if(jobQueue.Count == 0)
        {
            return null;
        }
        Job j = jobQueue[0];
        jobQueue.RemoveAt(0);
        return j;
    }

    public void Remove(Job j)
    {
        jobQueue.Remove(j);
    }
    public void ShelveJob(Job j)
    {
        //puts job pack on the queue, FIXME: do more later
        jobQueue.Add(j);
    }

    public void RegisterJobCreationCB(Action<Job> cb)
    {
        cbJobCreated += cb;
    }

    public void UnregisterJobCreationCB(Action<Job> cb)
    {
        cbJobCreated -= cb;
    }
}
