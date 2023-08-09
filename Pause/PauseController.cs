using EFT;
using EFT.UI.BattleTimer;
using UnityEngine;
using Comfort.Common;
using System;
using System.Reflection;
using System.Collections;

namespace Pause
{
    public class PauseController : MonoBehaviour
    {
       internal static bool isPaused { get; private set; } = false;
       internal static DateTime? pausedDate { get; private set; } = null;
       internal static DateTime? unpausedDate { get; private set; } = null;

       private GameTimerClass _gameTimerClass;
       private MainTimerPanel _mainTimerPanel;
       private GamePlayerOwner _gamePlayerOwner;

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
                // this feels slow and doesn't work if you are pressing other keys
                //if (Plugin.TogglePause.Value.IsDown()) 

                // this will break if user assigns a modifier key, but you wouldn't do that would you?
                if (Input.GetKeyDown(Plugin.TogglePause.Value.MainKey)) 
                {
                    isPaused = !isPaused;

                    // get references to necessary game objects
                    AbstractGame abstractGame = GameObject.FindObjectOfType<AbstractGame>();
                    _mainTimerPanel = GameObject.FindObjectOfType<MainTimerPanel>();
                    _gameTimerClass = abstractGame.GameTimer;

                    Plugin.Log.LogInfo($"MainTimerPanel found: {_mainTimerPanel != null}");
                    Plugin.Log.LogInfo($"GameTimerClass found: {_gameTimerClass != null}");

                    if (isPaused)
                    {
                        Time.timeScale = 0f;
                        pausedDate = DateTime.UtcNow;

                        _gamePlayerOwner.enabled = false;
                        _mainTimerPanel.DisplayTimer();
                    }
                    else 
                    {
                        Time.timeScale = 1f;
                        unpausedDate = DateTime.UtcNow;

                        _gamePlayerOwner.enabled = true;
                        StartCoroutine(CoHidePanel());

                        var timePaused = GetTimePaused();

                        UpdateTimers(timePaused);

                        Plugin.Log.LogInfo($"Time spent paused: {timePaused}");
                    }

                    Plugin.Log.LogInfo($"Game paused: [{isPaused}]");
                    Plugin.Log.LogInfo($"Past Time: {_gameTimerClass.PastTime}");
                }
            } 
       }

       private IEnumerator CoHidePanel() 
       {
			yield return new WaitForSeconds(4f);
			_mainTimerPanel.HideTimer();
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
            // dateTime_0 seems to be escape date
            // works opposite of GameTimerClass which depends on start date
            var fi2 = typeof(TimerPanel).GetField("dateTime_0", BindingFlags.Instance | BindingFlags.NonPublic);

            // get the underlying start date value from GameTimerClass nullable_0 private field
            var startDate = fi1.GetValue(_gameTimerClass) as DateTime?;
            // get the underlying escape date value from TimerPanel dateTime_0 private field
            var escapeDate = fi2.GetValue(_mainTimerPanel) as DateTime?;

            // add the time spent paused to the underlying start date 
            fi1.SetValue(_gameTimerClass, startDate.Value.Add(timePaused));

            // add the time spent paused 
            fi2.SetValue(_mainTimerPanel, escapeDate.Value.Add(timePaused));
       }

       bool IsGameReady() 
       {
            if (this.GameWorld == null) 
            {
                return false;
            }
            else if (this.Player == null) 
            {
                return false;
            }
            else if (this.GamePlayerOwner == null) 
            {
                _gamePlayerOwner = this.Player.GetComponent<GamePlayerOwner>();
                return false;
            }

            var fi = typeof(PlayerOwner).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var state = (int)fi.GetValue(GamePlayerOwner);

            // 1 = Started
            if (state != 1) 
            {
                return false;
            }
            
            return true;
       }

       GameWorld GameWorld { get => Singleton<GameWorld>.Instance; }
       Player Player { get => this.GameWorld.MainPlayer; }
       GamePlayerOwner GamePlayerOwner { get => _gamePlayerOwner; }
    }
}