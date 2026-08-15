using System.Collections.Generic;
using UnityEngine;

public static class SkillVfxSpawner
{
    private static readonly Dictionary<string, GameObject> Cache = new Dictionary<string, GameObject>();

    public static GameObject Spawn(string prefabPath, Vector3 position, Quaternion rotation, float duration = 0f)
    {
        if (string.IsNullOrEmpty(prefabPath)) return null;

        GameObject prefab = Load(prefabPath);
        if (prefab == null) return null;

        GameObject instance = Object.Instantiate(prefab, position, rotation);
        if (duration > 0f)
            Object.Destroy(instance, Mathf.Max(0.01f, duration));

        return instance;
    }

    public static GameObject Load(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath)) return null;

        if (Cache.TryGetValue(prefabPath, out GameObject cached))
            return cached;

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[Skills][VFX] Prefab '{prefabPath}' no encontrado en Resources.");
            return null;
        }

        Cache[prefabPath] = prefab;
        return prefab;
    }
}
