using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Populates a skill action button with live data from a RuntimeSkill.
/// Attach to the root Button GameObject of each action button in the battle UI.
/// Call Bind() once on Start and again whenever modifiers are applied.
/// </summary>
public class SkillButtonUI : MonoBehaviour
{
    [Header("Category Icon")]
    [SerializeField] private Image categoryIcon;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite defenseSprite;
    [SerializeField] private Sprite statusSprite;

    [Header("Labels")]
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text damageLabel;
    [SerializeField] private TMP_Text costLabel;

    [Header("Modifier List")]
    [SerializeField] private Transform modifierListContainer;
    [SerializeField] private TMP_Text modifierEntryPrefab;

    /// <summary>
    /// Refreshes all visible fields from the provided runtime data.
    /// Safe to call multiple times — clears and rebuilds the modifier list each call.
    /// </summary>
    public void Bind(RuntimeSkill skill, IReadOnlyList<ModifierDefinition> modifiers)
    {
        if (skill == null) return;

        SetName(skill);
        SetCategoryIcon(skill.Category);
        SetDamageLabel(skill);
        SetCostLabel(skill);
        RebuildModifierList(modifiers);
    }

    private void SetName(RuntimeSkill skill)
    {
        if (nameLabel != null)
            nameLabel.text = skill.DisplayName;
    }

    private void SetCategoryIcon(SkillCategory category)
    {
        if (categoryIcon == null) return;

        categoryIcon.sprite = category switch
        {
            SkillCategory.Attack  => attackSprite,
            SkillCategory.Defense => defenseSprite,
            SkillCategory.Status  => statusSprite,
            _                     => null
        };

        categoryIcon.enabled = categoryIcon.sprite != null;
    }

    private void SetDamageLabel(RuntimeSkill skill)
    {
        if (damageLabel == null) return;

        bool showDamage = skill.Category == SkillCategory.Attack && skill.BasePower > 0;
        damageLabel.gameObject.SetActive(showDamage);

        if (!showDamage) return;

        damageLabel.text = skill.HitCount > 1
            ? $"{skill.DamagePerHit} × {skill.HitCount}"
            : $"{skill.BasePower}";
    }

    private void SetCostLabel(RuntimeSkill skill)
    {
        if (costLabel == null) return;

        bool showCost = skill.EnergyCost > 0;
        costLabel.gameObject.SetActive(showCost);

        if (showCost)
            costLabel.text = $"{skill.EnergyCost} EN";
    }

    private void RebuildModifierList(IReadOnlyList<ModifierDefinition> modifiers)
    {
        if (modifierListContainer == null) return;

        foreach (Transform child in modifierListContainer)
            Destroy(child.gameObject);

        if (modifierEntryPrefab == null || modifiers == null) return;

        foreach (ModifierDefinition mod in modifiers)
        {
            TMP_Text entry = Instantiate(modifierEntryPrefab, modifierListContainer);
            entry.text = $"• {mod.DisplayName}";
        }
    }
}
