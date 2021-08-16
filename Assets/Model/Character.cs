using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public float X
    {
        get
        {
            return Mathf.Lerp(currTile.X, destTile.X, movementPercentage);
        }
    }
    public float Y
    {
        get
        {
            return Mathf.Lerp(currTile.Y, destTile.Y, movementPercentage);
        }
    }
    public Tile currTile
    {
        get; protected set;
    }
    Tile destTile; // if we aren't moving then destTile = currTile
    float movementPercentage; // goes from 0 to 1 as we move from curr tile to dest tile
    float speed = 2.5f; //tiles per second

    Job myJob;

    Action<Character> cbCharacterChanged;
    public Character(Tile tile)
    {
        this.currTile = tile;
        this.destTile = tile;


    }

    public void Update(float deltaTime)
    {


        if (myJob == null)
        {
            myJob = currTile.world.jobQueue.Dequeue();
            if (myJob != null)
            {
                destTile = myJob.tile;
            }
        }
        /*
        if (currTile == destTile)
        {
            return;
        }
        */
        //total distance from point a to b
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - destTile.X, 2) + Mathf.Pow(currTile.Y - destTile.Y, 2));
        //distance we can travel this updat
        float distThisFrame = speed * deltaTime;
        //how much is that in terms of percentage
        float percThisFram = distThisFrame / distToTravel;
        movementPercentage += percThisFram;

        if(movementPercentage >= 1){
            //reached our destination
            currTile = destTile;
            movementPercentage = 0;

            //FIXME? Do we want to retain any overshot movement?
        }

        


        if(cbCharacterChanged != null)
        {
            cbCharacterChanged(this);
        }
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
}
