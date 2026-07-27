using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _soundPanel;

    [Header("Main Menu")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _resumeText;
    [SerializeField] private TextMeshProUGUI _soundText;
    [SerializeField] private TextMeshProUGUI _languageText;
    [SerializeField] private TextMeshProUGUI _menuText;
    [SerializeField] private TextMeshProUGUI _quitText;

    [Header("Sound Panel")]
    [SerializeField] private TextMeshProUGUI _soundTitleText;
    [SerializeField] private TextMeshProUGUI _bgmLabel;
    [SerializeField] private TextMeshProUGUI _bgmMuteLabel;
    [SerializeField] private TextMeshProUGUI _sfxLabel;
    [SerializeField] private TextMeshProUGUI _sfxMuteLabel;
    [SerializeField] private TextMeshProUGUI _muteLabel;
    [SerializeField] private TextMeshProUGUI _backText;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Toggle _bgmMuteToggle;
    [SerializeField] private Toggle _sfxMuteToggle;

    [Header("Scene")]
    [SerializeField] private string _menuSceneName = "MainMenu_OFF";

    private InputSystem_Actions _input;
    private PlayerInputs _playerInputs;
    private LocalizationConfig _localization;
    private CanvasGroup _canvasGroup;
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

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _pausePanel.SetActive(false);
        _soundPanel.SetActive(false);

        AddButton(_resumeText, OnResumeClicked);
        AddButton(_soundText, OnSoundClicked);
        AddButton(_languageText, OnLanguageClicked);
        AddButton(_menuText, OnMenuClicked);
        AddButton(_quitText, OnQuitClicked);
        AddButton(_backText, OnBackClicked);

        if (_bgmSlider != null)
            _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (_bgmMuteToggle != null)
            _bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteChanged);
        if (_sfxMuteToggle != null)
            _sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteChanged);

        LoadSoundSettings();

        _localization.OnLanguageChanged += UpdateTexts;
        UpdateTexts();
    }

    private void AddButton(TextMeshProUGUI text, UnityEngine.Events.UnityAction action)
    {
        if (text == null) return;
        Button btn = text.GetComponent<Button>();
        if (btn == null)
            btn = text.gameObject.AddComponent<Button>();
        btn.onClick.AddListener(action);
    }

    private void OnDisable()
    {
        _input.Player.Pause.performed -= OnPausePerformed;
        _input.Player.Pause.Disable();

        if (_bgmSlider != null)
            _bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        if (_bgmMuteToggle != null)
            _bgmMuteToggle.onValueChanged.RemoveListener(OnBGMMuteChanged);
        if (_sfxMuteToggle != null)
            _sfxMuteToggle.onValueChanged.RemoveListener(OnSFXMuteChanged);

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
        _pausePanel.SetActive(true);
        _soundPanel.SetActive(false);
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
            _pausePanel.SetActive(false);
            _soundPanel.SetActive(false);
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

    private void LoadSoundSettings()
    {
        float bgm = GameSettingsManager.GetBGMVolume();
        float sfx = GameSettingsManager.GetSFXVolume();
        bool bgmMute = GameSettingsManager.GetBGMMute();
        bool sfxMute = GameSettingsManager.GetSFXMute();

        if (_bgmSlider != null) _bgmSlider.SetValueWithoutNotify(bgm);
        if (_sfxSlider != null) _sfxSlider.SetValueWithoutNotify(sfx);
        if (_bgmMuteToggle != null) _bgmMuteToggle.SetIsOnWithoutNotify(bgmMute);
        if (_sfxMuteToggle != null) _sfxMuteToggle.SetIsOnWithoutNotify(sfxMute);

        AudioManager.Instance?.SetBGMVolume(bgmMute ? 0f : bgm);
        AudioManager.Instance?.SetSFXVolume(sfxMute ? 0f : sfx);
    }

    private void UpdateTexts()
    {
        if (_localization == null) return;

        if (_titleText != null) _titleText.text = _localization.Get("pause.title");
        if (_resumeText != null) _resumeText.text = _localization.Get("pause.resume");
        if (_soundText != null) _soundText.text = _localization.Get("pause.sound");
        if (_languageText != null) _languageText.text = $"{GetLanguageFlag()}  {_localization.Get("pause.language")}";
        if (_menuText != null) _menuText.text = _localization.Get("pause.menu");
        if (_quitText != null) _quitText.text = _localization.Get("pause.quit");

        if (_soundTitleText != null) _soundTitleText.text = _localization.Get("sound.title");
        if (_bgmLabel != null) _bgmLabel.text = _localization.Get("sound.bgm");
        if (_bgmMuteLabel != null) _bgmMuteLabel.text = _localization.Get("sound.muteBgm");
        if (_sfxLabel != null) _sfxLabel.text = _localization.Get("sound.sfx");
        if (_sfxMuteLabel != null) _sfxMuteLabel.text = _localization.Get("sound.muteSfx");
        if (_muteLabel != null) _muteLabel.text = _localization.Get("sound.mute");
        if (_backText != null) _backText.text = _localization.Get("sound.back");
    }

    private string GetLanguageFlag()
    {
        string lang = _localization.CurrentLanguage;
        if (lang == "en") return "\U0001F1EC\U0001F1E7";
        if (lang == "es") return "\U0001F1EA\U0001F1F8";
        return "\U0001F310";
    }

    // --- Button Handlers ---

    private void OnResumeClicked() => Close();

    private void OnSoundClicked()
    {
        _pausePanel.SetActive(false);
        _soundPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        _soundPanel.SetActive(false);
        _pausePanel.SetActive(true);
    }

    private void OnLanguageClicked()
    {
        string next = _localization.CurrentLanguage == "en" ? "es" : "en";
        _localization.SetLanguage(next);
        GameSettingsManager.SetLanguage(next);
    }

    private void OnMenuClicked()
    {
        CharacterData character = SelectedCharacterManager.Instance?.SelectedCharacter;
        if (character != null && FindObjectOfType<Player>() is Player player)
        {
            new GameSaveService(new SaveManager()).SaveGameplay(player, character);
        }

        AudioManager.Instance?.StopBGM();
        Time.timeScale = 1f;
        if (_playerInputs != null) _playerInputs.enabled = true;
        SceneManager.LoadScene(_menuSceneName);
    }

    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // --- Sound Handlers ---

    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
        if (_bgmMuteToggle != null && _bgmMuteToggle.isOn)
        {
            _bgmMuteToggle.SetIsOnWithoutNotify(false);
            GameSettingsManager.SetBGMMute(false);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        if (_sfxMuteToggle != null && _sfxMuteToggle.isOn)
        {
            _sfxMuteToggle.SetIsOnWithoutNotify(false);
            GameSettingsManager.SetSFXMute(false);
        }
    }

    private void OnBGMMuteChanged(bool isMuted)
    {
        GameSettingsManager.SetBGMMute(isMuted);
        if (isMuted)
            AudioManager.Instance?.SetBGMVolume(0f);
        else
            AudioManager.Instance?.SetBGMVolume(_bgmSlider != null ? _bgmSlider.value : 0.5f);
    }

    private void OnSFXMuteChanged(bool isMuted)
    {
        GameSettingsManager.SetSFXMute(isMuted);
        if (isMuted)
            AudioManager.Instance?.SetSFXVolume(0f);
        else
            AudioManager.Instance?.SetSFXVolume(_sfxSlider != null ? _sfxSlider.value : 0.7f);
    }
}
