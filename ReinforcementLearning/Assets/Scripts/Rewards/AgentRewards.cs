using Unity.MLAgents;
public class AgentRewards
{
    public void AddHPReward(TankAgent agent, int maxHp, int currentHP) {
        float recivedHP = maxHp - currentHP;
        if (recivedHP != 0) {          
            float calculateReward = recivedHP / maxHp;
            agent.AddReward(calculateReward);
        }
    }
    public void AddAmmoReward(TankAgent agent, int maxAmmo, int currentAmmo) {
        float recivedAmmo = maxAmmo - currentAmmo;
        if (recivedAmmo != 0) {          
            float calculateReward = recivedAmmo / maxAmmo;
            agent.AddReward(calculateReward);
        }
    }
    public void AddTeamMateDistancePenalty(SimpleMultiAgentGroup group, float currentDistance, float maxDistance) {
        if (maxDistance < currentDistance) {
            group.AddGroupReward(-((currentDistance - maxDistance) / 100));
        }   
    }
    public void AddBulletHitReward(TankAgent agent, float reward) { 
        agent.AddReward(reward);
    }
    public void AddBulletHitPenalty(TankAgent agentThatGotHit, float reward) {
        agentThatGotHit.AddReward(-reward);
    }
    public void AddMissedShotPenalty(TankAgent agent, float missedShoots, float maxBullet) {
        float calculateReward = -(missedShoots / (maxBullet * 10));
        if (calculateReward > 0.1) {
            agent.AddReward(0.1f);
        } else {
            agent.AddReward(calculateReward);
        }       
    }
    public void AddNegativPositivRewards(SimpleMultiAgentGroup winner, SimpleMultiAgentGroup loser, float reward) {
        winner.AddGroupReward(reward);
        loser.AddGroupReward(-reward);
    }
    public void AddStepPenalty(TankAgent agent, float stepCount, float maxStep) {
        float calculateReward = -(stepCount / (maxStep * 100));
        if (calculateReward > 0.05) {
            agent.AddReward(0.05f);
        } else {
            agent.AddReward(calculateReward);
        }
        
    }
    public string GetAgentsReward(SimpleMultiAgentGroup group) {
        if(group == null) {
            return null;
        }
        string uzenet = null;
        foreach (var agent in group.GetRegisteredAgents()) {
            uzenet += $"{agent.name} {agent.GetCumulativeReward()} rewardot szerzett! " ;
        }
        return uzenet;
    }
}
