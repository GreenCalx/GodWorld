using System;
using UnityEngine;

public class NodeActionPool<T> where T : AgentToken
{
    public T target;
    public virtual void Freeze() {}
    public virtual void UnFreeze() {}
    public virtual void StartTrainingFX() {}
    public virtual void StopTrainingFX() {}
    public virtual void UpdateNeedsFromState() {}
}

public class PetNodeActionPool : NodeActionPool<PetToken>
{
    public override void Freeze()
    {
        target.pet.petStatus.isFrozen = true;
    }
    public override void UnFreeze()
    {
        target.pet.petStatus.isFrozen = false;
    }

    public override void StartTrainingFX()
    {
        //target.pet.
    }
    public override void StopTrainingFX()
    {
        //target.pet.
    }

    public override void UpdateNeedsFromState() 
    {
        GWSettings settings = GWSettings.Instance;
        switch(target.pet.currState)
        {
            case GWAState.ALIVE:
                target.pet.petNeeds.hungerGain  = settings.agentHungerLossPerSec * -1f;
                target.pet.petNeeds.thirstGain  = settings.agentThirstLossPerSec * -1f;
                target.pet.petNeeds.fatigueGain = settings.agentFatigueLossPerSec * -1f;
                break;
            case GWAState.TRAINING:
                target.pet.petNeeds.hungerGain  = settings.agentHungerLossPerSec * -1f;
                target.pet.petNeeds.thirstGain  = settings.agentThirstLossPerSec * -1f;
                target.pet.petNeeds.fatigueGain = settings.agentFatigueLossPerSec * settings.agentTrainFatigueMul * -1f;
                break;
            case GWAState.ASLEEP:
                target.pet.petNeeds.hungerGain  = settings.agentHungerLossPerSec * settings.agentSleepNeedsReductionFactor * -1f;
                target.pet.petNeeds.thirstGain  = settings.agentThirstLossPerSec * settings.agentSleepNeedsReductionFactor * -1f;
                target.pet.petNeeds.fatigueGain = settings.agentFatigueRegenPerSec;
                break;
            default:
                break;
        }
    }

}