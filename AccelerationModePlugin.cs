using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
// using System.Linq;
// using System.Reflection;
// using System.Security.Principal;
using UnityEngine;
using UnityEngine.UI;
using YARG.Gameplay;
using TMPro;
using UnityEngine.SceneManagement;
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
        private const string VersionString = "0.1.1";

        public Font mainFont;

        public float netSpeed;
        public bool spActive;

        public bool accelModeEnabled;

        public void ToggleAccelMode()
        {
            if (!isPlayingSong)
            {
                if (accelModeEnabled)
                {
                    accelModeEnabled = false;
                }
                else
                {
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
                    gmObj = GameObject.Find("Game Manager");
                    gameMgr = gmObj.GetComponent<YARG.Gameplay.GameManager>();
                    netSpeed -= 0.01f;
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
                if (!playerOne.Stats.IsStarPowerActive && gameMgr.ActualSongSpeed < 2f)
                {
                    netSpeed += 0.0025f;
                    
                    if (netSpeed > 2f)
                        netSpeed = 2f;

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
        

        // bool toggleWasMade;
        Button btnAccel;

        // Text btnTxt;

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

        public void Awake()
        {
            // toggleWasMade = false;
            accelModeEnabled = false;
            netSpeed = 1f;
            isPlayingSong = false;
            changedWM = false;

            Logger.LogInfo("YARGmod is loading...");
            Harmony.PatchAll();
            Logger.LogInfo("YARGmod is loaded.");
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
                
                // old functionality i guess

                // playerOne.Stats.StarPowerAmount -= 0.499999; 
                // if (playerOne.Stats.StarPowerAmount < 0) { playerOne.Stats.StarPowerAmount = 0; }
                // playerOne.Stats.IsStarPowerActive = false;
            }

        }

    }
}
