using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using System;
using Android.Content;
using Android.Speech;

namespace ConversationalBanking
{
    [Activity(MainLauncher = true, Theme = "@style/MyCustomThemeWithNoTitleBar")]
    public class MainActivity : AppCompatActivity
    {
        public static string primaryDomain;
        private readonly int VOICE = 10;
        FloatingActionButton getStarted;
        Switch microphoneToggle;
        CoordinatorLayout parentLayout;

        public void VoiceIntentStarter()
        {
            // Initialize the Voice Intent.
            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 20000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 20000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 25000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            StartActivityForResult(voiceIntent, VOICE);
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            primaryDomain = "https://xonshiz.heliohost.org";

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);
            FindViews();
            microphoneToggle.Checked = true;

            ClickEvents();

            await models.speakerClass.SpeakerAsync("Hello, Welcome to Convo Banking");
            await models.speakerClass.SpeakerAsync("what would you like to do. sign up or login?");

            VoiceIntentStarter();

        }

        private void FindViews()
        {
            parentLayout = FindViewById<CoordinatorLayout>(Resource.Id.getStarted_ParentLayout);
            getStarted = FindViewById<FloatingActionButton>(Resource.Id.convoBank_GetStarted);
            microphoneToggle = FindViewById<Switch>(Resource.Id.convoBank_microPhoneToggle);
        }

        private void ClickEvents()
        {
            getStarted.Click += GetStarted_Click;
        }

        private void GetStarted_Click(object sender, EventArgs e)
        {
            //Snackbar.Make(parentLayout, "Clicked!", Snackbar.LengthLong).SetAction("OK",(view)=> { }).Show();
            var intentLogin = new Intent(this, typeof(loginScreen));
            StartActivity(intentLogin);
        }
        protected async override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            try
            {
                Console.WriteLine("Request Code : " + requestCode);
                Console.WriteLine("resultVal Code : " + resultVal);
                Console.WriteLine("data Code : " + data);
                if (requestCode == VOICE)
                {
                    if (resultVal == Result.Ok)
                    {
                        var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);

                        if (matches.Count != 0)
                        {
                            Console.WriteLine("what I SPOKE : " + matches[0].Substring(0, 5).ToLower());

                            foreach (string item in matches)
                            {
                                Console.WriteLine("My Word : " + item);
                                if (item.Replace(" ", "").ToLower().Contains("login"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    getStarted.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("sign up"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    getStarted.PerformClick();
                                    break;
                                }

                                else if (item.ToLower().Contains("exit"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                                }
                                else
                                {
                                    await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                                    VoiceIntentStarter();
                                }
                            }

                        }
                        else
                        {
                            await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                            VoiceIntentStarter();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("My Mainscreen Errur : " + ex);
                await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                VoiceIntentStarter();
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }

    }
}