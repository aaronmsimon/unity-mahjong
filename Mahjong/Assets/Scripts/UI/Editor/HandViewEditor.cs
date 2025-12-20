using UnityEditor;
using UnityEngine;

namespace MJ.UI
{
    [CustomEditor(typeof(HandView))]
    public class HandViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            HandView hv = target as HandView;
            if (GUILayout.Button("Reveal Hand"))
                hv.RevealHand();
        }
    }
}