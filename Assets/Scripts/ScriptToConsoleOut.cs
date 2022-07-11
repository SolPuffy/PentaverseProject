using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScriptToConsoleOut : MonoBehaviour
{
    private static ScriptToConsoleOut privateInstance;
    
    private void Awake()
    {
        privateInstance = this;
    }
    public TextMeshProUGUI consoleText;
    public ScrollRect scroller;
    public TextMeshProUGUI toggleStateGUI;
    private bool receiveUpdates = true;
    private string consoleBuffer = "";
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            UpdateConsole("Barf");
        }    
    }
    public void ChangeUpdates()
    {
        receiveUpdates = !receiveUpdates;
        if(receiveUpdates)
        {
            toggleStateGUI.color = Color.green;
            toggleStateGUI.text = "E";
        }
        else
        {
            toggleStateGUI.color = Color.red;
            toggleStateGUI.text = "D";
        }
    }    
    public static void UpdateConsole(string input)
    {
        privateInstance.consoleBuffer += input + "\n";
        if (privateInstance.receiveUpdates)
        {
            privateInstance.consoleText.text = privateInstance.consoleBuffer;
            privateInstance.scroller.verticalNormalizedPosition = 0;
        }
    }
}
