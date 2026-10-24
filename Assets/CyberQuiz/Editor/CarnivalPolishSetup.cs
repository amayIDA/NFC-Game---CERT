using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CyberQuiz.Editor
{
    [InitializeOnLoad]static class CarnivalPolishSetup
    {
        static CarnivalPolishSetup(){EditorApplication.delayCall+=Apply;EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode)EditorApplication.delayCall+=Apply;};}
        static void Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            var q=Object.FindAnyObjectByType<CarnivalQuiz>();if(q==null||q.gameObject.scene.isDirty)return;
            if(q.transform.Find("Figma navy atmosphere")==null)
            {
                var go=new GameObject("Figma navy atmosphere",typeof(RectTransform),typeof(UnityEngine.UI.Image));go.transform.SetParent(q.transform,false);go.transform.SetSiblingIndex(1);
                var rect=go.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                var image=go.GetComponent<UnityEngine.UI.Image>();image.color=new Color(0,.015f,.065f,.36f);image.raycastTarget=false;
                EditorSceneManager.MarkSceneDirty(q.gameObject.scene);EditorSceneManager.SaveScene(q.gameObject.scene);
            }
            if(q.GetComponent<CarnivalAmbient>()!=null)return;
            var ambient=q.gameObject.AddComponent<CarnivalAmbient>();ambient.quiz=q;
            ambient.titleArtwork=q.splash.transform.Find("Cyber Carnival artwork") as RectTransform;ambient.tunnel=q.splash.transform.Find("Tunnel") as RectTransform;ambient.feedbackPanel=q.feedback.transform.Find("Figma Feedback Panel") as RectTransform;
            EditorSceneManager.MarkSceneDirty(q.gameObject.scene);EditorSceneManager.SaveScene(q.gameObject.scene);
        }
    }
}

