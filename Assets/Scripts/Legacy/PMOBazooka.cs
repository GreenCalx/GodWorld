using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMOBazooka : PMOWeapon
{
    public GameObject self_missileRef;
    private GameObject self_missileInst;
    public Transform self_shootOrigin;

    public float shootingForce = 100f;
    public float cooldown = 2f;

    private float elapsedTimeSinceLastShot = 0f;

    // Start is called before the first frame update
    void Start()
    {
        elapsedTimeSinceLastShot = 0f;
        currAmmo = maxAmmo;
        weaponType = PMOWeaponType.BAZOOKA;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        elapsedTimeSinceLastShot += Time.fixedDeltaTime;
    }

    public override bool TryFire()
    {
        if (elapsedTimeSinceLastShot < cooldown)
            return false;
        
        if (currAmmo==0)
            return false;
        
        if (!!self_missileInst)
        {
            Destroy(self_missileInst);
        }

        self_missileInst = Instantiate(self_missileRef);
        self_missileInst.transform.position = self_shootOrigin.position;
        self_missileInst.transform.rotation = transform.rotation;
        Vector3 shootDir = self_shootOrigin.position + (transform.parent.transform.forward* shootingForce); 

        PMOBazookaMissile m = self_missileInst.GetComponent<PMOBazookaMissile>();
        m.originator = this.holder;
        m.direction = shootDir;

        // Debug shoot dir
        //Debug.DrawRay(self_shootOrigin.position, transform.parent.transform.forward * shootingForce, Color.red, 1f);

        // Rigidbody rb = self_missileInst.GetComponent<Rigidbody>();
        // if (!!rb)
        // {
        //     rb.AddForce(shootDir.normalized * shootingForce, ForceMode.VelocityChange );
        // }
        elapsedTimeSinceLastShot = 0f;
        currAmmo--;
        
        return true;
    }
}
