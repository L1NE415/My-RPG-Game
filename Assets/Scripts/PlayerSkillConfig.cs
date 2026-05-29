using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines the player's four starting skills and their base stats.
/// Create via: Assets > Create > RPG > Player Skill Config.
/// Right-click the asset in the Inspector and choose "Populate Default Skills" to fill it
/// with the standard starting values.
/// </summary>
[CreateAssetMenu(fileName = "PlayerSkillConfig", menuName = "RPG/Player Skill Config")]
public class PlayerSkillConfig : ScriptableObject
{
    [SerializeField] private List<SkillInitData> _skills = new();

    public IReadOnlyList<SkillInitData> Skills => _skills;

    /// <summary>Builds a RuntimeSkill for the given ID. Returns null if no matching entry exists.</summary>
    public RuntimeSkill BuildSkill(SkillId id)
    {
        foreach (SkillInitData data in _skills)
        {
            if (data.Id == id)
                return data.ToRuntimeSkill();
        }

        Debug.LogWarning($"PlayerSkillConfig: no init data found for skill '{id}'.", this);
        return null;
    }

    /// <summary>Builds a RuntimeSkill for every entry in the config list, in order.</summary>
    public List<RuntimeSkill> BuildAll()
    {
        List<RuntimeSkill> result = new(_skills.Count);
        foreach (SkillInitData data in _skills)
            result.Add(data.ToRuntimeSkill());
        return result;
    }

#if UNITY_EDITOR
    [ContextMenu("Populate Default Skills")]
    private void PopulateDefaults()
    {
        _skills = new List<SkillInitData>
        {
            new() { Id = SkillId.Attack,       DisplayName = "Attack",        Category = SkillCategory.Attack,  BasePower = 20, HitCount = 1 },
            new() { Id = SkillId.DoubleAttack,  DisplayName = "Double-Attack", Category = SkillCategory.Attack,  BasePower = 40, HitCount = 2 },
            new() { Id = SkillId.Guard,         DisplayName = "Guard",         Category = SkillCategory.Defense, GuardDamageMultiplier = 0.5f },
            new() { Id = SkillId.Boost,         DisplayName = "Boost",         Category = SkillCategory.Status  },
        };
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"PlayerSkillConfig: populated {_skills.Count} default skills.", this);
    }
#endif
}

/// <summary>Serializable data bag that defines one skill's starting parameters.</summary>
[Serializable]
public class SkillInitData
{
    public SkillId Id;
    public string DisplayName;
    public SkillCategory Category;
    public int BasePower;
    [Min(1)] public int HitCount = 1;
    [Range(0f, 1f)] public float GuardDamageMultiplier = 0.5f;
    [Min(0f)] public float NextAttackDamageBonus;

    /// <summary>Creates a new RuntimeSkill from this init data.</summary>
    public RuntimeSkill ToRuntimeSkill() =>
        new(Id, DisplayName, Category, BasePower, HitCount, GuardDamageMultiplier, NextAttackDamageBonus);
}
