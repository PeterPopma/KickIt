using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal
{
    public string Name;
    public string Time;

    public Goal(string name, string time)
    {
        Name = name;
        Time = time;
    }
}

public class Stats
{
    public int Score;
    public int NumGoals;
    public int BallPosession;
    public int BallOnHalf;
    public int Shots;
    public int ShotsOnGoal;
    public int Corners;
    public List<Goal> Goals;

    public string GoalsAsText()
    {
        string goalText = "";
        int numlines = 0;
        foreach (Goal goal in Goals)
        {
            goalText += goal.Time + " - " + goal.Name + "<br>";
            numlines++;
        }
        while (numlines < 4)
        {
            numlines++;
            goalText += ".<br>";
        }

        return goalText;
    }
}



