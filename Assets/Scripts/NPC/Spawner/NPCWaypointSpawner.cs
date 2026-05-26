using UnityEngine;

public class NPCWaypointSpawner : MonoBehaviour
{
    [Header("Danh sách xe khách (Kéo nhiều xe vào đây)")]
    [SerializeField] private GameObject[] npcPrefabs;
    public bool spawnAutomatically = true;
    public bool loopQueue = true;

    [Header("Thiết lập Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private WaypointPath waypointPath;

    [Header("KỊCH BẢN ĐÁNH TRÁO (SWAP)")]
    public bool pauseAndHideAtFirstPath = false;

    [Tooltip("KÉO OBJECT TRẠM SỬA XE VÀO ĐÂY (Thay cho cái Event rườm rà)")]
    public RepairStation repairStation;

    [Header("Waypoint tiếp theo")]
    [SerializeField] private bool useNextPathAndDespawn = false;
    [SerializeField] private WaypointPath nextWaypointPath;
    [SerializeField] private bool despawnAtPathEnd = false;

    [Header("Thời gian chờ giữa các lần đẻ xe (giây)")]
    [SerializeField] private float spawnDelaySeconds = 60f;
    [SerializeField] private bool testSpawnNow = false;

    private float spawnTimer;
    private bool hasSpawned;
    private NPCMovement activeMovement;
    private PatrolBehavior activePatrol;
    private bool hasSwitchedToNextPath;
    private bool isPaused = false;
    private int currentNpcIndex = 0;
    private bool isFirstSpawn = true;

    void Update()
    {
        if (!hasSpawned)
        {
            if (testSpawnNow)
            {
                testSpawnNow = false;
                SpawnNpc();
                return;
            }

            if (spawnAutomatically)
            {
                if (isFirstSpawn)
                {
                    SpawnNpc();
                    isFirstSpawn = false;
                }
                else
                {
                    spawnTimer += Time.deltaTime;
                    if (spawnTimer >= spawnDelaySeconds) SpawnNpc();
                }
            }
        }
        else if (!isPaused)
        {
            if (activePatrol != null && activePatrol.HasCompletedPath())
            {
                if (pauseAndHideAtFirstPath && !hasSwitchedToNextPath)
                {
                    activePatrol.gameObject.SetActive(false);
                    isPaused = true;

                    // LẤY HỒ SƠ TỪ XE VÀ ĐƯA CHO TRẠM SỬA CHỮA
                    if (repairStation != null)
                    {
                        VehicleJobConfig config = activePatrol.GetComponent<VehicleJobConfig>();
                        repairStation.SetupAndShowJob(config);
                    }
                    return;
                }

                if (useNextPathAndDespawn && !hasSwitchedToNextPath && nextWaypointPath != null)
                {
                    hasSwitchedToNextPath = true;
                    nextWaypointPath.isLoop = false;
                    activePatrol.SetPath(nextWaypointPath);
                    activeMovement.SetBehavior(activePatrol);
                }
                else if (despawnAtPathEnd || (useNextPathAndDespawn && hasSwitchedToNextPath))
                {
                    Destroy(activePatrol.gameObject);

                    activeMovement = null;
                    activePatrol = null;
                    hasSpawned = false;
                    spawnTimer = 0f;

                    currentNpcIndex++;
                    if (currentNpcIndex >= npcPrefabs.Length)
                    {
                        if (loopQueue) currentNpcIndex = 0;
                        else spawnAutomatically = false;
                    }
                }
            }
        }
    }

    public void ResumeHiddenNPCAndDriveAway()
    {
        if (isPaused && activePatrol != null && nextWaypointPath != null)
        {
            Transform startOfPath2 = nextWaypointPath.GetWaypoint(0).transform;
            activePatrol.transform.position = startOfPath2.position;
            activePatrol.transform.rotation = startOfPath2.rotation;

            activePatrol.gameObject.SetActive(true);
            isPaused = false;
            hasSwitchedToNextPath = true;
            nextWaypointPath.isLoop = false;
            activePatrol.SetPath(nextWaypointPath);
            activeMovement.SetBehavior(activePatrol);
        }
    }

    public void SpawnNpc()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0 || waypointPath == null) return;
        if (currentNpcIndex >= npcPrefabs.Length) return;

        Transform spawnTransform = ResolveSpawnTransform();
        GameObject prefabToSpawn = npcPrefabs[currentNpcIndex];
        if (prefabToSpawn == null) return;

        GameObject newNpc = Instantiate(prefabToSpawn, spawnTransform.position, spawnTransform.rotation);

        NPCMovement movement = newNpc.GetComponent<NPCMovement>();
        if (movement == null) { Destroy(newNpc); return; }

        PatrolBehavior patrol = newNpc.GetComponent<PatrolBehavior>();
        if (patrol == null) patrol = newNpc.AddComponent<PatrolBehavior>();

        waypointPath.isLoop = false;
        patrol.SetPath(waypointPath);
        movement.SetBehavior(patrol);

        activeMovement = movement;
        activePatrol = patrol;
        hasSpawned = true;
        isPaused = false;
        hasSwitchedToNextPath = false;
        spawnTimer = 0f;
    }

    private Transform ResolveSpawnTransform()
    {
        if (spawnPoint != null) return spawnPoint;
        if (waypointPath != null)
        {
            Waypoint firstWaypoint = waypointPath.GetWaypoint(0);
            if (firstWaypoint != null) return firstWaypoint.transform;
        }
        return transform;
    }
}