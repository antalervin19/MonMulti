using MelonLoader;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Steamworks;

[assembly: MelonInfo(typeof(MonMulti.Core), "MonMulti", "1.0.0", "antalervin19", null)]
[assembly: MelonGame("Santa Goat", "Mon Bazou")]

namespace MonMulti
{
    public class Core : MelonMod
    {
        public static MelonLogger.Instance Logger;

        private const float TransformSyncInterval = 0.05f;
        private float transformSyncTimer;

        public override void OnInitializeMelon()
        {
            Logger = LoggerInstance;
            Logger.Msg("Initialized.");

            SceneManager.sceneLoaded += OnSceneLoaded;
            Steam.Initialize();
            Patches.Initialize();
        }

        public override void OnUpdate()
        {
            SteamAPI.RunCallbacks();

            if (Steam.GetState())
            {
                Networking.PacketReceiver.Update();
                Networking.VehicleSync.Update();
                Networking.ObjectSync.Update();

                transformSyncTimer += Time.deltaTime;

                if (transformSyncTimer >= TransformSyncInterval)
                {
                    transformSyncTimer = 0f;

                    GameObject localPlayer = Networking.PlayerManager.GetLocalPlayerObject();

                    if (localPlayer != null)
                    {
                        Transform cameraControls = Networking.PlayerManager.GetLocalPlayerCameraControls();

                        Quaternion bodyRotation;

                        if (cameraControls != null)
                        {
                            float yaw = cameraControls.rotation.eulerAngles.y;
                            bodyRotation = Quaternion.Euler(0f, yaw, 0f);
                        }
                        else
                        {
                            bodyRotation = Quaternion.identity;
                        }

                        Networking.PacketSender.BroadcastTransform(
                            localPlayer.transform.position,
                            bodyRotation
                        );
                    }
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.Msg($"Scene loaded: {scene.name} ({scene.buildIndex})");


            if (scene.name == "Master")
            {
                if (Steam.GetState())
                {
                    if (!Steam.IsHost())
                    {
                        Networking.NetworkManager.SendReady();
                    }
                    else
                    {
                        Networking.TimeSync.InitializeHost();
                    }

                    Networking.VehicleSync.Initialize();
                    Networking.ObjectSync.Initialize();
                }
            }


            if (scene.name == "MainMenu")
            {
                MelonCoroutines.Start(ModifyMenuDelayed());
            }
        }

        private IEnumerator ModifyMenuDelayed()
        {
            yield return new WaitForEndOfFrame();

            GameObject originalButton = GameObject.Find("Button_NewGame");
            if (originalButton == null)
                yield break;

            GameObject multiplayerButton = UnityEngine.Object.Instantiate(
                originalButton,
                originalButton.transform.parent
            );

            multiplayerButton.name = "multiplayerButton";
            multiplayerButton.transform.SetSiblingIndex(1);

            var localizeComponent = multiplayerButton.GetComponent(
                "UnityEngine.Localization.Components.LocalizeStringEvent"
            );
            if (localizeComponent != null)
            {
                UnityEngine.Object.Destroy(localizeComponent);
            }

            ButtonManager buttonManager = multiplayerButton.GetComponent<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.useCustomContent = true;
                buttonManager.SetText("Multiplayer");
                buttonManager.onClick = new UnityEngine.Events.UnityEvent();
                buttonManager.onClick.AddListener(() =>
                {
                    Steam.StartMultiplayerSession(false);
                });
                buttonManager.UpdateUI();
            }

            foreach (var tmp in multiplayerButton.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
            {
                tmp.text = "Multiplayer";
            }

            Transform normalTransform = multiplayerButton.transform.Find("Normal");
            if (normalTransform != null)
            {
                Transform iconParent = normalTransform.Find("Icon Parent");
                if (iconParent != null)
                    iconParent.gameObject.SetActive(false);
            }

            Transform highlightTransform = multiplayerButton.transform.Find("Highlighted");
            if (highlightTransform != null)
            {
                Transform iconParent = highlightTransform.Find("Icon Parent");
                if (iconParent != null)
                    iconParent.gameObject.SetActive(false);
            }

            foreach (var tmp in Resources.FindObjectsOfTypeAll<TMPro.TextMeshProUGUI>())
            {
                if (tmp.gameObject.name == "GameVersion")
                {
                    tmp.text = "MON BAZOU <color=#4FC3F7>MonMulti</color> - <size=60%><i>V1.06</i></size>";
                    tmp.SetAllDirty();
                    tmp.ForceMeshUpdate(true);
                    break;
                }
            }
        }
    }
}