using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager
{
    private string SavePath => Path.Combine(Application.persistentDataPath, "characters.json");

    private string GameplaySavePath(string characterId) =>
        Path.Combine(Application.persistentDataPath, $"save_{characterId}.json");

    public List<CharacterData> Load()
    {
        if (!File.Exists(SavePath))
            return new List<CharacterData>();

        try
        {
            string json = File.ReadAllText(SavePath);
            CharacterSaveData data = JsonUtility.FromJson<CharacterSaveData>(json);
            if (data?.characters != null)
                return new List<CharacterData>(data.characters);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load characters: {e.Message}");
        }

        return new List<CharacterData>();
    }

    public void Save(CharacterData[] characters)
    {
        try
        {
            CharacterSaveData data = new CharacterSaveData { characters = characters };
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save characters: {e.Message}");
        }
    }

    public void SaveGameplay(string characterId, PlayerSaveData saveData)
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(GameplaySavePath(characterId), json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save gameplay for {characterId}: {e.Message}");
        }
    }

    public PlayerSaveData LoadGameplay(string characterId)
    {
        string path = GameplaySavePath(characterId);
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerSaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load gameplay for {characterId}: {e.Message}");
        }

        return null;
    }

    public void DeleteGameplay(string characterId)
    {
        string path = GameplaySavePath(characterId);
        if (File.Exists(path))
        {
            try { File.Delete(path); }
            catch (Exception e) { Debug.LogError($"Failed to delete gameplay save: {e.Message}"); }
        }
    }

    public void UpdateCharacterMetadata(CharacterData character)
    {
        List<CharacterData> characters = Load();
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].id == character.id)
            {
                characters[i].level = character.level;
                characters[i].playTime = character.playTime;
                break;
            }
        }
        Save(characters.ToArray());
    }

    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }
}
