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
            if (ServerActions.Instance.SetupInProgress)
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
    [ClientRpc]
    public void UPDATESTUFF(List<PlayerShipStructure> shipsBruh)
    {
        ServerActions.Instance.PlayersList = shipsBruh;
    }
    [TargetRpc]
    public async void givePlayerPowerup(PowerUp power,int playerIndex)
    {
        ReturnDebugToServer($"Give player {playerIndex}, powerup {power}!");
        for (int i=0;i<4;i++)
        {
            if(!LocalPlayerActions.Instance.PowerupsInventory[i].isOccupied)
            {
                LocalPlayerActions.Instance.PowerupsInventory[i].CurrentlyHeldPowerup = power;
                LocalPlayerActions.Instance.PowerupsInventory[i].isOccupied = true;
                LocalPlayerActions.Instance.PowerupsInventory[i].IndexOfPlayerHoldingPowerup = playerIndex;
                LocalPlayerActions.Instance.PowerupsInventory[i].SlotButtonInstance.image.sprite = LocalPlayerActions.Instance.SpriteBun[power.PowerUpIconIndex];
                LocalPlayerActions.Instance.PowerupsInventory[i].powerupSlot = i;

                //Instantly trigger if gained powerup is misfire
                if ((int)power.PowerUpType == 3)
                {
                    await LocalPlayerActions.Instance.PowerupsInventory[i].CurrentlyHeldPowerup.OnUse(playerIndex, i, playerIndex);
                    ReturnDebugToServer($"Player at index {i} has Misfired!");
                }

                break;
            }
            else
            {
                Debug.Log($"Powerup Slot {i} is occupied, ignoring.");
            }
        }
    }
    [TargetRpc]
    public void takePlayerPowerup(int powerupSlot)
    {
        ReturnDebugToServer("Take Player's Powerup!");
        LocalPlayerActions.Instance.PowerupsInventory[powerupSlot].CurrentlyHeldPowerup = null;
        LocalPlayerActions.Instance.PowerupsInventory[powerupSlot].isOccupied = false;
        LocalPlayerActions.Instance.PowerupsInventory[powerupSlot].SlotButtonInstance.image.sprite = LocalPlayerActions.Instance.BlankPixel;
    }
    [Command]
    public void ReturnDebugToServer(string DebugInfo)
    {
        Debug.Log($"{DebugInfo}");
    }

    [Command]
    public void StartGame()
    {
        ServerActions.Instance.SetupBoard();
    }

    [Command]
    public async void LoadPowerup(PowerUp activatingPowerup, int IndexOfPlayerHoldingPowerup, int powerupSlot)
    {
        Debug.Log($"Loading player for player {IndexOfPlayerHoldingPowerup}");
        if (activatingPowerup.IsRetroactive)
        {
            await activatingPowerup.OnUse(IndexOfPlayerHoldingPowerup, powerupSlot, IndexOfPlayerHoldingPowerup);
        }
        else
        {
            Debug.Log($"PowerUP {activatingPowerup.ToString()} prepared for PlayerIndex {IndexOfPlayerHoldingPowerup}");
            ServerActions.Instance.PlayersList[IndexOfPlayerHoldingPowerup].CurrentHeldPowerup = activatingPowerup;
            ServerActions.Instance.PlayersList[IndexOfPlayerHoldingPowerup].PowerupSlotIndex = powerupSlot;
           // ServerActions.Instance.PlanesPlayers[IndexOfPlayerHoldingPowerup].preparePowerup(IndexOfPlayerHoldingPowerup, activatingPowerup, powerupSlot);
        }        
    }

    public override void OnStopServer()
    {
        Debug.Log($"Client {name}, index {playerIndex} Stopped on Server");
        try { ServerActions.Instance.PlayersList[playerIndex].Disconnected = true; }
        catch { }
        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if ((players.Length - 1) == 0)
        {
            Debug.Log($"No Players Left. Resetting ...");
            ServerActions.Instance.ResetBoard();
            return;
        }

        if (!HasEntered)
        {
            Debug.Log("Player tried to join with no space left");
        }
        else
        {
            //HitSlapRazboi.instance.RemovePlayer(playerIndex);
            //ServerActions.Instance.PlanesPlayers.Remove(this);
            ServerActions.Instance.CheckTurn(playerIndex);
        }
    }

    [ClientRpc]
    public void SetUpProgress(bool bul)
    {
        ServerActions.Instance.SetupInProgress = bul;
    }   
    [Command]
    public async void HitTile(Vector3Int TilePos)
    {
        ServerActions.Instance.HitCalled = true;
        if (ServerActions.Instance.PlayersList[ServerActions.Instance.CurrentPlayerTurn].CurrentHeldPowerup != null)
        {
            //if powerup, attempt using it
            /*
            if (ServerActions.Instance.PlayersList[ServerActions.Instance.CurrentPlayerTurn].CurrentHeldPowerup.IsRetroactive)
            {
                //powerup is passive, shoot normally
                await ServerActions.Instance.VerifyAndUpdateTile(TilePos);
                await ServerActions.Instance.HitCalledOnTileLocation(TilePos);
                return;
            }
            */
            await ServerActions.Instance.PlayersList[ServerActions.Instance.CurrentPlayerTurn].CurrentHeldPowerup.OnUse(TilePos, ServerActions.Instance.PlayersList[ServerActions.Instance.CurrentPlayerTurn].PowerupSlotIndex, ServerActions.Instance.CurrentPlayerTurn);
            
        }
        else
        {
            //if no powerup, shoot normally
            await ServerActions.Instance.VerifyAndUpdateTile(TilePos);
            await ServerActions.Instance.HitCalledOnTileLocation(TilePos);
        }
    }
    [TargetRpc]
    public void UpdateTile(Vector3Int pos, int TileIndex)
    {
        LocalPlayerActions.Instance.PlayingField.SetTile(pos, ServerActions.Instance.Tiles[TileIndex]);
        LocalPlayerActions.Instance.ShowText(pos.ToString());
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

    [ClientRpc]
    public void FinishGame()
    {
        LocalPlayerActions.Instance.FinishGame();
    }
    [Command]
    void SetPlayerIndex()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

        if (Players.Length > 5)
        {
            DC();
        }
        else
        {
            HasEntered = true;
            //AddDeck();
            ServerActions.Instance.PlanesPlayers.Add(this);
            playerIndex = ServerActions.Instance.PlanesPlayers.IndexOf(this);
            SetName();
            Debug.Log($"Setting {name} index to : " + playerIndex);
        }
    }

    void SetName()
    {
        Nome = "P " + (playerIndex + 1).ToString();
        //HitSlapRazboi.instance.PlayerNames.Add(Nome);
        Debug.Log($"Setting {name} name to : " + Nome);
    }

    [Command]
    public void ChangeBoardSize(int size)
    {
        ServerActions.Instance.BoardSize = size;
    }

    [ClientRpc]
    public void SetClientBoard(int size)
    {
        LocalPlayerActions.Instance.SetBackPanelToGridSize(size);
    }


    [Command]
    public async void GiveEveryonePowerups(PowerUp powerup)
    {
        Debug.Log($"Give Everyone Powerups : {powerup.name}");
        for(int i =0;i<5;i++)
        {
            ServerActions.Instance.PlayersList[i].CurrentHeldPowerup = powerup;

            if (ServerActions.Instance.PlayersList[i].CurrentHeldPowerup.IsRetroactive)
            {
                await ServerActions.Instance.PlayersList[i].CurrentHeldPowerup.OnUse(i,0, i);
                ServerActions.Instance.PlayersList[i].CurrentHeldPowerup = null;
            }
        }
    }

    //Visually update card images
    [Command]
    public void SendRequestToUpdateCards(int[] numbers)
    {

    }
    [ClientRpc]
    public void UpdateCardImagesToAllPlayers(int[] numbers)
    {

    }


}
