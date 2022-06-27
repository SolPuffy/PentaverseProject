using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Application.Quit();
    }
}
