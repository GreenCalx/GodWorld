using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GWTrainingFacility : GWInteractiveZone
{
    public GameObject PS_prefabTrainingEffect;
    public GWPetStats.STATS relatedStat;
    

    [Header("Internals")]
    private GWPet petInTrigger = null;
    private GameObject PS_instTrainingEffect;
    public Coroutine trainCo;

    void Update()
    {
        if ((petInTrigger!=null)&&(trainCo!=null))
        {
            if (!petInTrigger.IsAlive())
            {
                StopCoroutine(trainCo);
                trainCo = null;
                if (PS_instTrainingEffect!=null)
                {
                    Destroy(PS_instTrainingEffect.gameObject);
                }
                petInTrigger = null;
            }
            
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (petInTrigger!=null)
            return;

        GWPet pet = iCollider.gameObject.GetComponent<GWPet>();
        if (pet!=null)
        {
            petInTrigger = pet;
            petInTrigger.petStatus.interactibleZone = this;
        }
    }

    void OnTriggerExit(Collider iCollider)
    {
        if (petInTrigger==null)
            return;
        GWPet pet = iCollider.gameObject.GetComponent<GWPet>();
        if ((pet!=null)&&(pet==petInTrigger))
        {
            
            if (petInTrigger.petStatus.interactibleZone==this)
                petInTrigger.petStatus.interactibleZone = null;
            petInTrigger = null;
        }
    }

    public override bool interact()
    {
        return train();
    }
    public override int GetZoneType()
    {
        return (int)relatedStat;
    }

    public bool train()
    {
        if (petInTrigger==null)
            return false;
        if (interactionOngoing)
            return false;


        if (petInTrigger.petStatus.canInteract())
        {
            if (trainCo!=null)
            {
                this.StopCoroutine(trainCo);
                trainCo = null;
            }
            petInTrigger.currState = GWAState.TRAINING;
            trainCo = this.StartCoroutine(TrainCo());

            return true;
        }
        return false;
    }

    public IEnumerator TrainCo()
    {
        this.interactionOngoing = true;

        if (PS_instTrainingEffect!=null)
            Destroy(PS_instTrainingEffect.gameObject);
            
        petInTrigger.PlayFX(PS_prefabTrainingEffect);

        yield return new WaitForSeconds(gwSettings.trainingDurationInSec);

        gWEnvController.ResolveEvent(GWEvent.TrainedStat, petInTrigger );
        petInTrigger.petStats.increase(relatedStat, gwSettings.trainingStatGain);

        petInTrigger.DestroyFX();
        
        petInTrigger.currState = GWAState.ALIVE;

        this.interactionOngoing = false;
    }
}
