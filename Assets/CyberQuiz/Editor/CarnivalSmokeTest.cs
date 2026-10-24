using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CyberQuiz.Editor
{
    [InitializeOnLoad] public static class CarnivalSmokeTest
    {
        static int stage,qIndex,before;
        static double next,deadline;
        static CarnivalQuiz quiz;
        static Keyboard device;
        static Vector2 idleHome;
        const string Request="Temp/CarnivalSmoke.request";
        const string Report="Temp/CarnivalSmokeReport.txt";
        static CarnivalSmokeTest(){EditorApplication.update+=Check;}
        static void Check(){if(!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&File.Exists(Request)){File.Delete(Request);Run();}}
        [MenuItem("Cyber Quiz/Run Carnival Verification")]
        static void Run(){stage=-3;qIndex=0;deadline=EditorApplication.timeSinceStartup+90;next=0;EditorApplication.update-=Tick;EditorApplication.update+=Tick;if(!EditorApplication.isPlaying)EditorApplication.isPlaying=true;}
        static void Assert(bool value,string message){if(!value)throw new Exception(message);}
        static void Send(string value,bool enter=true){foreach(char c in value)InputSystem.QueueTextEvent(device,c);if(enter)InputSystem.QueueTextEvent(device,'\n');}
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup<next)return;
            try
            {
                Assert(EditorApplication.timeSinceStartup<deadline,"Test timed out");
                if(!EditorApplication.isPlaying)return;
                if(stage==-3)
                {
                    quiz=UnityEngine.Object.FindAnyObjectByType<CarnivalQuiz>();if(quiz==null||quiz.displays==null)return;
                    var d=quiz.displays;
                    Assert(d.camera1.targetDisplay==0&&d.camera2.targetDisplay==1,"Camera/display assignment incorrect");
                    Assert(d.display1.worldCamera==d.camera1&&d.display2.worldCamera==d.camera2,"Canvas camera binding incorrect");
                    Assert((d.camera1.cullingMask&d.camera2.cullingMask)==0,"Display culling masks overlap");
                    Assert(d.idle.gameObject.activeSelf&&quiz.leaderboard.gameObject.activeSelf,"Dual idle states missing");
                    Assert(d.display1.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution==new Vector2(1920,1080),"Display 1 design resolution incorrect");
                    quiz.RecordScores=false;
                    Capture(d.camera1,"Dual-01-idle-bottom");Capture(d.camera2,"Dual-01-idle-top");
                    d.idle.GetComponentInChildren<UnityEngine.UI.Button>(true).onClick.Invoke();
                    stage=-2;next=EditorApplication.timeSinceStartup+.6;
                }
                else if(stage==-2)
                {
                    var d=quiz.displays;
                    Assert(d.registration.gameObject.activeSelf&&!d.idle.gameObject.activeSelf,"Idle click did not open registration");
                    Assert(quiz.leaderboard.gameObject.activeSelf&&!quiz.instructions.gameObject.activeSelf,"Registration click changed top screen");
                    Capture(d.camera1,"Dual-02-registration-bottom");Capture(d.camera2,"Dual-02-registration-top");
                    d.SubmitRegistration();Assert(!d.Registered&&d.validation.text.Length>0,"Blank registration accepted");
                    d.nameField.text="Verification Player";d.employeeIdField.text="test@example.com";
                    d.SubmitRegistration();stage=-1;next=EditorApplication.timeSinceStartup+2.2;
                }
                else if(stage==-1)
                {
                    var d=quiz.displays;
                    Assert(d.Registered&&d.EmailId=="test@example.com","Registration state incorrect");
                    Assert(quiz.instructions.gameObject.activeSelf,"Instructions did not follow submission");
                    Assert(d.mirrors[0].view.gameObject.activeSelf,"Touchscreen instructions not visible");
                    Capture(d.camera1,"Dual-03-instructions-bottom");Capture(d.camera2,"Dual-03-instructions-top");
                    stage=0;
                }
                else if(stage==0)
                {
                    quiz=UnityEngine.Object.FindAnyObjectByType<CarnivalQuiz>();if(quiz==null)return;
                    before=System.Array.FindAll(quiz.GetComponentsInChildren<Transform>(true), t=>!t.name.EndsWith(" Input Caret")).Length;
                    Assert(quiz.questions.Length>=1,"Question panels required");
                    foreach(var q in quiz.questions){Assert(q.options.Length==4,"Four classification options required");var ids=new System.Collections.Generic.HashSet<string>();foreach(var o in q.options){Assert(o!=null&&o.button!=null,"Missing option reference");Assert(ids.Add(o.nfcValue),"Duplicate NFC value");}}
                    Assert(UnityEngine.Object.FindAnyObjectByType<NfcCyberQuiz>()==null,"Old spawning component present");
                    Assert(CarnivalQuiz.CompareEntries(new CarnivalQuiz.Entry{correct=5,seconds=100},new CarnivalQuiz.Entry{correct=4,seconds=1})<0,"Correct-count ranking failed");
                    Assert(CarnivalQuiz.CompareEntries(new CarnivalQuiz.Entry{correct=5,seconds=5},new CarnivalQuiz.Entry{correct=5,seconds=6})<0,"Time tie-break failed");
                    quiz.RecordScores=true;quiz.reducedMotion=true;quiz.feedbackSeconds=.06f;quiz.testScorePath=Path.Combine(Application.temporaryCachePath,"carnival-smoke-"+Guid.NewGuid()+".json");quiz.testCsvPath=Path.Combine(Application.temporaryCachePath,"carnival-smoke-"+Guid.NewGuid()+".csv");quiz.playerName.text="Verification Player";
                    device=InputSystem.AddDevice<Keyboard>();device.MakeCurrent();quiz.StartQuiz();stage=1;next=EditorApplication.timeSinceStartup+.15;
                }
                else if(stage==1){Capture(quiz.displays.camera1,"Dual-04-question-bottom");Capture(quiz.displays.camera2,"Dual-04-question-top");Send("9999999999",false);stage=2;next=EditorApplication.timeSinceStartup+.6;}
                else if(stage==2)
                {
                    Assert(quiz.readerStatus.text.Contains("not recognised"),"Unknown tag not rejected");Assert(quiz.CorrectCount==0,"Unknown tag scored");
                    Send(quiz.ActiveQuestion.options[quiz.ActiveQuestion.correctOption].nfcValue);Send(quiz.ActiveQuestion.options[quiz.ActiveQuestion.correctOption].nfcValue);qIndex=1;stage=3;next=EditorApplication.timeSinceStartup+.3;
                }
                else if(stage==3)
                {
                    Assert(quiz.CorrectCount==1,"Keyboard scan or duplicate guard failed");
                    Send(quiz.ActiveQuestion.options[(quiz.ActiveQuestion.correctOption+1)%4].nfcValue);stage=4;next=EditorApplication.timeSinceStartup+.3;
                }
                else if(stage==4)
                {
                    Assert(quiz.CorrectCount==1,"Wrong answer scored");
                    stage=5;
                }
                else if(stage==5)
                {
                    if(quiz.leaderboard.gameObject.activeSelf){stage=7;return;}
                    if(!quiz.IsBusy && quiz.VisiblePanel==quiz.ActiveQuestion.panel)
                        Send(quiz.ActiveQuestion.options[quiz.ActiveQuestion.correctOption].nfcValue);
                    next=EditorApplication.timeSinceStartup+.2;
                }
                else if(stage==7)
                {
                    Assert(quiz.CorrectCount==ClassificationBank.RoundSize-1,"Expected 9/10 final score");Assert(quiz.leaderboard.gameObject.activeSelf&&!quiz.results.gameObject.activeSelf,"Completion did not automatically open leaderboard");Assert(File.Exists(quiz.ScorePath),"Score file not saved");
                    var data=JsonUtility.FromJson<CarnivalQuiz.Scores>(File.ReadAllText(quiz.ScorePath));Assert(data.entries.Exists(e=>e.player=="Verification Player"&&e.correct==ClassificationBank.RoundSize-1),"Saved entry incorrect");
                    typeof(CarnivalQuiz).GetMethod("LoadScores",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(quiz,null);
                    Assert(quiz.LeaderboardSecondsRemaining>9,"Leaderboard timer did not start after completion");
                    Assert(System.Array.Exists(quiz.rows,r=>r.player.text=="Verification Player"),"Completed player missing from leaderboard");
                    stage=8;next=EditorApplication.timeSinceStartup+9;
                }
                else if(stage==8)
                {
                    Assert(quiz.rows[0].player.text.Length>0,"Leaderboard reload failed");Assert(before==System.Array.FindAll(quiz.GetComponentsInChildren<Transform>(true), t=>!t.name.EndsWith(" Input Caret")).Length,"Runtime hierarchy changed");
                    Assert(quiz.displays.Registered&&quiz.LeaderboardSecondsRemaining>0,"Idle reset occurred before 10 seconds");
                    stage=9;next=EditorApplication.timeSinceStartup+1.3;
                }
                else if(stage==9)
                {
                    Assert(quiz.splash.gameObject.activeSelf&&quiz.leaderboard.gameObject.activeSelf,"Dual-screen replay failed");
                    Assert(!quiz.displays.Registered&&quiz.displays.nameField.text==""&&quiz.displays.employeeIdField.text=="","Registration did not reset");
                    Assert(quiz.LeaderboardSecondsRemaining==0,"Idle reset timer still running");
                    quiz.reducedMotion=false;idleHome=quiz.GetComponent<CarnivalAmbient>().titleArtwork.anchoredPosition;stage=10;next=EditorApplication.timeSinceStartup+.8;
                }
                else if(stage==10)
                {
                    var ambient=quiz.GetComponent<CarnivalAmbient>();
                    Assert(Vector2.Distance(idleHome,ambient.titleArtwork.anchoredPosition)<.01f,"Carnival title should stay still");
                    Assert(ambient.idleGlints.Length==3&&ambient.idleGlints[0].gameObject.activeInHierarchy,"Idle border lights missing");
                    Capture(quiz.displays.camera1,"Completion-idle-animated");quiz.reducedMotion=true;stage=11;next=EditorApplication.timeSinceStartup+.15;
                }
                else
                {
                    Assert(Vector2.Distance(idleHome,quiz.GetComponent<CarnivalAmbient>().titleArtwork.anchoredPosition)<.01f,"Reduced motion did not restore idle artwork");
                    File.WriteAllText(Report,"PASS: dual-camera isolation; registration; ten random classification questions; four NFC cubes; unknown and duplicate scan guards; 9/10 score saved/reloaded; automatic leaderboard after final answer; completed player shown; leaderboard stays visible before 10 seconds; automatic dual idle reset after 10 seconds; cleared registration and timer; static carnival title; authored border lights; reduced-motion restore; unchanged authored quiz hierarchy ("+before+" objects). Test scores written only to temporary cache.");
                    Finish();
                }
            }
            catch(Exception e){File.WriteAllText(Report,"FAIL: "+e);Debug.LogError(e);Finish();}
        }
        static void Finish(){EditorApplication.update-=Tick;if(device!=null)InputSystem.RemoveDevice(device);EditorApplication.isPlaying=false;}
        public static void Capture(Camera camera,string name)
        {
            Canvas.ForceUpdateCanvases();
            var previous=camera.targetTexture;var active=RenderTexture.active;
            var rt=new RenderTexture(1920,1080,24);var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();File.WriteAllBytes("Temp/"+name+".png",texture.EncodeToPNG());}
            finally{camera.targetTexture=previous;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}
        }
    }
}

