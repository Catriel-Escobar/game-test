using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerResourcesUI _playerResourcesUI;
    [SerializeField] private PlayerInputs _playerInputs;
    private void Start()
    {
        var ConfigBoostrap = new ConfigBoostrap();
        ConfigBoostrap.Initialize();
        _player.Initialize(ConfigBoostrap);
        _playerResourcesUI.Initialize(_player);
        _playerInputs.Initialize(_player);

        if (FindObjectOfType<SpawnerManager>() == null)
        {
            GameObject managerObj = new GameObject("SpawnerManager");
            managerObj.AddComponent<SpawnerManager>();
        }

        GameObject pauseMenuObj = new GameObject("PauseMenu");
        PauseMenu pauseMenu = pauseMenuObj.AddComponent<PauseMenu>();
        pauseMenu.Initialize(_playerInputs);

        if (FindObjectOfType<AudioManager>() == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            AudioManager audioManager = audioObj.AddComponent<AudioManager>();
            AudioClip bgmClip = Resources.Load<AudioClip>("Sounds/game-sound-01");
            audioManager.PlayBGM(bgmClip);
        }
    }
}
