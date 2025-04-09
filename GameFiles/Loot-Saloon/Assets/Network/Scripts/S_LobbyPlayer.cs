using TMPro;
using UnityEngine;

public class S_LobbyPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerName;
    private S_LobbyPlayerData _data;

    public void SetData(S_LobbyPlayerData p_data)
    {
        _data = p_data;
        _playerName.text = _data.GamerTag;
        gameObject.SetActive(true);
    }
}