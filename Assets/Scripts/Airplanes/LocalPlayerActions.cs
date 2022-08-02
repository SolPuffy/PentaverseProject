using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;

public class LocalPlayerActions : MonoBehaviour
{
    public static LocalPlayerActions Instance;


    public GridBaseStructure PlayerVisibleGrid;
    public int CurrentPowerup;
    public int LocalPlayerIndex;


    public bool HitCalled = false;
    [SerializeField]
    public GameObject BackPanel;
    public GameObject PlayerCamera;
    public Tilemap PlayingField;
    public Tile TargetedTile;

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
    }
    //raycastToATile
    //ReturnInformation
    //DoStuffBasedOnTypeOfTileHit

    #region PlayerToServerCommands
    //[Command]
    public async void RaycastToTile()
    {
        Ray RayC = Camera.main.ScreenPointToRay(Input.mousePosition);
        await Cast3dRayTo2dCell(RayC);
        ServerActions.Instance.HitCalledOnTileLocation(TargetedTile);
    }
    #endregion
    #region FunctionsRunLocally
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
        if (Physics.Raycast(SnapshotOfRay.origin, SnapshotOfRay.direction, out RaycastHit RayH, 30f, 1 << LayerMask.NameToLayer("Tilemap")))
        {
            Debug.DrawLine(SnapshotOfRay.origin, RayH.point, Color.red, 3f);
            TargetedTile = (Tile)PlayingField.GetTile(PlayingField.WorldToCell(RayH.point));
        }
        else
        {
            TargetedTile = null;
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
