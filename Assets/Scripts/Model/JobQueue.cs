using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class JobQueue
{

    protected Queue<Job> jobQueue;
    Action<Job> cbJobCreated;
    public JobQueue()
    {
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        jobQueue.Enqueue(j);

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
        return jobQueue.Dequeue();
    }
    public void ShelveJob(Job j)
    {
        //puts job pack on the queue, FIXME: do more later
        jobQueue.Enqueue(j);
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
