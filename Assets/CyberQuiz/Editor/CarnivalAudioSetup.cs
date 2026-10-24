using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CyberQuiz.Editor
{
    [InitializeOnLoad] public static class CarnivalAudioSetup
    {
        static CarnivalAudioSetup(){EditorApplication.delayCall+=Apply;}
        [MenuItem("Cyber Quiz/Apply Audio")]
        public static void Apply()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            var q=Object.FindAnyObjectByType<CarnivalQuiz>();if(q==null)return;
            Import("CarnivalAmbient.mp3",true);
            Import("SfxCorrect.mp3",false);
            Import("SfxWrong.mp3",false);
            Import("SfxClick.mp3",false);
            if(Object.FindAnyObjectByType<AudioListener>()==null)
            {
                var cam=q.displays!=null&&q.displays.camera1!=null?q.displays.camera1:Object.FindAnyObjectByType<Camera>();
                if(cam!=null)cam.gameObject.AddComponent<AudioListener>();
            }
            var audio=q.GetComponent<CarnivalAudio>();if(audio==null)audio=q.gameObject.AddComponent<CarnivalAudio>();
            var sources=q.GetComponents<AudioSource>();
            if(sources.Length==0)audio.music=q.gameObject.AddComponent<AudioSource>();
            else audio.music=sources[0];
            if(q.GetComponents<AudioSource>().Length<2)audio.sfx=q.gameObject.AddComponent<AudioSource>();
            else audio.sfx=q.GetComponents<AudioSource>()[1];
            audio.ambient=Load("CarnivalAmbient.mp3");
            audio.correct=Load("SfxCorrect.mp3");
            audio.wrong=Load("SfxWrong.mp3");
            audio.click=Load("SfxClick.mp3");
            audio.key=audio.click;
            audio.whoosh=Load("SfxWhoosh.wav");audio.nfc=Load("SfxNfc.wav");
            audio.invalid=Load("SfxInvalid.wav");audio.register=Load("SfxRegister.wav");audio.complete=Load("SfxComplete.wav");
            audio.music.clip=audio.ambient;audio.music.loop=true;audio.music.playOnAwake=true;audio.music.volume=audio.musicVolume;
            audio.sfx.playOnAwake=false;audio.sfx.loop=false;audio.sfx.volume=audio.sfxVolume;
            EditorUtility.SetDirty(audio);EditorUtility.SetDirty(q);
            EditorSceneManager.MarkSceneDirty(q.gameObject.scene);
            EditorSceneManager.SaveScene(q.gameObject.scene);
        }
        static void Import(string file,bool stream)
        {
            var path="Assets/CyberQuiz/Audio/"+file;
            var importer=AssetImporter.GetAtPath(path) as AudioImporter;if(importer==null)return;
            importer.forceToMono=false;
            var s=importer.defaultSampleSettings;
            s.loadType=stream?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat=AudioCompressionFormat.Vorbis;s.quality=.7f;
            importer.defaultSampleSettings=s;
            importer.SaveAndReimport();
        }
        static AudioClip Load(string file)=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/CyberQuiz/Audio/"+file);
    }
}
