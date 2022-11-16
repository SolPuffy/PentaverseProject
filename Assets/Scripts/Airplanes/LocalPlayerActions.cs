using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LocalPlayerActions : MonoBehaviour
{
    public static LocalPlayerActions Instance;

    public Sprite[] SpriteBun;

    //public GridBaseStructure PlayerVisibleGrid;
    //public int CurrentPowerup;
    public int LocalPlayerIndex;
    [SerializeField] GameObject HostMenu;
    [SerializeField] TextMeshProUGUI NameField;
    [SerializeField] GameObject EndGame;
    [SerializeField] Image BGPlayer;
    [SerializeField] TextMeshProUGUI SmallConsole;
    [SerializeField] TMP_Dropdown choice;



    [SerializeField]
    public PowerUpSlot[] PowerupsInventory = new PowerUpSlot[4];
    public GameObject BackPanel;
    public GameObject PlayerCamera;
    public Tilemap PlayingField;
    public Vector3Int TargetedTileLocation;

    private int BardSize = 21;
    public int PowerupSlotForCommand;
    public Sprite BlankPixel;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        EndGame.SetActive(false);
        //SetBackPanelToGridSize();
        if (Application.isBatchMode)
        {
            Debug.Log("I am planes server");
        }

    }
    private void Update()
    {
        if (Application.isBatchMode) return;
        if (PlanesPlayer.localPlayer == null) return;

        UpdateName();
        UpdatePortraits();

        if (PlanesPlayer.localPlayer.playerIndex != 0 || !ServerActions.Instance.SetupInProgress)
            HostMenu.SetActive(false);
        else
            HostMenu.SetActive(true);


        
        if (!ServerActions.Instance.SetupInProgress && Input.GetMouseButtonDown(0) && ServerActions.Instance.CurrentPlayerTurn == PlanesPlayer.localPlayer.playerIndex && !ServerActions.Instance.HitCalled)
        {
            RaycastToTile();
        }
        if (Input.GetKey(KeyCode.Keypad0))
        {
            SendDebugCommandToServer();
        }
    }
    //[Command]
    private void SendDebugCommandToServer()
    {
        if (Input.GetKey(KeyCode.Keypad0) && Input.GetKeyDown(KeyCode.Keypad1))
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            PlanesPlayer.localPlayer.GiveEveryonePowerups(PowerupSlotForCommand);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        if(Input.GetKey(KeyCode.Keypad0) && Input.GetKeyDown(KeyCode.Keypad2))
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServerActions.Instance.RevealToAllPlayersCurrentBoardState();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        if (Input.GetKey(KeyCode.Keypad0) && Input.GetKeyDown(KeyCode.Keypad3))
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServerActions.Instance.ResetBoard();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        if(Input.GetKey(KeyCode.Keypad0) && Input.GetKeyDown(KeyCode.KeypadPeriod))
        {
            ServerActions.Instance.DebugPlayerIndexSwitcher();
        }
    }
    //raycastToATile
    //ReturnInformation
    //DoStuffBasedOnTypeOfTileHit

    #region PlayerToServerCommands
    //[Command]
    public void SendTileInformationToServer()
    {
        PlanesPlayer.localPlayer.HitTile(TargetedTileLocation);
    }
    #endregion
    #region FunctionsRunLocally
    public async void RaycastToTile()
    {
        Ray RayC = Camera.main.ScreenPointToRay(Input.mousePosition);
        await Cast3dRayTo2dCell(RayC);
        if(TargetedTileLocation == new Vector3Int(-1,-1))
        {
            return;
        }
        if(Int16.Parse(PlayingField.GetTile(TargetedTileLocation).name) == LocalPlayerIndex + 5)
        {
            Debug.Log("You can't target yourself. Dumbass.");
            return;
        }
        SendTileInformationToServer();
    }
    public void SetBackPanelToGridSize(int Bardsize)
    {
        BardSize = Bardsize;
        float BoardScaling = ((0.28f * Bardsize) + Bardsize) / 2f;
        BackPanel.transform.position = new Vector3(BoardScaling + 2f, BoardScaling,0);
        PlayerCamera.transform.position = new Vector3(BoardScaling, BoardScaling, -((0.28f * Bardsize) + Bardsize));
        BoardScaling = ((0.28f * Bardsize) + Bardsize) / 10f;
        BackPanel.transform.localScale = new Vector3(BoardScaling, 1, BoardScaling);
    }
    public async Task Cast3dRayTo2dCell(Ray SnapshotOfRay)
    {
        if (Physics.Raycast(SnapshotOfRay.origin, SnapshotOfRay.direction, out RaycastHit RayH, BardSize * 2.5f, 1 << LayerMask.NameToLayer("Tilemap")))
        {
            Debug.DrawLine(SnapshotOfRay.origin, RayH.point, Color.red, 3f);
            TargetedTileLocation = PlayingField.WorldToCell(RayH.point);
        }
        else
        {
            TargetedTileLocation = new Vector3Int(-1, -1);
            Debug.Log("Fetch Error");
        }
        await Task.Yield();
    }
    /*public async Task<Tile> Translate3dRayTo2d(Vector3 PointHitBy3dRay)
    {
        RaycastHit2D GridRayH = Physics2D.Raycast(new Vector3(PointHitBy3dRay.x, PointHitBy3dRay.y, PointHitBy3dRay.z - 1),PointHitBy3dRay,5f, 0 << LayerMask.NameToLayer("Tilemap"));
        Debug.DrawLine(new Vector3(PointHitBy3dRay.x, PointHitBy3dRay.y, PointHitBy3dRay.z - 1), PointHitBy3dRay, Color.blue, 3f);
        return await Task.FromResult();
    }*/
    #endregion
    //UseDiscoveredPowers

    public void StartGame()
    {
        PlanesPlayer.localPlayer.ChangeBoardSize(ChangeSquare());
        PlanesPlayer.localPlayer.StartGame();
    }

    private void UpdateName()
    {
        NameField.text = PlanesPlayer.localPlayer.Nome;
    }

    public void FinishGame()
    {
        EndGame.SetActive(true);
    }

    public void DC()
    {
        NetworkManager.singleton.StopClient();
    }
    public void ShowText(string STR)
    {
        SmallConsole.text = STR;
    }

    private int ChangeSquare()
    {
        int size;
        switch(choice.value)
        {
            case 0:
                size = 33;
                break;
            case 1:
                size = 42;
                break;
            case 2:
                size = 51;
                break;
            case 3:
                size = 69;
                break;
            case 4:
                size = 81;
                break;
            case 5:
                size = 99;
                break;
            default:
                size = 33;
                break;
        }
        return size;
    }

    private void UpdatePortraits()
    {
        for (int i = 0; i < OrbControl.accessPlayerSlots.Length; i++)
        {
            OrbControl.accessPlayerSlots[i].SetActive(false);
            OrbControl.acessLocalPlayer[i].enabled = false;
        }

        for(int i = 0; i<ServerActions.Instance.PlanesPlayers.Count; i++)
        {
            OrbControl.accessPlayerSlots[i].SetActive(true);

            if(PlanesPlayer.localPlayer.playerIndex == i)
                OrbControl.acessLocalPlayer[i].enabled = true;
            

            if (!ServerActions.Instance.SetupInProgress && ServerActions.Instance.CurrentPlayerTurn == i)
                OrbControl.ChangeOrbCustom(i, 0);
            else
                OrbControl.ChangeOrbCustom(i, 1);           

            if (!ServerActions.Instance.SetupInProgress && ServerActions.Instance.PlayersList[i].Misfire)
                OrbControl.ChangeOrbCustom(i, 4);

            if (!ServerActions.Instance.SetupInProgress && ServerActions.Instance.PlayersList[i].Disconnected)
                OrbControl.ChangeOrbCustom(i, 3);

            if (!ServerActions.Instance.SetupInProgress && ServerActions.Instance.PlayersList[i].isDestroyed)
                OrbControl.ChangeOrbCustom(i, 2);
        }

        /*
        if (!ServerActions.Instance.SetupInProgress && ServerActions.Instance.CurrentPlayerTurn == PlanesPlayer.localPlayer.playerIndex && !ServerActions.Instance.HitCalled)
        {
            BGPlayer.color = Color.green;
        }
        else
        {
            BGPlayer.color = Color.white;
        }
        */
    }
}
