using BeatThat.Properties;
using UnityEditor;
using UnityEngine;

namespace BeatThat
{
    [CustomEditor(typeof(FitRectTransformToTexture))]
    public class FitRectTransformToTextureEditor : UnityEditor.Editor
    {

        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Refit")) {
                (this.target as FitRectTransformToTexture).Refit();
            }
        }
    }
}


