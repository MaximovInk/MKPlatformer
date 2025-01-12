using UnityEditor;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [CustomEditor(typeof(MKControllerComponent), true)]
    public class MKControllerAbilityEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var ability = (MKControllerComponent)target;

            if (GUILayout.Button("Return to character"))
            {
                var c = ability.transform.GetComponentInParent<MKCharacterController>();
                if (c != null)
                {
                    Selection.activeGameObject = c.gameObject;
                }
                

            }

            base.OnInspectorGUI();
        }
    }
}
