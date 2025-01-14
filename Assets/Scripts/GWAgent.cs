using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public enum GWAState { DEAD=0, ALIVE=1, ASLEEP=2, TRAINING=3 }
public enum GWALastAction{ NONE=0, SUCCESS=1, FAIL=3}

public class GWAgent : Agent
{
    public GWAState currState;

    public GWSettings gwSettings;

    protected BehaviorParameters behaviorParameters;
    
    public GWEnvController envController;
    protected EnvironmentParameters resetParams;
    public Rigidbody agentRb;
    [Header("Actions")]
    protected GWALastAction lastActionSuccessfull = GWALastAction.NONE;
    protected float elapsedTimeSinceLastAction = 0f;
    [Header("Extras")]
    
    public Team teamId;



    void Start()
    {
        //currState = GWAState.ALIVE;
        //lastJumpTime = Time.time;
    }

    void Update()
    {

    }

    // called once at first launch
    public override void Initialize()
    {
        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        agentRb = GetComponent<Rigidbody>();
        resetParams = Academy.Instance.EnvironmentParameters;
    }

    // Called By env


    void MoveTowards(Vector3 targetPos, Rigidbody rb, float targetVel, float maxVel)
    {
        var moveToPos = targetPos - rb.worldCenterOfMass;
        var velocityTarget = Time.fixedDeltaTime * targetVel * moveToPos;
        if (float.IsNaN(velocityTarget.x) == false)
        {
            rb.velocity = Vector3.MoveTowards(
            rb.velocity, velocityTarget, maxVel);
        }
        envController.ResolveEvent(GWEvent.Exploring, this);
    }


    
    public void MoveAgent(float iForwardAxis, float iRightAxis, float iRotateAxis)
    {
        if (currState!=GWAState.ALIVE)
            return;

        // var grounded = CheckIfGrounded();
        var grounded = true;
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        //var jumpAction = false;

        dirToGo = transform.forward * iForwardAxis;
        dirToGo += transform.right * iRightAxis;
        rotateDir = -transform.up * iRotateAxis;
        
        if (!grounded)
        {
            //MoveTowards(dirToGo, agentRb, gwSettings.agentRunSpeed, gwSettings.agentMaxRunVel);
        }
        agentRb.AddForce(dirToGo * gwSettings.agentRunSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * gwSettings.agentTurnSpeed);
    }


    public virtual void TryContinuousActions(ActionBuffers buff)
    {

    }

    public virtual void TryDiscreteActions(ActionBuffers buff)
    {

    }

    protected void ValidateDiscreteAction()
    {
        lastActionSuccessfull = GWALastAction.SUCCESS;
        elapsedTimeSinceLastAction = 0f;
        envController.ResolveEvent(GWEvent.ActionSuccessfull, this);
    }

    protected void FailedDiscreteAction()
    {
        lastActionSuccessfull = GWALastAction.FAIL;
        elapsedTimeSinceLastAction = 0f;
        envController.ResolveEvent(GWEvent.ActionFailed, this);
    }

    public bool CheckIfGrounded()
    {
        Ray ray = new Ray( transform.localPosition, Vector3.up * -1f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.0f))
        {
            GWTerrain gwt = hit.collider.gameObject.GetComponent<GWTerrain>();
            if (!!gwt)
            {
                return true;
            }
            GWAgent gwa = hit.collider.gameObject.GetComponent<GWAgent>();
            if (!!gwa)
            {
                return true;
            }
        }
        return false;
    }

} 
