using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Support.V7.App;
using ConversationalBanking.sampleData;

/*
 * 
 *          RECYLERVIEW STUFF, WILL DEFINE IT LATER.
 * 
 */

namespace ConversationalBanking
{
    [Activity(Label = "Previous Transactions")]
    public class previousTransactions : AppCompatActivity
    {
        ProgressDialog progress;


        // RecyclerView instance that displays the photo album:
        RecyclerView mRecyclerView;

        // Layout manager that lays out each card in the RecyclerView:
        RecyclerView.LayoutManager mLayoutManager;

        // Adapter that accesses the data set (a photo album):
        prevTrans_Adapter mAdapter;

        // Photo album that is managed by the adapter:
        sampleDataRecyclerView prevTrans_rowArray;

        protected override void OnCreate(Bundle bundle)
        {
            models.speakerClass.SpeakerKiller();
            base.OnCreate(bundle);

            progress = new Android.App.ProgressDialog(this);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetCancelable(false);


            if (!dashboard.previousTransactionsFetched)
            {
                progress.SetMessage("Getting The List...");
                progress.Show();

                int temp_count = 0;
                while (!dashboard.previousTransactionsFetched)
                {
                    temp_count += temp_count;
                }
            }

            progress.Hide();

            // Instantiate the photo album:
            prevTrans_rowArray = new sampleDataRecyclerView();

            // Set our view from the "main" layout resource:
            SetContentView(Resource.Layout.previousTransactionsScreen);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.previousTransactions_Toolbar);
            toolbar.SetTitleTextColor(Android.Graphics.Color.White);
            SetSupportActionBar(toolbar);
            // Get our RecyclerView layout:
            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.previousTransaction_RecyclerView);

            //............................................................
            // Layout Manager Setup:

            // Use the built-in linear layout manager:
            mLayoutManager = new LinearLayoutManager(this);

            // Or use the built-in grid layout manager (two horizontal rows):

            // Plug the layout manager into the RecyclerView:
            mRecyclerView.SetLayoutManager(mLayoutManager);

            //............................................................
            // Adapter Setup:

            // Create an adapter for the RecyclerView, and pass it the
            // data set (the photo album) to manage:
            mAdapter = new prevTrans_Adapter(prevTrans_rowArray);

            // Plug the adapter into the RecyclerView:
            mRecyclerView.SetAdapter(mAdapter);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_menu, menu);
            return true;
        }
    }

    //----------------------------------------------------------------------
    // VIEW HOLDER

    // Implement the ViewHolder pattern: each ViewHolder holds references
    // to the UI components (ImageView and TextView) within the CardView 
    // that is displayed in a row of the RecyclerView:
    public class rowViewHolder : RecyclerView.ViewHolder
    {
        public TextView row_txnId { get; private set; }
        public TextView row_amount { get; private set; }
        public TextView row_accNumber { get; private set; }
        public TextView row_remarks { get; private set; }
        public TextView row_date { get; private set; }
        public TextView row_name { get; private set; }

        // Get references to the views defined in the CardView layout.
        public rowViewHolder(View itemView, Action<int> listener)
            : base(itemView)
        {
            // Locate and cache view references:
            row_txnId = itemView.FindViewById<TextView>(Resource.Id.recyclerView_TxnId);
            row_amount = itemView.FindViewById<TextView>(Resource.Id.recyclerView_Amount);
            row_accNumber = itemView.FindViewById<TextView>(Resource.Id.recyclerView_AccountNumber);
            row_remarks = itemView.FindViewById<TextView>(Resource.Id.recyclerView_Remarks);
            row_date = itemView.FindViewById<TextView>(Resource.Id.recyclerView_Date);
            row_name = itemView.FindViewById<TextView>(Resource.Id.recyclerView_AccountName);

            // Detect user clicks on the item view and report which item
            // was clicked (by layout position) to the listener:
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }

    //----------------------------------------------------------------------
    // ADAPTER

    // Adapter to connect the data set (photo album) to the RecyclerView: 
    public class prevTrans_Adapter : RecyclerView.Adapter
    {
        // Event handler for item clicks:
        public event EventHandler<int> ItemClick;

        // Underlying data set (a photo album):
        public sampleDataRecyclerView prevTrans_rowData;

        // Load the adapter with the data set (photo album) at construction time:
        public prevTrans_Adapter(sampleDataRecyclerView rowData)
        {
            prevTrans_rowData = rowData;
        }

        // Create a new photo CardView (invoked by the layout manager): 
        public override RecyclerView.ViewHolder
            OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.customRow, parent, false);

            // Create a ViewHolder to find and hold these view references, and 
            // register OnClick with the view holder:
            rowViewHolder vh = new rowViewHolder(itemView, OnClick);
            return vh;
        }

        // Fill in the contents of the photo card (invoked by the layout manager):
        public override void
            OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            rowViewHolder vh = holder as rowViewHolder;

            // Set the ImageView and TextView in this ViewHolder's CardView 
            // from this position in the photo album:
            vh.row_txnId.Text = prevTrans_rowData[position].txnId;
            vh.row_amount.Text = prevTrans_rowData[position].amount;
            vh.row_accNumber.Text = prevTrans_rowData[position].accNumber;
            vh.row_remarks.Text = prevTrans_rowData[position].remarks;
            vh.row_date.Text = prevTrans_rowData[position].date;
            vh.row_name.Text = prevTrans_rowData[position].prevTrans_accName;
        }

        // Return the number of photos available in the photo album:
        public override int ItemCount
        {
            get { return prevTrans_rowData.numRows; }
        }

        // Raise an event when the item-click takes place:
        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }
    }
}