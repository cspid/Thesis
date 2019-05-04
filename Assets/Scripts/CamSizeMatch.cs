using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSizeMatch : MonoBehaviour {
    public Camera middleCam;
    Camera thisCam;
	// Use this for initialization
	void Start () {
        thisCam = GetComponent<Camera>();	
	}
	
	// Update is called once per frame
	void Update () {
        thisCam.orthographicSize = middleCam.orthographicSize;
        thisCam.rect = middleCam.rect;
        //Do It with Rect
	}
}
