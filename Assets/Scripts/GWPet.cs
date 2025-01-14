using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public enum GWAPetTask { FREEWILL=0, GO_EAT = 1, GO_DRINK=2, GO_SLEEP=3, GO_SHIT=4, GO_TRAIN=5 }

[Serializable]
public class GWPetNeeds
{
    public float hungerGain = 0f;
    public float fatigueGain = 0f;
    public float thirstGain = 0f;
    public float currentHunger = 100f;
    public float currentThirst = 100f;
    public float currentFatigue = 100f;


    public void init(GWSettings iSettings)
    {
        currentHunger   = iSettings.agentTotalHunger;
        currentThirst   = iSettings.agentTotalThirst;
        currentFatigue  = iSettings.agentTotalFatigue;

        hungerGain  = iSettings.agentHungerLossPerSec * -1f;
        thirstGain = iSettings.agentThirstLossPerSec * -1f;
        fatigueGain  = iSettings.agentFatigueLossPerSec * -1f;
    }

    public void FatigueTick() { currentFatigue += fatigueGain; }
    public void HungerTick() { currentHunger += hungerGain; }
    public void ThirstTick() { currentThirst += thirstGain; }
}

[Serializable]
public class GWPetStatus
{
    public void init()
    {
        isHungry = false;
        isThirsty = false;
        isSleepy = false;

        isFrozen = false;

        interactibleZone = null;
    }


    public bool isHungry = false;
    public bool isThirsty = false;
    public bool isSleepy = false;

    public bool isFrozen = false;

    public GWInteractiveZone interactibleZone = null;
    public bool canInteract() { return interactibleZone!=null; }
}

[Serializable]
public class GWPetStats
{
    public enum STATS 
    {
        NONE=0,
        HP=1,
        MP=2,
        STR=3,
        VIT=4,
        INT=5,
        SAG=6
    };

    public void init()
    {
        stats = new Dictionary<STATS, float>(6);
        foreach(STATS s in Enum.GetValues((typeof(STATS))))
        {
            if (s==STATS.NONE)
                continue;
            stats.Add(s, 1f);
        }
    }
    public Dictionary<STATS,float> stats;
    public void increase(STATS iType, float iGain)
    {
        if (iType==STATS.NONE)
            return;
        stats[iType] += iGain;
    }

    public void reset(STATS iType, float iOptResetVal=1f)
    {
        if (iType==STATS.NONE)
            return;
        stats[iType] = iOptResetVal;
    }

    public float GetValue(STATS iType)
    {
        if (iType==STATS.NONE)
            return 0f;
        return stats[iType];
    }

}


public class GWPet : GWAgent
{
    public GWAPetTask agentJob;
    public GWPetStatus petStatus;
    public GWPetStats petStats;
    public GWPetNeeds petNeeds;
        
    [Header("Temps Manual Refs")]
    public GameObject prefab_sleepParticles;
    public Transform foodSpot;
    public Transform waterSpot;
    [Header("Internals")]
    private GameObject inst_PSFX;
    Vector3 lastExploredPos;

    
    private Coroutine hungerCo;
    private Coroutine thirstCo;
    private Coroutine fatigueCo;
    private Vector3 initPos = Vector3.zero;
    private Coroutine sleepingCo;

    public GWItem carriedItem;

    public override void Initialize()
    {
        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        agentRb = GetComponent<Rigidbody>();
        resetParams = Academy.Instance.EnvironmentParameters;

    }

    public void GWPetInit()
    {
        if (initPos==Vector3.zero)
        {
            initPos = transform.localPosition;
        }
        transform.localPosition = initPos;

        currState = GWAState.ALIVE;
        
        petStatus = new GWPetStatus();
        petStatus.init();

        petStats = new GWPetStats();
        petStats.init();

        petNeeds = new GWPetNeeds();
        petNeeds.init(gwSettings);

        agentJob = GWAPetTask.FREEWILL;

        lastExploredPos = transform.localPosition;

        if (carriedItem!=null)
        {
            Destroy(carriedItem.gameObject);
        }

        
        if (hungerCo!=null)
        {
            this.StopCoroutine(hungerCo);
            hungerCo = null;
        }
        hungerCo = this.StartCoroutine(HungerCo());

        
        if (thirstCo!=null)
        {
            this.StopCoroutine(thirstCo);
            thirstCo = null;
        }
        thirstCo = this.StartCoroutine(ThirstCo());

        
        if (fatigueCo!=null)
        {
            this.StopCoroutine(fatigueCo);
            fatigueCo = null;
        }
        fatigueCo = this.StartCoroutine(FatigueCo());

        // Destroy FX if exists
        DestroyFX();

    }

    public void DestroyFX()
    {
        if (inst_PSFX!=null)
        { Destroy(inst_PSFX.gameObject); }
    }

    public void PlayFX(GameObject iFXPrefab)
    {
        inst_PSFX = Instantiate(iFXPrefab, this.transform);
        inst_PSFX.transform.localPosition = Vector3.zero;
    }

    public bool IsAlive()
    {
        return currState != GWAState.DEAD;
    }

    public bool IsTraining()
    {
        return currState == GWAState.TRAINING;
    }

    public IEnumerator HungerCo()
    {
        while (currState!=GWAState.DEAD)
        {
            petNeeds.HungerTick();
            petStatus.isHungry = (petNeeds.currentHunger < gwSettings.agentIsHungryPercent * gwSettings.agentTotalHunger);
            if (petNeeds.currentHunger <= 0f)
            {
                // Dead from starving
                envController.OnAgentDeath(this);
                yield break;
            }
            yield return new WaitForSeconds(1f);            
        }
    }

    public IEnumerator ThirstCo()
    {
        while (currState!=GWAState.DEAD)
        {
            petNeeds.ThirstTick();
            petStatus.isThirsty = (petNeeds.currentThirst < gwSettings.agentIsThirstyPercent * gwSettings.agentTotalThirst);
            if (petNeeds.currentThirst <= 0f)
            {
                // Dead from starving
                envController.OnAgentDeath(this);
                yield break;
            }
            yield return new WaitForSeconds(1f);            
        }
    }

    public IEnumerator FatigueCo()
    {
        while (this.currState!=GWAState.DEAD)
        {
            petNeeds.FatigueTick();
            petStatus.isSleepy = (petNeeds.currentFatigue < gwSettings.agentIsFatiguedPercent * gwSettings.agentTotalFatigue);
            if (petNeeds.currentFatigue <= 0f)
            {
                envController.ResolveEvent(GWEvent.SleptFromExhaustion, this);
                Sleep();
            }
            yield return new WaitForSeconds(1f);            
        }
    }

    public IEnumerator SleepCo()
    {
        PlayFX(prefab_sleepParticles);

        while(petNeeds.currentFatigue<=gwSettings.agentTotalFatigue)
        {
            
            yield return new WaitForSeconds(1f);
        }

        DestroyFX();

        currState = GWAState.ALIVE;
    }

    public bool Sleep()
    {
        if (!petStatus.isSleepy)
            return false;

        if (this.currState==GWAState.ASLEEP)
            return false;
        
        this.currState = GWAState.ASLEEP;
        if (sleepingCo!=null)
        {
            this.StopCoroutine(sleepingCo);
            sleepingCo = null;
        }
        sleepingCo = this.StartCoroutine(SleepCo());

        return true;
    }

    public void CollectFood(GWFood iFood)
    {
        if (petNeeds.currentHunger > (gwSettings.agentTotalHunger - iFood.foodValue))
        {
            envController.ResolveEvent(GWEvent.OverAteFood, this);
        }
        else
        {
            envController.ResolveEvent(GWEvent.AteFood, this);
        }

        petNeeds.currentHunger += iFood.foodValue;
        if (petNeeds.currentHunger > gwSettings.agentTotalHunger)
            petNeeds.currentHunger = gwSettings.agentTotalHunger;
    }

    public void CollectWater(GWWaterBucket iWaterBucket)
    {
        if (petNeeds.currentThirst > (gwSettings.agentTotalThirst - iWaterBucket.waterValue))
        {
            envController.ResolveEvent(GWEvent.OverDrankWater, this);
        } else {
            envController.ResolveEvent(GWEvent.DrankWater, this);
        }

        petNeeds.currentThirst += iWaterBucket.waterValue;
        if (petNeeds.currentThirst > gwSettings.agentTotalThirst)
            petNeeds.currentThirst = gwSettings.agentTotalThirst;
    }

    public  bool TryCarryItem(GWItem iItem)
    {
        if (carriedItem==null)
        {
            carriedItem = iItem;
            iItem.SetAsCarried(this);
            iItem.transform.parent = transform;
            iItem.transform.localPosition = new Vector3(0f,gwSettings.carriedItemYOffset,0f);
            return true;
        }
        return false;
    }

    public bool TryDropCarriedItem()
    {
        if (carriedItem==null)
            return false;

        carriedItem.SetAsDropped();
        carriedItem.transform.parent = transform.parent;
        carriedItem.transform.localPosition = transform.localPosition + transform.forward.normalized* gwSettings.dropItemFwdOffset;
        carriedItem = null;
        return true;
    }

    public bool TryUseCarriedItem()
    {
        if (carriedItem==null)
            return false;

        carriedItem.OnUse(this);
        carriedItem.Consume();
        carriedItem  = null;
        return true;
    }

    public override void TryContinuousActions(ActionBuffers buff)
    {
        var continuousActions = buff.ContinuousActions;

        var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
        var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);

        if (!petStatus.isFrozen)
            MoveAgent(forward, right, rotate);
    }

    public override void TryDiscreteActions(ActionBuffers buff)
    {
        // discrete action
        if (elapsedTimeSinceLastAction < gwSettings.actionCooldown)
            return;

        var discreteActions = buff.DiscreteActions;

        // possible actions with carried items
        var itemAction = discreteActions[0];
        if (itemAction==0)
        {
            // do nothing
        } else if (itemAction==1)
        {
            //drop item
            if (TryDropCarriedItem())
            {
                // successful
                ValidateDiscreteAction();
            } else {
                FailedDiscreteAction();
            }
        } else if (itemAction==2)
        {
            // use item
            if (TryUseCarriedItem())
            {
                // successful
                ValidateDiscreteAction();
            } else {
                FailedDiscreteAction();
            }
        }

        // sleep action
        var sleepAction = discreteActions[1];
        if (petStatus.isSleepy)
        {
            if (sleepAction==1)
            {
                if (Sleep())
                {
                    ValidateDiscreteAction();
                } else {
                    FailedDiscreteAction();
                }
            }
        }

        var interactAction = discreteActions[2];
        if (petStatus.canInteract())
        {
            if (interactAction==1)
            {
                if (petStatus.interactibleZone.interact())
                {
                    ValidateDiscreteAction();
                } else {
                    FailedDiscreteAction();
                }
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3( UnityEngine.Random.Range(-2f,2f), 0f, UnityEngine.Random.Range(-2f, 2f));
        //OnEpisodeBeginEvent?.Invoke()(this, EventArgs.Empty);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        TryContinuousActions(actionBuffers);
        TryDiscreteActions(actionBuffers);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1
        sensor.AddObservation(envController.elapsedGameTime);

        // 3
        sensor.AddObservation(this.transform.localPosition.x);
        sensor.AddObservation(this.transform.localPosition.y);
        sensor.AddObservation(this.transform.localPosition.z);

        // Agent rotation (1 float)
        sensor.AddObservation(this.transform.localRotation.y);

        // Agent velocity (3 floats)
        sensor.AddObservation(agentRb.velocity);

        // can jump (1)
        //sensor.AddObservation(CheckIfGrounded());

        // Agent generic state
        sensor.AddObservation((int)currState);

        // pet stats
        sensor.AddObservation(petStats.GetValue(GWPetStats.STATS.HP));
        sensor.AddObservation(petStats.GetValue(GWPetStats.STATS.MP));
        sensor.AddObservation(petStats.GetValue(GWPetStats.STATS.STR));
        sensor.AddObservation(petStats.GetValue(GWPetStats.STATS.VIT));
        sensor.AddObservation(petStats.GetValue(GWPetStats.STATS.INT));
        sensor.AddObservation(petStats.GetValue(GWPetStats.STATS.SAG));

        // interaction status
        if (petStatus.canInteract())
        {
            sensor.AddObservation(true);
            sensor.AddObservation((int)petStatus.interactibleZone.interactType);
            sensor.AddObservation(petStatus.interactibleZone.GetZoneType());
        }
        else
        {
            sensor.AddObservation(false);
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }

        // hunger/water/sleep states
        sensor.AddObservation(petNeeds.currentHunger);
        sensor.AddObservation(petNeeds.currentThirst);
        sensor.AddObservation(petNeeds.currentFatigue);

        // Carried item type 1int
        if (carriedItem==null)
            sensor.AddObservation((int)GWItemType.NONE);
        else
            sensor.AddObservation((int)carriedItem.itemType);
        
        // elapsed action cooldown 1float
        sensor.AddObservation(elapsedTimeSinceLastAction);

        //last action attempt successfull 1bool
        sensor.AddObservation((int)lastActionSuccessfull);
        if (lastActionSuccessfull!=GWALastAction.NONE) // reset feedback
            lastActionSuccessfull = GWALastAction.NONE;

        // job spot
        Vector3 relativeFoodLoc = foodSpot.localPosition - transform.localPosition;
        sensor.AddObservation(relativeFoodLoc.x);
        sensor.AddObservation(relativeFoodLoc.z);

        Vector3 relativeWateRLoc = waterSpot.localPosition - transform.localPosition;
        sensor.AddObservation(relativeWateRLoc.x);
        sensor.AddObservation(relativeWateRLoc.z);
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[2] = -1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[0] = -1;
        }
        var discreteActionsOut = actionsOut.DiscreteActions;
        // item action
        if (Input.GetKey(KeyCode.Q))
            discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.E))
            discreteActionsOut[0] = 2;
        else
            discreteActionsOut[0] = 0;

        // stash action
        if (Input.GetKey(KeyCode.Keypad1))
            discreteActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.Keypad2))
            discreteActionsOut[1] = 2; 
        else
            discreteActionsOut[1] = 0;

    }


    // Update is called once per frame
    void Update()
    {
        elapsedTimeSinceLastAction += Time.deltaTime;
    }
}
