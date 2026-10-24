using UnityEngine;
using UnityEngine.InputSystem;

namespace CyberQuiz
{
    public class CarnivalAudio : MonoBehaviour
    {
        public static CarnivalAudio Instance { get; private set; }
        public AudioSource music, sfx;
        public AudioClip ambient, key, click, whoosh, nfc, invalid, correct, wrong, register, complete;
        [Range(0,1)] public float musicVolume=.28f;
        [Range(0,1)] public float sfxVolume=.7f;
        float clickLock;

        void Awake()
        {
            Instance=this;
            if(music==null)music=gameObject.AddComponent<AudioSource>();
            if(sfx==null)
            {
                var sources=GetComponents<AudioSource>();
                sfx=sources.Length>1?sources[1]:gameObject.AddComponent<AudioSource>();
            }
            music.loop=true;music.playOnAwake=true;music.spatialBlend=0;music.volume=musicVolume;
            sfx.loop=false;sfx.playOnAwake=false;sfx.spatialBlend=0;sfx.volume=sfxVolume;
            if(ambient!=null){music.clip=ambient;if(!music.isPlaying)music.Play();}
        }
        void Update()
        {
            clickLock-=Time.unscaledDeltaTime;
            bool down=false;
            if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)down=true;
            if(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.wasPressedThisFrame)down=true;
            if(down)Click();
        }
        void OnDestroy(){if(Instance==this)Instance=null;}
        public void Play(AudioClip clip,float volume=1)
        {
            if(clip==null||sfx==null)return;
            sfx.PlayOneShot(clip,sfxVolume*volume);
        }
        public static void Click()
        {
            if(Instance==null||Instance.clickLock>0)return;
            Instance.clickLock=.05f;
            Instance.Play(Instance.click!=null?Instance.click:Instance.key,.85f);
        }
        public static void Key(){Click();}
        public static void Whoosh(){if(Instance!=null)Instance.Play(Instance.whoosh,.5f);}
        public static void Nfc(){if(Instance!=null)Instance.Play(Instance.nfc,.8f);}
        public static void Invalid(){if(Instance!=null)Instance.Play(Instance.invalid,.75f);}
        public static void Correct(){if(Instance!=null)Instance.Play(Instance.correct);}
        public static void Wrong(){if(Instance!=null)Instance.Play(Instance.wrong);}
        public static void Register(){if(Instance!=null)Instance.Play(Instance.register);}
        public static void Complete(){if(Instance!=null)Instance.Play(Instance.complete);}
    }
}
