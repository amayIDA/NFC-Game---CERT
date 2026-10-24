using UnityEngine;
using UnityEngine.UI;
namespace CyberQuiz
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class CarnivalRing : MaskableGraphic
    {
        public float thickness=4;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float radius=Mathf.Min(r.width,r.height)*.5f;
            for(int i=0;i<=80;i++)
            {
                float a=i*Mathf.PI*2/80;var direction=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                vh.AddVert(r.center+direction*radius,color,Vector2.zero);
                vh.AddVert(r.center+direction*Mathf.Max(0,radius-thickness),color,Vector2.zero);
                if(i>0){int n=i*2;vh.AddTriangle(n-2,n,n-1);vh.AddTriangle(n-1,n,n+1);}
            }
        }
    }
}
