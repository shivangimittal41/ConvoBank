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
 *          VARIABLES USED
 *          1. confirmation_TxnId           - Transaction ID
 *          2. confirmation_Amount          - Transaction Amount
 *          3. confirmation_Date            - Transaction Date
 *          4. confirmation_AccountNumber   - Transaction Account Number(beneficiary)
 *          5. confirmation_Remarks         - Transaction Remarks
 *          
 *          BUTTONS
 *          1. confirmation_Dashboard       - Goes back to dashboard, finishing previous activity
 * 
 */
 
namespace ConversationalBanking
{
    [Activity(Label = "Fund Transfer Confirmation", NoHistory =false)]
    public class fundTransferConfirmation : AppCompatActivity
    {
        public string transaction_id, transaction_amount, transaction_receiver, transaction_remark, transaction_date;
        private readonly int VOICE = 10;
        CoordinatorLayout parentLayout;
        TextView confirmation_TxnId, confirmation_Amount, confirmation_Date, confirmation_AccountNumber, confirmation_Remarks ;
        Button confirmation_Dashboard;
        TextView confirmation_SelfAccountNumber, confirmation_SelfBalance;

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

            transaction_id = Intent.GetStringExtra("transaction_id") ?? "Transaction ID not available";
            transaction_amount = Intent.GetStringExtra("transaction_amount") ?? "Amount not found";
            transaction_receiver = Intent.GetStringExtra("transaction_receiver") ?? "Reciever's data not available";
            transaction_remark = Intent.GetStringExtra("transaction_remark") ?? "Remark not available";
            transaction_date = Intent.GetStringExtra("transaction_date") ?? "Date not available";


            SetContentView(Resource.Layout.fundTransferConfirmationScreen);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.fundTransferConfirmation_Toolbar);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);

            FindViews();
            ClickEvents();

            confirmation_TxnId.Text = transaction_id;
            confirmation_Amount.Text = transaction_amount;
            confirmation_AccountNumber.Text = transaction_receiver;
            confirmation_Remarks.Text = transaction_remark;
            confirmation_Date.Text = DateTime.Now.ToString("dd/MM/yyy");
            confirmation_SelfAccountNumber.Text = models.LoggedInUserData.u_acc_number;
            confirmation_SelfBalance.Text = models.LoggedInUserData.u_amount;

            await models.speakerClass.SpeakerAsync("Your transaction ID is" + confirmation_TxnId.Text.Aggregate(string.Empty, (c, i) => c + i + "  "));
            await models.speakerClass.SpeakerAsync("Transaction" + confirmation_Date.Text);
            await models.speakerClass.SpeakerAsync("You have successfully transfered rupees " + confirmation_Amount.Text);
            await models.speakerClass.SpeakerAsync("to the account number " + confirmation_AccountNumber.Text.Aggregate(string.Empty, (c, i) => c + i + "  "));
            await models.speakerClass.SpeakerAsync("The remark recorded is " + confirmation_Remarks.Text);
            await models.speakerClass.SpeakerAsync("Your current balance is rupees " + confirmation_SelfBalance.Text);
            await models.speakerClass.SpeakerAsync("in your Account number " + confirmation_SelfAccountNumber.Text.Aggregate(string.Empty, (c, i) => c + i + "  "));
            //await models.speakerClass.SpeakerAsync("You have successfully transfered rupees " + transaction_amount + "to the account number " + transaction_receiver.Aggregate(string.Empty, (c, i) => c + i + "  "));
            await models.speakerClass.SpeakerAsync("Reply with back to go back to the dashboard");
           
            VoiceIntentStarter();
        }

        private void FindViews()
        {
            parentLayout = FindViewById<CoordinatorLayout>(Resource.Id.fundTransferConfirmation_ParentLayout);
            confirmation_TxnId = FindViewById<TextView>(Resource.Id.fundTransferConf_txnID);
            confirmation_Amount = FindViewById<TextView>(Resource.Id.fundTransferConf_Amount);
            confirmation_Date = FindViewById<TextView>(Resource.Id.fundTransferConf_Date);
            confirmation_AccountNumber = FindViewById<TextView>(Resource.Id.fundTransferConf_AccNumber);
            confirmation_Remarks = FindViewById<TextView>(Resource.Id.fundTransferConf_Remark);
            confirmation_Dashboard = FindViewById<Button>(Resource.Id.fundTransferConf_MainDashboard);
            confirmation_SelfAccountNumber = FindViewById<TextView>(Resource.Id.fundTransferConfirmation_SelfAccountNumber);
            confirmation_SelfBalance = FindViewById<TextView>(Resource.Id.fundTransferConfirmation_SelfBalance);
        }

        private void ClickEvents()
        {
            confirmation_Dashboard.Click += Confirmation_Dashboard_Click;
        }

        private void Confirmation_Dashboard_Click(object sender, EventArgs e)
        {
            var intentMainDashboard = new Intent(this, typeof(dashboard));
            StartActivity(intentMainDashboard);
            Finish();
            //OnBackPressed();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }

        public override void OnBackPressed()
        {
            models.speakerClass.SpeakerKiller();
            var intentMainDashboard = new Intent(this, typeof(dashboard));
            StartActivity(intentMainDashboard);
            Finish();
            //base.OnBackPressed();
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
                                if (item.Replace(" ", "").ToLower().Contains("go"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    confirmation_Dashboard.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("back"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    confirmation_Dashboard.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("ok"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    confirmation_Dashboard.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("dashboard"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    confirmation_Dashboard.PerformClick();
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
