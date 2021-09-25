using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
public class Character : IXmlSerializable, ISelectableInterface
{
    public float X
    {
        get
        {
            return Mathf.Lerp(CurrTile.X, nextTile.X, movementPercentage);
        }
    }
    public float Y
    {
        get
        {
            return Mathf.Lerp(CurrTile.Y, nextTile.Y, movementPercentage);
        }
    }
    Tile _currTile;
    public Tile CurrTile
    {
        get {
            return _currTile;
        }
        protected set
        {
            if(value != _currTile)
            {
                if(_currTile != null)
                {
                    _currTile.CharacterLeave(this);
                }
               
                _currTile = value;

                _currTile.CharacterEnter(this);
            }
        }
    }

    Tile _destTile;
    Tile DestTile
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
        this.CurrTile = tile;
        this.DestTile = tile;
        this.nextTile = tile;

    }

    float jobSearchCooldown = 0;
    void GetNewJob()
    {
        
        //grab a new job
        //check to see if the job tile is reachable:
        myJob = World.current.jobQueue.Dequeue();
        if (myJob == null)
        {
            return;
        }

        DestTile = myJob.tile;
        myJob.RegisterJobStoppedCB(OnJobStopped);

        
        pathAStar = new Path_AStar(World.current, CurrTile, DestTile);
        if (pathAStar.Length() == 0)
        {
            Debug.LogError("Path_AStar returned no path");
            AbandonJob(myJob);
            pathAStar = null;
            DestTile = CurrTile;
        }


    }
    void Update_DoJob(float deltaTime)
    {
        jobSearchCooldown -= deltaTime;
        if (myJob == null)
        {
            if (jobSearchCooldown > 0)
            {
                return;
            }
            else
            {
                jobSearchCooldown = UnityEngine.Random.Range(.3f, .5f);
            }

            GetNewJob();
            if (myJob == null)
            {
                //there was no job on the queue for us, so just return
                DestTile = CurrTile;
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
                    

                    if(CurrTile == DestTile)
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
                        DestTile = myJob.tile;
                        return;
                    }
                }
                else
                {
                    //we have something, but job doesn't want it?
                    //Dump the looseObject

                    //TODO: Actually, walk to the nearest empty tile and dump it.

                    if(World.current.looseObjManager.PlaceLooseObj(CurrTile, looseObject) == false){
                        Debug.LogError("Character tried to dump loose object into an invalid tile, maybe there is something there?");
                        //FIXME: for the sake of continuing on, we are still going to dump reference
                        looseObject = null;
                    }
                }

                
            } else
            {
                //at this point the job still requires invenoty but we don't have it
                if(CurrTile.looseObject != null  && myJob.DesiresLooseObjType(CurrTile.looseObject) > 0)
                {
                    World.current.looseObjManager.PlaceLooseObj(this, CurrTile.looseObject, myJob.DesiresLooseObjType(CurrTile.looseObject));
                }
                else { 
                    //very simple and unoptimal set-up
                    LooseObject desiredObj = myJob.GetFirstDesiredLooseObj();
                    pathAStar = World.current.looseObjManager.GetPathToClosestLooseObject(desiredObj.objectType,
                        CurrTile, desiredObj.maxStackSize - desiredObj.stackSize);

                    if(pathAStar == null)
                    {
                        //Debug.Log("No tile contains object of type: Desired.ObjectType to satisfy job requirements");

                        AbandonJob(myJob);
                        return;
                    }
                    //then deliver the goods
                    //DestTile = supplier.tile;
                    _destTile = pathAStar.EndTile();
                    return;
                }
            }


            //walk to the job tile, then drop it off
            DestTile = myJob.tile;
            //if not, walt to a tile containing the required goods
            //   if already on such a tile, pick up the goods
            //set dest tile to be 


            return; //we can't continue until we get all mateials
        }

        //if we get here then the job has all the MATS it needs

        DestTile = myJob.tile;
        if (myJob != null && CurrTile == myJob.tile)
        //if (pathAStar != null && pathAStar.Length() == 1)
        {
            myJob.DoWork(deltaTime);
        }

        return;
    }

    public void AbandonJob(Job j)
    {
        nextTile = DestTile = CurrTile;
        pathAStar = null;
        World.current.jobQueue.Enqueue(j);
        myJob = null;
    }

    void Update_Movement(float deltaTime)
    {
        if (CurrTile == DestTile) { 
            pathAStar = null;
            return; //already where we want to be;
        }
        if (nextTile == null || nextTile == CurrTile)
        {
            //Get the next tile from the pathfinder.
            if(pathAStar == null || pathAStar.Length() == 0)
            {
                pathAStar = new Path_AStar(World.current, CurrTile, DestTile);
                if(pathAStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar returned no path");
                    //FIXME: job should be re-enqued instead
                    AbandonJob(myJob);
                    return;
                }
            }
            nextTile = pathAStar.GetNextTile();
            if(nextTile == CurrTile && pathAStar.Length() != 0)
            {
                nextTile = pathAStar.GetNextTile();
                if (nextTile == CurrTile)
                {
                    Debug.LogError("Update--Domovement - Nexttile is currtile?");
                }
            }
        }
        //total distance from point a to b
        float distToTravel = Mathf.Sqrt(Mathf.Pow(CurrTile.X - nextTile.X, 2) + Mathf.Pow(CurrTile.Y - nextTile.Y, 2));
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
        float distThisFrame = speed * deltaTime / ((CurrTile.movementCost+nextTile.movementCost)/2);
        //how much is that in terms of percentage
        float percThisFram = distThisFrame / distToTravel;
        movementPercentage += percThisFram;

        if (movementPercentage >= 1)
        {
            //reached our destination
            //Check for next tile and if there are no more tiles then we have truly reached out destingation
            CurrTile = nextTile;
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
        if (CurrTile.IsNeighbor(tile,true) == false)
        {
            Debug.LogError("Character::SetDestination: Destination tile is not actually neighbor");
        }

        DestTile = tile;
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
        writer.WriteAttributeString("X", CurrTile.X.ToString());
        writer.WriteAttributeString("Y", CurrTile.Y.ToString());
    }

    public void ReadXml(XmlReader reader)
    {

    }

    public string GetName()
    {
        throw new NotImplementedException();
    }

    public string GetDescription()
    {
        throw new NotImplementedException();
    }

    public string getHitPointString()
    {
        throw new NotImplementedException();
    }
}
