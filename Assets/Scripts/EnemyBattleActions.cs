using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class EnemyBattleActions : MonoBehaviour
{
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DefendHash = Animator.StringToHash("Defend");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private enum EnemyAction
    {
        Attack,
        Defend
    }

    [Header("Targets")]
    [SerializeField] private Transform player;
    [SerializeField] private CombatantHealth playerHealth;
    [SerializeField] private CombatantHealth enemyHealth;

    [Header("Battle Movement")]
    [SerializeField] private string idleStateName = "Lancer_Idle_Black";
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float stoppingDistance = 1.1f;

    [Header("Animation Timing")]
    [SerializeField] private float attackDuration = 0.7f;
    [SerializeField] private float defendDuration = 0.8f;
    [SerializeField] private float pauseBeforeIdle = 0.15f;

    [Header("Damage")]
    [SerializeField] private int attackDamage = 40;

    [Header("Sorting")]
    [SerializeField] private int defaultSortingOrder = 5;
    [SerializeField] private int activeAttackerSortingOrder = 10;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer playerSpriteRenderer;
    private Vector3 startPosition;
    private EnemyAction chosenAction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    private void Start()
    {
        RefreshPlayerRenderer();
        ResetSortingOrder();
        FacePlayer();

        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<CombatantHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning("EnemyBattleActions: Player Health is not assigned.", this);
        }

        if (enemyHealth == null)
        {
            Debug.LogWarning("EnemyBattleActions: Enemy Health is not assigned.", this);
        }
    }

    public void ChooseNextAction()
    {
        chosenAction = Random.value < 0.5f ? EnemyAction.Attack : EnemyAction.Defend;
        RefreshEnemyHealth();
        enemyHealth?.SetGuarding(chosenAction == EnemyAction.Defend);
        Debug.Log($"enemy action: {GetActionLogName(chosenAction)}", this);
    }

    public IEnumerator PlayChosenAction()
    {
        FacePlayer();

        if (chosenAction == EnemyAction.Attack)
        {
            yield return PlayAttack();
        }
        else
        {
            yield return PlayDefend();
        }

        FacePlayer();
        ResetSortingOrder();
        enemyHealth?.SetGuarding(false);
    }

    private IEnumerator PlayAttack()
    {
        Debug.Log("enemy action play: attack", this);

        if (player == null)
        {
            Debug.LogWarning("EnemyBattleActions needs a Player Transform assigned.", this);
            yield break;
        }

        SetEnemyAsActiveAttacker();

        Vector3 attackPosition = GetAttackPosition();
        yield return MoveTo(attackPosition);

        animator.SetBool(IsMovingHash, false);
        animator.Play(idleStateName);
        yield return null;

        animator.ResetTrigger(DefendHash);
        animator.SetTrigger(AttackHash);

        yield return new WaitForSeconds(attackDuration);
        if (playerHealth == null)
        {
            Debug.LogWarning($"EnemyBattleActions cannot deal {attackDamage} damage because Player Health is not assigned.", this);
        }
        else
        {
            playerHealth.TakeDamage(attackDamage);
        }
        yield return new WaitForSeconds(pauseBeforeIdle);

        yield return MoveTo(startPosition);
        animator.SetBool(IsMovingHash, false);
        animator.Play(idleStateName);
    }

    private IEnumerator PlayDefend()
    {
        Debug.Log("enemy action play: guard", this);
        ResetSortingOrder();
        animator.ResetTrigger(AttackHash);
        animator.SetTrigger(DefendHash);

        yield return new WaitForSeconds(defendDuration);
        yield return new WaitForSeconds(pauseBeforeIdle);
    }

    private Vector3 GetAttackPosition()
    {
        Vector3 directionFromPlayer = (transform.position - player.position).normalized;
        if (directionFromPlayer == Vector3.zero)
        {
            directionFromPlayer = Vector3.right;
        }

        Vector3 attackPosition = player.position + directionFromPlayer * stoppingDistance;
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
        animator.SetBool(IsMovingHash, false);
        FacePlayer();
    }

    private void FacePlayer()
    {
        if (spriteRenderer != null && player != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }

    private void FaceMoveDirection(Vector3 direction)
    {
        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }

    private void SetEnemyAsActiveAttacker()
    {
        RefreshPlayerRenderer();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = activeAttackerSortingOrder;
        }

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingOrder = defaultSortingOrder;
        }
    }

    private void ResetSortingOrder()
    {
        RefreshPlayerRenderer();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = defaultSortingOrder;
        }

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingOrder = defaultSortingOrder;
        }
    }

    private void RefreshPlayerRenderer()
    {
        if (player != null && playerSpriteRenderer == null)
        {
            playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        }
    }

    private void RefreshEnemyHealth()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<CombatantHealth>();
        }
    }

    private string GetActionLogName(EnemyAction action)
    {
        return action == EnemyAction.Attack ? "attack" : "guard";
    }
}
