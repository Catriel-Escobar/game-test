using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class GameSaveService
{
    private readonly SaveManager _saveManager;

    public GameSaveService(SaveManager saveManager)
    {
        _saveManager = saveManager;
    }

    public PlayerSaveData CreateNewSave(string characterId)
    {
        return new PlayerSaveData
        {
            characterId = characterId,
            level = 1,
            currentExperience = 0,
            unlockedAttackIds = new string[0],
            unlockedSkillIds = new string[0],
            equippedItems = new Dictionary<EquipmentSlot, EquippedItemData>(),
            inventoryItems = new ItemStack[0],
            playTime = 0f
        };
    }

    public void SaveGameplay(Player player, CharacterData character)
    {
        if (player == null || character == null) return;

        PlayerSaveData save = new PlayerSaveData
        {
            characterId = character.id,
            Position = player.transform.position,
            Rotation = player.transform.rotation,
            level = player.Progression.Level,
            currentExperience = player.Progression.CurrentExperience,
            strength = player.Stats.Strength,
            vitality = player.Stats.Vitality,
            intelligence = player.Stats.Intelligence,
            dexterity = player.Stats.Dexterity,
            currentHp = player.Resources.CurrentHp,
            currentMana = player.Resources.CurrentMana,
            unlockedAttackIds = player.UnlockedAttackIds.Keys.ToArray(),
            unlockedSkillIds = player.Skills?.UnlockedSkillIds.ToArray(),
            equippedItems = player.Equipment?.GetEquippedData(),
            inventoryItems = player.Inventory?.Stacks == null ? null : player.Inventory.Stacks.ToArray(),
            playTime = character.playTime
        };

        _saveManager.SaveGameplay(character.id, save);

        character.level = save.level;
        character.playTime = save.playTime;
        _saveManager.UpdateCharacterMetadata(character);
    }

    public PlayerSaveData LoadGameplay(string characterId)
    {
        return _saveManager.LoadGameplay(characterId);
    }

    public void DeleteGameplay(string characterId)
    {
        _saveManager.DeleteGameplay(characterId);
    }
}
