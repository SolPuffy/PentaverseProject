using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
public class PlanesPlayer : NetworkBehaviour
{
    public static PlanesPlayer localPlayer;
    [SyncVar] public int playerIndex = 20;
    [SyncVar] public string Nome = "P";
    [SyncVar] public bool HasEntered = false;

    private void Start()
    {
        if (isLocalPlayer)
        {
            localPlayer = this;
            if (!HitSlapRazboi.instance.InititalSetupDone)
            {
                {
                    SetPlayerIndex();
                }
            }
            else
            {
                NetworkManager.singleton.StopClient();
            }
        }
    }
    [Command]
    public void StartGame()
    {
        ServerActions.Instance.SetupBoard();
    }

    [ClientRpc]
    public void SetUpDone()
    {
        ServerActions.Instance.SetupInProgress = false;
    }
    [Command]
    public async void HitTile(Vector3Int TilePos)
    {
        ServerActions.Instance.HitCalled = true;
        if (ServerActions.Instance.PlayersList[ServerActions.Instance.CurrentPlayerTurn].CurrentHeldPowerup != null)
        {
            //if no powerup is present, shoot normally
            await ServerActions.Instance.PlayersList[ServerActions.Instance.CurrentPlayerTurn].CurrentHeldPowerup.OnUse(TilePos);
        }
        else
        {
            //else use powerup on your shot
            await ServerActions.Instance.VerifyAndUpdateTile(TilePos);
            await ServerActions.Instance.HitCalledOnTileLocation(TilePos);
        }
    }
    [TargetRpc]
    public void UpdateTile(Vector3Int pos, int TileIndex)
    {
        LocalPlayerActions.Instance.PlayingField.SetTile(pos, ServerActions.Instance.Tiles[TileIndex]);
    }

    [ClientRpc]
    public void SetHitCalled(bool bul)
    {
        ServerActions.Instance.HitCalled = bul;
    }

    [TargetRpc]
    public void DC()
    {
        NetworkManager.singleton.StopClient();
    }

    public override void OnStopClient()
    {
        Debug.Log($"Client {name}, index {playerIndex} Stopped  ");
        //ClientDisconnect();
    }

    [Command]
    void SetPlayerIndex()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        if (Players.Length > 4)
        {
            DC();
        }
        else
        {
            HasEntered = true;
            //AddDeck();
            SetName();
            Debug.Log($"Setting {name} index to : " + playerIndex);
        }
    }

    void SetName()
    {
        Nome = "P " + (playerIndex + 1).ToString();
        HitSlapRazboi.instance.PlayerNames.Add(Nome);
        Debug.Log($"Setting {name} name to : " + Nome);
    }
}
