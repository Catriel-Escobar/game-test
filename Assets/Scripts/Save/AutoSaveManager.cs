using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveManager : MonoBehaviour
{
    private Player _player;
    private CharacterData _character;
    private GameSaveService _saveService;
    private float _sessionStartTime;
    private float _autoSaveInterval = 60f;
    private float _lastAutoSaveTime;

    public void Initialize(Player player)
    {
        _player = player;
        _character = SelectedCharacterManager.Instance?.SelectedCharacter;

        if (_character == null)
        {
            Destroy(gameObject);
            return;
        }

        _saveService = new GameSaveService(new SaveManager());
        _sessionStartTime = Time.time;
        _lastAutoSaveTime = Time.time;

        if (_player.Progression != null)
            _player.Progression.OnLevelChanged += OnLevelChanged;

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Update()
    {
        if (_player == null || _character == null) return;

        if (Time.time - _lastAutoSaveTime >= _autoSaveInterval)
        {
            _lastAutoSaveTime = Time.time;
            Save();
        }
    }

    private void OnLevelChanged(int level, double currentXp, long xpToNext)
    {
        Save();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            Save();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            Save();
    }

    private void Save()
    {
        if (_player == null || _character == null) return;

        _character.playTime += Time.time - _sessionStartTime;
        _sessionStartTime = Time.time;
        _saveService.SaveGameplay(_player, _character);
    }

    private void OnDestroy()
    {
        if (_player?.Progression != null)
            _player.Progression.OnLevelChanged -= OnLevelChanged;

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
