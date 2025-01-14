using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


public enum PMOEvent 
{
    LastAlive = 0,
    FallFromStage=1,
    KillSomeone=2,
    GetKilledBySomeone=3,
    SurvivalBonus=4,
    WeaponHit=5,
    HitByWeapon=6,
    ZoneTick=7,
    CapturedAZone=8,
    WeaponMiss=9
}

public class PushMeOutEnvController : MonoBehaviour
{
    public List<PushMeOutAgent> agents;
    public float zoneControlTickRate = 5f;
    public float suddenDeathGameTimeTrh = 100f;
    
    public GameObject arenaRef;
    private GameObject arenaInst;
    public List<PMOTerrainChunk> platforms;
    public float timeBetweenPlatformFall = 1f;
    private float elapsedTimeSinceLastFall = 0f;

    public UIScore uiScore;

    public List<PMOItem> spawnableItems;
    public float itemSpawnRate = 10f;
    public List<PMOItem> spawnedItems;
    public int maxItemOnMap = 3;


    private float blue_score;
    private float purple_score;
    public float elapsedGameTime = 0f;
    private float elapsedZoneTickTime = 0f;
    private float elapsedItemSpawnTime =0f;

    public int MaxEnvironmentSteps;

    // Start is called before the first frame update
    void Start()
    {
        ResetScene();
    }

    private void initTerrain()
    {
        if (!!arenaInst)
        {
            arenaInst.transform.parent = null;
            Destroy(arenaInst);
            platforms.Clear();
            arenaInst = null;
        }
        arenaInst = Instantiate(arenaRef, transform);

        platforms = new List<PMOTerrainChunk>();
        platforms.AddRange(GetComponentsInChildren<PMOTerrainChunk>());
        platforms = platforms.Where(x => x!=null).ToList();
        elapsedTimeSinceLastFall = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        elapsedGameTime+=Time.fixedDeltaTime;
        elapsedZoneTickTime+=Time.fixedDeltaTime;
        elapsedItemSpawnTime += Time.fixedDeltaTime;

        if (elapsedItemSpawnTime >= itemSpawnRate)
        {
            SpawnItem();
            elapsedItemSpawnTime = 0f;
        }

        if (elapsedZoneTickTime >= zoneControlTickRate)
        {
            ZoneTick();
        }

        if ( elapsedGameTime > suddenDeathGameTimeTrh)
        {
            elapsedTimeSinceLastFall+=Time.fixedDeltaTime;
            if (elapsedTimeSinceLastFall >= timeBetweenPlatformFall)
            {
                fall();
                elapsedTimeSinceLastFall = 0f;
                foreach( PushMeOutAgent a in agents) 
                {
                    if (a.currState == PMOAState.ALIVE)
                        ResolveEvent(PMOEvent.SurvivalBonus, a);
                }
            }
        }
    }

    private void SpawnItem()
    {
        // select platform
        int n_platforms = platforms.Where(x => x.currState != PMOTChunkState.FALLING).ToList().Count;
        if (n_platforms==0)
            return;

        int selected = UnityEngine.Random.Range(0, n_platforms);

        // select item
        int n_items = spawnableItems.Count;
        if (n_items==0)
            return;

        int iselect = UnityEngine.Random.Range(0, n_items);

        // Spawn
        if (spawnedItems.Count >= maxItemOnMap)
            return;
        
        PMOItem newItem = Instantiate(spawnableItems[iselect]);
        newItem.transform.position = platforms[selected].transform.position;
        newItem.transform.position += new Vector3(0f,1f,0f);
        newItem.env = this;

        spawnedItems.Add(newItem);
    }

    public void notifyItemCollected(PMOItem iItem)
    {
        spawnedItems.Remove(iItem);
        Destroy(iItem.gameObject);
    }

    private void ZoneTick()
    {
        foreach ( PMOTerrainChunk pmotc in platforms)
        {
            if (pmotc.isControlled)
            {
                foreach(PushMeOutAgent pmoa in pmotc.controllers)
                {
                    ResolveEvent( PMOEvent.ZoneTick, pmoa);
                }
            }
        }
        elapsedZoneTickTime=0f;
    }

    private void fall()
    {
        int n_platforms = platforms.Where(x => x.currState != PMOTChunkState.FALLING).ToList().Count;
        if (n_platforms==0)
            return;

        int selected = UnityEngine.Random.Range(0, n_platforms);
        platforms[selected].SetState(PMOTChunkState.FALLING);

        platforms.RemoveAt(selected);
        platforms = platforms.Where(x => x != null).ToList();
    }

    private void spawnObject() {}

    public void OnAgentDeath(PushMeOutAgent iAgent)
    {
        // agents.Remove(iAgent);
        // Destroy(iAgent.gameObject);
        iAgent.currState = PMOAState.DEAD;
        ResolveEvent(PMOEvent.FallFromStage, iAgent);

        List<PushMeOutAgent> alive_agents = agents.Where(x => x.currState == PMOAState.ALIVE).ToList();
        int n_alive = alive_agents.Count;
        if (n_alive==1)
        {
            // used to win the game
        } else if (n_alive==0) {
            ResolveEvent(PMOEvent.LastAlive, iAgent);
            ResetScene();
        }
    }

    public void ResetScene()
    {
        initTerrain();

        foreach (var agent in agents)
        {
             // randomise starting positions and rotations
             var randomPosX = Random.Range(-5f, 5f);
             var randomPosZ = Random.Range(-5f, 5f);
             var randomPosY = Random.Range(0.5f, 3.75f); // depends on jump height
             var randomRot = Random.Range(-45f, 45f);

             agent.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
             agent.transform.eulerAngles = new Vector3(0, randomRot, 0);

            agent.GetComponent<Rigidbody>().velocity = default(Vector3);
            agent.currState = PMOAState.ALIVE;

            agent.DestroyWeapon();
        }

        foreach(PMOItem i in spawnedItems)
        {
            Destroy(i.gameObject);
        }
        spawnedItems.Clear();

        uiScore.raza();
        blue_score = 0f;
        purple_score = 0f;
        elapsedGameTime = 0f;
        elapsedItemSpawnTime =0f;

        // // reset ball to starting conditions
        // ResetBall();
    }

    public void ResolveEvent(PMOEvent triggerEvent, PushMeOutAgent iAgent)
    {
        switch (triggerEvent)
        {
            case PMOEvent.FallFromStage:
                updateTeamScore(iAgent, -5f);
                break;           
            case PMOEvent.LastAlive:
                foreach ( var a in agents)
                {
                    if (a==iAgent)
                    {
                        updateTeamScore(a, 2f);
                        continue;
                    }
                    updateTeamScore(a, -2f);
                    a.EndEpisode();
                }
                ResetScene();
                break;
            case PMOEvent.SurvivalBonus:
                updateTeamScore(iAgent, elapsedGameTime/100f);
                break;
            case PMOEvent.WeaponHit:
                updateTeamScore(iAgent, 1f);
                break;
            case PMOEvent.HitByWeapon:
                updateTeamScore(iAgent, -1f);
                break;
            case PMOEvent.ZoneTick:
                updateTeamScore(iAgent, 1f);
                break;
            case PMOEvent.CapturedAZone:
                updateTeamScore(iAgent, 1f);
                break;
            case PMOEvent.WeaponMiss:
                iAgent.AddReward(-1f);
                break;
        }        
    }

    public void updateTeamScore(PushMeOutAgent iPMOA, float iScore)
    {
        switch (iPMOA.teamId)
        {
            case Team.Blue:
                blue_score+=iScore;
                uiScore.SetBlueScore(blue_score);
                break;
            case Team.Purple:
                purple_score+=iScore;
                uiScore.SetPurpleScore(purple_score);
                break;
            default:
                break;
        }
        iPMOA.AddReward(iScore);
    }
}
