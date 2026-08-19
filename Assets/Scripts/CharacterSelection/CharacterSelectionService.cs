using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelectionService
{
    private readonly SaveManager _saveManager;
    private const int MaxSlots = 4;
    private const int MaxNameLength = 16;

    public CharacterSelectionService(SaveManager saveManager)
    {
        _saveManager = saveManager;
    }

    public List<CharacterData> GetCharacters()
    {
        return _saveManager.Load();
    }

    public CharacterData CreateCharacter(string name)
    {
        if (!CanCreateCharacter())
        {
            Debug.LogWarning("Cannot create character: all slots occupied");
            return null;
        }

        if (!IsValidName(name))
        {
            Debug.LogWarning($"Invalid character name: {name}");
            return null;
        }

        CharacterData character = new CharacterData
        {
            id = _saveManager.GenerateId(),
            name = name.Trim(),
            level = 1,
            playTime = 0f,
            classId = "",
            portraitId = ""
        };

        List<CharacterData> characters = _saveManager.Load();
        characters.Add(character);
        _saveManager.Save(characters.ToArray());

        return character;
    }

    public bool DeleteCharacter(string characterId)
    {
        List<CharacterData> characters = _saveManager.Load();
        int removed = characters.RemoveAll(c => c.id == characterId);

        if (removed > 0)
        {
            _saveManager.Save(characters.ToArray());
            return true;
        }

        return false;
    }

    public bool CanCreateCharacter()
    {
        return _saveManager.Load().Count < MaxSlots;
    }

    public bool IsValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (name.Trim().Length > MaxNameLength)
            return false;

        return true;
    }

    public int GetMaxSlots()
    {
        return MaxSlots;
    }

    public int GetMaxNameLength()
    {
        return MaxNameLength;
    }
}
