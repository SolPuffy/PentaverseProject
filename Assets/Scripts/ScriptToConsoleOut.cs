using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScriptToConsoleOut : MonoBehaviour
{
    private static ScriptToConsoleOut privateInstance;
    private void Awake()
    {
        privateInstance = this;
    }
    public TextMeshProUGUI consoleText;
    public static void UpdateConsole(string input)
    {
        privateInstance.consoleText.text += input + "\n";
    }
}
