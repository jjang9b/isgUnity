using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using WebSocketSharp;
using GooglePlayGames;
using LitJson;
using isg;

namespace isg
{
	public class storeController : MonoBehaviour
	{
		public GameObject productScroll;
		public GameObject content;
		public GameObject resultObj;
		public GameObject overlay;

		private rootController rc = null;
		private socketController sc = null;
		private WaitForSeconds moveCheckSec = new WaitForSeconds (0.05f);
		private WaitForSeconds waitSec = new WaitForSeconds (1);

		private bool isPurchaseSuccess = false;

		void Start ()
		{
			rc = new rootController ();
			sc = new socketController ();
		}

		void CoroutineInit ()
		{
			StartCoroutine (moveEndCoroutine ());
			StartCoroutine (receiptCoroutine ());
		}

		void UiInit ()
		{
			storeData.resultObj = resultObj;
			storeData.overlay = overlay;
			rootData.topNavObj.SetActive (true);
			resultObj.SetActive (false);
			overlay.SetActive (false);

			productScroll.GetComponent<ScrollRect> ().verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			setCellSize (4, content);
		}

		public void showStore ()
		{
			CoroutineInit ();
			rootData.clickBody = rootData.storeBody;
			rootData.isMoveStart = true;
			UiInit ();
		}

		public void showResult ()
		{
			storeData.overlay.SetActive (true);
			storeData.resultObj.SetActive (true);
		}

		public void hideResult ()
		{
			storeData.overlay.SetActive (false);
			storeData.resultObj.SetActive (false);
		}

		private void setCellSize (int cellCount, GameObject content)
		{
			RectTransform parent = content.transform.GetComponent<RectTransform> ();
			GridLayoutGroup grid = content.transform.GetComponent<GridLayoutGroup> ();

			float spaceW = (grid.padding.left + grid.padding.right) + (grid.spacing.x * (cellCount - 1));
			float maxW = parent.rect.width - spaceW;
			float width = Mathf.Min (parent.rect.width - (grid.padding.left + grid.padding.right) - (grid.spacing.x * (cellCount - 1)), maxW);

			grid.cellSize = new Vector2 (width / cellCount, grid.cellSize.y);
			grid.constraintCount = cellCount;
		}

		private IEnumerator moveEndCoroutine ()
		{
			while (true) {
				if (rootData.isMoveEnd) {
					rootData.currentBody = rootData.storeBody;
					rootData.isMoveEnd = false;
					yield break;
				}

				yield return moveCheckSec;
			}
		}

		private IEnumerator receiptCoroutine ()
		{
			while (true) {
				receiptCheck ();

				yield return waitSec;
			}
		}

		private void receiptCheck ()
		{
			JsonData res = storeData.receiptRes;

			if (res == null) {
				return;
			}

			if (bool.Parse (res ["isReceipt"].ToString ())) {
				storeData.resultObj.transform.Find ("Text").GetComponent<Text> ().text = message.purchaseSuccess;
			} else {
				storeData.resultObj.transform.Find ("Text").GetComponent<Text> ().text = message.purchaseFail;
			}

			storeData.receiptRes = null;
			showResult ();			
		}
	}
}