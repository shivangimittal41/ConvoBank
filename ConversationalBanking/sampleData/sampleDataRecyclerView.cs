using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.Widget;
using System.Collections.Generic;

namespace ConversationalBanking.sampleData
{
    // Photo: contains image resource ID and caption:
    public class prev_Transactions
    {
        // Caption text for this photo:
        public string prevTrans_txnId;
        public string prevTrans_amount;
        public string prevTrans_accNumber;
        public string prevTrans_remarks;
        public string prevTrans_date;
        public string prevTrans_accName;


        // Return the Caption of the photo:
        public string txnId
        {
            get { return prevTrans_txnId; }
        }

        public string amount
        {
            get { return prevTrans_amount; }
        }

        public string accNumber
        {
            get { return prevTrans_accNumber; }
        }

        public string remarks
        {
            get { return prevTrans_remarks; }
        }

        public string date
        {
            get { return prevTrans_date; }
        }

        public string name
        {
            get { return prevTrans_accName; }
        }

    }

    // Photo album: holds image resource IDs and caption:
    public class sampleDataRecyclerView
    {
        // Built-in photo collection - this could be replaced with
        // a photo database:
        List<prev_Transactions> prevTransactionData = new List<prev_Transactions>();

        // Array of photos that make up the album:
        private prev_Transactions[] prevTrans_Data;

        public sampleDataRecyclerView()
        {

            List<string> t_id_list = models.PreviousTransactions_skeleton.t_id;
            List<string> t_amount_list = models.PreviousTransactions_skeleton.t_amount;
            List<string> t_receiver_list = models.PreviousTransactions_skeleton.t_receiver;
            List<string> t_remark_list = models.PreviousTransactions_skeleton.t_remark;
            List<string> t_time_list = models.PreviousTransactions_skeleton.t_time;
            List<string> t_receiver_name_list = models.PreviousTransactions_skeleton.t_receiver_name;


            for (int i = 0; i < t_id_list.Count; i++)
            {
                prevTransactionData.Add(new prev_Transactions { prevTrans_txnId = Convert.ToString(t_id_list[i]),
                                                                prevTrans_amount = Convert.ToString(t_amount_list[i]),
                                                                prevTrans_accNumber = Convert.ToString(t_receiver_list[i]),
                                                                prevTrans_remarks = Convert.ToString(t_remark_list[i]),
                                                                prevTrans_date = Convert.ToString(t_time_list[i]),
                                                                prevTrans_accName = Convert.ToString(t_receiver_name_list[i]),
                });
            }
            prevTrans_Data = prevTransactionData.ToArray();
        }

        // Return the number of photos in the photo album:
        public int numRows
        {
            get { return prevTrans_Data.Length; }
        }

        // Indexer (read only) for accessing a photo:
        public prev_Transactions this[int i]
        {
            get { return prevTrans_Data[i]; }
        }
    }
}