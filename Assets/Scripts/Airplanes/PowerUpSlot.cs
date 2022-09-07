using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpSlot : MonoBehaviour
{
    public Button SlotButtonInstance;
    public PowerUp CurrentlyHeldPowerup;
    public int IndexOfPlayerHoldingPowerup;
    public int powerupSlot;
    public bool isOccupied = false;

    public void OnButtonPress()
    {
        if(isOccupied)
        {
            PlanesPlayer.localPlayer.LoadPowerup(CurrentlyHeldPowerup,IndexOfPlayerHoldingPowerup,powerupSlot);
        }        
    }    
}
