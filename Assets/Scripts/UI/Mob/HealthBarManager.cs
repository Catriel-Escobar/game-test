using UnityEngine;

public class EnemyHealthBarManager : MonoBehaviour
{
    public static EnemyHealthBarManager Instance;

    [SerializeField] private EnemyHealthBarUI prefab;
    [SerializeField] private Transform container;

    private void Awake()
    {
        Instance = this;
    }

    public void Create(Mob mob)
    {
        EnemyHealthBarUI ui =
            Instantiate(prefab, container);

        ui.Initialize(mob);
    }
}