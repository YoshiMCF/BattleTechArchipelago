
using HarmonyLib;
using HBS.Logging;
using System;

namespace BattleTechArchipelago;

public static class Main {
	public static readonly ILog Log = Logger.GetLogger(nameof(BattleTechArchipelago));
	public static void Start() {
		Log.Log("Starting");

		PatchAll();

		Log.Log("Started");
	}

	private static void PatchAll() {
		Log.Log("Patching");

		// apply all patches that are in classes annotated with [HarmonyPatch]
		Harmony.CreateAndPatchAll(typeof(Main).Assembly);

		// run a specific patch found in a class which wasn't annotated with HarmonyPatch and therefore wasn't applied earlier
		Harmony.CreateAndPatchAll(typeof(Main));
	}

	[HarmonyPatch(typeof(VersionInfo), nameof(VersionInfo.GetReleaseVersion))]
	[HarmonyPostfix]
	[HarmonyAfter("io.github.mpstark.ModTek")]
	static void GetReleaseVersion(ref string __result) {
		var old = __result;
		__result = old + "\nBTArchipelago v0.0.1";
	}
}
