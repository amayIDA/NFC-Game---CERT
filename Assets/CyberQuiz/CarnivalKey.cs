using UnityEngine;
using UnityEngine.UI;

namespace CyberQuiz
{
    public class CarnivalKey : MonoBehaviour
    {
        public CarnivalKeyboard keyboard;
        public string glyph;
        public Text label;
        public Button button;
        public void Press(){if(keyboard!=null)keyboard.Type(glyph);}
    }
}
