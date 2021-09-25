using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

public class InstalledObject : IXmlSerializable, ISelectableInterface
{
    protected Dictionary<string, object> instObjParameters;
    protected Action<InstalledObject,object> updateActions;
    protected Func<InstalledObject, Enterability> isEnterableActions;
   

    public int width;
    public int height;

    public Action<InstalledObject> cbOnChanged;
    public Action<InstalledObject> cbOnRemoved;

    public Func<Tile, bool> funcPositionValidation;

    public Vector2 jobSpotOffset = Vector2.zero;
    List<Job> jobs;
    //This represents the BASE tile of object, but in practice, large objects may actually require multiple times
    public Tile tile;
    //This "objecttype" will be queries by the visual system 
    private string _Name = null;
    public string Name
    {
        get
        {
            if(_Name == null || _Name == "")
            {
                return objectType;
            }
            return _Name;
        }
        set
        {
            _Name = value;
        }
    }
    public string objectType
    {
        get; protected set;
    }

    public bool linksToNeighbor
    {
        get; protected set;
    }

    //used for walking over beds and such
    //SPECIAL: if movement cost is 0, then this time is impasssible (wall)
    public float movementCost
    {
        get; protected set;

    }

    public bool roomEnclosure;

    //public Job constructionJobPrototype;
    public InstalledObject()
    {
        funcPositionValidation = __IsValidPosition;
        instObjParameters = new Dictionary<string, object>();
        jobs = new List<Job>();
        linksToNeighbor = false;
        roomEnclosure = false;
        width = 1;
        height = 1;
    }

    virtual public InstalledObject Clone()
    {
        return new InstalledObject(this);
    }
    public InstalledObject(InstalledObject proto)
    {
        this.objectType = proto.objectType;
        this.Name = proto.Name;
        this.movementCost = proto.movementCost;
        this.roomEnclosure = proto.roomEnclosure;
        this.width = proto.width;
        this.height = proto.height;
        this.linksToNeighbor = proto.linksToNeighbor;

        this.instObjParameters = new Dictionary<string, object>(proto.instObjParameters);
        if (proto.updateActions != null)
        {
            this.updateActions = proto.updateActions;
        }
        if (proto.isEnterableActions != null)
        {
            this.isEnterableActions = proto.isEnterableActions;
        }
        if (proto.funcPositionValidation != null)
        {
            this.funcPositionValidation = (Func<Tile, bool>)proto.funcPositionValidation.Clone();
        }
        this.jobs = new List<Job>(proto.jobs);
        this.jobSpotOffset = proto.jobSpotOffset;
    }
    //used by object factory to create the proto-typical object
    public InstalledObject(string objectType, float movementCost, int width = 1, int height = 1,
        bool linksToNeighbor = false, bool roomEnclosure = false)
    {
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.roomEnclosure = roomEnclosure;
        this.width = width;
        this.height = height;
        this.linksToNeighbor = linksToNeighbor;
        this.funcPositionValidation = __IsValidPosition;
        this.instObjParameters = new Dictionary<string, object>();
        this.jobs = new List<Job>();
    }
    public void Update(float deltaTime)
    {

        if(updateActions != null)
        {
            
            updateActions(this, deltaTime);
            
            //InstObjActions.CallUpdateFuncs(updateActions, this,deltaTime);
        }
    }
    public Enterability IsEnterable()
    {
        if(isEnterableActions == null)
        {
            return Enterability.Yes;
        }
        //return InstObjActions.CallEnterabilityFuncs(isEnterableActions, this);
        return isEnterableActions(this);
    }

    public object GetParameter(string key, object defaultval = null)
    {
        if(instObjParameters.ContainsKey(key) == false)
        {
            return defaultval;
        }
        return instObjParameters[key];
    }

    public bool GetParameterBool(string key)
    {
        return Convert.ToBoolean(GetParameter(key));
    }
    public float GetParameterFloat(string key)
    {
        return Convert.ToSingle(GetParameter(key));
    }
    public int GetParameterInt(string key)
    {
        return Convert.ToInt32(GetParameter(key));
    }

    public void SetParameter(string key, object val)
    {
        if (instObjParameters.ContainsKey(key))
        {
            instObjParameters[key] = val;
        } else
        {
            
        }
       
    }

    public void ChangeParameter(string key, object val)
    {
        if (instObjParameters.ContainsKey(key) == false)
        {
            instObjParameters[key] = val;
            return;
        }

        Type type = instObjParameters[key].GetType();
        if (type.Equals(typeof(float)))
        {
            instObjParameters[key] = (float)instObjParameters[key] + Convert.ToSingle(val);
        }
        else if (type.Equals(typeof(double)))
        {
            instObjParameters[key] = (double)instObjParameters[key] + Convert.ToDouble(val);
        }
        else if (type.Equals(typeof(string)))
        {
            instObjParameters[key] = (string)instObjParameters[key] + Convert.ToString(val);
        }else if (type.Equals(typeof(int)))
        {
            instObjParameters[key] = (int)instObjParameters[key] + Convert.ToInt32(val);
        }
    }

    public void RegisterUpdateAction(Action<InstalledObject,object> cb)
    {
        updateActions += cb;
    }

    public void UnregisterUpdateAction(Action<InstalledObject, object> cb)
    {
        updateActions -= cb;
    }

    public void RegisterIsEnterableAction(Func<InstalledObject,Enterability> cb)
    {
        isEnterableActions += cb;
    }

    public void UnregisterIsEnterableAction(Func<InstalledObject, Enterability> cb)
    {
        isEnterableActions += cb;
    }

    public void RegisterRemovedCB(Action<InstalledObject> a)
    {
        cbOnRemoved += a;
    }

    public void UnregisterRemovedCB(Action<InstalledObject> a)
    {
        cbOnRemoved -= a;
    }

    //this is a multiplier. So a value of 2 here means you move twice as slowly
    
    //TODO: Implement larger objects
    //TODO: Implement object rotation

    

    //empty constructor is used for serialization
    
    //copy constructor

    

    static void UpdateNeighbors(Tile tile, string objectType)
    {
        int x = tile.X;
        int y = tile.Y;
        Tile t = World.current.GetTileAt(x, y + 1);

        if (t != null && t.installedObject != null && t.installedObject.cbOnChanged != null && t.installedObject.objectType == objectType)
        {
            //we have a northern neighbor with the same object type as us so tell it that it has changed
            t.installedObject.cbOnChanged(t.installedObject);
        }
        t = World.current.GetTileAt(x + 1, y);
        if (t != null && t.installedObject != null && t.installedObject.cbOnChanged != null && t.installedObject.objectType == objectType)
        {
            t.installedObject.cbOnChanged(t.installedObject);
        }
        t = World.current.GetTileAt(x, y - 1);
        if (t != null && t.installedObject != null && t.installedObject.cbOnChanged != null && t.installedObject.objectType == objectType)
        {
            t.installedObject.cbOnChanged(t.installedObject);
        }
        t = World.current.GetTileAt(x - 1, y);
        if (t != null && t.installedObject != null && t.installedObject.cbOnChanged != null && t.installedObject.objectType == objectType)
        {
            t.installedObject.cbOnChanged(t.installedObject);
        }
    }
    static public InstalledObject PlaceObject(InstalledObject proto, Tile tile)
    {
        if(proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position validity function returned false");
            return null;
        }

        //we know our placement destination is valid;

        InstalledObject obj = proto.Clone();


        obj.tile = tile;
        if (tile.PlaceInstalledObject(obj) == false) {
            //for some reason, we weren't able to place our object inthis tile
            //probably it was already occupied

            //do not return ur newly instantiated object, it will be garbage collected
            return null;
        }

        if (obj.linksToNeighbor)
        {
            UpdateNeighbors(tile, obj.objectType);
        }
        return obj;
    }

    public void RegisterOnChangedCallback(Action<InstalledObject> cbInst)
    {
        cbOnChanged += cbInst;
    }

    public void UnregisterOnChangedCallback(Action<InstalledObject> cbInst)
    {
        cbOnChanged -= cbInst;
    }

    protected bool __IsValidPosition(Tile t)
    {

        for (int x_off = t.X; x_off < t.X + width; x_off++)
        {
            for (int y_off = t.Y; y_off < t.Y + height; y_off++)
            {
                Tile t2 = World.current.GetTileAt(x_off, y_off);
                if (t2 != null && t2.TileType != TileType.Floor)
                {
                    return false;
                }

                if (t2.installedObject != null)
                {

                    return false;
                }
            }
        }
        //check for a floor, check if it already has furniture
        
        return true;
    }
    public bool isValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        /*
        writer.WriteAttributeString("Type", Type.ToString());
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        */
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("objectType", objectType);
        writer.WriteAttributeString("movementCost", movementCost.ToString());

        foreach(string k in instObjParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("type", instObjParameters[k].GetType().ToString());
            writer.WriteAttributeString("value", instObjParameters[k].ToString());
            writer.WriteEndElement();

        }
    }
    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        //string functionName = "";
        objectType = reader_parent.GetAttribute("objectType");

        XmlReader reader = reader_parent.ReadSubtree();
        while (reader.Read())
        {


            switch (reader.Name)
            {
                case "Name":
                    reader.Read();
                    Name = reader.ReadContentAsString();
                    break;
                case "MovementCost":
                    reader.Read();
                    movementCost = reader.ReadContentAsFloat();
                    break;
                case "Width":
                    reader.Read();
                    width = reader.ReadContentAsInt();
                    break;
                case "Height":
                    reader.Read();
                    height = reader.ReadContentAsInt();
                    break;
                case "LinksToNeighbor":
                    reader.Read();
                    linksToNeighbor = reader.ReadContentAsBoolean();
                    break;
                case "EnclosesRoom":
                    reader.Read();
                    linksToNeighbor = reader.ReadContentAsBoolean();
                    break;
                case "JobSpotOffset":
                    reader.Read();
                    string[] coords = reader.ReadContentAsString().Split(',');
                    jobSpotOffset.x = Convert.ToInt32(coords[0]);
                    jobSpotOffset.y = Convert.ToInt32(coords[1]);
                    break;
                case "Params":
                    //reader.Read();
                    ReadXmlParams(reader);
                    break;
                /*
           case "OnUpdate":
               //reader.Read();
               functionName = reader.GetAttribute("FunctionName");
               RegisterUpdateAction(functionName);
               break;

           case "IsEnterable":
               //reader.Read();
               functionName = reader.GetAttribute("FunctionName");
               RegisterIsEnterableAction(functionName);
               break;
               */
                case "BuildingJob":
                    float jobTime = float.Parse(reader.GetAttribute("jobTime"));

                    List<LooseObject> looseObjs = new List<LooseObject>();

                    XmlReader lObjs_Reader = reader.ReadSubtree();
                    while (lObjs_Reader.Read())
                    {
                        if(lObjs_Reader.Name == "LooseObject")
                        {
                            //Found an inv requiremnt, so adding it
                            LooseObject looseObj = new LooseObject(
                                lObjs_Reader.GetAttribute("objectType"),
                                0,
                                int.Parse(lObjs_Reader.GetAttribute("amount"))
                            );
                            looseObjs.Add(looseObj);
                        }

                      
                    }
                    Job j = new Job(null,objectType, InstObjActions.JobComplete_InstalledObject, jobTime,looseObjs.ToArray());
                    World.current.SetInstObjJobPrototype(j, this);
                    break;
            }
        }
         //Read in the param tag
    }

    void GetLinksToSprites(int startX, int startY, int pixelsPerUnit, string baseName)
    {

    }
    public void ReadXml(XmlReader reader)
    {
        //Object type have already been set as well as tile so just read extra data
        //objectType = reader.GetAttribute("objectType");
        movementCost = float.Parse(reader.GetAttribute("movementCost"));

        ReadXmlParams(reader);
    } 
    public void ReadXmlParams(XmlReader reader)
    {
        //Object type have already been set as well as tile so just read extra data
        //objectType = reader.GetAttribute("objectType");

        //Type = (TileType)Enum.Parse(typeof(TileType), reader.GetAttribute("Type"));
        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = reader.GetAttribute("name");
                object v = null; 
                switch (reader.GetAttribute("type"))
                {
                    case "System.Single":
                        v = float.Parse(reader.GetAttribute("value"));
                        break;
                    case "System.Boolean":
                        v = bool.Parse(reader.GetAttribute("value"));
                        break;
                    case "System.Int32":
                        v = int.Parse(reader.GetAttribute("value"));
                        break;
                    case "System.String":
                        v = reader.GetAttribute("value").ToString();
                        break;
                }
                if(v != null)
                {
                    instObjParameters.Add(k,v);
                } else
                {

                    Debug.LogError("No type loaded for: " + k);
                }
                
            } while (reader.ReadToNextSibling("Param"));
        }
    }

    public void Deconstruct()
    {

       

        tile.RemoveInstObj();

        if (linksToNeighbor)
        {
            UpdateNeighbors(tile, this.objectType);
        }

        if (cbOnRemoved != null)
        {
            cbOnRemoved(this);
        }

        if (roomEnclosure)
        {
            Room.DoRoomFloodFill(tile);
        }
    }

    public Tile GetJobSpotTile()
    {
        return World.current.GetTileAt(tile.X + (int)jobSpotOffset.x, tile.Y + (int)jobSpotOffset.y);
    }


    public int JobCount()
    {
        return jobs.Count;
    }
    public void AddJob(Job j)
    {
        if(j == null)
        {
            return;
        }
        
        jobs.Add(j);
        j.workedInstObj = this;
        World.current.jobQueue.Enqueue(j);
        j.RegisterJobCompletedCB(RemoveJob);
        j.RegisterJobStoppedCB(RemoveJob);
    }

    public void RemoveJob(Job j)
    {
        jobs.Remove(j);
        j.workedInstObj = null;
        //FIXME: Canel the job
        World.current.jobQueue.Remove(j);
    }

    public void ClearJobs()
    {
        
        jobs.Clear();

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
