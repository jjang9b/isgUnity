using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace isg
{
	public class ObjectPool<T> where T : Component
	{
		private Stack<T> objectPool;
		private int overAllocateCount;
		private T oriObject;
		private Transform parent;
		private string objName;

		public ObjectPool (T oriObject, string objName, Transform parent, int count, int overAllocateCount)
		{
			this.overAllocateCount = overAllocateCount;
			this.oriObject = oriObject;
			this.parent = parent;
			this.objName = objName;
			this.objectPool = new Stack<T> (count);
			Allocate (count);
		}

		public void Allocate (int alloCount)
		{
			for (int i = 0; i < alloCount; ++i) {
				T obj = GameObject.Instantiate<T> (oriObject);
				obj.transform.SetParent (parent);
				obj.name = objName + i.ToString ();
				obj.gameObject.SetActive (false);
				objectPool.Push (obj);
			}
		}


		public T Pop (bool setActive = true)
		{
			if (objectPool.Count <= 0) {
				Allocate (overAllocateCount);
			}

			T retObj = objectPool.Pop ();
			retObj.gameObject.SetActive (setActive);
			return retObj;
		}

		private void Push (T obj)
		{
			objectPool.Push (obj);
		}
	}
}