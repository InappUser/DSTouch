using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DS4Api;
using PDollarGestureRecognizer;

public class Demo : MonoBehaviour {

	public Transform gestureOnScreenPrefab;

	private List<Gesture> trainingSet = new List<Gesture>();

	private List<Point> points = new List<Point>();
	private int strokeId = -1;

	private Vector3 virtualKeyPosition = Vector2.zero;
	private Rect drawArea;

	private RuntimePlatform platform;
	private int vertexCount = 0;

	private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
	private LineRenderer currentGestureLineRenderer;

	//GUI
	private string message;
	private bool recognized;
	private string newGestureName = "";
//1919-941 - from Screen.width - Screen.width / 3, Screen.height
	private float[] DS4TouchLength = new float[2] { 1919f, 941f };
    float modifier = .4f;
	DS4Data data;
	DS4 controller;
    int[] lastTouchPos= { -1, -1 };

	void Start () {
        DS4TouchLength[0] *= modifier;
        DS4TouchLength[1] *= modifier;
        print("screen width" + (Screen.width - Screen.width / 3)+" hieght "+ Screen.height +"ds4 width "+ DS4TouchLength[0]+ "hieght"+ DS4TouchLength[1] );
        platform = Application.platform;

        drawArea = new Rect(DS4TouchLength[0]-(DS4TouchLength[0] / 2), DS4TouchLength[1] - (DS4TouchLength[1] / 2), DS4TouchLength[0], DS4TouchLength[1] );

		//Load pre-made gestures
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
		foreach (TextAsset gestureXml in gesturesXml)
			trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

		//Load user custom gestures
		string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
		foreach (string filePath in filePaths)
			trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
	}
	void CheckDS4Data(){

        print("find wiimotes");
        DS4Data tentative = data;
        do
        {
            data = tentative;
            tentative = controller.ReadDS4Data();
        } while (tentative != null);
	}
    bool DS4TouchUsed()
    {
        if (data == null) return false;
        return (DS4Manager.HasWiimote() && data != null && (data.Touches[0, 0] > -1 || data.Touches[0, 1] > -1 || data.Touches[1, 0] > -1 || data.Touches[1, 1] > -1));
    }
    bool DS4TouchDown()
    {
        if (data == null) return false;
        return (data.Touches[0, 0] > -1 && lastTouchPos[0] == -1) || (data.Touches[0, 1] > -1 && lastTouchPos[1] == -1);
    }
    void Update () {

		if(!DS4Manager.HasWiimote()){
			DS4Manager.FindWiimotes();
			
		}else
        {
            controller = DS4Manager.Controllers[0];
			CheckDS4Data();
		}
        //if (data != null) print("data t" + data.Touches[0, 0]);
		//find the controller
		if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touchCount > 0) {
				virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
			}
		} else {
            if (DS4TouchUsed())
            {//if using the touchpad
                virtualKeyPosition = new Vector3(data.Touches[0, 0], -data.Touches[0, 1]+ 941f);
                print("hit");
            }else if (Input.GetMouseButton(0)) {
				virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			}

		}

		if (drawArea.Contains(virtualKeyPosition)) {

			if (DS4TouchDown()) {

                print("get down");
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
			
			if (DS4TouchUsed()) {
                print("add point");
				points.Add(new Point(virtualKeyPosition.x, -virtualKeyPosition.y, strokeId));

				currentGestureLineRenderer.SetVertexCount(++vertexCount);
				currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));
			}
		}
        if (data != null)
        {
            lastTouchPos[0] = data.Touches[0, 0];
            lastTouchPos[1] = data.Touches[0, 1];
        }
    }

	void OnGUI() {

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

			string fileName = String.Format("{0}/{1}-{2}.xml", Application.persistentDataPath, newGestureName, DateTime.Now.ToFileTime());

			#if !UNITY_WEBPLAYER
				GestureIO.WriteGesture(points.ToArray(), newGestureName, fileName);
			#endif

			trainingSet.Add(new Gesture(points.ToArray(), newGestureName));

			newGestureName = "";
		}
	}
}
