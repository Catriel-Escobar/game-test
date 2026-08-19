using System.Collections.Generic;
using UnityEngine;

public static class ImpactVfxSpawner
{
    private static readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();

    public static void Spawn(string prefabPath, Vector3 position, float duration = 1f)
    {
        if (string.IsNullOrEmpty(prefabPath)) return;

        if (!Cache.TryGetValue(prefabPath, out GameObject prefab))
        {
            prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Combat] VFX de impacto '{prefabPath}' no encontrado en Resources.");
                return;
            }

            Cache[prefabPath] = prefab;
        }

        GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity);
        Object.Destroy(instance, Mathf.Max(0.01f, duration));
    }
}
