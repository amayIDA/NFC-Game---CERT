using UnityEngine;
using UnityEngine.UI;

namespace CyberQuiz
{
    // Opaque interior beneath the exported translucent neon trim.
    [RequireComponent(typeof(CanvasRenderer))]
    public class CarnivalPlate : MaskableGraphic
    {
        public bool registrationCut;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rect=rectTransform.rect;
            var points=new System.Collections.Generic.List<Vector2>();
            if(registrationCut)
            {
                var uv=new[]{new Vector2(.013f,.075f),new Vector2(.954f,.075f),new Vector2(.987f,.205f),new Vector2(.987f,.835f),new Vector2(.192f,.835f),new Vector2(.168f,.925f),new Vector2(.028f,.925f),new Vector2(.013f,.835f)};
                foreach(var p in uv)points.Add(new Vector2(rect.xMin+p.x*rect.width,rect.yMax-p.y*rect.height));
            }
            else
            {
                float radius=Mathf.Min(38,rect.height/4);float inset=24;
                var r=new Rect(rect.xMin+inset,rect.yMin+inset,rect.width-2*inset,rect.height-2*inset);
                var centers=new[]{new Vector2(r.xMax-radius,r.yMax-radius),new Vector2(r.xMin+radius,r.yMax-radius),new Vector2(r.xMin+radius,r.yMin+radius),new Vector2(r.xMax-radius,r.yMin+radius)};
                for(int corner=0;corner<4;corner++)for(int i=0;i<=8;i++)
                {
                    float a=(corner*90+i*90f/8)*Mathf.Deg2Rad;
                    points.Add(centers[corner]+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);
                }
            }
            foreach(var p in points)vh.AddVert(p,color,Vector2.zero);
            // Ear clipping also covers the concave notch at the lower left of the input artwork.
            float area=0;
            for(int i=0;i<points.Count;i++){var a=points[i];var b=points[(i+1)%points.Count];area+=a.x*b.y-b.x*a.y;}
            float sign=area>=0?1:-1;
            var indices=new System.Collections.Generic.List<int>();for(int i=0;i<points.Count;i++)indices.Add(i);
            for(int guard=0;indices.Count>2&&guard<points.Count*points.Count;guard++)
            {
                bool clipped=false;
                for(int i=0;i<indices.Count;i++)
                {
                    int a=indices[(i+indices.Count-1)%indices.Count],b=indices[i],c=indices[(i+1)%indices.Count];
                    if(Cross(points[b]-points[a],points[c]-points[b])*sign<=.001f)continue;
                    bool contains=false;
                    foreach(int k in indices)
                    {
                        if(k==a||k==b||k==c)continue;
                        var p=points[k];
                        if(Cross(points[b]-points[a],p-points[a])*sign>=0&&Cross(points[c]-points[b],p-points[b])*sign>=0&&Cross(points[a]-points[c],p-points[c])*sign>=0){contains=true;break;}
                    }
                    if(contains)continue;
                    vh.AddTriangle(a,b,c);indices.RemoveAt(i);clipped=true;break;
                }
                if(!clipped)break;
            }
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
    }
}
