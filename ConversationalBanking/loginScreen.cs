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
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Android.Speech;
using System.Globalization;

namespace ConversationalBanking
{
    [Activity(Label = "ConvoBank", NoHistory = true)]
    public class loginScreen : AppCompatActivity
    {
        private string message;
        private bool signUpStatus;
        private bool SignInStatus = false;
        public string u_session_token, u_cust_id, u_acc_number, u_name, u_phone,
                      u_address, u_dob, u_password, u_bank_pin, u_amount, u_creation_date,
                      u_ifsc_code, u_pan, u_uidai, u_email, u_photo, u_status, u_account_type,
                      u_country, u_country_code, u_state;

        private bool custIdSpeaking = true, passwordSpeaking = false;

        public static HttpClient _client = new HttpClient();

        ProgressDialog progress;
        private readonly int VOICE = 10;
        EditText login_CustId, login_Password;
        Button loginButton;
        TextView signup;
        CoordinatorLayout parentLayout;

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
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.loginScreen);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.loginToolbar);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);

            FindViews();
            ClickEvents();

            progress = new Android.App.ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetCancelable(false);

            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                var alert = new Android.App.AlertDialog.Builder(this);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    return;
                });
                alert.Show();
            }

            await models.speakerClass.SpeakerAsync("Please speak your customer ID.");
            VoiceIntentStarter();
            await Task.Delay(5000);
            await models.speakerClass.SpeakerAsync("Please speak your password.");
            VoiceIntentStarter();

            //login_CustId.Text = "32456789";
            //login_Password.Text = "lol";

        }

        private void FindViews()
        {
            parentLayout = FindViewById<CoordinatorLayout>(Resource.Id.loginScreen_ParentLayout);
            login_CustId = FindViewById<EditText>(Resource.Id.login_CustomerId);
            login_Password = FindViewById<EditText>(Resource.Id.login_CustomerPassword);
            loginButton = FindViewById<Button>(Resource.Id.login_LoginButton);
            signup = FindViewById<TextView>(Resource.Id.login_SignupButton);
        }

        private void ClickEvents()
        {
            loginButton.Click += LoginButton_ClickAsync;
            signup.Click += Signup_Click;

            }

        private async void Signup_Click(object sender, EventArgs e)
        {
            //Toast.MakeText(this, "Locating Nearest Branch", ToastLength.Short).Show();
            await models.speakerClass.SpeakerAsync("Locating nearest branch.");
            Snackbar.Make(parentLayout, "Locating Nearest Branch", Snackbar.LengthLong).Show();

        }

        private async void LoginButton_ClickAsync(object sender, EventArgs e)
        {
            

            string device_id = (string) Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId);
           

            if (String.IsNullOrEmpty(login_CustId.Text) || String.IsNullOrEmpty(login_Password.Text))
            {
                //Toast.MakeText(this, "Empty Fields", ToastLength.Short).Show();
                await models.speakerClass.SpeakerAsync("Empty Fields.");
                Snackbar.Make(parentLayout, "Empty Fields", Snackbar.LengthLong).Show();
                return;
            }
            else
            {
                if (string.IsNullOrEmpty(device_id))
                    device_id = "Device Not Found";
                models.LoggedInUserData.u_device_id= device_id;

                Console.WriteLine("device_id fetch " + models.LoggedInUserData.u_device_id);

                //await models.speakerClass.SpeakerAsync("Your entered customer id is" + u_cust_id.Aggregate(string.Empty, (c, i) => c + i + "  "));
                //await models.speakerClass.SpeakerAsync("Entered password is " + u_password);
                //await models.speakerClass.SpeakerAsync("Say ok to confirm");
                //VoiceIntentStarter();

                new Thread(new ThreadStart(delegate {

                    RunOnUiThread(async () => await models.speakerClass.SpeakerAsync("Please wait while we verify you."));

                })).Start();
                
                progress.SetMessage("Logging In...");
                progress.Show();
                await SignInPoster(device_id);
                progress.Hide();

                if (this.SignInStatus)
                {
                    await models.speakerClass.SpeakerAsync("Login Successful");
                    new Thread(new ThreadStart(delegate {
                              
                        RunOnUiThread(() => Snackbar.Make(parentLayout, "Login Successful!", Snackbar.LengthLong).Show());

                    })).Start();

                    models.LoggedInUserData.u_pass = login_Password.Text;
                    var intentDashboard = new Intent(this, typeof(dashboard));
                    StartActivity(intentDashboard);
                   
                }
                else
                {
                    login_CustId.Text = null;
                    login_Password.Text = null;
                    //Toast.MakeText(this, "Please Enter proper credentials.", ToastLength.Long).Show();
                    await models.speakerClass.SpeakerAsync("Please Enter Proper Credentials");
                    Snackbar.Make(parentLayout, "Please Enter proper Credentials.", Snackbar.LengthLong).Show();
                }
                
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }
        private async Task SignInPoster(string device_id)
        {
            _client.BaseAddress = new System.Uri(MainActivity.primaryDomain);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("user_cust_id", login_CustId.Text),
                    new KeyValuePair<string, string>("user_password", login_Password.Text),
                    new KeyValuePair<string, string>("user_device_id", device_id )
                });
            var result = await _client.PostAsync("/conversational_api/registration/user_login.php", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            try
            {
                dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent);

                if (obj2.error_code == 1)
                {
                    await models.speakerClass.SpeakerAsync("Sign in failed. Please try again.");
                    Console.WriteLine("Sign In Failed : " + obj2.message);
                    this.message = obj2.message;
                    this.SignInStatus = false;
                }
                else
                {
                    this.message = obj2.message;
                    this.SignInStatus = true;
                    models.LoggedInUserData.u_session_token = (string)obj2.u_session_token;
                    models.LoggedInUserData.u_id = (string)obj2.u_cust_id;
                    models.LoggedInUserData.u_acc_number = (string)obj2.u_acc_number;
                    models.LoggedInUserData.u_name = (string)obj2.u_name;
                    models.LoggedInUserData.u_phone = (string)obj2.u_phone;
                    models.LoggedInUserData.u_address = (string)obj2.u_address;
                    models.LoggedInUserData.u_dob = (string)obj2.u_dob;
                    models.LoggedInUserData.u_password = (string)obj2.u_password;
                    models.LoggedInUserData.u_bank_pin = (string)obj2.u_bank_pin;
                    models.LoggedInUserData.u_amount = (string)obj2.u_amount;
                    models.LoggedInUserData.u_creation_date = (string)obj2.u_creation_date;
                    models.LoggedInUserData.u_ifsc_code = (string)obj2.u_ifsc_code;
                    models.LoggedInUserData.u_pan = (string)obj2.u_pan;
                    models.LoggedInUserData.u_uidai = (string)obj2.u_uidai;
                    models.LoggedInUserData.u_email = (string)obj2.u_email;
                    models.LoggedInUserData.u_photo = (string)obj2.u_photo;
                    models.LoggedInUserData.u_status = (string)obj2.u_status;
                    models.LoggedInUserData.u_account_type = (string)obj2.u_account_type;
                    models.LoggedInUserData.u_country_code = (string)obj2.u_country_code;
                    models.LoggedInUserData.u_country = (string)obj2.u_country;
                    models.LoggedInUserData.u_state = (string)obj2.u_state;
                    models.LoggedInUserData.u_ca_number = (string)obj2.u_ca_number;

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
                                if (item.Replace(" ", "").ToLower().Contains("ok"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    loginButton.PerformClick();
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
                                    if (custIdSpeaking)
                                    {
                                        login_CustId.Text = item.ToLower().Trim();
                                        custIdSpeaking = false;
                                        passwordSpeaking = true;
                                    }
                                    else if (passwordSpeaking)
                                    {
                                        login_Password.Text = item.ToLower().Trim();
                                        custIdSpeaking = true;
                                        passwordSpeaking = false;
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
                Console.WriteLine("My Errur : " + ex);
                await models.speakerClass.SpeakerAsync("I could not understand what you said. Please Speak Again.");
                VoiceIntentStarter();
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }

    }
}