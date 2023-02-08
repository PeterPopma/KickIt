using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationBatchImport : EditorWindow
{
    private static AnimationBatchImport editor;
    private static int width = 300;
    private static int height = 500;
    private static int x = 0;
    private static int y = 0;
    private static List<string> allFiles = new List<string>();
    private Vector2 scrollPos;

    private static Settings settings = new Settings();
    private const string settings_prefs_path = nameof(AnimationBatchImport) + "_lastsettings";
    private static Color linecolor = new Color32(128, 128, 128, 64);
    [System.Serializable]
    private class Settings
    {
        public string path = string.Empty;
        public bool rename_anim_clips = true;
        public bool rename_anim_clips_underscores = true;
        public bool rename_anim_clips_tolower = true;
        public bool change_loop_anim_clips = true;
        public bool loop_anim_clips_time = true;
        public bool loop_anim_clips_pose = false;
        public float loop_anim_clips_cycleoffset = 0f;
        public bool root_transform_rotation = false;
        public bool root_transform_rotation_bakeintopose = false;
        public int root_transform_rotation_popupindex = 0;
        public float root_transform_rotation_offset = 0f;
        public bool root_transform_positiony = false;
        public bool root_transform_positiony_bakeintopose = false;
        public int root_transform_positiony_popupindex = 0;
        public float root_transform_positiony_offset = 0f;
        public bool root_transform_positionxz = false;
        public bool root_transform_positionxz_bakeintopose = false;
        public int root_transform_positionxz_popupindex = 0;

        public bool disable_material_import = true;
        public bool mirror = false;
        public bool set_rig_to_humanoid = true;
        public Avatar rig_custom_avatar;
    }

    [MenuItem("Window/Animation/Animation Batch Import")]
    static void ShowEditor()
    {
        editor = EditorWindow.GetWindow<AnimationBatchImport>();
        CenterWindow();
        LoadSettings();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select directory"))
        {
            settings.path = EditorUtility.OpenFolderPanel("Select directory with files", "", "");
        }
        if (GUILayout.Button("Reset settings"))
        {
            settings = new Settings();
            SaveSettings();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label($"Path: {settings.path} ", EditorStyles.boldLabel);
        GUILayout.Label($"Selected: {Selection.gameObjects.Length} assets", EditorStyles.boldLabel);

        bool pathvalid = !string.IsNullOrEmpty(settings.path) && Directory.Exists(settings.path);
        if (!pathvalid)
        {
            GUI.color = Color.red;
            GUILayout.Label("Path invalid", EditorStyles.boldLabel);
            GUI.color = Color.white;
        }

        DrawUILine();
        settings.rename_anim_clips = EditorGUILayout.BeginToggleGroup("Rename animation clips to filename", settings.rename_anim_clips);
        {
            settings.rename_anim_clips_underscores = EditorGUILayout.Toggle("Spaces to underscores", settings.rename_anim_clips_underscores);
            settings.rename_anim_clips_tolower = EditorGUILayout.Toggle("To lowercase", settings.rename_anim_clips_tolower);
        }
        EditorGUILayout.EndToggleGroup();
        GUILayout.Space(5);

        DrawUILine();
        settings.change_loop_anim_clips = EditorGUILayout.BeginToggleGroup("Loop Time", settings.change_loop_anim_clips);
        {
            settings.loop_anim_clips_time = EditorGUILayout.Toggle("Loop Time", settings.loop_anim_clips_time);
            settings.loop_anim_clips_pose = EditorGUILayout.Toggle("Loop Pose", settings.loop_anim_clips_pose);
            settings.loop_anim_clips_cycleoffset = EditorGUILayout.FloatField("Cycle Offset", settings.loop_anim_clips_cycleoffset);
        }
        EditorGUILayout.EndToggleGroup();
        GUILayout.Space(5);

        DrawUILine();
        settings.root_transform_rotation = EditorGUILayout.BeginToggleGroup("Root Transform Rotation", settings.root_transform_rotation);
        {
            settings.root_transform_rotation_bakeintopose = EditorGUILayout.Toggle("Bake into pose", settings.root_transform_rotation_bakeintopose);
            settings.root_transform_rotation_popupindex = EditorGUILayout.Popup("Based Upon", settings.root_transform_rotation_popupindex, new string[]{
        "Original", "Body Orientation"
      });
            settings.root_transform_rotation_offset = EditorGUILayout.FloatField("Offset", settings.root_transform_rotation_offset);
        }
        EditorGUILayout.EndToggleGroup();
        GUILayout.Space(5);

        DrawUILine();
        settings.root_transform_positiony = EditorGUILayout.BeginToggleGroup("Root Transform Position (Y)", settings.root_transform_positiony);
        {
            settings.root_transform_positiony_bakeintopose = EditorGUILayout.Toggle("Bake into pose", settings.root_transform_positiony_bakeintopose);
            settings.root_transform_positiony_popupindex = EditorGUILayout.Popup("Based Upon", settings.root_transform_positiony_popupindex, new string[]{
        "Original", "Center of Mass", "Feet"
      });
            settings.root_transform_positiony_offset = EditorGUILayout.FloatField("Offset", settings.root_transform_positiony_offset);
        }
        EditorGUILayout.EndToggleGroup();
        GUILayout.Space(5);

        DrawUILine();
        settings.root_transform_positionxz = EditorGUILayout.BeginToggleGroup("Root Transform Position (XZ)", settings.root_transform_positionxz);
        {
            settings.root_transform_positionxz_bakeintopose = EditorGUILayout.Toggle("Bake into pose", settings.root_transform_positionxz_bakeintopose);
            settings.root_transform_positionxz_popupindex = EditorGUILayout.Popup("Based Upon", settings.root_transform_positionxz_popupindex, new string[]{
        "Original", "Center of Mass"
      });
        }
        EditorGUILayout.EndToggleGroup();
        GUILayout.Space(5);

        DrawUILine();
        settings.set_rig_to_humanoid = EditorGUILayout.Toggle("Set Rig to Humanoid", settings.set_rig_to_humanoid);
        settings.disable_material_import = EditorGUILayout.Toggle("Disable Material Import", settings.disable_material_import);
        settings.mirror = EditorGUILayout.Toggle("Mirror", settings.mirror);
        settings.rig_custom_avatar = EditorGUILayout.ObjectField("Custom Avatar", settings.rig_custom_avatar, typeof(Avatar), false) as Avatar;

        GUILayout.Space(30);
        DrawUILine();
        GUILayout.BeginHorizontal();

        GUI.enabled = pathvalid;
        if (GUILayout.Button("Process directory"))
        {
            process_dir();
            SaveSettings();
        }
        GUI.enabled = Selection.gameObjects.Length > 0;
        if (GUILayout.Button("Process selected assets"))
        {
            processSelectedAssets();
            SaveSettings();
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private static void SaveSettings()
    {
        string json = EditorJsonUtility.ToJson(settings);
        EditorPrefs.SetString(settings_prefs_path, json);
    }

    private static void LoadSettings()
    {
        settings = JsonUtility.FromJson<Settings>(EditorPrefs.GetString(settings_prefs_path));
        if (settings == null)
            settings = new Settings();
    }

    public void process_dir()
    {
        DirSearch(settings.path);

        if (allFiles.Count > 0)
        {
            for (int i = 0; i < allFiles.Count; i++)
            {
                int idx = allFiles[i].IndexOf("Assets");
                string filename = Path.GetFileName(allFiles[i]);
                string asset = allFiles[i].Substring(idx);
                AnimationClip orgClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(
                    asset, typeof(AnimationClip));

                var fileName = Path.GetFileNameWithoutExtension(allFiles[i]);
                var importer = (ModelImporter)AssetImporter.GetAtPath(asset);

                EditorUtility.DisplayProgressBar($"Processing {allFiles.Count} files", filename, (1f / allFiles.Count) * i);

                RenameAndImport(importer, fileName);
            }
        }

        EditorUtility.DisplayProgressBar($"Processing {allFiles.Count} files", "Saving assets", 1f);
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();

    }

    // Add a menu item called "rename animation" to a FBX's context menu.
    [MenuItem("Assets/RenameMixamoAnim")]
    static void RenameMixamoAnim()
    {
        //Rigidbody body = (Rigidbody)command.context;
        //body.mass = body.mass * 2;
        Debug.Log("Context: " + Selection.activeObject.ToString());
        UnityEngine.Object asset = Selection.activeGameObject;
        string assetpath = AssetDatabase.GetAssetPath(asset);
        AnimationClip orgClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetpath, typeof(AnimationClip));
        var fileName = asset.name;
        var importer = (ModelImporter)AssetImporter.GetAtPath(assetpath);
        RenameAndImport(importer, fileName);
    }

    public void processSelectedAssets()
    {
        int count = Selection.gameObjects.Length;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                UnityEngine.Object asset = Selection.gameObjects[i];
                string assetpath = AssetDatabase.GetAssetPath(asset);
                AnimationClip orgClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(
                    assetpath, typeof(AnimationClip));

                var fileName = asset.name;
                var importer = (ModelImporter)AssetImporter.GetAtPath(assetpath);

                EditorUtility.DisplayProgressBar($"Processing {count} files", fileName, (1f / count) * i);

                RenameAndImport(importer, fileName);
            }
        }

        EditorUtility.ClearProgressBar();

    }



    static void RenameAndImport(ModelImporter asset, string name)
    {
        ModelImporter modelImporter = asset as ModelImporter;
        ModelImporterClipAnimation[] clipAnimations = modelImporter.defaultClipAnimations;

        if (settings.disable_material_import)
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;

        if (settings.set_rig_to_humanoid)
            modelImporter.animationType = ModelImporterAnimationType.Human;

        if (settings.rig_custom_avatar != null)
            modelImporter.sourceAvatar = settings.rig_custom_avatar;

        if (settings.rename_anim_clips_underscores)
            name = name.Replace(' ', '_');

        if (settings.rename_anim_clips_tolower)
            name = name.ToLower();

        for (int i = 0; i < clipAnimations.Length; i++)
        {
            var clip = clipAnimations[i];

            if (settings.rename_anim_clips)
                clip.name = name;
            if (settings.change_loop_anim_clips)
            {
                clip.loopTime = settings.loop_anim_clips_time;
                clip.loopPose = settings.loop_anim_clips_pose;
                clip.cycleOffset = settings.loop_anim_clips_cycleoffset;
                if (settings.root_transform_rotation)
                {
                    clip.lockRootRotation = settings.root_transform_rotation_bakeintopose;
                    clip.keepOriginalOrientation = settings.root_transform_rotation_popupindex == 0;
                    clip.rotationOffset = settings.root_transform_rotation_offset;
                }
                if (settings.root_transform_positiony)
                {
                    clip.lockRootHeightY = settings.root_transform_positiony_bakeintopose;
                    clip.keepOriginalPositionY = settings.root_transform_positiony_popupindex == 0;
                    clip.heightFromFeet = settings.root_transform_positiony_popupindex == 2;
                    clip.heightOffset = settings.root_transform_positiony_offset;
                }
                if (settings.root_transform_positionxz)
                {
                    clip.lockRootPositionXZ = settings.root_transform_positionxz_bakeintopose;
                    clip.keepOriginalPositionXZ = settings.root_transform_positionxz_popupindex == 0;
                }
            }
        }

        modelImporter.clipAnimations = clipAnimations;
        modelImporter.SaveAndReimport();
    }

    private static void CenterWindow()
    {
        editor = EditorWindow.GetWindow<AnimationBatchImport>();
        x = (Screen.currentResolution.width - width) / 2;
        y = (Screen.currentResolution.height - height) / 2;
        editor.position = new Rect(x, y, width, height);
    }

    static void DirSearch(string path)
    {
        string[] fileInfo = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);
        foreach (string file in fileInfo)
        {
            if (file.EndsWith(".fbx"))
                allFiles.Add(file);
        }
    }

    private static void DrawUILine(int thickness = 1, int padding = 5)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, linecolor);
    }
}