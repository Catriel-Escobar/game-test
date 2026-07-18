


using System;
[Serializable]
public class ItemsConfig
{
    public Item[] items;
}
[Serializable]
public class Item
{
    public string id;
    public TypeItem type;
    public float damage;
    public float heal;
}



public enum TypeItem {
    weapon,
    consumable
}