using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;

    [Header("Combat")]
    public float attackRange = 1.5f;
    public float timeBetweenAttacks = 2f; 

    private NavMeshAgent agent;
    private Animator anim;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (player == null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }
    }

    void Update()
    {
        // Nếu không thấy Player, hoặc Agent bị tắt (lúc bị đấm văng), thì AI đứng im
        if (player == null || !agent.enabled) return;

        // 1. Dùng GPS chỉ đường dí theo Player
        agent.SetDestination(player.position);

        // 2. Truyền vận tốc vào Animator để chạy hoạt ảnh Run/Walk (nếu có)
        if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);

        // 3. Kiểm tra xem đã áp sát chưa
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Vào tầm đánh -> Dừng lại!
            agent.isStopped = true;

            // Xoay mặt về phía Player (để đấm không bị trượt)
            FaceTarget(player.position);

            // Đếm thời gian ra đòn
            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + timeBetweenAttacks; // Reset cooldown
            }
        }
        else
        {
            // Ngoài tầm đánh -> Tiếp tục chạy
            agent.isStopped = false;
        }
    }

    private void AttackPlayer()
    {
        // Gọi hoạt ảnh đấm của quái
        if (anim != null) anim.SetTrigger("Attack");
        Debug.Log("Enemy đang tung cú cào vào Player!");

        // Sát thương quái đấm người chơi sẽ được xử lý bằng Hitbox của quái (Giống hệt cách bạn làm cho Player)
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            // Xoay từ từ cho mượt (Time.deltaTime * 5f là tốc độ xoay)
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }
}