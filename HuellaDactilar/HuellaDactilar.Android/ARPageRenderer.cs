using Android.App;
using Android.Content;
using HuellaDactilar.Droid;
using HuellaDactilar;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ARPage), typeof(HuellaDactilar.Droid.ARPageRenderer))]
namespace HuellaDactilar.Droid
{
    class ARPageRenderer : PageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            var page = e.NewElement as ARPage;
            var activity = this.Context as Activity;
            var Fingerprint = new Intent(activity, typeof(FingerprintManagerApiActivity));

            Fingerprint.SetFlags(ActivityFlags.NoHistory);
            activity.StartActivity(Fingerprint);
            activity.OnBackPressed();
        }
        public void closeAct()
        {
            var activity = this.Context as Activity;
            activity.FinishActivity(0);
        }
    }
}