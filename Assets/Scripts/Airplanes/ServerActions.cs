using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;

[System.Serializable]
public class PlayerShipStructure
{
    public CoordsStructure PlayerShipCenter = new CoordsStructure();
    public CoordsStructure PlayerShipHead = new CoordsStructure();
    public PowerUp CurrentHeldPowerup;
    public float PowerupPity = 0;
    public int PowerupSlotIndex = 0;
    public int Health = 8;
    public string Orientation = "";
    public bool isDestroyed = false;
    public bool isShielded = false;
    public bool Misfire = false;
    public bool ReceivedCloneBefore = false;
    public bool Disconnected = false;
    public override string ToString()
    {
        string ret;
        ret = $"Coords (head/center): {PlayerShipHead.ToString()};  {PlayerShipHead.ToString()} \nHealth :{Health}; Orientation :{Orientation}; \nIsDestroyed :{isDestroyed}; Disconnected :{Disconnected} \nPowerUpPity :{PowerupPity}";
        return ret;
    }
}
public class ServerActions : NetworkBehaviour
{
    [SyncVar] public int activeShipsCount;
    public static ServerActions Instance;

    public SyncList<PlanesPlayer> PlanesPlayers = new SyncList<PlanesPlayer>();
    public List<PlayerShipStructure> PlayersList = new List<PlayerShipStructure>();


    [SerializeField] Tilemap map;
   
    public GridBaseStructure ServerVisibleGrid;
    public List<string> AvailableBuildSpaces = new List<string>();
    //public List<string> AvailableTreasureSpaces = new List<string>();
    public List<PowerUp> BasePowerupsList = new List<PowerUp>();
    public List<ToPlayersPowerUp> ActivePowerupsList = new List<ToPlayersPowerUp>();
   

    public Tile[] Tiles = new Tile[10];

    [SyncVar] public int CurrentPlayerTurn = 0;

    [HideInInspector][SyncVar] public bool SetupInProgress = true;
    public int BoardSize = 21;
    [Range(0.85f, 3f)]
    public float PowerupsDensity = 1;
    private int tileTypeBeforeUpdate = 0;
    private int playerIndexBeforeUpdate = 0;
    private bool DebugPlayerIndex = false; //for debug in-editor purposes only. To be removed after finishing up.
    [SyncVar]  public bool HitCalled = false;
    private void Awake()
    {
        Instance = this;
    }

    // tile 0 = normal water
    // tile 1 = undiscovered treasure                                                                                        //deprecated, not used anymore.
    // tile 2 = destroyed player
    // tile 3 = missed tile
    // tile 4 = success hit tile
    // tile 5 = player 0
    // tile 6 = player 1
    // tile 7 = player 2
    // tile 8 = player 3
    // tile 9 = player 4
    // tile 10 = fake player

    private void Update()
    {
        if (!Application.isBatchMode) return;
        if (SetupInProgress) return;
        PlanesPlayers[0].UPDATESTUFF(PlayersList);
    }
    //[TargetRpc]
    public async Task HitCalledOnTileLocation(Vector3Int targetedTile)
    {
        Debug.Log(targetedTile);
        //LocalPlayerActions.Instance.PlayerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = TValue;
        if (tileTypeBeforeUpdate < 1 || tileTypeBeforeUpdate > 4)
        {
            if (UnityEngine.Random.Range(0, 1164) > 999 / PowerupsDensity + PlayersList[playerIndexBeforeUpdate].PowerupPity)
            {               
                List<ToPlayersPowerUp> WeightedPowerUpChoice = new List<ToPlayersPowerUp>();
                int RollWeight = UnityEngine.Random.Range(1, 100);
                for (int i = 0; i < ActivePowerupsList.Count; i++)
                {
                    if (ActivePowerupsList[i].AvailableQuanity > 0 && RollWeight >= ActivePowerupsList[i].PowerUp.PowerUpRequiredWeight)
                    {
                        WeightedPowerUpChoice.Add(ActivePowerupsList[i]);
                    }
                }
                if (WeightedPowerUpChoice.Count > 0)
                {

                    RollWeight = UnityEngine.Random.Range(0, WeightedPowerUpChoice.Count - 1);

                    if ((int)WeightedPowerUpChoice[RollWeight].PowerUp.PowerUpType == 2 && PlayersList[playerIndexBeforeUpdate].ReceivedCloneBefore)
                    {
                        PlanesPlayers[playerIndexBeforeUpdate].ReturnDebugToServer($"Player {playerIndexBeforeUpdate} has already received a 'Powerup.Type(Clone). Trying to give another powerup.");

                        WeightedPowerUpChoice.Remove(WeightedPowerUpChoice[RollWeight]);

                        //If there's anything remaining in weighted powerup choice, roll for it and continue normally, else cancel operation
                        if (WeightedPowerUpChoice.Count > 0)
                        {
                            RollWeight = UnityEngine.Random.Range(0, WeightedPowerUpChoice.Count - 1);

                            //here sequence
                            GivePowerupSequence(WeightedPowerUpChoice, RollWeight);
                        }
                        else
                        {
                            PlanesPlayers[playerIndexBeforeUpdate].ReturnDebugToServer($"No powerup left available for rerolling, canceling operation.");
                        }
                    }
                    else
                    {
                        //here sequence
                        GivePowerupSequence(WeightedPowerUpChoice, RollWeight);
                    }


                    //checkIfPowerupIsPassive
                    /*if(PlayersList[playerIndexBeforeUpdate].CurrentHeldPowerup.IsRetroactive)
                    {
                        await PlayersList[CurrentPlayerTurn].CurrentHeldPowerup.OnUse(playerIndexBeforeUpdate);
                        PlayersList[CurrentPlayerTurn].CurrentHeldPowerup = null;
                    }*/
                }
                else
                {
                    Debug.Log("No Powers Selected by weight");
                }
                WeightedPowerUpChoice.Clear();
            }
            else
            {
                PlayersList[playerIndexBeforeUpdate].PowerupPity += 0.10f;
            }
        }
        PlanesPlayers[playerIndexBeforeUpdate].UpdateTile(targetedTile, ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y]);        
        //LocalPlayerActions.Instance.PlayingField.SetTile(targetedTile, Tiles[ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y]]);
        HitCalled = false;
        await Task.Yield();
    }

    private void GivePowerupSequence(List<ToPlayersPowerUp> WeightedPowerUpChoice,int RollWeight)
    {
        PlanesPlayers[playerIndexBeforeUpdate].givePlayerPowerup(WeightedPowerUpChoice[RollWeight].PowerUp, playerIndexBeforeUpdate);

        //PlayersList[playerIndexBeforeUpdate].CurrentHeldPowerup = WeightedPowerUpChoice[RollWeight].PowerUp;

        PlayersList[playerIndexBeforeUpdate].PowerupPity = 0;
        ActivePowerupsList[ActivePowerupsList.IndexOf(WeightedPowerUpChoice[RollWeight])].AvailableQuanity--;

        if ((int)WeightedPowerUpChoice[RollWeight].PowerUp.PowerUpType == 2)
        {
            PlayersList[playerIndexBeforeUpdate].ReceivedCloneBefore = true;
        }

        Debug.Log($"Player Index {playerIndexBeforeUpdate} got power-up {WeightedPowerUpChoice[RollWeight].PowerUp.ToString()}");
    }
    /*public bool RequestPowerupInformation()
    {
        if(PlayersList[CurrentPlayerTurn].CurrentHeldPowerup != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }*/
    public async Task VerifyAndUpdateTile(Vector3Int targetedTile)
    {
        if(ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] == CurrentPlayerTurn + 5)
        {
            Debug.Log("PreventHittingOwnShip");
            return;
        }
        string DebugStatement = $"Tile@Location:{targetedTile.x},{targetedTile.y}|";
        tileTypeBeforeUpdate = ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y];
        playerIndexBeforeUpdate = CurrentPlayerTurn;
        switch (tileTypeBeforeUpdate)
        {

            //Water To Miss
            case 0:
                {
                    DebugStatement += "TileType Water To Miss"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 3;
                    await IncreaseTurn();
                    break;
                }

            /*//Undiscovered Treasure To Discovered Treasure //rewardItemToPlayer //deprecated
            case 1:
                {
                    DebugStatement += "TileType Treasure To DTreasure"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 2;
                    CurrentPlayerTurn++; CurrentPlayerTurn = CurrentPlayerTurn % 5;
                    break;
                }*/

            //Destroyed To Destroyed
            case 2:
                {
                    DebugStatement += "TileType Destroyed To Destroyed"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 2;
                    await IncreaseTurn();
                    break;
                }

            //Miss To Miss
            case 3:
                {
                    DebugStatement += "TileType Miss To Miss"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 3;
                    await IncreaseTurn();
                    break;
                }

            //Success Hit to Success Hit
            case 4:
                {
                    DebugStatement += "TileType Success To Success"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 4;
                    await IncreaseTurn();
                    break;
                }

            //Player 0 To Success Hit //DamagePlayer0 //InstakillPlayer0 If targetedTile = head position
            case 5:
                {
                    if (PlayersList[0].isShielded)
                    {
                        PlayersList[0].isShielded = false;
                        do
                        {
                            targetedTile.x += UnityEngine.Random.Range(-1, 1);
                            targetedTile.y += UnityEngine.Random.Range(-1, 1);
                        }
                        while (ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] == 5);
                        DebugStatement += "(Shielded) TileType Player0 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 3;
                        return;
                    }
                    DebugStatement += "TileType Player0 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 4;
                    PlayersList[0].Health--;
                    try { PlanesPlayers[0].UpdateTile(new Vector3Int(targetedTile.x, targetedTile.y), 4); }
                    catch { Debug.Log("Player not existent"); }
                    if (PlayersList[0].Health < 1 || (targetedTile.x == PlayersList[0].PlayerShipHead.X && targetedTile.y == PlayersList[0].PlayerShipHead.Y))
                    {
                        PlayersList[0].isDestroyed = true;
                        playerShipDestroy(0);
                    }
                    
                    break;
                }

            //Player 1 To Success Hit //DamagePlayer1 //InstakillPlayer1 If targetedTile = head position
            case 6:
                {
                    if (PlayersList[1].isShielded)
                    {
                        PlayersList[1].isShielded = false;
                        do
                        {
                            targetedTile.x += UnityEngine.Random.Range(-1, 1);
                            targetedTile.y += UnityEngine.Random.Range(-1, 1);
                        }
                        while (ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] == 6);
                        DebugStatement += "(Shielded) TileType Player1 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 3;
                        return;
                    }
                    DebugStatement += "TileType Player1 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 4;
                    PlayersList[1].Health--;
                    try { PlanesPlayers[1].UpdateTile(new Vector3Int(targetedTile.x, targetedTile.y), 4); }
                    catch { Debug.Log("Player not existent"); }
                    if (PlayersList[1].Health < 1 || (targetedTile.x == PlayersList[1].PlayerShipHead.X && targetedTile.y == PlayersList[1].PlayerShipHead.Y))
                    {
                        PlayersList[1].isDestroyed = true;
                        playerShipDestroy(1);
                    }
                    break;
                }

            //Player 2 To Success Hit //DamagePlayer2 //InstakillPlayer2 If targetedTile = head position
            case 7:
                {
                    if (PlayersList[2].isShielded)
                    {
                        PlayersList[2].isShielded = false;
                        do
                        {
                            targetedTile.x += UnityEngine.Random.Range(-1, 1);
                            targetedTile.y += UnityEngine.Random.Range(-1, 1);
                        }
                        while (ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] == 7);
                        DebugStatement += "(Shielded) TileType Player2 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 3;
                        return;
                    }
                    DebugStatement += "TileType Player2 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 4;
                    PlayersList[2].Health--;
                    try { PlanesPlayers[2].UpdateTile(new Vector3Int(targetedTile.x, targetedTile.y), 4); }
                    catch { Debug.Log("Player not existent"); }
                    if (PlayersList[2].Health < 1 || (targetedTile.x == PlayersList[2].PlayerShipHead.X && targetedTile.y == PlayersList[2].PlayerShipHead.Y))
                    {
                        PlayersList[2].isDestroyed = true;
                        playerShipDestroy(2);
                    }
                    break;
                }

            //Player 3 To Success Hit //DamagePlayer3 //InstakillPlayer3 If targetedTile = head position
            case 8:
                {
                    if (PlayersList[3].isShielded)
                    {
                        PlayersList[3].isShielded = false;
                        do
                        {
                            targetedTile.x += UnityEngine.Random.Range(-1, 1);
                            targetedTile.y += UnityEngine.Random.Range(-1, 1);
                        }
                        while (ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] == 8);
                        DebugStatement += "(Shielded) TileType Player3 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 3;
                        return;
                    }
                    DebugStatement += "TileType Player3 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 4;
                    PlayersList[3].Health--;
                    try { PlanesPlayers[3].UpdateTile(new Vector3Int(targetedTile.x, targetedTile.y), 4); }
                    catch { Debug.Log("Player not existent"); }
                    if (PlayersList[3].Health < 1 || (targetedTile.x == PlayersList[3].PlayerShipHead.X && targetedTile.y == PlayersList[3].PlayerShipHead.Y))
                    {
                        PlayersList[3].isDestroyed = true;
                        playerShipDestroy(3);
                    }
                    break;
                }

            //Player 4 To Success Hit //DamagePlayer4 //InstakillPlayer4 If targetedTile = head position
            case 9:
                {
                    if (PlayersList[4].isShielded)
                    {
                        PlayersList[4].isShielded = false;
                        do
                        {
                            targetedTile.x += UnityEngine.Random.Range(-1, 1);
                            targetedTile.y += UnityEngine.Random.Range(-1, 1);
                        }
                        while (ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] == 9);
                        DebugStatement += "(Shielded) TileType Player4 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 3;
                        return;
                    }
                    DebugStatement += "TileType Player4 To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 4;
                    PlayersList[4].Health--;
                    try { PlanesPlayers[4].UpdateTile(new Vector3Int(targetedTile.x, targetedTile.y), 4); }
                    catch { Debug.Log("Player not existent"); }
                    if (PlayersList[4].Health < 1 || (targetedTile.x == PlayersList[4].PlayerShipHead.X && targetedTile.y == PlayersList[4].PlayerShipHead.Y))
                    {
                        PlayersList[4].isDestroyed = true;
                        playerShipDestroy(4);
                    }
                    break;
                }
            case 10:
                {
                    DebugStatement += "TileType FakePlayer To Suceess"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 4;
                    break;
                }
            //Destroy To Destroy
            //case 10: { DebugStatement += "TileType Destroyed To Destroyed"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 10; break; }
            default: break;
        }

        if(DebugPlayerIndex)
        {
            LocalPlayerActions.Instance.LocalPlayerIndex = CurrentPlayerTurn;
        }
        //Debug.Log(DebugStatement);
        await Task.Yield();
    }
    //[TargetRpc]
    public async Task PatternCalledOnTileLocation(Vector3Int[] targetedTiles)
    {
        for (int x = 0; x < targetedTiles.Length; x++)
        {
            if (map.GetTile(targetedTiles[x]) == null)
            {
                Debug.Log("PreventHitsOutsideMap");
                continue;
            }
            //LocalPlayerActions.Instance.PlayerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = TValue;
            //LocalPlayerActions.Instance.PlayingField.SetTile(targetedTiles[x], Tiles[ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y]]);
            PlanesPlayers[playerIndexBeforeUpdate].UpdateTile(targetedTiles[x], ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y]);
        }
        HitCalled = false;
        await Task.Yield();
    }
    public async Task VerifyAndUpdatePattern(Vector3Int[] targetedTiles)
    {
        Debug.Log("attempt pattern");
        bool hitPlayer = false;
        bool[] shieldbreak = { false, false, false, false, false };
        for (int x = 0; x < targetedTiles.Length; x++)
        {
            if (map.GetTile(targetedTiles[x]) == null)
            {
                Debug.Log("PreventHitsOutsideMap");
                continue;
            }
            if (ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] == CurrentPlayerTurn + 5)
            {
                Debug.Log("PreventHittingOwnShip");
                continue;
            }
            string DebugStatement = $"Tile@Location:{targetedTiles[x].x},{targetedTiles[x].y}|";
            tileTypeBeforeUpdate = ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y];
            playerIndexBeforeUpdate = CurrentPlayerTurn;
            switch (tileTypeBeforeUpdate)
            {

                //Water To Miss
                case 0:
                    {
                        DebugStatement += "TileType Water To Miss"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 3;
                        break;
                    }

                /*//Undiscovered Treasure To Discovered Treasure //rewardItemToPlayer //deprecated
                case 1:
                    {
                        DebugStatement += "TileType Treasure To DTreasure"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 2;
                        CurrentPlayerTurn++; CurrentPlayerTurn = CurrentPlayerTurn % 5;
                        break;
                    }*/

                //Destroyed To Destroyed
                case 2:
                    {
                        DebugStatement += "TileType Destroyed To Destroyed"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 2;
                        break;
                    }

                //Miss To Miss
                case 3:
                    {
                        DebugStatement += "TileType Miss To Miss"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 3;
                        break;
                    }

                //Success Hit to Success Hit
                case 4:
                    {
                        DebugStatement += "TileType Success To Success"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 4;
                        break;
                    }

                //Player 0 To Success Hit //DamagePlayer0 //InstakillPlayer0 If targetedTile = head position
                case 5:
                    {
                        if (PlayersList[0].isShielded)
                        {
                            shieldbreak[0] = true;
                            do
                            {
                                targetedTiles[x].x += UnityEngine.Random.Range(-1, 1);
                                targetedTiles[x].y += UnityEngine.Random.Range(-1, 1);
                            }
                            while (ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] == 5);

                            DebugStatement += "(Shielded) TileType Player0 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 3;
                            continue;
                        }
                        if (shieldbreak[0])
                        {
                            break;
                        }
                        hitPlayer = true;
                        DebugStatement += "TileType Player0 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 4;
                        PlayersList[0].Health--;

                        try { PlanesPlayers[0].UpdateTile(new Vector3Int(targetedTiles[x].x, targetedTiles[x].y), 4); }
                        catch { Debug.Log("Player not existent"); }

                        if (PlayersList[0].Health < 1 || (targetedTiles[x].x == PlayersList[0].PlayerShipHead.X && targetedTiles[x].y == PlayersList[0].PlayerShipHead.Y))
                        {
                            PlayersList[0].isDestroyed = true;
                            playerShipDestroy(0);
                        }
                        break;
                    }

                //Player 1 To Success Hit //DamagePlayer1 //InstakillPlayer1 If targetedTile = head position
                case 6:
                    {
                        if (PlayersList[1].isShielded)
                        {
                            shieldbreak[1] = true;
                            do
                            {
                                targetedTiles[x].x += UnityEngine.Random.Range(-1, 1);
                                targetedTiles[x].y += UnityEngine.Random.Range(-1, 1);
                            }
                            while (ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] == 6);
                            DebugStatement += "(Shielded) TileType Player0 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 3;
                            continue;
                        }
                        if (shieldbreak[1])
                        {
                            break;
                        }
                        hitPlayer = true;
                        DebugStatement += "TileType Player1 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 4;
                        PlayersList[1].Health--;
                        try { PlanesPlayers[1].UpdateTile(new Vector3Int(targetedTiles[x].x, targetedTiles[x].y), 4); }
                        catch { Debug.Log("Player not existent"); }
                        if (PlayersList[1].Health < 1 || (targetedTiles[x].x == PlayersList[1].PlayerShipHead.X && targetedTiles[x].y == PlayersList[1].PlayerShipHead.Y))
                        {
                            PlayersList[1].isDestroyed = true;
                            playerShipDestroy(1);
                        }
                        break;
                    }

                //Player 2 To Success Hit //DamagePlayer2 //InstakillPlayer2 If targetedTile = head position
                case 7:
                    {
                        if (PlayersList[2].isShielded)
                        {
                            shieldbreak[2] = true;
                            do
                            {
                                targetedTiles[x].x += UnityEngine.Random.Range(-1, 1);
                                targetedTiles[x].y += UnityEngine.Random.Range(-1, 1);
                            }
                            while (ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] == 7);
                            DebugStatement += "(Shielded) TileType Player0 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 3;
                            continue;
                        }
                        if (shieldbreak[2])
                        {
                            break;
                        }
                        hitPlayer = true;
                        DebugStatement += "TileType Player2 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 4;
                        PlayersList[2].Health--;
                        try { PlanesPlayers[2].UpdateTile(new Vector3Int(targetedTiles[x].x, targetedTiles[x].y), 4); }
                        catch { Debug.Log("Player not existent"); }
                        if (PlayersList[2].Health < 1 || (targetedTiles[x].x == PlayersList[2].PlayerShipHead.X && targetedTiles[x].y == PlayersList[2].PlayerShipHead.Y))
                        {
                            PlayersList[2].isDestroyed = true;
                            playerShipDestroy(2);
                        }
                        break;
                    }

                //Player 3 To Success Hit //DamagePlayer3 //InstakillPlayer3 If targetedTile = head position
                case 8:
                    {
                        if (PlayersList[3].isShielded)
                        {
                            shieldbreak[3] = true;
                            do
                            {
                                targetedTiles[x].x += UnityEngine.Random.Range(-1, 1);
                                targetedTiles[x].y += UnityEngine.Random.Range(-1, 1);
                            }
                            while (ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] == 8);
                            DebugStatement += "(Shielded) TileType Player0 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 3;
                            continue;
                        }
                        if (shieldbreak[3])
                        {
                            break;
                        }
                        hitPlayer = true;
                        DebugStatement += "TileType Player3 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 4;
                        PlayersList[3].Health--;
                        try { PlanesPlayers[3].UpdateTile(new Vector3Int(targetedTiles[x].x, targetedTiles[x].y), 4); }
                        catch { Debug.Log("Player not existent"); }
                        if (PlayersList[3].Health < 1 || (targetedTiles[x].x == PlayersList[3].PlayerShipHead.X && targetedTiles[x].y == PlayersList[3].PlayerShipHead.Y))
                        {
                            PlayersList[3].isDestroyed = true;
                            playerShipDestroy(3);
                        }
                        break;
                    }

                //Player 4 To Success Hit //DamagePlayer4 //InstakillPlayer4 If targetedTile = head position
                case 9:
                    {
                        if (PlayersList[4].isShielded)
                        {
                            shieldbreak[4] = true;
                            do
                            {
                                targetedTiles[x].x += UnityEngine.Random.Range(-1, 1);
                                targetedTiles[x].y += UnityEngine.Random.Range(-1, 1);
                            }
                            while (ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] == 9);
                            DebugStatement += "(Shielded) TileType Player0 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 3;
                            continue;
                        }
                        if(shieldbreak[4])
                        {
                            break;
                        }
                        hitPlayer = true;
                        DebugStatement += "TileType Player4 To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 4;
                        PlayersList[4].Health--;
                        try { PlanesPlayers[4].UpdateTile(new Vector3Int(targetedTiles[x].x, targetedTiles[x].y), 4); }
                        catch { Debug.Log("Player not existent"); }
                        if (PlayersList[4].Health < 1 || (targetedTiles[x].x == PlayersList[4].PlayerShipHead.X && targetedTiles[x].y == PlayersList[4].PlayerShipHead.Y))
                        {
                            PlayersList[4].isDestroyed = true;
                            playerShipDestroy(4);
                        }
                        break;
                    }
                case 10:
                    {
                        DebugStatement += "TileType FakePlayer To Suceess"; ServerVisibleGrid.Row[targetedTiles[x].x].Column[targetedTiles[x].y] = 4;
                        break;
                    }
                //Destroy To Destroy
                //case 10: { DebugStatement += "TileType Destroyed To Destroyed"; ServerVisibleGrid.Row[targetedTile.x].Column[targetedTile.y] = 10; break; }
                default: break;
            }
        }
        PlayersList[CurrentPlayerTurn].CurrentHeldPowerup = null;
        for (int i = 0; i < 5; i++)
        {
            if(shieldbreak[i])
            {
                PlayersList[i].isShielded = false;
            }
        }
        if (!hitPlayer)
        {
            await IncreaseTurn();
        }
        if (DebugPlayerIndex)
        {
            LocalPlayerActions.Instance.LocalPlayerIndex = CurrentPlayerTurn;
        }
        await Task.Yield();
    }
    public void playerShipDestroy(int switchTarget)
    {
        PlayersList[switchTarget].isDestroyed = true;
        //CenterOfShip
        int CenterX = PlayersList[switchTarget].PlayerShipCenter.X;
        int CenterY = PlayersList[switchTarget].PlayerShipCenter.Y;

        ServerVisibleGrid.Row[CenterX].Column[CenterY] = 2;

        switch (PlayersList[switchTarget].Orientation)
        {
            //North
            case "North":
                {
                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 2;
                    //HEAD
                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 2;
                    //
                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 2;
                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 2;
                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 2;
                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 2;
                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 2;
                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 2] = 2;
                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 2] = 2;
                    break;
                }
            //South
            case "South":
                {
                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 2;
                    //HEAD
                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 2;
                    //
                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 2;
                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 2;
                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 2;
                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 2;
                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 2;
                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 2] = 2;
                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 2] = 2;
                    break;
                }
            //West
            case "West":
                {
                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 2;
                    //HEAD
                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 2;
                    //
                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 2;
                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 2;
                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 2;
                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 2;
                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 2;
                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY - 1] = 2;
                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY + 1] = 2;
                    break;
                }
            //East
            case "East":
                {
                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 2;
                    //HEAD
                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 2;
                    //
                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 2;
                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 2;
                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 2;
                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 2;
                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 2;
                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY + 1] = 2;
                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY - 1] = 2;
                    break;
                }
        }

        //Reveal The rest of the destroyed ship
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (ServerVisibleGrid.Row[i].Column[j] == 2)
                {
                    foreach(PlanesPlayer _player in PlanesPlayers)
                    {
                        try { _player.UpdateTile(new Vector3Int(i, j), 2); }
                        catch { Debug.Log("player not present"); }                        
                    }                  
                    
                    //LocalPlayerActions.Instance.PlayingField.SetTile(new Vector3Int(i, j), Tiles[2]);
                }

            }
        }

        activeShipsCount--;
        Debug.Log($"{activeShipsCount} ships Left");
        if (activeShipsCount <= 1)
            FinishGame();

    }
    #region GameSetupPhase
    //DrawBoardForServer
    public async void SetupBoard()
    {
        PlanesPlayers[0].SetClientBoard(BoardSize);

        if (!SetupInProgress)
            return;

        await FillMapWithWater();
        await BuildEmptyAreaArray();
        await AttemptToArrangePlayers(false, 0);
        await BuildPowerUpsPool();
        //Powerups are now handled by Hitting tiles
        //await AddPowerUps();
        await DisplayIndividualShips();
        //await ShowTotalMap();
        SetupInProgress = false;
        PlanesPlayers[0].SetUpProgress(false);
        
        activeShipsCount = 5;
    }
    private async Task FillMapWithWater()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                //LocalPlayerActions.Instance.PlayingField.SetTile(new Vector3Int(i, j), Tiles[0]);
                foreach(PlanesPlayer _player in PlanesPlayers)
                {
                    try { _player.UpdateTile(new Vector3Int(i, j), 0); }
                    catch { Debug.Log("player not present"); }
                }
                map.SetTile(new Vector3Int(i, j), Tiles[0]);
                ServerVisibleGrid.Row[i].Column[j] = 0;
            }
        }
        await Task.Yield();
    }
    private async Task BuildEmptyAreaArray()
    {
        for(int i=2;i<BoardSize - 2;i++)
        {
            for (int j = 2; j < BoardSize - 2; j++)
            {
                if (i < 10 && j < 10)
                {
                    AvailableBuildSpaces.Add($"0{i}0{j}");
                }
                if (i > 10 && j < 10)
                {
                    AvailableBuildSpaces.Add($"{i}0{j}");
                }
                if (i < 10 && j > 10)
                {
                    AvailableBuildSpaces.Add($"0{i}{j}");
                }
                if (i > 10 && j > 10)
                {
                    AvailableBuildSpaces.Add($"{i}{j}");
                }
            }
        }
        //Available treasure spaces - deprecated in favor of new treasure sys
        /*
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (i < 10 && j < 10)
                {
                    AvailableTreasureSpaces.Add($"0{i}0{j}");
                }
                if (i > 10 && j < 10)
                {
                    AvailableTreasureSpaces.Add($"{i}0{j}");
                }
                if (i < 10 && j > 10)
                {
                    AvailableTreasureSpaces.Add($"0{i}{j}");
                }
                if (i > 10 && j > 10)
                {
                    AvailableTreasureSpaces.Add($"{i}{j}");
                }
            }
        }*/
        await Task.Yield();
    }    
    public async Task AttemptToArrangePlayers(bool fake, int indexFake)
    {
        int localRandomIndex;
        int orientation;
        int CenterX;
        int CenterY;

        for (int i = 0; i < 5; i++)
        {
            if (fake)
            {
                i = 5;
            }
            else
            {
                PlayersList.Add(new PlayerShipStructure());
            }
            localRandomIndex = UnityEngine.Random.Range(0, AvailableBuildSpaces.Count - 1);
            orientation = UnityEngine.Random.Range(0, 39) / 10;

            CenterX = short.Parse(AvailableBuildSpaces[localRandomIndex].Substring(0, 2));
            CenterY = short.Parse(AvailableBuildSpaces[localRandomIndex].Substring(2, 2));
            //For your own sake, do not open this switch region
            #region SwitchSwitchSwitchSwitchSwitchSwitch
            switch (i + 5)
            {
                case 5:
                    {
                        //CenterOfShip
                        PlayersList[i].PlayerShipCenter.X = CenterX;
                        PlayersList[i].PlayerShipCenter.Y = CenterY;
                        ServerVisibleGrid.Row[CenterX].Column[CenterY] = 5;

                        //BodyOfShip
                        switch (orientation)
                        {
                            //North
                            case 0:
                                {
                                    PlayersList[i].Orientation = "North";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 5;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 5;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY + 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 5;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 5;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 5;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 5;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 5;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 2] = 5;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 2] = 5;
                                    break;
                                }
                            //South
                            case 1:
                                {
                                    PlayersList[i].Orientation = "South";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 5;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 5;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY - 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 5;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 5;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 5;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 5;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 5;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 2] = 5;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 2] = 5;
                                    break;
                                }
                            //West
                            case 2:
                                {
                                    PlayersList[i].Orientation = "West";
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 5;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 5;
                                    PlayersList[i].PlayerShipHead.X = CenterX - 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 5;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 5;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 5;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 5;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 5;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY - 1] = 5;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY + 1] = 5;
                                    break;
                                }
                            //East
                            case 3:
                                {
                                    PlayersList[i].Orientation = "East";
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 5;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 5;
                                    PlayersList[i].PlayerShipHead.X = CenterX + 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 5;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 5;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 5;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 5;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 5;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY + 1] = 5;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY - 1] = 5;
                                    break;
                                }
                        }
                        break; }
                case 6:
                    {
                        //CenterOfShip
                        PlayersList[i].PlayerShipCenter.X = CenterX;
                        PlayersList[i].PlayerShipCenter.Y = CenterY;
                        ServerVisibleGrid.Row[CenterX].Column[CenterY] = 6;

                        //BodyOfShip
                        switch (orientation)
                        {
                            //North
                            case 0:
                                {
                                    PlayersList[i].Orientation = "North";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 6;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 6;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY + 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 6;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 6;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 6;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 6;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 6;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 2] = 6;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 2] = 6;
                                    break;
                                }
                            //South
                            case 1:
                                {
                                    PlayersList[i].Orientation = "South";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 6;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 6;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY - 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 6;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 6;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 6;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 6;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 6;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 2] = 6;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 2] = 6;
                                    break;
                                }
                            //West
                            case 2:
                                {
                                    PlayersList[i].Orientation = "West";
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 6;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 6;
                                    PlayersList[i].PlayerShipHead.X = CenterX - 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 6;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 6;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 6;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 6;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 6;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY - 1] = 6;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY + 1] = 6;
                                    break;
                                }
                            //East
                            case 3:
                                {
                                    PlayersList[i].Orientation = "East";
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 6;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 6;
                                    PlayersList[i].PlayerShipHead.X = CenterX + 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 6;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 6;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 6;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 6;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 6;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY + 1] = 6;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY - 1] = 6;
                                    break;
                                }
                        }
                        break;
                    }
                case 7:
                    {
                        //CenterOfShip
                        PlayersList[i].PlayerShipCenter.X = CenterX;
                        PlayersList[i].PlayerShipCenter.Y = CenterY;
                        ServerVisibleGrid.Row[CenterX].Column[CenterY] = 7;

                        //BodyOfShip
                        switch (orientation)
                        {
                            //North
                            case 0:
                                {
                                    PlayersList[i].Orientation = "North";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 7;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 7;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY + 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 7;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 7;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 7;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 7;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 7;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 2] = 7;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 2] = 7;
                                    break;
                                }
                            //South
                            case 1:
                                {
                                    PlayersList[i].Orientation = "South";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 7;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 7;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY - 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 7;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 7;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 7;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 7;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 7;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 2] = 7;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 2] = 7;
                                    break;
                                }
                            //West
                            case 2:
                                {
                                    PlayersList[i].Orientation = "West";
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 7;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 7;
                                    PlayersList[i].PlayerShipHead.X = CenterX - 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 7;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 7;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 7;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 7;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 7;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY - 1] = 7;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY + 1] = 7;
                                    break;
                                }
                            //East
                            case 3:
                                {
                                    PlayersList[i].Orientation = "East";
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 7;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 7;
                                    PlayersList[i].PlayerShipHead.X = CenterX + 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 7;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 7;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 7;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 7;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 7;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY + 1] = 7;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY - 1] = 7;
                                    break;
                                }
                        }
                        break;
                    }
                case 8:
                    {
                        //CenterOfShip
                        PlayersList[i].PlayerShipCenter.X = CenterX;
                        PlayersList[i].PlayerShipCenter.Y = CenterY;
                        ServerVisibleGrid.Row[CenterX].Column[CenterY] = 8;

                        //BodyOfShip
                        switch (orientation)
                        {
                            //North
                            case 0:
                                {
                                    PlayersList[i].Orientation = "North";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 8;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 8;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY + 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 8;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 8;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 8;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 8;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 8;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 2] = 8;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 2] = 8;
                                    break;
                                }
                            //South
                            case 1:
                                {
                                    PlayersList[i].Orientation = "South";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 8;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 8;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY - 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 8;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 8;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 8;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 8;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 8;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 2] = 8;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 2] = 8;
                                    break;
                                }
                            //West
                            case 2:
                                {
                                    PlayersList[i].Orientation = "West";
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 8;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 8;
                                    PlayersList[i].PlayerShipHead.X = CenterX - 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 8;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 8;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 8;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 8;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 8;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY - 1] = 8;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY + 1] = 8;
                                    break;
                                }
                            //East
                            case 3:
                                {
                                    PlayersList[i].Orientation = "East";
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 8;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 8;
                                    PlayersList[i].PlayerShipHead.X = CenterX + 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 8;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 8;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 8;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 8;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 8;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY + 1] = 8;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY - 1] = 8;
                                    break;
                                }
                        }
                        break;
                    }
                case 9:
                    {
                        //CenterOfShip
                        PlayersList[i].PlayerShipCenter.X = CenterX;
                        PlayersList[i].PlayerShipCenter.Y = CenterY;
                        ServerVisibleGrid.Row[CenterX].Column[CenterY] = 9;

                        //BodyOfShip
                        switch (orientation)
                        {
                            //North
                            case 0:
                                {
                                    PlayersList[i].Orientation = "North";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 9;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 9;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY + 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 9;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 9;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 9;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 9;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 9;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 2] = 9;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 2] = 9;
                                    break;
                                }
                            //South
                            case 1:
                                {
                                    PlayersList[i].Orientation = "South";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 9;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 9;
                                    PlayersList[i].PlayerShipHead.X = CenterX;
                                    PlayersList[i].PlayerShipHead.Y = CenterY - 2;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 9;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 9;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 9;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 9;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 9;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 2] = 9;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 2] = 9;
                                    break;
                                }
                            //West
                            case 2:
                                {
                                    PlayersList[i].Orientation = "West";
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 9;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 9;
                                    PlayersList[i].PlayerShipHead.X = CenterX - 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 9;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 9;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 9;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 9;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 9;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY - 1] = 9;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY + 1] = 9;
                                    break;
                                }
                            //East
                            case 3:
                                {
                                    PlayersList[i].Orientation = "East";
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 9;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 9;
                                    PlayersList[i].PlayerShipHead.X = CenterX + 2;
                                    PlayersList[i].PlayerShipHead.Y = CenterY;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 9;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 9;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 9;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 9;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 9;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY + 1] = 9;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY - 1] = 9;
                                    break;
                                }
                        }
                        break;
                    }
                case 10:
                    {
                        //CenterOfShip
                        ServerVisibleGrid.Row[CenterX].Column[CenterY] = 10;
                        Debug.Log($"Trying to spawn fake news at {CenterX} and {CenterY}");

                        //BodyOfShip
                        switch (orientation)
                        {
                            //North
                            case 0:
                                {
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 10;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 10;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 10;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 10;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 10;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 10;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 10;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 2] = 10;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 2] = 10;
                                    break;
                                }
                            //South
                            case 1:
                                {
                                   // PlayersList[i].Orientation = "South";
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 1] = 10;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 10;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 10;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 10;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 10;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 10;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 1] = 10;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 2] = 10;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 2] = 10;
                                    break;
                                }
                            //West
                            case 2:
                                {
                                    //PlayersList[i].Orientation = "West";
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 10;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY] = 10;
                                    //
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY - 1] = 10;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY + 1] = 10;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 10;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 10;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 10;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY - 1] = 10;
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY + 1] = 10;
                                    break;
                                }
                            //East
                            case 3:
                                {
                                    //PlayersList[i].Orientation = "East";
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY] = 10;
                                    //HEAD
                                    ServerVisibleGrid.Row[CenterX + 2].Column[CenterY] = 10;
                                    //
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY + 1] = 10;
                                    ServerVisibleGrid.Row[CenterX + 1].Column[CenterY - 1] = 10;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY + 2] = 10;
                                    ServerVisibleGrid.Row[CenterX].Column[CenterY - 2] = 10;
                                    ServerVisibleGrid.Row[CenterX - 1].Column[CenterY] = 10;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY + 1] = 10;
                                    ServerVisibleGrid.Row[CenterX - 2].Column[CenterY - 1] = 10;
                                    break;
                                }

                        }
                        ShowFakeShip(indexFake);
                        break;
                    }
                default: { Debug.Log("Out of bounds lmao"); break; }
            }
            //UpdateTreasuresSlotsList
            await WorstThingOfMyLife(CenterX,CenterY,orientation);
            
            
            if (fake)
            {
                //Display new ship to one who placed it don't remove build spaces
            }
            else
            {
                //Erase ConsumedSpaces
                switch (CenterX)
                {
                    case 2: break;
                    case 3: CenterX -= 1; break;
                    case 4: CenterX -= 2; break;
                    case 5: CenterX -= 3; break;
                    default: CenterX -= 4; break;
                }
                switch (CenterY)
                {
                    case 2: break;
                    case 3: CenterY -= 1; break;
                    case 4: CenterY -= 2; break;
                    case 5: CenterY -= 3; break;
                    default: CenterY -= 4; break;
                }
                for (int j = 0; j < 9; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (CenterX + j < 10 && CenterY + k < 10)
                        {
                            try
                            {
                                AvailableBuildSpaces.RemoveAt(AvailableBuildSpaces.IndexOf($"0{CenterX + j}0{CenterY + k}"));
                            }
                            catch
                            {
                                //Debug.Log("Trying To Delete Already Deleted Slot / Outside Border Overlap");
                            }
                            continue;
                        }
                        if (CenterX + j > 10 && CenterY + k < 10)
                        {
                            try
                            {
                                AvailableBuildSpaces.RemoveAt(AvailableBuildSpaces.IndexOf($"{CenterX + j}0{CenterY + k}"));
                            }
                            catch
                            {
                                //Debug.Log("Trying To Delete Already Deleted Slot / Outside Border Overlap");
                            }
                            continue;
                        }
                        if (CenterX + j < 10 && CenterY + k > 10)
                        {
                            try
                            {
                                AvailableBuildSpaces.RemoveAt(AvailableBuildSpaces.IndexOf($"0{CenterX + j}{CenterY + k}"));
                            }
                            catch
                            {
                                //Debug.Log("Trying To Delete Already Deleted Slot / Outside Border Overlap");
                            }
                            continue;
                        }
                        if (CenterX + j > 10 && CenterY + k > 10)
                        {
                            try
                            {
                                AvailableBuildSpaces.RemoveAt(AvailableBuildSpaces.IndexOf($"{CenterX + j}{CenterY + k}"));
                            }
                            catch
                            {
                                //Debug.Log("Trying To Delete Already Deleted Slot / Outside Border Overlap");
                            }
                            continue;
                        }
                    }
                }
            }
            #endregion
        }
        await Task.Yield();
    }

    private async Task BuildPowerUpsPool()
    {
        ActivePowerupsList.Clear();
        int randomMemory;
        int targetCount = BasePowerupsList.Count;
        List<PowerUp> TempList = new List<PowerUp>();
        TempList.AddRange(BasePowerupsList);
        for (int i = 0;i<Math.Min(targetCount,5);i++)
        {
            randomMemory = UnityEngine.Random.Range(0, TempList.Count - 1);
            ToPlayersPowerUp tppu = new ToPlayersPowerUp();
            tppu.PowerUp = TempList[randomMemory];
            tppu.AvailableQuanity = TempList[randomMemory].AvailableQuanity;
            ActivePowerupsList.Add(tppu);
            TempList.RemoveAt(randomMemory);
        }
        await Task.Yield();
    }
    /*private async Task AddPowerUps() //deprecated powerups
    {
        int localRandomIndex;
        int CenterX;
        int CenterY;
        for (int i=0;i<(int)(25f * PowerupsDensity);i++)
        {
            localRandomIndex = UnityEngine.Random.Range(0, (AvailableTreasureSpaces.Count - 1));
            CenterX = short.Parse(AvailableTreasureSpaces[localRandomIndex].Substring(0, 2));
            CenterY = short.Parse(AvailableTreasureSpaces[localRandomIndex].Substring(2, 2));

            ServerVisibleGrid.Row[CenterX].Column[CenterY] = 1;
            AvailableTreasureSpaces.RemoveAt(localRandomIndex);
        }    
        await Task.Yield();
    }    */
    
    private async Task WorstThingOfMyLife(int x,int y,int orient)
    {
        List<string> ShipPartsPositions = new List<string>();
        //center
        ShipPartsPositions.Add(await interToStringerLocation(x, y));

        //Body
        switch (orient)
        {
            //North
            case 0:
                {
                    ShipPartsPositions.Add(await interToStringerLocation(x, y + 1));
                    //HEAD
                    ShipPartsPositions.Add(await interToStringerLocation(x, y + 2));
                    //
                    ShipPartsPositions.Add(await interToStringerLocation(x - 1, y + 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 1, y + 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 2, y));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 2, y));
                    ShipPartsPositions.Add(await interToStringerLocation(x, y - 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 1, y - 2));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 1, y - 2));
                    break;
                }
            //South
            case 1:
                {
                    ShipPartsPositions.Add(await interToStringerLocation(x, y - 1));
                    //HEAD
                    ShipPartsPositions.Add(await interToStringerLocation(x, y - 2));
                    //
                    ShipPartsPositions.Add(await interToStringerLocation(x + 1, y - 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 1, y - 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 2, y));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 2, y));
                    ShipPartsPositions.Add(await interToStringerLocation(x, y + 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 1, y + 2));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 1, y + 2));
                    break;
                }
            //West
            case 2:
                {
                    ShipPartsPositions.Add(await interToStringerLocation(x - 1, y));
                    //HEAD
                    ShipPartsPositions.Add(await interToStringerLocation(x - 2, y));
                    //
                    ShipPartsPositions.Add(await interToStringerLocation(x - 1, y - 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 1, y + 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x, y - 2));
                    ShipPartsPositions.Add(await interToStringerLocation(x, y + 2));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 1, y));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 2, y - 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 2, y + 1));
                    break;
                }
            //East
            case 3:
                {
                    ShipPartsPositions.Add(await interToStringerLocation(x + 1, y));
                    //HEAD
                    ShipPartsPositions.Add(await interToStringerLocation(x + 2, y));
                    //
                    ShipPartsPositions.Add(await interToStringerLocation(x + 1, y + 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x + 1, y - 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x, y + 2));
                    ShipPartsPositions.Add(await interToStringerLocation(x, y - 2));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 1, y));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 2, y + 1));
                    ShipPartsPositions.Add(await interToStringerLocation(x - 2, y - 1));
                    break;
                }
        }
        //Available treasure spaces - deprecated in favor of new treasure sys
        /*
        for(int i=0;i<ShipPartsPositions.Count;i++)
        {
            if(ShipPartsPositions[i] != null)
            {
                AvailableTreasureSpaces.RemoveAt(AvailableTreasureSpaces.IndexOf(ShipPartsPositions[i]));
            }    
            
        }*/
    }    
    private async Task<string> interToStringerLocation(int x,int y)
    {
        if(x < 10 && y < 10)
        {
            return await Task.FromResult($"0{x}0{y}");
        }
        if(x < 10 && y > 10)
        {
            return await Task.FromResult($"0{x}{y}");
        }
        if (x > 10 && y < 10)
        {
            return await Task.FromResult($"{x}0{y}");
        }
        if (x > 10 && y > 10)
        {
            return await Task.FromResult($"{x}{y}");
        }
        return null;
        
    }
    //[ClientRpc]
    private async Task DisplayIndividualShips()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if(ServerVisibleGrid.Row[i].Column[j] > 4)
                {
                    if (ServerVisibleGrid.Row[i].Column[j] - 5 > PlanesPlayers.Count - 1)
                        continue;
                    //LocalPlayerActions.Instance.PlayingField.SetTile(new Vector3Int(i, j), Tiles[ServerVisibleGrid.Row[i].Column[j]]);
                    PlanesPlayers[ServerVisibleGrid.Row[i].Column[j] - 5].UpdateTile(new Vector3Int(i, j), ServerVisibleGrid.Row[i].Column[j]);
                }
                
            }
        }
        await Task.Yield();
    }
    //[ClientRpc]
    public async Task RevealToAllPlayersCurrentBoardState()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (ServerVisibleGrid.Row[i].Column[j] < 5)
                {
                    foreach (PlanesPlayer _player in PlanesPlayers)
                    {
                        try { _player.UpdateTile(new Vector3Int(i, j), ServerVisibleGrid.Row[i].Column[j]); }
                        catch { Debug.Log("Player not present"); }
                    }
                        
                    //LocalPlayerActions.Instance.PlayingField.SetTile(new Vector3Int(i, j), Tiles[ServerVisibleGrid.Row[i].Column[j]]);
                }

            }
        }
        await Task.Yield();
    }
    //[TargetRpc]
    public async Task ShowMap()
    {
        for(int i=0;i<BoardSize;i++)
        {
            for(int j=0;j<BoardSize;j++)
            {
                LocalPlayerActions.Instance.PlayingField.SetTile(new Vector3Int(i, j), Tiles[ServerVisibleGrid.Row[i].Column[j]]);
            }    
        }    
        await Task.Yield();
    }
    public void ResetBoard()
    {
        //For Debugging purposes on reset, remove after completion
        //LocalPlayerActions.Instance.LocalPlayerIndex = 0;
        //
        SetupInProgress = true;        
        CurrentPlayerTurn = 0;
        PlayersList.Clear();
        PlanesPlayers.Clear();
        AvailableBuildSpaces.Clear();
        foreach(PlayerShipStructure ship in PlayersList)
        {
            ship.Misfire = false;
        }
        //AvailableTreasureSpaces.Clear();
        //SetupBoard();        
    }
    public void DebugPlayerIndexSwitcher()
    {
        DebugPlayerIndex = !DebugPlayerIndex;
        if(DebugPlayerIndex)
        {
            Debug.Log(string.Format($"<color=white>Debugging Player Index = </color><color=lime><size=25><b>{DebugPlayerIndex}</b></size></color>"));
            LocalPlayerActions.Instance.LocalPlayerIndex = CurrentPlayerTurn;
        }
        else
        {
            Debug.Log(string.Format($"<color=white>Debugging Player Index = </color><color=red><size=25><b>{DebugPlayerIndex}</b></size></color>"));
            //Debug.LogFormat()
        }
        

    }
    #endregion

    private async Task IncreaseTurn()
    {
        int whileCounter = 0;
        do
        {
            CurrentPlayerTurn++;
            CurrentPlayerTurn = CurrentPlayerTurn % PlanesPlayers.Count;

            if (PlayersList[CurrentPlayerTurn].Misfire)
            {
                PlayersList[CurrentPlayerTurn].Misfire = false;
                continue;
            }

            whileCounter++;
        }
        while (PlayersList[CurrentPlayerTurn].Misfire || (PlayersList[CurrentPlayerTurn].isDestroyed  && whileCounter < 20));

        if (whileCounter >= 20)
            Debug.LogWarning("out of whiles [Increase Turn]");

        await Task.Yield();
    }

    public void CheckTurn(int PlayerIndex)
    {
        playerShipDestroy(PlayerIndex);

        Debug.Log("Current player turn " + CurrentPlayerTurn);
        int whileCounter = 0;


        //CurrentPlayerTurn = CurrentPlayerTurn % PlanesPlayers.Count;
        do
        {
            CurrentPlayerTurn++;
            CurrentPlayerTurn = CurrentPlayerTurn % PlanesPlayers.Count;

            if (PlayersList[CurrentPlayerTurn].Misfire)
            {
                PlayersList[CurrentPlayerTurn].Misfire = false;
                continue;
            }

            whileCounter++;
        }
        while (PlayersList[CurrentPlayerTurn].Misfire || (PlayersList[CurrentPlayerTurn].isDestroyed && whileCounter < 20));
        Debug.Log("new player turn " + CurrentPlayerTurn);
        if (whileCounter >= 20)
            Debug.LogWarning("out of whiles [Check Turn]");
    }

    private void FinishGame()
    {
        //SetupInProgress = true;
        //PlanesPlayers[0].FinishGame();
        foreach(PlayerShipStructure ship in PlayersList)
        {
            if(!ship.isDestroyed)
                PlanesPlayers[PlayersList.IndexOf(ship)].FinishGame();
        }
    }

    private void ShowFakeShip(int indexPlayer)
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (ServerVisibleGrid.Row[i].Column[j] == 10)
                {                    
                    PlanesPlayers[indexPlayer].UpdateTile(new Vector3Int(i, j), ServerVisibleGrid.Row[i].Column[j]);
                }
            }
        }
    }
    //DisplayBoardToEachPlayer   
}
