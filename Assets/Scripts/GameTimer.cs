using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    private const float GAME_DURATION = 120;        // game duration in seconds
    [SerializeField] private TextMeshProUGUI textGameTime;
    private float startTime;
    private float timePassed;


    public void InitTimer()
    {
        startTime = Time.time;
    }

    public string TimePassedAs90Minutes()
    {
        int gametimeSeconds = (int)((timePassed / GAME_DURATION) * 90 * 60);
        int minutesLeft = gametimeSeconds / 60;
        int secondsLeft = gametimeSeconds % 60;

        return minutesLeft.ToString("00") + ":" + secondsLeft.ToString("00");
    }

    // Update is called once per frame
    void Update()
    {
        if (!Game.Instance.GameState.Equals(GameState_.Playing) && !Game.Instance.GameState.Equals(GameState_.Penalty))
        {
            return;
        }
        timePassed = Time.time - startTime;
        if (timePassed >= GAME_DURATION)
        {
            Game.Instance.SetGameState(GameState_.MatchOver);
        }
        textGameTime.text = TimePassedAs90Minutes();
    }
}
