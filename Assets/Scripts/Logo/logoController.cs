using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;
using LitJson;
using isg;

public class logoController : MonoBehaviour
{
	public Image download;
	public GameObject downloadPercent;
	public GameObject updatePannel;
	public GameObject inspectPannel;
	public GameObject overlay;
	public Image fadePannel;
	private float fadeColor = 1f;
	private bool isResource = false;
	private bool isFade = false;
	private WaitForSeconds waitSec = new WaitForSeconds (1);
	private socketController sc = null;
	private rootController rc = null;

	void Start ()
	{
		sc = new socketController ();
		rc = new rootController ();
		downloadPercent.SetActive (false);
		updatePannel.SetActive (false);
		inspectPannel.SetActive (false);
		overlay.SetActive (false);
	}

	void Update ()
	{
		fadeColor -= 0.008f;
		fadePannel.color = new Color (0, 0, 0, fadeColor);

		if (fadeColor <= 0 && !isFade) {
			StartCoroutine (getServerInfo ());
			StartCoroutine (onWsErrorCoroutine ());

			isFade = true;
		}
	}

	public void goStore ()
	{
		Application.OpenURL ("market://details?id=com.BBplayworld.isg");
	}

	public IEnumerator checkResource ()
	{
		while (true) {
			if (wsData.isOpen) {
				if (isResource) {
					yield break;
				}

				isResource = true;

				AssetBundle bundle = AssetBundleManager.getAssetBundle ();

				if (!bundle) {
					StartCoroutine (DownloadAB ());
					yield break;
				}

				yield break;
			}

			yield return waitSec;
		}
	}

	private bool checkUpdate ()
	{
		float cdnBuildVersion = float.Parse (rootData.serverInfo ["buildVersion"].ToString ());

		if (cdnBuildVersion <= rootData.buildVersion) {
			return true;
		}

		return false;
	}

	private IEnumerator getServerInfo ()
	{
		if (rootData.serverInfo != null) {
			yield break;
		}

		UnityWebRequest www = UnityWebRequest.Get (rootData.serverInfoUrl);

		yield return www.Send ();
	
		if (www.isError) {
			wsData.isError = true;
			wsData.error = "Resource version acquisition error.";
			SceneManager.LoadScene ("errorScene", LoadSceneMode.Single);
		}

		JsonData res = JsonMapper.ToObject (www.downloadHandler.text);

		if (res == null) {
			wsData.isError = true;
			wsData.error = "Get server info error.";
		} else {
			rootData.serverInfo = res;
			IDictionary dic = res as IDictionary;

			if (dic.Contains ("inspect")) {
				bool inspect = bool.Parse (rootData.serverInfo ["inspect"].ToString ());

				if (inspect) {
					inspectPannel.SetActive (true);
					overlay.SetActive (true);
					yield break;
				}
			}

			if (!checkUpdate ()) {
				updatePannel.SetActive (true);
				overlay.SetActive (true);
				yield break;
			}

			StartCoroutine (fillAmountCoroutine ());
			StartCoroutine (checkResource ());

			sc.wsConn ();
		}
	}

	private IEnumerator DownloadAB ()
	{
		yield return StartCoroutine (AssetBundleManager.downloadAssetBundle ());
	}

	private IEnumerator fillAmountCoroutine ()
	{
		while (true) {
			download.fillAmount = rootData.fillAmount;
			AssetBundle bundle = AssetBundleManager.getAssetBundle ();

			if (download.fillAmount > 0) {
				downloadPercent.SetActive (true);
				downloadPercent.GetComponent<Text> ().text = Mathf.Floor (rootData.fillAmount * 100f) + "  %";
			}

			if (rootData.serverInfo != null && rootData.fillAmount >= 1 && bundle != null) {
				goScene ("mainScene");
				yield break;
			}

			yield return 0.1f;
		}
	}

	private IEnumerator onWsErrorCoroutine ()
	{
		while (true) {
			if (wsData.isError) {
				goScene ("errorScene");
			}

			yield return waitSec;
		}
	}

	private void goScene (string sceneName)
	{
		SceneManager.LoadScene (sceneName, LoadSceneMode.Single);
	}
}
