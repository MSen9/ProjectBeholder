using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class LooseObject: ISelectableInterface
{
    public string objectType;
    public int maxStackSize = 0;
    protected int _stackSize = 1;
    public int stackSize
    {
        get { return _stackSize; }
        set
        {
            if(_stackSize != value)
            {
                _stackSize = value;
                if(tile != null && cbLooseObjectChanged != null)
                {
                    cbLooseObjectChanged(this);
                }
            }
        }
    }
    Action<LooseObject> cbLooseObjectChanged;




    public Tile tile;
    public Character character;

    public LooseObject()
    {

    }


    public LooseObject(string objectType, int stackSize, int maxStackSize)
    {
        this.objectType = objectType;
        this.maxStackSize = maxStackSize;
        this.stackSize = stackSize;
    }
    protected LooseObject(LooseObject other)
    {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        stackSize = other.stackSize;
    }

    public virtual LooseObject Clone()
    {
        return new LooseObject(this);
    }

    public void RegisterLooseObjChanged(Action<LooseObject> callbackFunc)
    {
        cbLooseObjectChanged += callbackFunc;
    }

    public void UnregisterLooseObjChanged(Action<LooseObject> callbackFunc)
    {
        cbLooseObjectChanged -= callbackFunc;
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
