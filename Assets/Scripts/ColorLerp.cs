using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ColorLerp : MonoBehaviour
{
    Color backgroundColour;
    public Color firstColour;
    public Color lastColour;
    public Color newBackgroundColor;
    public float duration = 10;


    float t = 0;
    // Start is called before the first frame update
    void Start()
    {
       GetComponent<Camera>().backgroundColor = firstColour;
       backgroundColour =  GetComponent<Camera>().backgroundColor;


    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Camera>().backgroundColor = Color.Lerp(backgroundColour, newBackgroundColor, t);
        if (t < 1)
        { // while t below the end limit...
          // increment it at the desired rate every update:
            t += Time.deltaTime / duration;
        }
    }
}
