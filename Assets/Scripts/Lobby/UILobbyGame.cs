using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UILobbyGame : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Text;
    [SerializeField] Image BG;
    [SerializeField] Color Normal;
    [SerializeField] Color OnMouseOver;    
    [SerializeField] Color OnSelect;
    private string GameID = "";
    private bool selected = false;

    private void Awake()
    {
        BG.color = Normal;
    }
    private void Start()
    {
        UILobby.instance.AddGame(this);
    }

    public void SetName(string details)
    {
        Text.text = details;
    }
    public void Select()
    {
        UILobby.instance.SelectGame(this);
        BG.color = OnSelect;
        selected = true;        
    }

    public void Deselect()
    {
        BG.color = Normal;
        selected = false;
    }

    public void MouseOverEnter()
    {
        if (!selected)
        {
            BG.color = OnMouseOver;
        }
    }

    public void MouseOverExit()
    {
        if (!selected)
        {
            BG.color = Normal;
        }
    }

    public string GetGameID()
    {
        return GameID;
    }

    public void SetGameID(string ID)
    {
        GameID = ID;
    }
}
