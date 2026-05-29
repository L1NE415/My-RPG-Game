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

        // ── Attack / Regular ─────────────────────────────────────────────
        Add(ModifierEffectType.EnergyRestoreOnUse, SkillCategory.Attack, false,
            "Energy on Use",
            "Restore 1 energy when this skill is used.",
            primaryValue: 1);

        Add(ModifierEffectType.LowHpCostReduction, SkillCategory.Attack, false,
            "Desperation Cost",
            "For every 10% HP lost, this skill's cost −1.",
            primaryValue: 1, secondaryValue: 0.10f);

        Add(ModifierEffectType.LowHpPowerBoost, SkillCategory.Attack, false,
            "Desperation Power",
            "For every 5% HP lost, skill power +5%.",
            primaryValue: 0.05f, secondaryValue: 0.05f);

        Add(ModifierEffectType.HighHpPowerBoost, SkillCategory.Attack, false,
            "Dominant Power",
            "If HP > 50%, skill power +20.",
            primaryValue: 20, secondaryValue: 0.50f);

        Add(ModifierEffectType.CounterFollowUpPower, SkillCategory.Attack, false,
            "Counter Follow-Up",
            "If a counter succeeded last turn, skill power +20.",
            primaryValue: 20);

        Add(ModifierEffectType.GuardFollowUpPower, SkillCategory.Attack, false,
            "Guard Follow-Up",
            "If a Defense skill was used last turn, skill power +20.",
            primaryValue: 20);

        Add(ModifierEffectType.CounterAccumulatorPower, SkillCategory.Attack, false,
            "Counter Fury",
            "Each successful counter: skill power permanently +5.",
            primaryValue: 5);

        Add(ModifierEffectType.UseAccumulatorPower, SkillCategory.Attack, false,
            "Practiced Strike",
            "Each use: skill power permanently +2.",
            primaryValue: 2);

        Add(ModifierEffectType.KillAccumulatorPower, SkillCategory.Attack, false,
            "Hunter's Edge",
            "Each enemy defeated: skill power permanently +10.",
            primaryValue: 10);

        Add(ModifierEffectType.KillEnergyRestore, SkillCategory.Attack, false,
            "Kill Energy",
            "If this skill defeats an enemy, restore 2 energy.",
            primaryValue: 2);

        Add(ModifierEffectType.ApplyStatus, SkillCategory.Attack, false,
            "Ignite",
            "Apply 2 Burn stacks to the enemy.",
            primaryValue: 2, targetStatus: StatusEffectType.Burn);

        Add(ModifierEffectType.ApplyStatus, SkillCategory.Attack, false,
            "Envenom",
            "Apply 2 Poison stacks to the enemy.",
            primaryValue: 2, targetStatus: StatusEffectType.Poison);

        Add(ModifierEffectType.ApplyStatus, SkillCategory.Attack, false,
            "Chill",
            "Apply 2 Freeze stacks to the enemy.",
            primaryValue: 2, targetStatus: StatusEffectType.Freeze);

        Add(ModifierEffectType.PerHitApplyStatus, SkillCategory.Attack, false,
            "Multi-Ignite",
            "Each hit applies 1 Burn stack.",
            primaryValue: 1, targetStatus: StatusEffectType.Burn);

        Add(ModifierEffectType.StatusSynergyPower, SkillCategory.Attack, false,
            "Status Exploit",
            "+10 power per stack of any status effect on the enemy.",
            primaryValue: 10);

        // ── Attack / Counter ─────────────────────────────────────────────
        Add(ModifierEffectType.CounterPowerSurge, SkillCategory.Attack, true,
            "Counter Burst",
            "On counter: damage +100%. If no counter: −50%.",
            primaryValue: 1.00f, secondaryValue: 0.50f);

        Add(ModifierEffectType.InterruptSkill, SkillCategory.Attack, true,
            "Counter Interrupt",
            "Cancel the enemy's countered skill before it resolves.");

        Add(ModifierEffectType.ExtraAction, SkillCategory.Attack, true,
            "Swift Counter",
            "Gain 1 extra action this turn.",
            primaryValue: 1);

        Add(ModifierEffectType.StunEnemy, SkillCategory.Attack, true,
            "Counter Stun",
            "Stun the enemy for 1 turn.",
            primaryValue: 1);

        Add(ModifierEffectType.HitCountIncrease, SkillCategory.Attack, true,
            "Extra Hit",
            "Increase hit count by 1 on counter.",
            primaryValue: 1);

        Add(ModifierEffectType.HitCountDouble, SkillCategory.Attack, true,
            "Hit Doubler",
            "Double this skill's hit count on counter.");

        Add(ModifierEffectType.DoubleEnemyStatusStacks, SkillCategory.Attack, true,
            "Status Amplifier",
            "Double all enemy status stacks on counter.");

        Add(ModifierEffectType.EnergyRefund, SkillCategory.Attack, true,
            "Counter Refund",
            "Refund this skill's energy cost on counter.");

        Add(ModifierEffectType.Lifesteal, SkillCategory.Attack, true,
            "Counter Drain",
            "Heal for 50% of damage dealt on counter.",
            primaryValue: 0.50f);

        Add(ModifierEffectType.SelfAttackBuff, SkillCategory.Attack, true,
            "Counter Empower",
            "Own Attack +50% on counter.",
            primaryValue: 0.50f);

        Add(ModifierEffectType.SelfDefenseBuff, SkillCategory.Attack, true,
            "Counter Fortify",
            "Own Defense +50% on counter.",
            primaryValue: 0.50f);

        Add(ModifierEffectType.EnemyAttackDebuff, SkillCategory.Attack, true,
            "Counter Weaken",
            "Enemy Attack −50% on counter.",
            primaryValue: 0.50f);

        Add(ModifierEffectType.EnemyDefenseDebuff, SkillCategory.Attack, true,
            "Counter Shatter",
            "Enemy Defense −50% on counter.",
            primaryValue: 0.50f);

        Add(ModifierEffectType.DispelEnemyBuffs, SkillCategory.Attack, true,
            "Counter Dispel",
            "Remove all enemy buffs on counter.");

        // ── Defense / Regular ────────────────────────────────────────────
        Add(ModifierEffectType.HpForEnergy, SkillCategory.Defense, false,
            "Blood Price",
            "When energy is insufficient, consume 5% HP instead of 1 energy.",
            primaryValue: 0.05f);

        Add(ModifierEffectType.FreeAfterAttackSkill, SkillCategory.Defense, false,
            "Follow-Through Guard",
            "If an Attack skill was used last turn, Guard costs 0 energy.");

        // ── Defense / Counter ────────────────────────────────────────────
        Add(ModifierEffectType.NextAttackPowerDouble, SkillCategory.Defense, true,
            "Parry Empower",
            "Next attack power ×2 on counter.");

        Add(ModifierEffectType.DamageTakenToSelfAttack, SkillCategory.Defense, true,
            "Pain to Strength",
            "Own Attack +10% per hit received while guarding.",
            primaryValue: 0.10f);

        Add(ModifierEffectType.DamageTakenToSelfDefense, SkillCategory.Defense, true,
            "Pain to Armor",
            "Own Defense +10% per hit received while guarding.",
            primaryValue: 0.10f);

        Add(ModifierEffectType.EnemyAttackDebuff, SkillCategory.Defense, true,
            "Parry Weaken",
            "Enemy Attack −50% on counter.",
            primaryValue: 0.50f);

        Add(ModifierEffectType.EnemyDefenseDebuff, SkillCategory.Defense, true,
            "Parry Shatter",
            "Enemy Defense −50% on counter.",
            primaryValue: 0.50f);

        Add(ModifierEffectType.LastStand, SkillCategory.Defense, true,
            "Last Stand",
            "When receiving fatal damage this turn, survive with 1 HP.");

        Add(ModifierEffectType.EnergyFromCounteredSkillCost, SkillCategory.Defense, true,
            "Energy Siphon",
            "Restore energy equal to the countered skill's cost.");

        Add(ModifierEffectType.DamageToHeal, SkillCategory.Defense, true,
            "Absorb",
            "Damage mitigated while guarding is converted to HP recovery.");

        Add(ModifierEffectType.ReflectDamage, SkillCategory.Defense, true,
            "Reflect",
            "Interrupt the countered skill; deal damage equal to its base power.");

        Add(ModifierEffectType.ExtraAction, SkillCategory.Defense, true,
            "Parry Counter",
            "Gain 1 extra action after a successful counter.",
            primaryValue: 1);

        // ── Status / Regular ─────────────────────────────────────────────
        Add(ModifierEffectType.SelfAttackBuffTurns, SkillCategory.Status, false,
            "Battle Cry",
            "Own Attack +50% for 2 turns.",
            primaryValue: 0.50f, secondaryValue: 2);

        Add(ModifierEffectType.SelfDefenseBuffTurns, SkillCategory.Status, false,
            "Iron Skin",
            "Own Defense +50% for 2 turns.",
            primaryValue: 0.50f, secondaryValue: 2);

        Add(ModifierEffectType.EnemyAttackDebuff, SkillCategory.Status, false,
            "Curse",
            "Enemy Attack −30%.",
            primaryValue: 0.30f);

        Add(ModifierEffectType.EnemyDefenseDebuff, SkillCategory.Status, false,
            "Break Armor",
            "Enemy Defense −30%.",
            primaryValue: 0.30f);

        Add(ModifierEffectType.ApplyStatus, SkillCategory.Status, false,
            "Ignite (Status)",
            "Apply 3 Burn stacks to the enemy.",
            primaryValue: 3, targetStatus: StatusEffectType.Burn);

        Add(ModifierEffectType.ApplyStatus, SkillCategory.Status, false,
            "Envenom (Status)",
            "Apply 3 Poison stacks to the enemy.",
            primaryValue: 3, targetStatus: StatusEffectType.Poison);

        Add(ModifierEffectType.ApplyStatus, SkillCategory.Status, false,
            "Chill (Status)",
            "Apply 3 Freeze stacks to the enemy.",
            primaryValue: 3, targetStatus: StatusEffectType.Freeze);

        // ── Status / Counter ─────────────────────────────────────────────
        Add(ModifierEffectType.InterruptAndStun, SkillCategory.Status, true,
            "Status Interrupt",
            "Cancel the enemy's countered skill; enemy is stunned next turn.");

        Add(ModifierEffectType.SelfBuffAmplifier, SkillCategory.Status, true,
            "Buff Amplifier",
            "Own buff effects are doubled this turn.");

        Add(ModifierEffectType.DispelEnemyBuffsOnCounter, SkillCategory.Status, true,
            "Status Dispel",
            "Remove all enemy buffs on counter.");

        Add(ModifierEffectType.DoubleEnemyStatusStacks, SkillCategory.Status, true,
            "Status Surge",
            "Double all enemy status stacks on counter.");

        Add(ModifierEffectType.ExtraAction, SkillCategory.Status, true,
            "Status Counter",
            "Gain 1 extra action after a successful counter.",
            primaryValue: 1);

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"ModifierLibrary: populated {_definitions.Count} modifiers from GDD defaults.", this);
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
