using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckReplayList : MonoBehaviour
{    
    [SerializeField] private GameObject ScrollContentPrefab;
    [SerializeField] ScrollRect scrollView;
    [SerializeField] GameObject StartButton;

 
    private void OnEnable()
    {       

        for (int i = 0; i < scrollView.content.childCount; i++)
        {
            //Debug.Log("Destroying child " + i);
            Destroy(scrollView.content.GetChild(i).gameObject);
        }
        GetAllSaveFiles();
    }

    private void GetAllSaveFiles()
    {
        string[] files = System.IO.Directory.GetFiles(getFolderDataPath());
        foreach (string file in files)
        {
            if (file.Substring(file.Length - 4) == ".txt")
            {
                GameObject g = Instantiate(ScrollContentPrefab, scrollView.content.transform);
                string textToShow = "";
                textToShow = file.Substring(getFolderDataPath().Length + 1);
                textToShow = textToShow.Substring(0, textToShow.Length - 4);
                g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = textToShow;
            }
            else
            {
                Debug.Log("Unrecognized text file, probably META");
            }
        }
    }
    
    private string getFolderDataPath()
    {
#if UNITY_EDITOR
        //Debug.Log(Application.dataPath + "/SaveFiles");
        return Application.dataPath + "/SaveFiles";
#elif UNITY_ANDROID
        //Debug.Log(Application.persistentDataPath + "/SaveFiles");
        return Application.persistentDataPath + "/SaveFiles";
#elif UNITY_IPHONE
        //Debug.Log(Application.persistentDataPath + "/SaveFiles");
        return Application.persistentDataPath + "/SaveFiles";
#else
        //Debug.Log(Application.dataPath + "/SaveFiles");
        return Application.dataPath + "/SaveFiles";
#endif
    }  
}
