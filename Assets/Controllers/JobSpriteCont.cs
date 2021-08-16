using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class JobSpriteCont : MonoBehaviour
{
    //This bare-bones controller is monstly just going to piggyback
    // on furniture sprite controller because we don't yet fully know
    // what our job system is going to look like in the end
    Dictionary<Job, GameObject> jobGameObjectMap;
    InstObjSpriteController iosc;
    // Start is called before the first frame update
    void Start()
    {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        iosc = GameObject.FindObjectOfType<InstObjSpriteController>();
        //FIXME: no such thing
        WorldController.Instance.World.jobQueue.RegisterJobCreationCB(OnJobCreated);
    }

    // Update is called once per frame
    void OnJobCreated(Job job)
    {
        //FIXME: We can only do inst obj building jobs

        
        //created a visual gameobject linked to this data.
        GameObject job_go = new GameObject();

        //Tile tile_data = World.getTileAt(x, y);

        //add our tile-go pair to our dictionary
        jobGameObjectMap.Add(job, job_go);
        job_go.name = job.jobObjectType + job.tile.X + "_" + job.tile.Y;
        job_go.transform.position = new Vector3(job.tile.X, job.tile.Y);
        SpriteRenderer inst_sr = job_go.AddComponent<SpriteRenderer>();
        job_go.transform.SetParent(this.transform, true);

        inst_sr.sprite = iosc.GetSpriteForInstalledObject(job.jobObjectType);
        //inst_sr.sortingLayerName = "InstalledObject";
        inst_sr.color = new Color(.5f, 1f, .5f, 0.51f);
        inst_sr.sortingLayerName = "Jobs";
        //obj.RegisterOnChangedCallback(OnInstalledObjectChanged);
        job.RegisterJobCompleteCB(OnJobEnded);
        job.RegisterJobCancelledCB(OnJobEnded);
    }
    void OnJobEnded(Job job)
    {
        //FIXME: We can only do inst obj building jobs

        //TODO: Delete sprite
        GameObject job_go = jobGameObjectMap[job];
        job.UnregisterJobCompleteCB(OnJobEnded);
        job.UnregisterJobCancelledCB(OnJobEnded);

    }
}
