using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


// Maybe ?
public class PMOItem : MonoBehaviour
{
    [HideInInspector]
    public PushMeOutEnvController env;
    private bool collected = false;

    public virtual void OnCollect(PushMeOutAgent iAgent) {}

    void OnTriggerEnter(Collider iCollider)
    {
        if (collected)
            return;

        PushMeOutAgent pmoa = iCollider.gameObject.GetComponent<PushMeOutAgent>();
        if (!!pmoa)
        {
            //collect item
            OnCollect(pmoa);
            env.notifyItemCollected(this);
            collected = true;
        }
    }
}

