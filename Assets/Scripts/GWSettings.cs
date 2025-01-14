using UnityEngine;

public class GWSettings : MonoBehaviour
{
    private static GWSettings instance = null;
    public static GWSettings Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    [Header("AgentTweaks")]
    public float agentRunSpeed = 1.5f;
    public float agentMaxRunVel = 10f;
    public float agentTurnSpeed = 100f;
    public float agentJumpHeight = 2.75f;
    public float agentJumpVelocity = 777;
    public float agentJumpVelocityMaxChange = 10;
    public float speedReductionFactor = 0.75f;
    public float jumpCooldown = 0f;
    public float fallingForce = 150;
    public float carriedItemYOffset = 2f;
    public float dropItemFwdOffset = 2f;
    public float actionCooldown = 0.2f;
    [Header("Fatigue")]
    public float agentTotalFatigue = 100f;
    public float agentFatigueLossPerSec = 0.01f;
    public float agentIsFatiguedPercent = 0.5f;
    public float agentFatigueRegenPerSec = 0.5f;
    public float agentSleepNeedsReductionFactor = 0.2f;
    public float agentTrainFatigueMul = 1.5f;
    [Header("Hunger")]
    public float agentTotalHunger = 100f;
    public float agentHungerLossPerSec = 0.1f;
    public float agentIsHungryPercent = 0.5f;
    [Header("Thirst")]
    public float agentTotalThirst = 100f;
    public float agentThirstLossPerSec = 0.1f;
    public float agentIsThirstyPercent = 0.5f;
    [Header("Training")]
    public float trainingDurationInSec = 10f;
    public int trainingStatGain = 1; 
    
    [Header("Learning tweaks")]
    public float timeIntervalForEnvReward = 10f;
    public float minTravelDistForExploringReward = 2.5f;
}