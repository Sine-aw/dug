using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float lifetime = 4f;
    private Vector2 moveDirection;
    private float damage;

    void Start()
    {
        Destroy(gameObject, lifetime); // 지정 시간 경과 시 자동 제거 (메모리 관리)
    }

    public void Setup(Vector2 direction, float dmg)
    {
        moveDirection = direction.normalized;
        damage = dmg;

        // 회전 각도 계산하여 플레이어를 조준하도록 방향 설정
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        // 매 프레임 앞방향으로 비행
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 벽 타일맵이나 기타 장애물에 충돌 시 사라짐
        if (collision.CompareTag("Wall") || collision.gameObject.name.Contains("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        // 플레이어 충돌 시 데미지 부여 (DeepPlayerStats로 참조 수정)
        if (collision.CompareTag("Player"))
        {
            DeepPlayerStats stats = collision.GetComponent<DeepPlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}