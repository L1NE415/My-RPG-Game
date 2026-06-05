using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ScriptableObject asset that holds every possible modifier definition.
/// Create via: Assets > Create > RPG > Modifier Library.
/// Right-click the asset in the Inspector and choose "Populate from GDD Defaults"
/// to fill the list with all GDD-defined modifiers using tunable starting values.
/// </summary>
[CreateAssetMenu(fileName = "ModifierLibrary", menuName = "RPG/Modifier Library")]
public class ModifierLibrary : ScriptableObject
{
    [SerializeField] private List<ModifierDefinition> _definitions = new();

    public IReadOnlyList<ModifierDefinition> Definitions => _definitions;

    /// <summary>Returns all modifiers valid for the given category and slot type.</summary>
    public List<ModifierDefinition> GetPool(SkillCategory category, bool isCounterSlot)
    {
        return _definitions
            .Where(d => d.ApplicableCategory == category && d.IsCounterSlot == isCounterSlot)
            .ToList();
    }

    /// <summary>
    /// Draws <paramref name="count"/> unique modifiers at random from the matching pool.
    /// Returns fewer than <paramref name="count"/> if the pool is smaller.
    /// </summary>
    public List<ModifierDefinition> DrawUnique(SkillCategory category, bool isCounterSlot, int count)
    {
        List<ModifierDefinition> pool = GetPool(category, isCounterSlot);

        if (pool.Count == 0)
        {
            Debug.LogWarning(
                $"ModifierLibrary: no modifiers in pool for {category} " +
                $"(counterSlot={isCounterSlot}). Returning empty list.", this);
            return pool;
        }

        // Fisher-Yates in-place shuffle, then take first 'count'
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Take(Mathf.Min(count, pool.Count)).ToList();
    }

#if UNITY_EDITOR
    [ContextMenu("Populate from GDD Defaults")]
    private void PopulateFromGddDefaults()
    {
        _definitions.Clear();

        // ── Attack / Regular (Phase 1) ────────────────────────────────────────
        Add(ModifierEffectType.FlatPowerBoost, SkillCategory.Attack, false,
            "Power Up",
            "Attack power +20.",
            primaryValue: 20);

        Add(ModifierEffectType.HitCountIncrease, SkillCategory.Attack, false,
            "Extra Hit",
            "Gain 1 additional hit.",
            primaryValue: 1);

        Add(ModifierEffectType.HighHpPowerBoost, SkillCategory.Attack, false,
            "Dominant Power",
            "If HP > 50%, attack power +20.",
            primaryValue: 20, secondaryValue: 0.50f);

        Add(ModifierEffectType.LowHpPowerBoost, SkillCategory.Attack, false,
            "Desperation Power",
            "For every 5% HP lost, attack power +5%.",
            primaryValue: 0.05f, secondaryValue: 0.05f);

        Add(ModifierEffectType.Lifesteal, SkillCategory.Attack, false,
            "Drain",
            "Recover 30% of damage dealt as HP.",
            primaryValue: 0.30f);

        // ── Defense / Regular (Phase 1) ───────────────────────────────────────
        Add(ModifierEffectType.FlatGuardReduction, SkillCategory.Defense, false,
            "Hardened Guard",
            "Reduce damage taken while guarding by an additional 20%.",
            primaryValue: 0.20f);

        // ── Status / Regular (Phase 1) ────────────────────────────────────────
        Add(ModifierEffectType.NextAttackDamageBoost, SkillCategory.Status, false,
            "Empowered Boost",
            "Boost grants an additional +50% to the next attack.",
            primaryValue: 0.50f);

        Add(ModifierEffectType.NextAttackDamageBoost, SkillCategory.Status, false,
            "Greater Boost",
            "Boost grants an additional +100% to the next attack.",
            primaryValue: 1.00f);

        // ── Counter slots (Phase 4 — requires Counter detection) ──────────────
        // Add entries here once the Counter triangle is implemented.

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"ModifierLibrary: populated {_definitions.Count} modifiers (Phase 1 only).", this);
    }

    private void Add(
        ModifierEffectType effectType,
        SkillCategory applicableCategory,
        bool isCounterSlot,
        string displayName,
        string description,
        float primaryValue = 0f,
        float secondaryValue = 0f,
        StatusEffectType targetStatus = StatusEffectType.Burn)
    {
        _definitions.Add(new ModifierDefinition(
            effectType, applicableCategory, isCounterSlot,
            displayName, description,
            primaryValue, secondaryValue, targetStatus));
    }
#endif
}
