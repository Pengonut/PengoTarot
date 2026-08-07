
#nullable enable
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patches
{
    [HarmonyPatch(typeof(LocManager), "GetTable")]
    public static class LocManagerGetTablePatch
    {
        private static bool _injected = false;

        [HarmonyPostfix]
        static void Postfix()
        {
            if (_injected || LocManager.Instance == null)
                return;

            _injected = true;
            string lang = LocManager.Instance.Language;

            if (lang == "zhs")
                TarLocHelper.InjectAll();
            else if (lang == "jpn")
                TarJapaneseLocHelper.InjectAll();
            else if (lang == "kor")
                TarKoreanLocHelper.InjectAll();
            else
                TarEnglishLocHelper.InjectAll();

            PengoTarot.BalatroEffect.LocExtension.Inject();
            ConfigFloatingWindowLoc.Inject();
        }
    }
}