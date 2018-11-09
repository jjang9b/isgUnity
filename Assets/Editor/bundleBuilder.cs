using UnityEngine;
using UnityEditor;

public class bundleBuilder : MonoBehaviour
{
	[MenuItem ("Bundles/Build AssetBundles")]
	static void ExportResource ()
	{
		BuildPipeline.BuildAssetBundles ("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.Android);
	}
}
