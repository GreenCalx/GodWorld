using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public enum GWItemType
{
    NONE=0,
    FOOD=1,
    WATER=2
}

// Maybe ?
public class GWItem : MonoBehaviour
{
    [Header("Manual global ref")]
    public GameObject selfPrefab;
    [Header("Manual self refs")]
    public Rigidbody rb;
    public BoxCollider selfCollider;

    [HideInInspector]
    public GWEnvController env;
    [Header("Internal")]
    public GWItemSpawner originator;
    public bool collected = false;
    public GWItemType itemType;
    public bool stashed = false;

    public bool hasBeenStashed = false;
    public GWPet lastCarrier = null;
    

    public virtual bool OnCollect(GWPet iAgent) {return true;}
    public virtual bool OnUse(GWPet iAgent) {return true;}

    public GameObject SpawnSelf()
    {
        return Instantiate(selfPrefab);
    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (collected)
            return;

        GWPet pet = iCollider.gameObject.GetComponent<GWPet>();
        if (!!pet)
        {
            //collect item
            if (OnCollect(pet))
            {
                env.notifyItemCollected(this);
                collected = true;
            }
            
        }
    }

    public void SetAsCarried(GWPet iAgent)
    {
        collected = true;
        rb.isKinematic = true;
        selfCollider.enabled = false;
        lastCarrier = iAgent;
    }

    public void SetAsDropped()
    {
        StartCoroutine(DropCo());
    }

    public void Consume()
    {
        env.notifySpawnedItemDestroyed(this);
        Destroy(this.gameObject);
    }

    IEnumerator DropCo()
    {
        selfCollider.enabled = true;
        rb.isKinematic = false;
        yield return new WaitForFixedUpdate();
        collected = false;
    }
}

