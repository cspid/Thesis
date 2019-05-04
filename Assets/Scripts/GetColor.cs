using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class GetColor : MonoBehaviour
{


    Color newColor;
    Image image;
    public Camera backgroundCamera;
    // Start is called before the first frame update
    void Start()
    {
       image =  GetComponent<Image>();
       image.color = backgroundCamera.backgroundColor;

    }

    // Update is called once per frame
    void Update()
    {
        image.color = backgroundCamera.backgroundColor;
    }
}
