using System;
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
    [SerializeField] private int energyCost;

    public RuntimeSkill(
        SkillId id,
        string displayName,
        SkillCategory category,
        int basePower = 0,
        int hitCount = 1,
        float guardDamageMultiplier = 0.5f,
        float nextAttackDamageBonus = 0f,
        int energyCost = 0)
    {
        this.id = id;
        this.displayName = displayName;
        this.category = category;
        this.basePower = basePower;
        this.hitCount = Mathf.Max(1, hitCount);
        this.guardDamageMultiplier = Mathf.Clamp01(guardDamageMultiplier);
        this.nextAttackDamageBonus = Mathf.Max(0f, nextAttackDamageBonus);
        this.energyCost = Mathf.Max(0, energyCost);
    }

    public SkillId Id                           => id;
    public string DisplayName                   => displayName;
    public SkillCategory Category               => category;
    public int BasePower                        => basePower;
    public int HitCount                         => hitCount;
    public float GuardDamageMultiplier          => guardDamageMultiplier;
    public float NextAttackDamageBonus          => nextAttackDamageBonus;
    public int EnergyCost                        => energyCost;

    public int DamagePerHit
    {
        get
        {
            if (hitCount <= 1) return basePower;
            return Mathf.CeilToInt(basePower / (float)hitCount);
        }
    }

    /// <summary>Permanently increases base attack power (used by accumulator modifiers).</summary>
    public void AddPower(int amount) => basePower += amount;

    /// <summary>Permanently increases hit count.</summary>
    public void AddHitCount(int amount) => hitCount = Mathf.Max(1, hitCount + amount);

    /// <summary>Permanently reduces incoming damage fraction while guarding.</summary>
    public void AddGuardDamageReduction(float amount) =>
        guardDamageMultiplier = Mathf.Clamp01(guardDamageMultiplier - amount);

    /// <summary>Permanently increases the next-attack damage bonus set by Boost.</summary>
    public void AddNextAttackDamageBonus(float amount) => nextAttackDamageBonus += amount;
}
