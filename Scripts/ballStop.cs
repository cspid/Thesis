using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class ballStop : MonoBehaviour
{
    public bool active;
    bool active2;

    public Animator body;
    LookAtIK lookIK;
    public DemoSequence demoSequence;
    public GameObject Ball_BL;
    bool hasAdded;

    public void OpenFrameBL()
    {
        if (active == true)
        {
            print("Ball advancing counter");
            active2 = true;
            if (hasAdded == false)
            {
                demoSequence.counter++;
                hasAdded = true;            
            }
            print("counter +1 ballStop.cs");
            Ball_BL.SetActive(true);
        }
    }

    public void BallKill()
    {
        if (active2 == true)
        {
            body.enabled = false;
            lookIK = body.gameObject.GetComponent<RootMotion.FinalIK.LookAtIK>();
            lookIK.enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
