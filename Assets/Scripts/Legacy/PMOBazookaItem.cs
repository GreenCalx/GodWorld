using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


// Maybe ?
public class PMOBazookaItem  : PMOItem
{
    public PMOWeapon bazooka;
    public override void OnCollect(PushMeOutAgent iAgent)
    {
        iAgent.equipWeapon(bazooka);
    }
}

