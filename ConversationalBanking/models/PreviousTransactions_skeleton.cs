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
    class PreviousTransactions_skeleton
    {
        public static List<string> t_id { get; set; } = new List<string>();
        public static List<string> t_sender { get; set; } = new List<string>();
        public static List<string> t_receiver { get; set; } = new List<string>();
        public static List<string> t_amount { get; set; } = new List<string>();
        public static List<string> t_time { get; set; } = new List<string>();
        public static List<string> t_remark { get; set; } = new List<string>();
        public static List<string> t_type { get; set; } = new List<string>();
        public static List<string> t_receiver_name { get; set; } = new List<string>();
    }
}