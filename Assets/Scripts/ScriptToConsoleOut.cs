using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

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
    public async static void UpdateConsole(string input)
    {
        privateInstance.consoleBuffer += input + "\n";
        if (privateInstance.receiveUpdates)
        {
            privateInstance.consoleText.text = privateInstance.consoleBuffer;
            await Task.Delay(50);
            privateInstance.scroller.verticalNormalizedPosition = 0;
        }
    }
}
