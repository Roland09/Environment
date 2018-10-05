using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    public class PrefabPainterStyles
    {

        private static GUIStyle _boxTitleStyle;
        public static GUIStyle BoxTitleStyle
        {
            get
            {
                if (_boxTitleStyle == null)
                {
                    _boxTitleStyle = new GUIStyle("Label")
                    {
                        //fontStyle = FontStyle.BoldAndItalic
                        fontStyle = FontStyle.Italic
                    };
                }
                return _boxTitleStyle;
            }
        }
    }
}