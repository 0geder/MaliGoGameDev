using System;

namespace MaliGo.Data
{
    [Serializable]
    public class AppearanceData
    {
        public string skinTone = "medium";
        public string hairstyle = "short";
        public string hairColor = "black";
        public string clothing = "casual";
        public string accessories = "none";
        public string genderPresentation = "neutral";
        public string bodyType = "average";

        public AppearanceData Clone()
        {
            return new AppearanceData
            {
                skinTone = skinTone,
                hairstyle = hairstyle,
                hairColor = hairColor,
                clothing = clothing,
                accessories = accessories,
                genderPresentation = genderPresentation,
                bodyType = bodyType
            };
        }
    }
}
