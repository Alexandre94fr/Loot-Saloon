using UnityEngine;
using UnityEngine.UI;

public class S_PlayerCrosshair : MonoBehaviour
{
    [SerializeField] private Color _onCanInteractColor  = Color.white;
    [SerializeField] private Color _onCantInteractColor = Color.black;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void ChangeColor(S_Interactable interactable)
    {
        _image.color = interactable == null ? _onCantInteractColor : _onCanInteractColor;
    }
}
