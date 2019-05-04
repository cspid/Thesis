using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BorderScript))]

public class CustomInspectorBorder : Editor 
{

	public override void OnInspectorGUI() 
	{

		DrawDefaultInspector();
		BorderScript borderScript = (BorderScript)target;

		if (GUILayout.Button("Check for Cameras"))
        {
            borderScript.CheckForCams();
        }
		if (GUILayout.Button("Check BorderSize"))
        {
			borderScript.CheckBorderSize();
        }


	}

}
