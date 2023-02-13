using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Star : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        int level = gameObject.name[gameObject.name.Length - 1] - '0';
        Settings.ComputerLevel = level;
        for (int star = 1; star < 6; star++)
        {
            if (star <= Settings.ComputerLevel)
            {
                GameObject.Find("CanvasComputerStrength/Star" + star).GetComponent<Image>().color = Color.white;
            }
            else
            {
                GameObject.Find("CanvasComputerStrength/Star" + star).GetComponent<Image>().color = Color.gray;
            }
        }
    }
}
