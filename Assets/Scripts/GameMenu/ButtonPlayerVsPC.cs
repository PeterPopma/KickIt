using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPlayerVsPC : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] ChooseGame scriptChooseGame;

    public void OnPointerEnter(PointerEventData eventData)
    {
        scriptChooseGame.SetGameMode(GameMode_.PlayerVsPc);
    }
}
