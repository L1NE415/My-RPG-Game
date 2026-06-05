/// <summary>
/// Modifier effect types grouped by implementation phase.
/// Add new types to the appropriate phase block as systems are built.
/// </summary>
public enum ModifierEffectType
{
    // ── Phase 1: Fully implemented ───────────────────────────────────────────

    // Stat mutations — applied permanently to RuntimeSkill at reward time
    FlatPowerBoost,         // basePower += PrimaryValue
    HitCountIncrease,       // hitCount += PrimaryValue
    FlatGuardReduction,     // guardDamageMultiplier -= PrimaryValue
    NextAttackDamageBoost,  // nextAttackDamageBonus += PrimaryValue (Boost skill)

    // Dynamic — evaluated each time the skill is used
    HighHpPowerBoost,       // +PrimaryValue power if HP > SecondaryValue%
    LowHpPowerBoost,        // +PrimaryValue% power per SecondaryValue% HP lost
    Lifesteal,              // heal for PrimaryValue% of damage dealt

    // ── Phase 2: Requires energy system ─────────────────────────────────────
    EnergyRestoreOnUse,
    EnergyRefund,
    HpForEnergy,
    FreeAfterAttackSkill,

    // ── Phase 3: Requires StatusEffect system ────────────────────────────────
    ApplyStatus,
    StatusSynergyPower,

    // ── Phase 4: Requires Counter detection ──────────────────────────────────
    NextAttackPowerDouble,
    CounterPowerSurge,
    ExtraAction,
    InterruptSkill,
}
