using BepInEx;
using BepInEx.Configuration;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using static UnityEngine.InputSystem.InputRemoting;
using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.IO;
using LlamAcademy.Spring;

namespace ControlTime
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.Minor)]
    internal class JotunnModStub : BaseUnityPlugin
    {
        public const string PluginGUID = "ujhik.ControlTime";
        public const string PluginName = "ControlTime";
        public const string PluginVersion = "1.0.0";

        private static bool debugLogging = false;
        private static ConfigEntry<long> configMinutesFullDayLength;
        private static ConfigEntry<bool> isRemoveRain;
        private static ConfigEntry<bool> isSkipNight;
        private static ConfigEntry<int> skipTimeStep;
        private static ConfigEntry<bool> isForceTimeOfDay;
        private static ConfigEntry<float> forcedTimeOfDayFraction;
        private Harmony _harmony;

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            CreateConfigValues();

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGUID);
        }

        private void CreateConfigValues() {
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };
            AcceptableValueRange<float> timeOfDayFraction = new AcceptableValueRange<float>(0, 1);

            configMinutesFullDayLength = Config.Bind("General",
                "minutesFullDayLength",
                60L,
                new ConfigDescription("How much minutes a day lasts (From the \"day n\" to the \"day n-1\", so includes daylight/night)", null, isAdminOnly));

            isRemoveRain = Config.Bind("General",
                "isRemoveRain",
                false,
                new ConfigDescription("Disable rain weather", null, isAdminOnly));

            isSkipNight = Config.Bind("General",
                "isRemoveNight",
                false,
                new ConfigDescription("Skip night", null, isAdminOnly));
            skipTimeStep = Config.Bind("General",
                "skipTimeStep",
                1,
                new ConfigDescription("Skip time step", null, isAdminOnly));
            isForceTimeOfDay = Config.Bind("General",
                "isForceTimeOfDay",
                false,
                new ConfigDescription("Force the status of sun/moon fixed in time", null, isAdminOnly));
            forcedTimeOfDayFraction = Config.Bind("General",
                "forcedTimeOfDayFraction",
                0.5f,
                new ConfigDescription("Number between 0 and 1 that represents the time of day. 0-> Night, 0.26 -> Sunrise, 0.5-> Midday, 0.9-> Midnight", timeOfDayFraction, isAdminOnly));
        }

        private void OnDestroy() {
            _harmony?.UnpatchSelf();
        }

        [HarmonyPatch(typeof(EnvMan), "Awake")]
        public static class EnvMan_Awake_Patch {
            private static void Postfix(ref long ___m_dayLengthSec, ref List<EnvSetup>  ___m_environments, ref EnvMan ___s_instance) {
                // How much seconds a day lasts (From the "day n" to the "day n-1", so includes daylight/night) 
                ___m_dayLengthSec = getFullDaySecondsLength();

                // Show available environments
                //foreach (EnvSetup environment in ___m_environments) {
                //    Jotunn.Logger.LogInfo("Name of enviromnent: " + environment.m_name);
                //}

                // Force environment
                //___s_instance.SetForceEnvironment("Rain");
            }
        }

        [HarmonyPatch(typeof(EnvMan), "FixedUpdate")]
        public static class EnvMan_Update_Patch {
            private static void Prefix(ref long ___m_dayLengthSec, ref EnvMan ___s_instance) {
                ___m_dayLengthSec = getFullDaySecondsLength();

                //___s_instance.m_totalSeconds = 100;

                //Logging
                if (Time.frameCount % 30 == 0 && debugLogging) {
                    //Jotunn.Logger.LogInfo("Current environment: " + ___s_instance.GetCurrentEnvironment().m_name);
                    Jotunn.Logger.LogInfo("isAdmin?: " + SynchronizationManager.Instance.PlayerIsAdmin);
                    Jotunn.Logger.LogInfo("isNight?: " + EnvMan.s_isNight);
                    Jotunn.Logger.LogInfo("Seconds full day: " + getFullDaySecondsLength());
                    Jotunn.Logger.LogInfo("isRemoveRain: " + isRemoveRain.Value);
                    Jotunn.Logger.LogInfo("isSkipNight: " + isSkipNight.Value);
                    Jotunn.Logger.LogInfo("smoothDayFraction: " + ___s_instance.m_smoothDayFraction);

                    Jotunn.Logger.LogInfo("ZNet time: " + ZNet.instance.m_netTime);

                    //configMinutesFullDayLength.Value = configMinutesFullDayLength.Value + 1;
                    
                }

                // Disable nights
                if (isSkipNight.Value && EnvMan.s_isNight && !isForceTimeOfDay.Value) {
                    Jotunn.Logger.LogInfo("isNight?: " + EnvMan.s_isNight);
                    double timeSeconds = ZNet.instance.GetTimeSeconds();
                    double time = timeSeconds - (double)((float)___s_instance.m_dayLengthSec * 0.15f);
                    int day = ___s_instance.GetDay(time);

                    double morningStartSec = ___s_instance.GetMorningStartSec(day + 1);
                    ___s_instance.m_skipTime = true;
                    ___s_instance.m_skipToTime = morningStartSec;
                    ___s_instance.m_timeSkipSpeed = skipTimeStep.Value;

                    ZLog.Log((object)("Time " + timeSeconds + ", day:" + day + "    nextm:" + morningStartSec + "  skipspeed:" + ___s_instance.m_timeSkipSpeed));

                    //___s_instance.SkipToMorning();
                }
                else {
                    ___s_instance.m_skipTime = false;
                    ___s_instance.m_timeSkipSpeed = 1;
                }

                // Disable rain
                if (isRemoveRain.Value && ___s_instance.IsEnvironment(new List<string> { "Rain", "LightRain", "ThunderStorm" })) {
                    //Jotunn.Logger.LogInfo("Its raining");
                    //___s_instance.SetForceEnvironment("Clear");
                    ___s_instance.QueueEnvironment("Twilight_Clear");
                    ___s_instance.QueueEnvironment("Clear");
                }

                // Force time of day
                if (isForceTimeOfDay.Value) {
                    ___s_instance.m_debugTimeOfDay = true;
                    ___s_instance.m_debugTime = forcedTimeOfDayFraction.Value;
                }
                else {
                    ___s_instance.m_debugTimeOfDay = false;
                }
            }
        }

        public static long getFullDaySecondsLength() {
            return configMinutesFullDayLength.Value * 60;
        }
    }

    /*
     * m_smoothDayFraction (from 0 to 1):
     * 0-> Night
     * 0.26 -> Sunrise
     * 0.5-> Midday
     * 
     * 0-9-> Midnight
     * 
     * 1
     */

    /*Names of enviroments:
     *  Clear
        Twilight_Clear
        Misty
        Darklands_dark
        Heath clear
        DeepForest Mist
        GDKing
        Rain
        LightRain
        ThunderStorm
        Eikthyr
        GoblinKing
        nofogts
        SwampRain
        Bonemass
        Snow
        Twilight_Snow
        Twilight_SnowStorm
        SnowStorm
        Moder
        Crypt
        Ghosts
        SunkenCrypt
     */
}