using UnityEngine;
using UnityEngine.UI;

namespace CyberQuiz
{
    public class CarnivalMotion : MonoBehaviour
    {
        public CarnivalQuiz quiz;
        public RectTransform[] sparks;
        public CarnivalRing[] rings;
        public Text[] rewardLabels;
        public CanvasGroup[] effectRoots;
        public RectTransform feedbackTitle,feedbackBody;
        float clock=-10,lockClock=-10,resultClock=-10;
        Vector2 lockOrigin;
        Color signal;
        bool success;
        int streak,finalScore,total;
        double seconds;
        Vector2 titleHome,bodyHome;
        void Awake(){titleHome=feedbackTitle.anchoredPosition;bodyHome=feedbackBody.anchoredPosition;Clear();}
        public void LockAnswer(RectTransform selected,bool right)
        {
            if(quiz.reducedMotion)return;
            lockClock=0;
            lockOrigin=selected.anchoredPosition-new Vector2(960,-540);
            signal=right?new Color(.12f,1,.72f):new Color(1,0,0);
        }
        public void Feedback(bool right)
        {
            success=right;streak=right?streak+1:0;clock=0;
            signal=right?new Color(.1f,1,.67f):new Color(1,0,0);
            foreach(var label in rewardLabels)label.text=right?(streak>1?"+1  ·  "+streak+" IN A ROW":"+1  ·  NICE WORK!"):"NEXT QUESTION · YOU'VE GOT THIS";
        }
        public void Results(int score,int count,double time){finalScore=score;total=count;seconds=time;resultClock=0;}
        public void Clear()
        {
            clock=lockClock=resultClock=-10;streak=0;
            foreach(var root in effectRoots)root.alpha=0;
            foreach(var spark in sparks)spark.gameObject.SetActive(false);
            foreach(var ring in rings)ring.gameObject.SetActive(false);
            foreach(var text in rewardLabels)text.gameObject.SetActive(false);
        }
        void Update()
        {
            if(quiz.reducedMotion){foreach(var root in effectRoots)root.alpha=0;feedbackTitle.localScale=Vector3.one;feedbackTitle.anchoredPosition=titleHome;feedbackBody.anchoredPosition=bodyHome;quiz.hudCorrect.transform.localScale=Vector3.one;quiz.resultBody.transform.localScale=Vector3.one;return;}
            float dt=Time.unscaledDeltaTime;
            bool feedback=clock>=0&&clock<1.45f&&quiz.feedback.gameObject.activeSelf;
            bool locking=lockClock>=0&&lockClock<.35f;
            foreach(var root in effectRoots)root.alpha=feedback||locking?1:0;
            if(locking)
            {
                lockClock+=dt;float u=Mathf.Clamp01(lockClock/.35f);
                for(int i=0;i<rings.Length;i++)
                {
                    var ring=rings[i];ring.gameObject.SetActive(i%3==0);if(i%3!=0)continue;
                    ring.rectTransform.anchoredPosition=lockOrigin;
                    ring.rectTransform.localScale=Vector3.one*Mathf.Lerp(.08f,.5f,u);
                    var color=signal;color.a=(1-u)*.7f;ring.color=color;
                }
            }
            else if(feedback)
            {
                clock+=dt;float u=Mathf.Clamp01(clock/1.45f);
                for(int i=0;i<rings.Length;i++)
                {
                    float delay=(i%3)*.1f,v=Mathf.Clamp01((clock-delay)/.7f);
                    var ring=rings[i];ring.gameObject.SetActive(v<1);ring.rectTransform.anchoredPosition=new Vector2(0,40);
                    ring.rectTransform.localScale=Vector3.one*Mathf.Lerp(.06f,1.5f,1-Mathf.Pow(1-v,2));
                    var color=signal;color.a=(1-v)*.38f;ring.color=color;
                }
                for(int i=0;i<sparks.Length;i++)
                {
                    var spark=sparks[i];spark.gameObject.SetActive(success);
                    if(!success)continue;int n=i%48;
                    float side=n%2==0?-1:1,angle=(35+(n*37%100))*Mathf.Deg2Rad;
                    float speed=480+n%7*46;
                    spark.anchoredPosition=new Vector2(side*650+Mathf.Cos(angle)*speed*clock*-side,-260+Mathf.Sin(angle)*speed*clock-380*clock*clock);
                    spark.localRotation=Quaternion.Euler(0,0,n*29+clock*(n%2==0?270:-190));
                    spark.localScale=Vector3.one*Mathf.Lerp(1.3f,0,Mathf.Pow(u,2));
                }
                float punch=Mathf.Sin(Mathf.Min(clock/.45f,1)*Mathf.PI)*Mathf.Exp(-clock*3);
                feedbackTitle.localScale=Vector3.one*(1+punch*(success?.17f:.08f));
                feedbackTitle.anchoredPosition=titleHome+new Vector2(success?0:Mathf.Sin(clock*70)*12*Mathf.Exp(-clock*9),0);
                feedbackBody.anchoredPosition=bodyHome+new Vector2(0,-16*Mathf.Exp(-clock*8));
                quiz.hudCorrect.transform.localScale=Vector3.one*(1+(success?punch*.16f:0));
                foreach(var label in rewardLabels)
                {
                    label.gameObject.SetActive(true);var color=success?new Color(.5f,1,.8f):new Color(.9f,.94f,1);
                    color.a=Mathf.Clamp01(clock*6)*Mathf.Clamp01((1.45f-clock)*3);label.color=color;
                    label.rectTransform.anchoredPosition=new Vector2(0,-205+30*u);
                    label.transform.localScale=Vector3.one*(1+.05f*Mathf.Sin(u*Mathf.PI));
                }
            }
            else
            {
                foreach(var spark in sparks)spark.gameObject.SetActive(false);
                foreach(var ring in rings)ring.gameObject.SetActive(false);
                foreach(var label in rewardLabels)label.gameObject.SetActive(false);
                feedbackTitle.localScale=Vector3.one;feedbackTitle.anchoredPosition=titleHome;feedbackBody.anchoredPosition=bodyHome;quiz.hudCorrect.transform.localScale=Vector3.one;
            }
            if(resultClock>=0&&resultClock<.8f&&quiz.results.gameObject.activeSelf)
            {
                resultClock+=dt;float u=Mathf.Clamp01(resultClock/.8f),e=1-Mathf.Pow(1-u,3);
                quiz.resultBody.text="Thank you!\nYou got "+Mathf.RoundToInt(finalScore*e)+" out of "+total+" correct.\nTime : "+seconds.ToString("0.0")+" sec";
                quiz.resultBody.transform.localScale=Vector3.one*(1+.04f*Mathf.Sin(u*Mathf.PI));
            }
        }
    }
}
