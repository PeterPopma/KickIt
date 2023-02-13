using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class ChooseGame : MonoBehaviour
{
    [SerializeField] Button2PlayersVsPc button2PlayersVsPc;
    [SerializeField] ButtonPlayerVsPC buttonPlayerVsPC;
    [SerializeField] ButtonPlayerVsPlayer buttonPlayerVsPlayer;
    private GameMode_ selectedGameMode;
 
    public void Start()
    {
        GameObject.Find("CanvasComputerStrength").GetComponent<Canvas>().enabled = false;
        GameObject.Find("CanvasInputName").GetComponent<Canvas>().enabled = false;
        SetGameMode(GameMode_.PlayerVsPc);
    }

    public void SetGameMode(GameMode_ gameMode)
    {
        selectedGameMode = gameMode;        
        button2PlayersVsPc.GetComponent<Image>().color = selectedGameMode.Equals(GameMode_.TwoPlayersVsPc) ? Color.white : Color.gray;
        buttonPlayerVsPC.GetComponent<Image>().color = selectedGameMode.Equals(GameMode_.PlayerVsPc) ? Color.white : Color.gray;
        buttonPlayerVsPlayer.GetComponent<Image>().color = selectedGameMode.Equals(GameMode_.PlayerVsPlayer) ? Color.white : Color.gray;
    }

    public void OnGameModeButtonClick()
    {
        Settings.GameMode = selectedGameMode;
        GameObject.Find("CanvasChooseGame").GetComponent<Canvas>().enabled = false;
        if (selectedGameMode.Equals(GameMode_.PlayerVsPc))
        {
            GameObject.Find("CanvasComputerStrength").GetComponent<Canvas>().enabled = true;
        }
        else
        {
            GameObject.Find("CanvasInputName").GetComponent<Canvas>().enabled = true;
        }

    }



}
