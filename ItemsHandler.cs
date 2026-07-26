using Archipelago.MultiClient.Net.Models;
using BattleTech;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace BattleTechArchipelago;

public class ItemsHandler : MonoBehaviour {

	// int itemId -> what to do when that item is received from the AP server
	// Populated automatically using the [ItemHandler(id)] attribute
	protected static readonly IReadOnlyDictionary<long, Func<ItemsHandler, bool>> _itemHandlers;

	// Received items that need to be handled at the next opportunity
	protected List<ItemInfo> _itemQueue = new List<ItemInfo>();

	static ItemsHandler() {
		Dictionary<long, Func<ItemsHandler, bool>> itemHandlers = new Dictionary<long, Func<ItemsHandler, bool>>();
		_itemHandlers = itemHandlers;

		MethodInfo[] methods = typeof(ItemsHandler).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (MethodInfo method in methods) {
			ItemHandlerAttribute attribute = method.GetCustomAttribute<ItemHandlerAttribute>(inherit: true);
			if (attribute != null) {
				Assert.AreEqual(method.ReturnType, typeof(bool));
				Assert.AreEqual(method.GetParameters().Length, 0);
				Assert.IsFalse(_itemHandlers.ContainsKey(attribute.id));

				Func<ItemsHandler, bool> handler = (ItemsHandler self) => { return (bool)method.Invoke(self, []); };
				itemHandlers.Add(attribute.id, handler);
			}
		}
	}

	public static ItemsHandler Initialize() {
		GameObject go = new GameObject(nameof(ItemsHandler), [typeof(ItemsHandler)]);
		DontDestroyOnLoad(go);
		return go.GetComponent<ItemsHandler>();
	}

	void Update() {
		TryHandleItemQueue();
	}

	public void OnItemReceived(ItemInfo item) {
		lock (_itemQueue)
			_itemQueue.Add(item);
	}

	public void TryHandleItemQueue() {
		lock (_itemQueue) {
			for (int i = 0; i < _itemQueue.Count; ++i) {
				ItemInfo item = _itemQueue[i];

				if (!_itemHandlers.TryGetValue(item.ItemId, out Func<ItemsHandler, bool> itemAction)) {
					Main.Log.LogWarning($"No action found for item id={item.ItemId}, name='{item.ItemName}'");
					_itemQueue.RemoveAt(i);
					--i;
					continue;
				}

				bool handledAction = itemAction(this);
				if (handledAction) {
					Main.Log.Log($"Successfully handled {item.ItemName}");
					_itemQueue.RemoveAt(i);
					--i;
				}
			}
		}
	}

	[ItemHandler(1)]
	protected bool Add1MCash() {
		// See Battletech.SimGameState_Debug.isSimAvailable
		// Not sure if the check for Combat is necessary. Will anything go wrong if we add cash in combat?
		bool canAccessSimGame = UnityGameInstance.BattleTechGame.Simulation != null;// && UnityGameInstance.BattleTechGame.Combat == null;
		if (!canAccessSimGame)
			return false;

		SimGameState simGame = UnityGameInstance.BattleTechGame.Simulation;
		simGame.AddFunds(1000000, "Archipelago");
		return true;
	}
}
