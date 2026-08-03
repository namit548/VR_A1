using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class SafeMultiPlatformAssetBundleBuilder : EditorWindow
{
    // Bundle naming - customizable
    private string bundleName1 = "delta1a";
    private string bundleName2 = "delta2o";
    
    // Scene management - 2 scenes per platform
    private SceneAsset androidMainMenuScene;
    private SceneAsset androidMainScene;
    private SceneAsset oculusMainMenuScene;
    private SceneAsset oculusMainScene;
    private SceneAsset desktopMainMenuScene;
    private SceneAsset desktopMainScene;
    private SceneAsset webglMainMenuScene;
    private SceneAsset webglMainScene;

    // Build process state
    private bool isBuilding = false;
    private string currentStatus = "Ready to build";
    private float buildProgress = 0f;
    private string outputBasePath = "AssetBundles";
    
    // Timer-based platform switching
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private string waitReason = "";
    private int currentBuildStep = 0;
    private int totalBuildSteps = 3; // Android, Desktop, WebGL
    
    // UI State
    private Vector2 scrollPosition;

    [MenuItem("Tools/Safe Multi-Platform Bundle Builder")]
    public static void ShowWindow()
    {
        GetWindow<SafeMultiPlatformAssetBundleBuilder>("Safe AssetBundle Builder");
    }

    void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
        // Header
        GUILayout.Space(10);
        GUILayout.Label("Android AssetBundle Builder (Test Version)", EditorStyles.largeLabel);
        GUILayout.Space(5);
        
        // Status and Progress
        DrawStatusSection();
        
        GUILayout.Space(10);
        
        // Bundle Naming Section
        GUILayout.Label("Bundle Naming", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");
        bundleName1 = EditorGUILayout.TextField("Bundle Name 1 (Android scenes)", bundleName1);
        bundleName2 = EditorGUILayout.TextField("Bundle Name 2 (Oculus scenes)", bundleName2);
        GUILayout.EndVertical();

        GUILayout.Space(10);
        
        // Android Platform Only
        GUILayout.Label("Android Platform Only", EditorStyles.boldLabel);
        GUILayout.Space(5);
        
        // Android Platform
        GUILayout.BeginVertical("box");
        GUILayout.Label("📱 Android Platform", EditorStyles.boldLabel);
        GUILayout.Label($"Bundle: {bundleName1} (Android scenes)", EditorStyles.miniLabel);
        androidMainMenuScene = (SceneAsset)EditorGUILayout.ObjectField("Android Main Menu Scene", androidMainMenuScene, typeof(SceneAsset), false);
        androidMainScene = (SceneAsset)EditorGUILayout.ObjectField("Android Main Scene", androidMainScene, typeof(SceneAsset), false);
        
        GUILayout.Space(5);
        GUILayout.Label($"Bundle: {bundleName2} (Oculus scenes)", EditorStyles.miniLabel);
        oculusMainMenuScene = (SceneAsset)EditorGUILayout.ObjectField("Oculus Main Menu Scene", oculusMainMenuScene, typeof(SceneAsset), false);
        oculusMainScene = (SceneAsset)EditorGUILayout.ObjectField("Oculus Main Scene", oculusMainScene, typeof(SceneAsset), false);
        GUILayout.EndVertical();
        
        GUILayout.Space(20);
        
        // Build Button - Android Only
        GUI.enabled = !isBuilding && HasAndroidScenesToBuild();
        if (GUILayout.Button("🚀 Build Android AssetBundles Only", GUILayout.Height(40)))
        {
            BuildAndroidOnly();
        }
        GUI.enabled = true;
        
        if (isBuilding)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("🛑 Cancel Build"))
            {
                CancelBuild();
            }
        }
        
        GUILayout.EndScrollView();
        
        // Update progress if building
        if (isBuilding)
        {
            Repaint();
        }
    }

    void DrawStatusSection()
    {
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"Status: {currentStatus}", EditorStyles.boldLabel);
        
        if (isBuilding)
        {
            GUILayout.Space(5);
            Rect progressRect = GUILayoutUtility.GetRect(0, 20);
            EditorGUI.ProgressBar(progressRect, buildProgress, $"Progress: {currentBuildStep}/{totalBuildSteps} platforms completed");
            
            if (isWaiting)
            {
                GUILayout.Space(5);
                GUILayout.Label($"⏳ {waitReason}", EditorStyles.miniLabel);
                GUILayout.Label($"Time remaining: {Mathf.Ceil(waitTimer)} seconds", EditorStyles.miniLabel);
            }
        }
        
        GUILayout.EndVertical();
    }
    
    bool HasAndroidScenesToBuild()
    {
        return (androidMainMenuScene != null && androidMainScene != null) ||
               (oculusMainMenuScene != null && oculusMainScene != null);
    }
    
    // Commented out for testing
    /*
    bool HasScenesToBuild()
    {
        return (androidMainMenuScene != null && androidMainScene != null) ||
               (oculusMainMenuScene != null && oculusMainScene != null) ||
               (desktopMainMenuScene != null && desktopMainScene != null) ||
               (webglMainMenuScene != null && webglMainScene != null);
    }
    */
    
    void CancelBuild()
    {
        isBuilding = false;
        isWaiting = false;
        currentStatus = "Build cancelled";
        buildProgress = 0f;
        currentBuildStep = 0;
        waitTimer = 0f;
        Debug.Log("🛑 Build process cancelled by user");
    }

    // Draw scene list with proper Unity Object Picker
    void DrawSceneList(List<string> sceneList)
    {
        // Existing scenes
        for (int i = 0; i < sceneList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            
            // Display scene name instead of full path
            string sceneName = Path.GetFileNameWithoutExtension(sceneList[i]);
            GUILayout.Label($"📄 {sceneName}", GUILayout.Width(150));
            GUILayout.Label(sceneList[i], EditorStyles.miniLabel);
            
            if (GUILayout.Button("❌", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("Remove Scene", 
                    $"Remove '{sceneName}' from this platform?", 
                    "Remove", "Cancel"))
            {
                sceneList.RemoveAt(i);
                i--; // adjust index after removal
                }
            }
            GUILayout.EndHorizontal();
        }

        // Add new scene using Unity Object Picker
        GUILayout.BeginHorizontal();
        GUILayout.Label("Add Scene:", GUILayout.Width(80));
        SceneAsset newScene = (SceneAsset)EditorGUILayout.ObjectField("", null, typeof(SceneAsset), false);
        if (newScene != null)
        {
            string path = AssetDatabase.GetAssetPath(newScene);
            if (!sceneList.Contains(path))
            {
                sceneList.Add(path);
                Debug.Log($"✅ Added scene '{newScene.name}' to platform");
            }
            else
            {
                Debug.LogWarning($"⚠️ Scene '{newScene.name}' is already added to this platform");
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
    }

    // Simple Android-only build method
    void BuildAndroidOnly()
    {
        isBuilding = true;
        currentStatus = "Building Android AssetBundles...";
        buildProgress = 0f;
        
        Debug.Log("🧱 Starting Android-only build...");
        Debug.Log($"📦 Bundle names: {bundleName1} and {bundleName2}");
        
        // Build Android bundle (bundleName1)
        if (androidMainMenuScene != null && androidMainScene != null)
        {
            Debug.Log("📱 Building Android bundle...");
            BuildPlatformBundle("Android", bundleName1, new SceneAsset[] { androidMainMenuScene, androidMainScene }, BuildTarget.Android);
        }
        else
        {
            Debug.LogWarning("⚠️ No Android scenes assigned");
        }
        
        // Build Oculus bundle (bundleName2) - also on Android platform
        if (oculusMainMenuScene != null && oculusMainScene != null)
        {
            Debug.Log("🕶️ Building Oculus bundle...");
            BuildPlatformBundle("Android", bundleName2, new SceneAsset[] { oculusMainMenuScene, oculusMainScene }, BuildTarget.Android);
        }
        else
        {
            Debug.LogWarning("⚠️ No Oculus scenes assigned");
        }
        
        // Complete
        isBuilding = false;
        currentStatus = "Android build completed!";
        buildProgress = 1.0f;
        
        Debug.Log("🎉 Android AssetBundle build completed!");
        EditorUtility.DisplayDialog("Build Complete", 
            "Successfully built Android AssetBundles!\n\nCheck the AssetBundles/Android folder.", 
            "OK");
    }
    
    // Commented out complex build process for testing
    /*
    void StartBuildProcess()
    {
        // Reset build state
        isBuilding = true;
        currentBuildStep = 0;
        currentStatus = "Initializing build process...";
        buildProgress = 0f;
        
        Debug.Log("🧱 Starting timer-based multi-platform build...");
        Debug.Log("📋 Build order: Android → Desktop → WebGL");
        Debug.Log($"📦 Bundle names: {bundleName1} and {bundleName2}");
        
        // Start with Android (2 bundles)
        BuildAndroidBundles();
    }
    */
    
    // Commented out complex timer-based methods for testing
    /*
    void Update()
    {
        if (isBuilding && isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                waitTimer = 0f;
                ContinueBuildProcess();
            }
        }
    }
    
    void BuildAndroidBundles()
    {
        currentBuildStep = 1;
        currentStatus = "Building Android bundles...";
        buildProgress = 0.33f;
        
        Debug.Log("📱 Step 1: Building Android bundles");
        
        // Build Android bundle (bundleName1)
        if (androidMainMenuScene != null && androidMainScene != null)
        {
            BuildPlatformBundle("Android", bundleName1, new SceneAsset[] { androidMainMenuScene, androidMainScene }, BuildTarget.Android);
        }
        
        // Build Oculus bundle (bundleName2) - also on Android platform
        if (oculusMainMenuScene != null && oculusMainScene != null)
        {
            BuildPlatformBundle("Android", bundleName2, new SceneAsset[] { oculusMainMenuScene, oculusMainScene }, BuildTarget.Android);
        }
        
        Debug.Log("✅ Android bundles completed, switching to Desktop...");
        SwitchToDesktop();
    }
    
    void SwitchToDesktop()
    {
        currentStatus = "Switching to Desktop platform...";
        Debug.Log("🖥️ Switching to StandaloneWindows...");
        
        try
        {
            bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(BuildTarget.StandaloneWindows), BuildTarget.StandaloneWindows);
            if (switchResult)
            {
                Debug.Log("✅ Successfully switched to StandaloneWindows");
                BuildDesktopBundles();
            }
            else
            {
                Debug.LogError("❌ Failed to switch to StandaloneWindows");
                StartWaitTimer("Platform switch failed, retrying...", 5f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error switching to StandaloneWindows: {e.Message}");
            StartWaitTimer("Platform switch error, retrying...", 5f);
        }
    }
    
    void BuildDesktopBundles()
    {
        currentBuildStep = 2;
        currentStatus = "Building Desktop bundles...";
        buildProgress = 0.66f;
        
        Debug.Log("🖥️ Step 2: Building Desktop bundles");
        
        if (desktopMainMenuScene != null && desktopMainScene != null)
        {
            BuildPlatformBundle("Desktop", bundleName1, new SceneAsset[] { desktopMainMenuScene, desktopMainScene }, BuildTarget.StandaloneWindows);
        }
        
        Debug.Log("✅ Desktop bundles completed, waiting before WebGL switch...");
        StartWaitTimer("Waiting 2 minutes before WebGL switch for safety...", 120f); // 2 minutes
    }
    
    void ContinueBuildProcess()
    {
        if (currentBuildStep == 2) // Coming from Desktop wait
        {
            SwitchToWebGL();
        }
        else if (currentBuildStep == 3) // Coming from WebGL wait
        {
            BuildWebGLBundles();
        }
    }
    
    void SwitchToWebGL()
    {
        currentStatus = "Switching to WebGL platform...";
        Debug.Log("🌐 Switching to WebGL...");
        
        try
        {
            bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(BuildTarget.WebGL), BuildTarget.WebGL);
            if (switchResult)
            {
                Debug.Log("✅ Successfully switched to WebGL");
                StartWaitTimer("Waiting for WebGL compilation to complete...", 30f); // 30 seconds for WebGL
            }
            else
            {
                Debug.LogError("❌ Failed to switch to WebGL");
                StartWaitTimer("WebGL switch failed, retrying...", 10f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error switching to WebGL: {e.Message}");
            StartWaitTimer("WebGL switch error, retrying...", 10f);
        }
    }
    
    void BuildWebGLBundles()
    {
        currentBuildStep = 3;
        currentStatus = "Building WebGL bundles...";
        buildProgress = 1.0f;
        
        Debug.Log("🌐 Step 3: Building WebGL bundles");
        
        if (webglMainMenuScene != null && webglMainScene != null)
        {
            BuildPlatformBundle("WebGL", bundleName1, new SceneAsset[] { webglMainMenuScene, webglMainScene }, BuildTarget.WebGL);
        }
        
        // Complete the build
        CompleteBuild();
    }
    
    void CompleteBuild()
    {
        currentBuildStep = 3;
        currentStatus = "All builds completed successfully!";
        buildProgress = 1.0f;
        isBuilding = false;
        
        Debug.Log("🎉 All AssetBundle builds completed successfully!");
        EditorUtility.DisplayDialog("Build Complete", 
            "Successfully built AssetBundles for all platforms!\n\nCheck the AssetBundles folder for the generated bundles.", 
            "OK");
    }
    
    void StartWaitTimer(string reason, float seconds)
    {
        isWaiting = true;
        waitReason = reason;
        waitTimer = seconds;
        Debug.Log($"⏳ {reason} ({seconds} seconds)");
    }
    */
    
    void BuildPlatformBundle(string platformName, string bundleName, SceneAsset[] scenes, BuildTarget target)
    {
        Debug.Log($"📦 Building {platformName} bundle '{bundleName}' for {target}");
        
        string outputPath = Path.Combine(outputBasePath, platformName);
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
            Debug.Log($"📁 Created directory: {outputPath}");
        }

        // Create AssetBundleBuild array - 1 bundle with 2 scenes
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        
        // Get scene paths
        string mainMenuPath = AssetDatabase.GetAssetPath(scenes[0]);
        string mainScenePath = AssetDatabase.GetAssetPath(scenes[1]);
        
        // Single bundle with both scenes
        buildMap[0].assetBundleName = bundleName;
        buildMap[0].assetNames = new string[] { mainMenuPath, mainScenePath };
        
        Debug.Log($"  Creating bundle '{bundleName}' with scenes: {scenes[0].name} + {scenes[1].name}");
        
        try
        {
            // Build AssetBundles
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, buildMap, BuildAssetBundleOptions.None, target);
            
            if (manifest != null)
            {
                Debug.Log($"✅ Successfully built {platformName} bundle '{bundleName}'!");
                foreach (string bundle in manifest.GetAllAssetBundles())
                {
                    string bundlePath = Path.Combine(outputPath, bundle);
                    if (File.Exists(bundlePath))
                    {
                        FileInfo fileInfo = new FileInfo(bundlePath);
                        Debug.Log($"  📊 {bundle}: {fileInfo.Length / 1024 / 1024:F2} MB");
                    }
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to build {platformName} bundle '{bundleName}' - no manifest");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error building {platformName} bundle '{bundleName}': {e.Message}");
        }
    }

}
