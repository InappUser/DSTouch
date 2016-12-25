using System.Reflection;
using System;

public class TimerTrackerMethodExOnTick : TimerTrackerMethodEx
{
	object[] methodToExOnTickParams; //storing all the parameters
	MethodInfo methodToExOnTick;
	bool usingTrueState = false;

	public new void Init(object _creator, string _methodToExOnTick, object[] _methodToExOnTickParams, float _time, string _id, string _display)
    {
		InitVars (_creator, _methodToExOnTick, _methodToExOnTickParams, _time, _id, _display);
    }
	public void Init(object _creator, string _methodToExOnTick, object[] _methodToExOnTickParams, string _methodToExOnTrue, object[] _methodToExOnTrueParams, float _time, string _id, string _display)
	{
		InitVars (_creator, _methodToExOnTick, _methodToExOnTickParams, _time, _id, _display);
		executor = _methodToExOnTrue;
		methodParams = _methodToExOnTrueParams;
	}
	/// <summary>
	/// Inits the variables and ensures passed method parameters match passed method .
	/// </summary>
	/// <param name="__creator">Creator.</param>
	/// <param name="__methodToExOnTick">Method to ex on tick.</param>
	/// <param name="__methodParams">Method parameters.</param>
	/// <param name="__time">Time.</param>
	void InitVars(object __creator, string __methodToExOnTick, object[] __methodParams, float __time, string __id, string __display)
	{
		base.InitIDandDisplay (__id, __display);
		creator=__creator;
		methodToExOnTickParams = __methodParams;
		methodToExOnTick = GetMethod (__creator, __methodToExOnTick);
		//ensure parameters are correct
		CheckMethodParams (ref methodToExOnTick, ref methodToExOnTickParams);
		usingTrueState = (methodToExOnTick.ReturnType == typeof(bool) && (__time==0));
		time = __time;
	}
	public override void Update ()
	{
		//if not using true state (using the method's bool return as it being done) then use the timer to determine whether to finish executing method (held in base.update)
		if (!usingTrueState) {
			methodToExOnTick.Invoke (creator, methodToExOnTickParams);
			base.Update ();
		} else if((bool)methodToExOnTick.Invoke (creator, methodToExOnTickParams)){//else check if value1 == value2
			//Debug.Log"executor: "+executor +" true =\"\""+(executor ==null));
			if (executor !=null) {//testing whether this needs to execute a method on finish by checking if "executor" (what's used for executing a method 
				//in the inherited class is empty - if it is, there's no method to execute, so run normal inherited "Execute" method (via BaseExecute) 
				base.Execute ();//if it isn't then run directly inherited "Execute" which will execute the method held in "executor" string variable 
			} else {
				base.BaseExecute ();
			}
		}
	}
	new void CheckMethodParams(ref MethodInfo _method, ref object[] _methodParams)
	{
		//if void method  return type and passing null, don't do other checks
		if (_method.ReturnType == typeof(void)) {
			UnityEngine.Debug.LogWarning ("Know that this method will be executed forever");//potential use case if have reference to this timertracker
		}
		base.CheckMethodParams (ref _method, ref _methodParams);
	}
#if UNITY_EDITOR
    //Showing an ID, if one is provided
    [UnityEditor.CustomEditor(typeof(TimerTrackerMethodExOnTick))]
	public class TimerTrackerMethodExOnTickEditor: TimerTrackerBaseEditor{}
#endif
}
