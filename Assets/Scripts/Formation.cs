using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Formation
{
    public static void Set(Team team, Formation_ formation)
    {
        team.Formation = formation;
        switch (formation)
        {
            case Formation_._343:
                Vector2[] positions;
                positions = _343();
                SetPositions(team, positions);
                break;
            case Formation_._433:
                positions = _433();
                SetPositions(team, positions);
                break;
            case Formation_._442:
                positions = _442();
                SetPositions(team, positions);
                break;
            case Formation_._532:
                positions = _532();
                SetPositions(team, positions);
                break;
        }
    }

    private static void SetPositions(Team team, Vector2[] positions)
    {
        for (int player = 0; player < positions.Length; player++)
        {
            team.Players[player].SpawnPosition = positions[player];
        }
    }

    public static Vector2[] _343() {
        Vector2[] positions = new Vector2[10];
        positions[0] = new Vector2(10.08f, 0.46f);
        positions[1] = new Vector2(7.02f, -11.17f);
        positions[2] = new Vector2(7.33f, 11.09f);
        positions[3] = new Vector2(17.51f, -15.742f);
        positions[4] = new Vector2(19.82f, -6.55f);
        positions[5] = new Vector2(20.15f, 4.13f);
        positions[6] = new Vector2(17.74f, 15.89f);
        positions[7] = new Vector2(30.46f, -10.94f);
        positions[8] = new Vector2(29.53f, 0);
        positions[9] = new Vector2(30.57f, 11.31f);

        return positions;
    }

    public static Vector2[] _433()
    {
        Vector2[] positions = new Vector2[10];
        positions[0] = new Vector2(10.08f, 0.46f);
        positions[1] = new Vector2(7.02f, -11.17f);
        positions[2] = new Vector2(7.33f, 11.09f);
        positions[3] = new Vector2(15.04f, -14.94f);
        positions[4] = new Vector2(13.8f, 0.08f);
        positions[5] = new Vector2(15.43f, 13.12f);
        positions[6] = new Vector2(28.27f, -16.82f);
        positions[7] = new Vector2(31.76f, -7.91f);
        positions[8] = new Vector2(31.83f, 7.03f);
        positions[9] = new Vector2(28.58f, 18.54f);

        return positions;
    }

    public static Vector2[] _442()
    {
        Vector2[] positions = new Vector2[10];
        positions[0] = new Vector2(7.8f, -6.29f);
        positions[1] = new Vector2(8.23f, 5.34f);
        positions[2] = new Vector2(6.13f, 12.13f);
        positions[3] = new Vector2(15.04f, -14.94f);
        positions[4] = new Vector2(13.8f, 0.08f);
        positions[5] = new Vector2(15.43f, 13.12f);
        positions[6] = new Vector2(28.27f, -16.82f);
        positions[7] = new Vector2(31.76f, -7.91f);
        positions[8] = new Vector2(31.83f, 7.03f);
        positions[9] = new Vector2(28.58f, 18.54f);

        return positions;
    }

    public static Vector2[] _532()
    {
        Vector2[] positions = new Vector2[10];
        positions[0] = new Vector2(7.8f, -6.29f);
        positions[1] = new Vector2(8.23f, 5.34f);
        positions[2] = new Vector2(17.88f, -10.66f);
        positions[3] = new Vector2(18.5f, 0.46f);
        positions[4] = new Vector2(17.61f, 12.19f);
        positions[5] = new Vector2(28.27f, -16.82f);
        positions[6] = new Vector2(30.14f, -9.1f);
        positions[7] = new Vector2(30.11f, 0.41f);
        positions[8] = new Vector2(30.36f, 8.83f);
        positions[9] = new Vector2(28.58f, 16.54f);

        return positions;
    }
}
