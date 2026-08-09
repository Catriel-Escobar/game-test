using UnityEngine;

public class ConfigBoostrap
{
    public static ConfigBoostrap Current { get; private set; }
   public string GameJson = "GameData/Config/game";
   public AttackConfig AttackConfig;
   public StatsConfig StatsConfig;
   public ItemsConfig ItemsConfig;
   public EnemiesConfig EnemiesConfig;
   public ProgressionConfig ProgressionConfig;
    public PlayerConfig PlayerConfig;
    public SpawnersConfig SpawnersConfig;
    public SkillsConfig SkillsConfig;
   public LocalizationConfig LocalizationConfig;
    public CharacterClassesConfig CharacterClassesConfig;
    public AffixesConfig AffixesConfig;
    public DropConfig DropConfig;
  public void Initialize()
{
        Current = this;

    TextAsset file = Resources.Load<TextAsset>(GameJson);

    if (file == null)
    {
        Debug.LogError($"No se encontró el archivo: {GameJson}");
        return;
    }
    GameConfig gameConfig = JsonUtility.FromJson<GameConfig>(file.text);
    PlayerConfig = LoadConfig<PlayerConfig>(gameConfig.player);
    AttackConfig = LoadConfig<AttackConfig>(gameConfig.attacks);
    ProgressionConfig = LoadConfig<ProgressionConfig>(gameConfig.progression);
    StatsConfig = LoadConfig<StatsConfig>(gameConfig.stats);
    ItemsConfig = LoadConfig<ItemsConfig>(gameConfig.items);
    EnemiesConfig  = LoadConfig<EnemiesConfig>(gameConfig.enemies);
    SpawnersConfig = LoadConfig<SpawnersConfig>(gameConfig.spawners);
    SkillsConfig = LoadConfig<SkillsConfig>(gameConfig.skills);
    LocalizationConfig = LoadConfig<LocalizationConfig>(gameConfig.localization);
    LocalizationConfig.BuildLookup();
    CharacterClassesConfig = LoadConfig<CharacterClassesConfig>(gameConfig.characterClasses);
    AffixesConfig = LoadConfig<AffixesConfig>(gameConfig.affixes);
    DropConfig = LoadConfig<DropConfig>(gameConfig.drops);
    Debug.Log($"GameConfig:\n{JsonUtility.ToJson(gameConfig, true)}");
    Debug.Log($"PlayerConfig:\n{JsonUtility.ToJson(PlayerConfig, true)}");
    Debug.Log($"AttackConfig:\n{JsonUtility.ToJson(AttackConfig, true)}");
    Debug.Log($"ProgressionConfig:\n{JsonUtility.ToJson(ProgressionConfig, true)}");
    Debug.Log($"StatsConfig:\n{JsonUtility.ToJson(StatsConfig, true)}");
    Debug.Log($"ItemsConfig:\n{JsonUtility.ToJson(ItemsConfig, true)}");
    Debug.Log($"EnemiesConfig:\n{JsonUtility.ToJson(EnemiesConfig, true)}");
    Debug.Log($"SpawnersConfig:\n{JsonUtility.ToJson(SpawnersConfig, true)}");
}

private T LoadConfig<T>(string path)
{
    TextAsset file = Resources.Load<TextAsset>(path);
    if (file == null)
    {
        Debug.LogError($"No se encontró el archivo: {path}");
        return default;
    }

    return JsonUtility.FromJson<T>(file.text);
}
}
