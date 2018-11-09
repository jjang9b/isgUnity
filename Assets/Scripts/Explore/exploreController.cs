using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WebSocketSharp;
using LitJson;
using isg;

public class exploreController : MonoBehaviour
{
	public GameObject isSuccessObj;
	public GameObject resultObj;
	public GameObject overlay;
	public GameObject m1ExploreBtn;
	public GameObject m2ExploreBtn;
	public GameObject m3ExploreBtn;
	public GameObject m4ExploreBtn;
	public GameObject aniCard;
	public GameObject mapList;
	public GameObject m1Percent;
	public GameObject m2Percent;
	public GameObject m3Percent;
	public GameObject m4Percent;
	public GameObject tutorialOverlay;
	public GameObject cardTutorialOverlay;

	public Text savePercentTxt;
	public Text m1PercentTxt;
	public Text m2PercentTxt;
	public Text m3PercentTxt;
	public Text m4PercentTxt;
	public Text isCardNameTxt;
	public Text isCardStatTxt;
	public Text isCardMagicTxt;

	private rootController rc = null;
	private socketController sc = null;
	private WaitForSeconds moveCheckSec = new WaitForSeconds (0.05f);
	private WaitForSeconds waitSec = new WaitForSeconds (0.5f);
	private Color colOff = new Color (255 / 255f, 206 / 255f, 0);
	private Color colOn = new Color (137 / 255f, 0, 0);

	private bool isPercent = false;
	private bool isCardResult = false;

	void Constructor ()
	{
		rc = new rootController ();
		sc = new socketController ();
	}

	void Init ()
	{
		sc.getUserInfo ();
		CoroutineInit ();
		getPercent ();
	}

	void CoroutineInit ()
	{
		isPercent = false;
		isCardResult = false;

		StartCoroutine (limitCoroutine ());
		StartCoroutine (moveEndCoroutine ());
		StartCoroutine (mapPercentCoroutine ());
		StartCoroutine (isCardCoroutine ());
		StartCoroutine (saveCoroutine ());
	}

	void UiInit ()
	{
		rootData.topNavObj.SetActive (true);
		aniCard.SetActive (false);

		overlay.SetActive (false);
		resultObj.SetActive (false);
		isSuccessObj.SetActive (false);
		tutorialOverlay.SetActive (false);
		cardTutorialOverlay.SetActive (false);
		tutorialCheck ();

		checkMapBtn ();
		checkGetBtn ();
	}

	private void tutorialCheck ()
	{
		if (tutorialData.isTutorial) {
			tutorialOverlay.SetActive (true);
			rootData.rootOverlayObj.SetActive (true);
			rootData.mainTutorialObj.SetActive (false);
		}
	}

	public void showExploreCanvas ()
	{
		Constructor ();
		UiInit ();
		tutorialCheck ();
		Init ();
		rootData.clickBody = rootData.exploreBody;
		rootData.isMoveStart = true;
	}

	public void isCard (string mid)
	{
		isCardResult = false;
		StartCoroutine (isCardCoroutine ());

		if (tutorialData.isTutorial) {
			tutorialOverlay.SetActive (false);
		}

		overlay.SetActive (true);
		aniCard.SetActive (true);
		rootData.rootOverlayObj.SetActive (true);

		mapData.selMid = mid;
		Invoke ("isCardSend", 2.5f);
	}

	public void cardGiveUp ()
	{
		isCardResult = true;
		exploreData.res = null;
		exploreData.mid = null;
		exploreData.cid = null;

		isSuccessObj.SetActive (false);
		overlay.SetActive (false);
		rootData.rootOverlayObj.SetActive (false);
	}

	private void isCardSend ()
	{
		JsonData req = new JsonData ();
		req ["step"] = "c0";
		req ["uid"] = userData.userId;
		req ["mid"] = mapData.selMid;
		req ["istutorial"] = tutorialData.isTutorial;

		sc.wsSend (req);
	}

	private void getCardList ()
	{
		JsonData req = new JsonData ();
		req ["step"] = "c2";
		req ["uid"] = userData.userId;

		sc.wsSend (req);
	}

	public void saveCard ()
	{
		JsonData req = new JsonData ();
		req ["step"] = "c1";
		req ["uid"] = userData.userId;
		req ["mid"] = exploreData.mid;
		req ["cid"] = exploreData.cid;
		req ["istutorial"] = tutorialData.isTutorial;

		sc.wsSend (req);
	}

	private void getPercent ()
	{
		JsonData req = new JsonData ();
		req ["step"] = "m1";
		req ["uid"] = userData.userId;

		sc.wsSend (req);
	}

	private void checkMapBtn ()
	{
		int userTelescope = int.Parse (userData.res ["user"] ["telescope"].ToString ());

		if (userData.res != null) {
			if (userTelescope < 2) {
				m1ExploreBtn.SetActive (false);
				m2ExploreBtn.SetActive (false);
				m3ExploreBtn.SetActive (false);
				m4ExploreBtn.SetActive (false);
			}
		}		
	}

	private void checkGetBtn ()
	{
		int userTelescope = int.Parse (userData.res ["user"] ["telescope"].ToString ());

		GameObject getBtn = isSuccessObj.transform.Find ("getBtn").gameObject as GameObject;
		getBtn.SetActive (true);

		if (userTelescope < 1) {
			getBtn.SetActive (false);
		}
	}

	private void checkLimit ()
	{
		int userLev = int.Parse (userData.res ["user"] ["lev"].ToString ());

		GameObject map2HidePannel = mapList.transform.Find ("map2").Find ("hidePannel").gameObject;
		GameObject map3HidePannel = mapList.transform.Find ("map3").Find ("hidePannel").gameObject;
		GameObject map4HidePannel = mapList.transform.Find ("map4").Find ("hidePannel").gameObject;

		m1Percent.SetActive (false);
		m2Percent.SetActive (false);
		m3Percent.SetActive (false);
		m4Percent.SetActive (false);

		m1ExploreBtn.SetActive (false);
		m2ExploreBtn.SetActive (false);
		m3ExploreBtn.SetActive (false);
		m4ExploreBtn.SetActive (false);

		map2HidePannel.transform.Find ("hideLevel").GetComponent<Text> ().text = "5 레벨 이상";
		map2HidePannel.SetActive (true);
		map3HidePannel.transform.Find ("hideLevel").GetComponent<Text> ().text = "10 레벨 이상";
		map3HidePannel.SetActive (true);
		map4HidePannel.transform.Find ("hideLevel").GetComponent<Text> ().text = "15 레벨 이상";
		map4HidePannel.SetActive (true);

		if (userLev >= 1) {
			m1Percent.SetActive (true);
			m1ExploreBtn.SetActive (true);
		}

		if (userLev >= 5) {
			m2Percent.SetActive (true);
			m2ExploreBtn.SetActive (true);
			map2HidePannel.SetActive (false);
		}

		if (userLev >= 10) {
			m3Percent.SetActive (true);
			m3ExploreBtn.SetActive (true);
			map3HidePannel.SetActive (false);
		}

		if (userLev >= 15) {
			m4Percent.SetActive (true);
			m4ExploreBtn.SetActive (true);
			map4HidePannel.SetActive (false);
		}

		mapPercentCheck ();
		checkMapBtn ();
		checkGetBtn ();
	}

	private IEnumerator limitCoroutine ()
	{
		while (true) {
			checkLimit ();

			yield return waitSec;
		}
	}

	private IEnumerator moveEndCoroutine ()
	{
		while (true) {
			if (rootData.isMoveEnd) {
				rootData.currentBody = rootData.exploreBody;
				rootData.isMoveEnd = false;
				yield break;
			}

			yield return moveCheckSec;
		}
	}

	private IEnumerator mapPercentCoroutine ()
	{
		while (true) {
			mapPercentCheck ();

			if (isPercent) {
				isPercent = false;
				yield break;
			}

			yield return waitSec;
		}
	}

	private void mapPercentCheck ()
	{
		JsonData res = mapData.mapRes;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (dic.Contains ("err")) {
			wsData.debug = res.ToJson ();
			return;
		}

		m1PercentTxt.text = "<color=white>탐색 확률</color>\n<color=#FFE300FF>" + res ["percent"] ["m1"].ToString () + " %</color>";
		m2PercentTxt.text = "<color=white>탐색 확률</color>\n<color=#FFE300FF>" + res ["percent"] ["m2"].ToString () + " %</color>";
		m3PercentTxt.text = "<color=white>탐색 확률</color>\n<color=#FFE300FF>" + res ["percent"] ["m3"].ToString () + " %</color>";
		m4PercentTxt.text = "<color=white>탐색 확률</color>\n<color=#FFE300FF>" + res ["percent"] ["m4"].ToString () + " %</color>";
		tutorialOverlay.transform.Find ("map1").Find ("percent").Find ("Text").GetComponent<Text> ().text = "<color=white>탐색 확률</color>\n<color=#FFE300FF>" + res ["percent"] ["m1"].ToString () + " %</color>";

		isPercent = true;
		return;
	}

	private IEnumerator isCardCoroutine ()
	{
		while (true) {
			isCardCheck ();

			if (isCardResult) {
				yield break;
			}

			yield return waitSec;
		}
	}

	private void isCardCheck ()
	{
		JsonData res = exploreData.res;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (dic.Contains ("err")) {
			wsData.debug = res.ToJson ();
			return;
		}

		if (!dic.Contains ("isCard")) {
			return;
		}

		aniCard.SetActive (false);
		isCardResult = true;

		checkMapBtn ();

		if (!bool.Parse (res ["isCard"].ToString ())) {
			resultObj.transform.Find ("Text").GetComponent<Text> ().text = exploreResource.isFail;
			resultObj.SetActive (true);
			overlay.SetActive (true);
			Invoke ("hideModal", 3);
			exploreData.res = null;
			return;
		}

		checkGetBtn ();

		JsonData card = res ["card"];
		string cid = res ["card"] ["cid"].ToString ();
		exploreData.mid = res ["card"] ["mid"].ToString ();
		exploreData.cid = cid;

		string cardName = card ["stat"] ["n"] + "\n(" + card ["stat"] ["nk"] + ") G." + card ["stat"] ["g"];
		string grade = card ["stat"] ["g"].ToString ();
		string stat = "공격력  <color=#CA4C4CFF>" + card ["stat"] ["a"] + "</color>\n" +
		              "방어력  <color=#CA4C4CFF>" + card ["stat"] ["b"] + "</color>\n"
		              + "치명타 확률  <color=#CA4C4CFF>" + card ["stat"] ["c"] + "%</color>";
		string magicStr = "[궁극기]\n";

		foreach (JsonData magic in card["stat"]["magic"]) {
			magicStr += "<color=#CA4C4CFF>* " + magic.ToString () + "</color>\n";
		}

		Image cardImg = isSuccessObj.transform.Find ("cardImg").GetComponent<Image> ();
		cardImg.sprite = Sprite.Create (null, new Rect (0, 0, 0, 0), new Vector2 (0, 0));
		isSuccessObj.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = grade;
		rc.setSprite (isSuccessObj.transform.Find ("cardImg"), cid);

		if (res ["savePercent"] != null) {
			savePercentTxt.text = "획득 확률  <size=24>" + res ["savePercent"].ToString () + "%</size>";
		}

		isCardNameTxt.text = cardName;
		isCardStatTxt.text = stat;
		isCardMagicTxt.text = magicStr;

		resultObj.SetActive (false);
		isSuccessObj.SetActive (true);

		GameObject tutorialSuccessOverlay = isSuccessObj.transform.Find ("tutorialSuccessOverlay").gameObject as GameObject;

		tutorialSuccessOverlay.SetActive (false);

		if (tutorialData.isTutorial) {
			tutorialSuccessOverlay.SetActive (true);

			Image cardTutorialImg = tutorialSuccessOverlay.transform.Find ("cardImg").GetComponent<Image> ();
			cardTutorialImg.sprite = Sprite.Create (null, new Rect (0, 0, 0, 0), new Vector2 (0, 0));			
			rc.setSprite (tutorialSuccessOverlay.transform.Find ("cardImg"), cid);
			tutorialSuccessOverlay.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = grade;
			tutorialSuccessOverlay.transform.Find ("cardNameTxt").GetComponent<Text> ().text = cardName;
			tutorialSuccessOverlay.transform.Find ("cardStatTxt").GetComponent<Text> ().text = stat;
		}
	}

	private IEnumerator saveCoroutine ()
	{
		while (true) {
			saveCheck ();

			yield return waitSec;
		}
	}

	private void saveCheck ()
	{
		JsonData res = cardData.saveRes;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (dic.Contains ("err")) {
			wsData.debug = res.ToJson ();
			return;

		}

		exploreData.res = null;
		cardData.saveRes = null;

		if (bool.Parse (res ["isSave"].ToString ())) {
			resultObj.transform.Find ("Text").GetComponent<Text> ().text = exploreResource.getSuccess;
		} else {
			resultObj.transform.Find ("Text").GetComponent<Text> ().text = exploreResource.getFail;
		}

		cardData.saveRes = null;
		resultObj.SetActive (true);
		isSuccessObj.SetActive (false);

		Invoke ("hideModal", 3);
	}

	private void hideModal ()
	{
		resultObj.SetActive (false);
		rootData.rootOverlayObj.SetActive (false);
		overlay.SetActive (false);

		if (tutorialData.isTutorial) {
			cardTutorialOverlay.SetActive (true);
			rootData.rootOverlayObj.SetActive (true);
			rootData.cardTutorialObj.SetActive (true);
		}
	}
}