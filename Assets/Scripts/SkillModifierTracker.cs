using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks all ModifierDefinitions applied to the player's skills during the current run.
/// Enforces per-skill slot limits (3 regular + 1 counter slot per skill).
/// Attach this to the same GameObject as PlayerSkillSet.
/// </summary>
public class SkillModifierTracker : MonoBehaviour
{
    private const int MaxRegularSlots = 3;
    private const int MaxCounterSlots = 1;

    private readonly Dictionary<SkillId, List<ModifierDefinition>> _applied = new();

    /// <summary>Returns true when the given skill still has a free slot for the modifier.</summary>
    public bool CanAdd(SkillId skillId, ModifierDefinition def)
    {
        if (def == null) return false;

        int used = CountSlots(skillId, def.IsCounterSlot);
        int limit = def.IsCounterSlot ? MaxCounterSlots : MaxRegularSlots;
        return used < limit;
    }

    /// <summary>
    /// Records the modifier as applied to the given skill.
    /// Returns true on success, false when the slot is already full.
    /// </summary>
    public bool Add(SkillId skillId, ModifierDefinition def)
    {
        if (!CanAdd(skillId, def)) return false;

        GetOrCreate(skillId).Add(def);
        return true;
    }

    /// <summary>Returns all modifiers recorded for the given skill this run.</summary>
    public IReadOnlyList<ModifierDefinition> GetAll(SkillId skillId)
    {
        return _applied.TryGetValue(skillId, out List<ModifierDefinition> list)
            ? list
            : System.Array.Empty<ModifierDefinition>();
    }

    /// <summary>Clears all recorded modifiers. Call this when starting a new run.</summary>
    public void Clear() => _applied.Clear();

    private int CountSlots(SkillId skillId, bool counterSlot)
    {
        if (!_applied.TryGetValue(skillId, out List<ModifierDefinition> list))
            return 0;

        int count = 0;
        foreach (ModifierDefinition m in list)
        {
            if (m.IsCounterSlot == counterSlot) count++;
        }
        return count;
    }

    private List<ModifierDefinition> GetOrCreate(SkillId skillId)
    {
        if (!_applied.TryGetValue(skillId, out List<ModifierDefinition> list))
        {
            list = new List<ModifierDefinition>();
            _applied[skillId] = list;
        }
        return list;
    }
}
