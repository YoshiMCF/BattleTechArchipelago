using BattleTech;
using HarmonyLib;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace BattleTechArchipelago;

[HarmonyPatch]
public class ChecksHandler : MonoBehaviour {

	// int checkId -> function that returns true if the check has succeeded
	// Populated automatically using the [CheckHandler(id)] attribute
	protected static readonly IReadOnlyDictionary<long, Func<ChecksHandler, bool>> _checkHandlers;

	// Checks that have passed and no longer need to be checked again
	protected HashSet<long> _passedChecks = new HashSet<long>();

	static ChecksHandler() {
		Dictionary<long, Func<ChecksHandler, bool>> checkHandlers = new Dictionary<long, Func<ChecksHandler, bool>>();
		_checkHandlers = checkHandlers;

		MethodInfo[] methods = typeof(ChecksHandler).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (MethodInfo method in methods) {
			CheckHandlerAttribute attribute = method.GetCustomAttribute<CheckHandlerAttribute>(inherit: true);
			if (attribute != null) {
				Assert.AreEqual(method.ReturnType, typeof(bool));
				Assert.AreEqual(method.GetParameters().Length, 0);
				Assert.IsFalse(_checkHandlers.ContainsKey(attribute.id));

				Func<ChecksHandler, bool> handler = (ChecksHandler self) => { return (bool)method.Invoke(self, []); };
				checkHandlers.Add(attribute.id, handler);
			}
		}
	}

	public static ChecksHandler Initialize() {
		GameObject go = new GameObject(nameof(ChecksHandler), [typeof(ChecksHandler)]);
		DontDestroyOnLoad(go);
		return go.GetComponent<ChecksHandler>();
	}

	[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.ApplyEventAction))]
	[HarmonyPostfix]
	static void ApplyEventAction(SimGameResultAction action, object additionalObject) {
		Main.Log.LogDebug($"Event action {action} ({additionalObject}) applied, starting checks");
		ArchipelagoBridge.checksHandler?.DoChecks();
	}

	private void DoChecks() {
		foreach (KeyValuePair<long, Func<ChecksHandler, bool>> kvp in _checkHandlers) {
			if (_passedChecks.Contains(kvp.Key))
				continue;

			if (kvp.Value(this)) {
				ArchipelagoBridge.CompleteLocationCheck(kvp.Key);
				_passedChecks.Add(kvp.Key);
			}
		}
	}

	[CheckHandler(1)]
	protected bool CheckForArgo() {
		SimGameState SimGame = UnityGameInstance.BattleTechGame.Simulation;
		return SimGame != null && SimGame.CurDropship == DropshipType.Argo;
	}
}
