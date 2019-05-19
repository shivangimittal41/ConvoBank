using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
 *          Bill Payment Details
 *          1. billPayment_Amount               - bill amount
 *          2. billPayment_BillingMonth         - billing month
 *          3. billPayment_CANumber             - user auth Number (different than CustomerID or Acc number)
 *          4. billPayment_MeterReading         - user meter reading
 *          5. billPayment_MeterNumber          - user registeered meter number
 *          6. billPayment_DueDate              - bill due date
 *          
 *          Account Overview
 *          1. billPayment_SelfAccountNumber    - Your Current Balance
 *          2. billPayment_AccountBalance       - Your Accoount Number
 *          
 *          Buttons
 *          1. billPayment_Approve             - For doing transaction and going to billPaymentConfirmation Screen
 *          2. billPayment_Cancel              - same functionality as Back button
 */



namespace ConversationalBanking
{
    [Activity(Label = "Bill Payment",NoHistory = true)]
    public class billPayment : AppCompatActivity
    {
        private string billDetailMessage, billPayMessage;
        private bool billDetailsFlag=false, billPaymentFlag = false;
        private string b_creation_date, b_last_date, b_amount, b_id, b_meter_number, b_reading_date, b_pay_date, b_reading;
        private bool bool_acount_number=true, bool_amount = false, bool_remarks = false;

        ProgressDialog progress;
        private readonly int VOICE = 10;
        //private bool selfCardnumSpeaking = true;

        TextView billPayment_Amount, billPayment_BillingMonth, billPayment_CANumber, billPayment_MeterReading, billPayment_MeterNumber, billPayment_DueDate;
        TextView billPayment_SelfAccountNumber, billPayment_AccountBalance;
        Button billPayment_Cancel, billPayment_Approve, billPayment_BacktoDashboard;
        RelativeLayout billPresent, billNotPresent;

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

            SetContentView(Resource.Layout.billPaymentScreen);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.billPayment_Toolbar);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);

            progress = new Android.App.ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetCancelable(false);

            FindViews();
            ClickEvents();

            await models.speakerClass.SpeakerAsync("Getting the details.");
            progress.SetMessage("Fetching Bill Details");
            progress.Show();
            await BillDetailsFetcher();
            progress.Hide();
            //BillDetailsSetter();
            //await Task.Delay(5000);

            Console.WriteLine("billDetailsFlag : " + billDetailsFlag);
            if (billDetailsFlag)
            {
                billPresent.Visibility = ViewStates.Visible;
                billNotPresent.Visibility = ViewStates.Gone;
                
                billPayment_Amount.Text = b_amount;
                billPayment_BillingMonth.Text = b_creation_date;
                billPayment_CANumber.Text = models.LoggedInUserData.u_ca_number;
                billPayment_MeterReading.Text = b_reading;
                billPayment_MeterNumber.Text = b_meter_number;
                string bill_due_date = Convert.ToString(b_last_date);
                billPayment_DueDate.Text = bill_due_date.Split(null)[0];
                billPayment_SelfAccountNumber.Text = models.LoggedInUserData.u_acc_number;
                billPayment_AccountBalance.Text = models.LoggedInUserData.u_amount;

                await models.speakerClass.SpeakerAsync("You have initiated the bill payment.");
                await models.speakerClass.SpeakerAsync("Amount to be paid is " + billPayment_Amount.Text);
                await models.speakerClass.SpeakerAsync("Payment on " + billPayment_BillingMonth.Text);
                await models.speakerClass.SpeakerAsync("Your card number for the payment is " + billPayment_CANumber.Text.ToString().Aggregate(string.Empty, (c, i) => c + i + "  "));
                await models.speakerClass.SpeakerAsync("Your meter reading is " + billPayment_MeterReading.Text.ToString().Aggregate(string.Empty, (c, i) => c + i + "  "));
                await models.speakerClass.SpeakerAsync("And meter number is " + billPayment_MeterNumber.Text.ToString().Aggregate(string.Empty, (c, i) => c + i + "  "));
                await models.speakerClass.SpeakerAsync("The bill due date is " + billPayment_DueDate.Text);
                await models.speakerClass.SpeakerAsync("Your Current account balance is" + billPayment_AccountBalance.Text);
                await models.speakerClass.SpeakerAsync("Your current Account number is." + billPayment_SelfAccountNumber.Text.ToString().Aggregate(string.Empty, (c, i) => c + i + "  "));

                //while (true)
                //{
                //    await models.speakerClass.SpeakerAsync("Please speak your card Number for the payment.");
                //    VoiceIntentStarter();
                //    await Task.Delay(2000);
                //}

                await models.speakerClass.SpeakerAsync("Do you want to pay this bill? or go back to dashboard?");
                VoiceIntentStarter();
                await Task.Delay(2000);

                //await models.speakerClass.SpeakerAsync("Speak ok or back to visit the dashboard.");
                //VoiceIntentStarter();

            }
            else
            {
                billPresent.Visibility = ViewStates.Gone;
                billNotPresent.Visibility = ViewStates.Visible;
                //Console.WriteLine("Bhai chalra hai yo");

                ////THESE ARE NOT WORKING!
                //progress.SetMessage("Well, working");
                //progress.Show();
                ////Toast.MakeText(this, "No Pending Bills", ToastLength.Long);

                //progress.Hide();
            }
            //await models.speakerClass.SpeakerAsync("Speak Cancel to go back to the dashboard.");
            //VoiceIntentStarter();
        }


        private void FindViews()
        {
            billPayment_Amount = FindViewById<TextView>(Resource.Id.billPayment_Amount);
            billPayment_BillingMonth = FindViewById<TextView>(Resource.Id.billPayment_BillingMonth);
            billPayment_CANumber = FindViewById<TextView>(Resource.Id.billPayment_CANumber);
            billPayment_MeterReading = FindViewById<TextView>(Resource.Id.billPayment_MeterReading);
            billPayment_MeterNumber = FindViewById<TextView>(Resource.Id.billPayment_MeterNumber);
            billPayment_DueDate = FindViewById<TextView>(Resource.Id.billPayment_DueDate);

            billPayment_SelfAccountNumber = FindViewById<TextView>(Resource.Id.billPayment_SelfAccountNumber);
            billPayment_AccountBalance = FindViewById<TextView>(Resource.Id.billPayment_SelfBalance);

            billPayment_Cancel = FindViewById<Button>(Resource.Id.billPayment_Cancel);
            billPayment_Approve = FindViewById<Button>(Resource.Id.billPayment_Approve);
            billPayment_BacktoDashboard = FindViewById<Button>(Resource.Id.billPayment_BackDashboard);
            billPresent = FindViewById<RelativeLayout>(Resource.Id.bill_presentLayout);
            billNotPresent = FindViewById<RelativeLayout>(Resource.Id.bill_notPresentLayout);

        }

        private void ClickEvents()
        {
            billPayment_Approve.Click += BillPayment_Approve_Click;
            billPayment_Cancel.Click += BillPayment_Cancel_Click;
            billPayment_BacktoDashboard.Click += BillPayment_Cancel_Click;
        }

        private void BillPayment_Cancel_Click(object sender, EventArgs e)
        {
            OnBackPressed();
        }

        private async void BillPayment_Approve_Click(object sender, EventArgs e)
        {
            //Toast.MakeText(this, "Bill Payment", ToastLength.Short).Show();
            if (Convert.ToInt64(models.LoggedInUserData.u_amount) < Convert.ToInt64(b_amount))
            {
                // If transfer amount is more than the person's total amount in bank, then we can't initiate the transfer.
                await models.speakerClass.SpeakerAsync("You have insufficient balance.");
                Snackbar.Make(billNotPresent, "Insufficient Funds", Snackbar.LengthLong).Show();
                return;
            }

            if (billDetailsFlag)
            {
                await models.speakerClass.SpeakerAsync("Processing. Please Wait.");
                progress.SetMessage("Procesing Your Payment");
                progress.Show();
                await BillPaymentMaker();
                progress.Hide();

                if (billPaymentFlag)
                {
                    //Toast.MakeText(this, billPayMessage, ToastLength.Long);
                    Snackbar.Make(billNotPresent, billPayMessage, Snackbar.LengthLong).Show();
                    models.LoggedInUserData.u_amount = Convert.ToString(Convert.ToInt64(models.LoggedInUserData.u_amount) - Convert.ToInt64(b_amount));
                    var intentBillPaymentConfirmation = new Intent(this, typeof(billPaymentConfirmation));
                    intentBillPaymentConfirmation.PutExtra("bill_amount", b_amount);
                    string bill_pay_date = Convert.ToString(b_creation_date);
                    intentBillPaymentConfirmation.PutExtra("bill_month", bill_pay_date.Split(null)[0]);
                    StartActivity(intentBillPaymentConfirmation);
                    Finish();
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }

        private async Task BillDetailsFetcher()
        {
            loginScreen._client.BaseAddress = new System.Uri(MainActivity.primaryDomain);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("user_account_number", models.LoggedInUserData.u_acc_number)
                });
            var result = await loginScreen._client.PostAsync("/conversational_api/bills_payment/get_bill_details.php", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            try
            {
                dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent);

                if (obj2.error_code == 1)
                {
                    //Console.WriteLine("Sign Up Failed : " + obj2.message);
                    await models.speakerClass.SpeakerAsync("You have no pending bills.");
                    this.billDetailMessage = "No Pending Bills";
                    this.billDetailsFlag = false;

                }
                else
                {
                    this.billDetailMessage = obj2.message;
                    this.billDetailsFlag = true;
                    b_creation_date = (string)obj2.b_creation_date;
                    b_last_date = (string)obj2.b_last_date;
                    b_amount = (string)obj2.b_amount;
                    b_id = (string)obj2.b_id;
                    b_meter_number = (string)obj2.b_meter_number;
                    b_reading_date = (string)obj2.b_reading_date;
                    b_pay_date = (string)obj2.b_pay_date;
                    b_reading = (string)obj2.b_reading;
                }
            }
            catch (Exception SignInException)
            {
                Console.WriteLine("SignInException : " + SignInException);
                throw;
            }
        }

        private async Task BillPaymentMaker()
        {
            loginScreen._client.BaseAddress = new System.Uri(MainActivity.primaryDomain);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("bill_amount", b_amount),
                    new KeyValuePair<string, string>("user_account_number", models.LoggedInUserData.u_acc_number),
                    new KeyValuePair<string, string>("bill_id", b_id)
                });
            var result = await loginScreen._client.PostAsync("/conversational_api/bills_payment/electricity_bill_pay.php", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            try
            {
                dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent);

                if (obj2.error_code == 1)
                {
                    //Console.WriteLine("Sign Up Failed : " + obj2.message);
                    await models.speakerClass.SpeakerAsync("You have no pending bills.");
                    this.billPayMessage = "No Pending Bills";
                    this.billPaymentFlag = false;
                }
                else
                {
                    this.billPayMessage = obj2.message;
                    this.billPaymentFlag = true;
                }
            }
            catch (Exception SignInException)
            {
                Console.WriteLine("SignInException : " + SignInException);
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
                                    billPayment_Approve.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("okay"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment_Approve.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("pay"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment_Approve.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("confirm"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment_Approve.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("yes"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment_Approve.PerformClick();
                                    break;
                                }

                                else if (item.Replace(" ", "").ToLower().Contains("back"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment_Cancel.PerformClick();
                                    //base.OnBackPressed();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("no"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment_Cancel.PerformClick();
                                    //base.OnBackPressed();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("cancel"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment_Cancel.PerformClick();
                                    //base.OnBackPressed();
                                    break;
                                }

                                else if (item.Replace(" ", "").ToLower().Contains("dashboard"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    base.OnBackPressed();
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
                                    //if (bool_acount_number)
                                    //{
                                    //    billPayment_CANumber.Text = item.ToLower().Trim();
                                    //    bool_acount_number = false;
                                    //    bool_amount = true;
                                    //    bool_remarks = false;
                                    //}

                                    //if (bool_acount_number)
                                    //{
                                    //    billPayment_CANumber.Text = item.ToLower().Trim();
                                    //    bool_acount_number = false;
                                    //    bool_amount = true;
                                    //    bool_remarks = false;
                                    //}
                                    //else if (bool_amount)
                                    //{
                                    //    billPayment_CANumber.Text = item.ToLower().Trim();
                                    //    bool_acount_number = false;
                                    //    bool_amount = false;
                                    //    bool_remarks = true;
                                    //}
                                    //if (bool_remarks)
                                    //{
                                    //    billPayment_CANumber.Text = item.ToLower().Trim();
                                    //    bool_acount_number = false;
                                    //    bool_amount = false;
                                    //    bool_remarks = false;
                                    //}
                                    //else if (BenAmountSpeaking)
                                    //{
                                    //    beneficiary_TransferAmount.Text = item.ToLower().Trim();
                                    //    BenCommentSpeaking = true;
                                    //    BenAccNumSpeaking = false;
                                    //    BenAmountSpeaking = false;
                                    //}
                                    //else if (BenCommentSpeaking)
                                    //{
                                    //    beneficiary_TransferAmount.Text = item.ToLower().Trim();
                                    //    BenCommentSpeaking = false;
                                    //    BenAccNumSpeaking = true;
                                    //    BenAmountSpeaking = false;
                                    //}
                                    //else
                                    //{
                                    //    await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                                    //    VoiceIntentStarter();
                                    //}
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