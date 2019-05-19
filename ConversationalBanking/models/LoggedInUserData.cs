using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ConversationalBanking.models
{
    class LoggedInUserData
    {
        public static string u_id { get; set; }
        public static string u_session_token { get; set; }
        public static string u_acc_number { get; set; }
        public static string u_name { get; set; }
        public static string u_phone { get; set; }
        public static string u_address { get; set; }
        public static string u_dob { get; set; }
        public static string u_password { get; set; }
        public static string u_bank_pin { get; set; }
        public static string u_amount { get; set; }
        public static string u_creation_date { get; set; }
        public static string u_ifsc_code { get; set; }
        public static string u_pan { get; set; }
        public static string u_uidai { get; set; }
        public static string u_email { get; set; }
        public static string u_photo { get; set; }
        public static string u_status { get; set; }
        public static string u_account_type { get; set; }
        public static string u_country { get; set; }
        public static string u_country_code { get; set; }
        public static string u_state { get; set; }
        public static string u_device_id { get; set; }
        public static string u_ca_number { get; set; }
        public static string u_pass { get; set; }
        public static bool u_loggedin { get; set; } = true;
    }
}