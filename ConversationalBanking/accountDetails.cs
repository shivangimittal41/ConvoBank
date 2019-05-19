using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Speech;

/*
 *          VARIABLES USED:
 *          Bank Account Details
 *          1. account_Number       - Your Account Number
 *          2. account_CustID       - Your Customer Id
 *          3. account_balance      - Your Account Balance
 *          4. account_ifsc         - Bank's IFSC
 *          
 *          Personal Details
 *          1. account_selfName     - Your Name
 *          2. account_selfPhone    - Your Phone
 *          3. account_selfUIDAI    - Your Aadhar
 *          4. account_selfPAN      - Your PAN
 *          5. account_selfEmail    - Your E-Mail
 *          6. account_selfAddress  - Your Address
 * 
 */

namespace ConversationalBanking
{
    [Activity(Label = "Account Details", MainLauncher = false, NoHistory =true)]
    public class accountDetails : AppCompatActivity
    {
        CoordinatorLayout parentLayout;
        TextView account_Number, account_CustID, account_balance, account_ifsc;
        TextView account_selfName, account_selfPhone, account_selfUIDAI, account_selfPAN, account_selfEmail, account_selfAddress;
        private readonly int VOICE = 10;

        public void VoiceIntentStarter()
        {
            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 20000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 20000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 25000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            StartActivityForResult(voiceIntent, VOICE);
        }

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            models.speakerClass.SpeakerKiller();
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.accountDetailsScreen);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.accountDetails_Toolbar);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);

            FindViews();

            account_Number.Text = models.LoggedInUserData.u_acc_number;
            account_CustID.Text = models.LoggedInUserData.u_id;
            account_balance.Text = models.LoggedInUserData.u_amount;
            account_ifsc.Text = models.LoggedInUserData.u_ifsc_code;
            account_selfName.Text = models.LoggedInUserData.u_name;
            account_selfPhone.Text = models.LoggedInUserData.u_phone;
            account_selfUIDAI.Text = models.LoggedInUserData.u_uidai;
            account_selfPAN.Text = models.LoggedInUserData.u_pan;
            account_selfEmail.Text = models.LoggedInUserData.u_email;
            account_selfAddress.Text = models.LoggedInUserData.u_address;

            await models.speakerClass.SpeakerAsync("Welcome Back " + account_selfName.Text);
            await models.speakerClass.SpeakerAsync("Your Account Number Is " + account_Number.Text.ToString().Aggregate(string.Empty, (c, i) => c + i + "  "));
            await models.speakerClass.SpeakerAsync("Your Customer ID Is " + account_CustID.Text.ToString().Aggregate(string.Empty, (c, i) => c + i + "  "));
            await models.speakerClass.SpeakerAsync("Your Current Balance Is " + account_balance.Text);
            await models.speakerClass.SpeakerAsync("Your registered phone number Is " + account_selfPhone.Text.ToString().Aggregate(string.Empty, (c, i) => c + i + "  "));
            await models.speakerClass.SpeakerAsync("Your registered email ID Is " + account_selfEmail.Text);
            await models.speakerClass.SpeakerAsync("Your verified address is " + account_selfAddress.Text);

            await models.speakerClass.SpeakerAsync("Reply with Ok or go back to visit the dashboard");

            VoiceIntentStarter();
        }

        private void FindViews()
        {
            parentLayout = FindViewById<CoordinatorLayout>(Resource.Id.accountDetails_ParentLayout);
            account_Number = FindViewById<TextView>(Resource.Id.accountDetails_AccountNumber);
            account_CustID = FindViewById<TextView>(Resource.Id.accountDetails_AccountCustID);
            account_balance = FindViewById<TextView>(Resource.Id.accountDetails_AccountBalance);
            account_ifsc = FindViewById<TextView>(Resource.Id.accountDetails_AccountIFSC);

            account_selfName = FindViewById<TextView>(Resource.Id.accountDetails_SelfName);
            account_selfPhone = FindViewById<TextView>(Resource.Id.accountDetails_SelfPhone);
            account_selfUIDAI = FindViewById<TextView>(Resource.Id.accountDetails_SelfUIDAI);
            account_selfPAN = FindViewById<TextView>(Resource.Id.accountDetails_SelfPAN);
            account_selfEmail = FindViewById<TextView>(Resource.Id.accountDetails_SelfEmailID);
            account_selfAddress = FindViewById<TextView>(Resource.Id.accountDetails_SelfAddress);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }

        //public override void OnBackPressed()
        //{
        //    base.OnBackPressed();
        //}

        public override void OnBackPressed()
        {
            //base.OnStop();
            //base.OnDestroy();
            //CrossTextToSpeech.Dispose();
            models.speakerClass.SpeakerKiller();
            base.OnBackPressed();
            Finish();
            //base.OnDestroy();
        }
        protected async override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            try
            {
                if (requestCode == VOICE)
                {
                    if (resultVal == Result.Ok)
                    {
                        var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);

                        if (matches.Count != 0)
                        {
                            Console.WriteLine("what I SPOKE : " + matches[0].Substring(0, 5).ToLower());
                            /*
                             * Here I'm using IF ELSE, because I wanted to accept the long sentences as well.
                             * SWITCH statements had only one phrase. So, if a person said "ATM Location" or "ATM",
                             * they will be 2 separate cases. 
                             * But, in this case, the Speaker can say anything along with the word "ATM".
                             * So, as long as we hear "ATM", we want to show the user the "ATM" location.
                             * This will save us from generating various scenarios and phrases.
                             */
                            foreach (string item in matches)
                            {
                                Console.WriteLine("My Word : " + item);
                                //if (item.Replace(" ", "").ToLower().Contains("go"))
                                //{
                                //    models.speakerClass.SpeakerKiller();
                                //    a.PerformClick();
                                //    break;
                                //}
                                //else if (item.Replace(" ", "").ToLower().Contains("back"))
                                //{
                                //    models.speakerClass.SpeakerKiller();
                                //    billPaymentConfirmation_Dashboard.PerformClick();
                                //    break;
                                //}
                                //else if (item.Replace(" ", "").ToLower().Contains("ok"))
                                //{
                                //    models.speakerClass.SpeakerKiller();
                                //    billPaymentConfirmation_Dashboard.PerformClick();
                                //    break;
                                //}

                                //else 
                                if (item.ToLower().Contains("exit"))
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
                Console.WriteLine("My account detail Error : " + ex);
                await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                VoiceIntentStarter();
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }

    }
}