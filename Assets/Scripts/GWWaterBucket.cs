using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


// Maybe ?
public class GWWaterBucket  : GWItem
{
    public float waterValue = 10f;
    void Start()
    {
        itemType = GWItemType.WATER;
    }
    public override bool OnCollect(GWPet iAgent)
    {
        //iAgent.CollectFood(this);
        return iAgent.TryCarryItem(this);
    }

    public override bool OnUse(GWPet iAgent)
    {
        iAgent.CollectWater(this);
        return true;
    }
}

