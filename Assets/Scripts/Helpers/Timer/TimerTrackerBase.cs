using UnityEngine;
using System.Reflection;

public abstract class TimerTrackerBase : MonoBehaviour
{
    protected BindingFlags all = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public  | BindingFlags.IgnoreCase  | BindingFlags.Static;
    protected string executor;
    protected object creator;
   [SerializeField]
	protected float time;
    protected bool init = false;
	protected string id="";
	protected string display="";
	public bool Paused{set{ this.enabled = !value;}}//is there mayebe a way of just disabling a method so it doesn't do anything rather than have it check a bool on every call?
	/// <summary>
	/// Destroy this Tracker.
	/// </summary>
	public void Destroy(){
		Timer.instance.RemoveTracker (this);
	}
	public string GetID(){return id;}
	public string GetDisplay(){return display;}
	void Start()
	{
		RemoveConflictingIDs ();
	}
	protected void InitIDandDisplay(string _id, string _display)
	{
		id = _id;
		display = _display;
	}
	/// <summary>
	/// Update for basic Timer implementation.
	/// </summary>
	virtual public void Update()
    {
        time -= Time.deltaTime;
        if (time <= 0 && init)
        {
            Execute();
            init = false;
        }
    }
	virtual protected void Execute() 
    {
		Timer.instance.RemoveTracker(this);
    }

	protected void RemoveConflictingIDs()
	{
		if (id != "") {//don't remove everything without a passed ID
			Timer.instance.RemoveByID (id, this);
		}
		//if float time !=0
		//get instance by ID, reset timer and remove this? pointless? think if I were going to be doing this kind of logic it should be pior to "this" object creation
	}
    protected bool CheckLogTypeError(System.Type assignee, System.Type assigner)
    {
		//Debug.Log"assignee: "+assignee+"Assigner: "+assigner);
		if (!assignee.IsAssignableFrom(assigner) && assigner != typeof(System.Reflection.Missing))
        {
			Debug.LogError("Attempted assignment of incompatible type to field: " + assigner + " to " + assignee +". If assigning array to object[] ensure to use \"new object[]{\'array\'}\", not \"object[] o = \'array\'\"!");//making sure incorrect typing doesn't go unnoticed
        }
		return true;
    }
	/// <summary>
	/// Gets passed method, considering methods within an obejct's base class.
	/// </summary>
	/// <returns>The method.</returns>
	/// <param name="_creator">Creator.</param>
	/// <param name="_methodName">Method name.</param>
	protected MethodInfo GetMethod(object _creator, string _methodName)
	{
		MethodInfo method = _creator.GetType ().GetMethod (_methodName, all);
		//print ("!!!method" + method.Name);
		if (method == null) {
			method = _creator.GetType ().BaseType.GetMethod (executor, all);
		}
		return method;
	}
	/// <summary>
	/// Gets passed method, considering methods within an obejct's base class while ensuring the correct override method is called.
	/// </summary>
	/// <returns>The method.</returns>
	/// <param name="_creator">Creator.</param>
	/// <param name="_methodName">Method name.</param>
	/// <param name="types">Types.</param>
	protected MethodInfo GetMethod(object _creator, string _methodName, System.Type[] types)
	{
		MethodInfo method = _creator.GetType ().GetMethod (_methodName, all, null, types, null);
		//print ("method!" + method.Name);
		if (method == null) {
			method = _creator.GetType ().BaseType.GetMethod (executor, all, null, types, null);
			MethodInfo[] methods = _creator.GetType ().GetMethods(all);
			//int i=0;
			foreach(MethodInfo m in methods)
			{
				//i++;
				if(m.Name == executor){
					//print ("got m"+m.Name);

					return m;
				}

//				if (i++ == 13) {
//					string s = m.Name;
//					print (i + " " + s +" toGet:"+executor +"comp"+(s==executor));
//				}
				//print ("M" + i++ + ": " + m); 
				//methodstrings += ", "+m.Name;
			}
			//print ("can't find: "+methodstrings+" in "+_creator+ (method == null));
			//Debug.Log"type "+_creator.GetType()+" basetype "+_creator.GetType().BaseType);
		}
		return method;
	}

#if UNITY_EDITOR
    /// <summary>
    /// Editor to show the ID of a timer, if one is provided.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(TimerTrackerBase))]
	public class TimerTrackerBaseEditor: UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			TimerTrackerBase tracker = (TimerTrackerBase)target;
			if (tracker.id !="") {

				UnityEditor.EditorGUILayout.LabelField ("ID: " + tracker.id, UnityEditor.EditorStyles.boldLabel);
			}
			if (tracker.display !="") {
				
				UnityEditor.EditorGUILayout.LabelField ("Display: " + tracker.display, UnityEditor.EditorStyles.boldLabel);
			}
			DrawDefaultInspector ();
		}

	}
#endif
}