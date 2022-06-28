using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ReplayBackup : MonoBehaviour
{
    private BackupData DataHold;
    public InputField inputIndex;
    public int SimDelay = 1000;

    public async void inputFieldToData()
    {
        DataHold = await ServerBackup.RetrieveDataHoldFromServer(inputIndex.text);
    }
    public void SendDataToLocations()
    {
        //GAME DECK = DataHold.GameDeck;

        //Start the scene
    }
    public async void SimulateGame()
    {
        for(int i=0;i<DataHold.ActionsPerformed;i++)
        {
            switch (DataHold.playerActions[i])
            {
                case "Hit": 
                    {
                        HitSlapRazboi.instance.HitCards(DataHold.indexParameters[i]);
                        break; 
                    }
                case "Slap": 
                    {
                        HitSlapRazboi.instance.SlapCards(DataHold.indexParameters[i]);
                        break; 
                    }
                default://Error lmao
                    break;
            }
            await Task.Delay(SimDelay);
        }
    }
}
