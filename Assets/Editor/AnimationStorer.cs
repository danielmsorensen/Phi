using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimationStorer : EditorWindow {

    RuntimeAnimatorController ac;
    string clipName;
    AnimationClip anim;

    [MenuItem("Window/Animation Storer")]
    public static void ShowWindow() {
        GetWindow(typeof(AnimationStorer));
    }

    void OnGUI() {
        int nf = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = 17;
        GUILayout.Label("Animation Storer");
        GUI.skin.label.fontSize = nf;

        GUILayout.Space(20f);

        ac = EditorGUILayout.ObjectField("Animation Controller", ac, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;

        GUILayout.Space(10f);

        if (anim == null) clipName = EditorGUILayout.TextField("Animation Name", clipName);
        if (string.IsNullOrEmpty(clipName)) anim = EditorGUILayout.ObjectField("Animation", anim, typeof(AnimationClip), false) as AnimationClip;

        GUILayout.Space(20f);

        if(GUILayout.Button("Store In Controller")) {
            AddToController();
        }
        if (GUILayout.Button("Strip From Controller")) {
            RemoveFromController();
        }
    }

    void AddToController() {
         AnimationClip clip = (anim == null) ? new AnimationClip() : anim;
         clip.name = (anim == null) ? clipName : clip.name;
         AssetDatabase.AddObjectToAsset(clip, ac);
         AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip));
	}

    void RemoveFromController() {
        if(anim != null) {
            DestroyImmediate(anim, true);
            AssetDatabase.SaveAssets();
        }
        else {
            if (EditorUtility.DisplayDialog("Cannot Remove Animation", "You must use an object reference to remove an animation clip", "OK")) clipName = "";
        }
    }
}
