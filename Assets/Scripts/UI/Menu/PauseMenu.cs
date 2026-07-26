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
    private TextMeshProUGUI _soundText;
    private TextMeshProUGUI _languageText;
    private TextMeshProUGUI _quitText;

    private GameObject _mainContent;
    private GameObject _soundPanel;

    private TextMeshProUGUI _soundTitleText;
    private TextMeshProUGUI _bgmLabel;
    private TextMeshProUGUI _sfxLabel;
    private TextMeshProUGUI _muteLabel;
    private TextMeshProUGUI _backText;

    private Slider _bgmSlider;
    private Slider _sfxSlider;
    private Toggle _bgmMuteToggle;
    private Toggle _sfxMuteToggle;

    private bool _isOpen;
    private bool _isSoundOpen;
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
        _isSoundOpen = false;
        _playerInputs.enabled = true;
        Time.timeScale = 1f;
        Fade(0f, () =>
        {
            _canvas.gameObject.SetActive(false);
            _soundPanel.SetActive(false);
            _mainContent.SetActive(true);
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
        _soundText = CreateButton(content.transform, "SoundBtn", OnSoundClicked);
        CreateSpacer(content.transform, 10f);
        _languageText = CreateButton(content.transform, "LanguageBtn", OnLanguageClicked);
        CreateSpacer(content.transform, 10f);
        _quitText = CreateButton(content.transform, "QuitBtn", OnQuitClicked);

        _mainContent = content;

        _soundPanel = CreateSoundPanel(panel.transform);
        _soundPanel.SetActive(false);

        LoadSoundSettings();
    }

    private GameObject CreateSoundPanel(Transform parent)
    {
        GameObject panel = new GameObject("SoundPanel");
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 10f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(30, 30, 30, 30);

        _soundTitleText = CreateText(panel.transform, "SoundTitle", 48, TextAlignmentOptions.Center);
        CreateSpacer(panel.transform, 20f);

        _bgmLabel = CreateText(panel.transform, "BGMLabel", 24, TextAlignmentOptions.Left);
        _bgmSlider = CreateSlider(panel.transform, "BGMSlider", 0.5f, OnBGMVolumeChanged);
        CreateSpacer(panel.transform, 5f);

        _sfxLabel = CreateText(panel.transform, "SFXLabel", 24, TextAlignmentOptions.Left);
        _sfxSlider = CreateSlider(panel.transform, "SFXSlider", 0.7f, OnSFXVolumeChanged);
        CreateSpacer(panel.transform, 5f);

        _muteLabel = CreateText(panel.transform, "MuteLabel", 24, TextAlignmentOptions.Left);
        _bgmMuteToggle = CreateToggle(panel.transform, "BGMMute", OnBGMMuteChanged);
        _sfxMuteToggle = CreateToggle(panel.transform, "SFXMute", OnSFXMuteChanged);
        CreateSpacer(panel.transform, 10f);

        _backText = CreateButton(panel.transform, "BackBtn", OnBackClicked);

        return panel;
    }

    private void LoadSoundSettings()
    {
        float bgm = GameSettingsManager.GetBGMVolume();
        float sfx = GameSettingsManager.GetSFXVolume();
        bool bgmMute = GameSettingsManager.GetBGMMute();
        bool sfxMute = GameSettingsManager.GetSFXMute();

        _bgmSlider.SetValueWithoutNotify(bgm);
        _sfxSlider.SetValueWithoutNotify(sfx);
        _bgmMuteToggle.SetIsOnWithoutNotify(bgmMute);
        _sfxMuteToggle.SetIsOnWithoutNotify(sfxMute);

        AudioManager.Instance?.SetBGMVolume(bgmMute ? 0f : bgm);
        AudioManager.Instance?.SetSFXVolume(sfxMute ? 0f : sfx);
    }

    private Slider CreateSlider(Transform parent, string name, float defaultValue, UnityEngine.Events.UnityAction<float> onValueChanged)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 30f);
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 30f;

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        Slider slider = obj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = defaultValue;
        slider.onValueChanged.AddListener(onValueChanged);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(0.5f, 1f);
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.7f, 1f, 1f);

        slider.fillRect = fillRt;

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(obj.transform, false);
        RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.sizeDelta = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handle.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20f, 20f);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = handleRt;

        return slider;
    }

    private Toggle CreateToggle(Transform parent, string name, UnityEngine.Events.UnityAction<bool> onValueChanged)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300f, 30f);
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.preferredHeight = 30f;

        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        Toggle toggle = obj.AddComponent<Toggle>();
        toggle.isOn = false;
        toggle.onValueChanged.AddListener(onValueChanged);

        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(obj.transform, false);
        RectTransform checkRt = checkmark.AddComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0.1f, 0.1f);
        checkRt.anchorMax = new Vector2(0.9f, 0.9f);
        checkRt.sizeDelta = Vector2.zero;
        Image checkImg = checkmark.AddComponent<Image>();
        checkImg.color = new Color(0.4f, 0.7f, 1f, 1f);

        toggle.graphic = checkImg;

        return toggle;
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
        _soundText.text = _localization.Get("pause.sound");
        _languageText.text = $"{GetLanguageFlag()}  {_localization.Get("pause.language")}";
        _quitText.text = _localization.Get("pause.quit");

        _soundTitleText.text = _localization.Get("sound.title");
        _bgmLabel.text = _localization.Get("sound.bgm");
        _sfxLabel.text = _localization.Get("sound.sfx");
        _muteLabel.text = _localization.Get("sound.mute");
        _backText.text = _localization.Get("sound.back");
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

    private void OnSoundClicked()
    {
        _mainContent.SetActive(false);
        _soundPanel.SetActive(true);
        _isSoundOpen = true;
    }

    private void OnBackClicked()
    {
        _soundPanel.SetActive(false);
        _mainContent.SetActive(true);
        _isSoundOpen = false;
    }

    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
    }

    private void OnBGMMuteChanged(bool isMuted)
    {
        GameSettingsManager.SetBGMMute(isMuted);
        if (isMuted)
            AudioManager.Instance?.SetBGMVolume(0f);
        else
            AudioManager.Instance?.SetBGMVolume(_bgmSlider.value);
    }

    private void OnSFXMuteChanged(bool isMuted)
    {
        GameSettingsManager.SetSFXMute(isMuted);
        if (isMuted)
            AudioManager.Instance?.SetSFXVolume(0f);
        else
            AudioManager.Instance?.SetSFXVolume(_sfxSlider.value);
    }

    private void OnLanguageClicked()
    {
        string next = _localization.CurrentLanguage == "en" ? "es" : "en";
        _localization.SetLanguage(next);
        GameSettingsManager.SetLanguage(next);
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
