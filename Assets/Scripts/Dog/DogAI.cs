using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DogAI : MonoBehaviour
{
    public enum DogState { Idle, Wander, Chase, Attack, Dead }
    [Header("Current State")]
    public DogState currentState = DogState.Idle;

    [Header("--- HEALTH ---")]
    public int maxHealth = 30;
    private int currentHealth;

    [Header("--- WANDERING ---")]
    public Transform wanderCenter;
    public float wanderRadius = 10f;
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;
    private float idleTimer;

    [Header("--- COMBAT ---")]
    public float attackRange = 1.5f;
    public float timeBetweenAttacks = 1.5f;
    public int attackDamage = 10;

    public GameObject dogPrefab;
    public int packSize = 3;

    [Header("--- MOVEMENT ---")]
    public NavMeshAgent agent;
    public Animator anim;

    [Header("--- AUDIO ---")]
    public AudioSource audioSource;
    public AudioClip[] hurtSounds;
    public AudioClip[] barkSounds;
    public AudioClip[] biteSounds;

    [Header("Bark frequency (Seconds)")]
    public float minBarkDelay = 4f;
    public float maxBarkDelay = 10f;

    [Tooltip("Thời gian sủa dồn dập khi nổi điên (Cực nhanh)")]
    public float minAggroBarkDelay = 0.2f;
    public float maxAggroBarkDelay = 0.6f;

    private float barkTimer;

    [Header("--- POOP ---")]
    [Tooltip("Kéo Prefab cục chất thải vào đây (Có thể dùng file .obj tĩnh)")]
    public GameObject poopPrefab;
    [Tooltip("Kéo các file âm thanh tiếng Fart vào đây")]
    public AudioClip[] fartSounds;
    [Tooltip("Góc lật của cục mìn (Nếu ngược thì thử X = 180, 90 hoặc -90)")]
    public Vector3 poopRotationOffset = new Vector3(-90f, 0f, 0f); // <-- THÊM BIẾN LẬT TRỤC

    [Tooltip("Cứ sau khoảng bao nhiêu giây thì đi nặng 1 lần")]
    public float minPoopDelay = 5f;
    public float maxPoopDelay = 12f;
    private float poopTimer;
    // ==========================================

    private Transform playerTarget;
    private bool isAggro = false;
    private float nextAttackTime;

    void Start()
    {
        currentHealth = maxHealth;

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (wanderCenter == null)
        {
            GameObject centerObj = new GameObject("WanderCenter_" + gameObject.name);
            centerObj.transform.position = transform.position;
            wanderCenter = centerObj.transform;
        }

        barkTimer = Random.Range(minBarkDelay, maxBarkDelay);

        // Khởi tạo thời gian đi nặng ngẫu nhiên ban đầu
        poopTimer = Random.Range(minPoopDelay, maxPoopDelay);

        if (!isAggro)
        {
            SetState(DogState.Idle);
        }
    }

    void Update()
    {
        if (currentState == DogState.Dead) return;

        if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);

        HandleBarking();

        // Gọi hàm đi nặng liên tục ở mỗi Frame độc lập với State Machine
        HandlePooping();

        switch (currentState)
        {
            case DogState.Idle:
                HandleIdle();
                break;
            case DogState.Wander:
                HandleWander();
                break;
            case DogState.Chase:
                HandleChase();
                break;
            case DogState.Attack:
                HandleAttack();
                break;
        }
    }


    void HandlePooping()
    {
        poopTimer -= Time.deltaTime;
        if (poopTimer <= 0)
        {
            Poop();
            poopTimer = Random.Range(minPoopDelay, maxPoopDelay);
        }
    }

    void Poop()
    {
        PlayRandomSound(fartSounds);
        if (poopPrefab != null)
        {
            Vector3 spawnPosition = transform.position - (transform.forward * 0.4f);
            spawnPosition.y = transform.position.y - 0.075f;

            Quaternion spawnRotation = Quaternion.Euler(poopRotationOffset.x, Random.Range(0f, 360f), poopRotationOffset.z);

            GameObject freshPoop = Instantiate(poopPrefab, spawnPosition, spawnRotation);

            Destroy(freshPoop, 40f);
        }

        Debug.Log("💩 Chó đã xả mìn thành công!");
    }

    void HandleBarking()
    {
        barkTimer -= Time.deltaTime;
        if (barkTimer <= 0)
        {
            if (!isAggro && (currentState == DogState.Idle || currentState == DogState.Wander))
            {
                PlayRandomSound(barkSounds);
                barkTimer = Random.Range(minBarkDelay, maxBarkDelay);
            }
            else if (isAggro)
            {
                PlayRandomSound(biteSounds);
                barkTimer = Random.Range(minAggroBarkDelay, maxAggroBarkDelay);
            }
        }
    }

    void PlayRandomSound(AudioClip[] clips)
    {
        if (audioSource != null && clips != null && clips.Length > 0)
        {
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }

    void HandleIdle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            SetState(DogState.Wander);
        }
    }

    void HandleWander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetState(DogState.Idle);
        }
    }

    void SetState(DogState newState)
    {
        currentState = newState;

        if (newState == DogState.Idle)
        {
            agent.isStopped = true;
            idleTimer = Random.Range(minIdleTime, maxIdleTime);

            int randomIdle = Random.Range(1, 3);
            if (anim != null) anim.SetInteger("IdleType", randomIdle);
        }
        else if (newState == DogState.Wander)
        {
            agent.isStopped = false;
            if (anim != null) anim.SetInteger("IdleType", 0);

            Vector3 randomPoint = GetRandomNavMeshPoint(wanderCenter.position, wanderRadius);
            agent.SetDestination(randomPoint);
        }
    }

    Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius;
        randomDir += center;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDir, out hit, radius, 1);
        return hit.position;
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (currentState == DogState.Dead) return;

        currentHealth -= damage;
        PlayRandomSound(hurtSounds);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (!isAggro)
        {
            Debug.Log("WOOF! Get rid of it!");
            CallThePack();
            Enrage();
        }
    }

    private void Die()
    {
        currentState = DogState.Dead;

        if (agent != null) agent.enabled = false;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log("The dog is dead....");
        Destroy(gameObject, 3f);
    }

    public void Enrage()
    {
        isAggro = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No player tag to bite!");
        }

        if (anim != null) anim.SetInteger("IdleType", 0);
        agent.isStopped = false;
        barkTimer = 0f;

        currentState = DogState.Chase;
    }

    private void CallThePack()
    {
        if (dogPrefab == null) return;

        for (int i = 0; i < packSize; i++)
        {
            Vector3 spawnPos = GetRandomNavMeshPoint(transform.position, 3f);
            GameObject newDog = Instantiate(dogPrefab, spawnPos, Quaternion.identity);

            DogAI newDogAI = newDog.GetComponent<DogAI>();
            if (newDogAI != null)
            {
                newDogAI.Enrage();
            }
        }
    }

    void HandleChase()
    {
        if (playerTarget == null) return;

        agent.SetDestination(playerTarget.position);
        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= attackRange)
        {
            currentState = DogState.Attack;
        }
    }

    void HandleAttack()
    {
        if (playerTarget == null) return;

        agent.isStopped = true;
        FaceTarget(playerTarget.position);

        if (Time.time >= nextAttackTime)
        {
            if (anim != null) anim.SetTrigger("Bite");

            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.TakeDamage(attackDamage);
                Debug.Log($"Bitten by dog with {attackDamage} health!");

                if (Camera.main != null)
                {
                    CameraController cam = Camera.main.GetComponent<CameraController>();
                    if (cam != null)
                    {
                        cam.TriggerShake(0.15f, 0.2f);
                    }
                }
            }

            nextAttackTime = Time.time + timeBetweenAttacks;
        }

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance > attackRange)
        {
            agent.isStopped = false;
            currentState = DogState.Chase;
        }
    }

    void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }
}