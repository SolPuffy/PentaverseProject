using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InputFromPlayer : MonoBehaviour
{   
    public void HitCards()
    {
        CardPlayer.localPlayer.HitCards();
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
