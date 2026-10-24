using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CyberQuiz.Editor
{
    [InitializeOnLoad] public static class CarnivalContrastSetup
    {
        static CarnivalContrastSetup(){EditorApplication.update+=Check;}
        static void Check()
        {
            if(!File.Exists("Temp/CarnivalContrast.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            File.Delete("Temp/CarnivalContrast.request");
            var q=Object.FindAnyObjectByType<CarnivalQuiz>();if(q==null||q.displays==null)return;
            var d=q.displays;
            foreach(var plate in Object.FindObjectsByType<CarnivalPlate>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(plate.GetComponent<CanvasRenderer>()==null)plate.gameObject.AddComponent<CanvasRenderer>();
                plate.SetAllDirty();
            }
            foreach(var root in new[]{d.display1.transform,d.display2.transform})
            {
                foreach(var image in root.GetComponentsInChildren<Image>(true))
                {
                    if(image.sprite==null)continue;
                    bool field=image.sprite.name.StartsWith("Registration place holder");
                    bool panel=image.sprite.name.StartsWith("FigmaQuestionPanel");
                    if(!field&&!panel)continue;
                    if(image.transform.Find("Readable navy interior")!=null)continue;
                    var go=new GameObject("Readable navy interior",typeof(RectTransform),typeof(CarnivalPlate));go.transform.SetParent(image.transform,false);go.transform.SetAsFirstSibling();go.layer=image.gameObject.layer;
                    var r=go.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
                    var plate=go.GetComponent<CarnivalPlate>();plate.registrationCut=field;plate.color=new Color(.002f,.012f,.065f,field?.88f:.74f);plate.raycastTarget=false;
                }
            }
            foreach(var group in new[]{d.registration,d.thankYou})
            {
                var atmosphere=group.transform.Find("Navy atmosphere").GetComponent<Image>();atmosphere.color=new Color(0,.015f,.065f,.54f);
            }
            AddTable(q.leaderboard.transform);
            foreach(var mirror in d.mirrors)if(mirror.source==q.leaderboard)AddTable(mirror.view.transform);
            foreach(var mirror in d.mirrors)
            {
                mirror.sourceImages=mirror.source.GetComponentsInChildren<Image>(true);
                mirror.viewImages=mirror.view.GetComponentsInChildren<Image>(true);
            }
            float[] left={395,550,995,1295};float[] width={155,445,300,220};
            for(int i=0;i<q.rows.Length;i++)
            {
                var row=q.rows[i];var cells=new[]{row.rank,row.player,row.correct,row.time};
                for(int c=0;c<cells.Length;c++)
                {
                    Place(cells[c].rectTransform,left[c],518+i*64,width[c],64);cells[c].fontSize=32;
                    foreach(var mirror in d.mirrors)if(mirror.source==q.leaderboard)
                    {
                        int index=System.Array.IndexOf(mirror.sourceTexts,cells[c]);
                        if(index>=0){Place(mirror.viewTexts[index].rectTransform,left[c],518+i*64,width[c],64);mirror.viewTexts[index].fontSize=32;}
                    }
                }
            }
            // Keep the idle title bright; its small caption uses the same original neon frame.
            EditorSceneManager.MarkSceneDirty(q.gameObject.scene);EditorSceneManager.SaveScene(q.gameObject.scene);
            File.WriteAllText("Temp/CarnivalContrast.txt","PASS: registration and quiz contrast refined; leaderboard table styled");
        }
        static void AddTable(Transform root)
        {
            if(root.Find("Table header wash")!=null)return;
            var header=Box("Table header wash",root,395,440,1120,78,new Color(0,.38f,.55f,.55f));header.SetSiblingIndex(1);
            float[] cols={550,995,1295};
            foreach(float x in cols)Box("Table column rule",root,x,440,1.5f,398,new Color(.5f,.8f,1,.22f));
            for(int row=0;row<=5;row++)Box("Table row rule",root,395,518+row*64,1120,1.5f,new Color(.5f,.8f,1,.22f));
            // Match the Figma's balanced four-column table proportions.
            var texts=root.GetComponentsInChildren<Text>(true);
            foreach(var t in texts)
            {
                if(t.text=="RANK")Place(t.rectTransform,395,440,155,78);
                else if(t.text=="PLAYER")Place(t.rectTransform,550,440,445,78);
                else if(t.text=="Correct")Place(t.rectTransform,995,440,300,78);
                else if(t.text=="TIME")Place(t.rectTransform,1295,440,220,78);
            }
        }
        static RectTransform Box(string name,Transform parent,float x,float y,float w,float h,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);go.layer=parent.gameObject.layer;var r=go.GetComponent<RectTransform>();Place(r,x,y,w,h);var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=false;return r;
        }
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(x+w/2,-y-h/2);}
    }
}
