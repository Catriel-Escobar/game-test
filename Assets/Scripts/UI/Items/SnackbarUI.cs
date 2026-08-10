using System.Collections;
using TMPro;
using UnityEngine;

public class SnackbarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _messageText;

    [Header("Behavior")]
    [SerializeField] private float _displayDuration = 2f;
    [SerializeField] private float _fadeSpeed = 5f;

    private Coroutine _routine;
    private bool _subscribed;

    private void Awake()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        if (_messageText == null) _messageText = GetComponentInChildren<TMP_Text>();

        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
    }

    public void Initialize(Player player)
    {
        Subscribe(player);
    }

    private void Subscribe(Player player)
    {
        if (_subscribed) return;
        _subscribed = true;

        if (player?.Inventory != null)
            player.Inventory.OnInventoryFull += OnInventoryFull;
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;
        _subscribed = false;
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnInventoryFull()
    {
        string message = ConfigBoostrap.Current?.LocalizationConfig?.Get("inventory.full");
        Show(string.IsNullOrEmpty(message) ? "Inventory is full" : message);
    }

    public void Show(string message)
    {
        if (_messageText != null)
            _messageText.text = message;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(_displayDuration);

        if (_canvasGroup == null) yield break;

        while (_canvasGroup.alpha > 0f)
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 0f, _fadeSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }
}
