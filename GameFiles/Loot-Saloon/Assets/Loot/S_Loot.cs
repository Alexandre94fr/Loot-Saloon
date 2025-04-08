using UnityEngine;
using UnityEngine.UI;

public class S_Loot : MonoBehaviour, S_IPickable
{
    public SO_LootProperties properties;

    [SerializeField] private Image _image;

    private void Start()
    {
        _image.sprite = Instantiate(properties.sprite);
    }
}
