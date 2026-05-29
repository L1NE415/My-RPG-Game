using System;
using UnityEngine;

/// <summary>
/// One entry in the modifier library. Defines what a modifier does and which
/// skill slot it fills. primaryValue and secondaryValue carry the numeric
/// parameters described in each effect type's XML doc on ModifierEffectType.
/// </summary>
[Serializable]
public class ModifierDefinition
{
    [SerializeField] private ModifierEffectType _effectType;
    [SerializeField] private SkillCategory _applicableCategory;
    [SerializeField] private bool _isCounterSlot;
    [SerializeField] private string _displayName;
    [TextArea(2, 4)]
    [SerializeField] private string _description;
    [SerializeField] private float _primaryValue;
    [SerializeField] private float _secondaryValue;
    [SerializeField] private StatusEffectType _targetStatus;

    public ModifierEffectType EffectType        => _effectType;
    public SkillCategory ApplicableCategory     => _applicableCategory;
    public bool IsCounterSlot                   => _isCounterSlot;
    public string DisplayName                   => _displayName;
    public string Description                   => _description;
    public float PrimaryValue                   => _primaryValue;
    public float SecondaryValue                 => _secondaryValue;
    public StatusEffectType TargetStatus        => _targetStatus;

    public ModifierDefinition() { }

    public ModifierDefinition(
        ModifierEffectType effectType,
        SkillCategory applicableCategory,
        bool isCounterSlot,
        string displayName,
        string description,
        float primaryValue = 0f,
        float secondaryValue = 0f,
        StatusEffectType targetStatus = StatusEffectType.Burn)
    {
        _effectType          = effectType;
        _applicableCategory  = applicableCategory;
        _isCounterSlot       = isCounterSlot;
        _displayName         = displayName;
        _description         = description;
        _primaryValue        = primaryValue;
        _secondaryValue      = secondaryValue;
        _targetStatus        = targetStatus;
    }
}
