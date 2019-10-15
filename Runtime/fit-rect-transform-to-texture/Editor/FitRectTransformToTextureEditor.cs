using BeatThat.Properties;
using UnityEditor;
using UnityEngine;

namespace BeatThat
{
    [CustomEditor(typeof(FitRectTransformToTexture))]
    public class FitRectTransformToTextureEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            TryEnsurePropertyHasTarget();
        }

        /// <summary>
        /// Tries to ensure the there is a (IHasValue<Texture>) target/wrapper component
        /// to provide the Texture
        /// </summary>
        private void TryEnsurePropertyHasTarget()
        {
            PropertyBindingEditor.HandleDrivenProperty<Texture>(
                this.target, 
                this.serializedObject.FindProperty("m_hasTexture"), 
                true
            );
            this.serializedObject.ApplyModifiedProperties();
        }
        
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Refit"))
            {
                (this.target as FitRectTransformToTexture).Refit();
            }
        }
    }
}


