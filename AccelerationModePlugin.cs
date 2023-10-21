using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
// using System.Linq;
// using System.Reflection;
// using System.Security.Principal;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using YARG.Gameplay;
using TMPro;
using UnityEngine.SceneManagement;
using System.Security.Permissions;
// using YARG.Core.Chart;
// using YARG.Core.Engine;

namespace AccelerationMode
{
    [HarmonyPatch(typeof(YARG.Gameplay.Player.FiveFretPlayer), "OnNoteMissed")]
    public class FiveFretMissNoteHandler
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            AccelerationModePlugin.Instance.FiveFretMissNotePostfix();
        }
    }
    [HarmonyPatch(typeof(YARG.Gameplay.Player.FiveFretPlayer), "OnNoteHit")]
    public class FiveFretHitNoteHandler
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            AccelerationModePlugin.Instance.FiveFretHitNotePostfix();
        }
    }

    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class AccelerationModePlugin : BaseUnityPlugin
    {
        public static AccelerationModePlugin Instance {  get; private set; }
        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);
        public AccelerationModePlugin()
        {
            Instance = this;
        }

        private const string MyGUID = "com.yoshibyl.AccelerationMode";
        private const string PluginName = "AccelerationMode";
        private const string VersionString = "0.2.0";

        public float netSpeed;
        public float maxSpeed;
        public bool spActive;
        public bool maxSpeedEnabled;
        public bool accelModeEnabled;

        public void ToggleAccelMode()
        {
            if (!isPlayingSong)
            {
                if (accelModeEnabled)
                {
                    cfgAccelModeToggle.SetSerializedValue("false");
                    accelModeEnabled = false;
                }
                else
                {
                    cfgAccelModeToggle.SetSerializedValue("true");
                    accelModeEnabled = true;
                } // */
                UpdateWatermark("<size=20>(click to toggle)</size><br>");
            }
        }

        // private readonly Assembly asm = Assembly.Load("Assembly-CSharp");
        internal void FiveFretMissNotePostfix()
        {
            if (accelModeEnabled)
            {
                if (!playerOne.Stats.IsStarPowerActive)
                {
                    netSpeed -= gtrMissSlowdown;
                    if (netSpeed < 1f)
                        netSpeed = 1f;
                    if (gameMgr.ActualSongSpeed > 1f)
                        gameMgr.SetSongSpeed(netSpeed);
                }
                scoreTMP.text = "<mspace=0.538em>" + netSpeed.ToString("0.00") + "×</mspace>  <size=18>Speed</size>";
            }
        }

        internal void FiveFretHitNotePostfix()
        {
            if (accelModeEnabled)
            {
                if (!playerOne.Stats.IsStarPowerActive && gameMgr.ActualSongSpeed < songSpeedCap)
                {
                    netSpeed += gtrHitSpeedup;
                    
                    if (netSpeed > songSpeedCap)
                        netSpeed = songSpeedCap;
                    if (gameMgr.ActualSongSpeed < songSpeedCap)
                        gameMgr.SetSongSpeed(netSpeed);
                }
                playerOne.Stats.Score = 0;
                scoreTMP.text = "<mspace=0.538em>" + netSpeed.ToString("0.00") + "×</mspace>  <size=18>Speed</size>";
            }
        }

        // Drums acceleration mode planned (maybe)
        /*
        internal void DrumHitPostfix()
        {
            if (accelModeEnabled)
            {
                if (!spActive)
                {
                    gmObj = GameObject.Find("Game Manager");
                    gameMgr = gmObj.GetComponent<YARG.Gameplay.GameManager>();
                    netSpeed += 0.0025f;
                    if (netSpeed > 2f) { netSpeed = 2f; }
                    gameMgr.SetSongSpeed(netSpeed);
                }
                playerOne.Stats.Score = 0;
            }
        }
        // */

        GameObject gmObj;
        TextMeshProUGUI scoreTMP;
        // YARG.Core.Chart.SongChart chart;
        double currentTime;
        // string dbgTxt;
        YARG.Gameplay.GameManager gameMgr;
        bool isPlayingSong;
        bool changedWM;
        // GameObject trackObj;  // Highway
        YARG.Gameplay.Player.BasePlayer playerOne;
        
        Button btnAccel;

        // i tried to make a thing to show notifications using the in-game toasts
        /*
        public void ShowToast(string msg = "", string level = "Info")
        {
            GameObject toastObj = GameObject.Find("Persistent Canvas/Toast Manager/Toast Fab");
            toastObj.SetActive(true);
            if (msg != "")
            {
                if (level.Contains("Info"))
                {
                    YARG.Menu.Persistent.ToastManager.ToastInformation(msg);
                    return;
                }
                if (level.Contains("Warn"))
                {
                    YARG.Menu.Persistent.ToastManager.ToastWarning(msg);
                    return;
                }
                if (level.Contains("Succ"))
                {
                    YARG.Menu.Persistent.ToastManager.ToastSuccess(msg);
                    return;
                }
                if (level.Contains("Error"))
                {
                    YARG.Menu.Persistent.ToastManager.ToastWarning(msg);
                    return;
                }

            }
        }
        // */

        public void UpdateWatermark(string msg = "")
        {
            GameObject wmObj = GameObject.Find("Watermark Container");
            // GameObject topBar = GameObject.Find("Info Container");
            if (wmObj != null)
            {
                TMPro.TextMeshProUGUI wmTmp = wmObj.GetComponentInChildren<TextMeshProUGUI>();
                string accelStatusPrefix = "<color=#00CC00>ON";
                if (!accelModeEnabled)
                {
                    accelStatusPrefix = "<color=#FF0000>OFF";
                }

                wmTmp.text = "<b>YARG " + YARG.GlobalVariables.CurrentVersion.ToString() + "</b> Development Build <i>*MODDED*</i><br><b>Acceleration Mode: " + accelStatusPrefix + "</color></b><br>" + msg;
                wmTmp.alignment = TextAlignmentOptions.TopRight;
            }
        }

        // <CONFIG>
        ConfigFile cfg;
        public ConfigEntry<float> cfgSpeedCap;
        public ConfigEntry<bool>  cfgSpeedCapEnabled;
        public ConfigEntry<float> cfgGtrHitSpeedup;
        public ConfigEntry<float> cfgGtrMissSlowdown;
        public ConfigEntry<bool> cfgAccelModeToggle;

        public float songSpeedCap;
        public bool  songSpeedCapEnabled;
        public float gtrHitSpeedup;
        public float gtrMissSlowdown;
        // </CONFIG>

        public void Awake()
        {
            // init config stuff
            cfg = new ConfigFile(Path.Combine(Paths.ConfigPath, "AccelerationMode.cfg"), true);

            cfgAccelModeToggle = cfg.Bind("General",
                "acceleration_mode_enabled",
                true,
                "Enables the Acceleration Mode functionality.  Can be controlled in-game in the top-right corner.");
            cfgSpeedCapEnabled = cfg.Bind("General",
                "enable_speed_cap",
                true,
                "Whether the song speed should be limited.");
            cfgSpeedCap = cfg.Bind("General",
                "speed_cap_percent",
                200f,
                "The maximum song speed.  Only applies if enable_speed_cap is set to true.");

            cfgGtrHitSpeedup = cfg.Bind("Guitar",
                "hit_speedup_percent",
                0.25f,
                "Percentage to raise song speed by with each note hit.");
            cfgGtrMissSlowdown = cfg.Bind("Guitar",
                "miss_slowdown_percent",
                1f,
                "Percentage to lower song speed by with each missed note.");

            // variables innit bruv
            songSpeedCap        = System.Math.Abs(cfgSpeedCap.Value / 100);
            songSpeedCapEnabled = cfgSpeedCapEnabled.Value;
            gtrHitSpeedup       = System.Math.Abs(cfgGtrHitSpeedup.Value / 100);
            gtrMissSlowdown     = System.Math.Abs(cfgGtrMissSlowdown.Value / 100);

            if (!songSpeedCapEnabled || songSpeedCap > 50f)
                songSpeedCap = 50f;

            accelModeEnabled = cfgAccelModeToggle.Value;
            netSpeed = 1f;
            isPlayingSong = false;
            changedWM = false;

            Logger.LogInfo("AccelerationMode is loading...");
            Harmony.PatchAll();
            Logger.LogInfo("AccelerationMode is loaded.");
        }

        public void LateUpdate()
        {
            try
            {
                if (btnAccel == null)
                {
                    GameObject wmObj = GameObject.Find("Watermark Container");
                    GameObject topBar = GameObject.Find("Info Container");
                    TMPro.TextMeshProUGUI wmTmp = wmObj.GetComponentInChildren<TextMeshProUGUI>();
                    btnAccel = wmTmp.gameObject.AddComponent<UnityEngine.UI.Button>();
                    btnAccel.onClick.AddListener(ToggleAccelMode);
                }

                gmObj = GameObject.Find("Game Manager");
                if(gmObj != null)
                {
                    if (!isPlayingSong)
                    {
                        gameMgr = gmObj.GetComponent<YARG.Gameplay.GameManager>();
                        netSpeed = gameMgr.SelectedSongSpeed;
                        scoreTMP = GameObject.Find("Canvas/ScoreDisplay/Score Box/BGBox/Text").GetComponent<TextMeshProUGUI>();
                        if (accelModeEnabled)
                            scoreTMP.text = "<mspace=0.538em>" + netSpeed.ToString("0.00") + "×</mspace>  <size=18>Speed</size>";
                        currentTime = gmObj.GetComponent<YARG.Gameplay.GameManager>().RealSongTime;
                        playerOne = gameMgr.Players[0];
                        UpdateWatermark("<size=20><i>*Setting locked during play*</i></size><br>");
                        isPlayingSong = true;
                    }
                    
                    /*
                    // Disable highway to hide the jank
                    trackObj = GameObject.Find("Track Model");
                    if (trackObj.activeSelf)
                    {
                        trackObj.SetActive(false);
                    }

                    // THE FOLLOWING IS EXTRA DEBUG STUFF I WAS TINKERING WITH
                    
                    dbgTxt += "[Stats]<br>Notes hit P1:  " + playerOne.NotesHit.ToString() + "/" + playerOne.TotalNotes.ToString();
                    dbgTxt += "<br>";

                    // int indexPrev = YARG.Core.Chart.ChartEventExtensions.GetIndexOfPrevious(chart.GlobalEvents, currentTime);


                    // dbgTxt += "Song time:  " + currentTime.ToString() + "<br>";



                    // dbgTxt += "<br>[Global Chart events]";
                    // dbgTxt += "<br>First event: " + chart.GlobalEvents[0].Text.ToString();
                    // dbgTxt += "<br>";
                    // dbgTxt += "Last:  `" + chart.GlobalEvents.GetPrevious(currentTime).Text.ToString() + "`<br>";
                    // dbgTxt += "<br>Next:  " + chart.GlobalEvents[indexNext].Text.ToString();
                    // */
                }
                else
                {
                    if (isPlayingSong)
                    {
                        UpdateWatermark("<size=20>(click to toggle)</size><br>");
                        
                        isPlayingSong = false;
                    }
                }

                if (!changedWM)
                {
                    UpdateWatermark("<size=20>(click to toggle)</size><br>");
                    changedWM = true;
                }
            }
            catch
            {
                // UpdateWatermark("Play a chart to debug!");
            }
            // */

            if (playerOne.Stats.IsStarPowerActive && accelModeEnabled && netSpeed != 1f)
            {
                netSpeed = 1f;
                gameMgr.SetSongSpeed(netSpeed);
                scoreTMP.text = "<mspace=0.538em>" + netSpeed.ToString("0.00") + "×</mspace>  <size=18>Speed</size>";

                // old functionality i guess

                // playerOne.Stats.StarPowerAmount -= 0.499999; 
                // if (playerOne.Stats.StarPowerAmount < 0) { playerOne.Stats.StarPowerAmount = 0; }
                // playerOne.Stats.IsStarPowerActive = false;
            }

        }

    }
}
