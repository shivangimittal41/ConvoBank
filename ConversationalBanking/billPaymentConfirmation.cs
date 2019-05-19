using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Speech;

/*
 *          VARIABLES USED:
 *          
 *          Bill Payment Confirmation Details
 *          1. billPaymentConfirmation_Amount               - bill amount
 *          2. billPaymentConfirmation_BillingMonth         - billing month
 *          3. billPaymentConfirmation_AccountNumber        - user bank account number

 *          Account Overview
 *          1. billPaymentConfirmation_SelfAccountNumber    - Your Current Number
 *          2. billPayment_Balance                          - Your Accoount Balance
 *          
 *          Buttons
 *          1. billPaymentConfirmation_Dashboard            - Completing Transaction and going to Dashboard.
 */

namespace ConversationalBanking
{
    [Activity(Label = "Bill Payment Confirmation", NoHistory = false)]
    public class billPaymentConfirmation : AppCompatActivity
    {
        TextView billPaymentConfirmation_Amount, billPaymentConfirmation_BillingMonth, billPaymentConfirmation_AccountNumber;
        TextView billPayment_Balance, billPaymentConfirmation_SelfAccountNumber;
        Button billPaymentConfirmation_Dashboard;
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

            SetContentView(Resource.Layout.billPaymentConfirmationScreen);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.billPaymentConfirmation_Toolbar);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);

            FindViews();
            ClickEvents();

            billPaymentConfirmation_Amount.Text = Intent.GetStringExtra("bill_amount") ?? "Transaction amount not available";
            billPaymentConfirmation_BillingMonth.Text = Intent.GetStringExtra("bill_month") ?? "Bill Month not found";
            billPaymentConfirmation_AccountNumber.Text = models.LoggedInUserData.u_acc_number;
            billPayment_Balance.Text = models.LoggedInUserData.u_amount;
            billPaymentConfirmation_SelfAccountNumber.Text = models.LoggedInUserData.u_acc_number;

            await models.speakerClass.SpeakerAsync("Your bill payment was successfull.");
            await models.speakerClass.SpeakerAsync("You paid rupees " + billPaymentConfirmation_Amount.Text);
            await models.speakerClass.SpeakerAsync("to BSES for the electricity bill for the month of " + billPaymentConfirmation_BillingMonth.Text);
            await models.speakerClass.SpeakerAsync("from the account number " + billPaymentConfirmation_AccountNumber.Text.Aggregate(string.Empty, (c, i) => c + i + "  "));
            

            await models.speakerClass.SpeakerAsync("and your current balance is rupees " + billPayment_Balance.Text);

            await models.speakerClass.SpeakerAsync("Reply with back to go back to the dashboard");

            VoiceIntentStarter();
        }

        private void FindViews()
        {
            billPaymentConfirmation_Amount = FindViewById<TextView>(Resource.Id.billPaymentConfirmation_Amount);
            billPaymentConfirmation_BillingMonth = FindViewById<TextView>(Resource.Id.billPaymentConfirmation_billingMonth);
            billPaymentConfirmation_AccountNumber = FindViewById<TextView>(Resource.Id.billPaymentConfirmation_AccountNumber);
            billPayment_Balance = FindViewById<TextView>(Resource.Id.billPaymentConfirmation_AvailableBalance);
            billPaymentConfirmation_Dashboard = FindViewById<Button>(Resource.Id.billPaymentConfirmation_Dashboard);
            billPaymentConfirmation_SelfAccountNumber = FindViewById<TextView>(Resource.Id.billPaymentConfirmation_SelfAccountNumber);
           
        }

        private void ClickEvents()
        {
            billPaymentConfirmation_Dashboard.Click += BillPaymentConfirmation_Dashboard_Click;
        }

        private void BillPaymentConfirmation_Dashboard_Click(object sender, EventArgs e)
        {
            models.speakerClass.SpeakerKiller();
            var intentBillConfirmationDashboard = new Intent(this, typeof(dashboard));
            StartActivity(intentBillConfirmationDashboard);
            Finish();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }

        public override void OnBackPressed()
        {
            //Do Nothing Here.
            //base.OnStop();
            //base.OnDestroy();
            //CrossTextToSpeech.Dispose();
            //models.speakerClass.SpeakerKiller();
            //base.OnBackPressed();
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
                                if (item.Replace(" ", "").ToLower().Contains("back"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    //billPaymentConfirmation_Dashboard.PerformClick();
                                    Finish();
                                    break;
                                }
                                if (item.Replace(" ", "").ToLower().Contains("dashboard"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    //billPaymentConfirmation_Dashboard.PerformClick();
                                    Finish();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("ok"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    //billPaymentConfirmation_Dashboard.PerformClick();
                                    Finish();
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
                Console.WriteLine("My Errur : " + ex);
                await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                VoiceIntentStarter();
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }

    }
}