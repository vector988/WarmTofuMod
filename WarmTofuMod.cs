﻿using System;
using System.Reflection;
using UnityEngine;
using BepInEx;
using CodeStage.AntiCheat.Storage;
using CodeStage.AntiCheat.ObscuredTypes;

namespace WarmTofuMod
{
    [BepInPlugin("com.kert.warmtofumod", "WarmTofuMod", "1.0.0")]
    public class WarmTofuMod : BaseUnityPlugin
    {
        public enum Menus
        {
            MENU_NONE,
            MENU_TUNING,
            MENU_SUSPENSION
        }
        public static Menus currentMenu;
        public static float uiScaleX;
        public static float uiScaleY;
        public static GUIStyle sliderStyle;
        public static GUIStyle sliderStyleThumb;
        public static GUIStyle boxStyle;
        public static GUIStyle buttonStyle;
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            try
            {
                // hooks
                // camera view
                On.RCC_Camera.ChangeCamera += RCC_Camera_ChangeCamera;
                On.RCC_Camera.OnEnable += RCC_Camera_OnEnable;

                // transmission
                On.RCC_Customization.SetTransmission += RCC_Customization_SetTransmission;
                On.RCC_CarControllerV3.OnEnable += RCC_CarControllerV3_OnEnable;

                // additional suspension settings
                On.RCC_Customization.LoadStatsTemp += RCC_Customization_LoadStatsTemp;

                // mod GUI and logic
                On.RCC_PhotonManager.OnGUI += RCC_PhotonManager_OnGUI;
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to initialize");
                Logger.LogError(e);
                throw;
            }

            void RCC_Camera_ChangeCamera(On.RCC_Camera.orig_ChangeCamera orig, RCC_Camera self)
            {
                orig(self);
                PlayerPrefs.SetInt("CameraMode", (int)self.cameraMode);
            }

            void RCC_Camera_OnEnable(On.RCC_Camera.orig_OnEnable orig, RCC_Camera self)
            {
                orig(self);
                self.ChangeCamera((RCC_Camera.CameraMode)PlayerPrefs.GetInt("CameraMode", 0));
            }

            void RCC_Customization_SetTransmission(On.RCC_Customization.orig_SetTransmission orig, bool automatic)
            {
                orig(automatic);
                PlayerPrefs.SetInt("AutomaticTransmission", Convert.ToInt32(automatic));
            }

            void RCC_Customization_LoadStatsTemp(On.RCC_Customization.orig_LoadStatsTemp orig, RCC_CarControllerV3 vehicle)
            {
                orig(vehicle);
                LoadSuspensionSettings();
            }

            void RCC_CarControllerV3_OnEnable(On.RCC_CarControllerV3.orig_OnEnable orig, RCC_CarControllerV3 self)
            {
                orig(self);
                RCC_Settings.Instance.useAutomaticGear = Convert.ToBoolean(PlayerPrefs.GetInt("AutomaticTransmission", 1));
            }

            void RCC_PhotonManager_OnGUI(On.RCC_PhotonManager.orig_OnGUI orig, RCC_PhotonManager self)
            {
                orig(self);
                OnGUI();
            }

            void OnGUI()
            {
                if (ObscuredPrefs.GetBool("TOFU RUN", false))
                    ShowTofuTimer();

                ShowFooter();

                if (buttonStyle == null)
                    InitMenuStyles();

                // Additional suspension settings menus
                GameObject gameObject = GameObject.Find("Button_List"); // Original game tuning shop menu
                if (gameObject != null && gameObject.activeSelf)
                {
                    ShowModTuningMenu();
                }
                else if (currentMenu != Menus.MENU_NONE)
                {
                    RCC_CarControllerV3 activePlayerVehicle = RCC_SceneManager.Instance.activePlayerVehicle;
                    PlayerPrefs.SetFloat("SuspensionDamper", activePlayerVehicle.FrontLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring.damper);
                    PlayerPrefs.SetFloat("SuspensionSpring", activePlayerVehicle.FrontLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring.spring);
                    currentMenu = Menus.MENU_NONE;
                }
            }

            static void ShowModTuningMenu()
            {
                if (currentMenu == Menus.MENU_NONE)
                    currentMenu = Menus.MENU_TUNING;

                if (currentMenu != Menus.MENU_TUNING)
                {
                    if (currentMenu != Menus.MENU_SUSPENSION)
                    {
                        return;
                    }
                    SuspensionSettings();
                    SettingsBackButton();
                    return;
                }
                else if (GUI.Button(new Rect((float)((double)Screen.width / 1.73), (float)((double)Screen.height / 1.4), uiScaleX * 125f, uiScaleY * 22f), "WarmTofuMod Options", buttonStyle))
                {
                    currentMenu = Menus.MENU_SUSPENSION;
                    return;
                }
            }

            static void InitMenuStyles()
            {
                buttonStyle = new GUIStyle("Button");
                Vector2 vector = new Vector2(640f, 480f);
                buttonStyle.fontSize = (int)(12f * ((float)Screen.height / vector.y));
                buttonStyle.font = Resources.FindObjectsOfTypeAll<Font>()[4];
                uiScaleY = (float)Screen.height / vector.y;
                uiScaleX = (float)Screen.width / vector.x;
                sliderStyle = new GUIStyle("horizontalSlider");
                sliderStyle.fixedHeight = (float)((int)(12f * ((float)Screen.height / vector.y)));
                sliderStyleThumb = new GUIStyle("horizontalSliderThumb");
                sliderStyleThumb.fixedHeight = (sliderStyleThumb.fixedWidth = sliderStyle.fixedHeight);
                boxStyle = new GUIStyle("Box");
                boxStyle.font = buttonStyle.font;
                boxStyle.fontSize = buttonStyle.fontSize;
                boxStyle.normal.textColor = Color.white;
            }

            static void SettingsBackButton()
            {
                if (GUI.Button(new Rect((float)((double)Screen.width / 1.73), (float)((double)Screen.height / 1.4), uiScaleX * 125f, uiScaleY * 22f), "Back", buttonStyle))
                {
                    currentMenu = Menus.MENU_TUNING;
                    RCC_Customization.SaveStats(RCC_SceneManager.Instance.activePlayerVehicle);
                }
            }

            static void SuspensionSettings()
            {
                RCC_CarControllerV3 activePlayerVehicle = RCC_SceneManager.Instance.activePlayerVehicle;
                GUILayout.BeginArea(new Rect((float)((double)Screen.width / 1.73), (float)Screen.height / 3f, (float)Screen.width / 5f, (float)Screen.width / 4f));
                GUILayout.Box("Suspension spring force", boxStyle, Array.Empty<GUILayoutOption>());
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                float targetValue = GUILayout.HorizontalSlider(activePlayerVehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring, 10000f, 100000f, sliderStyle, sliderStyleThumb, Array.Empty<GUILayoutOption>());
                GUILayout.Box(targetValue.ToString(), boxStyle, new GUILayoutOption[]
                {
                    GUILayout.MaxWidth(uiScaleX * 40f)
                });
                GUILayout.EndHorizontal();
                GUILayout.Box("Suspension spring damper", boxStyle, Array.Empty<GUILayoutOption>());
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                float targetValue2 = GUILayout.HorizontalSlider(activePlayerVehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper, 1000f, 10000f, sliderStyle, sliderStyleThumb, Array.Empty<GUILayoutOption>());
                GUILayout.Box(targetValue2.ToString(), boxStyle, new GUILayoutOption[]
                {
                    GUILayout.MaxWidth(uiScaleX * 40f)
                });
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Reset", buttonStyle, Array.Empty<GUILayoutOption>()))
                {
                    RCC_Customization.SetFrontSuspensionsSpringForce(activePlayerVehicle, 40000f);
                    RCC_Customization.SetRearSuspensionsSpringForce(activePlayerVehicle, 40000f);
                    RCC_Customization.SetFrontSuspensionsSpringDamper(activePlayerVehicle, 1500f);
                    RCC_Customization.SetRearSuspensionsSpringDamper(activePlayerVehicle, 1500f);
                }
                else
                {
                    RCC_Customization.SetFrontSuspensionsSpringForce(activePlayerVehicle, targetValue);
                    RCC_Customization.SetRearSuspensionsSpringForce(activePlayerVehicle, targetValue);
                    RCC_Customization.SetFrontSuspensionsSpringDamper(activePlayerVehicle, targetValue2);
                    RCC_Customization.SetRearSuspensionsSpringDamper(activePlayerVehicle, targetValue2);
                }
                GUILayout.EndArea();
            }

            static void ShowFooter()
            {
                GUILayout.BeginArea(new Rect(5f, (float)Screen.height - 20f, 800f, 20f));
                GUILayout.Label($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} by Kert", Array.Empty<GUILayoutOption>());
                GUILayout.EndArea();
            }

            static void ShowDebugData()
            {
                RCC_CarControllerV3 activePlayerVehicle = RCC_SceneManager.Instance.activePlayerVehicle;
                string text = "";
                text = string.Concat(new object[]
                {
                    text,
                    activePlayerVehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.spring,
                    " ",
                    activePlayerVehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring.spring,
                    " ",
                    activePlayerVehicle.RearLeftWheelCollider.wheelCollider.suspensionSpring.damper,
                    " ",
                    activePlayerVehicle.FrontLeftWheelCollider.wheelCollider.suspensionSpring.damper
                });
                activePlayerVehicle.wheelTypeChoise = RCC_CarControllerV3.WheelType.AWD;
                activePlayerVehicle.TCS = false;
                activePlayerVehicle.ABS = false;
                RCC_Settings.Instance.useFixedWheelColliders = false;
                text = string.Concat(new object[]
                {
                    text,
                    "\n",
                    activePlayerVehicle.antiRollFrontHorizontal,
                    " ",
                    currentMenu.ToString()
                });
                GUILayout.Label(text, new GUILayoutOption[]
                {
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true)
                });
            }

            static void LoadSuspensionSettings()
            {
                RCC_CarControllerV3 activePlayerVehicle = RCC_SceneManager.Instance.activePlayerVehicle;
                float spring = PlayerPrefs.GetFloat("SuspensionSpring", 40000f);
                float damper = PlayerPrefs.GetFloat("SuspensionDamper", 1500f);
                RCC_Customization.SetFrontSuspensionsSpringForce(activePlayerVehicle, spring);
                RCC_Customization.SetRearSuspensionsSpringForce(activePlayerVehicle, spring);
                RCC_Customization.SetFrontSuspensionsSpringDamper(activePlayerVehicle, damper);
                RCC_Customization.SetRearSuspensionsSpringDamper(activePlayerVehicle, damper);
            }

            static void ShowTofuTimer()
            {
                int tofuTimer = (ObscuredInt)typeof(SRToffuManager).GetField("Compteur", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(GameObject.FindObjectOfType<SRToffuManager>());
                GUILayout.BeginArea(new Rect((float)(Screen.width / 2) - 120f, (float)Screen.height - 50f, 800f, 100f));
                GUIStyle guistyle = new GUIStyle();
                guistyle.font = buttonStyle.font;
                guistyle.fontSize = buttonStyle.fontSize;
                guistyle.normal.textColor = Color.white;
                GUILayout.Label("Tofu Time: " + tofuTimer.ToString() + " 's", guistyle, Array.Empty<GUILayoutOption>());
                GUILayout.EndArea();
            }
        }
    }
}