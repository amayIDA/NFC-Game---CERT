using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace CyberQuiz.Editor
{
    [InitializeOnLoad] public static class CarnivalMotionVerification
    {
        static CarnivalQuiz quiz;
        static int stage;
        static double next,deadline;
        static bool running;
        static CarnivalMotionVerification(){EditorApplication.update+=Tick;}
        static void Assert(bool v,string message){if(!v)throw new Exception(message);}
        static void Capture(string name){CarnivalSmokeTest.Capture(quiz.displays.camera2,"Motion-"+name+"-top");CarnivalSmokeTest.Capture(quiz.displays.camera1,"Motion-"+name+"-bottom");}
        static void Tick()
        {
            if(!running)
            {
                if(!File.Exists("Temp/CarnivalMotionVerify.request")||EditorApplication.isCompiling||EditorApplication.isPlayingOrWillChangePlaymode)return;
                File.Delete("Temp/CarnivalMotionVerify.request");stage=0;next=0;deadline=EditorApplication.timeSinceStartup+60;running=true;EditorApplication.isPlaying=true;return;
            }
            if(EditorApplication.timeSinceStartup<next)return;
            try
            {
                Assert(EditorApplication.timeSinceStartup<deadline,"Motion verification timed out");if(!EditorApplication.isPlaying)return;
                switch(stage)
                {
                    case 0:
                        quiz=UnityEngine.Object.FindAnyObjectByType<CarnivalQuiz>();if(quiz==null||quiz.motion==null)return;
                        foreach(var question in quiz.questions)foreach(var option in question.options)Assert(option.revealGroup!=null&&option.surface.color.a==0,"Choice has a missing reveal group or a flat tile fill");
                        foreach(var canvas in new[]{quiz.displays.display1,quiz.displays.display2})foreach(var button in canvas.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                        {
                            if(button.GetComponent<CarnivalOption>()!=null||button.transform.IsChildOf(quiz.displays.idle.transform))continue;
                            var image=button.GetComponent<UnityEngine.UI.Image>();Assert(image.sprite!=null&&image.sprite.name.StartsWith("Group 8")&&image.preserveAspect,"Action button does not use the original Figma artwork/proportions");
                        }
                        quiz.RecordScores=false;quiz.reducedMotion=true;quiz.displays.OpenRegistration();stage++;next=EditorApplication.timeSinceStartup+.12;break;
                    case 1:
                        quiz.displays.nameField.text="Motion Preview";quiz.displays.employeeIdField.text="preview@example.com";quiz.displays.SubmitRegistration();stage++;next=EditorApplication.timeSinceStartup+1.7;break;
                    case 2:
                        quiz.reducedMotion=false;quiz.feedbackSeconds=.6f;quiz.StartQuiz();stage++;next=EditorApplication.timeSinceStartup+.16;break;
                    case 3:
                        Assert(quiz.IsBusy,"Entrance is not being animated");
                        Assert(quiz.questions[0].options[0].revealGroup.alpha>quiz.questions[0].options[3].revealGroup.alpha,"Choice entrances are not staggered");
                        Capture("01-entrance");stage++;next=EditorApplication.timeSinceStartup+.6;break;
                    case 4:
                        Assert(!quiz.IsBusy,"Question never unlocked");Capture("02-question");
                        var correct=quiz.ActiveQuestion.correctOption;quiz.SubmitTag(quiz.ActiveQuestion.options[correct].nfcValue);quiz.SubmitTag(quiz.ActiveQuestion.options[correct].nfcValue);
                        Assert(quiz.CorrectCount==1,"Repeated NFC tag was scored twice");stage++;next=EditorApplication.timeSinceStartup+.1;break;
                    case 5:
                        Assert(quiz.motion.rings[0].gameObject.activeSelf&&quiz.motion.rings[3].gameObject.activeSelf,"NFC lock ring missing on one display");Capture("03-nfc-lock");stage++;next=EditorApplication.timeSinceStartup+.52;break;
                    case 6:
                        Assert(quiz.feedback.gameObject.activeSelf&&quiz.feedbackTitle.text=="CORRECT!","Correct feedback missing");
                        Assert(quiz.motion.sparks[0].gameObject.activeSelf&&quiz.motion.sparks[48].gameObject.activeSelf,"Celebration missing on one display");
                        Assert(Vector2.Distance(quiz.motion.sparks[0].anchoredPosition,quiz.motion.sparks[48].anchoredPosition)<.01f,"Display effects are not synchronized");
                        Capture("04-correct");stage++;next=EditorApplication.timeSinceStartup+1.1;break;
                    case 7:
                        if(quiz.IsBusy){next=EditorApplication.timeSinceStartup+.15;return;}
                        Assert(quiz.ActiveQuestion.panel.gameObject.activeSelf,"Question 2 did not follow feedback");
                        quiz.SubmitTag(quiz.ActiveQuestion.options[(quiz.ActiveQuestion.correctOption+1)%4].nfcValue);stage++;next=EditorApplication.timeSinceStartup+.6;break;
                    case 8:
                        Assert(quiz.feedback.gameObject.activeSelf&&quiz.feedbackTitle.text=="INCORRECT","Incorrect feedback missing");
                        Assert(!quiz.motion.sparks[0].gameObject.activeSelf&&!quiz.motion.sparks[48].gameObject.activeSelf,"Incorrect answer launched a celebration");
                        Capture("05-incorrect");stage++;next=EditorApplication.timeSinceStartup+1.5;break;
                    case 9:
                        if(quiz.IsBusy){next=EditorApplication.timeSinceStartup+.15;return;}
                        quiz.ReturnHome();stage++;next=EditorApplication.timeSinceStartup+.7;break;
                    default:
                        Assert(!quiz.displays.Registered&&quiz.splash.gameObject.activeSelf&&quiz.leaderboard.gameObject.activeSelf,"Replay failed");
                        foreach(var root in quiz.motion.effectRoots)Assert(root.alpha==0,"Effect pool leaked into idle");
                        File.WriteAllText("Temp/CarnivalMotionVerification.txt","PASS: original Figma button sprite/proportions applied; staggered entrances; transition lockout; duplicate NFC guard; synchronized lock rings; correct fireworks on both displays; distinct incorrect response without celebration; next-question progression; replay clears all effects. Test scores disabled. Captured paired camera frames in Temp/Motion-*.png.");Finish();break;
                }
            }
            catch(Exception e){File.WriteAllText("Temp/CarnivalMotionVerification.txt","FAIL: "+e);Debug.LogException(e);Finish();}
        }
        static void Finish(){running=false;EditorApplication.isPlaying=false;}
    }
}
