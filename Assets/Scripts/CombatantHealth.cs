using TMPro;
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

    public bool IsDefeated => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        RefreshUI();
    }

    public void SetGuarding(bool value)
    {
        isGuarding = value;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = isGuarding ? Mathf.CeilToInt(damage * guardDamageMultiplier) : damage;
        currentHealth = Mathf.Max(currentHealth - finalDamage, 0);
        RefreshUI();

        Debug.Log($"{name} took {finalDamage} damage. HP: {currentHealth}/{maxHealth}", this);
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
