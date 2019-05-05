using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class lerpRectTranform : baseLerper
{
    public bool pingPong = false;
    public float lerpDuration = 1f;
    public LerpUtility.lerpMode lerpMode;
    public Vector3 endPosition;
    private Vector3 startPositon;
    // Use this for initialization
    public bool local = false;
    private bool lerpingdown = false;
    private float currentLerpTime;
    public RectTransform rectTransform;
    bool updateStartPos = true;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (local)
        {
            startPositon = rectTransform.anchoredPosition ;
        }
        else
        {
            startPositon = rectTransform.anchoredPosition;
        }

    }
    public override void startLerp()
    {
        base.startLerp();

    }

    void ResetStart()
    {
        rectTransform = GetComponent<RectTransform>();
        if (local)
        {
            startPositon = rectTransform.anchoredPosition;
        }
        else
        {
            startPositon = rectTransform.anchoredPosition;
        }

    }
    // Update is called once per frame
    void Update()
    {
        

            if (isLerping == true)
            {
                if (updateStartPos == true)
                {
                    ResetStart();
                    updateStartPos = false;
                }

            currentLerpTime += Time.deltaTime;
            if (local)
            {
                rectTransform.anchoredPosition = Vector3.Lerp(startPositon, endPosition, LerpUtility.Lerp(currentLerpTime, lerpDuration, lerpMode));

            }
            else
            {
                rectTransform.anchoredPosition = Vector3.Lerp(startPositon, endPosition, LerpUtility.Lerp(currentLerpTime, lerpDuration, lerpMode));

            }
            if (currentLerpTime >= lerpDuration)
            {
                if (pingPong == false)
                {
                    isLerping = false;

                }
                else
                {
                    Vector3 temp = startPositon;
                    startPositon = endPosition;
                    endPosition = temp;
                    if (lerpingdown == true)
                    {
                        isLerping = false;
                        lerpingdown = false;
                    }
                    else
                    {
                        lerpingdown = true;

                    }


                }
                currentLerpTime = 0;

            }
        }

    }
}
