using UnityEngine;
using UnityEngine.UI;

public class S_ConsoleLogger : MonoBehaviour
{
    public int maxLines = 5; // Max Line To Display
    private Text _consoleText;
    private string _consoleContent = "";

    private void Start()
    {
        _consoleText = GetComponent<Text>();
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        _consoleContent += logString + "\n";
        _consoleText.text = _consoleContent;

        if (_consoleText.text.Split('\n').Length > maxLines)
        {
            string[] lines = _consoleText.text.Split('\n');
            _consoleContent = string.Join("\n", lines, 1, lines.Length - 1);
            _consoleText.text = _consoleContent;
        }
    }
}