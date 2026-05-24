using UnityEngine;
using UnityEngine.InputSystem;

public class NPCWaypointSpawner : MonoBehaviour
{
    [Header("Bản mẫu NPC (Phải có NPCMovement)")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private bool useExistingNpcIfInScene = true;

    [Header("Thiết lập Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private WaypointPath waypointPath;

    [Header("Waypoint tiếp theo")]
    [SerializeField] private bool useNextPathAndDespawn = false;
    [SerializeField] private WaypointPath nextWaypointPath;

    [Header("Despawn")]
    [SerializeField] private bool despawnAtPathEnd = false;

    [Header("Thời gian chờ sau input (giây)")]
    [SerializeField] private float spawnDelaySeconds = 15f;

    [Header("Test: bật để spawn ngay")]
    [SerializeField] private bool testSpawnNow = false;

    private float idleTimer;
    private bool hasSpawned;
    private NPCMovement activeMovement;
    private PatrolBehavior activePatrol;
    private bool hasSwitchedToNextPath;
    private bool usingExistingNpc;

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

            if (AnyInputThisFrame())
            {
                idleTimer = 0f;
            }
            else
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= spawnDelaySeconds)
                {
                    SpawnNpc();
                }
            }
        }
        else
        {
            if (activePatrol != null && activePatrol.HasCompletedPath())
            {
                if (useNextPathAndDespawn && !hasSwitchedToNextPath && nextWaypointPath != null)
                {
                    hasSwitchedToNextPath = true;
                    nextWaypointPath.isLoop = false;
                    activePatrol.SetPath(nextWaypointPath);
                    activeMovement.SetBehavior(activePatrol);
                }
                else if (despawnAtPathEnd)
                {
                    if (usingExistingNpc)
                    {
                        activePatrol.gameObject.SetActive(false);
                    }
                    else
                    {
                        Destroy(activePatrol.gameObject);
                    }

                    activeMovement = null;
                    activePatrol = null;
                    usingExistingNpc = false;
                }
            }
        }
    }

    private void SpawnNpc()
    {
        if (npcPrefab == null || waypointPath == null)
        {
            Debug.LogWarning("Thiếu thông tin! Không thể spawn NPC.");
            return;
        }

        Transform spawnTransform = ResolveSpawnTransform();
        GameObject newNpc = ResolveNpcInstance(spawnTransform.position, spawnTransform.rotation);
        if (newNpc == null)
        {
            Debug.LogWarning("Không thể tạo hoặc dùng NPC instance.");
            return;
        }
        NPCMovement movement = newNpc.GetComponent<NPCMovement>();
        if (movement == null)
        {
            Debug.LogWarning("NPC prefab thiếu NPCMovement.");
            Destroy(newNpc);
            return;
        }

        PatrolBehavior patrol = newNpc.GetComponent<PatrolBehavior>();
        if (patrol == null)
        {
            patrol = newNpc.AddComponent<PatrolBehavior>();
        }

        waypointPath.isLoop = false;
        patrol.SetPath(waypointPath);
        movement.SetBehavior(patrol);

        activeMovement = movement;
        activePatrol = patrol;
        hasSpawned = true;
        idleTimer = 0f;
        hasSwitchedToNextPath = false;
    }

    private Transform ResolveSpawnTransform()
    {
        if (spawnPoint != null)
        {
            return spawnPoint;
        }

        if (waypointPath != null)
        {
            Waypoint firstWaypoint = waypointPath.GetWaypoint(0);
            if (firstWaypoint != null)
            {
                return firstWaypoint.transform;
            }
        }

        return transform;
    }

    private GameObject ResolveNpcInstance(Vector3 position, Quaternion rotation)
    {
        if (npcPrefab == null)
        {
            return null;
        }

        if (useExistingNpcIfInScene && npcPrefab.scene.IsValid())
        {
            usingExistingNpc = true;
            npcPrefab.transform.SetPositionAndRotation(position, rotation);
            if (!npcPrefab.activeSelf)
            {
                npcPrefab.SetActive(true);
            }

            return npcPrefab;
        }

        usingExistingNpc = false;
        return Instantiate(npcPrefab, position, rotation);
    }

    private bool AnyInputThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame ||
                                      Mouse.current.rightButton.wasPressedThisFrame ||
                                      Mouse.current.middleButton.wasPressedThisFrame))
        {
            return true;
        }

        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            return true;
        }

        return false;
    }
}
