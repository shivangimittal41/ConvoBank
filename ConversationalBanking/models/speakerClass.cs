using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.TextToSpeech;

namespace ConversationalBanking.models
{
    class speakerClass
    {
        public async static Task SpeakerAsync(string speakingText)
        {
            await CrossTextToSpeech.Current.Speak(speakingText,
                    pitch: (float)1.0,
                    speakRate: (float)0.9,
                    volume: (float)2.0);
        }

        public static void SpeakerKiller()
        {
            CrossTextToSpeech.Dispose();
        }
    }
}