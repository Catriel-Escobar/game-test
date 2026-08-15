using UnityEngine;
using UnityEngine.InputSystem;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerResourcesUI _playerResourcesUI;
    [SerializeField] private PlayerInputs _playerInputs;
    [SerializeField] private PauseMenu _pauseMenu;
    [SerializeField] private SkillHotbarUI _skillHotbar;
    [SerializeField] private SkillUnlockNotificationUI _skillUnlockNotification;
    [SerializeField] private SpellBookUI _spellBook;
    [SerializeField] private InventoryUI _inventoryUI;
    [SerializeField] private EquipmentUI _equipmentUI;
    [SerializeField] private SnackbarUI _snackbar;
    [SerializeField] private GameObject _dropPrefab;
    [SerializeField] private GameObject _itemNameLabelPrefab;
    [SerializeField] private RectTransform _itemNameLabelContainer;

    private InputSystem_Actions _input;

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

        _player.Initialize(ConfigBoostrap, saveData);
        _playerResourcesUI.Initialize(_player);
        _playerInputs.Initialize(_player);

        if (_skillHotbar != null)
            _skillHotbar.Initialize(_player);
        if (_skillUnlockNotification != null)
            _skillUnlockNotification.Initialize(_player);
        if (_spellBook != null)
            _spellBook.Initialize(_player);

        EnsureInventoryAndEquipmentUI();

        if (_inventoryUI != null)
            _inventoryUI.Initialize(_player, _playerInputs);
        if (_equipmentUI != null)
            _equipmentUI.Initialize(_player);
        if (_snackbar != null)
            _snackbar.Initialize(_player);

        _input = new InputSystem_Actions();
        _input.Player.Inventory.performed += OnInventoryTogglePerformed;
        _input.Player.Inventory.Enable();

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

    private void EnsureInventoryAndEquipmentUI()
    {
        ValidateUIReference(_inventoryUI, "InventoryUI");
        ValidateUIReference(_equipmentUI, "EquipmentUI");
    }

    private void OnInventoryTogglePerformed(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        if (_inventoryUI != null) _inventoryUI.Toggle();
        if (_equipmentUI != null) _equipmentUI.Toggle();
    }

    private void OnDisable()
    {
        if (_input != null)
        {
            _input.Player.Inventory.performed -= OnInventoryTogglePerformed;
            _input.Player.Inventory.Disable();
        }
    }

    private void ValidateUIReference(Component ui, string label)
    {
        if (ui == null)
        {
            Debug.LogError($"[GameBootstrap] '{label}' no esta asignado. Arrastra la instancia en escena del panel al campo correspondiente.");
            return;
        }

        if (ui.gameObject.scene.name == null)
            Debug.LogError($"[GameBootstrap] '{label}' apunta a un prefab asset. Arrastra la INSTANCIA en escena (bajo el canvas UI Resources), no el prefab del Project.", ui);
        else if (ui.GetComponentInParent<Canvas>() == null)
            Debug.LogError($"[GameBootstrap] '{label}' esta fuera del canvas UI Resources. Movelo como hijo del canvas en la escena.", ui);
    }
}
