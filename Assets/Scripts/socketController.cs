using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using LitJson;
using isg;

namespace isg
{
	public class socketController : MonoBehaviour
	{
		private rootController rc = null;

		public socketController ()
		{
			rc = new rootController ();
		}

		public void wsClientPush ()
		{
			JsonData req = new JsonData ();
			req ["step"] = "s0";
			req ["uid"] = userData.userId;
			req ["uName"] = userData.userName;

			wsSend (req);
		}

		public void getUserInfo ()
		{
			JsonData req = new JsonData ();
			req ["step"] = "u1";
			req ["uid"] = userData.userId;
			req ["uName"] = userData.userName;

			wsSend (req);
		}

		// ws connect.
		public void newWs ()
		{
			if (rootData.serverInfo == null) {
				return;
			}

			string getWsFqdn = rootData.serverInfo ["getWsFqdn"].ToString ();

			wsData.ws = new WebSocket (getWsFqdn);
			wsData.ws.OnOpen += onOpenHandler;
			wsData.ws.OnClose += onCloseHandler;
			wsData.ws.OnMessage += onMessageHandler;
			wsData.ws.OnError += onErrorHandler;
			wsData.ws.Connect ();
		}

		public void wsConn ()
		{
			if (wsData.ws == null) {
				newWs ();
			}
		}

		public void wsSend (JsonData req)
		{
			if (wsData.ws == null || !wsData.isOpen) {
				newWs ();
				return;
			}

			wsData.ws.Send (req.ToJson ());
		}

		void onOpenHandler (object sender, System.EventArgs e)
		{
			wsData.isOpen = true;
		}

		void onMessageHandler (object sender, MessageEventArgs e)
		{
			try {
				JsonData res = JsonMapper.ToObject (e.Data);
				IDictionary dic = res as IDictionary;

				wsData.debug = res.ToJson ();

				if (dic.Contains ("err")) {
					wsData.error = res ["err"].ToString ();
					return;
				}

				switch (res ["step"].ToString ()) {
				case "ttr1":
					if (bool.Parse (res ["isMax"].ToString ())) {
						userData.isMaxTelescope = true;
					} else {
						userData.isMaxTelescope = false;
					}
					break;

				// event.
				case "e1":
					userData.eventRes = res;
					break;

				// user.
				case "u1":
					userData.res = res;
					break;

				// tier.
				case "ti1":
					userData.tierRes = res;
					break;

				// battle history.
				case "h1":
					userData.battleHistoryRes = res;
					break;

				// ranking
				case "rk1":
					rankData.rankRes = res;
					break;

				// battle.
				case "b1":
				case "b3":
				case "b5":
				case "b6":
					battleData.roomRes = res;
					break;

				// card.
				case "c0":
					exploreData.res = res;
					break;
				case "c1":
					cardData.saveRes = res;
					break;
				case "c2":
					cardData.cardList = res;
					break;
				case "c3":
					cardData.betterRes = res;
					break;
				case "c4":
					cardData.sellRes = res;
					break;

				// map.
				case "m1":
					mapData.mapRes = res;
					break;

				// adventure.
				case "ad1":
					botData.botList = res;
					break;

				// bot battle.
				case "bb1":
					battleData.roomRes = res;
					break;

				// receipt.
				case "pr1":
					storeData.receiptRes = res;
					break;

				// tutorial.
				case "tr1":
					if (dic.Contains ("isTutorial")) {
						tutorialData.res = res;
						tutorialData.isTutorial = bool.Parse (res ["isTutorial"].ToString ());
					}

					break;
				}
			} catch (JsonException exp) {
				Debug.Log (exp.Message);
			}
		}

		void onErrorHandler (object sender, ErrorEventArgs e)
		{
			wsData.error = e.Message;
			retryHandler ();
		}

		void onCloseHandler (object sender, CloseEventArgs e)
		{
			wsData.error = "서버 접속 오류.";
			retryHandler ();
		}

		void retryHandler ()
		{
			wsData.isError = true;
			wsData.ws = null;
		}
	}
}