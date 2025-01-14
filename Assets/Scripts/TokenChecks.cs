using System;
using UnityEngine;
using UnityEngine.Events;

public class TokenChecks<T> where T : AgentToken
{ 
    public T target;
    public TokenChecks() {}
}

public class AgentChecks : TokenChecks<AgentToken>
{
    public AgentChecks(AgentToken iTok) 
    {
        target = iTok;
    }
}

public class PetChecks : TokenChecks<PetToken>
{
    public PetChecks()
    {
        target = null;
    }
    public PetChecks(PetToken iTarget)
    {
        target = iTarget;
    }

    public bool DeathCond()
    {
        if (target==null)
            return false;

        return ((target.pet.petNeeds.currentHunger <= 0f)||(target.pet.petNeeds.currentThirst <= 0f));
    }

    public bool SleepCond()
    {
        if (target==null)
            return false;
        return (target.pet.agentJob == GWAPetTask.GO_SLEEP);
    }

    public bool SleepOverCond()
    {
        if (target==null)
            return false;
        return (target.pet.agentJob != GWAPetTask.GO_SLEEP);
    }

    public bool TrainCond()
    {
        if (target==null)
            return false;
        return (target.pet.agentJob == GWAPetTask.GO_TRAIN);
    }

    public bool TrainingOverCond()
    {
        if (target==null)
            return false;
        return (target.pet.agentJob != GWAPetTask.GO_TRAIN);
    }
}