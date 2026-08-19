using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SaveManager
{
    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented
    };

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
            CharacterSaveData data = JsonConvert.DeserializeObject<CharacterSaveData>(json);
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
            string json = JsonConvert.SerializeObject(data, JsonSettings);
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
            string json = JsonConvert.SerializeObject(saveData, JsonSettings);
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
            JObject root = JObject.Parse(json);

            if (root["equippedItems"] is JArray oldArray)
                root["equippedItems"] = MigrateEquippedArray(oldArray);

            return root.ToObject<PlayerSaveData>();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load gameplay for {characterId}: {e.Message}");
        }

        return null;
    }

    private static JObject MigrateEquippedArray(JArray oldArray)
    {
        JObject map = new JObject();
        if (oldArray == null) return map;

        foreach (JToken token in oldArray)
        {
            JObject entry = token as JObject;
            if (entry == null) continue;

            string slotName = entry["slot"]?.ToString();
            if (string.IsNullOrEmpty(slotName)) continue;

            entry.Remove("slot");
            map[slotName] = entry;
        }

        return map;
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
