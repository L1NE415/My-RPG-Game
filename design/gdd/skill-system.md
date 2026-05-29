---
status: reverse-documented
source: GDD.md, Assets/Scripts/
date: 2026-05-28
verified-by: Hans
---

# Skill System

> **Note**: This document was reverse-engineered from GDD.md and the existing implementation.
> Systems marked **[NOT YET IMPLEMENTED]** are designed but not yet in code.
> The current build contains a simplified placeholder reward system (1 pick from 4 fixed options)
> that will be replaced by the 3-roll draft system described in this document.

## Overview

Each combatant holds a set of skills organized into three categories: Attack, Defense, and Status. Skills have energy costs and base effects that scale with energy invested. After winning a battle, the player drafts 1 modifier from 3 random rolls for each of their skills, permanently upgrading that skill. Each skill can hold at most 1 counter modifier and 3 regular modifiers. Modifier effects stack within per-effect caps, enabling deep build customization across a run.

## Player Fantasy

Watching a skill evolve across battles into something uniquely powerful — a triple-hit attack that poisons on every hit and snowballs permanent damage from each kill, or a guard that converts blocked damage into healing. The 3-roll draft creates moments of unexpected synergy and run-defining choices. No two runs feel the same.

## Detailed Rules

### Skill Categories

| Category | Counters | Countered By |
|----------|----------|--------------|
| Attack | Status | Defense |
| Defense | Attack | Status |
| Status | Defense | Attack |

*Full counter resolution rules are in Combat System GDD.*

### Base Skills (Player)

| Skill ID | Category | Base Cost | Base Power | Hit Count | Starting Effect |
|----------|----------|-----------|------------|-----------|-----------------|
| Attack | Attack | 0 | 20 | 1 | Deals `base_power` damage |
| DoubleAttack | Attack | varies | 40 | 2 | `Ceil(40/2)` = 20 per hit |
| Guard | Defense | 1 | — | — | Reduces incoming damage by `(1 − guard_multiplier) × 100%`; starts at 50% reduction |
| Boost | Status | 1 | — | — | Multiplies next attack by `(1 + next_attack_bonus)`; starts at ×1.0 (0% bonus). Resets after the next attack resolves. |

### Attack Skill Cost Scaling **[NOT YET IMPLEMENTED]**

The player chooses how much energy to spend on an Attack-category skill each use. Higher spend = higher power. The player also selects the hit-count variant when multiple are available.

| Cost | Single Hit | 2-Hit | 3-Hit | 4-Hit | 5-Hit |
|------|-----------|-------|-------|-------|-------|
| 0 | 20 | — | — | — | — |
| 1 | 40 | 25×2 | 15×3 | — | — |
| 2 | 60 | 35×2 | 20×3 | — | — |
| 3 | 80 | 45×2 | 30×3 | — | — |
| 4 | 100 | 50×2 | 35×3 | 25×4 | — |
| 5 | 120 | 60×2 | 40×3 | — | 25×5 |

*Multi-hit variants become available as the skill gains modifiers unlocking them.*

### Defense Skill Cost Scaling **[NOT YET IMPLEMENTED]**

The player chooses how much energy to spend on the Guard skill each use.

| Cost | Damage Reduction | `guard_multiplier` |
|------|-----------------|-------------------|
| 1 | 70% | 0.30 |
| 2 | 80% | 0.20 |
| 3 | 90% | 0.10 |
| 4 | 100% | 0.00 |

### Modifier System (词条)

> **Current code status**: `RuntimeSkill.modifiers` (`List<SkillModifier>`) exists and records modifier history. The 3-roll draft reward flow is **not yet implemented**. Current build uses a placeholder: player picks 1 of 4 fixed upgrades after winning a battle.

#### Reward Flow (Intended Design)

After winning a battle (round):
1. For **each** of the player's skills, draw **3 random modifiers** from that skill's modifier pool
2. Player views 3 options and selects **1** to permanently apply to that skill (card-draft style)
3. The 2 unchosen rolls are discarded
4. This repeats for each skill in the player's set

#### Modifier Slot Caps

Each skill holds at most:
- **1 counter modifier** — activates only when this skill successfully counters the enemy
- **3 regular modifiers** — always active or trigger-based

If the drawn modifier would fill a slot that is already at its cap, the player cannot pick it (it is unavailable or auto-discarded — **TBD: UI behavior**).

All modifier effects stack, subject to per-effect caps (specific caps are tuning knobs, listed in Tuning Knobs section).

---

### Attack — Regular Modifiers

| Modifier | Effect |
|----------|--------|
| Energy restore | Restore 1 energy when this skill is used |
| Low HP cost reduction | For every 10% HP lost, this skill's cost −1 |
| Low HP power boost | For every 5% HP lost, this skill's power +5% |
| High HP power boost | If current HP > X%, skill power +X |
| Counter follow-up | If last turn's counter succeeded, skill power +X this use |
| Guard follow-up | If last turn used a Defense skill, skill power +X this use |
| Counter accumulator | Each successful counter: skill power permanently +X |
| Use accumulator | Each use of this skill: power permanently +X |
| Kill accumulator | Each enemy defeated: power permanently +X |
| Kill energy restore | If this skill defeats an enemy, restore X energy |
| Apply Burn | Apply X stacks of Burn to the enemy |
| Apply Poison | Apply X stacks of Poison to the enemy |
| Apply Freeze | Apply X stacks of Freeze to the enemy |
| Per-hit status | Each hit applies X stacks of the selected status effect |
| Status synergy | +10 power for each stack of any status effect on the enemy |

### Attack — Counter Modifier

*(Activates only when this Attack skill counters an enemy Status skill)*

| Modifier | Effect |
|----------|--------|
| Counter power surge | Skill damage +50% / +100% / +200%; if no counter occurred, power −50% |
| Interrupt | Cancel the enemy's countered skill before it resolves |
| Extra action | Gain 1 extra action this turn |
| Stun | Enemy is stunned for 1 turn |
| Hit count boost | Increase or double this use's hit count |
| Status doubler | Double all enemy status effect stacks |
| Energy refund | Return the energy spent on this skill |
| Lifesteal | Recover HP equal to damage dealt |
| Self buff | Own Attack or Defense +50% / +75% / +100% |
| Enemy debuff | Enemy Attack or Defense −50% / −75% / −100% |
| Dispel | Remove all buff effects from the enemy |

---

### Defense — Regular Modifiers

| Modifier | Effect |
|----------|--------|
| HP-for-energy | When energy is insufficient, consume 5% HP instead of 1 energy |
| Free after attack | If an Attack skill was used last turn, Guard costs 0 energy this use |

### Defense — Counter Modifier

*(Activates only when this Guard skill counters an enemy Attack skill)*

| Modifier | Effect |
|----------|--------|
| Next attack doubler | Next attack power ×2 |
| Damage-to-attack | For each hit received while guarding: own Attack +X% |
| Damage-to-defense | For each hit received while guarding: own Defense +X% |
| Enemy debuff | Enemy Attack or Defense −X% |
| Last stand | If this turn's damage would be fatal, survive with 1 HP instead |
| Energy from cost | Restore energy equal to the countered skill's energy cost |
| Damage-to-heal | Damage mitigated by guarding is converted to HP recovery |
| Reflect | Interrupt the countered skill and deal damage equal to that skill's base power |
| Extra action | Gain 1 extra action this turn |

---

### Status — Regular Modifiers

| Modifier | Effect |
|----------|--------|
| Self attack buff | Own Attack +X% for X turns |
| Self defense buff | Own Defense +X% for X turns |
| Enemy attack debuff | Enemy Attack −X% |
| Enemy defense debuff | Enemy Defense −X% |
| Apply Burn | Apply X stacks of Burn to the enemy |
| Apply Poison | Apply X stacks of Poison to the enemy |
| Apply Freeze | Apply X stacks of Freeze to the enemy |

### Status — Counter Modifier

*(Activates only when this Status skill counters an enemy Defense skill)*

| Modifier | Effect |
|----------|--------|
| Interrupt and stun | Cancel the enemy's countered skill; enemy is stunned next turn |
| Buff amplifier | Own buff effects are doubled this turn |
| Buff dispel | Remove all buff effects from the enemy |
| Status doubler | Double all enemy status effect stacks |
| Extra action | Gain 1 extra action this turn |

---

## Formulas

| Variable | Description |
|----------|-------------|
| `base_power` | Skill's base damage before any modifiers |
| `hit_count` | Number of hits the skill delivers |
| `damage_per_hit` | `Ceil(base_power / hit_count)` |
| `guard_multiplier` | Fraction of incoming damage taken while guarding (0.0 = 100% block) |
| `next_attack_bonus` | Additive multiplier bonus from Boost; resets to 0 after attack resolves |
| `power_modifier_sum` | Sum of all active additive power modifier contributions |

**Modified attack power:**
```
effective_power = Ceil(base_power × (1 + power_modifier_sum))
```

**Modified attack with Boost:**
```
final_damage = Ceil(effective_power × (1 + next_attack_bonus))
```

**Guard reduction:**
```
guarded_damage = Ceil(incoming_damage × guard_multiplier)
```
At 100% reduction: `guard_multiplier = 0.0` → `guarded_damage = 0`

**Multi-hit DPS:**
```
damage_per_hit = Ceil(effective_power / hit_count)
```

## Edge Cases

| Scenario | Resolution |
|----------|------------|
| Modifier roll draws a type that would exceed the slot cap | The capped modifier type cannot be selected; player picks from remaining valid rolls |
| Counter modifier drawn for a skill that already has one | Same as above — already-filled slot is unselectable |
| "Counter power surge" modifier: no counter fires this turn | Power is reduced by 50% as stated in the modifier |
| "Last stand" active when HP is already exactly 1 | Does not activate — only prevents HP from falling to 0 |
| Multiple "accumulator" modifiers on the same skill | Each modifier stacks its own accumulator independently |
| Boost `next_attack_bonus` after a missed attack | Bonus still resets to 0; consumed whether attack hits or misses |
| Lifesteal when enemy reaches 0 HP mid-combo | Lifesteal applies for hits that landed before the enemy reached 0 |
| "Dispel" when enemy has no active buffs | No effect; modifier fires but nothing is removed |

## Dependencies

- **Combat System GDD** (`design/gdd/combat-system.md`) — counter triangle, turn structure, energy pool rules
- **Status Effect System** — manages Burn, Poison, Freeze, Stun stacks and per-turn tick behavior **[NOT YET IMPLEMENTED]**
- **Enemy AI** — defines the enemy's skill set and selection logic
- **Battle UI** — skill selection buttons, modifier display, reward draft UI (3-option card pick per skill)

## Tuning Knobs

| Parameter | Current Value | Notes |
|-----------|---------------|-------|
| Regular modifier slots per skill | 3 | |
| Counter modifier slots per skill | 1 | |
| Reward rolls per skill after battle | 3 | Player picks 1 |
| Base Attack power | 20 | `basePower` in `PlayerSkillSet.EnsureDefaultSkills` |
| Base DoubleAttack power | 40 | `basePower`; 2 hits = 20 per hit |
| Base Guard reduction | 50% | `guardDamageMultiplier = 0.5f` in `RuntimeSkill` |
| Boost initial bonus | 0% | `nextAttackDamageBonus = 0f` in `RuntimeSkill` |
| Attack reward power bonus (placeholder) | +20 | `attackDamageRewardBonus` in `PlayerSkillSet` |
| DoubleAttack reward hit count bonus (placeholder) | +1 | `doubleAttackHitCountRewardBonus` |
| Guard reward reduction bonus (placeholder) | +10% | `guardDamageReductionRewardBonus = 0.1f` |
| Boost reward attack bonus (placeholder) | +100% | `boostAttackPowerRewardBonus = 1.0f` |
| Modifier effect caps | TBD | Per-modifier caps; needs balance pass |

## Acceptance Criteria

- [ ] Each skill belongs to exactly one category (Attack / Defense / Status)
- [ ] After each battle, each player skill presents 3 randomly drawn modifiers
- [ ] Player selects exactly 1 of the 3 drawn modifiers to apply
- [ ] Selected modifier is permanently added to the skill's modifier list
- [ ] A skill with 3 regular modifiers cannot receive additional regular modifier rolls (slot blocked)
- [ ] A skill with 1 counter modifier cannot receive additional counter modifier rolls (slot blocked)
- [ ] `RuntimeSkill.modifiers` accurately records all applied modifiers with description and value
- [ ] Guard at 100% reduction (`guard_multiplier = 0.0`) results in 0 incoming damage
- [ ] `next_attack_bonus` resets to 0 immediately after an attack resolves
- [ ] Modifier stacking respects per-effect caps defined in tuning data
- [ ] Multi-hit `DamagePerHit` formula is `Ceil(base_power / hit_count)`
