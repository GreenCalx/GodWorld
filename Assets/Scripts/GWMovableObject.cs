using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GWMovableObject : GWTerrain
{
    public GWAgent pushingAgent;
    private Vector3 lastPosition;
    public float minDistToRecordMoved = 1f;

    void FixedUpdate()
    {
        if(pushingAgent!=null)
        {
            if (Vector3.Distance(lastPosition, transform.position) > minDistToRecordMoved)
                MovedByAgent(pushingAgent);
        }
    }
    void OnCollisionStay(Collision iCollision)
    {
        GWAgent a = iCollision.gameObject.GetComponent<GWAgent>();
        if (a!=null)
        {
            pushingAgent = iCollision.gameObject.GetComponent<GWAgent>();
        }
    }

    void OnCollisionExit(Collision iCollision)
    {
        GWAgent a = iCollision.gameObject.GetComponent<GWAgent>();
        if (a == pushingAgent)
        {
            pushingAgent = null;
        }
    }

    public void MovedByAgent(GWAgent iAgent)
    {
        lastPosition = transform.position;
        //iAgent.envController.ResolveEvent(GWEvent.PushingObject, iAgent);
    }

}