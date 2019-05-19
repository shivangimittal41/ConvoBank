using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
 *          
 *          Fund Transfer Details
 *          1. beneficiary_AccountNumber    - reciever account number
 *          2. beneficiary_TransferAmount   - reiever amount
 *          3. beneficiary_Remarks          - reciving remarks
 *          
 *          Account Overview
 *          1. self_Balance                 - Your Current Balance
 *          2. self_AccountNumber           - Your Accoount Number
 *          
 *          Buttons
 *          1. transfer_Proceed             - For doing transaction and going to fundTransferConfirmation Screen
 *          2. transfer_Cancel              - For Resetting all the Data in Fund Transfer Field.
 */


namespace ConversationalBanking
{
    [Activity(Label = "Fund Transfers", NoHistory =true)]
    public class fundTransfers : AppCompatActivity
    {
        TextView transactionId, self_Balance, self_AccountNumber;
        EditText beneficiary_AccountNumber, beneficiary_TransferAmount, beneficiary_Remarks;
        Button transfer_Proceed, transfer_Cancel;
        CoordinatorLayout parentLayout;
        private readonly int VOICE = 10;
        ProgressDialog progress;
        private bool BenAccNumSpeaking = true, BenAmountSpeaking = false, BenCommentSpeaking = false;
        private string message;
        private bool transactionStatus = false;
        public string transaction_id, transaction_amount, transaction_receiver, transaction_remark, transaction_date;

        public void VoiceIntentStarter()
        {
            // Initialize the Voice Intent.
            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 50000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 50000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 55000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            StartActivityForResult(voiceIntent, VOICE);
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            models.speakerClass.SpeakerKiller();
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.fundTransferScreen);

            DateTime trial = DateTime.Now.ToLocalTime();
            string trial_string = DateTime.Now.ToString("dd-MM-yyyy");
            
            //transaction_id = Intent.GetStringExtra("transaction_id") ?? "Transaction ID not available";
            //transaction_amount = Intent.GetStringExtra("transaction_amount") ?? "Amount not found";
            //transaction_receiver = Intent.GetStringExtra("transaction_receiver") ?? "Reciever's data not available";
            //transaction_remark = Intent.GetStringExtra("transaction_remark") ?? "Remark not available";

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.fundTransfer_Toolbar);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);

            FindViews();
            ClickEvents();

            progress = new Android.App.ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetCancelable(false);

            self_AccountNumber.Text = models.LoggedInUserData.u_acc_number;
            self_Balance.Text = models.LoggedInUserData.u_amount;

            //beneficiary_AccountNumber.Text = "546897895213";
            await models.speakerClass.SpeakerAsync("Please speak Beneficiary's account Number.");
            VoiceIntentStarter();
            await Task.Delay(5000);
            await models.speakerClass.SpeakerAsync("Please speak the amount to be transferred.");
            VoiceIntentStarter();
            await Task.Delay(5000);
            await models.speakerClass.SpeakerAsync("Please speak the remark for the beneficiary.");
            VoiceIntentStarter();

            await Task.Delay(5000);
            await models.speakerClass.SpeakerAsync("Speak Proceed to go forward or stop to cancel..");
            VoiceIntentStarter();
            


        }

        private void FindViews()
        {
            //transactionId = FindViewById<TextView>(Resource.Id.fundTransfer_TransactionID);
            self_Balance = FindViewById<TextView>(Resource.Id.fundTransfer_SelfBalance);
            self_AccountNumber = FindViewById<TextView>(Resource.Id.fundTransfer_SelfAccountNumber);
            beneficiary_AccountNumber = FindViewById<EditText>(Resource.Id.fundTransfer_RecieverAccount);
            beneficiary_TransferAmount = FindViewById<EditText>(Resource.Id.fundTransfer_RecieverAmount);
            beneficiary_Remarks = FindViewById<EditText>(Resource.Id.fundTransfer_RecieverRemarks);
            transfer_Proceed = FindViewById<Button>(Resource.Id.servicefundTransfer_Approve);
            transfer_Cancel = FindViewById<Button>(Resource.Id.servicefundTransfer_Cancel);
            parentLayout = FindViewById<CoordinatorLayout>(Resource.Id.fundTransfer_ParentLayout);


        }

        private void ClickEvents()
        {
            transfer_Proceed.Click += Transfer_Proceed_ClickAsync;
            transfer_Cancel.Click += Transfer_Cancel_ClickAsync;
        }

        private async void Transfer_Cancel_ClickAsync(object sender, EventArgs e)
        {
            await models.speakerClass.SpeakerAsync("Transaction is Cancelled.");
            Snackbar.Make(parentLayout, "Transaction is Cancelled.", Snackbar.LengthLong).Show();

        }

        private async void Transfer_Proceed_ClickAsync(object sender, EventArgs e)
        {
            //string device_id = (string)Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);

            if (String.IsNullOrEmpty(beneficiary_AccountNumber.Text) || String.IsNullOrEmpty(beneficiary_TransferAmount.Text)
                || String.IsNullOrEmpty(beneficiary_Remarks.Text))
            {
                //Toast.MakeText(this, "Empty Fields", ToastLength.Short).Show();
                await models.speakerClass.SpeakerAsync("Please fill the empty fields.");
                Snackbar.Make(parentLayout, "Empty Fields", Snackbar.LengthLong).Show();
                return;
            }
            else
            {
                if (Convert.ToInt64(models.LoggedInUserData.u_amount) < Convert.ToInt64(transaction_amount))
                {
                    // If transfer amount is more than the person's total amount in bank, then we can't initiate the transfer.

                    await models.speakerClass.SpeakerAsync("You have Insufficient Balance.");
                    Snackbar.Make(parentLayout, "Insufficient Funds", Snackbar.LengthLong).Show();
                    return;
                }
                //if (string.IsNullOrEmpty(device_id))
                //    device_id = "Device Not Found";
                await models.speakerClass.SpeakerAsync("Transfer in process. Please wait");
                progress.SetMessage("Transfer in process...");
                progress.Show();
                await transferPoster();
                progress.Hide();
                
                if (this.transactionStatus)
                {
                    /* Since we've successfully transfered the money, we need to update the same in our internal cache as well. So that it can be
                     * reflected same everywhere.
                     */
                    models.LoggedInUserData.u_amount = Convert.ToString(Convert.ToDouble(models.LoggedInUserData.u_amount) - Convert.ToDouble(transaction_amount));

                    //Snackbar.Make(parentLayout, "Transaction Complete", Snackbar.LengthLong).Show();
                    new Thread(new ThreadStart(async delegate {
                        
                        RunOnUiThread(() => Snackbar.Make(parentLayout, "Transfer Successful!", Snackbar.LengthLong).Show());

                    })).Start();
                                        
                    var intentTransferConfirmation = new Intent(this, typeof(fundTransferConfirmation));

                    intentTransferConfirmation.PutExtra("transaction_id", transaction_id);
                    intentTransferConfirmation.PutExtra("transaction_date", transaction_date);
                    intentTransferConfirmation.PutExtra("transaction_amount", transaction_amount);
                    intentTransferConfirmation.PutExtra("transaction_receiver", transaction_receiver);
                    intentTransferConfirmation.PutExtra("transaction_remark", transaction_remark);
                   

                    await models.speakerClass.SpeakerAsync("Your transaction was successfull");
                    
                                                         
                    
                    StartActivity(intentTransferConfirmation);

                    
                    Finish();

                }
                              

            }
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }
        private async Task transferPoster()
        {
            loginScreen._client.BaseAddress = new System.Uri(MainActivity.primaryDomain);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("sender_account_number", models.LoggedInUserData.u_acc_number),
                    new KeyValuePair<string, string>("receiver_account_number", beneficiary_AccountNumber.Text),
                    new KeyValuePair<string, string>("device_id", models.LoggedInUserData.u_device_id ),
                    new KeyValuePair<string, string>("amount_to_transfer", beneficiary_TransferAmount.Text),
                    new KeyValuePair<string, string>("transaction_remark", beneficiary_Remarks.Text),
                    new KeyValuePair<string, string>("cust_id", models.LoggedInUserData.u_id ),
                    new KeyValuePair<string, string>("session_token", models.LoggedInUserData.u_session_token )

                });
            //Console.WriteLine("device_id post " + models.LoggedInUserData.u_device_id);
            var result = await loginScreen._client.PostAsync("/conversational_api/funds_transfer/funds_transfer.php", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            try
            {
                dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent);

                if (obj2.error_code == 1)
                {
                    await models.speakerClass.SpeakerAsync("Transfer failed. ");
                    Console.WriteLine("Transfer Failed : " + obj2.message);
                    this.message = obj2.message;
                    this.transactionStatus = false;
                }
                else
                {
                    this.message = obj2.message;
                    this.transactionStatus = true;
                    transaction_id = (string)obj2.transaction_id;
                    transaction_amount = (string)obj2.transaction_amount;
                    transaction_receiver = (string)obj2.transaction_receiver;
                    transaction_remark = (string)obj2.transaction_remark;
                    transaction_date = (string)obj2.transaction_date;
                }
            }
            catch (Exception transactionException)
            {
                Console.WriteLine("transactionException : " + transactionException);
                throw;
            }
        }

        public override void OnBackPressed()
        {
            //base.OnStop();
            //base.OnDestroy();
            //CrossTextToSpeech.Dispose();
            models.speakerClass.SpeakerKiller();
            base.OnBackPressed();
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

                            foreach (string item in matches)
                            {
                                Console.WriteLine("My Word : " + item);
                                if (item.Replace(" ", "").ToLower().Contains("proceed"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    transfer_Proceed.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("forward"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    transfer_Proceed.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("transfer"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    transfer_Proceed.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("ok"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    transfer_Proceed.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("cancel"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    transfer_Cancel.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("no"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    transfer_Cancel.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("back"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    transfer_Cancel.PerformClick();
                                    break;
                                }
                                else if (item.ToLower().Contains("exit"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                                }
                                else
                                {
                                    //await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                                    //VoiceIntentStarter();
                                    if (BenAccNumSpeaking)
                                    {
                                        beneficiary_AccountNumber.Text = item.ToLower().Trim();
                                        BenAccNumSpeaking = false;
                                        BenAmountSpeaking = true;
                                        BenCommentSpeaking = false;
                                    }
                                    else if (BenAmountSpeaking)
                                    {
                                        beneficiary_TransferAmount.Text = item.ToLower().Trim();
                                        BenCommentSpeaking = true;
                                        BenAccNumSpeaking = false;
                                        BenAmountSpeaking = false;
                                    }
                                    else if (BenCommentSpeaking)
                                    {
                                        beneficiary_TransferAmount.Text = item.ToLower().Trim();
                                        BenCommentSpeaking = false;
                                        BenAccNumSpeaking = true;
                                        BenAmountSpeaking = false;
                                    }
                                    else
                                    {
                                        await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                                        VoiceIntentStarter();
                                    }
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
                Console.WriteLine("My fund transfer Error : " + ex);
                await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                VoiceIntentStarter();
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }

    }
}