﻿using BepInEx;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.Storage;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using SickscoreGames.HUDNavigationSystem;

namespace WarmTofuMod
{
    public partial class WarmTofuMod : BaseUnityPlugin
    {
        public partial class CustomRaceManager : MonoBehaviour
        {
            private CustomRaceManager instance = null;
            private static RCC_CarControllerV3 rival = null;
            public struct RaceSettings
            {
               public string direction;
                public string order;
                public bool nitro;
                public bool collision;
                public RaceSettings(string direction = "Downhill", string order = "Parallel", bool nitro = false, bool collision = true)
                {
                    this.direction = direction;
                    this.order = order;
                    this.nitro = nitro;
                    this.collision = collision;
                }
            }
            public static RaceSettings raceSettings;
            public static bool customRaceStarted = false;
            public static bool customRaceInvite = false;
            public static bool isMyInvitation = true;
            CustomRaceManager()
            {
                if (instance)
                    Debug.Log("Trying to create CustomRaceManager when it already exists");
                instance = this;
                new CustomRaceMenu(this.gameObject);
                new CustomRacePlayerBattleButtons(GameObject.FindObjectOfType<SRPlayerListRoom>().PlayerListUI);
            }

            void OnDestroy()
            {
                instance = null;
            }

            public static void SetRival(RCC_CarControllerV3 carController)
            {
                rival = carController;
            }

            public static string GetRivalPlayerName()
            {
                return rival.GetComponentInParent<SRPlayerCollider>().gameObject.transform.GetComponentInChildren<TextMeshPro>()?.text;
            }

            public static string GetRivalPhotonName()
            {
                return rival.name;
            }

            private static string GetPlayerPhotonName()
            {
                return RCC_SceneManager.Instance.activePlayerVehicle.gameObject.GetComponent<SRPlayerCollider>().name;
            }

            public static void SendBattleInvitation()
            {
                customRaceInvite = true;
                isMyInvitation = true;
                RaceManager rm = GameObject.FindObjectOfType<RaceManager>();

                typeof(RaceManager).GetField("CibleName", bindingFlags).SetValue(rm, (ObscuredString)GetPlayerPhotonName());
                typeof(RaceManager).GetField("CiblePhotonName", bindingFlags).SetValue(rm, (ObscuredString)GetRivalPhotonName());
                typeof(RaceManager).GetField("TuPeuxAsk", bindingFlags).SetValue(rm, (ObscuredInt)1);

                rm.UIAskToPlayer.SetActive(true);
                rm.UIAskToPlayer.GetComponent<Animator>().Play("UATPappear");
                rm.CocheInvitationSent.SetActive(true);
                rm.SendRaceInvitation();
                rm.UIAskToPlayer.GetComponent<Text>().text = rm.InvitationSentTo + GetRivalPlayerName();
            }

            public static void SendBattleInvitationRPC()
            {
                SRPlayerCollider player = RCC_SceneManager.Instance.activePlayerVehicle.gameObject.GetComponent<SRPlayerCollider>();
                typeof(SRPlayerCollider).GetField("NameTarget", bindingFlags).SetValue(player, GetRivalPhotonName());
                typeof(SRPlayerCollider).GetField("EnvoyeurDelaDemande", bindingFlags).SetValue(player, GetPlayerPhotonName());
                PhotonView view = (PhotonView)typeof(SRPlayerCollider).GetField("view", bindingFlags).GetValue(player);

                string order = raceSettings.order;
                if (order == "You lead")
                    order = "You chase";
                else if (order == "You chase")
                    order = "You lead";

                view.RPC("ReceiveBattleInvitationRPC", RpcTarget.Others, new object[]
                {
                    GetRivalPhotonName(),
                    PlayerPrefs.GetString("PLAYERNAMEE"),
                    GetPlayerPhotonName(),
                    raceSettings.direction,
                    order,
                    raceSettings.nitro,
                    raceSettings.collision
                });
            }

            public static void RaceEnd()
            {
                customRaceStarted = false;
            }

            public static GameObject GetCustomRaceMenu()
            {
                return CustomRaceMenu.GetMenu();
            }

            public static void OpenCustomRaceMenu()
            {
                CustomRaceMenu.OpenMenu();
            }

            public static void CloseCustomRaceMenu()
            {
                CustomRaceMenu.CloseMenu();
            }
        }

        void SRPlayerCollider_SendRaceInvitation(On.SRPlayerCollider.orig_SendRaceInvitation orig, SRPlayerCollider self, string rivalPhotonName, string playerName)
        {
            if (CustomRaceManager.customRaceInvite)
                CustomRaceManager.SendBattleInvitationRPC();
            else
                orig(self, rivalPhotonName, playerName);
        }

        void RaceManager_ShowMyInvitation(On.RaceManager.orig_ShowMyInvitation orig, RaceManager self, string Sender, string EnvoyeurDelaDemande)
        {
            orig(self, Sender, EnvoyeurDelaDemande);
            if (CustomRaceManager.customRaceInvite)
            {
                CustomRaceManager.RaceSettings raceSettings = CustomRaceManager.raceSettings;
                self.RaceNotification.GetComponentInChildren<Text>().text = "<color=#B95353>" + Sender + "</color> \n" + self.ChallengeYou + "\n" +
                raceSettings.direction + ". " + raceSettings.order + "\n" + "Nitro: " + (raceSettings.nitro ? "Yes" : "No") + " Collision: " + (raceSettings.collision ? "Yes" : "No");
            }
        }

        void RaceManager_LetsGoRaceP1(On.RaceManager.orig_LetsGoRaceP1 orig, RaceManager self)
        {
            ChangeRacePositionData();
            orig(self);
        }

        void RaceManager_LetsGoRaceP2(On.RaceManager.orig_LetsGoRaceP2 orig, RaceManager self)
        {
            ChangeRacePositionData();
            orig(self);
        }

        void RaceManager_OtherLeaveRun(On.RaceManager.orig_OtherLeaveRun orig, RaceManager self)
        {
            orig(self);
            CustomRaceManager.RaceEnd();
        }

        void RaceManager_StopRun(On.RaceManager.orig_StopRun orig, RaceManager self)
        {
            orig(self);
            CustomRaceManager.RaceEnd();
        }

        void RaceManager_FinishFirst(On.RaceManager.orig_FinishFirst orig, RaceManager self)
        {
            orig(self);
            CustomRaceManager.RaceEnd();
        }

        void RaceManager_FinishSecond(On.RaceManager.orig_FinishSecond orig, RaceManager self)
        {
            orig(self);
            CustomRaceManager.RaceEnd();
        }

        public void ChangeRacePositionData()
        {
            RaceManager rm = GameObject.FindObjectOfType<RaceManager>();
            if (!CustomRaceManager.customRaceInvite)
            {
                var defaultData = defaultRaceData[SceneManager.GetActiveScene().name];
                rm.StartingPointP1.position = defaultData.posP1;
                rm.StartingPointP1.rotation = defaultData.rotP1;
                rm.StartingPointP2.position = defaultData.posP2;
                rm.StartingPointP2.rotation = defaultData.rotP2;
                GameObject.FindObjectOfType<FinishZone>().transform.SetPositionAndRotation(defaultData.posFinish, defaultData.rotFinish);
                return;
            }

            CustomRaceManager.RaceSettings battleSettings = CustomRaceManager.raceSettings;
            var mapData = customRaceData[SceneManager.GetActiveScene().name][battleSettings.direction];
            switch (battleSettings.order)
            {
                case "Parallel":
                    rm.StartingPointP1.position = mapData.posP1;
                    rm.StartingPointP1.rotation = mapData.rotP1;
                    rm.StartingPointP2.position = mapData.posP2;
                    rm.StartingPointP2.rotation = mapData.rotP2;
                    break;
                case "You lead":
                    rm.StartingPointP1.position = mapData.pos_lead;
                    rm.StartingPointP1.rotation = mapData.rot_lead;
                    rm.StartingPointP2.position = mapData.pos_lead + rm.StartingPointP1.TransformDirection(Vector3.back * 10f);
                    rm.StartingPointP2.rotation = rm.StartingPointP1.rotation;
                    break;
                case "You chase":
                    rm.StartingPointP2.position = mapData.pos_lead;
                    rm.StartingPointP2.rotation = mapData.rot_lead;
                    rm.StartingPointP1.position = mapData.pos_lead + rm.StartingPointP2.TransformDirection(Vector3.back * 10f);
                    rm.StartingPointP1.rotation = rm.StartingPointP2.rotation;
                    break;
            }

            if (!CustomRaceManager.isMyInvitation && battleSettings.order != "Parallel")
            {
                var tempP = rm.StartingPointP1.position;
                var tempR = rm.StartingPointP1.rotation;
                rm.StartingPointP1.position = rm.StartingPointP2.position;
                rm.StartingPointP1.rotation = rm.StartingPointP2.rotation;
                rm.StartingPointP2.position = tempP;
                rm.StartingPointP2.rotation = tempR;
            }

            FinishZone fz = GameObject.FindObjectOfType<FinishZone>();
            fz.transform.SetPositionAndRotation(mapData.posFinish, mapData.rotFinish);
            if (SceneManager.GetActiveScene().name == "Akagi")
            {
                fz.gameObject.transform.GetChild(1).rotation = GameObject.FindObjectOfType<SR_Minimap_Manager_RaceMap>().LaMegacam.transform.rotation;
                fz.gameObject.transform.GetChild(1).localPosition = new Vector3(0f, 0f, 0f);
            }
            else
            {
                fz.gameObject.transform.GetChild(0).transform.GetChild(0).rotation = GameObject.FindObjectOfType<SR_Minimap_Manager_RaceMap>().LaMegacam.transform.rotation;
                fz.gameObject.transform.GetChild(0).transform.GetChild(0).localPosition = new Vector3(0f, 0f, 0f);
            }
        }

        class CustomRacePlayerBattleButtons
        {
            public static List<GameObject> playerListItems = new();
            public static bool isButtonSelected = false;
            public CustomRacePlayerBattleButtons(GameObject parent)
            {
                playerListItems.Clear();
                Text[] texts = parent.GetComponentsInChildren<Text>();
                foreach (Text text in texts)
                {
                    if (text.gameObject.name == "P1 (0)")
                    {
                        Button playerButton = Instantiate(GameObject.FindObjectOfType<SRUIManager>().BtnLobby.GetComponent<Button>());
                        playerButton.GetComponentInChildren<Text>().text = "Battle";

                        BattleButtonExtension bh = playerButton.gameObject.AddComponent<BattleButtonExtension>();
                        playerButton.transform.SetParent(text.gameObject.transform);
                        RectTransform r = playerButton.GetComponent<RectTransform>();
                        r.anchoredPosition = new Vector3(120, 0, 0);
                        r.anchorMin = r.anchorMax = new Vector2(0, 0.5f);
                        r.sizeDelta = new Vector2(260f, 27f);
                        r.localScale = new Vector3(1, 1, 1);
                        Text t = playerButton.GetComponentInChildren<Text>();
                        t.fontSize = t.resizeTextMaxSize = 20;
                        t.resizeTextMinSize = 1;
                        playerButton.gameObject.SetActive(false);
                        ColorBlock colors = playerButton.colors;
                        colors.selectedColor = Color.green;
                        playerButton.colors = colors;
                        playerListItems.Add(playerButton.gameObject);
                        playerButton.onClick = new Button.ButtonClickedEvent();
                        playerButton.onClick.AddListener(() => OpenCustomRaceMenu(bh.rival));
                    }
                }
            }

            public static void Update()
            {
                string playerPhotonName = RCC_SceneManager.Instance.activePlayerVehicle.gameObject.GetComponent<SRPlayerCollider>().name;
                Button previousButton = null;
                int raceInProgress = PlayerPrefs.GetInt("ImInRun");
                foreach (GameObject playerItem in playerListItems)
                {
                    if (raceInProgress == 1)
                    {
                        playerItem.SetActive(false);
                        continue;
                    }
                    // playerItem.SetActive(true);
                    // BattleButtonExtension asd = playerItem.gameObject.GetComponent<BattleButtonExtension>();
                    // asd.rival = RCC_SceneManager.Instance.activePlayerVehicle.gameObject.GetComponent<RCC_CarControllerV3>();
                    // continue;

                    SRCheckOtherPlayerCam obj = playerItem.transform.GetParent().transform.GetParent().GetComponentInChildren<SRCheckOtherPlayerCam>();
                    RCC_CarControllerV3 photonCar = (RCC_CarControllerV3)typeof(SRCheckOtherPlayerCam).GetField("jack", bindingFlags).GetValue(obj);
                    int state = (int)typeof(SRCheckOtherPlayerCam).GetField("State", bindingFlags).GetValue(obj);
                    if (state == 0 || !photonCar)
                    {
                        playerItem.SetActive(false);
                        continue;
                    }
                    string photonName = photonCar.gameObject.name;
                    if (photonName == playerPhotonName)
                    {
                        playerItem.SetActive(false);
                        continue;
                    }

                    // disable everyone who is not warmtofu
                    Text buttonText = playerItem.gameObject.GetComponentInChildren<Text>();
                    if (!WarmTofuNetwork.PlayerHasMod(photonName))
                    {
                        playerItem.SetActive(false);
                        continue;
                    }

                    BattleButtonExtension bh = playerItem.gameObject.GetComponent<BattleButtonExtension>();
                    buttonText.text = bh.playerName = playerItem.gameObject.transform.GetParent().GetComponent<Text>().text;
                    if (bh.hovered)
                        buttonText.text = "Battle\n" + buttonText.text;
                    bh.rival = photonCar;
                    playerItem.SetActive(true);
                    Button currentButton = playerItem.GetComponentInChildren<Button>();
                    Navigation newNav = new Navigation();
                    newNav.mode = Navigation.Mode.Explicit;
                    if (previousButton != null)
                    {
                        newNav.selectOnDown = currentButton;
                        newNav.selectOnUp = previousButton.navigation.selectOnUp;
                        previousButton.navigation = newNav;

                        newNav = new Navigation();
                        newNav.mode = Navigation.Mode.Explicit;
                        newNav.selectOnUp = previousButton;
                        currentButton.navigation = newNav;
                    }
                    if (!isButtonSelected)
                    {
                        currentButton.interactable = false;
                        currentButton.interactable = true;
                        currentButton.Select();
                        isButtonSelected = true;
                    }
                    previousButton = currentButton;
                }
            }

            private class BattleButtonExtension : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
            {
                public string playerName;
                private Text hoverText;
                public bool hovered;
                RectTransform rect;
                public RCC_CarControllerV3 rival = null;
                void Start()
                {
                    rect = gameObject.GetComponent<RectTransform>();
                    hoverText = GetComponentInChildren<Text>();
                    playerName = gameObject.transform.GetParent().GetComponent<Text>().text;
                    hoverText.text = playerName;
                    hovered = false;
                }

                public void OnPointerEnter(PointerEventData eventData)
                {
                    hoverText.text = "Battle\n" + playerName;
                    rect.anchorMax = new Vector2(rect.anchorMax.x, 1.1f);
                    hovered = true;
                }

                public void OnPointerExit(PointerEventData eventData)
                {
                    hoverText.text = playerName;
                    rect.anchorMax = new Vector2(rect.anchorMax.x, 0.5f);
                    hovered = false;
                }
            }
        }

        void SRPlayerListRoom_Start(On.SRPlayerListRoom.orig_Start orig, SRPlayerListRoom self)
        {
            orig(self);
            self.gameObject.AddComponent<CustomRaceManager>();
        }

        static void OpenCustomRaceMenu(RCC_CarControllerV3 carControllerV3)
        {
            Debug.Log("rival is " + carControllerV3);
            CustomRaceManager.SetRival(carControllerV3);

            Debug.Log("Opened battle options against " + CustomRaceManager.GetRivalPhotonName() + " " + CustomRaceManager.GetRivalPlayerName());
            CustomRaceManager.OpenCustomRaceMenu();
            CustomRaceManager.GetCustomRaceMenu().GetComponentInChildren<Text>().text = "Race against " + CustomRaceManager.GetRivalPlayerName();
        }

        void SRPlayerListRoom_Update(On.SRPlayerListRoom.orig_Update orig, SRPlayerListRoom self)
        {
            if (Input.GetKeyDown(KeyCode.Tab) ||
            (Input.GetKeyDown(KeyCode.Joystick1Button8) && ObscuredPrefs.GetBool("TooglePlayerListXbox", false) && PlayerPrefs.GetString("ControllerTypeChoose") == "Xbox360One") ||
            (Input.GetButtonDown("PS4_ClickLeft") && ObscuredPrefs.GetBool("TooglePlayerListXbox", false) && PlayerPrefs.GetString("ControllerTypeChoose") == "PS4"))
            {
                self.PlayerListUI.SetActive(!self.PlayerListUI.activeSelf);
                CustomRaceManager.CloseCustomRaceMenu();
                if (self.PlayerListUI.activeSelf)
                {
                    self.SteamIcon();
                    self.PlayerListing();
                    self.StartCoroutine(PlayerListUpdate(self));
                }
                else
                    CustomRacePlayerBattleButtons.isButtonSelected = false;
            }
        }

        void SRPlayerListRoom_PlayerListing(On.SRPlayerListRoom.orig_PlayerListing orig, SRPlayerListRoom self)
        {
            orig(self);
            WarmTofuNetwork.myInstance.SendModInfo();
        }

        void SRPlayerListRoom_PlayerListingRefresh(On.SRPlayerListRoom.orig_PlayerListingRefresh orig, SRPlayerListRoom self)
        {
            CustomRacePlayerBattleButtons.Update();
            orig(self);
        }

        IEnumerator PlayerListUpdate(SRPlayerListRoom sRPlayerListRoom)
        {
            while (true)
            {
                sRPlayerListRoom.PlayerListingRefresh();
                yield return new WaitForSeconds(1);
                if (!sRPlayerListRoom.PlayerListUI.active)
                    break;
            }
            yield break;
        }

        IEnumerator RaceManager_DecompteRunningIE(On.RaceManager.orig_DecompteRunningIE orig, RaceManager self)
        {
            typeof(RaceManager).GetField("mononcleone", bindingFlags).SetValue(self, (ObscuredInt)0);
            self.MenuUI.SetActive(false);
            TextMeshPro text3d = self.TxtCompteur;

            Text text2d = self.StartingCount.GetComponentInChildren<Text>();
            GameObject playerVehicle = RCC_SceneManager.Instance.activePlayerVehicle.gameObject;
            RCC_CarControllerV3 rccCar = playerVehicle.GetComponent<RCC_CarControllerV3>();
            Rigidbody rigidBody = playerVehicle.GetComponent<Rigidbody>();
            AudioSource audio = self.GetComponent<AudioSource>();
            GameObject enemyCar = (GameObject)typeof(RaceManager).GetField("Enemyy", bindingFlags).GetValue(self);
            // don't show distance indicator with an arrow
            enemyCar.GetComponent<HUDNavigationElement>().showIndicator = false;

            text2d.text = "5";
            rccCar.speed = 0f;
            typeof(RCC_CarControllerV3).GetField("engineRPM", bindingFlags).SetValue(rccCar, (float)0);
            audio.PlayOneShot(self.CountClick);
            rigidBody.drag = 1000f;
            yield return new WaitForSeconds(1f);
            text2d.text = "4";
            audio.PlayOneShot(self.CountClick);
            yield return new WaitForSeconds(1f);
            text2d.text = "3";
            audio.PlayOneShot(self.CountClick);
            rccCar.speed = 0f;
            typeof(RCC_CarControllerV3).GetField("engineRPM", bindingFlags).SetValue(rccCar, (float)0);
            yield return new WaitForSeconds(1f);
            text2d.text = "2";
            audio.PlayOneShot(self.CountClick);
            rccCar.speed = 0f;
            typeof(RCC_CarControllerV3).GetField("engineRPM", bindingFlags).SetValue(rccCar, (float)0);
            rccCar.currentGear = 0;
            yield return new WaitForSeconds(1f);
            self.MenuUI.SetActive(false);
            text2d.text = "1";
            audio.PlayOneShot(self.CountClick);
            rigidBody.drag = 1000f;
            yield return new WaitForSeconds(1f);
            rccCar.enabled = true;
            rccCar.currentGear = 0;
            text2d.text = "GO";
            rigidBody.drag = 0.01f;
            if (CustomRaceManager.customRaceInvite)
            {
                CustomRaceManager.customRaceInvite = false;
                CustomRaceManager.customRaceStarted = true;
                if (CustomRaceManager.raceSettings.collision == true)
                    playerVehicle.GetComponentInParent<SRPlayerCollider>().AppelRPCSetGhostModeV2(8);
            }
            else
                playerVehicle.GetComponentInParent<SRPlayerCollider>().AppelRPCSetGhostModeV2(10);
            self.RunningLogo.SetActive(true);
            yield return new WaitForSeconds(1f);
            self.StartingCount.SetActive(false);
            yield break;
        }

        void SRNosManager_Update(On.SRNosManager.orig_Update orig, SRNosManager self)
        {
            if (CustomRaceManager.customRaceStarted && !CustomRaceManager.raceSettings.nitro)
                return;
            else
                orig(self);
        }

        void RCC_CarControllerV3_Inputs(On.RCC_CarControllerV3.orig_Inputs orig, RCC_CarControllerV3 self)
        {
            orig(self);
            if (CustomRaceManager.customRaceStarted && !CustomRaceManager.raceSettings.nitro)
                self.boostInput = 0f;
        }

        void RaceManager_Start(On.RaceManager.orig_Start orig, RaceManager self)
        {
            orig(self);
            // Fixed E button not working for race invitations on maps apart from Irohazaka and Haruna
            self.FightTouch = KeyCode.E;
        }
    }
}