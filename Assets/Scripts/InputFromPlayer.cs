using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class InputFromPlayer : MonoBehaviour
{
    public TMP_Dropdown leDrop;
    public void HitCards()
    {
        CardPlayer.localPlayer.HitCards();
    }
    public void UpdateCardRulesAtRuntime()
    {
        CardPlayer.localPlayer.SendRulesUpdateToServer(leDrop.value);
    }    
    public void SlapCards()
    {
        CardPlayer.localPlayer.SlapCards();
    }

    public void StartGame()
    {
        CardPlayer.localPlayer.BuildDeck();
    }

    public void ExitGame()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManager>().StopClient();
    }
}
