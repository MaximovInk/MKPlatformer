using UnityEditor;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [CustomEditor(typeof(MKCharacterController))]
    public class MKCharacterControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var c = (MKCharacterController)target;

            var abilities = c.GetComponentsInChildren<MKControllerComponent>();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label($"Abilities: {abilities.Length}");

            for (int i = 0; i < abilities.Length; i++)
            {
                if (GUILayout.Button($"{i+1}){abilities[i].name}"))
                {
                    Selection.activeGameObject = abilities[i].gameObject;
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
