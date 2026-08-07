using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
[SerializeField] private TMP_Text mobNameText;
[SerializeField] private TMP_Text hpText;
[SerializeField] private Image hpFill;
[SerializeField] private TMP_Text statusText;
    private Mob _mob;
    private MobResources _resources;
    private Camera _camera;
    private string _lastStatus;

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

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (statusText == null)
            return;

        string status = _mob.IsStunned
            ? "STUN"
            : _mob.IsSlowed
                ? "SLOW"
                : string.Empty;

        if (status == _lastStatus)
            return;

        _lastStatus = status;
        statusText.text = status;
        statusText.gameObject.SetActive(!string.IsNullOrEmpty(status));
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