using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
public class DemoSequence : MonoBehaviour
{
    [Header("General")]

    public int counter = 0;
    public SpeechBubbleSwitch bubble;
    public GameObject scene1;
    public GameObject scene2;
    public GameObject scene3;
    public GameObject scene4;
    public GameObject scene5;


    public ColorLerp colorLerp;
    public float timer = 3;

    [Header("Scene 1")]
    public lerpRectTranform openL;
    public lerpRectTranform openBR;
    public lerpRectTranform openT;
    public lerpRectTranform openBL;
    public lerpRectTranform closeTitle;
    public lerpRectTranform closeT;
    public lerpRectTranform closeBR;
    public lerpRectTranform closeBL;
    public lerpRectTranform closeL;
    public Text textCounter;
    public GameObject frames;

    bool newSceneTrigger;
    public ballStop stop;

    [Header("Scene 2")]
    public lerpRectTranform openL_S2;
    public lerpRectTranform openM_S2;
    public lerpRectTranform openR_S2;
    public lerpRectTranform closeL_S2;
    public lerpRectTranform closeM_S2;
    public lerpRectTranform closeR_S2;
    public GameObject frames2;
    bool newSceneTrigger2;



    [Header("Scene 3")]
    public lerpRectTranform openL_S3;
    public lerpRectTranform openM_S3;
    public lerpRectTranform openR_S3;
    public lerpRectTranform closeL_S3;
    public lerpRectTranform closeM_S3;
    public lerpRectTranform closeR_S3;
    public GameObject frames3;
    bool newSceneTrigger3;


    [Header("Scene 4")]
    public lerpRectTranform open_S4;
    public lerpRectTranform close_S4;
    bool newSceneTrigger4;

    [Header("Scene 5")]



    [Header("Yarn")]
    public DialogueRunner dialogueRunner;


    bool callLerp;
    int counterLastFrame;

    // Update is called once per frame
    void Update()
    {
        if(newSceneTrigger == true)
        {
            NewScene();
        }

        if (newSceneTrigger2 == true)
        {
            NewScene2();
        }

        if (newSceneTrigger3 == true)
        {
            NewScene3();
        }

        if (Input.GetButtonDown("Fire1"))
        {
            counter++;
        }

        if (counter > counterLastFrame)
        {
            textCounter.text = "" + counter;
            if (counter == 1)
            {
             // Open text  
                openT.isLerping = true;
                bubble.isSpeech = true;
                dialogueRunner.StartDialogue("Thesis_Start");
}
            if (counter == 2)
            {
                //Open BR
               openBR.isLerping = true;
               closeTitle.isLerping = true;
            }

            if (counter == 3)
            {
                //dialogue
            }

            if (counter == 4)
            {
            }
            if (counter == 5)

            {               
                //Kill the ball and
                stop.active = true;
            }

            if (counter == 6)
            {
                // open new frame
                openBL.isLerping = true;
            }

            if (counter == 7)
            {
                // open new frame
                dialogueRunner.StartDialogue("Thesis_Scene_1B");
            }

            if (counter == 7)
            {
             
                bubble.isSpeech = true;
            }

            if (counter == 9)
            {
                //Shutdown scene 1
                closeT.isLerping = true;
                closeBL.isLerping = true;
                closeBR.isLerping = true;
                closeL.isLerping = true;
                newSceneTrigger = true;               
            }

            if (counter == 10)
            {
                //Shutdown scene 2
                closeL_S2.isLerping = true;
                closeM_S2.isLerping = true;
                closeR_S2.isLerping = true;
                closeL.isLerping = true;
                timer = 4;
                newSceneTrigger2 = true;
            }

            if (counter == 11)
            {
                //Shutdown scene 3
                closeL_S3.isLerping = true;
                closeM_S3.isLerping = true;
                closeR_S3.isLerping = true;
                //closeL.isLerping = true;
                timer = 5;
                newSceneTrigger3 = true;
            }

            if (counter == 12)
            {
                close_S4.isLerping = true;
                colorLerp.t = 0;
                colorLerp.newBackgroundColor = colorLerp.firstColour;



            }
            if (counter == 13)
            {
                scene5.SetActive(true);
                scene4.SetActive(false);
                //open_S5.isLerping = true;
                
            }
        }
        counterLastFrame = counter; 
    }
    void NewScene()
    {
        timer = timer - Time.deltaTime;
        if(timer < 0)
        {
            foreach (Transform child in frames.transform)
            {
                DestroyImmediate(child.gameObject);
                colorLerp.enabled = true;
            }
            scene2.SetActive(true);
        
            openL_S2.isLerping = true;
            openM_S2.isLerping = true;
            openR_S2.isLerping = true;
            newSceneTrigger = false;
            timer = 3;
            scene1.SetActive(false);
        }
    }

    void NewScene2()
    {
        timer = timer - Time.deltaTime;
        if (timer < 0)
        {
            foreach (Transform child in frames2.transform)
            {
                DestroyImmediate(child.gameObject);
                colorLerp.enabled = true;
            }
            scene3.SetActive(true);

            openL_S3.isLerping = true;
            openM_S3.isLerping = true;
            openR_S3.isLerping = true;
            newSceneTrigger2 = false;
            timer = 3;
            scene2.SetActive(false);
        }
    }

    void NewScene3()
    {
        timer = timer - Time.deltaTime;
        if (timer < 0)
        {
            foreach (Transform child in frames3.transform)
            {
                DestroyImmediate(child.gameObject);
                colorLerp.enabled = true;
            }
            scene4.SetActive(true);

            open_S4.isLerping = true;           
            newSceneTrigger3 = false;
            timer = 3;
            scene3.SetActive(false);
        }
    }

}
