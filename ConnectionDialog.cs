using BattleTech.UI;
using HarmonyLib;
using HBS;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleTechArchipelago;

[HarmonyPatch]
class ConnectionDialog : MonoBehaviour {

	private static readonly int WINDOW_ID = nameof(ConnectionDialog).GetHashCode();
	private const int FONT_SIZE = 20;
	private const float ROW_HEIGHT = FONT_SIZE * 1.5f;
	private const float WINDOW_WIDTH = 400.0f;
	private const float WINDOW_HORIZONTAL_MARGIN = 20.0f;
	private const float WINDOW_USABLE_SPACE = WINDOW_WIDTH - WINDOW_HORIZONTAL_MARGIN * 2.0f;
	private Rect _windowRect;
	private string _serverUrl =
#if DEBUG
		"ws://localhost";
#else
		"wss://archipelago.gg";
#endif
	private uint _port = 12345;
	private string _username = "Kerensky";
	private string _password = "";

	[HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Init))]
	[HarmonyPostfix]
	static void Init(MainMenu __instance) {
		GameObject connectionDialogGO = new GameObject("ConnectionDialog", [typeof(RectTransform), typeof(ConnectionDialog)]);
		connectionDialogGO.transform.parent = __instance.gameObject.transform;
	}

	void Awake() {
		// Use Camera.main.pixelHeight|Width instead of Screen?
		float windowWidth = WINDOW_WIDTH;
		float windowHeight = ROW_HEIGHT * 6.5f;
		float windowX = 50.0f;
		float windowY = Screen.height / 3.0f;
		_windowRect = new Rect(windowX, windowY, windowWidth, windowHeight);
	}

	// TODO delete when leaving the scene
	// TODO hide when e.g. mod manager appears
	void Update() {

	}

	void OnGUI() {
		GUI.skin.window.fontSize = FONT_SIZE;
		GUI.skin.window.padding.top = FONT_SIZE;
		_windowRect = GUI.Window(WINDOW_ID, _windowRect, OnWindow, "Archipelago Login");
	}

	// Returns pos, then moves it right by x
	private Vector2 AddXPos(ref Vector2 pos, float x) {
		Vector2 result = pos;
		pos.x += x;
		return result;
	}

	// Returns pos, then resets pos.x back to the start and adds one line worth of y
	private Vector2 NextLine(ref Vector2 pos) {
		Vector2 result = pos;
		pos.Set(WINDOW_HORIZONTAL_MARGIN, pos.y + ROW_HEIGHT);
		return result;
	}

	private void OnWindow(int windowId) {
		//GUI.DragWindow(_windowRect); // TODO rect inside which the window can be dragged
		GUI.color = Color.white;
		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
		GUI.skin.label.clipping = TextClipping.Overflow;
		GUI.skin.label.fontSize = FONT_SIZE; // TODO scale this?
		GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
		GUI.skin.textField.fontSize = FONT_SIZE * 3 / 4;

		Vector2 pos = new Vector2(WINDOW_HORIZONTAL_MARGIN, FONT_SIZE * 1.5f);

		{
			const string SERVER_LABEL = "Server URL: ";
			float serverLabelWidth = GUI.skin.label.CalcSize(new GUIContent(SERVER_LABEL)).x;
			Rect serverLabelRect = new Rect(AddXPos(ref pos, serverLabelWidth), new Vector2(serverLabelWidth, ROW_HEIGHT));
			GUI.Label(serverLabelRect, SERVER_LABEL);

			float serverTextWidth = WINDOW_USABLE_SPACE - serverLabelWidth;
			Rect serverTextRect = new Rect(NextLine(ref pos), new Vector2(serverTextWidth, ROW_HEIGHT));
			_serverUrl = GUI.TextField(serverTextRect, _serverUrl);
		}

		{
			const string PORT_LABEL = "Port: ";
			float portLabelWidth = GUI.skin.label.CalcSize(new GUIContent(PORT_LABEL)).x;
			Rect portLabelRect = new Rect(AddXPos(ref pos, portLabelWidth), new Vector2(portLabelWidth, ROW_HEIGHT));
			GUI.Label(portLabelRect, PORT_LABEL);

			float portTextWidth = WINDOW_USABLE_SPACE - portLabelWidth;
			Rect portTextRect = new Rect(NextLine(ref pos), new Vector2(portTextWidth, ROW_HEIGHT));
			string portStr = GUI.TextField(portTextRect, _port == 0 ? "" : _port.ToString());
			if (portStr.Length == 0)
				_port = 0;
			else if (uint.TryParse(portStr, out uint port))
				_port = port;
		}

		{
			const string USERNAME_LABEL = "Username: ";
			float usernameLabelWidth = GUI.skin.label.CalcSize(new GUIContent(USERNAME_LABEL)).x;
			Rect usernameLabelRect = new Rect(AddXPos(ref pos, usernameLabelWidth), new Vector2(usernameLabelWidth, ROW_HEIGHT));
			GUI.Label(usernameLabelRect, USERNAME_LABEL);

			float usernameTextWidth = WINDOW_USABLE_SPACE - usernameLabelWidth;
			Rect usernameTextRect = new Rect(NextLine(ref pos), new Vector2(usernameTextWidth, ROW_HEIGHT));
			_username = GUI.TextField(usernameTextRect, _username);
		}

		{
			const string PASSWORD_LABEL = "Password: ";
			float passwordLabelWidth = GUI.skin.label.CalcSize(new GUIContent(PASSWORD_LABEL)).x;
			Rect passwordLabelRect = new Rect(AddXPos(ref pos, passwordLabelWidth), new Vector2(passwordLabelWidth, ROW_HEIGHT));
			GUI.Label(passwordLabelRect, PASSWORD_LABEL);

			float passwordTextWidth = WINDOW_USABLE_SPACE - passwordLabelWidth;
			Rect passwordTextRect = new Rect(NextLine(ref pos), new Vector2(passwordTextWidth, ROW_HEIGHT));
			_password = GUI.PasswordField(passwordTextRect, _password, '*');
		}

		{
			bool isValid = _serverUrl.Length > 0 && _port > 0 && _username.Length > 0;
			GUI.color = isValid ? Color.white : Color.grey;
			Rect newGameRect = new Rect(pos, new Vector2(WINDOW_USABLE_SPACE, ROW_HEIGHT));
			if (GUI.Button(newGameRect, "Start New Game") && isValid) {
				bool success = ArchipelagoBridge.CreateSession(_serverUrl, _port, _username, _password);
				if (success) {
					MainMenu mainMenu = LazySingletonBehavior<UIManager>.Instance.GetOrCreateUIModule<MainMenu>();
					const string BUTTON_NAME = // See MainMenu.ReceiveButtonPress
#if DEBUG
						"New_Debug_Campaign";
#else
						"New_Campaign";
#endif
					NewGamePopup ngPopup = LazySingletonBehavior<UIManager>.Instance.GetOrCreateUIModule<NewGamePopup>();
					ngPopup.Initialize(BUTTON_NAME, mainMenu);
				}
			}
		}
	}
}
