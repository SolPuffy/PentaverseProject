using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class fakeConsole : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    private static fakeConsole instance;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        text.text = "";
        Debug.developerConsoleVisible = true;
    }
 
    public static void MoreText(string t)
    {
        instance.text.text += "\n" + t;
    }
}
