using System;

namespace MaliGo.Data
{
    [Serializable]
    public class ProgressionData
    {
        public int level = 1;
        public float xp = 0f;
        public float xpToNextLevel = 100f;

        public float LevelProgressNormalized =>
            xpToNextLevel > 0f ? UnityEngine.Mathf.Clamp01(xp / xpToNextLevel) : 0f;
    }
}
