using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CyberQuiz
{
    public class CarnivalVideoBackground : MonoBehaviour
    {
        public VideoClip idleClip, quizClip;
        public RawImage[] screens;
        public CarnivalDisplays displays;
        VideoPlayer player;
        RenderTexture texture;
        VideoClip current;

        void Awake()
        {
            if(displays==null)displays=GetComponent<CarnivalDisplays>();
            var start=idleClip!=null?idleClip:quizClip;
            if(start==null||screens==null||screens.Length==0)return;
            texture=new RenderTexture(1920,1080,0);
            texture.name="Scene background";
            player=gameObject.GetComponent<VideoPlayer>();
            if(player==null)player=gameObject.AddComponent<VideoPlayer>();
            player.playOnAwake=false;
            player.isLooping=true;
            player.skipOnDrop=true;
            player.renderMode=VideoRenderMode.RenderTexture;
            player.audioOutputMode=VideoAudioOutputMode.None;
            player.source=VideoSource.VideoClip;
            player.targetTexture=texture;
            foreach(var screen in screens)
            {
                if(screen==null)continue;
                screen.texture=texture;
                screen.color=Color.white;
                screen.raycastTarget=false;
            }
            Play(start);
        }
        void LateUpdate()
        {
            bool quiz=displays!=null&&displays.Registered;
            Play(quiz&&quizClip!=null?quizClip:idleClip);
        }
        void Play(VideoClip clip)
        {
            if(clip==null||player==null||clip==current)return;
            current=clip;
            player.clip=clip;
            player.Play();
        }
        void OnDestroy()
        {
            if(player!=null)player.targetTexture=null;
            if(texture!=null){texture.Release();Destroy(texture);}
        }
    }
}
