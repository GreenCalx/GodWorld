using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillGWAgent : MonoBehaviour
{
    public GWEnvController ctrl;

    // Start is called before the first frame update
    void OnTriggerEnter(Collider iCollider)
    {
        GWAgent gwa = iCollider.gameObject.GetComponent<GWAgent>();
        if (!!gwa)
        {
            ctrl.OnAgentDeath(gwa);
        }
    }
}
