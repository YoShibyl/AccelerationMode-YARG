using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Security.Permissions;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom;
using System.Diagnostics.Eventing.Reader;

namespace AccelerationMode
{
    
    [HarmonyPatch(typeName:"YARG.Gameplay.Player.FiveFretPlayer", methodName:"OnNoteMissed")]
    class FiveFretMissNoteHandler
    {
        [HarmonyPostfix]
        static void Postfix(ref object __instance)
        {
            try
            {
                AccelerationModePlugin.Instance.FiveFretMissNotePostfix();
            } catch { }
        }
    }
    
    [HarmonyPatch(typeName: "YARG.Gameplay.Player.FiveFretPlayer", methodName: "OnNoteHit")]
    class FiveFretHitNoteHandler
    {
        [HarmonyPostfix]
        static void Postfix(ref object __instance)
        {
            try
            {
                AccelerationModePlugin.Instance.FiveFretHitNotePostfix();
            } catch { }
        }
    }
    [HarmonyPatch(typeName: "YARG.Gameplay.Player.DrumsPlayer", methodName: "OnNoteHit")]
    class DrumsHitNoteHandler
    {
        [HarmonyPostfix]
        static void Postfix(ref object __instance)
        {
            try
            {
                AccelerationModePlugin.Instance.FiveFretHitNotePostfix(); // change to drums later
            }
            catch { }
        }
    }
    [HarmonyPatch(typeName: "YARG.Gameplay.Player.DrumsPlayer", methodName: "OnNoteMissed")]
    class DrumsMissNoteHandler
    {
        [HarmonyPostfix]
        static void Postfix(ref object __instance)
        {
            try
            {
                AccelerationModePlugin.Instance.FiveFretMissNotePostfix(); // change to drums later
            }
            catch { }
        }
    }

    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class AccelerationModePlugin : BaseUnityPlugin
    {
        public static AccelerationModePlugin Instance {  get; private set; }
        public static readonly Harmony harmony = new Harmony(MyGUID);
        private static bool didPatch = false;
        public static ManualLogSource Log = new ManualLogSource(PluginName);
        public AccelerationModePlugin()
        {
            Instance = this;
        }

        private const string MyGUID = "com.yoshibyl.AccelerationMode";
        private const string PluginName = "AccelerationMode";
        private const string VersionString = "0.3.0-pre1";

        public string LogMsg(object obj)
        {
            Logger.LogMessage(obj);
            // Console.WriteLine(obj.ToString());
            return obj.ToString();
        }

        public Assembly asm;
        public Assembly yargCoreAsm;

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
                }
                UpdateWatermark("<size=20>(click to toggle)</size><br>");
            }
        }

        public void UpdateSongSpeed(float speed)
        {
            if (gmt != null)
            {
                MethodInfo songSpeedMethod = gmt.GetMethod("SetSongSpeed");
                if (gameMgr != null && songSpeedMethod != null)
                {
                    object[] args = new object[] { speed };
                    songSpeedMethod.Invoke(gameMgr, args);
                    // LogMsg("Song speed set to: " + GetSongSpeed());
                }
            }
        }

        public float GetSongSpeed()
        {
            float ret = 0f;

            if (gmt != null && gameMgr != null)
            {
                var gm = gameMgr;

                ret = (float)gmt.GetProperty("ActualSongSpeed").GetValue(gm);
            }
            return ret;
        }

        internal void FiveFretMissNotePostfix()
        {
            if (accelModeEnabled)
            {
                float actualSpeed = GetSongSpeed();
                if (!IsSPActive(0))
                {
                    netSpeed -= gtrMissSlowdown;
                    if (netSpeed < 1f)
                        netSpeed = 1f;
                    if (actualSpeed != netSpeed)
                    {
                        UpdateSongSpeed(netSpeed);
                    }
                }
                // scoreTMP.text = "<mspace=0.538em>" + netSpeed.ToString("0.00") + "×</mspace>  <size=18>Speed</size>";
            }
        }

        internal void FiveFretHitNotePostfix()
        {
            if (accelModeEnabled)
            {
                float actualSpeed = GetSongSpeed();
                if (!IsSPActive(0) && actualSpeed < songSpeedCap)
                {
                    netSpeed += gtrHitSpeedup;
                    
                    if (netSpeed > songSpeedCap)
                        netSpeed = songSpeedCap;
                    if (actualSpeed < songSpeedCap)
                        UpdateSongSpeed(netSpeed);
                }
                // SetScore();  // Score handling is broken currently :(
                // scoreTMP.text = "<mspace=0.538em>" + netSpeed.ToString("0.00") + "×</mspace>  <size=18>Speed</size>";
            }
        }

        // Separate drums acceleration mode handling planned (maybe)
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

        public Type gmt;

        public Type basePlayerT;
        public Type gtrPlayerT;
        public Type drumPlayerT;
        public Type statsType;

        public GameObject gmObj;
        public TextMeshProUGUI scoreTMP;
        public object gameMgr;
        bool isPlayingSong;
        bool changedWM;
        public Button btnAccel;
        string yargVersionString;

        public void UpdateWatermark(string msg = "")
        {
            // GameObject topBar = GameObject.Find("Info Container");
            if (GameObject.Find("Watermark Container")!=null)
            {
                GameObject wmObj = GameObject.Find("Watermark Container");
                TMPro.TextMeshProUGUI wmTmp = wmObj.GetComponentInChildren<TextMeshProUGUI>();
                string accelStatusPrefix = "<color=#00CC00>ON";
                if (!accelModeEnabled)
                {
                    accelStatusPrefix = "<color=#FF0000>OFF";
                }

                if (!changedWM)
                {
                    yargVersionString = wmTmp.text;

                    if (btnAccel == null)
                    {
                        btnAccel = wmTmp.gameObject.AddComponent<UnityEngine.UI.Button>();
                        btnAccel.onClick.AddListener(ToggleAccelMode);
                        
                        changedWM = true;
                    }
                }

                wmTmp.text = "<i>*MODDED*</i> " + yargVersionString + "<br><size=20><b>Acceleration Mode v" + VersionString + ": " + accelStatusPrefix + "</color></b><br>" + msg;
                wmTmp.alignment = TextAlignmentOptions.TopRight;
            }
        }

        public IList GetPlayers(int index = -1)
        {
            GameObject go = gmObj;
            if (go != null)
            {
                PropertyInfo pi = gmt.GetProperty("Players");
                object man = go.GetComponent(gmt);
                IList playerz = pi.GetValue(man, null) as IList;
                
                if (playerz.Count > 0)
                {
                    IList ret = null;
                    if (index >= 0)
                    {
                        for (int i = 0; i < playerz.Count; i++)
                        {
                            if (i == index)
                            {
                                ret.Add(playerz[i]);
                            }
                        }
                    }
                    else
                    {
                        ret = playerz;
                    }
                    return ret;
                }
                else
                {
                    return null;
                }
            }
            else { return null; }
        }

        // Methods to get stats
        public bool IsSPActive(int index = 0)
        {
            IList playerz = GetPlayers();
            if (playerz.Count > index)
            {
                PropertyInfo statsInfo = playerz[index].GetType().GetProperty("Stats");
                    
                dynamic pStats = statsInfo.GetValue(playerz[index]);
                object pStatsObj = (object) pStats;
                
                FieldInfo spField = statsType.GetField("IsStarPowerActive");
                bool sp = (bool) statsType.GetField("IsStarPowerActive").GetValue(pStatsObj);
                
                return sp;
            }
            else
            {
                return false;
            }
        }

        // TO DO: Fix score reset
        public void SetScore(int score = 0, object player = null)
        {
            if (player == null)
                player = GetPlayers()[0];

            if (player != null)
            {
                if (player.GetType().Name.EndsWith("Player"))
                {
                    Type playerType = player.GetType();
                    if (playerType.Name.Contains("Player"))
                    {
                        var stats = playerType.GetProperty("Stats").GetValue(player);
                        object scoreObj = statsType.GetProperty("Score").GetValue(stats);
                        statsType.GetField("Score").SetValue(scoreObj, score);
                        playerType.GetProperty("Stats").SetValue(player, stats);

                    }
                }
            }
        }

        // Attempt at wrapping player stats
        /*
        public class PlayerStats
        {
            object GetPlayerStats()
            {
                return null;
            }

            public IList statsList
            {
                get
                {
                    IList list = null;
                    return list;
                }
                set
                {
                    
                }
            }
        } // */


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

        public void Start()
        {
            SceneManager.sceneLoaded += SceneLoaded;

            asm = Assembly.Load("Assembly-CSharp.dll");
            yargCoreAsm = Assembly.Load("YARG.Core.dll");
            gmt = asm.GetType("YARG.Gameplay.GameManager");
            basePlayerT = asm.GetType("YARG.Gameplay.Player.BasePlayer");
            gtrPlayerT  = asm.GetType("YARG.Gameplay.Player.FiveFretPlayer");
            drumPlayerT = asm.GetType("YARG.Gameplay.Player.DrumsPlayer");
            statsType =   yargCoreAsm.GetType("YARG.Core.Engine.BaseStats");

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
                300f,
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

            if (!songSpeedCapEnabled || songSpeedCap > 49.95f)
                songSpeedCap = 49.95f;
            if (songSpeedCap < 1f + gtrHitSpeedup) {
                songSpeedCap = 1f + gtrHitSpeedup;
            }

            accelModeEnabled = cfgAccelModeToggle.Value;
            netSpeed = 1f;
            isPlayingSong = false;
            changedWM = false;
            yargVersionString = "";
        }

        public void LateUpdate()
        {
            if (!didPatch)
            {
                LogMsg("AccelerationMode is loading...");
                harmony.PatchAll();
                LogMsg("AccelerationMode is loaded.");
                didPatch = true;
            }

            if (!changedWM)
            {
                try
                {
                    UpdateWatermark("<size=20>(click to toggle)</size><br>");
                }
                catch { }
            }
            try
            {
                gmObj = GameObject.Find("Game Manager");
                gameMgr = gmObj.GetComponent(gmt);
            }
            catch { }
            if (gmObj)
            {
                if (!isPlayingSong)
                {
                    netSpeed = 1f;
                    try
                    {
                        
                        if (GameObject.Find("/Canvas/ScoreDisplay/Score Box/BGBox/Text"))
                        {
                            scoreTMP = GameObject.Find("/Canvas/ScoreDisplay/Score Box/BGBox/Text").GetComponent<TextMeshProUGUI>();
                            if (accelModeEnabled)
                                scoreTMP.text = "<mspace=0.538em>" + netSpeed.ToString("0.00") + "×</mspace>  <size=18>Speed</size>";

                        }
                    } catch { }
                    isPlayingSong = true;
                    UpdateWatermark("<size=20><i>*Setting locked during play*</i></size><br>");
                }

                if (isPlayingSong)
                {
                    bool succ = false;

                    try
                    {
                        gameMgr = gmObj.GetComponent("YARG.Gameplay.GameManager");
                        IList p = GetPlayers();
                        if (p != null)
                        {
                            // Logger.LogInfo("Players: " + p.Count);
                            succ = true;
                        }
                        
                    }
                    catch { }

                    if (succ)
                    {
                        if (IsSPActive(0) && accelModeEnabled && netSpeed != 1f)
                        //if (accelModeEnabled)
                        {
                            netSpeed = 1f;
                            UpdateSongSpeed(netSpeed);
                            // scoreTMP.text = "<mspace=0.538em>" + netSpeed.ToString("0.00") + "×</mspace>  <size=18>Speed</size>";
                        }
                    }
                    // */
                }
            }
            else
            {
                if (isPlayingSong)
                {
                    UpdateWatermark("<size=20>(click to toggle)</size><br>");
                    isPlayingSong = false;
                }

            }

        }
        public void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            
        }
    }
}
