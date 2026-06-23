using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// Editor共通のGUIStyle生成。
    /// </summary>
    public static class EditorStyleFactory
    {
        private static GUIStyle _stepButtonStyle;
        private static GUIStyle _stepLabelStyle;

        public static GUIStyle StepButtonStyle
        {
            get
            {
                if (_stepButtonStyle == null)
                {
                    _stepButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 36,
                        margin = new RectOffset(0, 0, 4, 4),
                        padding = new RectOffset(10, 10, 0, 0)
                    };
                }
                return _stepButtonStyle;
            }
        }

        public static GUIStyle StepLabelStyle
        {
            get
            {
                if (_stepLabelStyle == null)
                {
                    _stepLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 12,
                        margin = new RectOffset(0, 0, 8, 4)
                    };
                }
                return _stepLabelStyle;
            }
        }
    }
}
