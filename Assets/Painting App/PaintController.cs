using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;

public class PaintController : MonoBehaviour, PlacenoteListener {


	private UnityARSessionNativeInterface mSession;

	public GameObject paintBrushSceneObject;
	public GameObject drawingRootSceneObject;
	public Text textLabel;
	private bool pointCloudOn = false;

	public GameObject paintPanel;
	public GameObject startPanel;


	private bool mFrameUpdated = false;
	private UnityARImageFrameData mImage = null;
	private UnityARCamera mARCamera;
	private bool mARKitInit = false;

	private bool savedSceneLoaded = false;

	public int drawingHistoryIndex = 0;

	// Use this for initialization
	void Start () {

		mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface ();
		UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
		StartARKit ();

		//FeaturesVisualizer.EnablePointcloud ();
		LibPlacenote.Instance.RegisterListener (this);

		//StartCoroutine (startPlacenoteMapping ());

	}

	public void onClickEnablePointCloud()
	{
		if (pointCloudOn == false) {
			FeaturesVisualizer.EnablePointcloud ();
			pointCloudOn = true;
			Debug.Log ("Point Cloud On");
		} else {
			FeaturesVisualizer.DisablePointcloud ();
			pointCloudOn = false;
			Debug.Log ("Point Cloud Off");
		}

	}

	private void ARFrameUpdated (UnityARCamera camera)
	{
		mFrameUpdated = true;
		mARCamera = camera;
	}

	// Update is called once per frame
	void Update () {
		
		if (mFrameUpdated) {
			mFrameUpdated = false;
			if (mImage == null) {
				InitARFrameBuffer ();
			}

			if (mARCamera.trackingState == ARTrackingState.ARTrackingStateNotAvailable) {
				// ARKit pose is not yet initialized
				return;
			} else if (!mARKitInit) {
				mARKitInit = true;
				print("ARKit Initialized");
			}

			Matrix4x4 matrix = mSession.GetCameraPose ();

			Vector3 arkitPosition = PNUtility.MatrixOps.GetPosition (matrix);
			Quaternion arkitQuat = PNUtility.MatrixOps.GetRotation (matrix);

			LibPlacenote.Instance.SendARFrame (mImage, arkitPosition, arkitQuat, mARCamera.videoParams.screenOrientation);

		}

	}		

	public void onStartPaintingClick ()
	{
		//Vector3 shapePosition = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
		//Quaternion shapeRotation = Camera.main.transform.rotation;

		/*
		GameObject shape = GameObject.CreatePrimitive (PrimitiveType.Cube);
		shape.transform.position = new Vector3 (0.0f, 0.0f, 0f);
		shape.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
		shape.GetComponent<Renderer>().material.color = new Color(0.5f,1,1); 
        */

		// if you want to reset the drawing board everytime you click this, uncomment this
		//deleteAllObjects ();
		//replayDrawing ();

		startPanel.SetActive (false);
		paintPanel.SetActive (true);

		deleteAllObjects ();

		LibPlacenote.Instance.StopSession ();
		LibPlacenote.Instance.StartSession ();

		textLabel.text = "Touch the screen to paint";



		//replayDrawing ();
	}

	private void StartARKit ()
	{
		print("Initializing ARKit");
		Application.targetFrameRate = 60;
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();
		config.planeDetection = UnityARPlaneDetection.Horizontal;
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = true;
		config.enableLightEstimation = true;
		mSession.RunWithConfig (config);
	}

	IEnumerator startPlacenoteMapping ()
	{
		while (!LibPlacenote.Instance.Initialized() && mARKitInit) {
			Debug.Log ("SDK not yet initialized");
			yield return new WaitForSeconds(0.1f);
		}

		LibPlacenote.Instance.StartSession ();
	}



	public void OnSaveMapClick ()
	{
		

		if (!LibPlacenote.Instance.Initialized()) {
			Debug.Log ("SDK not yet initialized");
			ToastManager.ShowToast ("SDK not yet initialized", 2f);
			return;
		}

		//mLabelText.text = "Saving...";
		LibPlacenote.Instance.SaveMap (
			(mapId) => {
				LibPlacenote.Instance.StopSession ();

				print("Saved Map Id:" + mapId);

				saveScene (mapId);

				textLabel.text = "Saving Your Painting...";

				//mLabelText.text = "Saved Map ID: " + mapId;
				//mInitButtonPanel.SetActive (true);
				//mMappingButtonPanel.SetActive (false);

				//string jsonPath = Path.Combine(Application.persistentDataPath, mapId + ".json");
				//SaveShapes2JSON(jsonPath);
			},
			(completed, faulted, percentage) => {
				Debug.Log("Uploading map...");

				if(completed) {
					Debug.Log("Done Uploaded!!");

					textLabel.text = "Saved! Try Loading it!";

					startPanel.SetActive(true);
					paintPanel.SetActive(false);


				}

			}
		);
	}


	public void OnLoadMapClicked ()
	{
		savedSceneLoaded = false;

		if (!LibPlacenote.Instance.Initialized()) {
			Debug.Log ("SDK not yet initialized");
			ToastManager.ShowToast ("SDK not yet initialized", 2f);
			return;
		}

		// hard code a mapID in here until you can save mapID's
		//var mSelectedMapId = "adc0feb9-51ea-40fc-92fe-e45ba8c53b34";

		var mSelectedMapId = paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().loadMapIDFromFile ();

		if (mSelectedMapId == null) {
			Debug.Log ("The saved map id was null!");

		} else {
			LibPlacenote.Instance.LoadMap (mSelectedMapId,
				(completed, faulted, percentage) => {
					if (completed) {

						textLabel.text = "Loading Your Drawing";


						LibPlacenote.Instance.StartSession ();
						//mLabelText.text = "Loaded ID: " + mSelectedMapId;
					} else if (faulted) {
						//mLabelText.text = "Failed to load ID: " + mSelectedMapId;
					}
				}
			);
		}


	}


	public void replayDrawing()
	{
		deleteAllObjects ();

		StartCoroutine ( paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().replayDrawing () );

		//paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().replayDrawingFast ();

	}


	public void deleteAllObjects()
	{

		int numChildren = drawingRootSceneObject.transform.childCount;

		for (int i = 0; i < numChildren; i++) {

			GameObject toDestroy = drawingRootSceneObject.transform.GetChild (i).gameObject;

			if (string.Compare (toDestroy.name, "CubeBrushTip") != 0  && string.Compare (toDestroy.name, "SphereBrushTip") != 0   ) {
				Destroy (drawingRootSceneObject.transform.GetChild (i).gameObject);
			}
		}

	}


	public void onClearAllClick()
	{
		deleteAllObjects ();
		paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().resetHistory ();
	}


	public void saveScene (string mapid)
	{
		paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().saveDrawingHistory ();

		paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().saveMapIDToFile (mapid);
	}


	public void loadSavedScene()
	{
		// delete current scene
		deleteAllObjects();

		// load saved scene
		paintBrushSceneObject.GetComponent<DrawingHistoryManager> ().loadFromDrawingHistory ();

		// replay drawing
		replayDrawing();


	}

	private void InitARFrameBuffer ()
	{
		mImage = new UnityARImageFrameData ();

		int yBufSize = mARCamera.videoParams.yWidth * mARCamera.videoParams.yHeight;
		mImage.y.data = Marshal.AllocHGlobal (yBufSize);
		mImage.y.width = (ulong)mARCamera.videoParams.yWidth;
		mImage.y.height = (ulong)mARCamera.videoParams.yHeight;
		mImage.y.stride = (ulong)mARCamera.videoParams.yWidth;

		// This does assume the YUV_NV21 format
		int vuBufSize = mARCamera.videoParams.yWidth * mARCamera.videoParams.yWidth/2;
		mImage.vu.data = Marshal.AllocHGlobal (vuBufSize);
		mImage.vu.width = (ulong)mARCamera.videoParams.yWidth/2;
		mImage.vu.height = (ulong)mARCamera.videoParams.yHeight/2;
		mImage.vu.stride = (ulong)mARCamera.videoParams.yWidth;

		mSession.SetCapturePixelData (true, mImage.y.data, mImage.vu.data);
	}

	public void OnPose (Matrix4x4 outputPose, Matrix4x4 arkitPose) {}



	// This function runs when LibPlacenote sends a status change message like Localized!

	public void OnStatusChange (LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
	{
		Debug.Log ("prevStatus: " + prevStatus.ToString() + " currStatus: " + currStatus.ToString());


		if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST) {

			Debug.Log ("Localized!");

			textLabel.text = "Found It!";

			/*
			GameObject shape = GameObject.CreatePrimitive (PrimitiveType.Cube);
			shape.transform.position = new Vector3 (0.0f, 0.0f, 0f);
			shape.transform.localScale = new Vector3 (0.2f, 0.2f, 0.2f);
			shape.GetComponent<Renderer>().material.color = new Color(1,0.5f,1); 
			*/

			if (!savedSceneLoaded) {
				savedSceneLoaded = true;
				loadSavedScene ();
			}


			/*
			string jsonPath = Path.Combine (Application.persistentDataPath, mSelectedMapId + ".json");

			if (File.Exists (jsonPath) && shapeObjList.Count == 0) {
				LoadShapesJSON (jsonPath);
			}
			*/

		} else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING) {
			Debug.Log ("Mapping");

		} else if (currStatus == LibPlacenote.MappingStatus.LOST) {
			Debug.Log("Searching for position lock");

		} else if (currStatus == LibPlacenote.MappingStatus.WAITING) {


			/*
			 // shapeObjList will be filled from loadshapesjson
			 
			 
			if (shapeObjList.Count != 0) {
				ClearShapes ();
			}
			*/
		}


	}



}
