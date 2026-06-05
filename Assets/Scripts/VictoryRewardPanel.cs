using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryRewardPanel : MonoBehaviour
{
    private static readonly string[] OptionChildNames = { "Option 1", "Option 2", "Option 3", "Option 4" };

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private bool hideOnAwake = true;

    [Header("Option Buttons")]
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private Button confirmButton;

    [Header("Selection Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.86f, 0.35f, 1f);

    private PlayerBattleActions playerBattleActions;
    private List<ModifierDefinition> currentOptions = new();
    private int selectedIndex = -1;
    private bool initialized;
    private bool rewardConsumed;

    private void Awake()
    {
        Initialize();

        if (hideOnAwake)
        {
            Hide();
        }
    }

    /// <summary>Shows the panel and draws random modifier options from the player's skill set library.</summary>
    public void Show(PlayerBattleActions rewardTarget)
    {
        Initialize();
        playerBattleActions = rewardTarget;
        selectedIndex = -1;
        rewardConsumed = false;

        currentOptions = rewardTarget.SkillSet != null
            ? rewardTarget.SkillSet.DrawRewardOptions(optionButtons.Length)
            : new List<ModifierDefinition>();

        if (currentOptions.Count == 0)
        {
            Debug.LogWarning("VictoryRewardPanel: No modifier options available to show.", this);
        }

        UpdateOptionLabels();
        RefreshSelectionVisuals();
        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        Initialize();
        panelRoot.SetActive(false);
    }

    public void ConfirmSelection()
    {
        if (rewardConsumed) return;

        if (selectedIndex < 0 || selectedIndex >= currentOptions.Count)
        {
            Debug.LogWarning("VictoryRewardPanel confirm ignored: no reward is selected.", this);
            return;
        }

        if (playerBattleActions == null)
        {
            Debug.LogWarning("VictoryRewardPanel cannot apply reward: Player Battle Actions is missing.", this);
            return;
        }

        rewardConsumed = true;
        playerBattleActions.ApplyModifierReward(currentOptions[selectedIndex]);
        Hide();
    }

    private void SelectOption(int index)
    {
        if (index < 0 || index >= currentOptions.Count) return;
        selectedIndex = index;
        RefreshSelectionVisuals();
    }

    private void Initialize()
    {
        if (initialized) return;

        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        ResolveButtons();
        BindButtons();
        RefreshSelectionVisuals();
        initialized = true;
    }

    private void ResolveButtons()
    {
        if (optionButtons == null || optionButtons.Length == 0)
        {
            optionButtons = new Button[OptionChildNames.Length];
        }

        for (int i = 0; i < OptionChildNames.Length && i < optionButtons.Length; i++)
        {
            optionButtons[i] ??= FindChildButton(OptionChildNames[i]);
        }

        confirmButton ??= FindChildButton("Confirm");
    }

    private void BindButtons()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;
            int capturedIndex = i;
            optionButtons[i].onClick.AddListener(() => SelectOption(capturedIndex));
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmSelection);
        }
    }

    private void UpdateOptionLabels()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null) continue;

            bool hasOption = i < currentOptions.Count && currentOptions[i] != null;
            optionButtons[i].gameObject.SetActive(hasOption);

            if (!hasOption) continue;

            ModifierDefinition def = currentOptions[i];
            TMP_Text label = optionButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = $"{def.DisplayName}\n<size=80%>{def.Description}</size>";
            }
        }
    }

    private void RefreshSelectionVisuals()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            SetButtonSelected(optionButtons[i], i == selectedIndex);
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = selectedIndex >= 0;
        }
    }

    private Button FindChildButton(string childName)
    {
        Transform child = transform.Find(childName);
        return child == null ? null : child.GetComponent<Button>();
    }

    private void SetButtonSelected(Button button, bool isSelected)
    {
        if (button == null) return;

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = isSelected ? selectedColor : normalColor;
        }
    }
}
