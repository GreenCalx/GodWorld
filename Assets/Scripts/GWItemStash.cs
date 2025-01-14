using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GWItemStash : MonoBehaviour
{
    [Header("Mand prefab refs")]
    public GameObject prefabFoodStack;
    public GameObject prefabWaterStack;
    [Header("Manual Refs")]
    public GWEnvController envController;
    public GWPositionRandomizer positionRandomizer;
    //public List<GWItem> stashedItems;
    [Header("Internals")]
    public List<GWItemStack> stashedItems;

    public float minDistToFoodSpawners = 10f;

    // Start is called before the first frame update
    void Start()
    {
        // env controller calls reset
    }

    public void Reset()
    {
        var randomRot = Random.Range(0, 360f);
        transform.eulerAngles = new Vector3(0, randomRot, 0);
        
        // Temp logic while there is no builder..
        
        bool isTooCloseOfSpawner = true;
        
        while (isTooCloseOfSpawner) {
            transform.localPosition = positionRandomizer.GetRandPosition();
            foreach(GWItemSpawner s in envController.foodSpawners)
            {
                isTooCloseOfSpawner = false;
                if (Vector3.Distance(s.transform.localPosition, transform.localPosition) < minDistToFoodSpawners)
                {
                    isTooCloseOfSpawner = true;
                }
            }
        }
        foreach(GWItemStack stack in stashedItems)
        {
            Destroy(stack.gameObject);
        }
        stashedItems = new List<GWItemStack>();
    }

    public int GetItemStashCount(GWItemType iItemType)
    {
        foreach(GWItemStack stack in stashedItems)
        {
            if (stack.item.itemType == iItemType)
                return stack.count;
        }
        return 0;
    }

    void OnTriggerEnter(Collider iCollider)
    {
        GWItem as_item = iCollider.GetComponent<GWItem>();
        if (!!as_item)
        {
            if (as_item.collected)
                return; // still held by agent
            TryStash(as_item);
        }

        GWPet gwa = iCollider.GetComponent<GWPet>() ;
        if (!!gwa)
        {
            //gwa.IsInStash = true;
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        GWItem as_item = iCollider.GetComponent<GWItem>();
        if (!!as_item)
        {
            if (as_item.collected)
                return; // still held by agent

            TryStash(as_item);
        }
    }

    void OnTriggerExit(Collider iCollider)
    {
        GWPet gwa = iCollider.GetComponent<GWPet>() ;
        if (!!gwa)
        {
            //gwa.IsInStash = false;
        }
    }

    public void TryStash(GWItem iItem)
    {
        if (iItem.stashed)
            return;
        if (iItem.collected)
            return;

        iItem.stashed = true;
        GWPet lastCarrier = iItem.lastCarrier;
        GWItemStack targetStack = null;
        foreach(GWItemStack stack in stashedItems)
        {
            if (stack.item.itemType == iItem.itemType)
            {
                targetStack = stack;
                break;
            }
        }
        if (targetStack==null)
        {
            targetStack = MakeStack(iItem);
            envController.ResolveEvent(GWEvent.ItemStackCreated, lastCarrier);
        }

        if (targetStack.AddToStack(iItem))
        {
            if (envController.spawnedItems.Contains(iItem))
                envController.spawnedItems.Remove(iItem);
        }
        
    }

    public GWItemStack MakeStack(GWItem iItem)
    {
        GWItemStack newStack = null;
        switch(iItem.itemType)
        {
            case GWItemType.FOOD:
                newStack = Instantiate(prefabFoodStack).GetComponent<GWItemStack>();
                break;
            case GWItemType.WATER:
                newStack = Instantiate(prefabWaterStack).GetComponent<GWItemStack>();
                break;
            default:
                break;
        }
        newStack.transform.parent = iItem.transform.parent;
        newStack.transform.localPosition = iItem.transform.localPosition;
        newStack.transform.parent = transform;
        newStack.envController = envController;
        stashedItems.Add(newStack);

        return newStack;
    }

    public void DeleteStack(GWItemStack iItemStack)
    {
        if (stashedItems.Contains(iItemStack))
            stashedItems.Remove(iItemStack);
        stashedItems = stashedItems.Where(x=>x!=null).ToList();
    }

    public GWItem RequestFood(GWPet iAgent)
    {
        return TryUnstash(iAgent, GWItemType.FOOD);
    }
    public GWItem RequestWater(GWPet iAgent)
    {
        return TryUnstash(iAgent, GWItemType.WATER);
    }

    public GWItem TryUnstash(GWPet iAgent, GWItemType iItemType)
    {
        GWItem retval = null;
        foreach(GWItemStack stack in stashedItems)
        {
            if (stack.item.itemType == iItemType)
            {
                retval = stack.GetFromStack();
                break;
            }
        }
        if (retval==null)
        {
            // no stack available
            return retval;
        }

        retval.hasBeenStashed = true;
        retval.stashed = false;
        return retval;  
    }

}
