using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CounterToImageChange : MonoBehaviour
{
    public TextMeshProUGUI counter;
    public Button imageToChange;

    public Sprite imageOnHigherThanZero;
    public Sprite imageOnZero;

    private void Update()
    {
        if(counter.text.ConvertTo<int>() > 0)
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
