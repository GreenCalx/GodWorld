using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum PMOWeaponType { NONE=0, BAZOOKA=1}
public abstract class PMOWeapon : MonoBehaviour
{
    public PMOWeaponType weaponType;
    public PushMeOutAgent holder;
    public int maxAmmo = 3;
    public int currAmmo = 0;

    public virtual bool TryFire() { return true; }
}