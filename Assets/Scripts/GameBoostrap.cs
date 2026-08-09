using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerResourcesUI _playerResourcesUI;
    [SerializeField] private PlayerInputs _playerInputs;
    [SerializeField] private PauseMenu _pauseMenu;
    [SerializeField] private SkillHotbarUI _skillHotbar;
    [SerializeField] private SkillUnlockNotificationUI _skillUnlockNotification;
    [SerializeField] private SpellBookUI _spellBook;
    [SerializeField] private GameObject _dropPrefab;
    [SerializeField] private GameObject _itemNameLabelPrefab;
    [SerializeField] private RectTransform _itemNameLabelContainer;
    private void Start()
    {
        DropVisualConfig.Configure(_dropPrefab, _itemNameLabelPrefab, _itemNameLabelContainer);
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

        _player.Initialize(ConfigBoostrap, saveData, character?.classId);
        _playerResourcesUI.Initialize(_player);
        _playerInputs.Initialize(_player);

        if (_skillHotbar != null)
            _skillHotbar.Initialize(_player);
        if (_skillUnlockNotification != null)
            _skillUnlockNotification.Initialize(_player);
        if (_spellBook != null)
            _spellBook.Initialize(_player);

        if (_pauseMenu != null)
            _pauseMenu.Initialize(_playerInputs);

        if (FindObjectOfType<SpawnerManager>() == null)
        {
            GameObject managerObj = new GameObject("SpawnerManager");
            managerObj.AddComponent<SpawnerManager>();
        }

        if (FindObjectOfType<StatusEffectManager>() == null)
        {
            GameObject statusObj = new GameObject("StatusEffectManager");
            statusObj.AddComponent<StatusEffectManager>();
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
