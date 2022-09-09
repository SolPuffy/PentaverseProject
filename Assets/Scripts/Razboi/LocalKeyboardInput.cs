using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalKeyboardInput : MonoBehaviour
{
    public string KeyCodeForHit;
    public string KeyCodeForSlap;

    public Button linkToHitButton;
    public Button linkToSlapButton;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCodeForSlap))
        {
            linkToSlapButton.onClick.Invoke();
        }
        if(Input.GetKeyDown(KeyCodeForHit))
        {
            linkToHitButton.onClick.Invoke();
        }
    }
}
