using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace CyberQuiz.Editor
{
    [InitializeOnLoad] public static class CarnivalCompletionSetup
    {
        static CarnivalCompletionSetup(){EditorApplication.update+=Check;}
        static void Check()
        {
            if(!File.Exists("Temp/CarnivalCompletion.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            File.Delete("Temp/CarnivalCompletion.request");
            var quiz=Object.FindAnyObjectByType<CarnivalQuiz>();if(quiz==null||quiz.displays==null)return;
            quiz.leaderboardSeconds=10;
            var ambient=quiz.GetComponent<CarnivalAmbient>();ambient.floatHeight=10;ambient.titleScaleAmount=.009f;
            if(ambient.idleGlints==null||ambient.idleGlints.Length==0)
            {
                ambient.idleGlints=new RectTransform[3];
                for(int i=0;i<3;i++)
                {
                    var go=new GameObject("Idle border light "+(i+1),typeof(RectTransform),typeof(Image));go.transform.SetParent(quiz.displays.idle.transform,false);go.layer=quiz.displays.display1.gameObject.layer;
                    var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(28,3);
                    var image=go.GetComponent<Image>();image.raycastTarget=false;image.color=new Color(.45f,.95f,1,.5f);ambient.idleGlints[i]=rect;go.SetActive(false);
                }
            }
            EditorUtility.SetDirty(quiz);EditorUtility.SetDirty(ambient);EditorSceneManager.MarkSceneDirty(quiz.gameObject.scene);EditorSceneManager.SaveScene(quiz.gameObject.scene);
            File.WriteAllText("Temp/CarnivalCompletionSetup.txt","PASS: automatic completed-game leaderboard; 10-second idle delay; authored idle glints and floating foreground/tunnel motion");
        }
    }
}
