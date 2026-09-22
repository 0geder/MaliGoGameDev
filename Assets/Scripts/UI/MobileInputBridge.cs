namespace MaliGo.UI
{
    /// <summary>
    /// Lets the on-screen mobile interact button feed the same "was interact pressed this
    /// frame" question that MaliCompanionInteraction and ScenarioTrigger already ask about
    /// the keyboard E key - one shared flag instead of duplicating touch-handling in both.
    /// </summary>
    public static class MobileInputBridge
    {
        static bool interactRequested;

        public static void RequestInteract()
        {
            interactRequested = true;
        }

        /// <summary>Returns true at most once per request - consumes the flag.</summary>
        public static bool ConsumeInteractRequest()
        {
            if (!interactRequested)
            {
                return false;
            }

            interactRequested = false;
            return true;
        }
    }
}
