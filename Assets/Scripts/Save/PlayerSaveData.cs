using System;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public string characterId;
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;
    public int level;
    public double currentExperience;
    public int strength;
    public int vitality;
    public int intelligence;
    public int dexterity;
    public int currentHp;
    public int currentMana;
    public string[] unlockedAttackIds;
    public float playTime;

    public Vector3 Position
    {
        get => new Vector3(posX, posY, posZ);
        set { posX = value.x; posY = value.y; posZ = value.z; }
    }

    public Quaternion Rotation
    {
        get => new Quaternion(rotX, rotY, rotZ, rotW);
        set { rotX = value.x; rotY = value.y; rotZ = value.z; rotW = value.w; }
    }
}
