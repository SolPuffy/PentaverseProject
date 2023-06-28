using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorBasics {

    public class UIPlayer : MonoBehaviour {
        [SerializeField] GameObject Border;
        [SerializeField] Text text;
        CardPlayer player;

        public void SetPlayer (CardPlayer player) {
            this.player = player;
            text.text = "Spieler " + player.playerIndex.ToString ();
            if (CardPlayer.localPlayer == player)
                Border.SetActive(true);
            else
                Border.SetActive(false);
        }

    }
}