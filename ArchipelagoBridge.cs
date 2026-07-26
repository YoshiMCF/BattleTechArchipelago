using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Archipelago.Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace BattleTechArchipelago;

public static class ArchipelagoBridge {

	private static ArchipelagoSession _session;
	private static ItemsHandler _itemsHandler;

	public static bool CreateSession(string serverUrl, uint port, string username, string password) {
		Main.Log.Log($"Creating session {serverUrl}:{port}");

		_session = ArchipelagoSessionFactory.CreateSession(serverUrl, (int)port);
		if (_session == null)
			return false;

		CreateHandlers();
		SubscribeToArchipelagoCallbacks();

		Main.Log.Log($"Connecting and logging in as {username}");

		const string GAME_NAME = "BATTLETECH";
		LoginResult result = _session.TryConnectAndLogin(GAME_NAME, username, ItemsHandlingFlags.AllItems, password: password);
		Main.Log.Log(result.Successful ? "Login succeeded" : "Login failed");
		return result.Successful;
	}

	private static void CreateHandlers() {
		if (_itemsHandler != null)
			UnityEngine.Object.Destroy(_itemsHandler.gameObject);

		_itemsHandler = ItemsHandler.Initialize();
	}

	#region Callbacks
	private static void SubscribeToArchipelagoCallbacks() {
		Main.Log.Log("Subscribing to callbacks");

		_session.Socket.ErrorReceived += OnErrorReceived;
		_session.Socket.PacketReceived += OnPacketReceived;
		_session.Socket.PacketsSent += OnPacketSent;
		_session.Socket.SocketClosed += OnSocketClosed;
		_session.Socket.SocketOpened += OnSocketOpened;

		_session.Items.ItemReceived += OnItemReceived;

		_session.Locations.CheckedLocationsUpdated += OnCheckedLocationsUpdated;

		_session.MessageLog.OnMessageReceived += OnMessageReceived;
	}

	private static void OnErrorReceived(Exception e, string message) {
		Main.Log.LogWarning($"Error received: {message}\n{e}");
	}

	private static void OnPacketReceived(ArchipelagoPacketBase packet) {
		Main.Log.LogDebug($"Packet received: {packet}");
	}

	private static void OnPacketSent(ArchipelagoPacketBase[] packets) {
		string packetsStr = string.Join("\n", (object[])packets);
		Main.Log.LogDebug($"Sent {packets.Length} packets: {packetsStr}");
	}

	private static void OnSocketClosed(string reason) {
		Main.Log.Log($"Socket closed: {reason}");
	}

	private static void OnSocketOpened() {
		Main.Log.Log($"Socket opened");
	}

	private static void OnItemReceived(ReceivedItemsHelper items) {
		ItemInfo item = items.DequeueItem();
		Main.Log.Log($"Received item: {item.ItemName}");

		_itemsHandler.OnItemReceived(item);
	}

	private static void OnCheckedLocationsUpdated(ReadOnlyCollection<long> newCheckedLocations) {
		string locationsStr = string.Join(", ", newCheckedLocations);
		Main.Log.Log($"Checked locations updated: [{locationsStr}]");
	}

	private static void OnMessageReceived(LogMessage message) {
		Main.Log.Log($"Message received: {message}");
	}
	#endregion

	#region Locations
	public static void CompleteLocationCheck(long locationId) {
		Main.Log.LogDebug("Completed location check: {locationId}");
		_session.Locations.CompleteLocationChecks(locationId);
	}

	public static void ScoutLocation(long locationId) {
		Main.Log.LogDebug("Scouting location {locationId}");
		// _session.Locations.ScoutLocationsAsync(); // TODO async message queue
		throw new System.NotImplementedException();
	}
	#endregion

	// TODO death link
}
