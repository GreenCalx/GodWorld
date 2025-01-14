using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GWItemSpawnSlot
{
    public Transform spawnPos;
    public GWItem item;

    public GWItemSpawnSlot(Transform iSpawnPos)
    {
        spawnPos = iSpawnPos;
        item = null;
    }
    public bool IsFree()
    {
        return item == null;
    }
}

public class GWItemSpawner : GWBuilding
{
    //public int maxSpawn = 2;
    public bool spawnOnePerSpawnLoc;
    public List<Transform> spawnLocations;
    public GWItem prefab_spawnableItem;
    public float itemSpawnRate = 10f;
    private float elapsedItemSpawnTime = 0f;
    public GWEnvController envController;
    public bool spawnOnStart = true;
    [Header("Randomization Tweaks")]
    public bool alwaysFaceCenter = true;
    [Header("Internals")]
    public List<GWItemSpawnSlot> slots; 

    void Start() 
    { 
        // env controller calls reset 
    }

    public void Reset()
    {
        if (alwaysFaceCenter)
        {
            transform.LookAt(Vector3.zero, Vector3.up);
        } else {
            var randomRot = Random.Range(0, 360f);
            transform.eulerAngles = new Vector3(0, randomRot, 0);
        }
        
        slots = new List<GWItemSpawnSlot>();
        foreach(Transform t in spawnLocations)
        {
            slots.Add(new GWItemSpawnSlot(t));
        }
        if (spawnOnStart)
            SpawnItems();

        elapsedItemSpawnTime = 0f;
    }

    void FixedUpdate()
    {
        elapsedItemSpawnTime += Time.fixedDeltaTime;
        if (elapsedItemSpawnTime >= itemSpawnRate)
        {
            SpawnItems();
            elapsedItemSpawnTime = 0f;
        }
    }

    void SpawnItems()
    {
        foreach(GWItemSpawnSlot slot in slots)
        {
            if (!slot.IsFree())
                continue;
            
            GWItem newItem = Instantiate(prefab_spawnableItem);
            Rigidbody rb = newItem.GetComponent<Rigidbody>();
            if (rb!=null)
            {
                rb.useGravity = false;
            }
            newItem.transform.parent = transform;
            newItem.transform.localPosition = slot.spawnPos.localPosition;
            newItem.originator = this;
            newItem.env = envController;

            slot.item = newItem;
            envController.spawnedItems.Add(newItem);
            if (rb!=null)
            {
                rb.useGravity = true;
            }
        }
    }

    public void notifySpawnItemDestroyed(GWItem iItem)
    {
        if (iItem.originator!=this)
            return;
        foreach(GWItemSpawnSlot slot in slots)
        {
            if (slot.item==iItem)
            {
                slot.item = null;
                return;
            }
        }
    }

}