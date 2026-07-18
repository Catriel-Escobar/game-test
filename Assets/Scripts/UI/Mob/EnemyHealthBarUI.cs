using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
[SerializeField] private TMP_Text mobNameText;
[SerializeField] private TMP_Text hpText;
[SerializeField] private Image hpFill;
    private Mob _mob;
    private MobResources _resources;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mob == null)
            return;

        transform.position =
            _camera.WorldToScreenPoint(
                _mob.transform.position + Vector3.up * 2f);
    }
    public void Initialize(Mob mob)
    {
        _mob = mob;
        _resources = mob.GetComponent<MobResources>();

        mobNameText.text = mob.Name;

        _resources.OnHealthChanged += OnHealthChanged;
        _resources.OnDeath += OnDeath;

        OnHealthChanged(
            _resources.CurrentHp,
            _resources.MaxHp);
    }

    private void OnHealthChanged(int current, int max)
    {
        hpFill.fillAmount = (float)current / max;

        hpText.text = $"{current}/{max}";
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_resources == null)
            return;

        _resources.OnHealthChanged -= OnHealthChanged;
        _resources.OnDeath -= OnDeath;
    }
}