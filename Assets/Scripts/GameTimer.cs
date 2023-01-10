using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    private float GAME_DURATION = 300;        // game duration in seconds
    private float timePassed;
    private TextMeshProUGUI textGameTime;
    float startTime;


    // Start is called before the first frame update
    void Start()
    {
        textGameTime = GetComponent<TextMeshProUGUI>();
    }

    public void InitTimer()
    {
        startTime = Time.time;
    }

    private string PercentageToTime(float percentage)
    {
        int gametimeSeconds = (int)(percentage * 90 * 60);
        int minutesLeft = gametimeSeconds / 60;
        int secondsLeft = gametimeSeconds % 60;

        return minutesLeft.ToString("00") + ":" + secondsLeft.ToString("00");
    }

    // Update is called once per frame
    void Update()
    {
        if (!Game.Instance.GameState.Equals(GameState_.Playing))
        {
            return;
        }
        timePassed = Time.time - startTime;
        if (timePassed >= GAME_DURATION)
        {
            Game.Instance.SetGameState(GameState_.MatchOver);
        }
        textGameTime.text = PercentageToTime(timePassed / GAME_DURATION);
    }
}
