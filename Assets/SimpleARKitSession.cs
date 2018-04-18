using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.iOS; // Import ARKit Library

public class SimpleARKitSession : MonoBehaviour {

	// Unity ARKit Session handler
	private UnityARSessionNativeInterface mSession;
	public Material mCubeMaterial;

	void Start () {

		mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface ();
		StartARKit ();
	}

	void Update () {
		// Nothing to do here
	}

	// Add shape when button is clicked.
	public void OnDropShapeClick ()
	{
		GameObject shape = GameObject.CreatePrimitive (PrimitiveType.Cube);
		shape.transform.position = new Vector3 (0.0f, 0.0f, 0f);
		shape.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
		shape.GetComponent<Renderer> ().material = mCubeMaterial;

	}

	// Initialize ARKit
	private void StartARKit ()
	{
		Application.targetFrameRate = 60;
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();
		config.planeDetection = UnityARPlaneDetection.Horizontal;
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = true;
		config.enableLightEstimation = true;
		mSession.RunWithConfig (config);
	}

}
