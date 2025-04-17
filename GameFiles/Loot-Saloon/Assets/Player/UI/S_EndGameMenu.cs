using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_EndGamemenu : MonoBehaviour
{
    [SerializeField] private GameObject _endPanelGameObject;

    [SerializeField] private TextMeshProUGUI _myTeamResultText;
    [SerializeField] private TextMeshProUGUI _myTeamMoneyText;
    
    [SerializeField] private TextMeshProUGUI _otherTeamResultText;
    [SerializeField] private TextMeshProUGUI _otherTeamMoneyText;

    private int _myTeamMoney;
    private int _myTeamQuota;

    private int _otherTeamMoney;
    private int _otherTeamQuota;

    private void Start()
    {
        E_PlayerTeam myTeam = S_GameLobbyManager.instance.GetPlayerTeam();

        S_Cart.GetCartValue += (team, total) => {
            if (team == myTeam)
                _myTeamMoney = total;
        };
        S_Cart.GetCartValue += (team, total) => {
            if (team != myTeam)
                _otherTeamMoney = total;
        };

        S_Extract.GetQuota += (team, quota) => {
            if (team == myTeam)
                _myTeamQuota = quota;
        };
        S_Extract.GetQuota += (team, quota) => {
            if (team != myTeam)
                _otherTeamQuota = quota;
        };

        S_Extract.OnExtract += UpdateUI;
    }

    private void UpdateUI(E_PlayerTeam winner)
    {
        Cursor.lockState = CursorLockMode.None;

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

        if (winner == E_PlayerTeam.NONE)
        {
            myTeamResult    = DRAW;
            otherTeamResult = DRAW;
        }
        else if (winner == myTeam)
        {
            myTeamResult    = VICTORY;
            otherTeamResult = DEFEAT;
        }
        else
        {
            myTeamResult    = DEFEAT;
            otherTeamResult = VICTORY;
        }

        var players = S_GameLobbyManager.instance.GetPlayers();

        myTeamResult    = $"{string.Join(' ', players.Where(player => player.Team == myTeam).Select(player => player.GamerTag))} - {myTeamResult}";
        otherTeamResult = $"{otherTeamResult} - {string.Join(' ', players.Where(player => player.Team != myTeam).Select(player => player.GamerTag))}";

        const string format = "{0} - {1}$";

        string myTeamMoney    = string.Format(format, _myTeamMoney, _myTeamQuota);
        string otherTeamMoney = string.Format(format, _otherTeamMoney, _otherTeamQuota);

        _myTeamMoneyText.text    = myTeamMoney;
        _otherTeamMoneyText.text = otherTeamMoney;

        _myTeamResultText.text    = myTeamResult;
        _otherTeamResultText.text = otherTeamResult;

        _endPanelGameObject.SetActive(true);
    }

    public void GoBackToMainMenu()
    {
        S_Extract.ClearEvents();
        S_Cart.ClearEvents();
        S_LobbyEvents.ClearEvents();
        S_LifeManager.ClearEvents();
        S_PlayerInputsReciever.ClearEvents();
        S_GameTimer.ClearEvents();
        S_CircleLoad.ClearEvents();

        SceneManager.LoadScene("MainMenu");
    }
}