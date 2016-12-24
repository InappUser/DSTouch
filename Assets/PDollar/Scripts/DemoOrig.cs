using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using PDollarGestureRecognizer;

public class DemoOrig : MonoBehaviour {

	public Transform gestureOnScreenPrefab;

	private List<Gesture> trainingSet = new List<Gesture>();

	private List<Point> points = new List<Point>();
	private int strokeId = -1;

	private Vector3 virtualKeyPosition = Vector2.zero;
	[SerializeField] private Rect drawArea = new Rect();

	private RuntimePlatform platform;
	private int vertexCount = 0;

	private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
	private LineRenderer currentGestureLineRenderer;

	//GUI
	private string message;
	private bool recognized;
	private string newGestureName = "";

	float height=0f;
	float areaX=0f;
	float areaY=0f;
	private float[] DS4TouchLength = new float[2] { 1919f, 941f };

	void Start () {

		platform = Application.platform;
		//get ratio between touchpad length


		//Load pre-made gestures
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("Gestures");//("GestureSet/10-stylus-MEDIUM/");
		foreach (TextAsset gestureXml in gesturesXml)
			trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

		//Load user custom gestures
		string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
		foreach (string filePath in filePaths)
			trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
	}
	bool DrawContains(){
		Rect r = new Rect (drawArea.x + 5, drawArea.y + 5, drawArea.width - 7, drawArea.height - 7);
		return r.Contains (new Vector2(virtualKeyPosition.x+areaX-1,virtualKeyPosition.y+areaY-1));
	}
	void Update () {


		if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touchCount > 0) {
				virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
			}
		} else {
			if (Input.GetMouseButton(0)) {
				//this needs
				virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
				Debug.Log (virtualKeyPosition.x+virtualKeyPosition.y);
			}
		}

		if (DrawContains()) {
			Debug.Log ("Contains");
			if (Input.GetMouseButtonDown(0)) {

				if (recognized) {

					recognized = false;
					strokeId = -1;

					points.Clear();

					foreach (LineRenderer lineRenderer in gestureLinesRenderer) {

						lineRenderer.SetVertexCount(0);
						Destroy(lineRenderer.gameObject);
					}

					gestureLinesRenderer.Clear();
				}

				++strokeId;

				Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
				currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();

				gestureLinesRenderer.Add(currentGestureLineRenderer);

				vertexCount = 0;
			}

			if (Input.GetMouseButton(0) && currentGestureLineRenderer!=null) {
				points.Add(new Point(virtualKeyPosition.x, virtualKeyPosition.y, strokeId));

				currentGestureLineRenderer.SetVertexCount(++vertexCount);
				currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));
			}
		}
	}

	void OnGUI() {

		float screenGap = Screen.width/Screen.height;
		float width = DS4TouchLength [0];//*(1+(Screen.width*0.1f));//Screen.width - Screen.width / 3;
		height = Screen.height - Screen.height / 3;
		areaX = (Screen.width *0.5f) - ((width * 0.5f)+screenGap);
		areaY = (Screen.height - (Screen.height * 0f)) - (height+screenGap);
		drawArea.Set(areaX, areaY, width, height);


		GUI.Box(drawArea, "Draw Area");

		GUI.Label(new Rect(10, Screen.height - 40, 500, 50), message);

		if (GUI.Button(new Rect(Screen.width - 100, 10, 100, 30), "Recognize")) {

			recognized = true;

			Gesture candidate = new Gesture(points.ToArray());
			Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

			message = gestureResult.GestureClass + " " + gestureResult.Score;
		}

		GUI.Label(new Rect(Screen.width - 200, 150, 70, 30), "Add as: ");
		newGestureName = GUI.TextField(new Rect(Screen.width - 150, 150, 100, 30), newGestureName);

		if (GUI.Button(new Rect(Screen.width - 50, 150, 50, 30), "Add") && points.Count > 0 && newGestureName != "") {
			string path = Application.dataPath+"/Resources/Gestures";
			string fileName = String.Format("{0}/{1}-{2}.xml", path, newGestureName, DateTime.Now.ToFileTime());

			//#if !UNITY_WEBPLAYER
			GestureIO.WriteGesture(points.ToArray(), newGestureName, fileName);
			//#endif

			trainingSet.Add(new Gesture(points.ToArray(), newGestureName));

			newGestureName = "";
		}
	}
}
