using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkBorders : MonoBehaviour
{
    [Header("Offset Groups")]

    public Transform topExtender;
    public Transform bottomExtender;
    public Transform leftExtender;
    public Transform rightExtender;

    [Header("Offset Values")]
    public float topOffset;
    public float bottomOffset;
    public float leftOffset;
    public float rightOffset;



    [Header("UI Elements")]

    public Transform leftBorder;
    public Transform rightBorder;
    public Transform topBorder;
    public Transform bottomBorder;
    public Transform topLeft;
    public Transform topRight;
    public Transform bottomLeft;
    public Transform bottomRight;




    float topLastFrame;
    float bottomLastFrame;
    float leftLastFrame;
    float rightLastFrame;



    //// Start is called before the first frame update
    //void OnGUI()
    //{
    //    //lock Rect positions so thedont extend beyond the screen
    //    if (topExtender.GetComponent<RectTransform>().position.y > 0) topExtender.GetComponent<RectTransform>().position = new Rect(dsf);
    //}

    // Update is called once per frame
    void Update()
    {
        //Get Rect Position of frames
        topOffset = topExtender.GetComponent<RectTransform>().position.y;
        bottomOffset = bottomExtender.GetComponent<RectTransform>().position.y;
        leftOffset = leftExtender.GetComponent<RectTransform>().position.x;
        rightOffset = rightExtender.GetComponent<RectTransform>().position.x;

       

        //---------------------------------//
        //---Manage the top frame offset---//

        if (topOffset != topLastFrame)
        {
            print("Offsetting Top");
            topLastFrame = topOffset;
            topExtender.SetSiblingIndex(0);
            foreach (Transform child in transform)
            {
                if (child == topBorder || child == topLeft || child == topRight)
                {
                    child.transform.parent = topExtender;
                }
                else
                {
                    child.transform.parent = transform;
                }
            }            
        }
        else
        {
            //Reset parent to the canvas if value not changing
            foreach (Transform child in topExtender)
            {
                child.parent = transform;
            }
        }

        //------------------------------------//
        //---Manage the bottom frame offset---//
        if (bottomOffset != bottomLastFrame)
        {
            print("Offsetting bottom");
            bottomLastFrame = bottomOffset;
            bottomExtender.SetSiblingIndex(0);

            foreach (Transform child in transform)
            {
                if (child == bottomBorder || child == bottomLeft || child == bottomRight)
                {
                    child.transform.parent = bottomExtender;                 
                }
                else
                {
                    child.transform.parent = transform;
                }
            }        
        }
        else
        {
            //Reset parent to the canvas if value not changing
            foreach (Transform child in bottomExtender)
            {
                child.parent = transform;
            }
        }

        //----------------------------------//
        //---Manage the left frame offset---//

        if (leftOffset != leftLastFrame)
        {
            print("Offsetting Left");
            leftExtender.SetSiblingIndex(0);
            leftLastFrame = leftOffset;
            foreach (Transform child in transform)
            {
                if (child == leftBorder || child == bottomLeft || child == topLeft)
                {
                    child.transform.parent = leftExtender;
                }
                else
                {
                    child.transform.parent = transform;
                }
            }          
        }
        else
        {
            //Reset parent to the canvas if value not changing
            foreach (Transform child in leftExtender)
            {
                child.parent = transform;
            }
        }

        //-----------------------------------//
        //---Manage the right frame offset---//

        if (rightOffset != rightLastFrame)
        {
            print("Offsetting right");
            topExtender.SetSiblingIndex(0);

            rightLastFrame = rightOffset;
            foreach (Transform child in transform)
            {
                if (child == rightBorder || child == bottomRight || child == topRight)
                {
                    child.transform.parent = rightExtender;
                }
                else
                {
                    child.transform.parent = transform;
                }
            }
        }
        else
        {
            //Reset parent to the canvas if value not changing
            foreach (Transform child in rightExtender)
            {
                child.parent = transform;
            }
        }
    }
}

    