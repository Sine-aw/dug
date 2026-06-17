using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public enum EnemyType { BasicMelee, Ranged, FastChaser, Bomber, Boss }

    [Header("적 종류 설정")]
    public EnemyType enemyType;

    [Header("기본 능력치")]
    public float maxHealth = 30f;
    private float currentHealth;
    public float moveSpeed = 2.5f;
    public float damage = 10f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    [Header("인식 및 사거리")]
    public float detectionRange = 8f;
    public float attackRange = 1.5f;

    [Header("원거리/보스 설정")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public GameObject minionPrefab;

    [Header("자폭병 전용 설정")]
    public float explosionRadius = 2f;
    private bool isExploding = false;

    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isKnockback = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        currentHealth = maxHealth;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        FindPlayerTarget();

        SetupStatsByType();
    }

    // 플레이어를 유실하여 가만히 있는 경우를 대비한 자동 타겟 지정 보완 시스템
    void FindPlayerTarget()
    {
        // 1. 태그로 찾기 시도
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        // 2. 태그가 유실되었을 경우 플레이어 컴포넌트를 찾아 자동으로 강제 지정하는 보완 장치
        if (playerObj == null)
        {
            DeepPlayerStats stats = Object.FindAnyObjectByType<DeepPlayerStats>();
            if (stats != null) playerObj = stats.gameObject;
        }

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: 플레이어 대상(DeepPlayerStats 혹은 Player 태그)을 씬에서 찾을 수 없어 멈춤 상태입니다!");
        }
    }

    void SetupStatsByType()
    {
        switch (enemyType)
        {
            case EnemyType.BasicMelee:
                break;
            case EnemyType.Ranged:
                maxHealth = 20f;
                moveSpeed = 2f;
                attackRange = 5f;
                break;
            case EnemyType.FastChaser:
                maxHealth = 15f;
                moveSpeed = 4f;
                attackCooldown = 0.8f;
                break;
            case EnemyType.Bomber:
                maxHealth = 12f;
                moveSpeed = 3.5f;
                attackRange = 1.2f;
                break;
            case EnemyType.Boss:
                maxHealth = 200f;
                moveSpeed = 1.8f;
                attackRange = 4f;
                break;
        }
        currentHealth = maxHealth;
    }

    void Update()
    {
        // 만약 예외 상황으로 타겟을 놓친 상태면 실시간으로 재추적합니다.
        if (player == null)
        {
            FindPlayerTarget();
            if (player == null) return;
        }

        if (isExploding || isKnockback) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            LookAtPlayer();

            if (distanceToPlayer <= attackRange)
            {
                rb.linearVelocity = Vector2.zero;
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    ExecuteAttack();
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                MoveTowardsPlayer(distanceToPlayer);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void MoveTowardsPlayer(float distance)
    {
        Vector2 direction = (player.position - transform.position).normalized;

        if (enemyType == EnemyType.Ranged && distance < 3f)
        {
            rb.linearVelocity = -direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
    }

    void ExecuteAttack()
    {
        switch (enemyType)
        {
            case EnemyType.BasicMelee:
            case EnemyType.FastChaser:
                MeleeAttack();
                break;
            case EnemyType.Ranged:
                ShootProjectile();
                break;
            case EnemyType.Bomber:
                StartCoroutine(ExplodeSequence());
                break;
            case EnemyType.Boss:
                int pattern = Random.Range(0, 3);
                if (pattern == 0) ShootProjectile();
                else if (pattern == 1) BossRingAttack();
                else SummonMinions();
                break;
        }
    }

    void MeleeAttack()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.AddForce(dir * 5f, ForceMode2D.Impulse);

        DeepPlayerStats stats = player.GetComponent<DeepPlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(damage);
        }
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        GameObject proj = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);

        Vector2 dir = (player.position - spawnPoint.position).normalized;

        EnemyProjectile projScript = proj.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.Setup(dir, damage);
        }
    }

    void BossRingAttack()
    {
        if (projectilePrefab == null) return;

        int bulletCount = 8;
        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float bulletDirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180f);
            float bulletDirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180f);

            Vector2 bulletVector = new Vector2(bulletDirX, bulletDirY);
            Vector2 bulletDirection = (bulletVector - (Vector2)transform.position).normalized;

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            EnemyProjectile projScript = proj.GetComponent<EnemyProjectile>();
            if (projScript != null)
            {
                projScript.Setup(bulletDirection, damage * 0.8f);
            }

            angle += angleStep;
        }
    }

    void SummonMinions()
    {
        if (minionPrefab == null) return;

        for (int i = 0; i < 2; i++)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0f);
            Instantiate(minionPrefab, transform.position + spawnOffset, Quaternion.identity);
        }
    }

    IEnumerator ExplodeSequence()
    {
        isExploding = true;
        rb.linearVelocity = Vector2.zero;

        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.15f);
        }

        float finalDist = Vector2.Distance(transform.position, player.position);
        if (finalDist <= explosionRadius)
        {
            DeepPlayerStats stats = player.GetComponent<DeepPlayerStats>();
            if (stats != null) stats.TakeDamage(damage * 1.5f);
        }

        Die();
    }

    public void TakeDamage(float amount, Vector2 knockbackDir)
    {
        if (isExploding) return;

        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        StartCoroutine(KnockbackRoutine(knockbackDir));

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    IEnumerator KnockbackRoutine(Vector2 dir)
    {
        isKnockback = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * 4f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = Vector2.zero;
        isKnockback = false;
    }

    void Die()
    {
        if (player != null)
        {
            DeepPlayerStats stats = player.GetComponent<DeepPlayerStats>();
            if (stats != null) stats.AddKill();
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}