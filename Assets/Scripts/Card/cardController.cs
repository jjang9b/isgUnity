using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LitJson;
using isg;

namespace isg
{
	public class cardController : MonoBehaviour
	{
		public GameObject content;
		public GameObject cardTutorialList;
		public GameObject cardDesc;
		public GameObject msgPannel;
		public GameObject timer;
		public GameObject noData;
		public GameObject overlay;
		public GameObject betterPannel;
		public GameObject betterContent;
		public GameObject betterOverlay;
		public GameObject aniCard;
		public GameObject sellPannel;
		public GameObject tutorialOverlay;
		public Button cardObj;
		public Button betterCardObj;
		public Text timerTxt;

		private rootController rc = null;
		private socketController sc = null;
		private WaitForSeconds moveCheckSec = new WaitForSeconds (0.05f);
		private WaitForSeconds timerWaitSec = new WaitForSeconds (0.3f);
		private WaitForSeconds waitSec = new WaitForSeconds (0.5f);
		private float waitStartTime = 0;
		private bool isCardList = false;
		private bool isBetter = false;
		private bool isSell = false;

		private string _cid = null;
		private string _originId = null;
		private string _targetId = null;

		void Constructor ()
		{
			rc = new rootController ();
			sc = new socketController ();
		}

		void Init ()
		{
			isCardList = false;
			sc.getUserInfo ();
			getCardList ();
			CoroutineInit ();
		}

		void CoroutineInit ()
		{
			StartCoroutine (cardListCoroutine ());
			StartCoroutine (moveEndCoroutine ());
		}

		void UiInit ()
		{
			rootData.topNavObj.SetActive (true);
			battleData.isBattle = false;
			battleData.roomRes = null;
			cardDesc.SetActive (false);
			timer.SetActive (false);
			msgPannel.SetActive (false);
			overlay.SetActive (false);
			betterPannel.SetActive (false);
			betterOverlay.SetActive (false);
			aniCard.SetActive (false);
			sellPannel.SetActive (false);
			tutorialOverlay.SetActive (false);

			for (int i = 0, j = content.transform.childCount; i < j; i++) {
				Destroy (content.transform.GetChild (i).gameObject);
			}

			if (content.transform.childCount <= 0) {
				noData.SetActive (true);
			}
		}

		private void tutorialCheck ()
		{
			if (tutorialData.isTutorial) {
				tutorialOverlay.SetActive (true);
				rootData.rootOverlayObj.SetActive (true);
				rootData.cardTutorialObj.SetActive (false);
				rootData.adventureTutorialObj.SetActive (true);
			}
		}

		public void showCardCanvas ()
		{
			Constructor ();
			UiInit ();
			tutorialCheck ();
			Init ();
			rootData.clickBody = rootData.cardBody;
			rootData.isMoveStart = true;
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
			req ["step"] = "b1";
			req ["uid"] = userData.userId;
			req ["_id"] = _id;

			sc.wsSend (req);

			timer.SetActive (true);
			rootData.rootOverlayObj.SetActive (true);
			overlay.SetActive (true);
			waitStartTime = Time.time;
			StartCoroutine (waitTimeCoroutine ());
			StartCoroutine (battleRoomCoroutine ());
		}

		public void rejectRequest ()
		{
			JsonData req = new JsonData ();
			req ["step"] = "b2";
			req ["uid"] = userData.userId;

			sc.wsSend (req);

			timer.SetActive (false);
			overlay.SetActive (false);
			rootData.rootOverlayObj.SetActive (false);
		}

		private void betterRequest ()
		{
			JsonData req = new JsonData ();
			req ["step"] = "c3";
			req ["uid"] = userData.userId;
			req ["_originId"] = _originId;
			req ["_targetId"] = _targetId;

			sc.wsSend (req);

			isBetter = false;
			StartCoroutine (betterCoroutine (_originId, _cid));
		}

		public void sellConfirm (string _id, string cardShortName)
		{
			Button sellBtn = sellPannel.transform.Find ("sellBtn").GetComponent<Button> ();
			Button cancelBtn = sellPannel.transform.Find ("cancelBtn").GetComponent<Button> ();

			sellPannel.transform.Find ("Text").GetComponent<Text> ().text = "<color=#FFAE00FF>" + cardShortName + "</color>\n카드를 정말 판매하시겠습니까?";
			sellPannel.SetActive (true);
			overlay.SetActive (true);
			rootData.rootOverlayObj.SetActive (true);

			sellBtn.onClick.RemoveAllListeners ();
			cancelBtn.onClick.RemoveAllListeners ();

			sellBtn.onClick.AddListener (() => {
				sellPannel.SetActive (false);
				overlay.SetActive (true);
				sell (_id);
			});

			cancelBtn.onClick.AddListener (() => {
				sellPannel.SetActive (false);
				overlay.SetActive (false);
				rootData.rootOverlayObj.SetActive (false);
			});
		}

		private void sell (string _id)
		{
			JsonData req = new JsonData ();
			req ["step"] = "c4";
			req ["uid"] = userData.userId;
			req ["_id"] = _id;

			sc.wsSend (req);

			isSell = false;
			StartCoroutine (sellCoroutine ());			
		}

		public void betterList (string _id, string cid)
		{
			for (int i = 0, j = betterContent.transform.childCount; i < j; i++) {
				Destroy (betterContent.transform.GetChild (i).gameObject);
			}

			JsonData res = cardData.cardList;
			ObjectPool<Button> op = new ObjectPool<Button> (betterCardObj, "betterCard", betterContent.transform, 1, 1);

			foreach (JsonData card in res["cardList"]) {
				IDictionary cardDic = card ["card"] as IDictionary;

				if (!cid.Equals (card ["card"] ["cid"].ToString ())) {
					continue;
				}

				if (_id.Equals (card ["card"] ["_id"].ToString ())) {
					continue;
				}

				if (card ["stat"] == null) {
					continue;
				}

				string grade = card ["stat"] ["g"].ToString ();
				string cardShortName = card ["stat"] ["nk"].ToString ();
				int better = 0;

				if (cardDic.Contains ("better")) {
					better = int.Parse (card ["card"] ["better"].ToString ());
				}

				Button newObj = op.Pop ();

				rc.setSprite (newObj.transform.Find ("cardImg"), cid);
				newObj.transform.Find ("cardNamePannel").Find ("cardName").GetComponent<Text> ().text = cardShortName;
				newObj.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = grade;

				if (better > 0) {
					newObj.transform.Find ("better").GetComponent<Text> ().text = "+" + card ["card"] ["better"].ToString ();
				}

				newObj.transform.Find ("betterBtn").GetComponent<Button> ().onClick.AddListener (() => {
					_cid = cid;
					_originId = _id;
					_targetId = card ["card"] ["_id"].ToString ();

					cardData.betterRes = null;

					betterOverlay.SetActive (true);
					rootData.rootOverlayObj.SetActive (true);
					aniCard.SetActive (true);

					Invoke ("betterRequest", 3);
				});

				newObj.GetComponent<RectTransform> ().localPosition = new Vector3 (newObj.transform.position.x, newObj.transform.position.y, 0);
				newObj.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
			}

			setCellSize (3, betterContent);
			float height = betterContent.transform.GetComponent<RectTransform> ().rect.height + 1000;
			betterContent.GetComponent<RectTransform> ().localPosition = new Vector3 (betterContent.transform.localPosition.x, -height, betterContent.transform.localPosition.z);

			betterPannel.SetActive (true);
		}

		public void closeBetter ()
		{
			betterPannel.SetActive (false);
			rootData.rootOverlayObj.SetActive (false);
		}

		public void closeDesc ()
		{
			cardDesc.SetActive (false);
			overlay.SetActive (false);
			rootData.rootOverlayObj.SetActive (false);
		}

		private void showDesc (string cid, string cardName, string grade, int better, string stat, string magicStr)
		{
			Image cardImg = cardDesc.transform.Find ("cardImg").GetComponent<Image> ();

			cardImg.sprite = Sprite.Create (null, new Rect (0, 0, 0, 0), new Vector2 (0, 0));
			cardDesc.transform.Find ("nameTxt").GetComponent<Text> ().text = cardName;
			cardDesc.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = grade;
			cardDesc.transform.Find ("statTxt").GetComponent<Text> ().text = stat;
			cardDesc.transform.Find ("better").GetComponent<Text> ().text = "";

			if (better > 0) {
				cardDesc.transform.Find ("better").GetComponent<Text> ().text = "+" + better.ToString ();
			}

			cardDesc.transform.Find ("magicTxt").GetComponent<Text> ().text = magicStr;

			overlay.SetActive (true);
			cardDesc.SetActive (true);
			rootData.rootOverlayObj.SetActive (true);

			rc.setSprite (cardDesc.transform.Find ("cardImg"), cid);
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
					rootData.currentBody = rootData.cardBody;
					rootData.isMoveEnd = false;
					yield break;
				}

				yield return moveCheckSec;
			}
		}

		private IEnumerator cardListCoroutine ()
		{
			while (true) {
				cardListCheck ();

				if (isCardList) {
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

			for (int i = 0, j = content.transform.childCount; i < j; i++) {
				Destroy (content.transform.GetChild (i).gameObject);
			}

			if (!dic.Contains ("cardList") || res ["cardList"].Count <= 0) {
				return;
			}

			noData.SetActive (false);
			ObjectPool<Button> op = null;

			if (!tutorialData.isTutorial) {
				op = new ObjectPool<Button> (cardObj, "card", content.transform, 1, 1);
			} else {
				op = new ObjectPool<Button> (cardObj, "card", cardTutorialList.transform, 1, 1);
			}

			foreach (JsonData card in res["cardList"]) {
				IDictionary cardDic = card ["card"] as IDictionary;

				int better = 0;

				if (cardDic.Contains ("better")) {
					better = int.Parse (card ["card"] ["better"].ToString ());
				}

				if (card ["stat"] == null) {
					continue;
				}

				string _id = card ["card"] ["_id"].ToString ();
				string cid = card ["card"] ["cid"].ToString ();
				string grade = card ["stat"] ["g"].ToString ();
				string cardShortName = card ["stat"] ["nk"].ToString ();
				string cardName = card ["stat"] ["n"] + "\n(" + card ["stat"] ["nk"] + ") G." + card ["stat"] ["g"];
				string stat = null;

				if (better > 0) {
					stat = "공격력  <color=#CA4C4CFF>" + card ["stat"] ["a"] + "</color> <color=#FFAE00FF>(+" + better + ")</color>\n"
					+ "방어력  <color=#CA4C4CFF>" + card ["stat"] ["b"] + "</color> <color=#FFAE00FF>(+" + better + ")</color>\n"
					+ "치명타 확률  <color=#CA4C4CFF>" + card ["stat"] ["c"] + "%</color> <color=#FFAE00FF>(+" + better + "%)</color>";
				} else {
					stat = "공격력  <color=#CA4C4CFF>" + card ["stat"] ["a"] + "</color>\n"
					+ "방어력  <color=#CA4C4CFF>" + card ["stat"] ["b"] + "</color>\n"
					+ "치명타 확률  <color=#CA4C4CFF>" + card ["stat"] ["c"] + "%</color>";
				}

				string magicStr = "[궁극기]\n";

				foreach (JsonData magic in card["stat"]["magic"]) {
					magicStr += "<color=#CA4C4CFF>* " + magic.ToString () + "</color>\n";
				}

				Button newObj = op.Pop ();
				int telescope = int.Parse (userData.res ["user"] ["telescope"].ToString ());

				rc.setSprite (newObj.transform.Find ("cardImg"), cid);
				newObj.transform.Find ("cardNamePannel").Find ("cardName").GetComponent<Text> ().text = cardShortName;
				newObj.transform.Find ("grade").Find ("Text").GetComponent<Text> ().text = grade;

				if (!tutorialData.isTutorial) {
					if (better > 0) {
						newObj.transform.Find ("better").GetComponent<Text> ().text = "+" + card ["card"] ["better"].ToString ();
					}

					newObj.transform.Find ("requestBtn").GetComponent<Button> ().onClick.AddListener (() => {
						battleRequest (_id);
					});

					if (better >= 8 || telescope <= 0) {
						newObj.transform.Find ("betterBtn").gameObject.SetActive (false);
					} else {
						newObj.transform.Find ("betterBtn").GetComponent<Button> ().onClick.AddListener (() => {
							betterList (_id, cid);
							rootData.rootOverlayObj.SetActive (true);
						});					
					}

					newObj.transform.Find ("descBtn").GetComponent<Button> ().onClick.AddListener (() => {
						showDesc (cid, cardName, grade, better, stat, magicStr);
					});

					newObj.transform.Find ("sellBtn").GetComponent<Button> ().onClick.AddListener (() => {
						sellConfirm (_id, cardShortName);
					});

					newObj.GetComponent<RectTransform> ().localPosition = new Vector3 (newObj.transform.position.x, newObj.transform.position.y, 0);
					newObj.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
				} else {
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
				setCellSize (3, content);
				float height = content.transform.GetComponent<RectTransform> ().rect.height + 1000;
				content.GetComponent<RectTransform> ().localPosition = new Vector3 (content.transform.localPosition.x, -height, content.transform.localPosition.z);
			}

			isCardList = true;
		}

		private IEnumerator waitTimeCoroutine ()
		{
			while (true) {
				if (battleData.isBattle) {
					timerTxt.text = "잠시 후 전투가 시작됩니다.";
					waitStartTime = 0;
					yield break;
				}
					
				float now = Time.time;
				float spendTime = now - waitStartTime;

				float minutes = (spendTime / 60) % 60;
				float seconds = (spendTime % 60);

				timerTxt.text = "대기 시간\n" + string.Format ("{0:00}:{1:00}", minutes, seconds);

				yield return timerWaitSec;
			}
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

			if (!dic.Contains ("room")) {
				return;
			}

			msgPannel.SetActive (true);
			msgPannel.transform.Find ("msgTxt").GetComponent<Text> ().text = "상대가 정해졌습니다. 전투를 시작합니다!";
			timer.SetActive (false);
			battleData.isBattle = true;

			Invoke ("goBattle", 3);
		}

		private IEnumerator betterCoroutine (string _id, string cid)
		{
			while (true) {
				betterCheck (_id, cid);

				if (isBetter) {
					yield break;
				}

				yield return waitSec;
			}
		}

		private void betterCheck (string _id, string cid)
		{
			JsonData res = cardData.betterRes;
			IDictionary dic = res as IDictionary;

			if (res == null) {
				return;
			}

			if (!dic.Contains ("isBetter")) {
				return;
			}

			aniCard.SetActive (false);
			rootData.rootOverlayObj.SetActive (true);

			bool isCardBetter = bool.Parse (res ["isBetter"].ToString ());

			if (isCardBetter) {
				int better = int.Parse (res ["card"] ["better"].ToString ());
				msgPannel.transform.Find ("msgTxt").GetComponent<Text> ().text = "<b><color=#FFAE00FF>" + better + "강 강화 성공!</color></b> 강력해진 카드를 확인해 보세요!";
			} else {
				msgPannel.transform.Find ("msgTxt").GetComponent<Text> ().text = "<b><color=#FF1C1CFF>강화 실패.</color></b> 아쉽지만 강화에 실패하였습니다.";
			}

			msgPannel.SetActive (true);
			isBetter = true;
			betterList (_id, cid);
			cardListCheck ();
			Invoke ("hideMsg", 3);
		}

		private IEnumerator sellCoroutine ()
		{
			while (true) {
				sellCheck ();

				if (isSell) {
					yield break;
				}

				yield return waitSec;
			}
		}

		private void sellCheck ()
		{
			JsonData res = cardData.sellRes;
			IDictionary dic = res as IDictionary;

			if (res == null) {
				return;
			}

			if (!dic.Contains ("isSell")) {
				return;
			}

			if (bool.Parse (res ["isSell"].ToString ())) {
				msgPannel.transform.Find ("msgTxt").GetComponent<Text> ().text = "<b><color=#FFAE00FF>판매 성공!</color></b> 망원경 및 카드 리스트를 확인해 보세요.";
			}

			isSell = true;
			msgPannel.SetActive (true);
			cardData.sellRes = null;

			Invoke ("hideMsg", 2.5f);
			Invoke ("hideRootOverlay", 2.5f);
			cardListCheck ();
		}

		private void hideMsg ()
		{
			betterOverlay.SetActive (false);
			msgPannel.SetActive (false);
			overlay.SetActive (false);
		}

		private void hideRootOverlay ()
		{
			rootData.rootOverlayObj.SetActive (false);
		}

		private void goBattle ()
		{
			Camera.main.GetComponent<battleController> ().showBattle ();
		}
	}
}