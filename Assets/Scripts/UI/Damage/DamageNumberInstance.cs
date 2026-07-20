using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumberInstance : MonoBehaviour
{
    private TextMeshProUGUI text;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(string value, Color color, float scale)
    {
        text.text = value;
        text.color = color;
        transform.localScale = Vector3.one * scale;
        canvasGroup.alpha = 1f;
    }

    public void Animate(Vector3 worldPos, float speed, float duration, Camera cam, Action onFinished)
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(AnimateRoutine(worldPos, speed, duration, cam, onFinished));
    }

    public void Deactivate()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateRoutine(Vector3 worldPos, float speed, float duration, Camera cam, Action onFinished)
    {
        Vector3 startPos = cam.WorldToScreenPoint(worldPos);
        rectTransform.position = startPos;

        float elapsed = 0f;
        float punchDuration = 0.1f;
        Vector3 punchScale = Vector3.one * 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 pos = cam.WorldToScreenPoint(worldPos + Vector3.up * speed * elapsed);
            rectTransform.position = pos;

            if (elapsed < punchDuration)
            {
                float pt = elapsed / punchDuration;
                transform.localScale = Vector3.Lerp(punchScale, Vector3.one, pt);
            }

            canvasGroup.alpha = 1f - t;

            yield return null;
        }

        canvasGroup.alpha = 0f;
        activeCoroutine = null;
        onFinished?.Invoke();
    }
}
