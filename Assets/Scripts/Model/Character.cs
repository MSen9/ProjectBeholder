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

    Tile _destTile;
    Tile destTile
    {
        get
        {
            return _destTile;
        }
        set
        {
            if(_destTile != value)
            {
                _destTile = value;
                pathAStar = null; //reset our pathfinding for a new destination
            }
        }
    }// if we aren't moving then destTile = currTile
    Tile nextTile;
    Path_AStar pathAStar;
    public LooseObject looseObject;
       
    float movementPercentage; // goes from 0 to 1 as we move from curr tile to dest tile
    float speed = 8f; //tiles per second

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

    void GetNewJob()
    {
        //grab a new job
        //check to see if the job tile is reachable:
        myJob = World.current.jobQueue.Dequeue();
        if (myJob == null)
        {
            return;
        }

        destTile = myJob.tile;
        myJob.RegisterJobStoppedCB(OnJobStopped);

        
        pathAStar = new Path_AStar(World.current, currTile, destTile);
        if (pathAStar.Length() == 0)
        {
            Debug.LogError("Path_AStar returned no path");
            AbandonJob(myJob);
            pathAStar = null;
            destTile = currTile;
        }


    }
    void Update_DoJob(float deltaTime)
    {
        if (myJob == null)
        {

            GetNewJob();
            if (myJob == null)
            {
                //there was no job on the queue for us, so just return
                destTile = currTile;
                return;
            } 
        }

        //we have a job and the tile is reachable
        //Step 1: deos the job have all the materials it needs????


        if (myJob.HasAllMaterials() == false)
        {
            //step 2: are we CARRYING anything the job needs
            if(looseObject != null)
            {
                if (myJob.DesiresLooseObjType(looseObject) > 0)
                {
                    

                    if(currTile == destTile)
                    {
                        //we are at the job site, so deliver the goods;
                        World.current.looseObjManager.PlaceLooseObj(myJob, looseObject);
                        //myJob.looseObjRequirements[looseObject.objectType]

                        if(looseObject.stackSize == 0)
                        {
                            looseObject = null;
                        }
                        else{
                            Debug.LogError("Character is still carrying inventory (HOW!?)");
                            looseObject = null;
                        }
                    } else
                    {
                        //need to walk to job site
                        destTile = myJob.tile;
                        return;
                    }
                }
                else
                {
                    //we have something, but job doesn't want it?
                    //Dump the looseObject

                    //TODO: Actually, walk to the nearest empty tile and dump it.

                    if(World.current.looseObjManager.PlaceLooseObj(currTile, looseObject) == false){
                        Debug.LogError("Character tried to dump loose object into an invalid tile, maybe there is something there?");
                        //FIXME: for the sake of continuing on, we are still going to dump reference
                        looseObject = null;
                    }
                }

                
            } else
            {
                //at this point the job still requires invenoty but we don't have it
                if(currTile.looseObject != null  && myJob.DesiresLooseObjType(currTile.looseObject) > 0)
                {
                    World.current.looseObjManager.PlaceLooseObj(this, currTile.looseObject, myJob.DesiresLooseObjType(currTile.looseObject));
                }
                else { 
                    //very simple and unoptimal set-up
                    LooseObject desiredObj = myJob.GetFirstDesiredLooseObj();
                    LooseObject supplier = World.current.looseObjManager.GetClosestLooseObjectOfType(desiredObj.objectType,
                        currTile, desiredObj.maxStackSize - desiredObj.stackSize);

                    if(supplier == null)
                    {
                        Debug.Log("No tile contains object of type: Desired.ObjectType to satisfy job requirements");

                        AbandonJob(myJob);
                        return;
                    }
                    //then deliver the goods
                    destTile = supplier.tile;
                    return;
                }
            }


            //walk to the job tile, then drop it off
            destTile = myJob.tile;
            //if not, walt to a tile containing the required goods
            //   if already on such a tile, pick up the goods
            //set dest tile to be 


            return; //we can't continue until we get all mateials
        }

        //if we get here then the job has all the MATS it needs

        destTile = myJob.tile;
        if (myJob != null && currTile == myJob.tile)
        //if (pathAStar != null && pathAStar.Length() == 1)
        {
            myJob.DoWork(deltaTime);
        }

        return;
    }

    public void AbandonJob(Job j)
    {
        nextTile = destTile = currTile;
        pathAStar = null;
        World.current.jobQueue.Enqueue(j);
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
                pathAStar = new Path_AStar(World.current, currTile, destTile);
                if(pathAStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar returned no path");
                    //FIXME: job should be re-enqued instead
                    AbandonJob(myJob);
                    return;
                }
            }
            nextTile = pathAStar.GetNextTile();
            if(nextTile == currTile && pathAStar.Length() != 0)
            {
                nextTile = pathAStar.GetNextTile();
                if (nextTile == currTile)
                {
                    Debug.LogError("Update--Domovement - Nexttile is currtile?");
                }
            }
        }
        //total distance from point a to b
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - nextTile.X, 2) + Mathf.Pow(currTile.Y - nextTile.Y, 2));
        //distance we can travel this updat
        if (nextTile.TryEnter() == Enterability.Never)
        {
            Debug.LogError("not possible to move through next tile");
            nextTile = null;
            pathAStar = null;
            return;
        } else if (nextTile.TryEnter() == Enterability.Soon)
        {
            //tile we're trying to enter is walkable, but could it be  a door?
            //like a door, can be entered soon, don't bail on movement/path.

            return;
        }
        float distThisFrame = speed * deltaTime / ((currTile.movementCost+nextTile.movementCost)/2);
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

    void OnJobStopped(Job j)
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
