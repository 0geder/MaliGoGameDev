using UnityEngine;
using UnityEngine.EventSystems;

namespace MaliGo.UI
{
    /// <summary>
    /// Ensures exactly one EventSystem exists, with the input module that actually matches
    /// this project's input backend. ProjectSettings has activeInputHandler=1 (Input System
    /// Package only) - legacy UnityEngine.Input calls throw at runtime, so the classic
    /// StandaloneInputModule (which polls legacy Input) cannot process clicks or touches at
    /// all. Every runtime-built canvas with a Button (dialogue, scenario choices, HUD,
    /// character creation, mobile controls) depends on this being right.
    /// </summary>
    public static class EventSystemUtility
    {
        public static void EnsureEventSystem(bool persistAcrossScenes = true)
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif

            if (persistAcrossScenes)
            {
                Object.DontDestroyOnLoad(go);
            }
        }
    }
}
