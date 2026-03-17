// File: WaypointPathManager.cs
// Mô tả: Quản lý tất cả WaypointPath trong scene - tìm kiếm, truy xuất path theo tên hoặc ngẫu nhiên.
// Tương tự PathManager trong GTA V - quản lý hàng trăm đường đi cho NPC trong thành phố.
// Singleton pattern để truy cập nhanh từ bất cứ đâu.

using System.Collections.Generic;
using UnityEngine;

public class WaypointPathManager : MonoBehaviour
{
    [Header("=== DANH SÁCH PATH TRONG SCENE ===")]
    [SerializeField] private List<WaypointPath> paths = new List<WaypointPath>();

    // Singleton - chỉ có duy nhất 1 PathManager trong scene
    private static WaypointPathManager instance;
    public static WaypointPathManager Instance => instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Tự động quét toàn bộ scene để tìm tất cả WaypointPath
    /// </summary>
    public void CollectAllPaths()
    {
        paths.Clear();
        WaypointPath[] found = FindObjectsByType<WaypointPath>(FindObjectsSortMode.None);
        paths.AddRange(found);
    }

    /// <summary>
    /// Tìm path theo tên GameObject
    /// </summary>
    public WaypointPath GetPathByName(string pathName)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            if (paths[i] != null && paths[i].gameObject.name == pathName)
            {
                return paths[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Lấy một path ngẫu nhiên trong danh sách
    /// </summary>
    public WaypointPath GetRandomPath()
    {
        if (paths.Count == 0) return null;
        return paths[Random.Range(0, paths.Count)];
    }

    /// <summary>
    /// Lấy toàn bộ danh sách path
    /// </summary>
    public List<WaypointPath> GetAllPaths()
    {
        return paths;
    }
}
