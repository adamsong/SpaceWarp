﻿// Attribution Notice To Lawrence/HatBat of https://github.com/Halbann/LazyOrbit
// This file is licensed under https://creativecommons.org/licenses/by-sa/4.0/

using System;
using System.Collections;
using HarmonyLib;
using KSP.Api;
using KSP.Api.CoreTypes;
using KSP.Sim.impl;
using KSP.UI.Binding;
using KSP.UI.Flight;
using SpaceWarp.API.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpaceWarp.API.Toolbar
{
    public static class ToolbarBackend
    {
        private static BaseModLogger _logger = new ModLogger("ToolbarBackend");
        public static GameObject AddButton(string buttonText, Sprite buttonIcon, string buttonId, Action<bool> function)
        {
            // Find the resource manager button and "others" group.

            // Say the magic words...
            GameObject list = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Popup Canvas/Container/ButtonBar/BTN-App-Tray/appbar-others-group");
            GameObject resourceManger = list?.GetChild("BTN-Resource-Manager");

            if (list == null || resourceManger == null)
            {
                _logger.Info("Couldn't find appbar.");
                return null;
            }

            // Clone the resource manager button.
            GameObject appButton = Object.Instantiate(resourceManger, list.transform);
            appButton.name = buttonId;

            // Change the text.
            TextMeshProUGUI text = appButton.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>();
            text.text = buttonText;

            // Change the icon.
            GameObject icon = appButton.GetChild("Content").GetChild("GRP-icon");
            Image image = icon.GetChild("ICO-asset").GetComponent<Image>();
            image.sprite = buttonIcon;

            // Add our function call to the toggle.
            ToggleExtended utoggle = appButton.GetComponent<ToggleExtended>();
            utoggle.onValueChanged.AddListener(state => function(state));

            // Set the initial state of the button.
            UIValue_WriteBool_Toggle toggle = appButton.GetComponent<UIValue_WriteBool_Toggle>();
            toggle.BindValue(new Property<bool>(false));

            // Bind the action to close the tray after pressing the button.
            IAction action = resourceManger.GetComponent<UIAction_Void_Toggle>().Action;
            appButton.GetComponent<UIAction_Void_Toggle>().BindAction(action);

            _logger.Info($"Added appbar button: {buttonId}");

            return appButton;
        }

        public static UnityEvent AppBarInFlightSubscriber = new UnityEvent();

        internal static void SubscriberSchedulePing()
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent<ToolbarBackendObject>();
            gameObject.SetActive(true);
        }
    }

    class ToolbarBackendObject : KerbalBehavior
    {
        public void Start()
        {
            
            StartCoroutine(awaiter());
        }

        private IEnumerator awaiter()
        {
            yield return new WaitForSeconds(1);
            ToolbarBackend.AppBarInFlightSubscriber.Invoke();
            Destroy(this);
        }
    }

    //TODO: Much better way of doing this
    [HarmonyPatch(typeof(UIFlightHud))]
    [HarmonyPatch("Start")]
    class ToolbarBackendAppBarPatcher
    {
        public static void Postfix(UIFlightHud __instance) => ToolbarBackend.SubscriberSchedulePing();
    }
}