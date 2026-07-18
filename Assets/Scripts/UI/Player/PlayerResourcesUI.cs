
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResourcesUI : MonoBehaviour
{
    private Player _player;
    [SerializeField] private Image hpBar;
    [SerializeField] private Image mpBar;
    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI HPText;
    [SerializeField] private TextMeshProUGUI MPText;
    [SerializeField] private TextMeshProUGUI currentExp;
    [SerializeField] private TextMeshProUGUI expToNextLevel;
    [SerializeField] private TextMeshProUGUI level;

    private double _currentExp;
    private int _level;
    private long _expToNextLevel;


     private void Start()
    {

    }
    public void Initialize(Player player)
    {

        _player = player;

        _player.Resources.OnHealthChanged += UpdateHealth;
        _player.Resources.OnManaChanged += UpdateMana;
        _player.Progression.OnCurrentExperienceChanged += UpdateCurrentExperience;
        _player.Progression.OnLevelChanged += UpdateLevelAndExp;
        UpdateHealth(_player.Resources.CurrentHp,_player.Resources.MaxHp);
        UpdateMana(_player.Resources.CurrentMana,_player.Resources.MaxMana);
        UpdateLevelAndExp(_player.Progression.Level,_player.Progression.CurrentExperience,_player.Progression.ExperiencePerLevel[_player.Progression.Level+1]);
        UpdateCurrentExperience(_player.Progression.CurrentExperience);
    }


    private void UpdateCurrentExperience(double exp)
    {  
        _currentExp = exp;
        currentExp.text = $"XP {_currentExp}/{_expToNextLevel}";
        float percentage = (float)exp/ _expToNextLevel;
        expBar.fillAmount = percentage;
    }
    private void UpdateLevelAndExp(int level,double currentExp, long expToNextLevel)
    {
        _level = level;
        _expToNextLevel = expToNextLevel;
        _currentExp = currentExp;
        this.level.text = $"Lv. {level}";
        this.currentExp.text = $"XP {_currentExp}/{_expToNextLevel}";
        float percentage = (float)currentExp/ _expToNextLevel;
        expBar.fillAmount = percentage;

    }
    private void UpdateHealth(int currentHp,int maxHp)
    {
        float percentage = (float)currentHp / maxHp;
        HPText.text = $"HP {currentHp}/{maxHp}";
        hpBar.fillAmount = percentage;
    }

    private void UpdateMana(int currentMana,int maxMana)
    {
        // actualizar barra Mana
        MPText.text = $"MP {currentMana}/{maxMana}";
        float percentage = (float)currentMana/maxMana;
        mpBar.fillAmount = percentage;
    }
}