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

    public async void OnButtonPress()
    {
        if(CurrentlyHeldPowerup != null)
        {
            await CurrentlyHeldPowerup.LoadPowerup((int)CurrentlyHeldPowerup.PowerUpType,CurrentlyHeldPowerup,IndexOfPlayerHoldingPowerup,powerupSlot);
        }
        CurrentlyHeldPowerup = null;
    }    
}
