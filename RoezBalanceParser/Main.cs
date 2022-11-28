using Cysharp.Threading.Tasks;
using HarmonyLib;
using RoezBalanceParser.Commands;
using RoezBalanceParser.UI;
using SDG.Framework.Modules;
using SDG.Unturned;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;

namespace RoezBalanceParser
{
    public class Main : MonoBehaviour, IModuleNexus
    {
        internal Harmony? m_Harmony;

        public delegate void OnCreated();
        public static Main Instance { get; private set; }
        public static event OnCreated? OnMenuUICreated;
        public static event OnCreated? OnGameUICreated;
        public static event OnCreated? OnCommandsCreated;

        public void initialize()
        {
            if (!PlayerLoopHelper.IsInjectedUniTaskPlayerLoop())
            {
                var unitySynchronizationContextField =
                    typeof(PlayerLoopHelper).GetField("unitySynchronizationContext",
                        BindingFlags.Static | BindingFlags.NonPublic);

                // For older version of UniTask
                unitySynchronizationContextField ??=
                    typeof(PlayerLoopHelper).GetField("unitySynchronizationContetext",
                        BindingFlags.Static | BindingFlags.NonPublic)
                    ?? throw new Exception("Could not find PlayerLoopHelper.unitySynchronizationContext field");

                unitySynchronizationContextField.SetValue(null, SynchronizationContext.Current);

                var mainThreadIdField =
                    typeof(PlayerLoopHelper).GetField("mainThreadId", BindingFlags.Static | BindingFlags.NonPublic)
                    ?? throw new Exception("Could not find PlayerLoopHelper.mainThreadId field");
                mainThreadIdField.SetValue(null, Thread.CurrentThread.ManagedThreadId);

                var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
                PlayerLoopHelper.Initialize(ref playerLoop);
            }

            m_Harmony = new("balance-parser-ui");
            m_Harmony.PatchAll();

            var go = new GameObject("Balance parser", typeof(Main));
            DontDestroyOnLoad(go);

            Instance = go.GetComponent<Main>();
        }

        public void Start()
        {
            new MenuWorkshopUIManager().Load();
            new PlayerDashboardCraftingUIManager().Load();
            new CommandsManager().Load();
        }

        public void shutdown()
        {

        }

        [HarmonyPatch]
        private static class Patches
        {
            [HarmonyPatch(typeof(MenuUI), "customStart")]
            [HarmonyPostfix]
            public static void CustomStart()
            {
                OnMenuUICreated?.Invoke();
            }

            [HarmonyPatch(typeof(PlayerUI), "InitializePlayer")]
            [HarmonyPostfix]
            public static void InitializePlayer()
            {
                OnGameUICreated?.Invoke();
            }

            [HarmonyPatch(typeof(Commander), "init")]
            [HarmonyPostfix]
            public static void Init()
            {
                OnCommandsCreated?.Invoke();
            }
        }
    }
}