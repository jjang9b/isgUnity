using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LitJson;
using isg;

public class playerController : MonoBehaviour
{
	public ScrollRect battleHistoryScroll;
	public GameObject battleHistoryContent;
	public GameObject historyObj;
	public GameObject noData;

	public Text infoTxt;
	public Text tierTxt;
	private rootController rc = null;
	private socketController sc = null;
	private WaitForSeconds moveCheckSec = new WaitForSeconds (0.05f);
	private WaitForSeconds waitSec = new WaitForSeconds (0.5f);

	private Color colOff = new Color (255 / 255f, 206 / 255f, 0);
	private Color colOn = new Color (137 / 255f, 0, 0);

	private bool isBattleHistoryList = false;

	void Constructor ()
	{
		rc = new rootController ();
		sc = new socketController ();
	}

	void Init ()
	{
		rc = new rootController ();
		sc = new socketController ();
		isBattleHistoryList = false;
		sc.getUserInfo ();
		CoroutineInit ();
		getBattleHistory ();
		getTier ();
	}

	void CoroutineInit ()
	{
		StartCoroutine (moveEndCoroutine ());
		StartCoroutine (playerCoroutine ());
		StartCoroutine (tierCoroutine ());
		StartCoroutine (battleHistoryCoroutine ());
	}

	void UiInit ()
	{
		rootData.topNavObj.SetActive (true);

		if (battleHistoryContent.transform.childCount <= 0) {
			noData.SetActive (true);
		}
	}

	public void showPlayerCanvas ()
	{
		Constructor ();
		UiInit ();
		Init ();
		rootData.clickBody = rootData.playerBody;
		rootData.isMoveStart = true;
	}

	private void getBattleHistory ()
	{
		JsonData req = new JsonData ();
		req ["step"] = "h1";
		req ["uid"] = userData.userId;

		sc.wsSend (req);
	}

	private void getTier ()
	{
		JsonData req = new JsonData ();
		req ["step"] = "ti1";
		req ["uid"] = userData.userId;

		sc.wsSend (req);
	}

	private IEnumerator moveEndCoroutine ()
	{
		while (true) {
			if (rootData.isMoveEnd) {
				rootData.currentBody = rootData.playerBody;
				rootData.isMoveEnd = false;
				yield break;
			}

			yield return moveCheckSec;
		}
	}

	private IEnumerator playerCoroutine ()
	{
		while (true) {
			playerCheck ();

			yield return waitSec;
		}
	}

	private void playerCheck ()
	{
		JsonData res = userData.res;

		if (res == null) {
			return;
		}

		infoTxt.text = "<color=white>유저 정보</color>\n"
		+ "레벨  <color=#FFE300FF><b>" + res ["user"] ["lev"].ToString () + "</b></color>\n"
		+ "망원경  <color=#FFE300FF><b>" + res ["user"] ["telescope"].ToString () + "</b></color> 개\n"
		+ "교감력  <color=#FFE300FF><b>" + res ["user"] ["a"].ToString () + "</b></color>";
	}

	private IEnumerator tierCoroutine ()
	{
		while (true) {
			tierCheck ();

			yield return waitSec;
		}
	}

	private void tierCheck ()
	{
		JsonData res = userData.tierRes;

		if (res == null) {
			return;
		}

		tierTxt.text = "<color=white>랭킹 정보</color>\n"
		+ "<size=20><color=#FFE300FF>" + res ["userTier"] ["tier"].ToString () + "</color></size>  등급\n"
		+ "랭킹 포인트   <color=#FFE300FF>" + res ["userTier"] ["tierPoint"].ToString () + "</color>\n"
		+ "<color=#FFE300FF>" + res ["userTier"] ["win"].ToString () + "</color> 승"
		+ "  <color=#FFE300FF>" + res ["userTier"] ["draw"].ToString () + "</color> 무"
		+ "  <color=#FFE300FF>" + res ["userTier"] ["lose"].ToString () + "</color> 패";
	}

	private IEnumerator battleHistoryCoroutine ()
	{
		while (true) {
			battleHistoryCheck ();

			if (isBattleHistoryList) {
				yield break;
			}

			yield return waitSec;
		}
	}

	private void battleHistoryCheck ()
	{
		JsonData res = userData.battleHistoryRes;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (!dic.Contains ("historyList") || res ["historyList"].Count <= 0) {
			return;
		}

		noData.SetActive (false);

		for (int i = 0, j = battleHistoryContent.transform.childCount; i < j; i++) {
			Destroy (battleHistoryContent.transform.GetChild (i).gameObject);
		}

		foreach (JsonData history in res["historyList"]) {
			string winResult = "무승부";
			string color = "#89DDA2FF";
			string winColor = "#96BCF0FF";
			string loseColor = "#FF1C1CFF";
			string rivalName = null;

			IDictionary historyDic = history as IDictionary;

			if (historyDic.Contains ("rivalName")) {
				rivalName = history ["rivalName"].ToString ();
			} else {
				rivalName = history ["rivalId"].ToString ();
			}

			if (history ["winId"] != null) {
				if (history ["winId"].Equals (userData.userId)) {
					winResult = "승리";
					color = winColor;
				}

				if (history ["winId"].Equals (history ["rivalId"])) {
					winResult = "패배";
					color = loseColor;
				}
			}

			string line = "[" + DateTime.Parse (history ["regDate"].ToString ()).ToString ("yy-MM-dd HH:mm:ss") + "] "
			              + history ["cardName"]
			              + "\n <color=" + color + ">VS " + rivalName + " [" + history ["rivalCardName"] + "] " + winResult + "</color>";

			GameObject newObj = GameObject.Instantiate<GameObject> (historyObj);

			newObj.transform.SetParent (battleHistoryContent.transform);
			newObj.transform.Find ("Text").GetComponent<Text> ().text = line;

			newObj.GetComponent<RectTransform> ().localPosition = new Vector3 (newObj.transform.position.x, newObj.transform.position.y, 0);
			newObj.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
		}

		isBattleHistoryList = true;
	}
}
