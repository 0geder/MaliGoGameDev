using UnityEngine;

namespace MaliGo.World
{
    /// <summary>
    /// Places Home/Bank/Work into the already-built MaliGoWorld scene at runtime, the same
    /// self-healing pattern as ScenarioWorldWiring - no manual Editor step required.
    /// </summary>
    public static class WorldLocationWiring
    {
        const string PlayerHouseAnchor = "Player_House";
        const string CommercialHubAnchor = "Local_Commercial_Hub";
        const string BankBuildingAnchor = "Local_Bank_Building";
        const string EastRoadAnchor = "Road_Main_4";

        public static void EnsureLocations()
        {
            EnsureHome();
            EnsureBank();
            EnsureWork();
        }

        static void EnsureHome()
        {
            if (GameObject.Find("Location_Home") != null)
            {
                return;
            }

            GameObject anchor = GameObject.Find(PlayerHouseAnchor);
            Vector3 position = anchor != null
                ? anchor.transform.position + new Vector3(0f, 0f, 1.0f)
                : new Vector3(2.0f, 0.05f, -1.2f);

            var obj = new GameObject("Location_Home");
            obj.transform.position = position;
            obj.AddComponent<HomeInteraction>();
        }

        static void EnsureBank()
        {
            if (GameObject.Find("Location_Bank") != null)
            {
                return;
            }

            GameObject anchor = GameObject.Find(BankBuildingAnchor) ?? GameObject.Find(CommercialHubAnchor);
            Vector3 position = anchor != null
                ? anchor.transform.position + new Vector3(0.3f, 0f, 0.3f)
                : new Vector3(-1.7f, 0.05f, 4.7f);

            var obj = new GameObject("Location_Bank");
            obj.transform.position = position;
            obj.AddComponent<BankInteraction>();
        }

        static void EnsureWork()
        {
            if (GameObject.Find("Location_Work") != null)
            {
                return;
            }

            // Deliberately away from the commercial hub cluster (already 4 scenario
            // triggers there) - the far end of the main road stands in for "town."
            GameObject anchor = GameObject.Find(EastRoadAnchor);
            Vector3 position = anchor != null
                ? anchor.transform.position + new Vector3(0f, 0f, 0.3f)
                : new Vector3(4f, 0.05f, 0.3f);

            var obj = new GameObject("Location_Work");
            obj.transform.position = position;
            obj.AddComponent<WorkInteraction>();
        }
    }
}
