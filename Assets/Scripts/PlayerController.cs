using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 및 공격 설정")]
    public float moveSpeed = 5f;
    public float attackLungeForce = 4f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public LayerMask enemyLayer;
    public Transform attackPoint;
    public Animator animator;
    public UIManager uiManager;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePos;
    private float baseScaleX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        // 이동 입력
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 마우스 위치 (InputSystem)
        if (Camera.main != null)
        {
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            mouseScreenPos.z = Camera.main.nearClipPlane;   // 10f → nearClipPlane 권장
            mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        }

        // 좌우 반전
        if (moveInput.x != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x) * baseScaleX, transform.localScale.y, 1f);

        // 좌클릭 공격
        if (Mouse.current.leftButton.wasPressedThisFrame)
            Attack();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void Attack()
    {
        if (animator) animator.SetTrigger("Attack");

        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        // 기존 속도 초기화 후 돌진
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * attackLungeForce, ForceMode2D.Impulse);

        // 공격 판정
        Vector3 pos = attackPoint != null ? attackPoint.position : transform.position + (Vector3)dir * 0.6f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(attackDamage, dir);
        }
    }
}