using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button2PlayersVsPc : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] ChooseGame scriptChooseGame;

    public void OnPointerEnter(PointerEventData eventData)
    {
        scriptChooseGame.SetGameMode(GameMode_.TwoPlayersVsPc);
    }
}
