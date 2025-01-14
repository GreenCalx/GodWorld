using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;


public enum PMOAState { DEAD=0, ALIVE=1 }
public class PushMeOutAgent : Agent
{
    public PMOAState currState;

    public GameObject PMOEnv;
    Rigidbody agentRb;
    BehaviorParameters behaviorParameters;
    public Team teamId;

    VolleyballSettings volleyballSettings;
    public PushMeOutEnvController envController;

    // Controls jump behavior
    public Collider[] hitGroundColliders = new Collider[3];
    float jumpingTime;
    Vector3 jumpTargetPos;
    Vector3 jumpStartingPos;
    float agentRot;

    private float lastJumpTime;

    EnvironmentParameters resetParams;

    public Transform self_WeaponSlotRef;
    public PMOWeapon weapon = null;

    void Start()
    {
        envController = PMOEnv.GetComponent<PushMeOutEnvController>();
        currState = PMOAState.ALIVE;
        lastJumpTime = Time.time;
    }

    public override void Initialize()
    {
        volleyballSettings = FindObjectOfType<VolleyballSettings>();
        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        agentRb = GetComponent<Rigidbody>();
        
        // for symmetry between player side
        if (teamId == Team.Blue)
        {
            agentRot = -1;
        }
        else
        {
            agentRot = 1;
        }

        resetParams = Academy.Instance.EnvironmentParameters;
    }

    /// <summary>
    /// Moves  a rigidbody towards a position smoothly.
    /// </summary>
    /// <param name="targetPos">Target position.</param>
    /// <param name="rb">The rigidbody to be moved.</param>
    /// <param name="targetVel">The velocity to target during the
    ///  motion.</param>
    /// <param name="maxVel">The maximum velocity posible.</param>
    void MoveTowards(
        Vector3 targetPos, Rigidbody rb, float targetVel, float maxVel)
    {
        var moveToPos = targetPos - rb.worldCenterOfMass;
        var velocityTarget = Time.fixedDeltaTime * targetVel * moveToPos;
        if (float.IsNaN(velocityTarget.x) == false)
        {
            rb.velocity = Vector3.MoveTowards(
                rb.velocity, velocityTarget, maxVel);
        }
    }

    /// <summary>
    /// Check if agent is on the ground to enable/disable jumping
    /// </summary>
    public bool CheckIfGrounded()
    {
        // hitGroundColliders = new Collider[3];
        // var o = gameObject;
        // Physics.OverlapBoxNonAlloc(
        //     o.transform.localPosition + new Vector3(0, -0.05f, 0),
        //     new Vector3(0.95f / 2f, 0.5f, 0.95f / 2f),
        //     hitGroundColliders,
        //     o.transform.rotation);
        // var grounded = false;
        // foreach (var col in hitGroundColliders)
        // {
        //     if (col==null)
        //         continue;

        //     if (col.GetComponent<PMOTerrainChunk>())
        //     {
        //         grounded = true; //then we're grounded
        //         Debug.Log(o.name + " grounded");
        //         break;
        //     }
        // }
        // return grounded;
        
        Ray ray = new Ray( transform.position, Vector3.up * -1f);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1.0f))
        {
            PMOTerrainChunk pmot = hit.collider.gameObject.GetComponent<PMOTerrainChunk>();
            if (!!pmot)
            {
                return true;
            }
            PushMeOutAgent pmoa = hit.collider.gameObject.GetComponent<PushMeOutAgent>();
            if (!!pmoa)
            {
                return true;
            }
        }
        return false;
    }

    public void WeaponHit()
    {
        envController.ResolveEvent(PMOEvent.WeaponHit, this);
    }

    public void HitByMissile(Vector3 iMissilePosition)
    {
        envController.ResolveEvent(PMOEvent.HitByWeapon, this);
        agentRb.AddForce(iMissilePosition.normalized, ForceMode.Impulse);
    }

    public void equipWeapon(PMOWeapon iWeapon)
    {
        weapon = Instantiate(iWeapon, transform);
        weapon.transform.position = self_WeaponSlotRef.position;
        weapon.transform.rotation = self_WeaponSlotRef.rotation;
        weapon.holder = this;
        //weapon = iWeapon;
    }


    /// <summary>
    /// Called when agent collides with the ball
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        // if (c.gameObject.CompareTag("ball"))
        // {
        //     //envController.UpdateLastHitter(teamId);
        // }
    }

    /// <summary>
    /// Starts the jump sequence
    /// </summary>
    public void Jump()
    {
        jumpingTime = 0.2f;
        jumpStartingPos = agentRb.position;
    }

    /// <summary>
    /// Resolves the agent movement
    /// </summary>
    public void MoveAgent(ActionSegment<int> act)
    {
        var grounded = CheckIfGrounded();
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var dirToGoSideAction = act[2];
        var jumpAction = act[3];
        var shootAction = act[4];

        if (dirToGoForwardAction == 1)
            dirToGo = (grounded ? 1f : 0.5f) * transform.forward * 1f;
        else if (dirToGoForwardAction == 2)
            dirToGo = (grounded ? 1f : 0.5f) * transform.forward * volleyballSettings.speedReductionFactor * -1f;
        else
            dirToGo = Vector3.zero;

        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;
        else
            rotateDir = Vector3.zero;

        if (dirToGoSideAction == 1)
            dirToGo = (grounded ? 1f : 0.5f) * transform.right * volleyballSettings.speedReductionFactor * -1f;
        else if (dirToGoSideAction == 2)
            dirToGo = (grounded ? 1f : 0.5f) * transform.right * volleyballSettings.speedReductionFactor;

        if (jumpAction == 1)
            if (((jumpingTime <= 0f) && grounded))
            {
                Jump();
            }

        if (rotateDir != Vector3.zero)
            transform.Rotate(rotateDir, Time.fixedDeltaTime * 100f);
            
        agentRb.AddForce(agentRot * dirToGo * volleyballSettings.agentRunSpeed,
            ForceMode.VelocityChange);

        if (jumpingTime > 0f)
        {
            if (Time.time > lastJumpTime + volleyballSettings.jumpCooldown)
            {
                jumpTargetPos =
                    new Vector3(agentRb.position.x,
                        jumpStartingPos.y + volleyballSettings.agentJumpHeight,
                        agentRb.position.z) + agentRot*dirToGo;

                MoveTowards(jumpTargetPos, agentRb, volleyballSettings.agentJumpVelocity,
                    volleyballSettings.agentJumpVelocityMaxChange);
                
                lastJumpTime = Time.time;
            }
        }

        if (!(jumpingTime > 0f) && !grounded)
        {
            agentRb.AddForce(
                Vector3.down * volleyballSettings.fallingForce, ForceMode.Acceleration);
        }

        if (jumpingTime > 0f)
        {
            jumpingTime -= Time.fixedDeltaTime;
        }

        if (shootAction==1)
        {
            if (weapon!=null)
            {
                if (weapon.TryFire())
                {
                    checkWeaponAmmmo();
                }
            }
        }
    }

    public void checkWeaponAmmmo()
    {
        if (weapon.currAmmo<=0)
        {
            DestroyWeapon();
        }
    }

    public void DestroyWeapon()
    {
        if (weapon==null)
            return;
        Destroy(weapon.gameObject);
        weapon = null;
    }

    public List<int> pollGroundState()
    {
        List<int> retvals = new List<int>(9){-1, -1, -1, -1, -1, -1, -1, -1, -1};
        BoxCollider box_col = GetComponent<BoxCollider>();
        Vector3 agent_size = box_col.size;
        float half_size = agent_size.z/2f;
        List<Vector3> offsets = new List<Vector3>(9)
        {
            //center
            Vector3.zero,
            // medians
            new Vector3(half_size, 0f, 0f),
            new Vector3(-half_size, 0f, 0f),
            new Vector3(0f, 0f, half_size),
            new Vector3(0f, 0f, -half_size),
            //corners
            new Vector3(half_size, 0f, half_size),
            new Vector3(-half_size, 0f, half_size),
            new Vector3(half_size, 0f, -half_size),
            new Vector3(-half_size, 0f, -half_size)
        };

        
        for(int i=0; i < 9; i++)
        {
            Vector3 rayRoot = transform.position + offsets[i];

            Ray ray = new Ray( rayRoot, Vector3.up * -1f);
            
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5.0f))
            {
                PMOTerrainChunk pmot = hit.collider.gameObject.GetComponent<PMOTerrainChunk>();
                if (!!pmot)
                {
                    retvals[i] = (int)pmot.currState;
                    Debug.DrawRay(rayRoot, Vector3.up * -5f, (retvals[i]==0)?Color.green: Color.yellow);
                }
                else {
                    Debug.DrawRay(rayRoot, Vector3.up * -5f, Color.red);
                }
            } else {
                Debug.DrawRay(rayRoot, Vector3.up * -5f, Color.red);
            }
        }
        return retvals;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(envController.elapsedGameTime);

        sensor.AddObservation(this.transform.position.x);
        sensor.AddObservation(this.transform.position.y);
        sensor.AddObservation(this.transform.position.z);

        // Agent rotation (1 float)
        sensor.AddObservation(this.transform.rotation.y);

        // Agent velocity (3 floats)
        sensor.AddObservation(agentRb.velocity);

        // can jump ?
        sensor.AddObservation(CheckIfGrounded());

        // ground states below player
        foreach (int v in pollGroundState())
        {
            sensor.AddObservation(v);
        }

        //has weapon?
        // bool hasWeapon = (weapon!=null);
        // int weaponID = (int) (hasWeapon ? weapon.weaponType : PMOWeaponType.NONE);
        // sensor.AddObservation(weaponID);
        // sensor.AddObservation(hasWeapon? weapon.currAmmo:0);

        // All remaining platforms
        // foreach( PMOTerrainChunk pmotc in envController.platforms )
        // {
        //     Vector3 pos = pmotc.transform.position;
        //     sensor.AddObservation(pos);
        //     sensor.AddObservation(pos*)
        // }
        
    }

    // For human controller
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // forward
            discreteActionsOut[0] = 1;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // backward
            discreteActionsOut[0] = 2;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // rotate left
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            // rotate right
            discreteActionsOut[1] = 2;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // move left
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // move right
            discreteActionsOut[2] = 2;
        }

        // jump
        discreteActionsOut[3] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        // shoot missile
        discreteActionsOut[4] = Input.GetKey(KeyCode.Q) ? 1 : 0;

    }
}
