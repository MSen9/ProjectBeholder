using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
public class Character : IXmlSerializable
{
    public float X
    {
        get
        {
            return Mathf.Lerp(currTile.X, nextTile.X, movementPercentage);
        }
    }
    public float Y
    {
        get
        {
            return Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage);
        }
    }
    public Tile currTile
    {
        get; protected set;
    }
    Tile destTile; // if we aren't moving then destTile = currTile
    Tile nextTile;
    Path_AStar pathAStar;
    float movementPercentage; // goes from 0 to 1 as we move from curr tile to dest tile
    float speed = 2.5f; //tiles per second

    Job myJob;

    Action<Character> cbCharacterChanged;

    public Character()
    {

    }
    public Character(Tile tile)
    {
        this.currTile = tile;
        this.destTile = tile;
        this.nextTile = tile;

    }
    void Update_DoJob(float deltaTime)
    {
        if (myJob == null)
        {
            //grab a new job
            myJob = currTile.world.jobQueue.Dequeue();
            if (myJob != null)
            {
                //we have a job
                destTile = myJob.tile;
                myJob.RegisterJobCancelledCB(OnJobEnded);
                myJob.RegisterJobCompleteCB(OnJobEnded);
            }
        }

        if (currTile == destTile)
        //if (pathAStar != null && pathAStar.Length() == 1)
        {
            
            if (myJob != null)
            {
                myJob.DoWork(deltaTime);
            }
            return;
        }

        return;
    }

    public void AbandonJob(Job j)
    {
        nextTile = destTile = currTile;
        pathAStar = null;
        currTile.world.jobQueue.Enqueue(j);
        myJob = null;
    }

    void Update_Movement(float deltaTime)
    {
        if (currTile == destTile) { 
            pathAStar = null;
            return; //already where we want to be;
        }
        if (nextTile == null || nextTile == currTile)
        {
            //Get the next tile from the pathfinder.
            if(pathAStar == null || pathAStar.Length() == 0)
            {
                pathAStar = new Path_AStar(currTile.world, currTile, destTile);
                if(pathAStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar returned no path");
                    //FIXME: job should be re-enqued instead
                    AbandonJob(myJob);
                    return;
                }
            }
            nextTile = pathAStar.GetNextTile();
            if(nextTile == currTile)
            {
                Debug.LogError("Update--Domovement - Nexttile is currtile?");
            }
        }
        //total distance from point a to b
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - nextTile.X, 2) + Mathf.Pow(currTile.Y - nextTile.Y, 2));
        //distance we can travel this updat
        float distThisFrame = speed * deltaTime;
        //how much is that in terms of percentage
        float percThisFram = distThisFrame / distToTravel;
        movementPercentage += percThisFram;

        if (movementPercentage >= 1)
        {
            //reached our destination
            //Check for next tile and if there are no more tiles then we have truly reached out destingation
            currTile = nextTile;
            movementPercentage = 0;

            //FIXME? Do we want to retain any overshot movement?
        }
        if (cbCharacterChanged != null)
        {
            cbCharacterChanged(this);
        }
        return;
    }
    public void Update(float deltaTime)
    {


        Update_DoJob(deltaTime);

        Update_Movement(deltaTime);


        return;


        
    }
    public void SetDesination(Tile tile)
    {
        if (currTile.IsNeighbor(tile,true) == false)
        {
            Debug.LogError("Character::SetDestination: Destination tile is not actually neighbor");
        }

        destTile = tile;
    }
   
    public void RegisterOnChangedCB(Action<Character> cb)
    {
        cbCharacterChanged += cb;
    }

    public void UnregisterPnChangedCB(Action<Character> cb)
    {
        cbCharacterChanged -= cb;
    }

    void OnJobEnded(Job j)
    {
        //job completed or was cancelled
        if(j != myJob)
        {
            Debug.LogError("Character being told about job that wasn't his, forgot to register something");
        }

        myJob = null;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        //writer.WriteAttributeString("Type", Type.ToString());
        writer.WriteAttributeString("X", currTile.X.ToString());
        writer.WriteAttributeString("Y", currTile.Y.ToString());
    }

    public void ReadXml(XmlReader reader)
    {

    }
}
