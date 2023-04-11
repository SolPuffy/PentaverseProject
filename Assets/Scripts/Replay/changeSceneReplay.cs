using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class changeSceneReplay : MonoBehaviour
{
    public void ButtonInteractionOpenReplayMode()
    {
        SceneManager.LoadSceneAsync("ReplayScene", LoadSceneMode.Single);
    }
    public void ButtonInteractionReturnToMainScreen()
    {
        SceneManager.LoadSceneAsync("ConnectScene");
    }
}
