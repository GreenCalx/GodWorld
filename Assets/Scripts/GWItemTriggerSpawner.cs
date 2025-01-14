using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GWItemTriggerSpawner : MonoBehaviour
{
    public GWItem prefab_spawnableItem;
    public GWPositionRandomizer positionRandomizer;

    private GWPet petInTrigger = null;

    // Update is called once per frame
    void OnTriggerStay(Collider iCollider)
    {
        if (petInTrigger!=null)
            return;

        GWPet pet = iCollider.gameObject.GetComponent<GWPet>();
        if (pet!=null)
        {
            petInTrigger = pet;
        }
    }

    void OnTriggerExit(Collider iCollider)
    {
        if (petInTrigger==null)
            return;
        GWPet pet = iCollider.gameObject.GetComponent<GWPet>();
        if ((pet!=null)&&(pet==petInTrigger))
        {
            petInTrigger = null;
        }
    }
}
