using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CyberQuiz.Editor
{
    [InitializeOnLoad]
    public static class QuizSceneSetup
    {
        const string ScenePath="Assets/CyberQuiz/CyberSecurityQuiz.unity";
        static QuizSceneSetup() { EditorApplication.delayCall+=EnsureScene; }
        static void EnsureScene()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || File.Exists(ScenePath)) return;
            CreateScene();
        }
        [MenuItem("Cyber Quiz/Create Quiz Scene")]
        public static void CreateScene()
        {
            if(File.Exists(ScenePath)) { EditorUtility.DisplayDialog("Cyber Quiz", "The quiz scene already exists at "+ScenePath, "OK"); return; }
            var previous=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            new GameObject("NFC Cybersecurity Quiz").AddComponent<NfcCyberQuiz>();
            EditorSceneManager.SaveScene(scene,ScenePath);
            var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if(!scenes.Exists(s=>s.path==ScenePath)) scenes.Insert(0,new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=scenes.ToArray();
            bool safe=true;
            for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i)!=scene && SceneManager.GetSceneAt(i).isDirty) safe=false;
            if(safe)
            {
                for(int i=SceneManager.sceneCount-1;i>=0;i--) { var other=SceneManager.GetSceneAt(i); if(other!=scene) EditorSceneManager.CloseScene(other,true); }
            }
            else { EditorSceneManager.CloseScene(scene,true); SceneManager.SetActiveScene(previous); }
            File.WriteAllText("Assets/CyberQuiz/SetupStatus.txt","Quiz scene created successfully. Open "+ScenePath+" and press Play. Keyboard NFC reader input uses the Unity Input System.");
            AssetDatabase.Refresh();
            Debug.Log("Cyber Quiz: scene created at "+ScenePath);
        }
    }
}
