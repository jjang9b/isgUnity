using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;
using WebSocketSharp;
using GooglePlayGames;
using LitJson;
using isg;

namespace isg
{
	public class rootController : MonoBehaviour
	{
		private string uid = "g09309352203240853389";
		private string uName = "BBplayworld1";
		public GameObject settingBody;
		public GameObject exploreBody;
		public GameObject mainBody;
		public GameObject playerBody;
		public GameObject adventureBody;
		public GameObject cardBody;
		public GameObject battleBody;
		public GameObject storeBody;

		public GameObject topNavObj;
		public GameObject menuNavObj;
		public GameObject settingObj;
		public GameObject battleRequestObj;
		public GameObject rootOverlayObj;
		public GameObject mainOverlayObj;
		public GameObject mainTutorialObj;
		public GameObject cardTutorialObj;
		public GameObject adventureTutorialObj;
		public GameObject bgmObj;
		public GameObject telescopeAni;
		public AudioSource mainBgm;

		public Text userIdSetting;
		public Text userNameSetting;
		public Text regDateSetting;
		public Text userName;
		public Text userLevel;
		public Text	tierTxt;
		public Text	telescopeTimerTxt;
		public Text userTelescope;

		public Slider soundValue;

		private socketController sc = null;
		private WaitForSeconds moveSec = new WaitForSeconds (0.001f);
		private WaitForSeconds waitOneSec = new WaitForSeconds (1);
		private WaitForSeconds waitTwoSec = new WaitForSeconds (2);
		private WaitForSeconds waitTenSec = new WaitForSeconds (10);
		private bool isSetting = false;
		private bool isPaused = false;
		private float telescopWaitSTime = 0;
		private float telescopRefreshSecond = 300;

		/* FPS */
		private float accum = 0;
		private int frames = 0;
		private float timeleft;

		private const float inch = 2.54f;

		[SerializeField]
		private EventSystem es = null;

		[SerializeField]
		private float dragThreadCm = 0.5f;

		void Start ()
		{
			Init ();
		}

		void Awake ()
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			if (es != null) {
				es.pixelDragThreshold = (int)(dragThreadCm * Screen.dpi / inch);
			}
		}

		void Setting ()
		{
			Application.targetFrameRate = 60;
		}

		void Init ()
		{
			Setting ();
			setAllObject ();
			UiInit ();
			initHideBody ();

			sc = new socketController ();

			if (!wsData.isOpen) {
				goScene ("logoScene");
				return;
			}

			googleLogin ();
			checkResource ();

			if (rootData.serverInfo ["telescopeTimer"] != null) {
				telescopRefreshSecond = float.Parse (rootData.serverInfo ["telescopeTimer"].ToString ());
			}

			telescopWaitSTime = Time.time;

			StartCoroutine (moveBody ());
			StartCoroutine (userInfoCoroutine ());
			StartCoroutine (userTierCoroutine ());
			StartCoroutine (telescopeTimerCoroutine ());
			StartCoroutine (onWsErrorCoroutine ());
		}

		void UiInit ()
		{
			rootData.currentBody = mainBody;
			mainOverlayObj.SetActive (false);
			rootOverlayObj.SetActive (false);
			mainTutorialObj.SetActive (false);
			cardTutorialObj.SetActive (false);
			adventureTutorialObj.SetActive (false);
			telescopeAni.GetComponent<Animator> ().Play ("effect");
		}

		void OnApplicationPause (bool pause)
		{
			if (rootData.currentBody == rootData.storeBody || userData.isLogining) {
				return;
			}

			if (pause) {
				isPaused = true;
			} else {
				if (isPaused) {
					isPaused = false;

					if (userData.userId == null) {
						wsData.error = "Login fail.";
						goScene ("errorScene");
						return;
					}

					goScene ("logoScene");
				}
			}
		}

		public void goScene (string sceneName)
		{
			SceneManager.LoadScene (sceneName, LoadSceneMode.Single);
		}

		public void setSprite (Transform obj, string assetName)
		{
			AssetBundle bundle = AssetBundleManager.getAssetBundle ();

			if (bundle) {
				Texture2D cardImg = bundle.LoadAsset (assetName, typeof(Texture2D)) as Texture2D;

				if (!cardImg) {
					Texture2D defaultImg = bundle.LoadAsset ("cdefault", typeof(Texture2D)) as Texture2D;

					Rect rect = new Rect (0, 0, defaultImg.width, defaultImg.height);
					obj.GetComponent<Image> ().sprite = Sprite.Create (defaultImg, rect, new Vector2 (0, 0));					
					return;
				}

				if (cardImg) {
					Rect rect = new Rect (0, 0, cardImg.width, cardImg.height);
					obj.GetComponent<Image> ().sprite = Sprite.Create (cardImg, rect, new Vector2 (0, 0));
				}
			}
		}

		public void initHideBody ()
		{
			settingBody.SetActive (false);
		}

		public void checkResource ()
		{
			AssetBundle bundle = AssetBundleManager.getAssetBundle ();

			if (!bundle) {
				StartCoroutine (DownloadAB ());
			}
		}

		private void telescopeRefresh ()
		{
			JsonData req = new JsonData ();
			req ["step"] = "ttr1";
			req ["uid"] = userData.userId;

			sc.wsSend (req);		
		}

		private void setAllObject ()
		{
			rootData.rc = new rootController ();

			rootData.mainBody = mainBody;
			rootData.settingBody = settingBody;
			rootData.exploreBody = exploreBody;
			rootData.playerBody = playerBody;
			rootData.adventureBody = adventureBody;
			rootData.cardBody = cardBody;
			rootData.battleBody = battleBody;
			rootData.storeBody = storeBody;

			rootData.topNavObj = topNavObj;
			rootData.menuNavObj = menuNavObj;
			rootData.rootOverlayObj = rootOverlayObj;
			rootData.mainOverlayObj = mainOverlayObj;
			rootData.mainTutorialObj = mainTutorialObj;
			rootData.cardTutorialObj = cardTutorialObj;
			rootData.adventureTutorialObj = adventureTutorialObj;
			rootData.mainBgm = mainBgm;
		}

		private void googleLogin ()
		{
			PlayGamesPlatform.DebugLogEnabled = true;
			PlayGamesPlatform.Activate ();

			if (rootData.isTest) {
				userData.userId = "G-" + uid;
				userData.userName = uName;
				setSocialInfo ();
				return;
			}

			if (!Social.localUser.authenticated) {
				userData.isLogining = true;

				Social.localUser.Authenticate (result => {
					userData.isLogining = false;

					if (!result) {
						wsData.error = "구글 플레이 로그인 실패.";
						goScene ("errorScene");
						return;
					}

					userData.userId = "G-" + Social.localUser.id;
					userData.userName = Social.localUser.userName;

					setSocialInfo ();
				});

				return;
			}

			userData.userId = "G-" + Social.localUser.id;
			userData.userName = Social.localUser.userName;
			setSocialInfo ();
		}

		private void setSocialInfo ()
		{
			userName.text = userData.userName;
			sc.wsClientPush ();
			sc.getUserInfo ();
			setUserId ();
		}

		private void hideRootOverlay ()
		{
			rootData.rootOverlayObj.SetActive (false);
		}

		public void setUserId ()
		{
			userIdSetting.GetComponent<Text> ().text = "유저 아이디 (플랫폼 아이디) <color=#FFAE00FF>" + userData.userId + "</color>";
			userNameSetting.GetComponent<Text> ().text = "유저 이름 <color=#FFAE00FF>" + userData.userName + "</color>";
		}

		public void logOut ()
		{
			if (Social.localUser.authenticated) {
				((PlayGamesPlatform)Social.Active).SignOut ();
			}
				
			goScene ("logoScene");
		}

		public void goSite ()
		{
			Application.OpenURL (rootData.siteUrl);
		}

		public void showSetting ()
		{
			isSetting = !isSetting;
			settingBody.SetActive (isSetting);
			rootData.mainOverlayObj.SetActive (isSetting);
			rootData.rootOverlayObj.SetActive (isSetting);
		}

		public void closeSetting ()
		{
			isSetting = false;
			settingBody.SetActive (isSetting);
			rootData.mainOverlayObj.SetActive (isSetting);
			rootData.rootOverlayObj.SetActive (isSetting);
		}

		public void soundValueChange ()
		{
			mainBgm.volume = soundValue.value;
		}

		public void resetRootValue ()
		{
			wsData.error = null;
			wsData.isError = false;
		}

		// coroutins.
		private IEnumerator DownloadAB ()
		{
			yield return StartCoroutine (AssetBundleManager.downloadAssetBundle ());
		}

		private IEnumerator telescopeTimerCoroutine ()
		{
			while (true) {
				if (userData.isMaxTelescope) {
					telescopeTimerTxt.text = "일일 최대 충전";
					telescopeAni.GetComponent<Animator> ().Play ("normal");
					yield return waitTenSec;
				}

				float now = Time.time;
				float spendTime = (telescopWaitSTime + telescopRefreshSecond) - now;

				if (spendTime <= 1) {
					telescopWaitSTime = Time.time;
					telescopeRefresh ();
					telescopeTimerTxt.text = string.Format ("{0:00}:{1:00}", 0, 0);
				} else {
					float minutes = Mathf.Floor ((spendTime / 60)) % 60;
					float seconds = (spendTime % 60);

					telescopeTimerTxt.text = string.Format ("{0:00}:{1:00}", minutes, seconds);					
				}

				yield return waitOneSec;
			}
		}

		private IEnumerator moveBody ()
		{
			while (true) {
				if (rootData.isMoveStart) {
					if (rootData.clickBody != rootData.battleBody && !tutorialData.isTutorial) {
						rootData.rootOverlayObj.SetActive (true);
					}

					rootData.currentBody.transform.Translate (Vector3.up * 2f);
					rootData.clickBody.transform.Translate (Vector3.up * 2f);
				}

				if (rootData.isMoveStart && rootData.currentBody.transform.position.y > 2.5) {
					rootData.currentBody.transform.GetComponent<RectTransform> ().offsetMin = new Vector2 (0, -1290);
					rootData.currentBody.transform.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, -1290);
					rootData.clickBody.transform.GetComponent<RectTransform> ().localPosition = new Vector3 (0, 0, 0);
					rootData.isMoveStart = false;
					rootData.isMoveEnd = true;

					if (rootData.clickBody != rootData.battleBody && !tutorialData.isTutorial) {
						Invoke ("hideRootOverlay", 0.8f);
					}
				}

				yield return moveSec;
			}
		}

		private IEnumerator userInfoCoroutine ()
		{
			while (true) {
				userInfoCheck ();

				yield return waitTwoSec;
			}
		}

		private void userInfoCheck ()
		{
			JsonData res = userData.res;
			IDictionary dic = res as IDictionary;

			if (res == null) {
				return;
			}

			regDateSetting.GetComponent<Text> ().text = "계정 생성일자 <color=#FFAE00FF>" + DateTime.Parse (res ["user"] ["regDate"].ToString ()).ToString ("yy-MM-dd HH:mm:ss") + "</color>";

			userData.res = res;
			userLevel.text = "레벨  <color=#FFCE00FF>" + res ["user"] ["lev"].ToString ()
			+ "</color> 경험치  <color=#FFCE00FF>" + res ["user"] ["exp"].ToString () + "%</color>";
			userTelescope.text = res ["user"] ["telescope"].ToString ();
		}

		private IEnumerator userTierCoroutine ()
		{
			while (true) {
				userTierCheck ();

				yield return waitTwoSec;
			}
		}

		private void userTierCheck ()
		{
			JsonData res = userData.tierRes;
			IDictionary dic = res as IDictionary;

			if (res == null) {
				return;
			}
				
			tierTxt.text = res ["userTier"] ["tier"].ToString ();
		}

		private IEnumerator onWsErrorCoroutine ()
		{
			while (true) {
				if (wsData.isError) {
					goScene ("errorScene");
				}

				yield return waitOneSec;
			}
		}
	}
}