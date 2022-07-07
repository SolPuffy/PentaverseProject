using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

[System.Serializable]
public class BackupData
{
    public string TimeOFGameStart;
    public string TimeOfGameEnd;
    public int ActionsPerformed;
    public List<playerActions> Actions = new List<playerActions>();
    public List<CardValueType> GameDeck = new List<CardValueType>();
    
}
[System.Serializable]
public class playerActions
{
    public int playerIndex;
    public string playerName;
    public string actionType;
    public bool SlapSuccessful;
    public float SlapResponseTime;
}
public class ServerBackup : MonoBehaviour
{
    //DO NOT CREATE ANY PUBLIC ACCESS POINT TO EITHER 'ServerInstance' OR 'DataHold' FOR SECURITY REASONS

    private static ServerBackup ServerInstance;
    private string fileDataPath;
    [SerializeField] private BackupData DataHold = new BackupData();
    private void Awake()
    {
        ServerInstance = this;
        //makesSureSavefilesFolderAlwaysExists
        CheckFolderDataPath();
    }
    private void Start()
    {
        //PerformBackup();
    }
    //CONSOLE OUT
    private void calloutToConsole(string input)
    {
        HitSlapRazboi.instance.firstPlayer.DisplayConsoleOut(input);
    }    
    //
    #region BackupFunctions
    private async Task PerformBackup()
    {
        GetFileDataPath();

        DataHold.ActionsPerformed = DataHold.Actions.Count;
        DataHold.TimeOfGameEnd = DateTime.Now.ToString("G");

        string JsonOutput = JsonUtility.ToJson(DataHold, true);
        await System.IO.File.WriteAllTextAsync(fileDataPath, JsonOutput);

        Debug.Log($"Backup location: {fileDataPath}, Backup date: {DataHold.TimeOfGameEnd}");
    }
    public async Task ReadBackup(string inputToFile)
    {
        if (inputToFile.Length == 9)
        {
            inputToFile = ReadFileDataPath(inputToFile);
            if (File.Exists(inputToFile))
            {
                string JsonInput = await System.IO.File.ReadAllTextAsync(inputToFile);
                JsonUtility.FromJsonOverwrite(JsonInput, DataHold);
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
    #region Statics
    public static void BackupDeck(List<CardValueType> DeckList)
    {
        ServerInstance.DataHold.GameDeck = DeckList;
    }
    public static void AddHitToList(int playerIndex, string playerName)
    {
        playerActions act = new playerActions();
        act.actionType = "Hit";
        act.playerIndex = playerIndex;
        act.playerName = playerName;
        act.SlapSuccessful = false;
        act.SlapResponseTime = 0;
        ServerInstance.DataHold.Actions.Add(act);
        ServerInstance.calloutToConsole($"Player {playerName} has Hit at {DateTime.Now.ToString("T")}");
    }
    public static void AddSlapToList(int playerIndex, string playerName, bool Success, float ResponseTime)
    {
        playerActions act = new playerActions();
        act.actionType = "Slap";
        act.playerIndex = playerIndex;
        act.playerName = playerName;
        act.SlapSuccessful = Success;
        act.SlapResponseTime = ResponseTime;
        ServerInstance.DataHold.Actions.Add(act);
        ServerInstance.calloutToConsole($"Player {playerName} has Slapped at {DateTime.Now.ToString("T")} with {ResponseTime} Delay, Success = {Success}");
    }
    public static void CleanDataHold()
    {
        ServerInstance.DataHold.GameDeck.Clear();
        ServerInstance.DataHold.Actions.Clear();
    }
    public static void RegisterStartTime()
    {
        ServerInstance.DataHold.TimeOFGameStart = DateTime.Now.ToString("G");
    }
    public async static void PerformServerBackup()
    {
        await ServerInstance.PerformBackup();
        Debug.Log("Backup Complete");
    }
    public async static Task<BackupData> RetrieveDataHoldFromServer(string fileIndex)
    {
        await ServerInstance.ReadBackup(fileIndex);
        return ServerInstance.DataHold;
    }
    #endregion
    #region ChecksAndRandomGeneration
    private string generateRandomSaveId()
    {
        string RandomToReturn = "";
        for (int i = 0; i < 9; i++)
        {
            RandomToReturn += UnityEngine.Random.Range(0, 9).ToString();
        }
        return RandomToReturn;
    }
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
    #endregion
    #region FilesLocations
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
        fileDataPath = Application.dataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#elif UNITY_ANDROID
        fileDataPath = Application.persistentDataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#elif UNITY_IPHONE
        fileDataPath = Application.persistentDataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
#else
        fileDataPath = Application.dataPath + "/SaveFiles/" + generateRandomSaveId() + ".txt";
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