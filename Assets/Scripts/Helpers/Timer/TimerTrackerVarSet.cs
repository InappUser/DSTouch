using UnityEngine;
using System.Collections;
using System.Reflection;

public class TimerTrackerVarSet : TimerTrackerBase
{
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
	/// <summary>
	/// Set the passed variable to the passed value, executed when timer has reached 0.
	/// </summary>
    override protected void Execute()
    {
        try
        {
            FieldInfo fieldToSet = creator.GetType().GetField(executor, all);
            CheckLogTypeError(fieldToSet.GetValue(creator).GetType(), val.GetType());
            fieldToSet.SetValue(creator, val);
        }
        catch
        {
            //good amount of redundancy. Will look into consolidating this.
            PropertyInfo propertyToSet = creator.GetType().GetProperty(executor, all);
            CheckLogTypeError(propertyToSet.GetValue(creator, null).GetType(), val.GetType());
            propertyToSet.SetValue(creator, val, null);
        }
        base.Execute();

    }

#if UNITY_EDITOR
    //Showing an ID, if one is provided
    [UnityEditor.CustomEditor(typeof(TimerTrackerVarSet))]
	public class TimerTrackerVarSetEditor: TimerTrackerBaseEditor{}
#endif
}
