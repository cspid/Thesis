using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechBubbleSwitch : MonoBehaviour
{

    public Animator anim1;
    public Animator anim2;
    public Animator anim3;
    public Animator anim4;
    bool isBox = true;
    public bool isSpeech;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpeech == true)
        {
            if(isBox == true)
            {
                BoxToSpeech();
                isBox = false;
            }
            else
            {
                CurveToBox();
                isBox = true;
            }
            isSpeech = false;
        }
    }
    public void BoxToSpeech()
    {
        anim1.SetTrigger("BoxToCurved");
        anim2.SetTrigger("BoxToCurved");
        anim3.SetTrigger("BoxToCurved");
        anim4.SetTrigger("BoxToCurved");
    }

    public void CurveToBox()
    {
        anim1.SetTrigger("CurveToBox");
        anim2.SetTrigger("CurveToBox");
        anim3.SetTrigger("CurveToBox");
        anim4.SetTrigger("CurveToBox");
    }
}
