using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PlayerBattleActions : MonoBehaviour
{
    private static readonly int Attack1Hash = Animator.StringToHash("Attack1");
    private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int IsGuardingHash = Animator.StringToHash("IsGuarding");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    [Header("Battle Movement")]
    [SerializeField] private Transform enemy;
    [SerializeField] private EnemyBattleActions enemyActions;
    [SerializeField] private CombatantHealth playerHealth;
    [SerializeField] private CombatantHealth enemyHealth;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float stoppingDistance = 1.1f;
    [SerializeField] private PlayerSkillSet skillSet;

    [Header("Animation Timing")]
    [SerializeField] private float attack1Duration = 0.55f;
    [SerializeField] private float attack2Duration = 0.55f;
    [SerializeField] private float guardDuration = 0.8f;
    [SerializeField] private float pauseBeforeReturn = 0.15f;

    [Header("Sorting")]
    [SerializeField] private int defaultSortingOrder = 5;
    [SerializeField] private int activeAttackerSortingOrder = 10;

    [Header("Victory UI")]
    [SerializeField] private VictoryRewardPanel victoryRewardPanel;
    [SerializeField] private bool autoFindVictoryRewardPanel = true;

    [Header("Action Menu")]
    [SerializeField] private GameObject actionMenuRoot;
    [SerializeField] private bool autoFindActionMenu = true;

    [Header("Skill Button UIs")]
    [SerializeField] private SkillButtonUI[] skillButtonUIs;
    [SerializeField] private SkillId[] skillButtonIds;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer enemySpriteRenderer;
    private Vector3 startPosition;
    private Coroutine currentAction;
    private bool battleEnded;
    private bool victoryPending;
    private float nextAttackDamageMultiplier = 1f;
    private Button[] actionMenuButtons;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    private void Start()
    {
        RefreshEnemyRenderer();
        ResetSortingOrder();
        RefreshActionMenuButtons();
        SetActionMenuInteractable(true);

        if (enemyActions == null)
        {
            Debug.LogWarning("PlayerBattleActions: Enemy Actions is not assigned.", this);
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<CombatantHealth>();
        }

        if (skillSet == null)
        {
            skillSet = GetComponent<PlayerSkillSet>();
        }

        if (skillSet == null)
        {
            skillSet = gameObject.AddComponent<PlayerSkillSet>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerBattleActions: Player Health is not assigned.", this);
        }

        if (enemyHealth == null)
        {
            Debug.LogWarning("PlayerBattleActions: Enemy Health is not assigned.", this);
        }
        else
        {
            enemyHealth.Defeated += HandleEnemyDefeated;
        }

        RefreshSkillButtonUIs();
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Defeated -= HandleEnemyDefeated;
        }
    }

    public void Attack()
    {
        if (!CanStartPlayerAction())
        {
            return;
        }

        Debug.Log("player action: attack", this);
        StartAction(AttackRoutine(false));
    }

    public void DoubleAttack()
    {
        if (!CanStartPlayerAction())
        {
            return;
        }

        Debug.Log("player action: double attack", this);
        StartAction(AttackRoutine(true));
    }

    public void Defend()
    {
        if (!CanStartPlayerAction())
        {
            return;
        }

        Debug.Log("player action: guard", this);
        StartAction(DefendRoutine());
    }

    public void Boost()
    {
        if (!CanStartPlayerAction())
        {
            return;
        }

        Debug.Log("player action: boost", this);
        StartAction(BoostRoutine());
    }

    private void StartAction(IEnumerator action)
    {
        if (!CanStartPlayerAction())
        {
            return;
        }

        SetActionMenuInteractable(false);

        if (enemyActions == null)
        {
            Debug.LogWarning("PlayerBattleActions cannot choose an enemy action because Enemy Actions is not assigned.", this);
        }
        else
        {
            enemyActions.ChooseNextAction();
        }

        currentAction = StartCoroutine(action);
    }

    private void StopCurrentAction()
    {
        if (currentAction != null)
        {
            StopCoroutine(currentAction);
            currentAction = null;
        }

        ResetSortingOrder();
        SetActionMenuInteractable(true);
    }

    private IEnumerator AttackRoutine(bool useSecondAttack)
    {
        if (enemy == null)
        {
            Debug.LogWarning("PlayerBattleActions needs an enemy Transform assigned.", this);
            yield break;
        }

        animator.SetBool(IsGuardingHash, false);
        playerHealth?.SetGuarding(false);
        SetPlayerAsActiveAttacker();

        Vector3 attackPosition = GetAttackPosition();
        yield return MoveTo(attackPosition);

        animator.SetBool(IsMovingHash, false);
        animator.SetTrigger(Attack1Hash);
        yield return new WaitForSeconds(attack1Duration);

        if (useSecondAttack)
        {
            animator.SetTrigger(Attack2Hash);
            yield return new WaitForSeconds(attack2Duration);
        }

        if (enemyHealth == null)
        {
            Debug.LogWarning("PlayerBattleActions cannot deal damage because Enemy Health is not assigned.", this);
        }
        else
        {
            if (useSecondAttack)
            {
                RuntimeSkill skill = skillSet.GetSkill(SkillId.DoubleAttack);
                if (skill != null)
                {
                    int damagePerHit = GetModifiedDamage(skill.DamagePerHit, SkillId.DoubleAttack);
                    int totalDealt = 0;
                    for (int hitIndex = 0; hitIndex < skill.HitCount; hitIndex++)
                    {
                        totalDealt += enemyHealth.TakeDamage(damagePerHit);
                        if (enemyHealth.IsDefeated) break;
                    }
                    ApplyLifesteal(SkillId.DoubleAttack, totalDealt);
                }
            }
            else
            {
                RuntimeSkill skill = skillSet.GetSkill(SkillId.Attack);
                if (skill != null)
                {
                    int dealt = enemyHealth.TakeDamage(GetModifiedDamage(skill.BasePower, SkillId.Attack));
                    ApplyLifesteal(SkillId.Attack, dealt);
                }
            }

            nextAttackDamageMultiplier = 1f;
        }

        yield return new WaitForSeconds(pauseBeforeReturn);
        yield return MoveTo(startPosition);

        animator.SetBool(IsMovingHash, false);
        FaceEnemy();
        ResetSortingOrder();

        if (victoryPending)
        {
            ShowVictoryPanel();
            currentAction = null;
            yield break;
        }

        if (enemyActions != null)
        {
            yield return StartCoroutine(enemyActions.PlayChosenAction());
        }

        currentAction = null;
        SetActionMenuInteractable(true);
    }

    private IEnumerator DefendRoutine()
    {
        animator.SetBool(IsMovingHash, false);
        FaceEnemy();
        RuntimeSkill guardSkill = skillSet.GetSkill(SkillId.Guard);
        if (guardSkill != null)
        {
            playerHealth?.SetGuardDamageMultiplier(guardSkill.GuardDamageMultiplier);
        }
        playerHealth?.SetGuarding(true);
        animator.SetBool(IsGuardingHash, true);

        yield return new WaitForSeconds(guardDuration);

        animator.SetBool(IsGuardingHash, false);
        FaceEnemy();
        ResetSortingOrder();

        if (enemyActions != null)
        {
            yield return StartCoroutine(enemyActions.PlayChosenAction());
        }

        playerHealth?.SetGuarding(false);
        currentAction = null;
        SetActionMenuInteractable(true);
    }

    private IEnumerator BoostRoutine()
    {
        animator.SetBool(IsMovingHash, false);
        FaceEnemy();

        RuntimeSkill boostSkill = skillSet.GetSkill(SkillId.Boost);
        if (boostSkill != null && boostSkill.NextAttackDamageBonus > 0f)
        {
            nextAttackDamageMultiplier = Mathf.Max(nextAttackDamageMultiplier, 1f + boostSkill.NextAttackDamageBonus);
            Debug.Log($"boost empowered next attack. Multiplier: {nextAttackDamageMultiplier}", this);
        }

        yield return new WaitForSeconds(guardDuration);

        if (enemyActions != null)
        {
            yield return StartCoroutine(enemyActions.PlayChosenAction());
        }

        currentAction = null;
        SetActionMenuInteractable(true);
    }

    private Vector3 GetAttackPosition()
    {
        Vector3 directionFromEnemy = (transform.position - enemy.position).normalized;
        if (directionFromEnemy == Vector3.zero)
        {
            directionFromEnemy = Vector3.left;
        }

        Vector3 attackPosition = enemy.position + directionFromEnemy * stoppingDistance;
        attackPosition.z = transform.position.z;
        return attackPosition;
    }

    private IEnumerator MoveTo(Vector3 targetPosition)
    {
        animator.SetBool(IsMovingHash, true);

        while ((transform.position - targetPosition).sqrMagnitude > 0.0025f)
        {
            Vector3 nextPosition = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            FaceMoveDirection(nextPosition - transform.position);
            transform.position = nextPosition;
            yield return null;
        }

        transform.position = targetPosition;
    }

    private void FaceMoveDirection(Vector3 direction)
    {
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }

    private void FaceEnemy()
    {
        if (spriteRenderer != null && enemy != null)
        {
            spriteRenderer.flipX = enemy.position.x < transform.position.x;
        }
    }

    private void SetPlayerAsActiveAttacker()
    {
        RefreshEnemyRenderer();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = activeAttackerSortingOrder;
        }

        if (enemySpriteRenderer != null)
        {
            enemySpriteRenderer.sortingOrder = defaultSortingOrder;
        }
    }

    private void ResetSortingOrder()
    {
        RefreshEnemyRenderer();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = defaultSortingOrder;
        }

        if (enemySpriteRenderer != null)
        {
            enemySpriteRenderer.sortingOrder = defaultSortingOrder;
        }
    }

    private void RefreshEnemyRenderer()
    {
        if (enemy != null && enemySpriteRenderer == null)
        {
            enemySpriteRenderer = enemy.GetComponent<SpriteRenderer>();
        }
    }

    private void HandleEnemyDefeated(CombatantHealth defeatedCombatant)
    {
        battleEnded = true;
        victoryPending = true;
        SetActionMenuInteractable(false);
    }

    private void ShowVictoryPanel()
    {
        if (victoryRewardPanel == null && autoFindVictoryRewardPanel)
        {
            victoryRewardPanel = FindFirstObjectByType<VictoryRewardPanel>(FindObjectsInactive.Include);
        }

        if (victoryRewardPanel == null)
        {
            Debug.LogWarning("PlayerBattleActions cannot show victory UI because Victory Reward Panel is not assigned.", this);
            return;
        }

        victoryRewardPanel.Show(this);
    }

    /// <summary>Applies a modifier from the library to the player's skill set and resets the battle.</summary>
    public void ApplyModifierReward(ModifierDefinition modifier)
    {
        if (modifier == null)
        {
            Debug.LogWarning("PlayerBattleActions: ApplyModifierReward called with null modifier.", this);
            return;
        }

        string log = skillSet.ApplyModifier(modifier);
        Debug.Log($"Modifier reward applied: {modifier.DisplayName}. {log}", this);
        ResetBattleForNextTry();
        RefreshSkillButtonUIs();
    }

    public PlayerSkillSet SkillSet => skillSet;

    private void ApplyLifesteal(SkillId skillId, int damageDealt)
    {
        if (playerHealth == null || skillSet == null || damageDealt <= 0) return;

        foreach (ModifierDefinition mod in skillSet.GetAppliedModifiers(skillId))
        {
            if (mod.EffectType == ModifierEffectType.Lifesteal)
                playerHealth.Heal(Mathf.CeilToInt(damageDealt * mod.PrimaryValue));
        }
    }

    private int GetModifiedDamage(int baseDamage, SkillId skillId)
    {
        float multiplier = nextAttackDamageMultiplier;

        if (skillSet != null && playerHealth != null)
        {
            float hpRatio = playerHealth.MaxHealth > 0
                ? (float)playerHealth.CurrentHealth / playerHealth.MaxHealth
                : 1f;

            foreach (ModifierDefinition mod in skillSet.GetAppliedModifiers(skillId))
            {
                switch (mod.EffectType)
                {
                    case ModifierEffectType.HighHpPowerBoost when hpRatio > mod.SecondaryValue:
                        baseDamage += (int)mod.PrimaryValue;
                        break;
                    case ModifierEffectType.LowHpPowerBoost:
                        int steps = Mathf.FloorToInt((1f - hpRatio) / mod.SecondaryValue);
                        multiplier += steps * mod.PrimaryValue;
                        break;
                }
            }
        }

        return Mathf.CeilToInt(baseDamage * multiplier);
    }

    private void ResetBattleForNextTry()
    {
        battleEnded = false;
        victoryPending = false;
        nextAttackDamageMultiplier = 1f;
        currentAction = null;

        playerHealth?.SetGuarding(false);
        playerHealth?.ResetHealth();
        enemyHealth?.SetGuarding(false);
        enemyHealth?.ResetHealth();

        transform.position = startPosition;
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsGuardingHash, false);
        FaceEnemy();
        ResetSortingOrder();
        SetActionMenuInteractable(true);

        Debug.Log("Battle reset after reward selection. Player and enemy health restored for another test.", this);
    }

    private void RefreshSkillButtonUIs()
    {
        if (skillButtonUIs == null || skillSet == null) return;

        int count = Mathf.Min(skillButtonUIs.Length, skillButtonIds?.Length ?? 0);
        for (int i = 0; i < count; i++)
        {
            if (skillButtonUIs[i] == null) continue;

            RuntimeSkill skill = skillSet.GetSkill(skillButtonIds[i]);
            if (skill == null) continue;

            skillButtonUIs[i].Bind(skill, skillSet.GetAppliedModifiers(skillButtonIds[i]));
        }
    }

    private bool CanStartPlayerAction()
    {
        return !battleEnded && currentAction == null;
    }

    private void SetActionMenuInteractable(bool interactable)
    {
        RefreshActionMenuButtons();

        if (actionMenuButtons == null)
        {
            return;
        }

        foreach (Button button in actionMenuButtons)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }

    private void RefreshActionMenuButtons()
    {
        if (actionMenuRoot == null && autoFindActionMenu)
        {
            Transform actionMenu = FindChildRecursive("ActionMenu");
            if (actionMenu != null)
            {
                actionMenuRoot = actionMenu.gameObject;
            }
        }

        if (actionMenuRoot != null && (actionMenuButtons == null || actionMenuButtons.Length == 0))
        {
            actionMenuButtons = actionMenuRoot.GetComponentsInChildren<Button>(true);
        }
    }

    private Transform FindChildRecursive(string childName)
    {
        GameObject[] roots = gameObject.scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            Transform child = FindChildRecursive(root.transform, childName);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }

        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
