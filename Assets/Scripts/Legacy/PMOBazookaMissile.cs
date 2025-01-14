using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMOBazookaMissile : MonoBehaviour
{
    public ParticleSystem self_explosionPSRef;
    public float lifeTime = 4f;
    public float missileVelocity;
    public float missileMaxVelocity;
    [Header("Internals")]
    public PushMeOutAgent originator;
    public Vector3 direction;

    private float elapsedLifetime;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        elapsedLifetime = 0f;
        gameObject.layer = originator.gameObject.layer;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveTowards(direction, rb, missileVelocity, missileMaxVelocity);

        elapsedLifetime += Time.deltaTime;
        if (elapsedLifetime > lifeTime)
        {
            Debug.Log("elapsedLifetime > lifeTime");
            originator.envController.ResolveEvent(PMOEvent.WeaponMiss, originator);
            explode();
        }
    }



    void OnCollisionEnter(Collision iCol)
    {
        PushMeOutAgent agent = iCol.collider.GetComponent<PushMeOutAgent>();
        if (!!agent)
        {
            // missile hit !
            agent.HitByMissile(transform.position);
            explode();
            Debug.Log("!!agent");
        }

        PMOTerrainChunk pmotc = iCol.collider.GetComponent<PMOTerrainChunk>();
        if (!!pmotc)
        {
            explode();
            originator.envController.ResolveEvent(PMOEvent.WeaponMiss, originator);
            Debug.Log("!!pmotc");
        }

        Debug.Log(iCol.collider.name);
    }

    void explode()
    {
        GameObject ps = Instantiate<GameObject>(self_explosionPSRef.gameObject);
        ps.transform.position = transform.position;
        Destroy(ps, 2f);

        Destroy(gameObject);
    }

    void MoveTowards(Vector3 targetPos, Rigidbody rb, float targetVel, float maxVel)
    {
        var moveToPos = targetPos - rb.worldCenterOfMass;
        var velocityTarget = Time.fixedDeltaTime * targetVel * moveToPos;
        if (float.IsNaN(velocityTarget.x) == false)
        {
            rb.velocity = Vector3.MoveTowards(
                rb.velocity, velocityTarget, maxVel);
        }
    }
}
