using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using ShorthandDebug;
using UnityEngine.UI;
using PDollarGestureRecognizer;

public class DemoOrig : MonoBehaviourDebug {
	[SerializeField] RectTransform gestureDrawArea;
	[SerializeField] bool axis=false;
	[SerializeField] float axisSpeed=2;
	public Transform gestureOnScreenPrefab;

	private List<Gesture> trainingSet = new List<Gesture>();

	private List<Point> points = new List<Point>();
	private int strokeId = -1;

	private Vector3 virtualKeyPosition = Vector2.zero;
	private Rect drawArea = new Rect();

	private RuntimePlatform platform;
	private int vertexCount = 0;

	private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
	private LineRenderer currentGestureLineRenderer;

	private LineRenderer drawnGestureLinerenderer;

	bool populateGestureSelection;
	bool hitBtnRecognize=false;
	//GUI
	bool allPointsRepeating = true; //if all of the points repeat, then is true and considers the point a tap
	bool pointRepeatingException=true;
	bool axisDown=false;
	bool axisUp = false;
	private string message;
	private bool recognized;

	private string newGestureName = "";

	float areaX=0f;
	float areaY=0f;
	private float[] dS4TouchLength = new float[2] { 0.1919f, 0.0941f };

	[SerializeField]Vector2 direction;

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
		return DrawContains (virtualKeyPosition);
//		float margin = (drawArea.x * 0.05f);
//		Rect r = new Rect (drawArea.x + drawArea.x + margin, drawArea.y + margin, drawArea.width - margin *2, drawArea.height - margin*2);
//		return r.Contains (new Vector2(virtualKeyPosition.x+areaX-1,virtualKeyPosition.y+areaY-1));
	}
	bool DrawContains(Vector2 _direction){
		float marginX = (drawArea.x * 0.05f);
		float marginY = (drawArea.y * 0.05f);
		Rect r = new Rect (drawArea.x + drawArea.x + marginX, drawArea.y + marginY, drawArea.width - marginX *2, drawArea.height - marginY*2);
//		if (Input.GetKey (KeyCode.Z)) {
//			//bool jerry= r.Contains (new Vector2(virtualKeyPosition.x+areaX-1,virtualKeyPosition.y+areaY-1));
//			float t = Input.GetAxis ("Vertical");
//			l.g ("2@e2");
//			l.g ("WDW{0}", virtualKeyPosition);
//		}
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
//	bool GetAxisDown(){
//		
//			if ((axisDown == false) && (Input.GetAxisRaw ("Vertical") != 0 || Input.GetAxisRaw ("Horizontal") != 0)) {
//				return true;
//			} else if(axisDown == true) {
//				l.g ("ax don{0} , vertical: {1} , hori: {2} ", axisDown,(Input.GetAxisRaw ("Vertical")!=0),(Input.GetAxisRaw ("Horizontal")!=0));
//				Timer.instance.SetVariable (this, "axisDown", false, 1.5f, id: "ResetAxisDown");//set axisDown to false on a delay if there is not input
//				return false;
//			}
//
//		return false;
//	}
	void FindVirtualKey(){
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
					if (direction.Equals(Vector2.zero)) {
						direction = Input.mousePosition;
//						if (Input.GetKey (KeyCode.Z)) {
//							//bool jerry= r.Contains (new Vector2(virtualKeyPosition.x+areaX-1,virtualKeyPosition.y+areaY-1));
//							float t = Input.GetAxis ("Vertical");
//							l.g ("2@e2");
//							l.g ("WDW{0}", virtualKeyPosition);
//						}
					} 
					if(GetVKeys) {
						//turn axis into onscreen coord
						var tmpDir = new Vector2(Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
						if (tmpDir.magnitude > 1)//get rid of diagonal speed increase
							tmpDir = tmpDir.normalized;

						var toAddTmpDir = new Vector2 (direction.x + (tmpDir.x*axisSpeed),
							direction.y +  (tmpDir.y *axisSpeed));
						
						//make official if within bounds
						if (DrawContains (toAddTmpDir)) {
							direction = toAddTmpDir;
							virtualKeyPosition = (Vector3)direction;
							l.g ("axis hit");
						}
					}
				}
			}else if (Input.GetMouseButton(0)) {
				//this needs
				virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			}
		}
	}
	bool GetVKeysUp{
		get{ 
			if (axis) {
				if (axisUp = false && (Input.GetAxis ("Vertical") == 0 && Input.GetAxis ("Horizontal") == 0)) {
					axisUp = true;
					return true;
				} else if (axisUp == true && !GetVKeys) {
					Timer.instance.SetVariable (this, "axisUp", false, 1.5f, id: "ResetAxisUp");//set axisDown to false on a delay if there is not input
					return false;
				}
				return false;
			} else {
				return Input.GetMouseButtonUp (0);
			}
		}
	}
	bool GetVKeysDown{get{ 
		if (axis) {
//				l.g("axisDown: {0} , vertical: {1} , hori: {2} ", axisDown,(Input.GetAxisRaw ("Vertical")),(Input.GetAxisRaw ("Horizontal")));
			if ((axisDown == false) && (Input.GetAxisRaw ("Vertical") != 0 || Input.GetAxisRaw ("Horizontal") != 0)) {
				axisDown = true;
				return true;
			} else if(axisDown == true && GetVKeys) {
//				l.g ("ax don{0} , vertical: {1} , hori: {2} ", axisDown,(Input.GetAxisRaw ("Vertical")!=0),(Input.GetAxisRaw ("Horizontal")!=0));
				Timer.instance.SetVariable (this, "axisDown", false, 1.5f, id: "ResetAxisDown");//set axisDown to false on a delay if there is not input
				return false;
			}
			return false;
		} else {
			return Input.GetMouseButtonDown (0);
		}
	}}
	bool GetVKeys{get{ 
		if (axis) {
			return (Input.GetAxis ("Vertical") != 0 || Input.GetAxis ("Horizontal") != 0);
		} else {
			return Input.GetMouseButton (0);
		}
	}}
	void Update () {
		if (currentGestureLineRenderer!=null && (  GetVKeysUp  ) && !hitBtnRecognize) {
			l.g ("recognise from update");
			Recognize ();
			Timer.instance.SetVariable (this, "recognized", true, 2f,id:"ResetRecognizeWait");
		}
		FindVirtualKey ();
		if (DrawContains ()) {
			if (GetVKeysDown) {

				if (recognized) {
					ResetPoints ();
				}
				++strokeId;

				Transform tmpGesture = Instantiate (gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
				currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer> ();

				gestureLinesRenderer.Add (currentGestureLineRenderer);

				vertexCount = 0;
			}
			if (GetVKeys && currentGestureLineRenderer != null) {//&& (vertexCount<34)
				points.Add (new Point (virtualKeyPosition.x, virtualKeyPosition.y, strokeId));
				if (points.Count > 1) {
					int i = points.Count - 1;
					bool tmp = (points [i].Equals (points [i - 1]));
//					l.g("!!!X:  {0} | {1}  , Y:  {2} | {3}  , Y:  {4} | {5}  equals? {6}",points[i].X,points[i-1].X,points[i].Y,points[i-1].Y,points[i].StrokeID,points[i-1].StrokeID, (points [i].Equals (points [i - 1])));
					allPointsRepeating = (allPointsRepeating == true && tmp);//if any don't match, then is a gesture
					//l.g ("");
				} else if (allPointsRepeating) {
					allPointsRepeating = false;
				}
				currentGestureLineRenderer.SetVertexCount (++vertexCount);
				currentGestureLineRenderer.SetPosition (vertexCount - 1, Camera.main.ScreenToWorldPoint (new Vector3 (virtualKeyPosition.x, virtualKeyPosition.y, 10)));
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
			l.g ("Recognize",points.Count);
			//string.Format ("dog{0}", points.Count);
			Gesture candidate = new Gesture (points.ToArray ());//transofrms array of any length into a "gesture", producing a an array the size of the sampling resolution  
			Result gestureResult = PointCloudRecognizer.Classify (candidate, trainingSet.ToArray ());

			message = gestureResult.GestureClass + " " + gestureResult.Score;
		} else {
			//l.g ("All same");
		}
	}
	void OnGUI() {

		float screenGap = Screen.width/Screen.height;

		float width = (dS4TouchLength [0]*3f)*(1+(Screen.height));//Screen.width - Screen.width / 3;
		float height = (dS4TouchLength [1]*3f)*(1+(Screen.height));//Screen.width - Screen.width / 3;//Screen.height - Screen.height / 3;

//		float width = (100+((float)dS4TouchLength [0]*0.01f))*(1+(Screen.width*0.005f));//Screen.width - Screen.width / 3;
//		float height = (100+((float)dS4TouchLength [1]*0.01f))*(1+(Screen.height*0.005f));//Screen.width - Screen.width / 3;//Screen.height - Screen.height / 3;
//
//		areaX = (Screen.width *0.5f) - ((width * 0.5f)+screenGap);
//		areaY = (Screen.height - (Screen.height * 0f)) - (height+screenGap);
		areaX = (Screen.width *0.5f) - ((width * 0.5f)+screenGap);
		areaY = (Screen.height - (Screen.height * 0f)) - (height+screenGap);
		//drawArea = gestureDrawArea.rect;
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
