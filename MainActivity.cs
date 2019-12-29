using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace Electra_Remote
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        Button powerOn;
        bool powerState = false;
        public static Android.Hardware.ConsumerIrManager mCIR;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            powerOn = FindViewById<Button>(Resource.Id.btnPower);
            powerOn.Click += OnClickPowerOn;

            // Get a reference to the ConsumerIrManager
            mCIR = (ConsumerIrManager)GetSystemService(Context.ConsumerIrService);

            if (!mCIR.HasIrEmitter)
            {
                Toast.MakeText(ApplicationContext, "No IR Emitter found!", ToastLength.Long).Show();
                return;
            }

            var sb = new StringBuilder();
            // Get the available carrier frequency ranges
            ConsumerIrManager.CarrierFrequencyRange[] freqs = mCIR.GetCarrierFrequencies();
            sb.Append("IR Carrier Frequencies:\n");
            foreach (var range in freqs)
            {
                sb.Append($"{range.MinFrequency} - {range.MaxFrequency}\n");
            }

            Toast.MakeText(ApplicationContext, $"{sb.ToString()}", ToastLength.Long).Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void tryVibrate(int ms)
        {
            try
            {
                // Use default vibration length
                Vibration.Vibrate();

                // Or use specified time
                var duration = TimeSpan.FromMilliseconds(ms);
                Vibration.Vibrate(duration);
            }
            catch (FeatureNotSupportedException ex)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }
        public void OnClickPowerOn(object sender, EventArgs e)
        {
            powerState ^= true;
            powerOn.Text = $"POWER ({((powerState == true) ? "ON" : "OFF")})";
            //int[] patt = { 0xBF, 0x40, 0xe0, 0xe0 };
            //mCIR.Transmit(37900, patt);
            //int[] samsung = { 0x00a9 * 25, 0x00a8 * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0040 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x003f * 25, 0x0015 * 25, 0x0702 * 25, 0x00a9 * 25, 0x00a8 * 25, 0x0015 * 25, 0x0015 * 25, 0x0015 * 25, 0x0e6e * 25 }; ;

            int[] electraPowerPatt = {2979, 3875, 1979, 1916, 2000,  937,
                1000,  937, 1020,  937, 1020,  937,
                1000,  958, 1000,  937, 1020,  937,
                1020,  937, 1000,  937, 1020, 1916,
                1979,  958, 1000,  937, 1020,  937,
                1000,  958, 1000,  958, 1000,  937,
                1000,  958, 1000,  958, 1000,  958,
                979,  958, 1000,  958, 1000,  958,
                1000,  958,  979,  958, 1000,  958,
                1000,  958, 1000,  937, 1000,  958,
                2958, 3875, 1979, 1937, 1979,  937,
                1000,  958, 1000,  958, 1000,  937,
                1000,  958, 1000,  958, 1000,  958,
                 979,  958, 1000,  958, 1000, 1937,
                1958,  958, 1000,  958, 1000,  958,
                 979,  958, 1000,  958, 1000,  958,
                 979,  979,  979,  958, 1000,  958,
                 979,  979,  979,  979,  979,  958,
                1000,  958,  979,  979,  979,  958,
                1000,  958,  979,  979,  979,  979,
                2916, 3916, 1958, 1937, 1958,  979,
                 979,  979,  979,  958, 1000,  958,
                 979,  979,  979,  958, 1000,  958,
                1000,  958,  979,  979,  979, 1937,
                1979,  958,  979,  958, 1000,  958,
                1000,  958,  979,  979,  979,  958,
                1000,  958, 1000,  958,  979,  958,
                1000,  958, 1000,  958,  979,  979,
                 979,  958, 1000,  958,  979,  979,
                 979,  979,  979,  958,  979,  979,
                3916,  979, 2979, 3854, 2000, 1916,
                1979,  937, 1020,  937, 1000,  958,
                1000,  937, 1020,  937, 1020,  937,
                1000,  958, 1000,  937, 1020,  937,
                1000, 1937, 1979,  937, 1020,  937,
                1000,  958, 1000,  958, 1000,  937,
                1000,  958, 1000,  958,  979,  958,
                1000,  958, 1000,  958, 1000,  958,
                 979,  958, 1000,  958, 1000,  958,
                1000,  958,  979,  958, 1000,  958,
                1000,  958, 2937, 3895, 1979, 1916,
                1979,  958, 1000,  958, 1000,  937,
                1000,  958, 1000,  958, 1000,  937,
                1000,  958, 1000,  958, 1000,  958,
                 979, 1937, 1979,  958,  979,  979,
                 979,  958, 1000,  958, 1000,  958,
                 979,  979,  979,  958, 1000,  958,
                 979,  979,  979,  958, 1000,  958,
                 979,  979,  979,  979,  979,  958,
                 979,  979,  979,  979,  979,  979,
                 979,  958, 2937, 3895, 1958, 1958,
                1958,  958, 1000,  958,  979,  979,
                 979,  979,  979,  958, 1000,  958,
                 979,  979,  979,  979,  979,  958,
                1000, 1937, 1958,  958, 1000,  958,
                1000,  958,  979,  979,  979,  958,
                1000,  958,  979,  979,  979,  958,
                1000,  958, 1000,  958,  979,  979,
                 979,  958, 1000,  958,  979,  979,
                 979,  958, 1000,  958,  979,  979,
                 979,  979, 3895};

            int[] electraPowPatt2 = {2979, 3875, 1979, 1916, 1020,  937,
1000,  937, 2000,  937, 1020,  937,
1000,  958, 1000,  937, 1020,  937,
1020, 1916, 1979, 1916, 1979,  958,
1000,  937, 1020,  937, 1020,  937,
1000,  958, 1000,  937, 1020,  937,
1000,  958, 1000,  958,  979,  958,
1000,  958, 1000,  958, 1000,  937,
1000,  958, 1000,  958, 1000,  958,
1000,  937, 1000,  958, 2958, 3875,
1979, 1937, 1000,  937, 1000,  958,
1979,  958, 1000,  937, 1000,  958,
1000,  958, 1000,  958, 1000, 1916,
1979, 1937, 1958,  958, 1000,  958,
1000,  958, 1000,  937, 1000,  958,
1000,  958, 1000,  958,  979,  958,
1000,  958, 1000,  958,  979,  979,
 979,  958, 1000,  958,  979,  979,
 979,  958, 1000,  958,  979,  979,
 979,  979, 2937, 3895, 1958, 1937,
 979,  979,  979,  979, 1958,  958,
1000,  958,  979,  979,  979,  958,
1000,  958, 1000, 1937, 1958, 1937,
1958,  979,  979,  958, 1000,  958,
1000,  958,  979,  979,  979,  958,
1000,  958, 1000,  958,  979,  958,
1000,  958, 1000,  958,  979,  979,
 979,  958, 1000,  958, 1000,  958,
 979,  979,  979,  958, 1000,  958,
3916};
            //mCIR.Transmit(38000, electraPowPatt2);
            //return;
            
            Toast.MakeText(ApplicationContext, $"Now powering {((powerState == true) ? "ON" : "OFF")} your AC!", ToastLength.Long).Show();

            if (powerState == true) tryVibrate(100);

            IRelectra electra = new IRelectra();
            electra.sendElectra(powerState, IRelectra.IRElectraMode.IRElectraModeCool, IRelectra.IRElectraFan.IRElectraFanLow, 18, false, IRelectra.IRElectraIFeel.Off, false, false);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

