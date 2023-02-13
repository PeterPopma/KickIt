using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    int number;
    Stats stats = new Stats();
    List<Player> players = new List<Player>();
    Player playerClosestToBall;
    Player goalKeeper;

    public Team(int number)
    {
        this.number = number;
    }

    public int Number { get => number; set => number = value; }
    public List<Player> Players { get => players; set => players = value; }
    public Player PlayerClosestToBall { get => playerClosestToBall; set => playerClosestToBall = value; }

    public Player GoalKeeper { get => goalKeeper; set => goalKeeper = value; }
    public Stats Stats { get => stats; set => stats = value; }
}
