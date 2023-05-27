using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StadiumCamera : MonoBehaviour
{
    [SerializeField] Transform ball;


    // Update is called once per frame
    void Update()
    {
        float z = ball.position.z + 25;
        if (z > 29)
        {
            z = 29;
        }
        transform.position = new Vector3(ball.position.x, transform.position.y, z);
    }
}
