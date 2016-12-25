using System.Reflection;
using System;

public class TimerTrackerMethodEx : TimerTrackerBase
{
	protected object[] methodParams; //storing all the parameters

	public void Init(object _creator, string _executor, object[] _methodParams, float _time, string _id, string _display)
    {
        creator = _creator;
        time = _time;
        executor = _executor;
        init = true; //just making sure no funny business goes on
		methodParams = _methodParams;
		base.InitIDandDisplay (_id,_display);
    }
	/// <summary>
	/// Ensure that the parameters passed are compatible with what the passed method expects
	/// </summary>
	/// <param name="_method">Method.</param>
	public void CheckMethodParams(ref MethodInfo _method, ref object[] _methodParams)
	{
		//if void method  return type and passing null, don't do other checks
		UnityEngine.Debug.Log("display:"+display+", id:"+id);
		ParameterInfo[] actualParams = _method.GetParameters ();
		bool check = (actualParams.Length > 0); //? (actualParams.GetValue(0).GetType()!=typeof(void)) : ;
		if(methodParams != null && check){
			//check passed params are compatible

			if (actualParams.Length != _methodParams.Length) {
			//check if the param is an array
				bool isArray=false;
				for (int i = 0; i < actualParams.Length; i++) {
					if (CheckLogTypeError(_methodParams [i].GetType (), actualParams [i].ParameterType)){//.GetCustomAttributes (typeof(ParamArrayAttribute))) {
						//Debug.Log"is array? "+_methodParams [i].GetType () +" m name "+_method.Name);
						isArray = true;//_methodParams [i].GetType ().IsArray;
						break;
					}
				}
				if (!isArray) {
					UnityEngine.Debug.LogError ("Wrong number of arguments: " + actualParams.Length + " actual, " + _methodParams.Length + " passed." +
					" If passing array, use \"params\" keyword.");
				}
			}
			//don't want to be doing this if it is suspected that the argument amounts don't match paramater amounts
			for (int i = 0; i < actualParams.Length; i++) {
				CheckLogTypeError (actualParams [i].ParameterType, _methodParams [i].GetType ());
			}
		} 
	}
    /// <summary>
    /// Execute the passed method - called when inherited update decides it's time
    /// </summary>
	override protected void Execute()
    {
		MethodInfo method;
        try{
			if(methodParams!=null){
				Type[] types = Array.ConvertAll<object, Type>(methodParams, x=>x.GetType());  //get all the types from the method params
				method = GetMethod(creator, executor,types);//explicitly stating which overload method to use

				ParameterInfo[] actualParams = method.GetParameters ();
				string p="";
				for (int i = 0; i < actualParams.Length; i++) {p+=", "+actualParams[i].ParameterType;}
				UnityEngine.Debug.Log(string.Format("expected params length:{0}, type {1}. params passed :{2} ",actualParams.Length,p,methodParams[0]));
			}else{
				method = GetMethod(creator, executor);

			}
			CheckMethodParams(ref method, ref methodParams);
			//object[] o  = new object[]{0,""};
		//object o = new object(); 
		method.Invoke(creator, methodParams);
			//UnityEngine.Debug.Log("execute");
		}catch (Exception e){
			UnityEngine.Debug.LogError("Is the method for {0} "+gameObject.name+" correct? display:"+display+", id:"+id+" error: "+e);
        }
		BaseExecute ();
    }
	protected void BaseExecute() //bad practice, right? - violating inheritance rules (want the original "Execute" in TimerTrackerMethoExOnTick)
	{
		base.Execute ();
	}
#if UNITY_EDITOR
    //Showing an ID, if one is provided
    [UnityEditor.CustomEditor(typeof(TimerTrackerMethodEx))]
	public class TimerTrackerMethodExEditor: TimerTrackerBaseEditor{}
#endif
}
