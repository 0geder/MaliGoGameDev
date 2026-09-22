using UnityEngine;

namespace MaliGo.Scenarios
{
    /// <summary>
    /// Places scenario triggers into MaliGoWorld at runtime, the same way
    /// MaliGoIdentityRuntimeBootstrap patches in identity systems. Keeps the existing
    /// scene file untouched - no manual Editor step required to add a scenario location.
    /// </summary>
    public static class ScenarioWorldWiring
    {
        const string CommercialHubAnchor = "Local_Commercial_Hub";
        const string PlayerHouseAnchor = "Player_House";
        const string DrivewayAnchor = "Road_Player_Driveway";

        public static void EnsureScenarioManager(GameObject systemsRoot)
        {
            if (Object.FindFirstObjectByType<ScenarioManager>() == null)
            {
                systemsRoot.AddComponent<ScenarioManager>();
            }
        }

        public static void EnsureFoodDecisionTrigger()
        {
            EnsureTrigger("ScenarioTrigger_FoodDecision", CommercialHubAnchor,
                new Vector3(0.41f, 0f, 0.41f), new Vector3(0.41f, 0.05f, 0.41f),
                ScenarioLibrary.FoodDecisionId, "Press E for today's food decision");
        }

        /// <summary>Places the rest of the P1 scenario triggers. Positions are a first pass -
        /// nudge in the Editor if anything clips a fence/planter.</summary>
        public static void EnsureAllScenarioTriggers()
        {
            EnsureFoodDecisionTrigger();

            EnsureTrigger("ScenarioTrigger_Transport", DrivewayAnchor,
                new Vector3(0.3f, 0f, 0.3f), new Vector3(2.3f, 0.05f, 0.3f),
                ScenarioLibrary.TransportDecisionId, "Press E to decide how to get around");

            EnsureTrigger("ScenarioTrigger_Impulse", CommercialHubAnchor,
                new Vector3(0f, 0f, -0.6f), new Vector3(-2.0f, 0.05f, 3.8f),
                ScenarioLibrary.ImpulsePurchaseId, "Press E - something in the window caught your eye");

            EnsureTrigger("ScenarioTrigger_Emergency", PlayerHouseAnchor,
                new Vector3(0.5f, 0f, 0.3f), new Vector3(2.5f, 0.05f, -1.9f),
                ScenarioLibrary.EmergencyExpenseId, "Press E - something needs attention at home");

            EnsureTrigger("ScenarioTrigger_Windfall", PlayerHouseAnchor,
                new Vector3(-0.5f, 0f, 0.3f), new Vector3(1.5f, 0.05f, -1.9f),
                ScenarioLibrary.WindfallId, "Press E - check today's mail");

            EnsureTrigger("ScenarioTrigger_FamilyObligation", PlayerHouseAnchor,
                new Vector3(0f, 0f, 0.6f), new Vector3(2.0f, 0.05f, -1.6f),
                ScenarioLibrary.FamilyObligationId, "Press E - a call from home");

            EnsureTrigger("ScenarioTrigger_Stokvel", CommercialHubAnchor,
                new Vector3(0.5f, 0f, -0.3f), new Vector3(-1.5f, 0.05f, 4.1f),
                ScenarioLibrary.StokvelDecisionId, "Press E to hear about the stokvel");

            EnsureTrigger("ScenarioTrigger_CreditBnpl", CommercialHubAnchor,
                new Vector3(-0.5f, 0f, -0.3f), new Vector3(-2.5f, 0.05f, 4.1f),
                ScenarioLibrary.CreditBnplId, "Press E to check out the easy-payment offer");
        }

        static void EnsureTrigger(string triggerObjectName, string anchorName, Vector3 offsetFromAnchor, Vector3 fallbackPosition, string scenarioId, string promptText)
        {
            if (GameObject.Find(triggerObjectName) != null)
            {
                return;
            }

            GameObject anchor = GameObject.Find(anchorName);
            Vector3 position = anchor != null
                ? anchor.transform.position + offsetFromAnchor
                : fallbackPosition;

            var triggerObject = new GameObject(triggerObjectName);
            triggerObject.transform.position = position;

            var trigger = triggerObject.AddComponent<ScenarioTrigger>();
            trigger.Configure(scenarioId, promptText);
        }
    }
}
