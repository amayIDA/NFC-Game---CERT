using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyberQuiz
{
    public class CarnivalKeyboard : MonoBehaviour
    {
        public InputField nameField, emailField;
        public InputField active;
        public CarnivalKey[] letterKeys;
        public Image nameHighlight, emailHighlight;
        public CanvasGroup group;
        public Text targetLabel;

        void Awake()
        {
            if(nameField!=null)nameField.shouldHideMobileInput=true;
            if(emailField!=null)emailField.shouldHideMobileInput=true;
            if(group==null)group=GetComponent<CanvasGroup>();
            Hide();
        }
        void Update()
        {
            var selected=EventSystem.current!=null?EventSystem.current.currentSelectedGameObject:null;
            if(nameField!=null&&selected==nameField.gameObject)Focus(nameField);
            else if(emailField!=null&&selected==emailField.gameObject)Focus(emailField);
        }
        public void Focus(InputField field)
        {
            if(field==null)return;
            active=field;
            Show();
            Refresh();
        }
        public void Show()
        {
            if(group==null)return;
            bool closed=group.alpha<0.5f;
            group.alpha=1;group.interactable=true;group.blocksRaycasts=true;
            gameObject.SetActive(true);
            if(closed)CarnivalAudio.Whoosh();
        }
        public void Hide()
        {
            if(group==null)group=GetComponent<CanvasGroup>();
            if(group==null)return;
            group.alpha=0;group.interactable=false;group.blocksRaycasts=false;
            if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(null);
        }
        public void Type(string glyph)
        {
            CarnivalAudio.Key();
            if(glyph=="HIDE"){Hide();return;}
            if(glyph=="NEXT")
            {
                Focus(active==nameField?emailField:nameField);
                if(active!=null)active.Select();
                return;
            }
            if(active==null)return;
            if(glyph=="CLEAR"){active.text="";KeepField();return;}
            if(glyph=="BACK")
            {
                if(active.text.Length>0)active.text=active.text.Substring(0,active.text.Length-1);
            }
            else if(glyph=="SPACE") Append(" ");
            else if(!string.IsNullOrEmpty(glyph)) Append(glyph.ToLowerInvariant());
            KeepField();
        }
        void KeepField()
        {
            if(active==null)return;
            active.caretPosition=active.text.Length;
            active.ForceLabelUpdate();
            active.ActivateInputField();
            active.Select();
        }
        void Append(string value)
        {
            if(active.characterLimit>0&&active.text.Length>=active.characterLimit)return;
            active.text+=value;
        }
        void Refresh()
        {
            if(nameHighlight!=null)nameHighlight.color=Color.white;
            if(emailHighlight!=null)emailHighlight.color=Color.white;
            if(targetLabel!=null)targetLabel.text=active==emailField?"Email":"Name";
        }
    }
}
