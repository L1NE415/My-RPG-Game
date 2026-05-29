using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RuntimeSkill
{
    [SerializeField] private SkillId id;
    [SerializeField] private string displayName;
    [SerializeField] private SkillCategory category;
    [SerializeField] private int basePower;
    [SerializeField] private int hitCount = 1;
    [SerializeField] private float guardDamageMultiplier = 0.5f;
    [SerializeField] private float nextAttackDamageBonus;
    [SerializeField] private List<SkillModifier> modifiers = new();

    public RuntimeSkill(
        SkillId id,
        string displayName,
        SkillCategory category,
        int basePower = 0,
        int hitCount = 1,
        float guardDamageMultiplier = 0.5f,
        float nextAttackDamageBonus = 0f)
    {
        this.id = id;
        this.displayName = displayName;
        this.category = category;
        this.basePower = basePower;
        this.hitCount = Mathf.Max(1, hitCount);
        this.guardDamageMultiplier = Mathf.Clamp01(guardDamageMultiplier);
        this.nextAttackDamageBonus = Mathf.Max(0f, nextAttackDamageBonus);
    }

    public SkillId Id                           => id;
    public string DisplayName                   => displayName;
    public SkillCategory Category               => category;
    public int BasePower                        => basePower;
    public int HitCount                         => hitCount;
    public float GuardDamageMultiplier          => guardDamageMultiplier;
    public float NextAttackDamageBonus          => nextAttackDamageBonus;
    public IReadOnlyList<SkillModifier> Modifiers => modifiers;

    public int DamagePerHit
    {
        get
        {
            if (hitCount <= 1) return basePower;
            return Mathf.CeilToInt(basePower / (float)hitCount);
        }
    }

    /// <summary>Increases base attack power and records the change.</summary>
    public void AddPower(int amount)
    {
        basePower += amount;
        modifiers.Add(new SkillModifier(SkillRewardType.AttackDamage, $"Power +{amount}", amount));
    }

    /// <summary>Increases hit count and records the change.</summary>
    public void AddHitCount(int amount)
    {
        hitCount = Mathf.Max(1, hitCount + amount);
        modifiers.Add(new SkillModifier(SkillRewardType.DoubleAttackHitCount, $"Hit Count +{amount}", amount));
    }

    /// <summary>Reduces incoming damage while guarding and records the change.</summary>
    public void AddGuardDamageReduction(float amount)
    {
        guardDamageMultiplier = Mathf.Clamp01(guardDamageMultiplier - amount);
        modifiers.Add(new SkillModifier(SkillRewardType.GuardDamageReduction, $"Guard Damage Reduction +{Mathf.RoundToInt(amount * 100f)}%", amount));
    }

    /// <summary>Increases the bonus applied to the next attack and records the change.</summary>
    public void AddNextAttackDamageBonus(float amount)
    {
        nextAttackDamageBonus += amount;
        modifiers.Add(new SkillModifier(SkillRewardType.BoostAttackPower, $"Next Attack Damage +{Mathf.RoundToInt(amount * 100f)}%", amount));
    }
}
