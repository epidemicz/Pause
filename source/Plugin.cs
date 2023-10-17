using BepInEx;
using UnityEngine;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace Pause
{
    [BepInPlugin("com.epi.pause", "PAUSE", "1.0.4")]
    public class Plugin : BaseUnityPlugin
    {
        public static GameObject Hook;
        const string KeybindSectionName = "Keybinds";
        const string OptionsSectionName = "Options";
        internal static ConfigEntry<KeyboardShortcut> TogglePause;
        //internal static ConfigEntry<bool> UseAlternatePause;
        internal static ManualLogSource Log;

        void Awake()
        {
            Log = base.Logger;
            TogglePause = Config.Bind(KeybindSectionName, "Toggle Pause", new KeyboardShortcut(KeyCode.P));
            //UseAlternatePause = Config.Bind(OptionsSectionName, "Use Alternate Pause", false);
            Logger.LogInfo($"PAUSE: Loading");
            new WorldTickPatch().Enable();
            new OtherWorldTickPatch().Enable();
            // seems to be removed in 3.7.0
            // new ActiveHealthControllerClassPatch().Enable();
            new GameTimerClassPatch().Enable();
            new TimerPanelPatch().Enable();
            Hook = new GameObject("PAUSE");
            Hook.AddComponent<PauseController>();
            DontDestroyOnLoad(Hook);
        }
    }
}