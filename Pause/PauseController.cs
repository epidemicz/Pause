using EFT;
using EFT.UI.BattleTimer;
using UnityEngine;
using Comfort.Common;
using System;
using System.Reflection;

namespace Pause
{
    public class PauseController : MonoBehaviour
    {
       internal static bool isPaused { get; private set; } = false;
       internal static DateTime? pausedDate { get; private set; } = null;
       internal static DateTime? unpausedDate { get; private set; } = null;

       private GameTimerClass _gameTimerClass;
       private MainTimerPanel _mainTimerPanel;

       internal static TimeSpan GetTimePaused() 
       {
            if (pausedDate.HasValue && unpausedDate.HasValue) 
            {
                return unpausedDate.Value - pausedDate.Value;
            }

            return TimeSpan.Zero;
       }
       
       void Update()
       {
            if (IsGameReady())
            {
                if (Plugin.TogglePause.Value.IsDown()) 
                {
                    isPaused = !isPaused;

                    var playerOwner = player.GetComponent<GamePlayerOwner>();

                    // get references to necessary game objects
                    AbstractGame abstractGame = GameObject.FindObjectOfType<AbstractGame>();
                    _mainTimerPanel = GameObject.FindObjectOfType<MainTimerPanel>();
                    _gameTimerClass = abstractGame.GameTimer;

                    Plugin.Log.LogInfo($"MainTimerPanel found: {_mainTimerPanel != null}");
                    Plugin.Log.LogInfo($"GameTimerClass found: {_gameTimerClass != null}");

                    if (isPaused)
                    {
                        Time.timeScale = 0f;
                        playerOwner.enabled = false;
                        pausedDate = DateTime.UtcNow;
                        _mainTimerPanel.DisplayTimer();
                    }
                    else 
                    {
                        Time.timeScale = 1f;
                        playerOwner.enabled = true;
                        unpausedDate = DateTime.UtcNow;
                        _mainTimerPanel.HideTimer();

                        var timePaused = GetTimePaused();

                        UpdateTimers(timePaused);

                        Plugin.Log.LogInfo($"Time spent paused: {timePaused}");
                    }

                    Plugin.Log.LogInfo($"Game paused: [{isPaused}]");
                    Plugin.Log.LogInfo($"Past Time: {_gameTimerClass.PastTime}");
                }
            } 
       }

       private void UpdateTimers(TimeSpan timePaused)
       {
            // GameTimerClass controls the overall game state
            // If PastTime > SessionTime game ends
            // PastTime is calculated based on nullable_0
            var fi1 = typeof(GameTimerClass).GetField("nullable_0", BindingFlags.Instance | BindingFlags.NonPublic);

            // MainTimerPanel is the in-game ui clock, which operates separately
            // from the timer in GameTimerClass
            // needs to be cast as a TimerPanel
            var fi2 = typeof(TimerPanel).GetField("dateTime_0", BindingFlags.Instance | BindingFlags.NonPublic);

            // get the underlying startDate value from GameTimerClass nullable_0 private field
            // The in-game ui clock has a separate value but maybe we can use this one
            var startDate = fi1.GetValue(_gameTimerClass) as DateTime?;

            // increment the time spent paused to the underlying start date values
            // so we gain raid time
            var newDate = startDate.Value.Add(timePaused);

            fi1.SetValue(_gameTimerClass, newDate);
            fi2.SetValue(_mainTimerPanel, newDate);
       }

       bool IsGameReady() => gameWorld != null && gameWorld.AllAlivePlayersList != null && gameWorld.AllAlivePlayersList.Count > 0 && player != null;
       GameWorld gameWorld { get => Singleton<GameWorld>.Instance; }
       Player player { get => gameWorld.MainPlayer; }
    }
}