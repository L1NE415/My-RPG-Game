using System;

[Serializable]
public class SkillModifier
{
    public SkillModifier(SkillRewardType rewardType, string description, float value)
    {
        RewardType = rewardType;
        Description = description;
        Value = value;
    }

    public SkillRewardType RewardType;
    public string Description;
    public float Value;
}
