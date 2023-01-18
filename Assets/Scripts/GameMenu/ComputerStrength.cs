using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComputerStrength : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textLoading;

    public void Start()
    {
        textLoading.enabled = false;
    }

    public void OnStarClick()
    {
        textLoading.enabled = true;
        SceneManager.LoadSceneAsync("Game");
    }
}
