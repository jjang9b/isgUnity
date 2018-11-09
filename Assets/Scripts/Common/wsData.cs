using System;
using WebSocketSharp;
using UnityEngine;
using LitJson;

namespace isg
{
	public class wsData
	{
		public static WebSocket ws;
		public static string debug;
		public static string error = null;
		public static bool isOpen = false;
		public static bool isError = false;

		public static bool isWs ()
		{
			return (wsData.ws != null);
		}
	}
}

