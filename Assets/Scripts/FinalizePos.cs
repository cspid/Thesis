using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalizePos : MonoBehaviour {

	Camera cam;
	Rect newRect;
	private void Start()
	{
		cam = GetComponent<Camera>();
	}
	// Update is called once per frame
	void Finalize () {
		print("butts");
		newRect = cam.rect;
	}

	void SetPos()
    {
        print("butts");
        cam.rect = newRect;
    }
}
