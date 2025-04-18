using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_EndGameUI : MonoBehaviour
{
    [Header(" Internal references :")]
    [SerializeField] private Button _backToLobbyButton;

    [Space]
    [SerializeField] private TextMeshProUGUI _myTeamResultText;
    [SerializeField] private TextMeshProUGUI _myTeamMoneyText;

    [Space]
    [SerializeField] private TextMeshProUGUI _otherTeamResultText;
    [SerializeField] private TextMeshProUGUI _otherTeamMoneyText;

    private int _myTeamMoney;
    private int _myTeamQuota;

    private int _otherTeamMoney;
    private int _otherTeamQuota;

    private void Start()
    {
        E_PlayerTeam myTeam = S_GameLobbyManager.instance.GetPlayerTeam();

        S_Cart.GetCartValue += (p_team, p_total) => {
            if (p_team == myTeam)
                _myTeamMoney = p_total;
        };
        S_Cart.GetCartValue += (p_team, p_total) => {
            if (p_team != myTeam)
                _otherTeamMoney = p_total;
        };

        S_Extract.GetQuota += (p_team, p_quota) => {
            if (p_team == myTeam)
                _myTeamQuota = p_quota;
        };
        S_Extract.GetQuota += (p_team, p_quota) => {
            if (p_team != myTeam)
                _otherTeamQuota = p_quota;
        };

        S_Extract.OnExtract += UpdateUI;
        _backToLobbyButton.onClick.AddListener(FindAnyObjectByType<S_GameLobbyManager>().HandleHostDisconnection);

    }

    private void UpdateUI(E_PlayerTeam p_winnerTeam)
    {
        const string VICTORY = "VICTORY";
        const string DEFEAT  = "DEFEAT";
        const string DRAW    = "DRAW";

        E_PlayerTeam myTeam = S_GameLobbyManager.instance.GetPlayerTeam();

        E_PlayerTeam otherTeam = myTeam switch
        {
            E_PlayerTeam.BLUE => E_PlayerTeam.RED,
            E_PlayerTeam.RED => E_PlayerTeam.BLUE,
            _ => E_PlayerTeam.NONE,
        };

        string myTeamResult, otherTeamResult;

        if (p_winnerTeam == E_PlayerTeam.NONE)
        {
            myTeamResult = DRAW;
            otherTeamResult = DRAW;
        }
        else if (p_winnerTeam == myTeam)
        {
            myTeamResult = VICTORY;
            otherTeamResult = DEFEAT;
        }
        else
        {
            myTeamResult = DEFEAT;
            otherTeamResult = VICTORY;
        }

        List<S_LobbyPlayerData> players = S_GameLobbyManager.instance.GetPlayers();

        myTeamResult = $"{string.Join(' ', players.Where(p_player => p_player.Team == myTeam).Select(p_player => p_player.GamerTag))} - {myTeamResult}";
        otherTeamResult = $"{otherTeamResult} - {string.Join(' ', players.Where(p_player => p_player.Team != myTeam).Select(p_player => p_player.GamerTag))}";

        const string format = "{0} - {1}$";

        string myTeamMoney = string.Format(format, _myTeamMoney, _myTeamQuota);
        string otherTeamMoney = string.Format(format, _otherTeamMoney, _otherTeamQuota);

        _myTeamMoneyText.text = myTeamMoney;
        _otherTeamMoneyText.text = otherTeamMoney;

        _myTeamResultText.text = myTeamResult;
        _otherTeamResultText.text = otherTeamResult;

        GetComponentInChildren<Image>(true).gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}