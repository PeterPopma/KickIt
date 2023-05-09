using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    int number;
    int playingSide;            // 0 = left side, 1 = right side
    Stats stats = new Stats();
    List<Player> players = new List<Player>();
    Player playerClosestToBall;
    Player goalKeeper;
    bool isHuman;
    Formation_ formation = Formation_._433;

    public Team(int number, int playingSide, bool isHuman)
    {
        this.number = number;
        this.playingSide = playingSide;
        this.isHuman = isHuman;
    }

    public int Number { get => number; set => number = value; }
    public List<Player> Players { get => players; set => players = value; }
    public Player PlayerClosestToBall { get => playerClosestToBall; set => playerClosestToBall = value; }

    public Player GoalKeeper { get => goalKeeper; set => goalKeeper = value; }
    public Stats Stats { get => stats; set => stats = value; }
    public int PlayingSide { get => playingSide; set => playingSide = value; }
    public bool IsHuman { get => isHuman; set => isHuman = value; }
    public Formation_ Formation { get => formation; set => formation = value; }
}
