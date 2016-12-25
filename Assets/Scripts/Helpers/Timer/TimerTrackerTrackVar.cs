using UnityEngine;
using System.Collections;

public class TimerTrackerTrackVar : TimerTrackerBase {

	object val;

	public void Init(object _creator, float _time, string _variableToSet, object _val, string _id, string _display)
	{
		creator = _creator;
		time = _time;
		executor = _variableToSet;
		init = true; //just making sure no funny business goes on
		val = _val;
		base.InitIDandDisplay (_id,_display);

	}
	
	// Update is called once per frame
	override public void Update () {
	//check the field passed value against that of val
		//iif true execute
	}


}
