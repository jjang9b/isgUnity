using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using WebSocketSharp;
using GooglePlayGames;
using LitJson;
using isg;

namespace isg
{
	public class mainController : MonoBehaviour
	{
		public GameObject loadingObj;
		public GameObject guidePannel;
		public GameObject rankingPannel;
		public GameObject rankingContent;
		public GameObject rankObj;
		public GameObject ani1;
		public GameObject tutorialExploreOverlay;
		public GameObject tutorialGuideOverlay;
		public GameObject eventPannel;
		public GameObject eventObj;
		public GameObject mainTopNav;

		private socketController sc = null;
		private rootController rc = null;
		private WaitForSeconds moveCheckSec = new WaitForSeconds (0.05f);
		private WaitForSeconds waitSec = new WaitForSeconds (1);
		private WaitForSeconds rankWaitSec = new WaitForSeconds (0.5f);
		private float fadeColor = 0.7f;
		private bool isLoading = false;
		private bool isRankingList = false;

		void Start ()
		{
			Init ();
		}

		void Update ()
		{
			if (!isLoading) {
				fadeColor -= 0.01f;
				loadingObj.GetComponent<Image> ().color = new Color (fadeColor, fadeColor, fadeColor, 255);

				if (fadeColor <= 0) {
					isLoading = true;
					UiInit ();
				}
			}
		}

		void Init ()
		{
			sc = new socketController ();
			rc = new rootController ();
		}

		void CoroutineInit ()
		{
			StartCoroutine (moveEndCoroutine ());
		}

		void UiInit ()
		{
			ani1.GetComponent<SpriteRenderer> ().sortingOrder = 1;

			mainTopNav.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (130f, (-220f), 0);

			float topNavHeight = rootData.topNavObj.GetComponent<RectTransform> ().rect.height;

			if (topNavHeight < 60f) {
				mainTopNav.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (130f, (-220f + topNavHeight), 0);
			} else {
				mainTopNav.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (130f, (-220f - (90 - topNavHeight)), 0);
			}

			if (rootData.topNavObj != null) {
				rootData.topNavObj.SetActive (true);
			}

			eventPannel.SetActive (false);
			guidePannel.SetActive (false);
			rankingPannel.SetActive (false);

			loadingObj.SetActive (false);
			tutorialExploreOverlay.SetActive (false);
			tutorialGuideOverlay.SetActive (false);

			tutorialCheck ();
		}

		private void getRanking ()
		{
			JsonData req = new JsonData ();
			req ["step"] = "rk1";

			sc.wsSend (req);
		}

		private void tutorialEnd ()
		{
			tutorialData.isTutorial = false;

			JsonData req = new JsonData ();
			req ["step"] = "tr2";
			req ["uid"] = userData.userId;

			sc.wsSend (req);
		}

		private void eventCheck ()
		{
			if (userData.eventRes != null) {
				ani1.GetComponent<SpriteRenderer> ().sortingOrder = 0;

				rootData.rootOverlayObj.SetActive (true);
				eventPannel.SetActive (true);

				foreach (JsonData item in userData.eventRes["itemList"]) {
					GameObject eventItem = Instantiate (eventObj) as GameObject;
					Transform itemImg = eventItem.transform.Find ("itemImg").transform;
					rc.setSprite (itemImg, item ["itemId"].ToString ());
					itemImg.GetComponent<Image> ().preserveAspect = true;
					eventItem.transform.Find ("itemMsg").GetComponent<Text> ().text = item ["itemMsg"].ToString ();
					eventItem.GetComponent<Button> ().onClick.AddListener (() => {
						if (eventPannel.transform.childCount <= 1) {
							ani1.GetComponent<SpriteRenderer> ().sortingOrder = 2;	
							rootData.rootOverlayObj.SetActive (false);
							eventPannel.SetActive (false);
						}

						Destroy (eventItem);
					});

					eventItem.transform.SetParent (eventPannel.transform);
					eventItem.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, 0);
					eventItem.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, 0);
					eventItem.GetComponent<RectTransform> ().localPosition = new Vector3 (1, 1, 1);
					eventItem.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
				}

				userData.eventRes = null;
			} else {
				ani1.GetComponent<SpriteRenderer> ().sortingOrder = 2;
				rootData.rootOverlayObj.SetActive (false);
			}
		}

		private void tutorialCheck ()
		{
			if (tutorialData.isTutorial) {
				if (tutorialData.isBattle) {
					tutorialGuideOverlay.SetActive (true);
					rootData.rootOverlayObj.SetActive (true);
					return;	
				}

				tutorialExploreOverlay.SetActive (true);
				rootData.rootOverlayObj.SetActive (true);
				rootData.mainTutorialObj.SetActive (true);
			} else {
				eventCheck ();
			}
		}

		public void goMain ()
		{
			sc.getUserInfo ();
			UiInit ();
			CoroutineInit ();
			rootData.clickBody = rootData.mainBody;
			rootData.isMoveStart = true;
		}

		public void tutorialSkip ()
		{
			tutorialEnd ();
			rootData.mainTutorialObj.SetActive (false);
			tutorialExploreOverlay.SetActive (false);
			ani1.GetComponent<SpriteRenderer> ().sortingOrder = 2;

			Invoke ("eventCheck", 1f);
		}

		public void showGuide ()
		{
			if (tutorialData.isTutorial) {
				tutorialExploreOverlay.SetActive (false);
				tutorialGuideOverlay.SetActive (false);
				tutorialData.isBattle = false;
				ani1.GetComponent<SpriteRenderer> ().sortingOrder = 2;

				tutorialEnd ();
			}

			ani1.SetActive (false);
			guidePannel.SetActive (true);
			rootData.rootOverlayObj.SetActive (true);
		}

		public void closeGuide ()
		{
			eventCheck ();

			ani1.SetActive (true);
			guidePannel.SetActive (false);
		}

		public void showRanking ()
		{
			ani1.SetActive (false);
			rankingPannel.SetActive (true);
			rootData.rootOverlayObj.SetActive (true);
			isRankingList = false;
			StartCoroutine (rankingListCoroutine ());
			getRanking ();
		}

		public void closeRanking ()
		{
			ani1.SetActive (true);
			rankingPannel.SetActive (false);
			rootData.rootOverlayObj.SetActive (false);
			isRankingList = true;
		}

		private IEnumerator rankingListCoroutine ()
		{
			while (true) {
				rankingListCheck ();

				if (isRankingList) {
					yield break;
				}

				yield return rankWaitSec;
			}
		}

		private void rankingListCheck ()
		{
			JsonData res = rankData.rankRes;
			IDictionary dic = res as IDictionary;

			if (res == null) {
				return;
			}

			for (int i = 0, j = rankingContent.transform.childCount; i < j; i++) {
				Destroy (rankingContent.transform.GetChild (i).gameObject);
			}

			if (!dic.Contains ("rankList") || res ["rankList"].Count <= 0) {
				return;
			}
	
			int num = 1;

			foreach (JsonData rank in res["rankList"]) {
				IDictionary rankDic = rank as IDictionary;

				string userName = rank ["uid"].ToString ();
		
				if (rankDic.Contains ("uName")) {
					userName = rank ["uName"].ToString ();
				}

				string tier = rank ["tier"].ToString ();
				string tierPoint = rank ["tierPoint"].ToString ();
				string win = rank ["win"].ToString ();
				string draw = rank ["draw"].ToString ();
				string lose = rank ["lose"].ToString ();

				GameObject newObj = Instantiate (rankObj) as GameObject;
		
				newObj.transform.Find ("rankImg").transform.Find ("Text").GetComponent<Text> ().text = num.ToString ();
				newObj.transform.Find ("tierImg").transform.Find ("Text").GetComponent<Text> ().text = tier;
				newObj.transform.Find ("rankTxt").GetComponent<Text> ().text = "<color=#FFE300FF>" + userName
				+ "</color>  랭킹 포인트 <b><color=#F17100FF>" + tierPoint + "</color></b>\n"
				+ "<color=#FFE300FF>" + win + "</color>승"
				+ " <color=#FFE300FF>" + draw + "</color>무"
				+ " <color=#FFE300FF>" + lose + "</color>패";
				newObj.transform.SetParent (rankingContent.transform);

				newObj.GetComponent<RectTransform> ().localPosition = new Vector3 (newObj.transform.position.x, newObj.transform.position.y, 0);
				newObj.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);

				num++;
			}

			isRankingList = true;
		}

		private IEnumerator moveEndCoroutine ()
		{
			while (true) {
				if (rootData.isMoveEnd) {
					rootData.currentBody = rootData.mainBody;
					rootData.isMoveEnd = false;
					yield break;
				}

				yield return moveCheckSec;
			}
		}
	}
}