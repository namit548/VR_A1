//using UnityEngine;
//using UnityEditor;
//using UnityEditor.SceneManagement;
//using UnityEngine.UI;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using System.IO;

//public class UIDuplicateSpriteBatchReplacer : EditorWindow
//{
//    private DefaultAsset originalSpriteFolder;
//    private List<SceneAsset> targetScenes = new List<SceneAsset>();

//    private Dictionary<string, Sprite> originalSprites = new Dictionary<string, Sprite>();
//    private static readonly Regex duplicatePattern = new Regex(@"^(.*?)(?:\s\(\d+\)|\s\d+| copy)$", RegexOptions.IgnoreCase);

//    [MenuItem("Tools/Batch Replace Duplicate UI Sprites")]
//    public static void ShowWindow()
//    {
//        GetWindow<UIDuplicateSpriteBatchReplacer>("Batch UI Sprite Replacer");
//    }

//    private void OnGUI()
//    {
//        EditorGUILayout.LabelField("Original Sprites Folder", EditorStyles.boldLabel);
//        originalSpriteFolder = (DefaultAsset)EditorGUILayout.ObjectField(originalSpriteFolder, typeof(DefaultAsset), false);

//        GUILayout.Space(10);
//        EditorGUILayout.LabelField("Target Scenes", EditorStyles.boldLabel);
//        for (int i = 0; i < targetScenes.Count; i++)
//        {
//            EditorGUILayout.BeginHorizontal();
//            targetScenes[i] = (SceneAsset)EditorGUILayout.ObjectField(targetScenes[i], typeof(SceneAsset), false);
//            if (GUILayout.Button("Remove", GUILayout.Width(60)))
//            {
//                targetScenes.RemoveAt(i);
//                i--;
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        if (GUILayout.Button("Add Scene"))
//        {
//            targetScenes.Add(null);
//        }

//        GUILayout.Space(20);
//        if (GUILayout.Button("Replace Duplicates"))
//        {
//            if (originalSpriteFolder == null || targetScenes.Count == 0)
//            {
//                Debug.LogError("Assign both the original sprite folder and at least one target scene.");
//                return;
//            }

//            LoadOriginalSprites();
//            ReplaceInScenes();
//        }
//    }

//    private void LoadOriginalSprites()
//    {
//        originalSprites.Clear();

//        string folderPath = AssetDatabase.GetAssetPath(originalSpriteFolder);
//        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

//        foreach (string guid in guids)
//        {
//            string path = AssetDatabase.GUIDToAssetPath(guid);
//            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
//            if (sprite != null && !originalSprites.ContainsKey(sprite.name))
//            {
//                originalSprites[sprite.name] = sprite;
//            }
//        }

//        Debug.Log($"Loaded {originalSprites.Count} original sprites.");
//    }

//    private void ReplaceInScenes()
//    {
//        int totalReplacements = 0;

//        foreach (var sceneAsset in targetScenes)
//        {
//            if (sceneAsset == null) continue;

//            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
//            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

//            int sceneReplacements = 0;

//            var images = GameObject.FindObjectsOfType<Image>(true);
//            foreach (var img in images)
//            {
//                if (img.sprite != null)
//                {
//                    Sprite replacement = GetOriginalMatch(img.sprite);
//                    if (replacement != null && replacement != img.sprite)
//                    {
//                        Debug.Log($"[Image] {img.name}: {img.sprite.name} -> {replacement.name}");
//                        img.sprite = replacement;
//                        EditorUtility.SetDirty(img);
//                        sceneReplacements++;
//                    }
//                }
//            }

//            var selectables = GameObject.FindObjectsOfType<Selectable>(true);
//            foreach (var selectable in selectables)
//            {
//                var state = selectable.spriteState;
//                bool changed = false;

//                Sprite orig;

//                orig = GetOriginalMatch(state.highlightedSprite);
//                if (orig != null && orig != state.highlightedSprite) { state.highlightedSprite = orig; changed = true; }

//                orig = GetOriginalMatch(state.pressedSprite);
//                if (orig != null && orig != state.pressedSprite) { state.pressedSprite = orig; changed = true; }

//                orig = GetOriginalMatch(state.selectedSprite);
//                if (orig != null && orig != state.selectedSprite) { state.selectedSprite = orig; changed = true; }

//                orig = GetOriginalMatch(state.disabledSprite);
//                if (orig != null && orig != state.disabledSprite) { state.disabledSprite = orig; changed = true; }

//                if (changed)
//                {
//                    selectable.spriteState = state;
//                    EditorUtility.SetDirty(selectable);
//                    sceneReplacements++;
//                }
//            }

//            EditorSceneManager.MarkSceneDirty(scene);
//            EditorSceneManager.SaveScene(scene);
//            Debug.Log($" Replaced {sceneReplacements} references in scene: {scene.name}");
//            totalReplacements += sceneReplacements;
//        }

//        Debug.Log($" Total replacements across all scenes: {totalReplacements}");
//    }

//    private Sprite GetOriginalMatch(Sprite sprite)
//    {
//        if (sprite == null) return null;

//        string name = sprite.name;

//        // Exact match
//        if (originalSprites.ContainsKey(name))
//            return originalSprites[name];

//        // Clean up known duplicate suffix patterns
//        string cleanedName = Regex.Replace(name, @"(?:\s*[-_]?\s*(copy|\(\d+\)|\d+))+$", "", RegexOptions.IgnoreCase).Trim();

//        if (originalSprites.ContainsKey(cleanedName))
//            return originalSprites[cleanedName];

//        return null;
//    }

//}
