using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CombatantHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private float guardDamageMultiplier = 0.5f;

    private int currentHealth;
    private bool isGuarding;
    private bool defeatNotified;

    public bool IsDefeated => currentHealth <= 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public event Action<CombatantHealth> Defeated;

    private void Awake()
    {
        currentHealth = maxHealth;
        RefreshUI();
    }

    public void SetGuarding(bool value)
    {
        isGuarding = value;
    }

    public void SetGuardDamageMultiplier(float multiplier)
    {
        guardDamageMultiplier = Mathf.Clamp01(multiplier);
        Debug.Log($"{name} guard damage multiplier: {guardDamageMultiplier}", this);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isGuarding = false;
        defeatNotified = false;
        RefreshUI();

        Debug.Log($"{name} health reset. HP: {currentHealth}/{maxHealth}", this);
    }

    /// <summary>Deals damage, applying guard reduction if active. Returns the amount of HP actually lost.</summary>
    public int TakeDamage(int damage)
    {
        int finalDamage = isGuarding ? Mathf.CeilToInt(damage * guardDamageMultiplier) : damage;
        currentHealth = Mathf.Max(currentHealth - finalDamage, 0);
        RefreshUI();

        Debug.Log($"{name} took {finalDamage} damage. HP: {currentHealth}/{maxHealth}", this);

        if (IsDefeated && !defeatNotified)
        {
            defeatNotified = true;
            Defeated?.Invoke(this);
        }

        return finalDamage;
    }

    /// <summary>Restores HP up to the maximum. Has no effect if already at full HP.</summary>
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        RefreshUI();
        Debug.Log($"{name} healed {amount} HP. HP: {currentHealth}/{maxHealth}", this);
    }

    private void RefreshUI()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
}
