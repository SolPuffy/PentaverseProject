using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class UIPlayer : MonoBehaviour {
        [SerializeField] GameObject Border;
        [SerializeField] Text text;
        CardPlayer player;

        public void SetPlayer (CardPlayer player) {
            this.player = player;
            text.text = player.Nome;
            if (CardPlayer.localPlayer == player)
                Border.SetActive(true);
            else
                Border.SetActive(false);
        }

    }