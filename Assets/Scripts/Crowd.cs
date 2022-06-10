using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowd : MonoBehaviour
{
    [SerializeField] private GameObject pfCrowd;

    // Start is called before the first frame update
    void Start()
    {
        float x = -36.9199982f;
        while (x <= -20.8099995)
        {
            GameObject newPerson = Instantiate(pfCrowd, new Vector3(x, 11.6000004f, -40.4799995f), Quaternion.identity);
            newPerson.transform.Rotate(new Vector3(0,90,0));
            x += 1.099968f;
        }
        x = -36.3410011f;
        while (x <= -20.743)
        {
            GameObject newPerson = Instantiate(pfCrowd, new Vector3(x, 10.7989998f, -39.4529991f), Quaternion.identity);
            newPerson.transform.Rotate(new Vector3(0, 90, 0));
            x += 1.099968f;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
