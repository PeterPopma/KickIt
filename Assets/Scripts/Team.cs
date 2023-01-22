using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    int number;
    int score;
    List<Player> players = new List<Player>();
    Player playerClosestToBall;
    GoalKeeper goalKeeper;

    public Team(int number)
    {
        this.number = number;
    }

    public int Number { get => number; set => number = value; }
    public List<Player> Players { get => players; set => players = value; }
    public Player PlayerClosestToBall { get => playerClosestToBall; set => playerClosestToBall = value; }
    public int Score { get => score; set => score = value; }
    public GoalKeeper GoalKeeper { get => goalKeeper; set => goalKeeper = value; }
}
