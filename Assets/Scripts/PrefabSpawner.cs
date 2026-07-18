using UnityEngine;
using UnityEngine.AI;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int amount = 10;
    [SerializeField] private float radius = 10f;
    [SerializeField] private string enemyId = "zombie";

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        for (int i = 0; i < amount; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * radius;

            Vector3 spawnPosition = transform.position +
                                    new Vector3(randomPoint.x, 0f, randomPoint.y);

            GameObject obj = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity);

            MobSpawnData spawnData = new()
            {
                SpawnPosition = spawnPosition,
                PatrolRadius = radius,
                EnemyId = enemyId
            };

            
            if (obj.TryGetComponent<Mob>(out var mob))
            {
                mob.Initialize(spawnData);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}