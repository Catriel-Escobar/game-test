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

        Vector3 worldPos = _target.transform.position + _offset;
        Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);
        bool behind = screenPos.z < 0f;

        transform.position = screenPos;
        if (_text.gameObject.activeSelf != !behind)
            _text.gameObject.SetActive(!behind);
    }
}
