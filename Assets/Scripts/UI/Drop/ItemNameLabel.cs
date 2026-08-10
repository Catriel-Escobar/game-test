using TMPro;
using UnityEngine;

public class ItemNameLabel : MonoBehaviour
{
    private TMP_Text _text;
    private WorldDrop _target;
    private Camera _camera;
    private Vector3 _offset;

    private void Awake()
    {
        _text = GetComponentInChildren<TMP_Text>();
        _camera = Camera.main;
    }

    public void Initialize(WorldDrop target, string text, Color color)
    {
        _target = target;
        _text.text = text;
        _text.color = color;
        _offset = new Vector3(0f, 1.15f, 0f);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        if (_camera == null) _camera = Camera.main;
        if (_camera == null) return;

        RectTransform rect = transform as RectTransform;
        if (rect == null || _text == null) return;

        Vector3 worldPos = _target.transform.position + _offset;
        Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);
        bool behind = screenPos.z < 0f;

        if (!behind)
        {
            RectTransform parent = rect.parent as RectTransform;
            if (parent != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent, screenPos, GetEventCamera(), out Vector2 localPoint))
            {
                rect.localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
            }
        }

        if (_text.gameObject.activeSelf != !behind)
            _text.gameObject.SetActive(!behind);
    }

    private Camera GetEventCamera()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return null;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return canvas.worldCamera != null ? canvas.worldCamera : _camera;
    }
}
