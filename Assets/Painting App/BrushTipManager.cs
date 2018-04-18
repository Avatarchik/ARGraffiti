using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;

public class BrushTipManager : MonoBehaviour {

	/// <summary>
	/// This class defines the brush tip, width and color
	/// </summary>


	public float brushScale = 0.053f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Get ray end point for the brush tip the phone.
	public Vector3 getRayEndPoint(float dist)
	{
		Ray ray = Camera.main.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0.5f));
		Vector3 endPoint = ray.GetPoint (dist);
		return endPoint;
	}

}
