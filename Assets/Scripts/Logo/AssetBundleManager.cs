using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using isg;

static public class AssetBundleManager
{
	static private Dictionary<string, AssetBundleRef> dictAssetBundleRefs;

	static AssetBundleManager ()
	{
		dictAssetBundleRefs = new Dictionary<string, AssetBundleRef> ();
	}

	private class AssetBundleRef
	{
		public AssetBundle assetBundle = null;
		public int version;
		public string url;

		public AssetBundleRef (string strUrlIn, int intVersionIn)
		{
			url = strUrlIn;
			version = intVersionIn;
		}
	};

	public static AssetBundle getAssetBundle ()
	{
		if (rootData.serverInfo == null) {
			return null;
		}

		string getBundleUrl = rootData.serverInfo ["getBundleUrl"].ToString ();
		string bundleVersion = rootData.serverInfo ["bundleVersion"].ToString ();
		string keyName = getBundleUrl + bundleVersion.ToString ();

		AssetBundleRef abRef;

		if (dictAssetBundleRefs.TryGetValue (keyName, out abRef))
			return abRef.assetBundle;
		else
			return null;
	}

	public static IEnumerator downloadAssetBundle ()
	{
		string getBundleUrl = rootData.serverInfo ["getBundleUrl"].ToString ();
		string bundleVersion = rootData.serverInfo ["bundleVersion"].ToString ();
		int bVersion = int.Parse (bundleVersion);
		string keyName = getBundleUrl + bundleVersion.ToString ();

		if (dictAssetBundleRefs.ContainsKey (keyName)) {
			yield return null;

		} else {
			WWW www = WWW.LoadFromCacheOrDownload (getBundleUrl, bVersion);

			while (!www.isDone) {
				if (www.error != null) {
					throw new Exception ("WWW download :" + www.error);
				}

				rootData.fillAmount = www.progress;
				yield return null;
			}

			AssetBundleRef abRef = new AssetBundleRef (getBundleUrl, bVersion);

			if (www.isDone && www.error == null) {
				rootData.fillAmount = 1;
				abRef.assetBundle = www.assetBundle;
				dictAssetBundleRefs.Add (keyName, abRef);
			}
		}
	}
}