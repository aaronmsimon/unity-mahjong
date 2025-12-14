using UnityEditor;
using UnityEngine;

namespace MJ.UI
{
    [CustomEditor(typeof(TileView))]
    public class TileViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            TileView tv = target as TileView;
            if (GUILayout.Button("Flip"))
                tv.SetFaceUp(!tv.isFaceUp);
        }
    }
}