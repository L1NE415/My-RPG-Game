using UnityEngine;
using System.Collections;

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

    [Header("Damage")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private int doubleAttackDamage = 40;

    [Header("Animation Timing")]
    [SerializeField] private float attack1Duration = 0.55f;
    [SerializeField] private float attack2Duration = 0.55f;
    [SerializeField] private float guardDuration = 0.8f;
    [SerializeField] private float pauseBeforeReturn = 0.15f;

    [Header("Sorting")]
    [SerializeField] private int defaultSortingOrder = 5;
    [SerializeField] private int activeAttackerSortingOrder = 10;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer enemySpriteRenderer;
    private Vector3 startPosition;
    private Coroutine currentAction;

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

        if (enemyActions == null)
        {
            Debug.LogWarning("PlayerBattleActions: Enemy Actions is not assigned.", this);
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<CombatantHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerBattleActions: Player Health is not assigned.", this);
        }

        if (enemyHealth == null)
        {
            Debug.LogWarning("PlayerBattleActions: Enemy Health is not assigned.", this);
        }
    }

    public void Attack()
    {
        Debug.Log("player action: attack", this);
        StartAction(AttackRoutine(false));
    }

    public void DoubleAttack()
    {
        Debug.Log("player action: double attack", this);
        StartAction(AttackRoutine(true));
    }

    public void Defend()
    {
        Debug.Log("player action: guard", this);
        StartAction(DefendRoutine());
    }

    public void Boost()
    {
        // Reserved for a later skill.
    }

    private void StartAction(IEnumerator action)
    {
        StopCurrentAction();

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

        int damage = useSecondAttack ? doubleAttackDamage : attackDamage;
        if (enemyHealth == null)
        {
            Debug.LogWarning($"PlayerBattleActions cannot deal {damage} damage because Enemy Health is not assigned.", this);
        }
        else
        {
            enemyHealth.TakeDamage(damage);
        }

        yield return new WaitForSeconds(pauseBeforeReturn);
        yield return MoveTo(startPosition);

        animator.SetBool(IsMovingHash, false);
        FaceEnemy();
        ResetSortingOrder();

        if (enemyActions != null)
        {
            yield return StartCoroutine(enemyActions.PlayChosenAction());
        }

        currentAction = null;
    }

    private IEnumerator DefendRoutine()
    {
        animator.SetBool(IsMovingHash, false);
        FaceEnemy();
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
}
