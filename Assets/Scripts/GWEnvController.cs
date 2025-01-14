using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public enum GWEvent
{
    AgentDeath = 0,
    LastStanding = 1,
    Exploring = 2,
    ActionSuccessfull = 3,
    AteFood = 4,
    OverAteFood = 5,
    DrankWater=6,
    OverDrankWater=7,
    ItemStackCreated=8,
    OriginOfOtherAgentDeath=9,
    SleptFromExhaustion=10,
    TrainedStat=11,
    GlobalStatBonus=12,
    ActionFailed=13
}

public class GWEnvController : MonoBehaviour
{
    public GWSettings gwSettings;
    public List<GWPet> pet_agents;
    public float elapsedGameTime = 0f;
    public float score;
    public GWPositionRandomizer globalRandomizer;
    public List<GWItemSpawner> foodSpawners;
    private float elapsedTimeSinceEnvReward = 0f;

    [Header("Optionals")]
    public TextMeshProUGUI scoreUIText;

    [Header("Internals")]
    public List<GWItem> spawnedItems;
    public StateMachine<PetToken, PetChecks, PetNodeActionPool> petStateMachine;

    void Start()
    {
        ResetScene();
    }

    void FixedUpdate()
    {
        elapsedGameTime+=Time.fixedDeltaTime;

        elapsedTimeSinceEnvReward += Time.fixedDeltaTime;
        if (elapsedTimeSinceEnvReward >= gwSettings.timeIntervalForEnvReward)
        {
            // periodic env reward here
            elapsedTimeSinceEnvReward = 0f;
        }
    }

    void Update()
    {
        if (petStateMachine!=null)
        {
            if (petStateMachine.playMode)
            {
                petStateMachine.Update();
            }
        }
    }

    public void MakePetGraph()
    {
        petStateMachine = new StateMachine<PetToken, PetChecks, PetNodeActionPool>();

        // Nodes
        petStateMachine.Build(GWAState.ALIVE, Enum.GetValues(typeof(GWAState)).Cast<GWAState>().ToList());

        // connections
        petStateMachine.AddConnection(GWAState.ALIVE, GWAState.DEAD,        petStateMachine.checks.DeathCond);

        petStateMachine.AddConnection(GWAState.ALIVE, GWAState.ASLEEP,      petStateMachine.checks.SleepCond);
        petStateMachine.AddConnection(GWAState.ASLEEP, GWAState.ALIVE,      petStateMachine.checks.SleepOverCond);

        petStateMachine.AddConnection(GWAState.ALIVE, GWAState.TRAINING,    petStateMachine.checks.TrainCond);
        petStateMachine.AddConnection(GWAState.TRAINING, GWAState.ALIVE,    petStateMachine.checks.TrainingOverCond);


        // Node properties callbacks
        petStateMachine.EditNodeEnterCB(GWAState.ALIVE,     petStateMachine.nodeActionPool.UnFreeze);
        petStateMachine.EditNodeEnterCB(GWAState.ALIVE,     petStateMachine.nodeActionPool.UpdateNeedsFromState);

        petStateMachine.EditNodeEnterCB(GWAState.ASLEEP,    petStateMachine.nodeActionPool.Freeze);
        petStateMachine.EditNodeEnterCB(GWAState.ASLEEP,     petStateMachine.nodeActionPool.UpdateNeedsFromState);

        petStateMachine.EditNodeEnterCB(GWAState.TRAINING,  petStateMachine.nodeActionPool.Freeze);
        petStateMachine.EditNodeEnterCB(GWAState.TRAINING,  petStateMachine.nodeActionPool.UpdateNeedsFromState);
        
        petStateMachine.EditNodeEnterCB(GWAState.DEAD,      petStateMachine.nodeActionPool.Freeze);


        // Add agent tokens on graph
        foreach(GWPet p in pet_agents)
        {
            petStateMachine.AddToken(new PetToken(petStateMachine.RootNode, p));


        }

        // Auto play
        petStateMachine.Play(true);
    }

    public void ResetScene()
    {
        foreach (var pet in pet_agents)
        {
            pet.gameObject.SetActive(true);
            var randomRot = UnityEngine.Random.Range(-45f, 45f);

            pet.transform.eulerAngles = new Vector3(0, randomRot, 0);

            pet.GetComponent<Rigidbody>().velocity = default(Vector3);
            pet.GWPetInit();
        }

        foreach(GWItem i in spawnedItems)
        {
            i.originator.notifySpawnItemDestroyed(i);
            Destroy(i.gameObject);
        }
        spawnedItems.Clear();

        globalRandomizer.init();
        globalRandomizer.RandomizeAll();
        foreach(var spawner in foodSpawners)
        {
            spawner.Reset();
        }
        
        score = 0f;
        scoreUIText.text = score.ToString();

        elapsedGameTime = 0f;
        elapsedTimeSinceEnvReward = 0f;

        MakePetGraph();
    }

    public void OnPetDeath(GWPet iPet)
    {
        iPet.currState = GWAState.DEAD;
        ResolveEvent(GWEvent.AgentDeath, iPet);
        foreach(GWPet pet in pet_agents)
        {
            pet.EndEpisode();
        }
    }

    public void OnAgentDeath(GWAgent iAgent)
    {
        iAgent.currState = GWAState.DEAD;

        // Determine origin of death and punish accordingly
        ResolveEvent(GWEvent.AgentDeath, iAgent);

        iAgent.EndEpisode();

        ResetScene();
    }

    public void notifyItemCollected(GWItem iItem)
    {
        iItem.originator.notifySpawnItemDestroyed(iItem);
    }

    public void notifySpawnedItemDestroyed(GWItem iItem)
    {
        if (spawnedItems.Contains(iItem))
        {
            spawnedItems.Remove(iItem);
            spawnedItems = spawnedItems.Where(x => x!=null).ToList();
        }
    }

    public void ResolveEvent(GWEvent triggerEvent, GWAgent iAgent)
    {
        switch (triggerEvent)
        {
            case GWEvent.AgentDeath:
                updateScore(iAgent, -50f);
                break;
            case GWEvent.Exploring:
                updateScore(iAgent, 0.0001f);
                break;
            case GWEvent.ActionSuccessfull:
                updateScore(iAgent, 1f);
                break;
            case GWEvent.AteFood:
                updateScore(iAgent, 2f);
                break;
            case GWEvent.OverAteFood:
                updateScore(iAgent, -1f);
                break;
            case GWEvent.DrankWater:
                updateScore(iAgent, 2f);
                break;
            case GWEvent.OverDrankWater:
                updateScore(iAgent, -1f);
                break;
            case GWEvent.SleptFromExhaustion:
                updateScore(iAgent, -10f);
                break;
            case GWEvent.TrainedStat:
                updateScore(iAgent, 2f);
                break;
            case GWEvent.ActionFailed:
                updateScore(iAgent, -1f);
                break;
            default:
                break;
        }        
    }

    public void updateScore(GWAgent iPMOA, float iScore)
    {
        score += iScore;
        iPMOA.AddReward(iScore);
        scoreUIText.text = score.ToString();
    }

}