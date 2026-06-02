using UnityEngine;
using System.Collections;
using System.Reflection;

/// <summary>
/// Gán vào m?t GameObject trong m?i scene (ví d? object SettingsRoot).
/// Kéo th? các object Volumetric Cloud và PostProcessing vào các m?ng trong Inspector.
// Script này s? ??ng ký v?i GraphicManager khi scene kh?i t?o.
/// </summary>
public class SceneGraphicsRegistrar : MonoBehaviour
{
    [Header("Scene Volumetric Clouds / Post Processing")]
    public GameObject[] volumetricCloudObjects;
    public GameObject[] postProcessingObjects;

    IEnumerator Start()
    {
        const int maxAttempts = 60;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // Find any MonoBehaviour named "GraphicManager"
            MonoBehaviour[] all = FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < all.Length; i++)
            {
                var mb = all[i];
                if (mb == null) continue;
                var t = mb.GetType();
                if (t.Name == "GraphicManager")
                {
                    // Found GraphicManager instance, call SetSceneObjects via reflection
                    MethodInfo mi = t.GetMethod("SetSceneObjects", BindingFlags.Public | BindingFlags.Instance);
                    if (mi != null)
                    {
                        mi.Invoke(mb, new object[] { volumetricCloudObjects, postProcessingObjects });
                    }
                    yield break;
                }
            }

            attempts++;
            yield return null;
        }

        Debug.LogWarning("GraphicManager not found after waiting. Make sure a persistent GraphicManager exists in the initial scene.");
    }
}
