using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowd : MonoBehaviour
{
    [SerializeField] private GameObject pfCrowd;

    // Start is called before the first frame update
    void Start()
    {
        GameObject crowdRoot = GameObject.Find("Crowd");
        var crowdList = new List<GameObject>();
        /*  
            y start: 11.7089996
            y end: 2.81900001
            y step: -0.7989998
            z start: -40.4700012
            z end: -29.941
            z step: 1.0149994 
        */
        float z = -40.4700012f;
        for (float y = 11.7089996f; y >= 2.81900001; y -= 0.7989998f)
        {
            for (float x = -36.9199982f; x <= -20.8099995; x += 1.099968f)
            {
                GameObject newPerson = Instantiate(pfCrowd, new Vector3(x, y, z), Quaternion.identity);
                newPerson.transform.Rotate(new Vector3(0, 90, 0));
                newPerson.isStatic = true;
                newPerson.transform.parent = crowdRoot.transform;
                crowdList.Add(newPerson);
            }
            z += 1.0149994f;
        }

        GameObject[] gos = crowdList.ToArray();
        StaticBatchingUtility.Combine(gos, crowdRoot);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
