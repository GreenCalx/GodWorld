using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PMOTChunkState { NORMAL=0, FALLING = 1, BLUE_CONTROLLED=2, PURPLE_CONTROLLED=3}

public class PMOTerrainChunk : MonoBehaviour
{
    public PMOTChunkState currState;
    public Rigidbody rb;
    private Material matOnDefault;
    public Material matOnFall;
    public Material matOnPurpleControlled;
    public Material matOnBlueControlled;
    
    public float timeToControlZone = 3f;
    public List<PushMeOutAgent> contenders;
    public List<PushMeOutAgent> controllers;

    ///
    private MeshRenderer meshRenderer;
    private bool controllable = true;
    public bool isControlled = false;
    private float elapsedTimeToCtrl = 0f;

    void Start()
    {
        
        init();
    }

    ///
    private void init()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        matOnDefault = meshRenderer.sharedMaterial;
        
        rb = GetComponent<Rigidbody>();
        SetState(PMOTChunkState.NORMAL);
        elapsedTimeToCtrl = 0f;
        contenders = new List<PushMeOutAgent>(0);
        isControlled = false;
    }

    void Update()
    {
        if (currState == PMOTChunkState.FALLING)
            return;

        if (controllable)
        {
            if (contenders.Count > 0)
            {
                zoneControlUpdate();
            }
        }
    }

    private void zoneControlUpdate()
    {
        int n_blue =0, n_purple =0;
        foreach ( PushMeOutAgent pmoa in contenders)
        {
            switch (pmoa.teamId)
            {
                case Team.Blue:
                    n_blue++;
                    break;
                case Team.Purple:
                    n_purple++;
                    break;
                default:
                    break;
            }
        }
        if ( n_blue > n_purple )
        {
            if (currState==PMOTChunkState.PURPLE_CONTROLLED)
            {
                SetState(PMOTChunkState.NORMAL);
                elapsedTimeToCtrl =0f;
            }
            elapsedTimeToCtrl += Time.deltaTime;
            isControlled = (elapsedTimeToCtrl > timeToControlZone);
            if (isControlled && (currState!=PMOTChunkState.BLUE_CONTROLLED))
            {
                SetState(PMOTChunkState.BLUE_CONTROLLED);
                controllers = getBlueContenders();
                RewardContenders();
                
            }
            
        } else if (n_purple > n_blue)
        {
            if (currState==PMOTChunkState.BLUE_CONTROLLED)
            {
                SetState(PMOTChunkState.NORMAL);
                controllers = new List<PushMeOutAgent>(0);
                elapsedTimeToCtrl =0f;
            }
            elapsedTimeToCtrl += Time.deltaTime;
            isControlled = (elapsedTimeToCtrl > timeToControlZone);
            if (isControlled && (currState!=PMOTChunkState.PURPLE_CONTROLLED))
            {
                SetState(PMOTChunkState.PURPLE_CONTROLLED);
                controllers = getPurpleContenders();
                RewardContenders();
            }
        } else {
            elapsedTimeToCtrl = 0f;
            isControlled = false;
            if (currState!=PMOTChunkState.NORMAL)
            {
                SetState(PMOTChunkState.NORMAL);
                controllers = new List<PushMeOutAgent>(0);
            }
        }

        if (isControlled)
            materialUpdate();

    }

    private void RewardContenders()
    {
        foreach ( PushMeOutAgent pmoa in contenders)
        {
            switch (pmoa.teamId)
            {
                case Team.Blue:
                    if (currState==PMOTChunkState.BLUE_CONTROLLED)
                        pmoa.envController.ResolveEvent(PMOEvent.CapturedAZone, pmoa);
                    break;
                case Team.Purple:
                    if (currState==PMOTChunkState.PURPLE_CONTROLLED)
                        pmoa.envController.ResolveEvent(PMOEvent.CapturedAZone, pmoa);
                    break;
                default:
                    break;
            }
        }
    }

    private void materialUpdate()
    {
        if (!meshRenderer)
            return;

        if (controllable && isControlled)
        {
            if (currState == PMOTChunkState.BLUE_CONTROLLED)
                meshRenderer.sharedMaterial = matOnBlueControlled;
            else if (currState == PMOTChunkState.PURPLE_CONTROLLED)
                meshRenderer.sharedMaterial = matOnPurpleControlled;
        } else {
                if (currState == PMOTChunkState.FALLING)
                    meshRenderer.sharedMaterial = matOnFall;
                else
                    meshRenderer.sharedMaterial = matOnDefault;
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }


    public void SetState(PMOTChunkState iState)
    {
        switch (iState)
        {
            case PMOTChunkState.NORMAL:
                currState = PMOTChunkState.NORMAL;
                rb.isKinematic = true;
                rb.useGravity = false;
                controllable = true;
                isControlled = false;
                materialUpdate();
                break;
            case PMOTChunkState.BLUE_CONTROLLED:
                currState = PMOTChunkState.BLUE_CONTROLLED;
                rb.isKinematic = true;
                rb.useGravity = false;
                controllable = true;
                break;
            case PMOTChunkState.PURPLE_CONTROLLED:
                currState = PMOTChunkState.PURPLE_CONTROLLED;
                rb.isKinematic = true;
                rb.useGravity = false;
                controllable = true;
                isControlled = (elapsedTimeToCtrl > timeToControlZone);
                break;
            case PMOTChunkState.FALLING :
                if (currState==PMOTChunkState.FALLING)
                    return;
                currState = PMOTChunkState.FALLING;
                controllable = false;
                isControlled  = false;
                StartCoroutine(fall());
                break;
            default:
                currState = PMOTChunkState.NORMAL;
                rb.isKinematic = true;
                rb.useGravity = false;
                break;
        }
    }

    IEnumerator fall()
    {
        materialUpdate();
        gameObject.tag = "void";
        yield return new WaitForSeconds(1f);
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    void OnCollisionEnter(Collision iCol)
    {
        PushMeOutAgent pmoa = iCol.collider.GetComponent<PushMeOutAgent>();
        if (!!pmoa)
        {
            contenders.Add(pmoa);
        }
    }

    void OnCollisionExit(Collision iCol)
    {
        PushMeOutAgent pmoa = iCol.collider.GetComponent<PushMeOutAgent>();
        if (!!pmoa)
        {
            contenders.Remove(pmoa);
        }
    }

    public List<PushMeOutAgent> getBlueContenders()
    {
        return contenders.Where( x => (x.teamId==Team.Blue)).ToList();
    }
    public List<PushMeOutAgent> getPurpleContenders()
    {
        return contenders.Where( x => (x.teamId==Team.Purple)).ToList();
    }
}
