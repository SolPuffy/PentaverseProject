using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CounterToImageChange : MonoBehaviour
{
    public TextMeshProUGUI counter;
    public Button imageToChange;

    public Sprite imageOnHigherThanZero;
    public Sprite imageOnZero;

    public bool slap = false;

    private void Update()
    {
        if (Application.isBatchMode) return;
        if (CardPlayer.localPlayer == null) return;
        if (slap)
        {
            if (int.Parse(counter.text) > 0 && HitSlapRazboi.instance.InititalSetupDone && HitSlapRazboi.instance.CardCount[CardPlayer.localPlayer.playerIndex] > 0)
            {
                counter.enabled = true;
                imageToChange.image.sprite = imageOnHigherThanZero;
            }
            else
            {
                counter.enabled = false;
                imageToChange.image.sprite = imageOnZero;
            }
        }
        else
        {
            if (HitSlapRazboi.instance.IndexOfActivePlayer == CardPlayer.localPlayer.playerIndex && HitSlapRazboi.instance.InititalSetupDone)
            {
                counter.enabled = true;
                imageToChange.image.sprite = imageOnHigherThanZero;
            }
            else
            {
                counter.enabled = false;
                imageToChange.image.sprite = imageOnZero;
            }
        }
    }
}
