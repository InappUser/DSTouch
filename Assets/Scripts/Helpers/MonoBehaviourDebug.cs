using UnityEngine;
using System.Collections;

public abstract class MonoBehaviourDebug : MonoBehaviour {

	public void log(object _logstr, params object[] objs)
	{
		Debug.Log (string.Format(_logstr.ToString(),objs));
	}
}
namespace OtherShorthandDebug{
	public static class l
	{
		public static void g(object _logstr, params object[] objs)
		{
			Debug.Log (string.Format(_logstr.ToString(),objs));

		}
	}
}
