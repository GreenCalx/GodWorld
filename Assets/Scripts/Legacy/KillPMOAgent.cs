using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPMOAgent : MonoBehaviour
{
    public PushMeOutEnvController ctrl;

    // Start is called before the first frame update
    void OnTriggerEnter(Collider iCollider)
    {
        PushMeOutAgent pmo = iCollider.gameObject.GetComponent<PushMeOutAgent>();
        if (!!pmo)
        {
            ctrl.OnAgentDeath(pmo);
        }
    }
}
