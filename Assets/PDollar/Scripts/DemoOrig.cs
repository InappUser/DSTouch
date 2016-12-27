using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ShorthandDebug;
using PDollarGestureRecognizer;

public class DemoOrig : MonoBehaviourDebug {

	[SerializeField] bool axis=false;
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

	private LineRenderer drawnGestureLinerenderer;

	bool populateGestureSelection;
	bool hitBtnRecognize=false;
	//GUI
	bool allPointsRepeating = true; //if eall of the points repeat, then is true and considers the point a tap
	bool pointRepeatingException=true;
	private string message;
	private bool recognized;
	private string newGestureName = "";

	float height=0f;
	float areaX=0f;
	float areaY=0f;
	private int[] dS4TouchLength = new int[2] { 1919, 941 };

	Vector2? direction;

	void Start () {

		platform = Application.platform;
		//get ratio between touchpad length
		//int gcd = GCD(dS4TouchLength);
		//Debug.Log (string.Format("GDC between {0} and {1} = {2}",dS4TouchLength[0],dS4TouchLength[1],gcd));
		//Load pre-made gestures
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("Gestures");//("GestureSet/10-stylus-MEDIUM/");
		foreach (TextAsset gestureXml in gesturesXml)
			trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

		TextAsset[] templates = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
		foreach (TextAsset gestureXml in templates)
			trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
		
		//Load user custom gestures
		string[] filePaths = Directory.GetFiles(Application.dataPath+"/Gestures", "*.xml");
		foreach (string filePath in filePaths)
			trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
	}
	bool DrawContains(){
		Rect r = new Rect (drawArea.x + 5, drawArea.y + 5, drawArea.width - 7, drawArea.height - 7);
		return r.Contains (new Vector2(virtualKeyPosition.x+areaX-1,virtualKeyPosition.y+areaY-1));
	}
	bool DrawContains(Vector2 _direction){
		Rect r = new Rect (drawArea.x + 5, drawArea.y + 5, drawArea.width - 7, drawArea.height - 7);
		return r.Contains (new Vector2(_direction.x+areaX-1,_direction.y+areaY-1));
	}
	void PopulateGestureSelection()
	{	int i = 0;
		GUIStyle guis = new GUIStyle();
		guis.fontSize = 12;
		foreach (Gesture gesture in trainingSet) {
			if (GUI.Button (new Rect(0,i,gesture.Name.Length*20, 20),gesture.Name,guis)) {
				if (drawnGestureLinerenderer != null) {
					drawnGestureLinerenderer.SetVertexCount (0);
					Destroy (drawnGestureLinerenderer);
				}
				//ResetPoints ?
				Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
				drawnGestureLinerenderer = tmpGesture.GetComponent<LineRenderer>();
				Vector3[] ps = System.Array.ConvertAll<Point,Vector3>(gesture.Points, x=>new Vector3(x.X*3f,x.Y*3f));
				drawnGestureLinerenderer.SetVertexCount (ps.Length );
				//drawnGestureLinerenderer.SetPositions (ps);

				points.AddRange (gesture.Points);
				for (int j = ps.Length-1; j> 0; j--) {
					//Debug.Log (ps [(j==0)?j:j-1]);
					drawnGestureLinerenderer.SetPosition (j, ps [j]);
				}
				populateGestureSelection = false;
			}

			i += 20;
		}


	} 
	void Update () {
		if (currentGestureLineRenderer!=null && Input.GetMouseButtonUp (0) && !hitBtnRecognize) {
			//Timer.instance.SetMethod(this,"Recognize",null,2f,id:"RecognizeWait");
			Recognize ();
			Timer.instance.SetVariable (this, "recognized", true, 2f,id:"ResetRecognizeWait");

		}
		if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touchCount > 0) {
				virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
			}
		} else {
			if (axis) {
				//if(lastVirtual key position == null)
				//set position to mouse position- in final it doesn't really matter the positon - though if I can figure this out it will be easier
				//get last virtual key position
				//add axis
				if (DrawContains(Input.mousePosition)) {
					float x, y;
					if (direction == null) {
						direction = Input.mousePosition;
	
					}
					x = direction.Value.x * Input.GetAxis ("Horizontal");
					y = direction.Value.y * Input.GetAxis ("Vertical");

					direction = new Vector2 (x, y);
					direction *= Time.deltaTime;

					//transform axis to on screen coordinate


					if (DrawContains (direction.Value)) {
						//SetCursor virtual key as direction?
					}
				}
			}else if (Input.GetMouseButton(0)) {
				//this needs
				virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			}
		}

		if (DrawContains()) {
			if (Input.GetMouseButtonDown(0)) {

				if (recognized) {

					ResetPoints ();
				}

				++strokeId;

				Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
				currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();

				gestureLinesRenderer.Add(currentGestureLineRenderer);

				vertexCount = 0;
			}
			if (Input.GetMouseButton(0) && currentGestureLineRenderer!=null ) {//&& (vertexCount<34)
				points.Add(new Point(virtualKeyPosition.x, virtualKeyPosition.y, strokeId));
				if (points.Count > 1) {
					int i = points.Count - 1;
					bool tmp = (points [i].Equals (points [i - 1]));
//					l.g("!!!X:  {0} | {1}  , Y:  {2} | {3}  , Y:  {4} | {5}  equals? {6}",points[i].X,points[i-1].X,points[i].Y,points[i-1].Y,points[i].StrokeID,points[i-1].StrokeID, (points [i].Equals (points [i - 1])));
					allPointsRepeating = (allPointsRepeating == true && tmp);//if any don't match, then is a gesture
					l.g ("");
				} else if(allPointsRepeating){
					allPointsRepeating = false;
				}
				currentGestureLineRenderer.SetVertexCount(++vertexCount);
				currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));
			}
		}
		if (axis) {
		//if getaxis down
			//if recognised
				//reset recognized and stroke id(-1)
				//clear points
				//destroy linerenderer
			++strokeId;

			Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
			currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();

			gestureLinesRenderer.Add(currentGestureLineRenderer);

			vertexCount = 0;

			if (Input.GetAxis ("Vertical") != 0) {
				
			}
		}
	}

	void ResetPoints()
	{
		l.g ("reset points");
		recognized = false;
		strokeId = -1;

		points.Clear();
		allPointsRepeating = true;
		foreach (LineRenderer lineRenderer in gestureLinesRenderer) {

			lineRenderer.SetVertexCount(0);
			Destroy(lineRenderer.gameObject);
		}

		gestureLinesRenderer.Clear();
	}
	void Recognize()
	{
		if (!allPointsRepeating) {
			l.g ("not All same {0}",points.Count);
			//string.Format ("dog{0}", points.Count);
			Gesture candidate = new Gesture (points.ToArray ());//transofrms array of any length into a "gesture", producing a an array the size of the sampling resolution  
			Result gestureResult = PointCloudRecognizer.Classify (candidate, trainingSet.ToArray ());

			message = gestureResult.GestureClass + " " + gestureResult.Score;
		} else {
			l.g ("All same");
		}
	}
	void OnGUI() {

		float screenGap = Screen.width/Screen.height;
		float width = (100+((float)dS4TouchLength [0]*0.01f))*(1+(Screen.width*0.005f));//Screen.width - Screen.width / 3;
		height = (100+((float)dS4TouchLength [1]*0.01f))*(1+(Screen.height*0.005f));//Screen.width - Screen.width / 3;//Screen.height - Screen.height / 3;
		areaX = (Screen.width *0.5f) - ((width * 0.5f)+screenGap);
		areaY = (Screen.height - (Screen.height * 0f)) - (height+screenGap);
		drawArea.Set(areaX, areaY, width, height);

		PopulateGestureSelection ();


		GUI.Box(drawArea, "Draw Area");

		GUI.Label(new Rect(10, Screen.height - 40, 500, 50), message);

		if (GUI.Button(new Rect(Screen.width - 100, 10, 100, 30), "Recognize")) {
			hitBtnRecognize=true;
			Recognize();
			Timer.instance.SetVariable (this, "hitBtnRecognize", false, 1.2f, id:"BtnRecognizeFalse");
		}

		GUI.Label(new Rect(Screen.width - 280, 150, 70, 30), "Add or display ");
		newGestureName = GUI.TextField(new Rect(Screen.width - 220, 150, 100, 30), newGestureName);

		if (GUI.Button(new Rect(Screen.width - 110, 150, 50, 30), "Add") && points.Count > 0 && newGestureName != "") {
			string path = Application.dataPath+"/Gestures";
			string fileName = String.Format("{0}/{1}-{2}.xml", path, newGestureName, DateTime.Now.ToFileTime());

			//#if !UNITY_WEBPLAYER
			GestureIO.WriteGesture(points.ToArray(), newGestureName, fileName);
			//#endif

			trainingSet.Add(new Gesture(points.ToArray(), newGestureName));

			newGestureName = "";
		}
		if (GUI.Button(new Rect(Screen.width - 50, 150, 50, 30), "Remove")) {
			if (drawnGestureLinerenderer != null) {
				drawnGestureLinerenderer.SetVertexCount (0);
				Destroy (drawnGestureLinerenderer);
			}
		}
	}
//	static int GCD(int[] nums)
//	{
//		return nums.Aggregate (GCD);
//	}
//	static int GCD(int a, int b)
//	{
//		return b == 0 ? a : GCD (b, a%b);
//	}
}
