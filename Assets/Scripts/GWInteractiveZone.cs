using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum INTERACTION
{
    NONE=0,
    TRAIN=1
}

public class GWInteractiveZone : GWBuilding
{
    public GWSettings gwSettings;
    public GWEnvController gWEnvController;
    public INTERACTION interactType;
    public bool interactionOngoing = false;
    public virtual bool interact() { return false; }
    public virtual int GetZoneType() {return 0;}
}