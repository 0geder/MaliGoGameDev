using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Android tooling + Player Settings configuration for the MaliGo beta.
///
/// This Unity install ships ONLY the il2cpp player variation (no mono variation exists at
/// PlaybackEngines/AndroidPlayer/Variations), so IL2CPP is the only viable backend - Mono
/// fails with "Mono2x library missing for the selected architecture". IL2CPP needs the NDK,
/// installed into the Android Studio SDK via sdkmanager rather than Unity's own copy.
/// </summary>
public static class MaliGoAndroidSetup
{
    const string SdkPath = @"C:\Users\0geda\AppData\Local\Android\Sdk";
    // Unity 6000.3 requires JDK 17 specifically. Android Studio's bundled JBR is Java 21,
    // which Unity rejects outright ("Incompatible Java version '21.0.10'"), so a dedicated
    // Temurin JDK 17 is installed here just for Unity's Gradle step.
    const string JdkPath = @"C:\JDK17";
    const string NdkPath = @"C:\Users\0geda\AppData\Local\Android\Sdk\ndk\27.2.12479018";

    [MenuItem("MaliGo/Android/Configure Tooling + Player Settings")]
    public static void Configure()
    {
        // Unity ignores custom tool paths while these "use embedded" flags are set, which is
        // why the JDK kept reverting to Unity's own (non-existent) bundled OpenJDK folder.
        EditorPrefs.SetBool("SdkUseEmbedded", false);
        EditorPrefs.SetBool("JdkUseEmbedded", false);
        EditorPrefs.SetBool("NdkUseEmbedded", false);

        // Setting these throws if a path fails Unity's validation. Don't let that abort the
        // whole build - a previously valid configuration can still carry it through.
        TrySetPath("SDK", SdkPath, p => AndroidExternalToolsSettings.sdkRootPath = p);
        TrySetPath("JDK", JdkPath, p => AndroidExternalToolsSettings.jdkRootPath = p);
        TrySetPath("NDK", NdkPath, p => AndroidExternalToolsSettings.ndkRootPath = p);

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.maligo.app");
        PlayerSettings.companyName = "MaliGo";
        PlayerSettings.productName = "MaliGo";

        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        PlayerSettings.bundleVersion = string.IsNullOrWhiteSpace(PlayerSettings.bundleVersion) ? "0.1.0" : PlayerSettings.bundleVersion;
        PlayerSettings.Android.bundleVersionCode = Mathf.Max(1, PlayerSettings.Android.bundleVersionCode);

        Debug.Log($"[MaliGoAndroidSetup] SDK -> {AndroidExternalToolsSettings.sdkRootPath}\n" +
                  $"JDK -> {AndroidExternalToolsSettings.jdkRootPath}\n" +
                  $"NDK -> {AndroidExternalToolsSettings.ndkRootPath}\n" +
                  $"Package -> {PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)}\n" +
                  $"Scripting backend -> {PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}\n" +
                  $"Architecture -> {PlayerSettings.Android.targetArchitectures}\n" +
                  $"Min SDK -> {PlayerSettings.Android.minSdkVersion}, Target SDK -> {PlayerSettings.Android.targetSdkVersion}\n" +
                  $"Version {PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode})");
    }

    static void TrySetPath(string label, string path, System.Action<string> setter)
    {
        if (!System.IO.Directory.Exists(path))
        {
            Debug.LogWarning($"[MaliGoAndroidSetup] {label} path does not exist: {path}");
            return;
        }

        try
        {
            setter(path);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MaliGoAndroidSetup] Could not set {label} path to '{path}': {ex.Message}\n" +
                             "Continuing with whatever is already configured.");
        }
    }

    [MenuItem("MaliGo/Android/Verify Tooling")]
    public static void Verify()
    {
        bool sdkOk = System.IO.Directory.Exists(AndroidExternalToolsSettings.sdkRootPath);
        bool jdkOk = System.IO.Directory.Exists(AndroidExternalToolsSettings.jdkRootPath);
        bool ndkOk = System.IO.Directory.Exists(AndroidExternalToolsSettings.ndkRootPath);
        Debug.Log($"[MaliGoAndroidSetup] SDK '{AndroidExternalToolsSettings.sdkRootPath}' exists: {sdkOk}\n" +
                  $"JDK '{AndroidExternalToolsSettings.jdkRootPath}' exists: {jdkOk}\n" +
                  $"NDK '{AndroidExternalToolsSettings.ndkRootPath}' exists: {ndkOk}");
    }
}
