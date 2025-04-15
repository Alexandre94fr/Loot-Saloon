using System.Collections;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_UIManager : MonoBehaviour
{
    [SerializeField] private Canvas _playerCanvas;
    
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private Image _respawningCountdownImage;
    
    [Space]
    [SerializeField] private GameObject _endPanelGameObject;

    [SerializeField] private TextMeshProUGUI _myTeamResultText;
    [SerializeField] private TextMeshProUGUI _myTeamMoneyText;
    
    [SerializeField] private TextMeshProUGUI _otherTeamResultText;
    [SerializeField] private TextMeshProUGUI _otherTeamMoneyText;
    

    private void Awake()
    {
        S_LifeManager.OnDie += StartRespawnCountdown;
        S_Extract.OnExtract += EndPanel;
    }

    IEnumerator RespawnCountdown()
    {
        for (int i = 5; i > 0; i--)
        {
            _countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        _respawningCountdownImage.gameObject.SetActive(false); 

    }

    private void StartRespawnCountdown()
    {
        _respawningCountdownImage.gameObject.SetActive(true); 
        StartCoroutine(RespawnCountdown());
    }

    private void EndPanel(E_PlayerTeam team)
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

        if (team == E_PlayerTeam.NONE)
        {
            myTeamResult    = DRAW;
            otherTeamResult = DRAW;
        }
        else if (team == myTeam)
        {
            myTeamResult = VICTORY;
            otherTeamResult = DEFEAT;
        }
        else
        {
            myTeamResult = DEFEAT;
            otherTeamResult = VICTORY;
        }

        var players = S_GameLobbyManager.instance.GetPlayers();

        myTeamResult = $"{string.Join(' ', players.Where(player => player.Team == myTeam).Select(player => player.GamerTag))} - {myTeamResult}";
        otherTeamResult = $"{otherTeamResult} - {string.Join(' ', players.Where(player => player.Team != myTeam).Select(player => player.GamerTag))}";

        const string format = "{0} - {1}$";

        string myTeamMoney = string.Format(format, 0, 0);
        string otherTeamMoney = string.Format(format, 0, 0);

        S_Cart[] carts       = FindObjectsByType<S_Cart>(FindObjectsSortMode.None);
        S_Extract[] extracts = FindObjectsByType<S_Extract>(FindObjectsSortMode.None);

        _endPanelGameObject.SetActive(true);

        WriteTotalMoney(ref myTeamMoney, format, myTeam, carts, extracts);
        WriteTotalMoney(ref otherTeamMoney, format, otherTeam, carts, extracts);

        _myTeamMoneyText.text = myTeamMoney;
        _otherTeamMoneyText.text = otherTeamMoney;

        _myTeamResultText.text = myTeamResult;
        _otherTeamResultText.text = otherTeamResult;
    }

    // what the fuck is this????
    private void WriteTotalMoney(ref string moneyString, string format, E_PlayerTeam team, S_Cart[] carts, S_Extract[] extracts)
    {
        foreach (var cart in carts.Where(cart => cart.team == team))
        {
            foreach (var extract in extracts.Where(extract => extract.team == team))
            {
                moneyString = string.Format(format, cart.total, extract.GetComponent<S_Quota>().quota);
                break;
            }

            break;
        }
    }
}
