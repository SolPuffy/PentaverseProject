using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
   
    void Awake()
    {
        if (GameObject.Find("NetworkManager") == null)
            SceneManager.LoadScene(0);
    }   
}
