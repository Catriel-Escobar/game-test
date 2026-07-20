using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private InputSystem_Actions _input;
    private PlayerInputs _playerInputs;
    private LocalizationConfig _localization;

    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _resumeText;
    private TextMeshProUGUI _settingsText;
    private TextMeshProUGUI _languageText;
    private TextMeshProUGUI _quitText;

    private bool _isOpen;
    private Coroutine _fadeCoroutine;

    private const float FadeDuration = 0.2f;

    public void Initialize(PlayerInputs playerInputs)
    {
        _playerInputs = playerInputs;
        _localization = ConfigBoostrap.Current.LocalizationConfig;
        _input = new InputSystem_Actions();
        _input.Player.Pause.performed += OnPausePerformed;
        _input.Player.Pause.Enable();

        CreateUI();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvas.gameObject.SetActive(false);

        _localization.OnLanguageChanged += UpdateTexts;
        UpdateTexts();
    }

    private void OnDisable()
    {
        _input.Player.Pause.performed -= OnPausePerformed;
        _input.Player.Pause.Disable();
        if (_localization != null)
            _localization.OnLanguageChanged -= UpdateTexts;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        Toggle();
    }

    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        _canvas.gameObject.SetActive(true);
        _playerInputs.enabled = false;
        Time.timeScale = 0f;
        Fade(1f);
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        _playerInputs.enabled = true;
        Time.timeScale = 1f;
        Fade(0f, () =>
        {
            _canvas.gameObject.SetActive(false);
        });
    }

    private void Fade(float targetAlpha, Action onFinished = null)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, onFinished));
    }

    private IEnumerator FadeRoutine(float targetAlpha, Action onFinished)
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / FadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;
        _canvasGroup.interactable = targetAlpha > 0.9f;
        _canvasGroup.blocksRaycasts = targetAlpha > 0.9f;
        _fadeCoroutine = null;
        onFinished?.Invoke();
    }

    private void CreateUI()
    {
        GameObject canvasObj = new GameObject("PauseMenuCanvas");
        canvasObj.transform.SetParent(transform, false);
        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        _canvasGroup = canvasObj.AddComponent<CanvasGroup>();

        GameObject panel = CreatePanel(canvasObj.transform);
        GameObject content = CreateContent(panel.transform);

        _titleText = CreateText(content.transform, "Title", 48, TextAlignmentOptions.Center);
        CreateSpacer(content.transform, 20f);
        _resumeText = CreateButton(content.transform, "ResumeBtn", OnResumeClicked);
        CreateSpacer(content.transform, 10f);
        _settingsText = CreateButton(content.transform, "SettingsBtn", OnSettingsClicked);
        CreateSpacer(content.transform, 10f);
        _languageText = CreateButton(content.transform, "LanguageBtn", OnLanguageClicked);
        CreateSpacer(content.transform, 10f);
        _quitText = CreateButton(content.transform, "QuitBtn", OnQuitClicked);
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400f, 500f);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.85f);
        return panel;
    }

    private GameObject CreateContent(Transform parent)
    {
        GameObject content = new GameObject("Content");
        content.transform.SetParent(parent, false);
        RectTransform rt = content.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(30f, 30f);
        rt.offsetMax = new Vector2(-30f, -30f);
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 5f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return content;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 60f);
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 60f;
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        return tmp;
    }

    private TextMeshProUGUI CreateButton(Transform parent, string name, UnityEngine.Events.UnityAction onClick)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 55f);
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 55f;

        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return tmp;
    }

    private void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(parent, false);
        LayoutElement le = spacer.AddComponent<LayoutElement>();
        le.preferredHeight = height;
    }

    private void UpdateTexts()
    {
        if (_localization == null) return;
        _titleText.text = _localization.Get("pause.title");
        _resumeText.text = _localization.Get("pause.resume");
        _settingsText.text = _localization.Get("pause.settings");
        _languageText.text = $"{GetLanguageFlag()}  {_localization.Get("pause.language")}";
        _quitText.text = _localization.Get("pause.quit");
    }

    private string GetLanguageFlag()
    {
        string lang = _localization.CurrentLanguage;
        if (lang == "en") return "\U0001F1EC\U0001F1E7";
        if (lang == "es") return "\U0001F1EA\U0001F1F8";
        return "\U0001F310";
    }

    private void OnResumeClicked() => Close();

    private void OnSettingsClicked()
    {
        Debug.Log("Settings menu - proximamente");
    }

    private void OnLanguageClicked()
    {
        string next = _localization.CurrentLanguage == "en" ? "es" : "en";
        _localization.SetLanguage(next);
    }

    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
