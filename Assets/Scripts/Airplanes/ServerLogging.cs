using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading.Tasks;

[System.Serializable]
public class ServerData
{
    public string TimeOfGameStart;
    public string TimeOfLoggingBackup;
    public int PlayerCountDuringLogging;
    public int PlayerCountOnStart;
    public int AverageActionsPerformed;
    public List<playerActionsAirplanes> ActionsPerformed = new List<playerActionsAirplanes>();
    public serverAssets WordLists = new serverAssets();
}
[System.Serializable]
public class playerActionsAirplanes
{
    public int playerInGameIndex;
    public string playerUniqueID;
    public string actionType;
    public List<Vector3Int> coordsOfAction = new List<Vector3Int>();
    public List<string> actionContent = new List<string>();
    public string timeOfAction;
}
[System.Serializable]
public class serverAssets
{
    public List<string> UsedWords = new List<string>();
    public string[] AvailableWordsThisGame;
}
public class ServerLogging : MonoBehaviour
{
    private static ServerLogging InstanceLogging;
    private string PathToFile;
    [SerializeField] private ServerData InstanceData = new ServerData();

    private void Awake()
    {
        InstanceLogging = this;
        CheckFolderDataPath();
    }
    #region BackupFunctions
    private async Task PerformBackup()
    {
        GetFileDataPath();

        if(InstanceData.TimeOfGameStart == "")
        {
            InstanceData.TimeOfGameStart = "Game was never started";
        }
        InstanceData.TimeOfLoggingBackup = DateTime.Now.ToString("G");
        InstanceData.PlayerCountDuringLogging = ServerActions.Instance.PlayersList.Count;
        InstanceData.AverageActionsPerformed = Mathf.FloorToInt(InstanceData.ActionsPerformed.Count / InstanceData.PlayerCountOnStart);

        string JsonOutput = JsonUtility.ToJson(InstanceData, true);
        await System.IO.File.WriteAllTextAsync(PathToFile, JsonOutput);

        Debug.Log($"Backup location: {PathToFile}, Backup date: {InstanceData.TimeOfLoggingBackup}");
    }
    public async Task ReadBackup(string inputToFile)
    {
        if (inputToFile.Length == 9)
        {
            inputToFile = ReadFileDataPath(inputToFile);
            if (File.Exists(inputToFile))
            {
                string JsonInput = await System.IO.File.ReadAllTextAsync(inputToFile);
                JsonUtility.FromJsonOverwrite(JsonInput, InstanceData);
            }
            else
            {
                Debug.LogError("ReadBackup >> File does not exist");
            }
        }
        else
        {
            Debug.LogError("ReadBackup >> Invalid Input");
        }
    }
    #endregion
    #region StaticFunctions
    public static void AddActionFromPlayerToList(playerActionsAirplanes action)
    {
        ServerLogging.InstanceLogging.InstanceData.ActionsPerformed.Add(action);
    }
    public static void RegisterPlayerCountAtStart(int count)
    {
        ServerLogging.InstanceLogging.InstanceData.PlayerCountOnStart = count;
    }    
    public static void ResetCurrentLogData()
    {
        ServerLogging.InstanceLogging.InstanceData = new ServerData();
    }
    public static void RegisterStartTime()
    {
        ServerLogging.InstanceLogging.InstanceData.TimeOfGameStart = DateTime.Now.ToString("G");
    }    
    public async static void RequestLogBackup()
    {
        await ServerLogging.InstanceLogging.PerformBackup();
    }    
    public async static Task<ServerData> RequestDataFromServer(string fileIndex)
    {
        await ServerLogging.InstanceLogging.ReadBackup(fileIndex);
        return ServerLogging.InstanceLogging.InstanceData;
    }    
    #endregion
    #region FilePathingAndGeneration
    private void CheckFolderDataPath()
    {
        string path = getFolderDataPath();
        if (Directory.Exists(path))
        {
            Debug.Log("FolderFound");
            //doNothing
        }
        else
        {
            Directory.CreateDirectory(path);
        }
    }
    private string generateRandomSaveId()
    {
        string RandomToReturn = "";
        for (int i = 0; i < 9; i++)
        {
            RandomToReturn += UnityEngine.Random.Range(0, 9).ToString();
        }
        return RandomToReturn;
    }
    private string ReadFileDataPath(string textInput)
    {
#if UNITY_EDITOR
        return Application.dataPath + "/SaveFiles/" + textInput + ".txt";
#elif UNITY_ANDROID
        return Application.persistentDataPath + "/SaveFiles/" + textInput + ".txt";
#elif UNITY_IPHONE
        return Application.persistentDataPath + "/SaveFiles/" + textInput + ".txt";
#else
        return Application.dataPath + "/SaveFiles/" + textInput + ".txt";
#endif
    }
    private void GetFileDataPath()
    {
#if UNITY_EDITOR
        PathToFile = Application.dataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#elif UNITY_ANDROID
        PathToFile = Application.persistentDataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#elif UNITY_IPHONE
        PathToFile = Application.persistentDataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#else
        PathToFile = Application.dataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#endif
    }
    private string getFolderDataPath()
    {
#if UNITY_EDITOR
        Debug.Log(Application.dataPath + "/SaveFiles");
        return Application.dataPath + "/SaveFiles";
#elif UNITY_ANDROID
        Debug.Log(Application.persistentDataPath + "/SaveFiles");
        return Application.persistentDataPath + "/SaveFiles";
#elif UNITY_IPHONE
        Debug.Log(Application.persistentDataPath + "/SaveFiles");
        return Application.persistentDataPath + "/SaveFiles";
#else
        Debug.Log(Application.dataPath + "/SaveFiles");
        return Application.dataPath + "/SaveFiles";
#endif
    }
    #endregion

}
