using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

[System.Serializable]
public class BackupData
{
    public int ActionsPerformed;
    public string Date;
    public List<CardValueType> GameDeck = new List<CardValueType>();
    public List<string> playerActions = new List<string>();
    public List<int> indexParameters = new List<int>();
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
    #region Statics
    public static void BackupDeck(List<CardValueType> DeckList)
    {
        ServerInstance.DataHold.GameDeck = DeckList;
    }
    public static void AddHitToList(int playerIndex)
    {
        ServerInstance.DataHold.playerActions.Add("Hit");
        ServerInstance.DataHold.indexParameters.Add(playerIndex);
    }
    public static void AddSlapToList(int playerIndex)
    {
        ServerInstance.DataHold.playerActions.Add("Slap");
        ServerInstance.DataHold.indexParameters.Add(playerIndex);
    }
    public static void CleanDataHold()
    {
        ServerInstance.DataHold.GameDeck.Clear();
        ServerInstance.DataHold.playerActions.Clear();
        ServerInstance.DataHold.indexParameters.Clear();
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
    #region BackupFunctions
    private async Task PerformBackup()
    {
        GetFileDataPath();

        DataHold.ActionsPerformed = DataHold.playerActions.Count;
        DataHold.Date = DateTime.Now.ToString("G");

        string JsonOutput = JsonUtility.ToJson(DataHold, true);
        await System.IO.File.WriteAllTextAsync(fileDataPath, JsonOutput);

        Debug.Log($"Backup location: {fileDataPath}, Backup date: {DataHold.Date}");
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
