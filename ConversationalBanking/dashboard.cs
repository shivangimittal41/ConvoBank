
using Android.App;
using Android.Views;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Content.Res;
using ConversationalBanking.Fragments;
using Android.Widget;
using System;
using Android.Support.V7.Widget;
using Android.Content;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;
using System.Globalization;
using Android.Speech;

/*
 *          Variables used:
 *          1. dashboard_ProfileName - FOR SETTING USERNAME IN DASHBOARD DRAWER. (SET IN LINE 126)
 *          2. dashboard_availableBalance
 *
 */


namespace ConversationalBanking
{
    [Activity(Label = "Dashboard", MainLauncher = false, NoHistory = false)]
    public class dashboard : AppCompatActivity
    {
        public bool PreviousTransaction = false;
        public string message;
        public static bool previousTransactionsFetched = false;
        //static List<models.PreviousTransactions_skeleton> previousData = new List<models.PreviousTransactions_skeleton>();

        private readonly int VOICE = 10;
        
        //Declaring Variables to access throught this activity  
        DrawerLayout drawerLayout;
        NavigationView navigationView;
        IMenuItem previousItem;
        Android.Support.V7.App.ActionBarDrawerToggle toggle;
        FrameLayout mainDashboardLayout;
        TextView dashboard_ProfileName, dashboard_availableBalance, dashboard_LogoutText;
        View headerView;
        CardView fundTransfer, billPayment, prevTransaction, accDetails, newServices;
        CoordinatorLayout dashboard_ParentLayout;
        ImageView dashboard_LogoutImage;


        ProgressDialog progress;


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

        protected override void OnRestart()
        {
            base.OnRestart();
            dashboard_availableBalance.Text = models.LoggedInUserData.u_amount;

            // We will fetch updated account details and previous transaction list again.
            new Thread(new ThreadStart(delegate {

                RunOnUiThread(async () => await Task.Run(() => PreviousTransactionFetcher()));

            })).Start();

            new Thread(new ThreadStart(delegate {

                RunOnUiThread(async () => await Task.Run(() => SignInPoster()));

            })).Start();
        }
        
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.mainDashboardDrawer);

            //Everytime this page re-appears, we will fetch latest data. So, keep this false.
            previousTransactionsFetched = false;

            DrawerFunctionality();
            FindViews();
            ClickEvents();

            progress = new ProgressDialog(this)
            {
                Indeterminate = true
            };
            progress.SetProgressStyle(Android.App.ProgressDialogStyle.Spinner);
            progress.SetCancelable(false);

            //Console.WriteLine("Fetching The List");
            ////await Task.Run(() => PreviousTransactionFetcher());
            //new Thread(new ThreadStart(delegate {

            //    RunOnUiThread(async () => await Task.Run(() => PreviousTransactionFetcher()));

            //})).Start();
            //Console.WriteLine("Fetched The List");

            dashboard_availableBalance.Text = models.LoggedInUserData.u_amount;
            

        }

        protected async override void OnResume()
        {
            base.OnResume();
            // Announce the services available to the user.
            // If the dashboard is opened once, we greet, otherwise, we say welcome back to dashboard.
            if (models.LoggedInUserData.u_loggedin)
            {
                await models.speakerClass.SpeakerAsync("Hello " + models.LoggedInUserData.u_name);
                await models.speakerClass.SpeakerAsync("What would you like to do?");
                models.LoggedInUserData.u_loggedin = false;
            }
            else
            {
                await models.speakerClass.SpeakerAsync("Welcome back to your dashboard.");
                await models.speakerClass.SpeakerAsync("What would you like to do?");
            }
            await models.speakerClass.SpeakerAsync("Funds Transfer");
            await models.speakerClass.SpeakerAsync("Electricity Bill Payment");
            await models.speakerClass.SpeakerAsync("Check Previous Transactions");
            await models.speakerClass.SpeakerAsync("Check Your Account Information");

            // Start the voice activity Intent
            VoiceIntentStarter();
        }

        private void FindViews()
        {
            dashboard_ParentLayout = FindViewById<CoordinatorLayout>(Resource.Id.dashboard_ParentLayout);
            fundTransfer = FindViewById<CardView>(Resource.Id.mainService_FundTransfer);
            billPayment = FindViewById<CardView>(Resource.Id.mainService_BillPayment);
            prevTransaction = FindViewById<CardView>(Resource.Id.mainService_PrevTransaction);
            accDetails = FindViewById<CardView>(Resource.Id.mainService_AccountDetails);
            newServices = FindViewById<CardView>(Resource.Id.mainService_NewServices);
            dashboard_availableBalance = FindViewById<TextView>(Resource.Id.dashboard_availableBalance);
            dashboard_LogoutImage = FindViewById<ImageView>(Resource.Id.drawer_LogoutImage);
            dashboard_LogoutText = FindViewById<TextView>(Resource.Id.drawer_LogoutText);
        }

        private void ClickEvents()
        {
            fundTransfer.Click += FundTransfer_Click;
            billPayment.Click += BillPayment_Click;
            prevTransaction.Click += PrevTransaction_Click;
            accDetails.Click += AccDetails_Click;
            newServices.Click += NewServices_Click;
            dashboard_LogoutText.Click += Dashboard_Logout;
            dashboard_LogoutImage.Click += Dashboard_Logout;
        }

        private async void Dashboard_Logout(object sender, EventArgs e)
        {
            //Snackbar.Make(dashboard_ParentLayout, "Logout Initiated", Snackbar.LengthLong).Show();
            await models.speakerClass.SpeakerAsync("Signing Out. Please Wait.");
            progress.SetMessage("Signing Out...Please Wait");
            progress.Show();
            Task.Delay(5000);
            progress.Hide();

            // This will end the activity properly.
            //base.OnBackPressed();
            Finish();
        }

        private void NewServices_Click(object sender, EventArgs e)
        {
            //Toast.MakeText(this, "Upcoming Services", ToastLength.Short).Show();
            Snackbar.Make(dashboard_ParentLayout, "Upcoming Services", Snackbar.LengthLong).Show();
        }

        private async void AccDetails_Click(object sender, EventArgs e)
        {
            //Toast.MakeText(this, "Account Details", ToastLength.Short).Show();
            await models.speakerClass.SpeakerAsync("Opening Account Details.");
            var intentAccountDetails = new Intent(this, typeof(accountDetails));
            StartActivity(intentAccountDetails);
        }

        private async void PrevTransaction_Click(object sender, EventArgs e)
        {
            //Toast.MakeText(this, "Previous Transactions", ToastLength.Short).Show();
            await models.speakerClass.SpeakerAsync("Opening Previous Transactions.");
            var intentPreviousTransactions = new Intent(this, typeof(previousTransactions));
            StartActivity(intentPreviousTransactions);
        }

        private async void BillPayment_Click(object sender, EventArgs e)
        {
            //Toast.MakeText(this, "Bill Payment", ToastLength.Short).Show();
            await models.speakerClass.SpeakerAsync("Opening Bill Payments.");
            var intentBillPayment = new Intent(this, typeof(billPayment));
            StartActivity(intentBillPayment);

        }

        private async void FundTransfer_Click(object sender, EventArgs e)
        {
            //Toast.MakeText(this, "Fund Transfer", ToastLength.Short).Show();
            await models.speakerClass.SpeakerAsync("Opening Transfer Funds Window.");
            var intentFundTransfers = new Intent(this, typeof(fundTransfers));
            StartActivity(intentFundTransfers);
        }

        private void DrawerFunctionality()
        {
            //Customising Toolbar
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.dashboard_toolbar);
            toolbar.SetTitle(Resource.String.mainDashboardTitle);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);
            //For showing back button  
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            //setting Hamburger icon Here  
            //SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.microphone_off);
            //Getting Drawer Layout declared in UI and handling closing and open events  
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawerLayout.DrawerOpened += DrawerLayout_DrawerOpened;
            drawerLayout.DrawerClosed += DrawerLayout_DrawerClosed;
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            headerView = navigationView.GetHeaderView(0);
            dashboard_ProfileName = headerView.FindViewById<TextView>(Resource.Id.dashboardUser_Name);
            mainDashboardLayout = FindViewById<FrameLayout>(Resource.Id.dashboard_MainContentFrame);


            // Use this Line to set the Name of USER in Dashboard Drawer, 
            // Replace GetString Method with your Own.
            dashboard_ProfileName.Text = models.LoggedInUserData.u_name;



            toggle = new ActionBarDrawerToggle
            (
                    this,
                    drawerLayout,
                    //drawerLayout,
                    Resource.String.openDrawer,
                    Resource.String.closeDrawer
            );
            drawerLayout.AddDrawerListener(toggle);
            //Synchronize the state of the drawer indicator/affordance with the linked DrawerLayout  
            toggle.SyncState();
            //Handling click events on Menu items  
            navigationView.NavigationItemSelected += (sender, e) =>
            {

                if (previousItem != null)
                    previousItem.SetChecked(false);

                navigationView.SetCheckedItem(e.MenuItem.ItemId);

                previousItem = e.MenuItem;

                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_home:
                        ListItemClicked(0);
                        break;

                    case Resource.Id.nav_profileDetails:
                        ListItemClicked(1);
                        break;
                }
                drawerLayout.CloseDrawers();
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }

        private void DrawerLayout_DrawerClosed(object sender, DrawerLayout.DrawerClosedEventArgs e)
        {
            //SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.microphone);
        }

        private void DrawerLayout_DrawerOpened(object sender, DrawerLayout.DrawerOpenedEventArgs e)
        {
            //SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.microphone_off);
        }

        private void ListItemClicked(int position)
        {
            Android.Support.V4.App.Fragment fragment = null;
            switch (position)
            {
                case 0:
                    //fragment = new profileDetails();
                    //fragment = new mainDashboard();
                    //mainDashboardLayout.Visibility = ViewStates.Visible;
                    //fragment = new mainDashboard();
                    break;
                case 1:
                    //fragment = new profileDetails();
                    break;
            }
            if (fragment != null)
            {
                SupportFragmentManager.BeginTransaction()
                               .Replace(Resource.Id.dashboard_MainContentFrame, fragment)
                               .Commit();
            }
        }

        //Handling Back Key Press  
        public async override void OnBackPressed()
        {
            if (drawerLayout.IsDrawerOpen((int)GravityFlags.Start))
            {
                drawerLayout.CloseDrawer((int)GravityFlags.Start);

            }
            else
            {
                await models.speakerClass.SpeakerAsync("Logging You Out. Please Wait.");
                models.speakerClass.SpeakerKiller();
                progress.SetMessage("Signing Out...Please Wait");
                progress.Show();
                await Task.Delay(5000);
                progress.Hide();

                //base.OnBackPressed();
                Finish();
            }

        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;
                case Android.Resource.Id.CloseButton:
                    //Toast.MakeText(this, "BACKBUTTON", ToastLength.Short).Show();
                    Snackbar.Make(dashboard_ParentLayout, "BACKBUTTON", Snackbar.LengthLong).Show();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        //Resposnible for mainting state,suppose if you suddenly rotated screen than drawer should not losse it context so you have save drawer states like below  
        protected override void OnPostCreate(Bundle savedInstanceState)
        {

            base.OnPostCreate(savedInstanceState);
            toggle.SyncState();

        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            toggle.OnConfigurationChanged(newConfig);
        }

        private async Task PreviousTransactionFetcher()
        {
            loginScreen._client.BaseAddress = new System.Uri(MainActivity.primaryDomain);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("user_account_number", models.LoggedInUserData.u_acc_number)
                });
            var result = await loginScreen._client.PostAsync("/conversational_api/account_details/previous_transactions.php", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            try
            {
                dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent);

                if (obj2.error_code == 1)
                {
                    await models.speakerClass.SpeakerAsync("Sign Up Failed, Please try again.");
                    Console.WriteLine("Sign Up Failed : " + obj2.message);
                    this.message = obj2.message;
                    this.PreviousTransaction = false;
                }
                else
                {
                    this.message = obj2.message;
                    this.PreviousTransaction = true;

                    dynamic dynObj = JsonConvert.DeserializeObject(Convert.ToString(obj2.records));
                    //Console.WriteLine("Records : " + Convert.ToString(dynObj));


                    // Need to clear the lists every time we're about to populate it.
                    // Otherwise, old data + new data = bahut sara repeated data.
                    models.PreviousTransactions_skeleton.t_id.Clear();
                    models.PreviousTransactions_skeleton.t_amount.Clear();
                    models.PreviousTransactions_skeleton.t_receiver.Clear();
                    models.PreviousTransactions_skeleton.t_remark.Clear();
                    models.PreviousTransactions_skeleton.t_time.Clear();
                    models.PreviousTransactions_skeleton.t_receiver_name.Clear();

                    foreach (var data in dynObj)
                    {
                        models.PreviousTransactions_skeleton.t_id.Add(Convert.ToString(data.t_id));
                        models.PreviousTransactions_skeleton.t_amount.Add(Convert.ToString(data.t_amount));
                        models.PreviousTransactions_skeleton.t_receiver.Add(Convert.ToString(data.t_receiver));
                        models.PreviousTransactions_skeleton.t_remark.Add(Convert.ToString(data.t_remark));
                        string temp_date = Convert.ToString(data.t_time);
                        DateTime d = DateTime.ParseExact(temp_date.Split(null)[0].Trim().Replace("-", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
                        models.PreviousTransactions_skeleton.t_time.Add(d.ToString("dd/MM/yyyy"));
                        models.PreviousTransactions_skeleton.t_receiver_name.Add(Convert.ToString(data.beneficiary_name));
                    }

                    // Entries have been fetched/refreshed. We have new data, we can show it to the users.
                    previousTransactionsFetched = true;

                }
            }
            catch (Exception SignInException)
            {
                Console.WriteLine("SignInException : " + SignInException);
                throw;
            }
        }

        private async Task SignInPoster()
        {
            loginScreen._client.BaseAddress = new System.Uri(MainActivity.primaryDomain);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("user_cust_id", models.LoggedInUserData.u_ca_number),
                    new KeyValuePair<string, string>("user_password", models.LoggedInUserData.u_pass),
                    new KeyValuePair<string, string>("user_device_id", models.LoggedInUserData.u_device_id)
                });
            var result = await loginScreen._client.PostAsync("/conversational_api/registration/user_login.php", content);
            string resultContent = await result.Content.ReadAsStringAsync();

            try
            {
                dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent);

                if (obj2.error_code == 1)
                {
                    Console.WriteLine("Sign In Failed : " + obj2.message);
                    return;
                }
                else
                {
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
                    return;
                }
            }
            catch (Exception SignInException)
            {
                Console.WriteLine("SignInException : " + SignInException);
                throw;
            }
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
                                if (item.Replace(" ", "").ToLower().Contains("funds"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    fundTransfer.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("transfer"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    fundTransfer.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("electricity"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("bill"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    billPayment.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("previous"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    prevTransaction.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("transactions"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    prevTransaction.PerformClick();
                                    break;
                                }
                                else if (item.Replace(" ", "").ToLower().Contains("account"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    accDetails.PerformClick();
                                    break;
                                }
                                else if (item.ToLower().Contains("details"))
                                {
                                    models.speakerClass.SpeakerKiller();
                                    accDetails.PerformClick();
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