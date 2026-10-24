using UnityEngine;
using UnityEngine.UI;
namespace CyberQuiz
{
    public class CarnivalAmbient : MonoBehaviour
    {
        public CarnivalQuiz quiz;
        public RectTransform titleArtwork,tunnel,feedbackPanel;
        public RectTransform[] idleGlints;
        public float titleScaleAmount=.009f;
        public float floatHeight=10f;
        Vector2 titleHome,tunnelHome;
        Vector3 titleScale,tunnelScale;
        Quaternion titleRotation;
        float clock,idleClock;
        bool wasIdle;
        void Awake()
        {
            if(titleArtwork!=null){titleHome=titleArtwork.anchoredPosition;titleScale=titleArtwork.localScale;titleRotation=titleArtwork.localRotation;}
            if(tunnel!=null){tunnelHome=tunnel.anchoredPosition;tunnelScale=tunnel.localScale;}
        }
        void Update()
        {
            if(quiz==null)return;
            bool idle=titleArtwork!=null&&titleArtwork.gameObject.activeInHierarchy;
            if(quiz.reducedMotion){Restore();SetGlints(false);return;}
            clock+=Time.unscaledDeltaTime;
            if(idle)
            {
                if(!wasIdle)idleClock=0;
                idleClock+=Time.unscaledDeltaTime;float ease=Mathf.SmoothStep(0,1,idleClock/.9f);
                if(titleArtwork!=null){titleArtwork.anchoredPosition=titleHome;titleArtwork.localScale=titleScale;titleArtwork.localRotation=titleRotation;}
                if(tunnel!=null)
                {
                    tunnel.localScale=tunnelScale*(1+(.025f+Mathf.Sin(idleClock*.29f)*.007f)*ease);
                    tunnel.anchoredPosition=tunnelHome+new Vector2(-Mathf.Sin(idleClock*.31f)*5,Mathf.Sin(idleClock*.37f)*3)*ease;
                }
                SetGlints(true);
                if(idleGlints!=null)for(int i=0;i<idleGlints.Length;i++)
                {
                    float distance=Mathf.Repeat(idleClock*95+i*1040,3120);Vector2 p;float rotation;
                    if(distance<1400){p=new Vector2(260+distance,840);rotation=0;}
                    else if(distance<1560){p=new Vector2(1660,840+distance-1400);rotation=90;}
                    else if(distance<2960){p=new Vector2(1660-(distance-1560),1000);rotation=0;}
                    else{p=new Vector2(260,1000-(distance-2960));rotation=90;}
                    var glint=idleGlints[i];glint.anchoredPosition=new Vector2(p.x-960,540-p.y);glint.localRotation=Quaternion.Euler(0,0,rotation);
                    var image=glint.GetComponent<Image>();image.color=new Color(.45f,.95f,1,(.35f+.25f*Mathf.Sin(idleClock*1.8f+i))*ease);
                }
            }
            else if(wasIdle){Restore();SetGlints(false);}
            wasIdle=idle;
            if(quiz.feedback.gameObject.activeInHierarchy&&feedbackPanel!=null)feedbackPanel.localScale=Vector3.one*(1+Mathf.Sin(clock*4)*.003f);
        }
        void SetGlints(bool show){if(idleGlints!=null)foreach(var glint in idleGlints)if(glint!=null&&glint.gameObject.activeSelf!=show)glint.gameObject.SetActive(show);}
        void Restore(){if(titleArtwork!=null){titleArtwork.anchoredPosition=titleHome;titleArtwork.localScale=titleScale;titleArtwork.localRotation=titleRotation;}if(tunnel!=null){tunnel.anchoredPosition=tunnelHome;tunnel.localScale=tunnelScale;}}
    }
}
