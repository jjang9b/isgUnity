using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LitJson;
using isg;

public class adventureController : MonoBehaviour
{
	public GameObject botList;
	public GameObject botContent;
	public GameObject botTutorialList;
	public GameObject botCardDesc;
	public GameObject myCardPannel;
	public GameObject myCardTutorialPannel;
	public GameObject myCardContent;
	public GameObject myCardTutorialList;
	public GameObject noTelescope;
	public GameObject overlay;
	public GameObject tutorialOverlay;
	public GameObject fingerAni;
	public GameObject fingerAniMyCard;
	public Button botCardObj;
	public Button myCardObj;

	private rootController rc = null;
	private socketController sc = null;
	private WaitForSeconds moveCheckSec = new WaitForSeconds (0.05f);
	private WaitForSeconds waitSec = new WaitForSeconds (0.5f);
	private bool isCardList = false;
	private bool isMyCardList = false;
	private string bcid = null;

	void Constructor ()
	{
		rc = new rootController ();
		sc = new socketController ();
	}

	void Init ()
	{
		sc.getUserInfo ();
		getBotList ();
		getCardList ();
		CoroutineInit ();
	}

	void CoroutineInit ()
	{
		StartCoroutine (moveEndCoroutine ());
		StartCoroutine (botListCoroutine ());
		StartCoroutine (cardListCoroutine ());
	}

	void UiInit ()
	{
		rootData.topNavObj.SetActive (false);
		myCardPannel.SetActive (false);
		botCardDesc.SetActive (false);
		overlay.SetActive (false);
		tutorialOverlay.SetActive (false);

		rc.setSprite (noTelescope.transform.Find ("Image"), "c3003");

		int telescope = int.Parse (userData.res ["user"] ["telescope"].ToString ());

		if (telescope > 0) {
			noTelescope.SetActive (false);
			botList.SetActive (true);
		} else {
			noTelescope.SetActive (true);
			botList.SetActive (false);
		}
	}

	private void tutorialCheck ()
	{
		if (tutorialData.isTutorial) {
			tutorialOverlay.SetActive (true);
			rootData.rootOverlayObj.SetActive (true);
			rootData.adventureTutorialObj.SetActive (false);
			myCardTutorialPannel.SetActive (false);
			fingerAni.SetActive (false);
			fingerAniMyCard.SetActive (false);
		}
	}

	public void showAdventureBody ()
	{
		Constructor ();
		UiInit ();
		tutorialCheck ();
		Init ();
		rootData.clickBody = rootData.adventureBody;
		rootData.isMoveStart = true;
	}

	private void getBotList ()
	{
		JsonData req = new JsonData ();
		req ["step"] = "ad1";

		sc.wsSend (req);
	}

	private void getCardList ()
	{
		JsonData req = new JsonData ();
		req ["step"] = "c2";
		req ["uid"] = userData.userId;

		sc.wsSend (req);
	}

	public void battleRequest (string _id)
	{
		JsonData req = new JsonData ();
		req ["step"] = "bb1";
		req ["uid"] = userData.userId;
		req ["_id"] = _id;
		req ["bcid"] = bcid;
		req ["istutorial"] = tutorialData.isTutorial;

		sc.wsSend (req);

		StartCoroutine (battleRoomCoroutine ());
	}

	public void closeMyCard ()
	{
		myCardPannel.SetActive (false);
		rootData.rootOverlayObj.SetActive (false);
	}

	private void battleMyCard (string botCid)
	{
		bcid = botCid;

		if (!tutorialData.isTutorial) {
			myCardPannel.SetActive (true);
		} else {
			myCardTutorialPannel.SetActive (true);
		}
	}

	private void goBattle ()
	{
		Camera.main.GetComponent<battleController> ().showBattle ();
	}

	private void showDesc (string cid, string cardName, string grade, string stat, string magicStr)
	{
		Image cardImg = botCardDesc.transform.Find ("cardImg").GetComponent<Image> ();
		overlay.SetActive (true);
		rootData.rootOverlayObj.SetActive (true);

		cardImg.sprite = Sprite.Create (null, new Rect (0, 0, 0, 0), new Vector2 (0, 0));
		botCardDesc.transform.Find ("nameTxt").GetComponent<Text> ().text = cardName;
		botCardDesc.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = grade;
		botCardDesc.transform.Find ("statTxt").GetComponent<Text> ().text = stat;
		botCardDesc.transform.Find ("magicTxt").GetComponent<Text> ().text = magicStr;

		botCardDesc.SetActive (true);

		rc.setSprite (botCardDesc.transform.Find ("cardImg"), cid);
	}

	public void closeDesc ()
	{
		botCardDesc.SetActive (false);
		rootData.rootOverlayObj.SetActive (false);
		overlay.SetActive (false);
	}

	private void setCellSize (int cellCount, GameObject content)
	{
		RectTransform parent = content.transform.GetComponent<RectTransform> ();
		GridLayoutGroup grid = content.transform.GetComponent<GridLayoutGroup> ();

		float spaceW = (grid.padding.left + grid.padding.right) + (grid.spacing.x * (cellCount - 1));
		float maxW = parent.rect.width - spaceW;
		float width = Mathf.Min (parent.rect.width - (grid.padding.left + grid.padding.right) - (grid.spacing.x * (cellCount - 1)), maxW);

		grid.cellSize = new Vector2 (width / cellCount, grid.cellSize.y);
	}

	private IEnumerator moveEndCoroutine ()
	{
		while (true) {
			if (rootData.isMoveEnd) {
				rootData.currentBody = rootData.adventureBody;
				rootData.isMoveEnd = false;
				yield break;
			}

			yield return moveCheckSec;
		}
	}

	private IEnumerator botListCoroutine ()
	{
		while (true) {
			botListCheck ();

			if (isCardList) {
				yield break;
			}

			yield return waitSec;
		}
	}

	private void botListCheck ()
	{
		JsonData res = botData.botList;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (dic.Contains ("err")) {
			wsData.debug = res.ToJson ();
			return;
		}

		for (int i = 0, j = botContent.transform.childCount; i < j; i++) {
			Destroy (botContent.transform.GetChild (i).gameObject);
		}

		if (!dic.Contains ("botList") || res ["botList"].Count <= 0) {
			return;
		}

		IDictionary dicList = res ["botList"] as IDictionary;
		ObjectPool<Button> op = null;

		if (!tutorialData.isTutorial) {
			op = new ObjectPool<Button> (botCardObj, "card", botContent.transform, 1, 1);
		} else {
			op = new ObjectPool<Button> (botCardObj, "card", botTutorialList.transform, 1, 1);
		}

		foreach (ICollection a in dicList.Values) {
			JsonData card = (JsonData)a;

			string cid = card ["cid"].ToString ();
			string grade = card ["stat"] ["g"].ToString ();
			string cardShortName = card ["stat"] ["nk"].ToString ();
			string cardName = card ["stat"] ["n"] + "\n(" + card ["stat"] ["nk"] + ") G." + card ["stat"] ["g"];
			string stat = "공격력  <color=#CA4C4CFF>" + card ["stat"] ["a"] + "</color>\n"
			              + "방어력  <color=#CA4C4CFF>" + card ["stat"] ["b"] + "</color>\n"
			              + "치명타 확률  <color=#CA4C4CFF>" + card ["stat"] ["c"] + "%</color>";
			string magicStr = "[궁극기]\n";

			foreach (JsonData magic in card["stat"]["magic"]) {
				magicStr += "<color=#CA4C4CFF>* " + magic.ToString () + "</color>\n";
			}

			Button newObj = op.Pop ();

			rc.setSprite (newObj.transform.Find ("cardImg"), cid);
			newObj.transform.Find ("cardNamePannel").Find ("cardName").GetComponent<Text> ().text = cardShortName;
			newObj.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = grade;
			newObj.transform.Find ("requestBtn").GetComponent<Button> ().onClick.AddListener (() => {
				if (tutorialData.isTutorial) {
					fingerAni.SetActive (false);
					fingerAniMyCard.SetActive (true);
				}

				battleMyCard (cid);
				rootData.rootOverlayObj.SetActive (true);
			});

			if (!tutorialData.isTutorial) {
				newObj.transform.Find ("descBtn").GetComponent<Button> ().onClick.AddListener (() => {
					showDesc (cid, cardName, grade, stat, magicStr);
				});
			}

			GameObject hidePannel = newObj.transform.Find ("hidePannel").gameObject as GameObject;
			GameObject hideLevel = hidePannel.transform.Find ("hideLevel").gameObject as GameObject;
			hidePannel.SetActive (false);
			hideLevel.SetActive (false);

			checkLevel (newObj, grade, hidePannel, hideLevel);

			if (!tutorialData.isTutorial) {
				newObj.GetComponent<RectTransform> ().localPosition = new Vector3 (newObj.transform.position.x, newObj.transform.position.y, 0);
				newObj.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
			} else {
				RectTransform rect = newObj.GetComponent<RectTransform> ();
				rect.localPosition = new Vector3 (0, 0, 0);
				rect.localScale = new Vector3 (1, 1, 1);
				rect.anchoredPosition = new Vector2 (0, 0);
				rect.anchorMin = new Vector2 (0, 0);
				rect.anchorMax = new Vector2 (1, 1);
				fingerAni.SetActive (true);
				break;
			}
		}

		if (!tutorialData.isTutorial) {
			setCellSize (3, botContent);
			float height = botContent.transform.GetComponent<RectTransform> ().rect.height + 1000;
			botContent.GetComponent<RectTransform> ().localPosition = new Vector3 (botContent.transform.localPosition.x, -height, botContent.transform.localPosition.z);
		}

		isCardList = true;
	}

	private void checkLevel (Button newObj, string gradeStr, GameObject hidePannel, GameObject hideLevel)
	{
		int userLev = int.Parse (userData.res ["user"] ["lev"].ToString ());
		int grade = int.Parse (gradeStr);
		bool isCheck = false;

		if (userLev < 5) {
			if (grade >= 20 && grade < 25) {
				hideLevel.GetComponent<Text> ().text = "5 레벨 이상.";
				isCheck = true;
			} else if (grade >= 25 && grade < 30) {
				hideLevel.GetComponent<Text> ().text = "10 레벨 이상.";
				isCheck = true;
			} else if (grade >= 30 && grade < 35) {
				hideLevel.GetComponent<Text> ().text = "15 레벨 이상.";
				isCheck = true;
			} else if (grade >= 35 && grade < 40) {
				hideLevel.GetComponent<Text> ().text = "20 레벨 이상.";
				isCheck = true;
			} else if (grade >= 40) {
				hideLevel.GetComponent<Text> ().text = "20 레벨 이상.";
				isCheck = true;
			}

		} else if (userLev >= 5 && userLev < 10) {
			if (grade >= 25 && grade < 30) {
				hideLevel.GetComponent<Text> ().text = "10 레벨 이상.";
				isCheck = true;
			} else if (grade >= 30 && grade < 35) {
				hideLevel.GetComponent<Text> ().text = "15 레벨 이상.";
				isCheck = true;
			} else if (grade >= 35 && grade < 40) {
				hideLevel.GetComponent<Text> ().text = "20 레벨 이상.";
				isCheck = true;
			} else if (grade >= 40) {
				hideLevel.GetComponent<Text> ().text = "20 레벨 이상.";
				isCheck = true;
			}

		} else if (userLev >= 10 && userLev < 15) {
			if (grade >= 30 && grade < 35) {
				hideLevel.GetComponent<Text> ().text = "15 레벨 이상.";
				isCheck = true;
			} else if (grade >= 35 && grade < 40) {
				hideLevel.GetComponent<Text> ().text = "20 레벨 이상.";
				isCheck = true;
			} else if (grade >= 40) {
				hideLevel.GetComponent<Text> ().text = "20 레벨 이상.";
				isCheck = true;
			}

		} else if (userLev >= 15 && userLev < 20) {
			if (grade >= 35 && grade < 40) {
				hideLevel.GetComponent<Text> ().text = "20 레벨 이상.";
				isCheck = true;
			} else if (grade >= 40) {
				hideLevel.GetComponent<Text> ().text = "20 레벨 이상.";
				isCheck = true;
			}
		}

		if (isCheck) {
			hidePannel.SetActive (true);
			hideLevel.SetActive (true);
		}
	}

	private IEnumerator cardListCoroutine ()
	{
		while (true) {
			cardListCheck ();

			if (isMyCardList) {
				yield break;
			}

			yield return waitSec;
		}
	}

	private void cardListCheck ()
	{
		JsonData res = cardData.cardList;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (dic.Contains ("err")) {
			wsData.debug = res.ToJson ();
			return;
		}

		for (int i = 0, j = myCardContent.transform.childCount; i < j; i++) {
			Destroy (myCardContent.transform.GetChild (i).gameObject);
		}

		if (!dic.Contains ("cardList") || res ["cardList"].Count <= 0) {
			return;
		}

		ObjectPool<Button> op = null;

		if (!tutorialData.isTutorial) {
			op = new ObjectPool<Button> (myCardObj, "card", myCardContent.transform, 1, 1);
		} else {
			op = new ObjectPool<Button> (myCardObj, "card", myCardTutorialList.transform, 1, 1);
		}

		foreach (JsonData card in res["cardList"]) {
			IDictionary cardDic = card ["card"] as IDictionary;

			int better = 0;

			if (cardDic.Contains ("better")) {
				better = int.Parse (card ["card"] ["better"].ToString ());
			}

			string _id = card ["card"] ["_id"].ToString ();
			string cid = card ["card"] ["cid"].ToString ();
			string grade = card ["stat"] ["g"].ToString ();
			string cardShortName = card ["stat"] ["nk"].ToString ();

			Button newObj = op.Pop ();

			rc.setSprite (newObj.transform.Find ("cardImg"), cid);
			newObj.transform.Find ("cardNamePannel").Find ("cardName").GetComponent<Text> ().text = cardShortName;
			newObj.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = grade;

			if (better > 0) {
				newObj.transform.Find ("better").GetComponent<Text> ().text = "+" + card ["card"] ["better"].ToString ();
			}

			newObj.transform.Find ("requestBtn").GetComponent<Button> ().onClick.AddListener (() => {
				if (tutorialData.isTutorial) {
					rootData.adventureTutorialObj.SetActive (false);
				}

				battleRequest (_id);
			});

			if (!tutorialData.isTutorial) {
				newObj.transform.SetParent (myCardContent.transform);
				newObj.GetComponent<RectTransform> ().localPosition = new Vector3 (newObj.transform.position.x, newObj.transform.position.y, 0);
				newObj.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
			} else {
				newObj.transform.SetParent (myCardTutorialList.transform);
				RectTransform rect = newObj.GetComponent<RectTransform> ();
				rect.localPosition = new Vector3 (0, 0, 0);
				rect.localScale = new Vector3 (1, 1, 1);
				rect.anchoredPosition = new Vector2 (0, 0);
				rect.anchorMin = new Vector2 (0, 0);
				rect.anchorMax = new Vector2 (1, 1);
				break;
			}
		}

		if (!tutorialData.isTutorial) {
			setCellSize (3, myCardContent);
			myCardContent.GetComponent<RectTransform> ().localPosition = new Vector3 (myCardContent.transform.localPosition.x, -1000, myCardContent.transform.localPosition.z);
		}

		isMyCardList = true;
	}

	private IEnumerator battleRoomCoroutine ()
	{
		while (true) {
			battleRoomCheck ();

			if (battleData.isBattle) {
				yield break;
			}

			yield return waitSec;
		}
	}

	private void battleRoomCheck ()
	{
		JsonData res = battleData.roomRes;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (dic.Contains ("err")) {
			wsData.debug = res.ToJson ();
			return;
		}

		if (!dic.Contains ("room")) {
			return;
		}
			
		battleData.isBattle = true;

		Invoke ("goBattle", 0.5f);
	}
}
