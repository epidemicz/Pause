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

        // track time spent paused
        internal static DateTime? pausedDate { get; private set; } = null;
        internal static DateTime? unpausedDate { get; private set; } = null;

        // used to determine if game is in the right state to be able to pause
        private GameWorld _gameWorld;
        private Player _player;
        private GamePlayerOwner _gamePlayerOwner;

        // GameTimerClass controls actual raid time
        private GameTimerClass _gameTimerClass;
        // MainTimerPanel controls on screen raid time
        private MainTimerPanel _mainTimerPanel;

        private TimeSpan GetTimePaused()
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
                        Pause();
                    }
                    else
                    {
                        Unpause();
                    }

                    Plugin.Log.LogInfo($"Game paused: [{isPaused}]");
                    Plugin.Log.LogInfo($"Past Time: {_gameTimerClass.PastTime}");
                }
            }
        }

        private void Pause()
        {
            Time.timeScale = 0f;
            pausedDate = DateTime.UtcNow;

            // disable player control
            _gamePlayerOwner.enabled = false;

            ShowTimer();
        }

        private void Unpause()
        {
            Time.timeScale = 1f;
            unpausedDate = DateTime.UtcNow;

            // enable player control
            _gamePlayerOwner.enabled = true;
            StartCoroutine(CoHideTimer());

            // get timespan spent paused
            var timePaused = GetTimePaused();

            // add time back to timers
            UpdateTimers(GetTimePaused());

            Plugin.Log.LogInfo($"Time spent paused: {timePaused}");
        }

        private void ShowTimer()
        {
            if (_mainTimerPanel != null)
            {
                _mainTimerPanel.DisplayTimer();
            }
        }

        private IEnumerator CoHideTimer()
        {
            if (_mainTimerPanel != null)
            {
                yield return new WaitForSeconds(4f);
                _mainTimerPanel.HideTimer();
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
            if (_gameWorld == null)
            {
                _gameWorld = Singleton<GameWorld>.Instance;
                return false;
            }
            else if (_player == null)
            {
                _player = _gameWorld.MainPlayer;
                return false;
            }
            else if (_gamePlayerOwner == null)
            {
                _gamePlayerOwner = _player.GetComponent<GamePlayerOwner>();
                return false;
            }
            else if (_gamePlayerOwner is HideoutPlayerOwner)
            {
                // disable pause when in the hideout
                return false;
            }
            else if (_gamePlayerOwner != null)
            {
                var fi = typeof(PlayerOwner).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var state = (int)fi.GetValue(_gamePlayerOwner);

                // 1 = Started
                if (state == 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}