using System;

[Serializable]
public class CharacterData
{
    public string id;
    public string name;
    public int level;
    public float playTime;
    public string classId;
    public string portraitId;
    public PlayerSaveData gameplaySave;
}

[Serializable]
public class CharacterSaveData
{
    public CharacterData[] characters;
}
