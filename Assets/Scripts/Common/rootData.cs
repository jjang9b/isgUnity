using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace isg
{
	public static class rootData
	{
		public static float buildVersion = 1.55f;
		public static bool isTest = true;
		public static string serverInfoUrl = "http://bbplayisg.com:41999/get/serverInfo";
		public static string siteUrl = "http://bbplayisg.com";
		public static JsonData serverInfo = null;

		public static int version = 1;
		public static float fillAmount = 0;
		public static GameObject currentBody;
		public static GameObject clickBody;
		public static GameObject mainBody;
		public static GameObject settingBody;
		public static GameObject exploreBody;
		public static GameObject playerBody;
		public static GameObject adventureBody;
		public static GameObject cardBody;
		public static GameObject battleBody;
		public static GameObject storeBody;
		public static GameObject topNavObj;
		public static GameObject mainTopNavObj;
		public static GameObject menuNavObj;
		public static GameObject rootOverlayObj;
		public static GameObject mainOverlayObj;

		public static GameObject mainTutorialObj;
		public static GameObject cardTutorialObj;
		public static GameObject adventureTutorialObj;

		public static AudioSource mainBgm;

		public static rootController rc;

		public static bool isMoveStart = false;
		public static bool isMoveEnd = false;
	}
}