using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

[System.Serializable]
public class BackupData
{
    public string TimeOFGameStart;
    public string TimeOfGameEnd;
    public int ActionsPerformed;
    public List<Action> Actions = new List<Action>();
    public List<CardValueType> GameDeck = new List<CardValueType>();    
}
[System.Serializable]
public class Action
{
    public int playerIndex;
    public string playerName;
    public string actionType;
    public bool WonRound;
    public int CardValue;
    public string CardType;
    public bool SlapSuccessful;
    public int SlapResponseTime;
    public GameState CurrentGameState;
}
[System.Serializable]
public class FileNamesResponse
{
    public List<string> fileNames;
}

[System.Serializable]
public class GameState
{
    public int[] CardCount;
    public int[] CardsonGroundIndexes;
    public int TurnIndex;
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
        Debug.Log("server instance");
        //makesSureSavefilesFolderAlwaysExists
        //CheckFolderDataPath();
    }
    private void Start()
    {
        //PerformBackup();
    }
    //CONSOLE OUT
    private void calloutToConsole(string input)
    {
        //HitSlapRazboi.instance.firstPlayer.DisplayConsoleOut(input);
    }    
    //
    #region BackupFunctions
    private async Task PerformBackup()
    {
        //GetFileDataPath();

        DataHold.ActionsPerformed = DataHold.Actions.Count;
        DataHold.TimeOfGameEnd = DateTime.Now.ToString("G");

        string JsonOutput = JsonUtility.ToJson(DataHold);
        //await System.IO.File.WriteAllTextAsync(PathToFile, JsonOutput);

        string key = "d34efaf7-ab1b-4d91-897a-63ed3efc2abf-48326fdb-a038-4ca2-ad29-2b11a170ad0b";
        string generatedID = generateDateSaveId();
        //subDir pentacards = cards | pentaplanes = planes | pentawords = words !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        string subDir = "cards";
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        byte[] payload = System.Text.Encoding.UTF8.GetBytes(JsonOutput);
        //Debug.Log($"Trying to send element at adress: http://localhost:3000/api/postfile?subDir={subDir}&fileName={generatedID}");
        //188.166.90.161
        using (UnityWebRequest request = UnityWebRequest.Post($"http://188.166.90.161:3000/api/postfile?subDir={subDir}&fileName={generatedID}", "POST"))
        {
            request.SetRequestHeader("x-api-key", key);
            request.uploadHandler = new UploadHandlerRaw(payload);
            //request.downloadHandler = new DownloadHandlerBuffer();

            await Task.FromResult(request.SendWebRequest());
            while (request.result == UnityWebRequest.Result.InProgress)
            {
                Debug.LogError($"Save attempt in progress! Code:{request.uploadProgress}");
                await Task.Yield();
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Save failed! Error:{request.error}");
                return;
            }
            else
            {
                string text = request.downloadHandler.text;
                try
                {
                    Debug.Log(text);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse JSON response. Error: {ex.Message}");
                }
                Debug.Log("Save Successful");
            }

            UnityEngine.Debug.Log($"Backup location: {generatedID}, Backup date: {DataHold.TimeOfGameEnd}");
        }
    }
    public async Task ReadBackup(string inputToFile)
    {
        string key = "d34efaf7-ab1b-4d91-897a-63ed3efc2abf-48326fdb-a038-4ca2-ad29-2b11a170ad0b";
        string subDir = "cards";
        //Debug.Log($"Trying to find element at adress: http://localhost:3000/api/getfile?subDir={subDir}&fileName={fileIndex}");
        //188.166.90.161
        using (UnityWebRequest request = UnityWebRequest.Get($"http://188.166.90.161:3000/api/getfile?subDir={subDir}&fileName={inputToFile}"))
        {
            request.SetRequestHeader("x-api-key", key);

            await Task.FromResult(request.SendWebRequest());
            while (request.result == UnityWebRequest.Result.InProgress)
            {
                await Task.Yield();
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Load failed! Error:{request.error}");
                return;
            }
            else
            {
                string text = request.downloadHandler.text;
                try
                {
                    string ClearedText = Regex.Replace(text, @"\\", "");
                    string RetrievedData = ClearedText.Substring(1, ClearedText.Length - 2);

                    JsonUtility.FromJsonOverwrite(RetrievedData, DataHold);
                    Debug.Log("Load Successful");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse JSON response. Error: {ex.Message}");
                }
            }
        }
    }
    private async Task<string[]> ReadDir()
    {
        string key = "d34efaf7-ab1b-4d91-897a-63ed3efc2abf-48326fdb-a038-4ca2-ad29-2b11a170ad0b";
        string subDir = "cards";
        //Debug.Log($"Trying to find element at adress: http://localhost:3000/api/getdir?subDir={subDir}&fileName={fileIndex}");
        //188.166.90.161
        using (UnityWebRequest request = UnityWebRequest.Get($"http://188.166.90.161:3000/api/getdir?subDir={subDir}"))
        {
            request.SetRequestHeader("x-api-key", key);

            await Task.FromResult(request.SendWebRequest());
            while (request.result == UnityWebRequest.Result.InProgress)
            {
                await Task.Yield();
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Load failed! Error:{request.error}");
                return null;
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                try
                {
                    FileNamesResponse response = JsonUtility.FromJson<FileNamesResponse>(jsonResponse);
                    List<string> fileNames = response.fileNames;
                    return await Task.FromResult(fileNames.ToArray());
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to parse JSON response. Error: {ex.Message}");
                    return null;
                }
            }
        }
    }
    #endregion
    #region Statics
    public static void BackupDeck(List<CardValueType> DeckList)
    {
        ServerInstance.DataHold.GameDeck = DeckList;
    }
    public static void AddHitToList(int playerIndex, string playerName, int CardValue, string CardType, Mirror.SyncList<int> _cardCount, SyncListCards _cardsOnGround, CardValueType SlapCard, int _currentIndexTurn, bool won)
    {
        //Create Action
        Action act = new Action();
        act.actionType = "Hit";
        act.playerIndex = playerIndex;
        act.playerName = playerName;
        act.CardValue = CardValue;
        act.CardType = CardType;
        act.WonRound = won;
        act.SlapSuccessful = false;
        act.SlapResponseTime = 0;
        ///

        //Create GameState
        GameState gameState = new GameState();        

        gameState.CardCount = new int[_cardCount.Count];
        for(int i = 0; i < _cardCount.Count; i++)
        {
            gameState.CardCount[i] = _cardCount[i];
        }

        gameState.CardsonGroundIndexes = new int[4];
        if (SlapCard == null) { gameState.CardsonGroundIndexes[0] = -1; } else { gameState.CardsonGroundIndexes[0] = SlapCard.CardSpriteIndex; }

        switch (_cardsOnGround.Count)
        {
            case 0:
                {
                    gameState.CardsonGroundIndexes[1] = -1;
                    gameState.CardsonGroundIndexes[2] = -1;
                    gameState.CardsonGroundIndexes[3] = -1;
                    break;
                }
            case 1:
                {
                    gameState.CardsonGroundIndexes[1] = -1;
                    gameState.CardsonGroundIndexes[2] = -1;
                    gameState.CardsonGroundIndexes[3] = _cardsOnGround[_cardsOnGround.Count - 1].CardSpriteIndex;
                    break;
                }
            case 2:
                {
                    gameState.CardsonGroundIndexes[1] = -1;
                    gameState.CardsonGroundIndexes[2] = _cardsOnGround[_cardsOnGround.Count - 2].CardSpriteIndex;
                    gameState.CardsonGroundIndexes[3] = _cardsOnGround[_cardsOnGround.Count - 1].CardSpriteIndex;
                    break;
                }
            //over 3
            default:
                {
                    gameState.CardsonGroundIndexes[1] = _cardsOnGround[_cardsOnGround.Count - 3].CardSpriteIndex;
                    gameState.CardsonGroundIndexes[2] = _cardsOnGround[_cardsOnGround.Count - 2].CardSpriteIndex;
                    gameState.CardsonGroundIndexes[3] = _cardsOnGround[_cardsOnGround.Count - 1].CardSpriteIndex;
                    break;
                }
        }

        gameState.TurnIndex = _currentIndexTurn;
        ////
        
        act.CurrentGameState = gameState;

        ServerInstance.DataHold.Actions.Add(act);
       
    }
   
    public static void AddSlapToList(int playerIndex, string playerName,bool Success, int ResponseTime,  int CardValue, string CardType, Mirror.SyncList<int> _cardCount, SyncListCards _cardsOnGround, CardValueType SlapCard, int _currentIndexTurn, bool won)
    {
        //Create Action
        Action act = new Action();
        act.actionType = "Slap";
        act.playerIndex = playerIndex;
        act.playerName = playerName;
        act.SlapSuccessful = Success;
        act.SlapResponseTime = ResponseTime;
        act.CardValue = CardValue;
        act.CardType = CardType;
        act.WonRound = won;
        ////       

        //Create GameState
        GameState gameState = new GameState();

        gameState.CardCount = new int[_cardCount.Count];
        for (int i = 0; i < _cardCount.Count; i++)
        {
            gameState.CardCount[i] = _cardCount[i];
        }

        gameState.CardsonGroundIndexes = new int[4];
        if (SlapCard == null) { gameState.CardsonGroundIndexes[0] = -1; } else { gameState.CardsonGroundIndexes[0] = SlapCard.CardSpriteIndex; }

        switch (_cardsOnGround.Count)
        {
            case 0:
                {
                    gameState.CardsonGroundIndexes[1] = -1;
                    gameState.CardsonGroundIndexes[2] = -1;
                    gameState.CardsonGroundIndexes[3] = -1;
                    break;
                }
            case 1:
                {
                    gameState.CardsonGroundIndexes[1] = -1;
                    gameState.CardsonGroundIndexes[2] = -1;
                    gameState.CardsonGroundIndexes[3] = _cardsOnGround[_cardsOnGround.Count-1].CardSpriteIndex;                    
                    break;
                }
            case 2:
                {
                    gameState.CardsonGroundIndexes[1] = -1;
                    gameState.CardsonGroundIndexes[2] = _cardsOnGround[_cardsOnGround.Count - 2].CardSpriteIndex; 
                    gameState.CardsonGroundIndexes[3] = _cardsOnGround[_cardsOnGround.Count - 1].CardSpriteIndex;                    
                    break;
                }
            //over 3
            default:
                {
                    gameState.CardsonGroundIndexes[1] = _cardsOnGround[_cardsOnGround.Count - 3].CardSpriteIndex;
                    gameState.CardsonGroundIndexes[2] = _cardsOnGround[_cardsOnGround.Count - 2].CardSpriteIndex;
                    gameState.CardsonGroundIndexes[3] = _cardsOnGround[_cardsOnGround.Count - 1].CardSpriteIndex;
                    break;                    
                }
        }        

        gameState.TurnIndex = _currentIndexTurn;
        ////

        act.CurrentGameState = gameState;
        ServerInstance.DataHold.Actions.Add(act);
        
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

    public async static Task<string[]> RequestDirData()
    {
        return await Task.FromResult(await ServerInstance.ReadDir());
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
    private string generateDateSaveId()
    {
        return $"{DateTime.Now.ToString("dd")}-{DateTime.Now.ToString("MM")}-{DateTime.Now.ToString("yy")}_{DateTime.Now.ToString("HH")},{DateTime.Now.ToString("mm")},{DateTime.Now.ToString("ss")}";
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