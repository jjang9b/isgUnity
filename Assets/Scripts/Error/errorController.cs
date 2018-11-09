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
	public class errorController : MonoBehaviour
	{
		public Text errorTxt;
	
		private socketController sc = null;
		private rootController rc = null;

		void Start ()
		{
			sc = new socketController ();
			rc = new rootController ();
			errorTxt.text = wsData.error;
		}

		public void reConnect ()
		{
			SceneManager.LoadScene ("logoScene", LoadSceneMode.Single);
			rc.resetRootValue ();
			sc.newWs ();
		}
	}
}