using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

[System.Serializable]
public class SimData
{
    public int AmountOfSimulations;
    public string RulesUsed;
    public float AverageRunTime;
    public int AverageActions;
    public int AverageHits;
    public int AverageSlaps;
    public RunTimeCollection CollectionDataHold = new RunTimeCollection();
}
[System.Serializable]
public class RunTimeCollection
{
    public float TotalRunTime = 0;
    public int TotalActions = 0;
    public int TotalHits = 0;
    public int TotalSlaps = 0;
    public string TimeStart = "";
    public string TimeEnd = "";
}
public class SimulationCollection : MonoBehaviour
{
    private static SimulationCollection Instance;
    public SimmulationSettings SettingsRef;
    public SimmulateGame GameSimRef;
    private string fileDataPath;
    private int pings = 0;
    [SerializeField] private SimData SimDataHold = new SimData();
    private void Awake()
    {
        Instance = this;
        //makesSureSavefilesFolderAlwaysExists
        CheckFolderDataPath();
    }
    private void Start()
    {
        //PerformBackup();
    }

    public async void BeginSim()
    {
        GameSimRef.rulesSetting = (int)SettingsRef.Rules;
        await LoadSims();
        PerformBackup();
    }
    public async Task LoadSims()
    {
        List<Task> tasks = new List<Task>();
        for(int i=0;i<SettingsRef.AmountOfSimulations;i++)
        {
            SimmulateGame sim = new SimmulateGame();

            sim.A = UnityEngine.Random.Range(0.65f, 1.75f);
            sim.B = UnityEngine.Random.Range(0.65f, 1.75f);
            sim.C = UnityEngine.Random.Range(0.65f, 1.75f);
            sim.D = UnityEngine.Random.Range(0.65f, 1.75f);
            sim.E = UnityEngine.Random.Range(0.65f, 1.75f);

            sim.RandomTickA = UnityEngine.Random.Range(0, 8);
            sim.RandomTickB = UnityEngine.Random.Range(0.15f, 0.35f);

            tasks.Add(await Task.Factory.StartNew(() => sim.Simmulate()));
            Debug.Log($"Iterration Progress");
        }
        Task.WaitAll(tasks.ToArray());

        //Debug.Log("Finished All Tasks");
    }    
    #region BackupFunctions
    private async void PerformBackup()
    {
        GetFileDataPath();

        Instance.SimDataHold.AmountOfSimulations = Instance.SettingsRef.AmountOfSimulations;
        switch((int)Instance.SettingsRef.Rules)
        {
            case 0: { Instance.SimDataHold.RulesUsed = "Normal"; break; }
            case 1: { Instance.SimDataHold.RulesUsed = "Passable"; break; }
            case 2: { Instance.SimDataHold.RulesUsed = "Hybrid"; break; }
            case 3: { Instance.SimDataHold.RulesUsed = "Bullet"; break; }
        }
        Instance.SimDataHold.AverageRunTime = Instance.SimDataHold.CollectionDataHold.TotalRunTime / Instance.SettingsRef.AmountOfSimulations;
        Instance.SimDataHold.AverageActions = Instance.SimDataHold.CollectionDataHold.TotalActions / Instance.SettingsRef.AmountOfSimulations;
        Instance.SimDataHold.AverageHits = Instance.SimDataHold.CollectionDataHold.TotalHits / Instance.SettingsRef.AmountOfSimulations;
        Instance.SimDataHold.AverageSlaps = Instance.SimDataHold.CollectionDataHold.TotalSlaps / Instance.SettingsRef.AmountOfSimulations;
        
        string JsonOutput = JsonUtility.ToJson(SimDataHold, true);
        await System.IO.File.WriteAllTextAsync(fileDataPath, JsonOutput);

        Debug.Log($"Simulation location: {fileDataPath}");
    }
    #endregion
    #region Statics
    public static void CollectHit()
    {
        Instance.SimDataHold.CollectionDataHold.TotalHits++;
        Instance.SimDataHold.CollectionDataHold.TotalActions++;
    }
    public static void CollectSlap()
    {
        Instance.SimDataHold.CollectionDataHold.TotalSlaps++;
        Instance.SimDataHold.CollectionDataHold.TotalActions++;
    }
    public static void CollectTime(float amount)
    {
        Instance.SimDataHold.CollectionDataHold.TotalRunTime += amount;
    }  
    public static void SetTimeStart(string time)
    {
        Instance.SimDataHold.CollectionDataHold.TimeStart = time;
    }
    public static void SetTimeEnd(string time)
    {
        Instance.SimDataHold.CollectionDataHold.TimeEnd = time;
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
    private void GetFileDataPath()
    {
#if UNITY_EDITOR
        fileDataPath = Application.dataPath + "/SaveFiles/" + Instance.SettingsRef.Rules + "_" + Instance.SettingsRef.AmountOfSimulations + "_" + generateRandomSaveId() + ".txt";
#elif UNITY_ANDROID
        fileDataPath = Application.persistentDataPath + "/SaveFiles/" + Instance.SimDataHold.RulesUsed + "_" + Instance.SettingsRef.AmountOfSimulations + "_" + generateRandomSaveId() + ".txt";
#elif UNITY_IPHONE
        fileDataPath = Application.persistentDataPath + "/SaveFiles/" + Instance.SimDataHold.RulesUsed + "_" + Instance.SettingsRef.AmountOfSimulations + "_" + generateRandomSaveId() + ".txt";
#else
        fileDataPath = Application.dataPath + "/SaveFiles/" + Instance.SimDataHold.RulesUsed + "_" + Instance.SettingsRef.AmountOfSimulations + "_" + generateRandomSaveId() + ".txt";
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
