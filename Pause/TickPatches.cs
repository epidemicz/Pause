using EFT;
using EFT.UI.BattleTimer;
using System.Reflection;
using Aki.Reflection.Patching;
using System;
using UnityEngine.UI;
using TMPro;
using BepInEx;

namespace Pause
{
    public class WorldTickPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod("DoWorldTick", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix() => !PauseController.isPaused;
    }

    public class OtherWorldTickPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod("DoOtherWorldTick", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        //static bool Prefix() => !PauseController.isPaused;
        static bool Prefix(GameWorld __instance) 
        {
            if (PauseController.isPaused) 
            {
                return false;
            }

            return true;
        }
    }

    public class ActiveHealthControllerClassPatch : ModulePatch 
    {
        protected override MethodBase GetTargetMethod() => typeof(ActiveHealthControllerClass).GetMethod("ManualUpdate", BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix() => !PauseController.isPaused;
    }

    public class GameTimerClassPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GameTimerClass).GetMethod("method_0", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix() 
        {
            if (PauseController.isPaused) 
            {
                return false;
            }

            return true;
        } 
    }

    public class TimerPanelPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(TimerPanel).GetMethod("UpdateTimer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        [PatchPrefix]
        static bool Prefix(TextMeshProUGUI ____timerText)
        {
            if (PauseController.isPaused) 
            {
                ____timerText.SetMonospaceText("PAUSED", false);
                return false;
            }
            
            return true;
        }
    }
}