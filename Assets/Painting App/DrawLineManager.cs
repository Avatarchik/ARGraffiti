using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;


public class DrawLineManager : MonoBehaviour {

	private float rayDist = 0.3f;
	public Material lMat;
	public GameObject arCanvas; 
	public GameObject paintPanel;
	public Text textLabel;

	private GraphicsLineRenderer currLine;

	private float colorRed = 0.9f;
	private float colorBlue = 0.0f;
	private float colorGreen = 0.0f;

	private int numClicks = 0;
	private int numReplayClicks = 0;

	private Vector3 prevPaintPoint;
	private float paintLineThickness = 0.02f;

	public Slider slider;


	//private BrushTipManager brushFunctions = new BrushTipManager() ;

	public EventSystem eventSystemManager;

	public GameObject drawingRootSceneObject;
	public GameObject paintBrushSceneObject;
	private GameObject shape;


	public void setLineWidth(float thickness)
	{
		paintLineThickness = thickness;
	}


	public void setLineColor(float red, float green, float blue)
	{
		colorRed = red;
		colorGreen = green;
		colorBlue = blue;
	}

	public Vector3 getRayEndPoint(float dist)
	{
		Ray ray = Camera.main.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0.5f));
		Vector3 endPoint = ray.GetPoint (dist);
		return endPoint;
	}

	// Use this for initialization
	void Start () {

		//shape = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		//shape.transform.localScale = new Vector3 (0.02f, 0.02f, 0.02f);
		//shape.GetComponent<Renderer> ().material = lMat;
		//shape.GetComponent<Renderer> ().material.color = new Color (1, 1, 1);
	}
	
	// Update is called once per frame
	void Update () {


		Vector3 endPoint = getRayEndPoint (rayDist);

		//renderSphereAsBrushTip (endPoint);


		bool firstTouchCondition;
		bool whileTouchedCondition;

		GameObject currentSelection = eventSystemManager.currentSelectedGameObject;
		bool isPanelSelected = currentSelection == null;

		firstTouchCondition = (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began && isPanelSelected); 
		whileTouchedCondition = (Input.touchCount == 1 && isPanelSelected);

		if (firstTouchCondition == true) {

			// check if you're in drawing mode. if not, return.
			if (!paintPanel.activeSelf) {
				textLabel.text = "First Click Start Painting";
				return;
			}

			Debug.Log ("First touch");

			// start drawing line
			GameObject go = new GameObject ();
			go.transform.position = endPoint;
			go.transform.parent = drawingRootSceneObject.transform;

			go.AddComponent<MeshFilter> ();
			go.AddComponent<MeshRenderer> ();
			currLine = go.AddComponent<GraphicsLineRenderer> ();

			currLine.lmat = new Material(lMat);
			currLine.SetWidth (paintLineThickness);


			Color newColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));



			//currLine.lmat.color = new Color (colorRed, colorGreen, colorBlue); 
			currLine.lmat.color = newColor;

			numClicks = 0;

			prevPaintPoint = endPoint;

			// add to history and increment index

			Debug.Log ("Adding History 1");

			int index = arCanvas.GetComponent<PaintController> ().drawingHistoryIndex;
			index++;

			Debug.Log ("Adding History 2");


			paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().addDrawingCommand (index, 0, endPoint, currLine.lmat.color, paintLineThickness);

			Debug.Log ("Adding History 3");
			arCanvas.GetComponent<PaintController> ().drawingHistoryIndex = index;

			Debug.Log ("Done Adding History");

		} else if (whileTouchedCondition == true) {

			if ((endPoint - prevPaintPoint).magnitude > 0.01f) {

				// continue drawing line
				//currLine.SetVertexCount (numClicks + 1);
				//currLine.SetPosition (numClicks, endPoint); 

				currLine.AddPoint (endPoint);
				numClicks++;

				prevPaintPoint = endPoint;

				// add to history without incrementing index
				int index = arCanvas.GetComponent<PaintController> ().drawingHistoryIndex;

				paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().addDrawingCommand (index, 0, endPoint, currLine.lmat.color, paintLineThickness);

			}
		}
		
	}

	public void renderSphereAsBrushTip(Vector3 endPoint)
	{
		shape.transform.position = endPoint;
	
	}


	public void addReplayLineSegment(bool toContinue, float lineThickness, Vector3 position, Color color)
	{
		if (toContinue == false) {

			// start drawing line
			GameObject go = new GameObject ();
			go.transform.position = position;
			go.transform.parent = drawingRootSceneObject.transform;

			go.AddComponent<MeshFilter> ();
			go.AddComponent<MeshRenderer> ();
			currLine = go.AddComponent<GraphicsLineRenderer> ();

			currLine.lmat = new Material(lMat);
			currLine.SetWidth (lineThickness);
			currLine.lmat.color = color;
			numReplayClicks = 0;


		} else {

			// continue line
			currLine.AddPoint (position);
			numReplayClicks++;

		}

	}

}
