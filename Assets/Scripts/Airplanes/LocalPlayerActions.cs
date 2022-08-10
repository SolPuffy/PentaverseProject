using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;

public class LocalPlayerActions : MonoBehaviour
{
    public static LocalPlayerActions Instance;


    //public GridBaseStructure PlayerVisibleGrid;
    //public int CurrentPowerup;
    public int LocalPlayerIndex;


    public bool HitCalled = false;
    [SerializeField]
    public GameObject BackPanel;
    public GameObject PlayerCamera;
    public Tilemap PlayingField;
    public Vector3Int TargetedTileLocation;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SetBackPanelToGridSize();
    }
    private void Update()
    {
        if (!ServerActions.Instance.SetupInProgress && Input.GetMouseButtonDown(0) && ServerActions.Instance.CurrentPlayerTurn == LocalPlayerIndex && !HitCalled)
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
            ServerActions.Instance.ShowMap();
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
    public async void SendTileInformationToServer()
    {
        HitCalled = true;
        if(ServerActions.Instance.PlayersList[ServerActions.Instance.CurrentPlayerTurn].CurrentHeldPowerup != null)
        {
            //if no powerup is present, shoot normally
            await ServerActions.Instance.PlayersList[ServerActions.Instance.CurrentPlayerTurn].CurrentHeldPowerup.OnUse(TargetedTileLocation);
        }
        else
        {
            //else use powerup on your shot
            await ServerActions.Instance.VerifyAndUpdateTile(TargetedTileLocation);
            await ServerActions.Instance.HitCalledOnTileLocation(TargetedTileLocation);
        }
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
    public void SetBackPanelToGridSize()
    {
        float BoardScaling = ((0.28f * ServerActions.Instance.BoardSize) + ServerActions.Instance.BoardSize) / 2f;
        BackPanel.transform.position = new Vector3(BoardScaling, BoardScaling,0);
        PlayerCamera.transform.position = new Vector3(BoardScaling, BoardScaling, -((0.28f * ServerActions.Instance.BoardSize) + ServerActions.Instance.BoardSize));
        BoardScaling = ((0.28f * ServerActions.Instance.BoardSize) + ServerActions.Instance.BoardSize) / 10f;
        BackPanel.transform.localScale = new Vector3(BoardScaling, 1, BoardScaling);
    }
    public async Task Cast3dRayTo2dCell(Ray SnapshotOfRay)
    {
        if (Physics.Raycast(SnapshotOfRay.origin, SnapshotOfRay.direction, out RaycastHit RayH, ServerActions.Instance.BoardSize * 2.5f, 1 << LayerMask.NameToLayer("Tilemap")))
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
}
