using System;
using UnityEditor;

namespace isg
{
	public class buildData
	{
		public float getBuildVersion ()
		{
			return float.Parse (PlayerSettings.bundleVersion);
		}
	}
}