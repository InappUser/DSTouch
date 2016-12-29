using UnityEngine;
using System.Collections;

public class BiDictionary<TKey,TValue> : DictionaryBase {

	public IDictionary GetByValue{get{ return getByValue;}}
	IDictionary getByValue = new Hashtable();

	public void Add(TKey key, TValue value)
	{
		if (this.Dictionary.Contains (key.GetHashCode())) {
			this.Dictionary.Remove (key);
		}
		this.Dictionary.Add(key, value);
	}

	public void Remove(TKey key)
	{
		this.Dictionary.Remove(key);
	}
	public void Remove(TValue value)
	{
		TKey t = GetKey (value);
		this.Dictionary.Remove (t);
	}
	public TKey[] Keys{
		get{ 
			Debug.Log ("dictionary? "+this.Dictionary == null);
			TKey[] tmp = new TKey[this.Dictionary.Count];
			this.Dictionary.Keys.CopyTo(tmp,0);
			return tmp;
		}
	}
	//	public bool GetKeys(out TKey[] keys){
	////		if (lookup != null)
	////		{
	////			if (value.Equals((TValue)lookup)) {
	////				this.Dictionary.Remove (key);
	////				return true;
	////			} 
	////		}
	////		return false;
	//	}
	public int KeyCount{get{ return this.Dictionary.Keys.Count;}}
	public bool RemoveMatching(TKey key, TValue value)
	{
		object lookup = Dictionary[key];
		if (lookup != null)
		{
			if (value.Equals((TValue)lookup)) {
				this.Dictionary.Remove (key);
				return true;
			} 
		}
		return false;
	}
	public bool ContainsMatch(TKey key, TValue value)
	{
		object lookup = this.Dictionary[key];
		if (lookup != null)
		{
			if (value.Equals((TValue)lookup)) {
				return true;
			} 
		}
		return false;
	}
	public bool ContainsValue(TValue value)
	{
		foreach (DictionaryEntry kvp in getByValue) {
			Debug.Log ("key by "+value+" val "+kvp);
		}
		object lookup = getByValue[value];
		if (lookup == null)
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	public bool ContainsKey(TKey key)
	{
		object lookup = this.Dictionary[key];
		if (lookup == null)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	public bool GetValue(TKey key, out TValue value)
	{
		object lookup = this.Dictionary[key];
		if (lookup == null)
		{
			value = default(TValue); //null
			return false;
		}
		else
		{
			value = (TValue) lookup;
			return true;
		}
	}
	public TValue GetValue(TKey key)
	{
		object lookup = this.Dictionary[key];
		if (lookup == null)
		{
			//Debug.LogError ("no such value");
			return default(TValue); //null
		}
		else
		{
			return (TValue) lookup;;
		}
	}
	public bool SetValue(TKey key, TValue newValue)
	{
		object lookup = this.Dictionary[key];
		if (lookup == null)
		{
			return false;
		}
		else
		{
			this.Dictionary [key] = newValue;
			return true;
		}
	}

	public bool GetKey(TValue value, out TKey key)
	{
		object lookup = getByValue[value];
		if (lookup == null)
		{
			key = default(TKey);
			return false;
		}
		else
		{
			key = (TKey)lookup;
			return true;
		}
	}
	public TKey GetKey(TValue value)
	{
		object lookup = getByValue[value];
		if (lookup == null)
		{
			//Debug.LogError ("no such value, ret:"+default(TKey));
			return default(TKey);
		}
		else
		{
			return(TKey)lookup;
		}
	}
	public bool SetKey(TValue value,TKey newKey)
	{
		object lookup = getByValue[value];
		if (lookup == null)
		{
			//Debug.LogError ("no such value");
			return false;
		}
		else
		{
			getByValue [value] = newKey;
			return true;
		}
	}

	//Rolling back operation 
	protected override void OnInsertComplete(object key, object value)
	{
		getByValue.Add(value, key);
	}

	protected override void OnSetComplete(object key, object oldValue, object newValue)
	{
		if (getByValue.Contains(newValue))
			Debug.LogError("Duplicate value");
		if (oldValue != null)
			getByValue.Remove(oldValue);
		getByValue[newValue] = key;
	}

	protected override void OnRemoveComplete(object key, object value)
	{
		getByValue.Remove(value);
	}
}
