---
status: reverse-documented
source: GDD.md, Assets/Scripts/
date: 2026-05-28
verified-by: Hans
---

# Combat System

> **Note**: This document was reverse-engineered from GDD.md and the existing implementation.
> Systems marked **[NOT YET IMPLEMENTED]** are designed but not yet in code.

## Overview

Turn-based 1v1 combat between a player character and an enemy. Each turn, both sides simultaneously select a skill from one of three categories — Attack, Defense, or Status. A counter triangle determines whether one choice beats the other, granting the winning side an extra action. Combat continues until one side is reduced to 0 HP. Each new battle (round) begins with the player's HP and energy fully restored.

## Player Fantasy

The thrill of reading your opponent — correctly predicting which skill type the enemy will use and landing a counter that grants a burst of power. Every turn is a meaningful bet: play aggressively and risk being countered, or play safe and sacrifice damage output. The extra action from a counter feels earned and explosive.

## Detailed Rules

### Turn Structure

1. Player selects a skill (Attack, Defense, or Status category)
2. Enemy simultaneously selects a skill
3. Player's skill resolves first
4. **Counter check**: if the player's skill category beats the enemy's skill category per the counter triangle, the player gains 1 extra action immediately **[NOT YET IMPLEMENTED]**
5. **Reverse counter check**: if the enemy's category beats the player's, the enemy gains 1 extra action after their turn **[NOT YET IMPLEMENTED]**
6. Enemy's skill resolves
7. Repeat until one combatant reaches 0 HP

### Counter Triangle **[NOT YET IMPLEMENTED]**

| Player Selects | Enemy Selects | Result |
|----------------|---------------|--------|
| Attack | Status | Player counters — player gains 1 extra action |
| Status | Defense | Player counters — player gains 1 extra action |
| Defense | Attack | Player counters — player gains 1 extra action |
| Status | Attack | Enemy counters — enemy gains 1 extra action |
| Defense | Status | Enemy counters — enemy gains 1 extra action |
| Attack | Defense | Enemy counters — enemy gains 1 extra action |
| Same category | Same category | No counter — no extra action granted |

Counter grants exactly **1 extra action** to the winning side. A tie (same category) results in no extra action for either side.

### Combat Round

- A **round** = one complete battle against one enemy (from round start until either side reaches 0 HP)
- At the **start** of each new round: player HP and energy fully restore
- **Within** a round: HP and energy only recover through skill effects or modifiers
- There is no round timer — combat continues until one combatant is defeated

### Energy System **[NOT YET IMPLEMENTED]**

- Player starts each round with **10 energy**
- Each skill consumes energy when used (see Skill System GDD for costs per skill and tier)
- Energy recovers only via specific modifiers (e.g., "Restore 1 energy on use")
- If energy is insufficient, the skill cannot be selected
  - **Exception (Defense modifier)**: "Consume 5% HP instead of 1 energy when energy is insufficient"

### Status Effects **[NOT YET IMPLEMENTED]**

Stack-based effects applied by skill modifiers. Effects persist across turns until removed or expired.

| Effect | Behavior |
|--------|----------|
| Burn | Deals damage per stack at the start of the affected combatant's turn |
| Poison | Deals damage per stack at the start of the affected combatant's turn |
| Freeze | Reduces or prevents actions based on stack count |
| Stun | Target loses their next action (does not stack — re-applying resets duration) |

*Per-stack damage values for Burn and Poison are tuning knobs. Stack thresholds for Freeze action loss are TBD.*

## Formulas

| Variable | Description |
|----------|-------------|
| `base_power` | Skill's base damage value |
| `hit_count` | Number of hits the skill delivers |
| `damage_per_hit` | `Ceil(base_power / hit_count)` |
| `guard_multiplier` | Fraction of damage taken while guarding (0.0–1.0); lower = more reduction |
| `attack_multiplier` | Bonus multiplier applied to attack before guard (default 1.0; set by Boost) |

**Single-hit attack damage:**
```
final_damage = Ceil(base_power × attack_multiplier)
```

**Multi-hit attack (per hit):**
```
damage_per_hit = Ceil(base_power / hit_count)
total_damage   = damage_per_hit × hits_landed
```
*Loop stops early if enemy HP reaches 0.*

**Guarded damage:**
```
guarded_damage = Ceil(incoming_damage × guard_multiplier)
```
At 100% guard (`guard_multiplier = 0.0`): incoming damage is reduced to 0.

## Edge Cases

| Scenario | Resolution |
|----------|------------|
| Enemy reaches 0 HP mid-combo | Battle ends immediately; remaining hits in the combo do not apply |
| Player reaches 0 HP mid-turn | Battle lost; current turn does not complete |
| Both sides select the same skill category | No counter fires for either side |
| Counter modifier "Interrupt countered skill" activates | The countered skill does not execute; the interrupting side still gains its counter benefit |
| Player energy is exactly 0 | 0-cost skills remain available; energy-cost skills are blocked. Defense modifier may allow HP-for-energy trade |
| Stun applied while a combatant is already mid-action | Stun applies starting the next turn; the current action completes |
| Fatal damage when "Last Stand" modifier is active | HP is set to 1 instead of 0; modifier does not activate if HP is already at 1 |

## Dependencies

- **Skill System GDD** (`design/gdd/skill-system.md`) — defines all skill categories, energy costs, base effects, and modifier pools
- **Status Effect System** — manages Burn, Poison, Freeze, Stun stacks and tick behavior **[NOT YET IMPLEMENTED]**
- **Battle UI** — player skill selection interface, HP bars, energy bar, animation triggers
- **Enemy AI** — determines enemy skill selection logic (currently: random 50/50 Attack/Defend)

## Tuning Knobs

| Parameter | Current Value | Notes |
|-----------|---------------|-------|
| Player starting energy per round | 10 | **[NOT YET IMPLEMENTED]** |
| Extra actions granted per counter | 1 | **[NOT YET IMPLEMENTED]** |
| Burn damage per stack per turn | TBD | Needs balance pass |
| Poison damage per stack per turn | TBD | Needs balance pass |
| Freeze action reduction per stack | TBD | Needs balance pass |
| Player starting HP | 100 | `maxHealth` in `CombatantHealth` |
| Enemy HP | 100 | `maxHealth` in `CombatantHealth` (default) |
| Enemy attack damage | 40 | `attackDamage` in `EnemyBattleActions` |

## Acceptance Criteria

- [ ] Player can select one of three skill categories (Attack, Defense, Status) each turn
- [ ] Counter triangle correctly detects the winning category in all 9 matchup combinations
- [ ] Counter grants exactly 1 extra action to the winning side
- [ ] Extra action resolves before the enemy's turn
- [ ] Same-category matchup grants no extra action to either side
- [ ] HP and energy fully restore at the start of each new round
- [ ] Battle ends immediately when either combatant reaches 0 HP
- [ ] Energy cost is deducted when a skill is used
- [ ] Skills with insufficient energy are blocked in the UI (cannot be selected)
- [ ] Burn and Poison deal damage at the start of the affected combatant's turn, per stack
- [ ] Stun causes target to skip their next action
