using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CyberQuiz.Editor
{
    [InitializeOnLoad] public static class VideoBackgroundSetup
    {
        static VideoBackgroundSetup(){EditorApplication.delayCall+=Apply;}
        [MenuItem("Cyber Quiz/Apply Video Background")]
        public static void Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            var q=Object.FindAnyObjectByType<CarnivalQuiz>();if(q==null||q.displays==null)return;
            var idle=AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/CyberQuiz/Art/BG_Idle.mp4");
            var quiz=AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/CyberQuiz/Art/CYBER_CARNIVAL_Background_4K_BG.mp4");
            if(idle==null&&quiz==null)return;
            var d=q.displays;
            HideLogos();
            SwapCopy(q);
            var bg=q.GetComponent<CarnivalVideoBackground>();
            if(bg==null)bg=q.gameObject.AddComponent<CarnivalVideoBackground>();
            bg.idleClip=idle;bg.quizClip=quiz;bg.displays=d;
            bg.screens=new[]{EnsureScreen(d.display2.transform,0),EnsureScreen(d.display1.transform,0)};
            foreach(var canvas in new[]{d.display1,d.display2})
                foreach(var image in canvas.GetComponentsInChildren<Image>(true))
                {
                    var n=image.gameObject.name;
                    var sprite=image.sprite!=null?image.sprite.name:"";
                    if(n=="Figma Background"||n=="Background"||n=="Tunnel"||n=="Navy atmosphere"||sprite=="FigmaAtmosphere"||sprite=="Tunnel"||sprite.StartsWith("FigmaBackground"))
                        image.enabled=false;
                }
            EditorUtility.SetDirty(q);EditorUtility.SetDirty(bg);
            EditorSceneManager.MarkSceneDirty(q.gameObject.scene);
            EditorSceneManager.SaveScene(q.gameObject.scene);
        }
        static RawImage EnsureScreen(Transform canvas,int sibling)
        {
            var existing=canvas.Find("Video background");
            RectTransform rect;
            RawImage raw;
            if(existing==null)
            {
                var go=new GameObject("Video background",typeof(RectTransform),typeof(RawImage));
                go.transform.SetParent(canvas,false);
                rect=go.GetComponent<RectTransform>();
                raw=go.GetComponent<RawImage>();
            }
            else
            {
                rect=existing.GetComponent<RectTransform>();
                raw=existing.GetComponent<RawImage>();
                if(raw==null)raw=existing.gameObject.AddComponent<RawImage>();
            }
            rect.SetSiblingIndex(sibling);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            raw.color=Color.white;raw.raycastTarget=false;
            rect.gameObject.layer=canvas.gameObject.layer;
            return raw;
        }
        static void HideLogos()
        {
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                var n=t.name;
                if(n.IndexOf("Department",System.StringComparison.OrdinalIgnoreCase)>=0||n.IndexOf("FigmaDepartment",System.StringComparison.OrdinalIgnoreCase)>=0||n=="image 1"||n.IndexOf("Department Logo",System.StringComparison.OrdinalIgnoreCase)>=0)
                    t.gameObject.SetActive(false);
            }
        }
        static void SwapCopy(CarnivalQuiz q)
        {
            foreach(var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(string.IsNullOrEmpty(text.text))continue;
                var value=text.text;
                value=ReplaceWord(value,"PLAYER","AGENT");
                value=ReplaceWord(value,"Player","Agent");
                value=ReplaceWord(value,"player","agent");
                if(value!=text.text)text.text=value;
            }
        }
        static string ReplaceWord(string value,string from,string to)
        {
            return value.Replace(from,to);
        }
    }
}
