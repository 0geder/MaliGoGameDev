using UnityEngine;

namespace MaliGo.Environment
{
    /// <summary>
    /// Switches an opaque material into "fade" transparency mode so it can be
    /// alpha-animated for the building cutaway reveal. Supports URP Lit/Simple Lit
    /// and the legacy Standard shader, since your project may have either depending
    /// on where an asset pack material comes from.
    /// </summary>
    public static class MaterialTransparencyUtility
    {
        public static void EnableFadeTransparency(Material mat)
        {
            if (mat == null) return;

            bool isURP = mat.shader != null && mat.shader.name.Contains("Universal Render Pipeline");

            if (isURP)
            {
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // 0 = Opaque, 1 = Transparent
                if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);     // 0 = Alpha blend
                if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                if (mat.HasProperty("_SrcBlend")) mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (mat.HasProperty("_DstBlend")) mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else // legacy Standard shader
            {
                if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 2f); // Fade mode
                if (mat.HasProperty("_SrcBlend")) mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (mat.HasProperty("_DstBlend")) mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
        }

        public static void SetAlpha(Material mat, float alpha)
        {
            if (mat == null) return;

            string prop = mat.HasProperty("_BaseColor") ? "_BaseColor" : (mat.HasProperty("_Color") ? "_Color" : null);
            if (prop == null) return;

            Color c = mat.GetColor(prop);
            c.a = alpha;
            mat.SetColor(prop, c);
        }
    }
}