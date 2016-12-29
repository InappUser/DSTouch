using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using ShorthandDebug;
public class Timer
{
	

	private  BiDictionary<TimerTrackerBase,string> timerTrackerBiDicObjectPool = new BiDictionary<TimerTrackerBase,string>();
	//private List<TimerTrackerBase> timerTrackerObjectPool = new List<TimerTrackerBase>();
	private static Timer thisInstance;
	//public static implicit operator bool(Timer t){return t == null;}
	private Timer(){}
	public static Timer instance{
		get{
			if (thisInstance == null) { //initializing Timer class
				thisInstance = new Timer();
				//timerTrackerObjectPool = new IterativeBehaviourPool<TimerTrackerBase> (timerTrackerObjectPool,0,null);
				//make my own version of this at some point
//				GameStateManager.instance.GetState(GameState.GAMEPLAY_PAUSE).onStateActivate += Timer.instance.PauseTimers;//register with pause state
//				GameStateManager.instance.GetState(GameState.GAMEPLAY_PAUSE).onStateDeactivate += Timer.instance.UnPauseTimers;
			}
			return thisInstance;
		}
		private set{}
	}
	void CheckIfAlreadyExists(ref float _time, ref string _id, ref bool _ignoreMultipleCalls, out TimerTrackerBase trackerToReturnEarly)
	{
		if (_id != "" && timerTrackerBiDicObjectPool.ContainsValue (_id)) {
			if (_ignoreMultipleCalls) {
				l.g ("ignore! {0}",_id);
				trackerToReturnEarly = timerTrackerBiDicObjectPool.GetKey (_id);
			} else if (_time != 0) {
				l.g ("reset!! {0}",_id);
					trackerToReturnEarly = ResetConflictingIDsTime (_id, _time);
			} else {
				l.g ("remove!! {0}",_id);
				RemoveConflictingID (_id);
				trackerToReturnEarly = null;
			}
		} else {
			l.g ("nothing {0}",_id);
			trackerToReturnEarly = null;
		}
	}
    /// <summary>
    /// Sets a variable to a passed value over a certain amount of time
    /// </summary>
    /// <param name="creator">A reference to the object being called from: 'this'.</param>
    /// <param name="variableToSetName">string name of the variable to set.</param>
	/// <param name="val">the value to set the variable to.</param>
	/// <param name="time">The delay before the passed variable's value is changed.</param>
	/// <param name="attachTo">(optional)The gameobject the Timer will be attached to (defaults to creator)</param>
	/// <param name="id">(optional)An ID to show up in the inspector to track which Timer is being used. This ensures that two of the same kind are not attached.</param>
	/// <param name="display">(optional)An name to show up in the inspector to track which Timer is being used. This is purely aesthetic</param>
	public TimerTrackerVarSet SetVariable(object creator, string variableToSetName, object val, float time, GameObject attachTo = null, string id="", string display="", bool ignoreMultipleCalls=false)
	{
		TimerTrackerBase t;
		CheckIfAlreadyExists (ref time, ref id, ref ignoreMultipleCalls, out  t);
		if (t != null) {
			l.g ("return t");
			return (TimerTrackerVarSet)t;
		}

		TimerTrackerVarSet timerTracker = InitTimerTracker<TimerTrackerVarSet>(attachTo==null ? creator : attachTo, id, time);
		timerTracker.Init(creator, time, variableToSetName, val, id, display);
		return timerTracker;
	}
    /// <summary>
    /// Set a method to execute after a designated period of time.
    /// </summary>
    /// <param name="creator">this</param>
    /// <param name="methodName">the name of the method wished to be executed</param>
	/// <param name="methodParams">an array of the paramters to be passed to the passed method</param>
	/// <param name="time">The amount of delay before the method is executed</param>
	/// <param name="attachTo">(optional)The gameobject the Timer will be attached to (defaults to creator)</param>
	/// <param name="id">(optional)An ID to show up in the inspector to track which Timer is being used. This ensures that two of the same kind are not attached.</param>
	/// <param name="display">(optional)An name to show up in the inspector to track which Timer is being used. This is purely aesthetic</param>
	public TimerTrackerMethodEx SetMethod(object creator, string methodName, object[] methodParams, float time, GameObject attachTo = null, string id="", string display="")
    {
		TimerTrackerMethodEx timerTracker = InitTimerTracker<TimerTrackerMethodEx>( attachTo==null ? creator : attachTo);
		timerTracker.Init(creator, methodName, methodParams, time, id, display);
		return timerTracker;
    }

	/// <summary>
	/// Executes the passed method as string on tick. Works for both executing for a passed duration or until condition true. For Latter: 
	/// Pass 0 or leave out argument for "time" and make return type "bool" for passed method (if method returns true then it will stop executing)
	/// </summary>
	/// <param name="creator">Creator.</param>
	/// <param name="methodName">the name of the method wished to be executed</param>
	/// <param name="methodParams">an array of the paramters to be passed to the passed method as an object array.</param>
	/// <param name="attachTo">(optional) The gameobject the Timer will be attached to (defaults to creator, can set to null if need "time")</param>
	/// <param name="id">(optional)An ID to show up in the inspector to track which Timer is being used. This ensures that two of the same kind are not attached.</param>
	/// <param name="display">(optional)An name to show up in the inspector to track which Timer is being used. This is purely aesthetic</param>
	/// <param name="time">(optional) The amount of delay before the method is executed (for if the method returning true is not enough)</param>
	public TimerTrackerMethodExOnTick SetMethodOnTick(object creator, string methodName, object[] methodParams, GameObject attachTo = null, string id="", string display="", float time=0f)
	{
		TimerTrackerMethodExOnTick timerTracker = InitTimerTracker<TimerTrackerMethodExOnTick>( attachTo==null ? creator : attachTo);
		timerTracker.Init(creator, methodName, methodParams, time, id, display);
		return timerTracker;
	}

	/// <summary>
	/// Executes the passed method as string on tick and another method once first method is "done". Works for both executing for a passed duration or until condition true. For Latter: 
	/// Pass 0 or leave out argument for "time" and make return type "bool" for passed method (if method returns true then it will stop executing)               
	/// </summary>
	/// <param name="creator">Creator.</param>
	/// <param name="methodToExOnTickName">The name of the method wished to be executed on tick </param>
	/// <param name="methodToExOnTickParams">An array of the paramters to be passed to the method to be executed on tick, as an object array.</param>
	/// <param name="methodToExOnCompleteName">The name of the method wished to be executed once the first method has finished</param>
	/// <param name="methodToExOnCompleteParams">An array of the paramters to be passed to the passed method (executing once the first method has finished) as an object array.</param>
	/// <param name="attachTo">(optional) The gameobject the Timer will be attached to (defaults to creator, can set to null if need "time")</param>
	/// <param name="id">(optional)An ID to show up in the inspector to track which Timer is being used. This ensures that two of the same kind are not attached.</param>
	/// <param name="display">(optional)An name to show up in the inspector to track which Timer is being used. This is purely aesthetic</param>
	/// <param name="time">(optional) The amount of delay before the method is executed (for if the method returning true is not enough)</param>
	public TimerTrackerMethodExOnTick SetMethodOnTick(object creator, string methodToExOnTickName, object[] methodToExOnTickParams,
																		string methodToExOnCompleteName, object[] methodToExOnCompleteParams, 
													  GameObject attachTo = null, string id="", string display="", float time=0f)
	{
		TimerTrackerMethodExOnTick timerTracker = InitTimerTracker<TimerTrackerMethodExOnTick>( attachTo==null ? creator : attachTo);
		timerTracker.Init(creator, methodToExOnTickName, methodToExOnTickParams,methodToExOnCompleteName,methodToExOnCompleteParams, time, id, display);
		return timerTracker;
	}

	public void Track(object creator, string variableToTrack, object executeValue)
	{
		
	}
	/// <summary>
	/// Inits each timer tracker.
	/// </summary>
	/// <returns>The timer tracker.</returns>
	/// <param name="_attachTo">Attach to.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	private T InitTimerTracker<T>( object _attachTo, string _id="", float _time=0) where T : TimerTrackerBase
	{
		T t;
		if (_attachTo.GetType () != typeof(GameObject)) { // if not using passed gameobject
			MonoBehaviour m = (MonoBehaviour)_attachTo;
			t = m.gameObject.AddComponent<T> ();
		} else {
			GameObject go = (GameObject)_attachTo;
			t = go.AddComponent<T> ();
		}

//		l.g ("Add tracker | attach to {0}",_attachTo);
		timerTrackerBiDicObjectPool.Add(t,_id);
		//timerTrackerObjectPool.Add (t);
		return t;
//		if (timerTrackerObjectPool == null) {
//			timerTrackerObjectPool = new IterativeBehaviourPool<TimerTrackerBase> (t);
//		} else {
//			timerTrackerObjectPool.
//		}
	}
	public void RemoveTracker(TimerTrackerBase _tracker)
	{
		l.g ("Remove tracker");
		timerTrackerBiDicObjectPool.Remove(_tracker);
		UnityEngine.MonoBehaviour.DestroyImmediate (_tracker);
	}
//	void PauseTimers(GameState _state){
//		Debug.Log ("paused? " + (_state == GameState.GAMEPLAY_PAUSE));
//		PauseAll (true);
//	}
//	void UnPauseTimers(GameState _state){
//		Debug.Log ("paused? " + (_state == GameState.GAMEPLAY_PAUSE));
//		PauseAll (false);}
	TimerTrackerBase ResetConflictingIDsTime(string _id, float _time)
	{
		//l.g ("get tracker");
		TimerTrackerBase t;
		if (timerTrackerBiDicObjectPool.GetKey (_id, out t)) {
			//l.g ("resetTime");
			t.ThisTime = _time;
		}
		return t;
	}
	void RemoveConflictingID(string id)
	{
		//l.g ("Remove conflicting tracker");
		timerTrackerBiDicObjectPool.Remove (id);
	}
//	public void RemoveByID(string id, TimerTrackerBase caller)
//	{
////		for (int i = 0; i < d.Count; i++) {
////			Debug.Log(d.Values[0]);
////		}
//
//		for (int i = 0; i < timerTrackerObjectPool.Count; i++) { 
//			if (timerTrackerObjectPool[i].gameObject == caller.gameObject && //check is on the same object
//				timerTrackerObjectPool [i].GetID () == id && //check if has the right ID
//			    timerTrackerObjectPool [i] != caller) { 
//				//check that there is a caller and this isn't it or that there isn't a caller
//				Debug.Log ("removing " + timerTrackerObjectPool[i].GetDisplay());
//				RemoveTracker(timerTrackerObjectPool[i]);
//			}
//				//				if ((caller != null && temp != caller) || caller==null) {
//		}
////		List<TimerTrackerBase> temp=new List<TimerTrackerBase>();
////		foreach (TimerTrackerBase tt in timerTrackerObjectPool) {
////			if (tt.GetID () == id) {
////				if ((caller != null && temp != caller) || caller==null) {
////					timerTrackerObjectPool.Remove (tt);
////					temp.Add (tt);
////				}
////			}
////		}
////		timerTrackerObjectPool.RemoveAll (x => temp.Contains (x));
//	}
}
