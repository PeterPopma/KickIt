using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (!Game.Instance.GameState.Equals(GameState_.Playing))
        {
            return;
        }

        if (other.GetComponent<Ball>() != null)
        {
            if (name.EndsWith("0"))
            {
                Game.Instance.ScoreGoal(0);
            }
            else
            {
                Game.Instance.ScoreGoal(1);
            }
        }
    }
}
