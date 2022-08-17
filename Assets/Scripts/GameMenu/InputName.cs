using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputName : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textLoading;
    [SerializeField] TextMeshProUGUI inputField;

    public void Start()
    {
        textLoading.enabled = false;
    }

    public void SetName()
    {
        string name = inputField.GetComponent<TextMeshProUGUI>().text;
        if (name != null && name.Length > 0)
        {
            GlobalParams.PreferredPlayerName = name;
        }

        if (GlobalParams.GameMode.Equals(GameMode_.PlayerVsPlayer))
        {
            textLoading.enabled = true;
            SceneManager.LoadSceneAsync("Level1");
        }
        else
        {
            GameObject.Find("CanvasInputName").GetComponent<Canvas>().enabled = false;
            GameObject.Find("CanvasComputerStrength").GetComponent<Canvas>().enabled = true;
        }
    }
}
