using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerResourcesUI _playerResourcesUI;
    [SerializeField] private PlayerInputs _playerInputs;
    [SerializeField] private PauseMenu _pauseMenu;
    private void Start()
    {
        var ConfigBoostrap = new ConfigBoostrap();
        ConfigBoostrap.Initialize();

        PlayerSaveData saveData = null;
        CharacterData character = SelectedCharacterManager.Instance?.SelectedCharacter;
        if (character != null)
        {
            SaveManager saveManager = new SaveManager();
            GameSaveService saveService = new GameSaveService(saveManager);
            saveData = saveService.LoadGameplay(character.id);
        }

        _player.Initialize(ConfigBoostrap, saveData);
        _playerResourcesUI.Initialize(_player);
        _playerInputs.Initialize(_player);

        if (_pauseMenu != null)
            _pauseMenu.Initialize(_playerInputs);

        if (FindObjectOfType<SpawnerManager>() == null)
        {
            GameObject managerObj = new GameObject("SpawnerManager");
            managerObj.AddComponent<SpawnerManager>();
        }

        if (FindObjectOfType<AudioManager>() == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            AudioManager audioManager = audioObj.AddComponent<AudioManager>();
        }

        AudioClip bgmClip = Resources.Load<AudioClip>("Sounds/game-sound-01");
        AudioManager.Instance?.PlayBGM(bgmClip);

        GameObject autoSaveObj = new GameObject("AutoSaveManager");
        AutoSaveManager autoSave = autoSaveObj.AddComponent<AutoSaveManager>();
        autoSave.Initialize(_player);
    }
}
