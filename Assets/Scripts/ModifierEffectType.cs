/// <summary>
/// All modifier effect types defined in the Skill System GDD.
/// Shared types (e.g. ExtraAction, EnemyAttackDebuff) appear once;
/// the modifier's ApplicableCategory + IsCounterSlot fields provide context.
/// </summary>
public enum ModifierEffectType
{
    // ── Attack / Regular ──────────────────────────────────────────────────
    EnergyRestoreOnUse,           // Restore X energy when this skill is used
    LowHpCostReduction,           // Cost −1 per 10% HP lost (primaryValue = per-step reduction)
    LowHpPowerBoost,              // Power +5% per 5% HP lost
    HighHpPowerBoost,             // Power +primaryValue if HP > secondaryValue%
    CounterFollowUpPower,         // Power +X if a counter succeeded last turn
    GuardFollowUpPower,           // Power +X if a Defense skill was used last turn
    CounterAccumulatorPower,      // Permanent power +X per successful counter
    UseAccumulatorPower,          // Permanent power +X per use
    KillAccumulatorPower,         // Permanent power +X per enemy defeated
    KillEnergyRestore,            // Restore X energy if this skill defeats the enemy
    ApplyStatus,                  // Apply primaryValue stacks of TargetStatus on hit
    PerHitApplyStatus,            // Apply primaryValue stacks of TargetStatus per hit
    StatusSynergyPower,           // +primaryValue power per stack of any status on the enemy

    // ── Shared (Attack Counter / Defense Counter / Status Counter) ────────
    ExtraAction,                  // Gain 1 extra action this turn
    EnemyAttackDebuff,            // Enemy Attack −primaryValue%
    EnemyDefenseDebuff,           // Enemy Defense −primaryValue%
    DoubleEnemyStatusStacks,      // Double all enemy status stacks

    // ── Attack / Counter ─────────────────────────────────────────────────
    CounterPowerSurge,            // +primaryValue% damage on counter; −secondaryValue% if no counter
    InterruptSkill,               // Cancel the enemy's countered skill
    StunEnemy,                    // Stun the enemy for 1 turn
    HitCountIncrease,             // Increase hit count by primaryValue
    HitCountDouble,               // Double this skill's hit count
    EnergyRefund,                 // Refund this skill's energy cost
    Lifesteal,                    // Heal for primaryValue% of damage dealt
    SelfAttackBuff,               // Own Attack +primaryValue%
    SelfDefenseBuff,              // Own Defense +primaryValue%
    DispelEnemyBuffs,             // Remove all buff effects from the enemy

    // ── Defense / Regular ────────────────────────────────────────────────
    HpForEnergy,                  // Consume primaryValue% HP instead of 1 energy when short
    FreeAfterAttackSkill,         // Guard costs 0 energy if an Attack skill was used last turn

    // ── Defense / Counter ────────────────────────────────────────────────
    NextAttackPowerDouble,        // Next attack power ×2
    DamageTakenToSelfAttack,      // Own Attack +primaryValue% per hit received while guarding
    DamageTakenToSelfDefense,     // Own Defense +primaryValue% per hit received while guarding
    LastStand,                    // Survive with 1 HP when receiving fatal damage
    EnergyFromCounteredSkillCost, // Restore energy equal to the countered skill's cost
    DamageToHeal,                 // Damage mitigated while guarding converts to HP recovery
    ReflectDamage,                // Interrupt countered skill; deal damage equal to its base power

    // ── Status / Regular ─────────────────────────────────────────────────
    SelfAttackBuffTurns,          // Own Attack +primaryValue% for secondaryValue turns
    SelfDefenseBuffTurns,         // Own Defense +primaryValue% for secondaryValue turns
    // (EnemyAttackDebuff / EnemyDefenseDebuff shared above)
    // (ApplyStatus shared above)

    // ── Status / Counter ─────────────────────────────────────────────────
    InterruptAndStun,             // Cancel enemy's countered skill; stun enemy next turn
    SelfBuffAmplifier,            // Own buff effects doubled this turn
    DispelEnemyBuffsOnCounter,    // Remove all enemy buffs (counter-specific variant)
    // (DoubleEnemyStatusStacks / ExtraAction shared above)
}
