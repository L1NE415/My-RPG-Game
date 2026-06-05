using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillSet : MonoBehaviour
{
    [Header("Initial Skills")]
    [SerializeField] private PlayerSkillConfig skillConfig;

    [Header("Modifier Library")]
    [SerializeField] private ModifierLibrary modifierLibrary;

    private List<RuntimeSkill> skills = new();
    private SkillModifierTracker modifierTracker;
    private bool initialized;

    private void Awake()
    {
        modifierTracker = GetComponent<SkillModifierTracker>();
        if (modifierTracker == null)
            modifierTracker = gameObject.AddComponent<SkillModifierTracker>();

        InitializeSkills();
    }

    /// <summary>Returns all ModifierDefinitions applied to the given skill this run.</summary>
    public IReadOnlyList<ModifierDefinition> GetAppliedModifiers(SkillId skillId)
        => modifierTracker != null
            ? modifierTracker.GetAll(skillId)
            : System.Array.Empty<ModifierDefinition>();

    /// <summary>Returns the RuntimeSkill matching the given ID, or null if not found.</summary>
    public RuntimeSkill GetSkill(SkillId skillId)
    {
        InitializeSkills();

        foreach (RuntimeSkill skill in skills)
        {
            if (skill.Id == skillId)
                return skill;
        }

        Debug.LogWarning($"PlayerSkillSet: skill not found — {skillId}", this);
        return null;
    }

    /// <summary>
    /// Draws <paramref name="count"/> random modifier options from the library,
    /// restricted to definitions that can still fit into at least one skill slot.
    /// </summary>
    public List<ModifierDefinition> DrawRewardOptions(int count)
    {
        InitializeSkills();

        if (modifierLibrary == null)
        {
            Debug.LogWarning("PlayerSkillSet: ModifierLibrary is not assigned. Cannot draw reward options.", this);
            return new List<ModifierDefinition>();
        }

        List<ModifierDefinition> eligible = new();
        foreach (ModifierDefinition def in modifierLibrary.Definitions)
        {
            foreach (RuntimeSkill skill in skills)
            {
                if (skill.Category == def.ApplicableCategory && modifierTracker.CanAdd(skill.Id, def))
                {
                    eligible.Add(def);
                    break;
                }
            }
        }

        if (eligible.Count == 0)
        {
            Debug.LogWarning("PlayerSkillSet: No eligible modifiers found — all skill slots may be full.", this);
            return eligible;
        }

        // Fisher-Yates shuffle, then take the first 'count' entries.
        for (int i = eligible.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (eligible[i], eligible[j]) = (eligible[j], eligible[i]);
        }

        return eligible.GetRange(0, Mathf.Min(count, eligible.Count));
    }

    /// <summary>
    /// Applies a modifier to the first skill whose category matches and that still has an open slot.
    /// Stat-mutation types (FlatPowerBoost, HitCountIncrease, FlatGuardReduction, NextAttackDamageBoost)
    /// permanently update the RuntimeSkill immediately on application.
    /// Returns a log string describing the result.
    /// </summary>
    public string ApplyModifier(ModifierDefinition def)
    {
        if (def == null) return "No modifier provided.";
        InitializeSkills();

        RuntimeSkill target = FindSkillForModifier(def);
        if (target == null)
            return $"No available slot for '{def.DisplayName}' (Category: {def.ApplicableCategory}).";

        modifierTracker.Add(target.Id, def);
        ApplyStatMutation(target, def);
        return $"'{def.DisplayName}' applied to {target.DisplayName}.";
    }

    private static void ApplyStatMutation(RuntimeSkill skill, ModifierDefinition def)
    {
        switch (def.EffectType)
        {
            case ModifierEffectType.FlatPowerBoost:
                skill.AddPower((int)def.PrimaryValue);
                break;
            case ModifierEffectType.HitCountIncrease:
                skill.AddHitCount((int)def.PrimaryValue);
                break;
            case ModifierEffectType.FlatGuardReduction:
                skill.AddGuardDamageReduction(def.PrimaryValue);
                break;
            case ModifierEffectType.NextAttackDamageBoost:
                skill.AddNextAttackDamageBonus(def.PrimaryValue);
                break;
        }
    }

    private RuntimeSkill FindSkillForModifier(ModifierDefinition def)
    {
        foreach (RuntimeSkill skill in skills)
        {
            if (skill.Category == def.ApplicableCategory && modifierTracker.CanAdd(skill.Id, def))
                return skill;
        }
        return null;
    }

    private void InitializeSkills()
    {
        if (initialized) return;

        if (skillConfig != null)
        {
            skills = skillConfig.BuildAll();
        }
        else
        {
            Debug.LogWarning("PlayerSkillSet: PlayerSkillConfig is not assigned — falling back to hardcoded defaults.", this);
            skills = BuildDefaultSkills();
        }

        initialized = true;
    }

    private static List<RuntimeSkill> BuildDefaultSkills() => new()
    {
        new RuntimeSkill(SkillId.Attack,      "Attack",        SkillCategory.Attack,  basePower: 20),
        new RuntimeSkill(SkillId.DoubleAttack, "Double-Attack", SkillCategory.Attack,  basePower: 40, hitCount: 2),
        new RuntimeSkill(SkillId.Guard,        "Guard",         SkillCategory.Defense, guardDamageMultiplier: 0.5f),
        new RuntimeSkill(SkillId.Boost,        "Boost",         SkillCategory.Status),
    };
}
