using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using LitJson;
using isg;

public class battleController : MonoBehaviour
{
	public GameObject actionBtns;
	public GameObject actionOverlay;
	public GameObject battleMsg;
	public GameObject rewardResult;
	public GameObject rewardPannel;
	public GameObject overlay;
	public GameObject myCardObj;
	public GameObject rivalCardObj;
	public GameObject turnPannel;

	public GameObject myDamageAniObj;
	public GameObject rivalDamageAniObj;
	public GameObject myAddHpAniObj;
	public GameObject rivalAddHpAniObj;
	public GameObject tutorialOverlay;
	public GameObject myMagicSuccess;
	public GameObject myMagicFail;
	public GameObject rivalMagicSuccess;
	public GameObject rivalMagicFail;
	public Image myHp;
	public Image rivalHp;
	public Text myCardNameTxt;
	public Text rivalCardNameTxt;
	public Text myStatTxt;
	public Text rivalStatTxt;
	public Text myHpTxt;
	public Text rivalHpTxt;
	public Text actionTxt;
	public Text turnTxt;
	public Text myBetterTxt;
	public Text rivalBetterTxt;
	public Text battleMsgTxt;
	public AudioSource attackBgm;
	public AudioSource defenceBgm;
	public AudioSource magicBgm;

	private rootController rc = null;
	private socketController sc = null;
	private WaitForSeconds waitSec = new WaitForSeconds (1);
	private WaitForSeconds moveCheckSec = new WaitForSeconds (0.05f);
	private WaitForSeconds scoreWaitSec = new WaitForSeconds (1.5f);
	private float rewardWaitTime = 0;
	private float acStartTime = 0;
	private float timeout = 10f;
	private bool isAction = false;
	private bool isGameEnd = false;
	private bool isGameItem = false;
	private bool isTutorialAction = false;
	private float myFadeColor = 1f;
	private float rivalFadeColor = 1f;
	private Dictionary<string, bool> isShowScore = new Dictionary<string, bool> ();

	void Init ()
	{
		rc = new rootController ();
		sc = new socketController ();
		CoroutineInit ();
	}

	void UiInit ()
	{
		rootData.mainBgm.Stop ();
		rootData.rootOverlayObj.SetActive (false);
		rootData.topNavObj.SetActive (false);
		rootData.menuNavObj.SetActive (false);
		rootData.battleBody.SetActive (true);
		actionOverlay.SetActive (false);
		overlay.SetActive (false);
		battleMsg.SetActive (false);
		tutorialOverlay.SetActive (false);
		myMagicSuccess.SetActive (false);
		myMagicFail.SetActive (false);
		rivalMagicSuccess.SetActive (false);
		rivalMagicFail.SetActive (false);
		turnTxt.GetComponent<Text> ().text = "";

		if (!tutorialData.isTutorial) {
			acStartTime = Time.time;
		}
	}

	void CoroutineInit ()
	{
		StartCoroutine (moveEndCoroutine ());
		StartCoroutine (cardStatCoroutine ());
		StartCoroutine (scoreCoroutine ());
		StartCoroutine (turnTimeoutCoroutine ());
		StartCoroutine (gameEndCoroutine ());
	}

	public void showBattle ()
	{
		UiInit ();
		tutorialCheck ();
		Init ();
		rootData.clickBody = rootData.battleBody;
		rootData.isMoveStart = true;
	}

	private void tutorialCheck ()
	{
		if (tutorialData.isTutorial) {
			tutorialOverlay.SetActive (true);
		}
	}

	public void goAction (int aid)
	{
		if (tutorialData.isTutorial && !isTutorialAction) {
			acStartTime = Time.time;
			isTutorialAction = true;
			tutorialOverlay.SetActive (false);
		}

		turnPannel.transform.Find ("Text").GetComponent<Text> ().text = "";
		isAction = true;
		actionOverlay.SetActive (true);

		JsonData req = new JsonData ();
		req ["step"] = "b3";
		req ["rid"] = battleData.roomRes ["room"] ["rid"].ToString ();
		req ["uid"] = userData.userId;
		req ["rivalId"] = battleData.roomRes ["rivalId"].ToString ();
		req ["aid"] = aid;
		req ["isBotBattle"] = bool.Parse (battleData.roomRes ["room"] ["isBotBattle"].ToString ());

		sc.wsSend (req);
	}

	private void turnTimeout ()
	{
		isAction = true;
		actionOverlay.SetActive (true);

		JsonData req = new JsonData ();
		req ["step"] = "b5";
		req ["rid"] = battleData.roomRes ["room"] ["rid"].ToString ();
		req ["uid"] = userData.userId;
		req ["rivalId"] = battleData.roomRes ["rivalId"].ToString ();
		req ["isBotBattle"] = bool.Parse (battleData.roomRes ["room"] ["isBotBattle"].ToString ());

		sc.wsSend (req);		
	}

	public void giveUp ()
	{
		actionOverlay.SetActive (true);

		JsonData req = new JsonData ();
		req ["step"] = "b4";
		req ["uid"] = userData.userId;
		req ["rid"] = battleData.roomRes ["room"] ["rid"].ToString ();
		req ["rivalId"] = battleData.roomRes ["rivalId"].ToString ();
		req ["isBotBattle"] = bool.Parse (battleData.roomRes ["room"] ["isBotBattle"].ToString ());

		sc.wsSend (req);

		battleMsgTxt.GetComponent<Text> ().text = "전투를 포기했습니다.";
		overlay.SetActive (true);
		battleMsg.SetActive (true);
	}

	private void hideActionOverlay ()
	{
		actionOverlay.SetActive (false);
	}

	private void gameEndMsg ()
	{
		bool isBotBattle = bool.Parse (battleData.roomRes ["room"] ["isBotBattle"].ToString ());
		string msg = "";

		allAnimationStop ();

		if (battleData.roomRes ["room"] ["win"] == null) {
			
			msg = "<color=#FFAE00FF>무승부</color> 입니다.";
		} else {
			string winId = battleData.roomRes ["room"] ["win"].ToString ();
			JsonData winUser = battleData.roomRes ["room"] ["users"] [winId];
			bool isBotCard = bool.Parse (winUser ["card"] ["isBotCard"].ToString ());

			if (isBotCard) {
				msg = "<color=#FFAE00FF>모험 카드 " + winUser ["card"] ["stat"] ["nk"].ToString () + "</color> 카드의 승리입니다.";	
			} else {
				IDictionary winUserDic = winUser ["user"] as IDictionary;

				if (winUserDic.Contains ("uName")) {
					msg = "<color=#FFAE00FF>" + winUser ["user"] ["uName"].ToString () + " 탐험가 " + winUser ["card"] ["stat"] ["nk"].ToString () + "</color> 카드의 승리입니다.";
				} else {
					msg = "<color=#FFAE00FF>" + winId + " 탐험가 " + winUser ["card"] ["stat"] ["nk"].ToString () + "</color> 카드의 승리입니다.";
				}
			}
		}

		battleMsgTxt.GetComponent<Text> ().text = "전투가 끝났습니다.\n" + msg;
		overlay.SetActive (true);
		battleMsg.SetActive (true);

		if (isBotBattle) {
			JsonData roomRes = battleData.roomRes;
			IDictionary dic = roomRes as IDictionary;

			if (roomRes == null) {
				return;
			}

			if (!dic.Contains ("isItem")) {
				return;
			}

			isGameItem = true;

			bool isItem = bool.Parse (roomRes ["isItem"].ToString ());

			if (isItem) {
				Invoke ("rewardShow", 2.5f);
				return;
			}
		}

		Invoke ("battleEnd", 2);
	}

	private void rewardShow ()
	{
		JsonData roomRes = battleData.roomRes;
		IDictionary dic = roomRes as IDictionary;
		battleMsg.SetActive (false);

		if (!dic.Contains ("itemList")) {
			Invoke ("battleEnd", 2);
			return;
		}

		rewardWaitTime = Time.time;

		foreach (JsonData item in roomRes["itemList"]) {
			GameObject newReward = Instantiate (rewardPannel) as GameObject;
			rc.setSprite (newReward.transform.Find ("ItemImg"), item ["id"].ToString ());
			newReward.transform.Find ("ItemImg").GetComponent<Image> ().preserveAspect = true;
			newReward.transform.Find ("ItemName").GetComponent<Text> ().text = item ["name"].ToString ();
			newReward.GetComponent<Button> ().onClick.AddListener (() => {
				Destroy (newReward);
			});
			newReward.transform.SetParent (rewardResult.transform, false);
			newReward.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
		}

		StartCoroutine (rewardResultCoroutine ());
	}

	private void battleEnd ()
	{
		bool isBotBattle = bool.Parse (battleData.roomRes ["room"] ["isBotBattle"].ToString ());

		isAction = false;
		isGameEnd = false;
		isGameItem = false;
		acStartTime = 0;
		isShowScore = new Dictionary<string, bool> ();
		rootData.menuNavObj.SetActive (true);
		rootData.topNavObj.SetActive (true);
		rootData.mainBgm.Play ();

		if (tutorialData.isTutorial) {
			tutorialData.isBattle = true;
			Camera.main.GetComponent<mainController> ().goMain ();
			return;
		}

		if (isBotBattle) {
			Camera.main.GetComponent<adventureController> ().showAdventureBody ();
		} else {
			Camera.main.GetComponent<cardController> ().showCardCanvas ();
		}
	}

	private void cardStatCheck ()
	{
		JsonData res = battleData.roomRes;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (!dic.Contains ("room")) {
			return;
		}

		JsonData myCard = res ["room"] ["users"] [userData.userId] ["card"];

		if (myCard == null) {
			return;
		}

		JsonData rival = res ["room"] ["users"] [res ["room"] ["users"] [userData.userId] ["rivalId"].ToString ()];
		JsonData rivalCard = rival ["card"];

		if (rivalCard == null) {
			return;
		}

		IDictionary myCardDic = myCard as IDictionary;
		IDictionary rivalDic = rival as IDictionary;
		IDictionary rivalCardDic = rivalCard as IDictionary;

		int myBetter = 0;
		int rivalBetter = 0;

		if (myCardDic.Contains ("better")) {
			myBetter = int.Parse (myCard ["better"].ToString ());
		}

		if (rivalCardDic.Contains ("better")) {
			rivalBetter = int.Parse (rivalCard ["better"].ToString ());
		}

		string rivalName = rival ["uid"].ToString ();
		string myCid = myCard ["cid"].ToString ();
		string rivalCid = rivalCard ["cid"].ToString ();
		string myGrade = myCard ["stat"] ["g"].ToString ();
		string rivalGrade = rivalCard ["stat"] ["g"].ToString ();
		string myCardName = myCard ["stat"] ["n"] + " (" + myCard ["stat"] ["nk"] + ") G." + myCard ["stat"] ["g"];
		string rivalCardName = rivalCard ["stat"] ["n"] + " (" + rivalCard ["stat"] ["nk"] + ") G." + rivalCard ["stat"] ["g"];
		string myStat = "";
		string rivalStat = "";

		if (rivalDic.Contains ("user")) {
			if (rival ["user"] != null) {

				IDictionary rivalUserDic = rival ["user"] as IDictionary;

				if (rivalUserDic.Contains ("uName")) {
					rivalName = rival ["user"] ["uName"].ToString ();
				}
			}
		}

		if (myBetter > 0) {
			myStat = "공격력 <color=white>" + myCard ["stat"] ["a"] + "</color><color=#FFAE00FF>(+" + myBetter + ")</color>"
			+ " 방어력 <color=white>" + myCard ["stat"] ["b"] + "</color><color=#FFAE00FF>(+" + myBetter + ")</color>\n"
			+ "치명타 확률 <color=white>" + myCard ["stat"] ["c"] + "%</color><color=#FFAE00FF>(+" + myBetter + "%)</color>\n\n";
		} else {
			myStat = "공격력 <color=white>" + myCard ["stat"] ["a"] + "</color>"
			+ " 방어력 <color=white>" + myCard ["stat"] ["b"] + "</color>\n"
			+ "치명타 확률 <color=white>" + myCard ["stat"] ["c"] + "%</color>\n\n";
		}

		foreach (JsonData magic in myCard["stat"]["magic"]) {
			myStat += "<color=#FFAE00FF>*" + magic.ToString () + "</color>\n";
		}

		if (rivalBetter > 0) {
			rivalStat = "공격력 <color=white>" + rivalCard ["stat"] ["a"] + "</color><color=#FFAE00FF>(+" + rivalBetter + ")</color>"
			+ " 방어력 <color=white>" + rivalCard ["stat"] ["b"] + "</color><color=#FFAE00FF>(+" + rivalBetter + ")</color>\n"
			+ "치명타 확률 <color=white>" + rivalCard ["stat"] ["c"] + "%</color><color=#FFAE00FF>(+" + rivalBetter + "%)</color>\n\n";
		} else {
			rivalStat = "공격력 <color=white>" + rivalCard ["stat"] ["a"] + "</color>"
			+ " 방어력 <color=white>" + rivalCard ["stat"] ["b"] + "</color>\n"
			+ "치명타 확률 <color=white>" + rivalCard ["stat"] ["c"] + "%</color>\n\n";
		}

		foreach (JsonData magic in rivalCard["stat"]["magic"]) {
			rivalStat += "<color=#FFAE00FF>*" + magic.ToString () + "</color>\n";
		}

		float allMyHp = float.Parse (myCard ["aHp"].ToString ());
		float allRivalHp = float.Parse (rivalCard ["aHp"].ToString ());
		float myHpF = float.Parse (myCard ["hp"].ToString ());
		float rivalHpF = float.Parse (rivalCard ["hp"].ToString ());

		Transform myCardImg = myCardObj.transform.Find ("cardImg");
		Transform rivalCardImg = rivalCardObj.transform.Find ("cardImg");

		rc.setSprite (myCardImg, myCid);
		rc.setSprite (rivalCardImg, rivalCid);

		myCardObj.transform.Find ("title").GetComponent<Text> ().text = userData.userName;
		rivalCardObj.transform.Find ("title").GetComponent<Text> ().text = rivalName;

		myCardObj.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = myGrade;
		rivalCardObj.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = rivalGrade;

		myCardNameTxt.text = myCardName;
		rivalCardNameTxt.text = rivalCardName;

		myStatTxt.text = myStat;
		rivalStatTxt.text = rivalStat;

		myBetterTxt.text = "";
		rivalBetterTxt.text = "";

		if (myBetter > 0) {
			myBetterTxt.text = "+" + myBetter.ToString ();
		}

		if (rivalBetter > 0) {
			rivalBetterTxt.text = "+" + rivalBetter.ToString ();
		}

		myHpTxt.text = myHpF + "/" + allMyHp;
		rivalHpTxt.text = rivalHpF + "/" + allRivalHp;

		myHp.GetComponent<Image> ().fillAmount = (myHpF / allMyHp);
		rivalHp.GetComponent<Image> ().fillAmount = (rivalHpF / allRivalHp);
	}

	private void scoreCheck ()
	{
		JsonData res = battleData.roomRes;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (!dic.Contains ("room") || !dic.Contains ("rivalId")) {
			return;
		}

		JsonData room = res ["room"];
		IDictionary dicRoom = room as IDictionary;

		string turn = room ["turn"].ToString ();
		int currentTurn = int.Parse (turn);
		string rivalId = battleData.roomRes ["rivalId"].ToString ();

		if (!dicRoom.Contains ("action")) {
			return;
		}

		if (res ["room"] ["action"].Count <= 0) {
			return;
		}

		JsonData actions = res ["room"] ["action"] ["t" + currentTurn];
		JsonData scoreNumber = res ["room"] ["scoreNumber"] ["t" + currentTurn];
		JsonData aids = res ["room"] ["aid"] ["t" + currentTurn];
		JsonData magicFail = res ["room"] ["magicFail"] ["t" + currentTurn];

		string mKey = turn + "-" + userData.userId;
		string rKey = turn + "-" + rivalId;
		bool myAction = bool.Parse (actions [userData.userId].ToString ());
		bool rivalAction = bool.Parse (actions [rivalId].ToString ());
		int myMagic = int.Parse (magicFail [userData.userId].ToString ());
		int rivalMagic = int.Parse (magicFail [rivalId].ToString ());

		turnTxt.text = turn + "턴";

		if (!isShowScore.ContainsKey (mKey) && myAction) {
			isShowScore.Add (mKey, true);

			int myAid = int.Parse (aids [userData.userId].ToString ());
			int rivalDamage = int.Parse (scoreNumber [rivalId] ["damage"].ToString ());
			int myAddHp = int.Parse (scoreNumber [userData.userId] ["addHp"].ToString ());

			if (myAid == 3 && myMagic <= 0) {
				myMagicSuccess.SetActive (true);
				StartCoroutine (magicResultCoroutine (myMagicSuccess));
			}

			if (myAid == 3 && myMagic > 0) {
				myMagicFail.SetActive (true);
				StartCoroutine (magicResultCoroutine (myMagicFail));
			}

			if (rivalDamage > 0) {
				if (myAid == 3) {
					magicBgm.Play ();
				} else {
					attackBgm.Play ();
				}

				rivalCardObj.GetComponent<Animator> ().Play ("cardAnimation");
				rivalDamageAniObj.GetComponent<Text> ().text = "-" + rivalDamage.ToString ();
				rivalDamageAniObj.GetComponent<Animator> ().Play ("scoreAni");

				Invoke ("stopRivalAttackedAnimation", 2);
				Invoke ("hideRivalDamageNumberAni", 2f);
			}

			if (myAddHp > 0) {
				if (myAid == 3) {
					magicBgm.Play ();
				} else {
					defenceBgm.Play ();
				}

				myCardObj.GetComponent<Animator> ().Play ("cardAnimation");
				myAddHpAniObj.GetComponent<Text> ().text = "+" + myAddHp.ToString ();
				myAddHpAniObj.GetComponent<Animator> ().Play ("scoreAni");

				Invoke ("stopMyDefenceAnimation", 2);
				Invoke ("hideMyAddHpNumberAni", 2f);
			}
		}

		if (!isShowScore.ContainsKey (rKey) && rivalAction) {
			isShowScore.Add (rKey, true);

			int rivalAid = int.Parse (aids [rivalId].ToString ());
			int myDamage = int.Parse (scoreNumber [userData.userId] ["damage"].ToString ());
			int rivalAddHp = int.Parse (scoreNumber [rivalId] ["addHp"].ToString ());

			if (rivalAid == 3 && rivalMagic <= 0) {
				rivalMagicSuccess.SetActive (true);
				StartCoroutine (magicResultCoroutine (rivalMagicSuccess));
			}

			if (rivalAid == 3 && rivalMagic > 0) {
				rivalMagicFail.SetActive (true);
				StartCoroutine (magicResultCoroutine (rivalMagicFail));
			}

			if (myDamage > 0) {
				if (rivalAid == 3) {
					magicBgm.Play ();
				} else {
					attackBgm.Play ();
				}

				myCardObj.GetComponent<Animator> ().Play ("cardAnimation");
				myDamageAniObj.GetComponent<Text> ().text = "-" + myDamage.ToString ();
				myDamageAniObj.GetComponent<Animator> ().Play ("scoreAni");

				Invoke ("stopMyAttackedAnimation", 2);
				Invoke ("hideMyDamageNumberAni", 2f);
			}

			if (rivalAddHp > 0) {
				if (rivalAid == 3) {
					magicBgm.Play ();
				} else {
					defenceBgm.Play ();
				}

				rivalCardObj.GetComponent<Animator> ().Play ("cardAnimation");
				rivalAddHpAniObj.GetComponent<Text> ().text = "+" + rivalAddHp.ToString ();
				rivalAddHpAniObj.GetComponent<Animator> ().Play ("scoreAni");

				Invoke ("stopRivalDefenceAnimation", 2);
				Invoke ("hideRivalAddHpNumberAni", 2f);
			}
		}

		if (myAction && rivalAction) {
			if (isAction) {
				acStartTime = Time.time;
				Invoke ("hideActionOverlay", 2f);				
			}

			isAction = false;
		}
	}

	private void allAnimationStop ()
	{
		myCardObj.GetComponent<Animator> ().Play ("normal");
		rivalCardObj.GetComponent<Animator> ().Play ("normal");
	}

	private void stopMyAttackedAnimation ()
	{
		myCardObj.GetComponent<Animator> ().Play ("normal");
	}

	private void stopMyDefenceAnimation ()
	{
		myCardObj.GetComponent<Animator> ().Play ("normal");
	}

	private void stopRivalAttackedAnimation ()
	{
		rivalCardObj.GetComponent<Animator> ().Play ("normal");
	}

	private void stopRivalDefenceAnimation ()
	{
		rivalCardObj.GetComponent<Animator> ().Play ("normal");
	}

	private void hideMyDamageNumberAni ()
	{
		myDamageAniObj.GetComponent<Text> ().text = "";
		myDamageAniObj.GetComponent<Animator> ().Play ("normal");
	}

	private void hideRivalDamageNumberAni ()
	{
		rivalDamageAniObj.GetComponent<Text> ().text = "";
		rivalDamageAniObj.GetComponent<Animator> ().Play ("normal");
	}

	private void hideMyAddHpNumberAni ()
	{
		myAddHpAniObj.GetComponent<Text> ().text = "";
		myAddHpAniObj.GetComponent<Animator> ().Play ("normal");
	}

	private void hideRivalAddHpNumberAni ()
	{
		rivalAddHpAniObj.GetComponent<Text> ().text = "";
		rivalAddHpAniObj.GetComponent<Animator> ().Play ("normal");
	}

	private IEnumerator magicResultCoroutine (GameObject magicResult)
	{
		while (true) {
			myFadeColor -= 0.08f;
			magicResult.SetActive (true);
			magicResult.GetComponent<Image> ().color = new Color (255, 255, 255, myFadeColor);

			if (myFadeColor <= 0) {
				myFadeColor = 1f;
				magicResult.SetActive (false);
				yield break;
			}

			yield return moveCheckSec;
		}
	}

	private void gameEndItemCheck ()
	{
		JsonData res = battleData.roomRes;
		IDictionary dic = res as IDictionary;

		if (res == null) {
			return;
		}

		if (!dic.Contains ("room")) {
			return;
		}

		bool isEnd = bool.Parse (res ["room"] ["isEnd"].ToString ());

		if (isEnd) {
			isGameEnd = true;
			gameEndMsg ();
		}
	}

	private IEnumerator moveEndCoroutine ()
	{
		while (true) {
			if (rootData.isMoveEnd) {
				rootData.currentBody = rootData.battleBody;
				rootData.isMoveEnd = false;
				yield break;
			}

			yield return moveCheckSec;
		}
	}

	private IEnumerator cardStatCoroutine ()
	{
		while (true) {
			cardStatCheck ();

			yield return waitSec;
		}
	}

	private IEnumerator scoreCoroutine ()
	{
		while (true) {
			scoreCheck ();

			yield return scoreWaitSec;
		}
	}

	private IEnumerator turnTimeoutCoroutine ()
	{
		while (true) {
			if (isGameEnd) {
				yield break;
			}

			if (!isAction) {
				float now = Time.time;
				float spendTime = now - acStartTime;
				float seconds = Mathf.Floor ((spendTime % 60));

				if (acStartTime == 0) {
					seconds = 0;
				}

				turnPannel.transform.Find ("Text").GetComponent<Text> ().text = timeout + "초 제한\n<b><color=red><size=26>" + (timeout - seconds) + "</size></color></b>초";

				if (acStartTime != 0 && seconds > timeout) {
					turnPannel.transform.Find ("Text").GetComponent<Text> ().text = "턴 시간 초과";
					turnTimeout ();
				}
			}

			yield return waitSec;
		}
	}

	private IEnumerator gameEndCoroutine ()
	{
		while (true) {
			if (isGameItem || isGameEnd) {
				yield break;
			}

			gameEndItemCheck ();

			yield return waitSec;
		}
	}

	private IEnumerator rewardResultCoroutine ()
	{
		while (true) {
			float now = Time.time;
			float spendTime = now - rewardWaitTime;

			float seconds = (spendTime % 60);

			if (seconds >= 5) {
				for (int i = 0, j = rewardResult.transform.childCount; i < j; i++) {
					Destroy (rewardResult.transform.GetChild (i).gameObject);
				}

				battleEnd ();
				yield break;				
			}

			if (rewardResult.transform.childCount <= 0) {
				battleEnd ();
				yield break;
			}

			yield return waitSec;
		}
	}
}