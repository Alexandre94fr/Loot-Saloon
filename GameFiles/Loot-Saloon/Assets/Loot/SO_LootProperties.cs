using UnityEngine;

[CreateAssetMenu(fileName = "SO_LootProperties", menuName = "Scriptable Objects/SO_LootProperties")]
public class SO_LootProperties : ScriptableObject
{
    public enum Size
    {
        Small,
        Medium,
        Large
    }

    public string lootName = "";
    public Sprite sprite;

    [Range(1, 5000)] public int moneyValue = 1;
    public Size size = Size.Medium;

    public GameObject PB_prefab;
}
