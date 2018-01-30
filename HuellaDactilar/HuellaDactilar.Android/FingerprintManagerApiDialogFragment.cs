using System;
using Android.App;
using Android.Hardware.Fingerprints;
using Android.OS;
using Android.Support.V4.Hardware.Fingerprint;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Javax.Crypto;
using CancellationSignal = Android.Support.V4.OS.CancellationSignal;
using Res = Android.Resource;
using HuellaDactilar;
using System.Security.Principal;
// ReSharper disable InconsistentNaming
// ReSharper disable UseStringInterpolation
// ReSharper disable UseNullPropagation
using Android.Content;
using System.Threading.Tasks;

namespace HuellaDactilar.Droid
{
    /// <summary>
    ///     This DialogFragment is displayed when the app is scanning for fingerprints.
    /// </summary>
    /// <remarks>
    ///     This DialogFragment doesn't perform any checks to see if the device
    ///     is actually eligible for fingerprint authentication. All of those checks are performed by the
    ///     Activity.
    /// </remarks>
    public class FingerprintManagerApiDialogFragment : DialogFragment
    {
        static readonly string TAG = "X:" + typeof (FingerprintManagerApiDialogFragment).Name;

        Button _cancelButton;
        CancellationSignal _cancellationSignal;
        FingerprintManagerCompat _fingerprintManager;
        bool ScanForFingerprintsInOnResume { get; set; } = true;

        bool UserCancelledScan { get; set; }

        CryptoObjectHelper CryptObjectHelper { get; set; }

        bool IsScanningForFingerprints
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return _cancellationSignal != null; }
        }

        public static FingerprintManagerApiDialogFragment NewInstance(FingerprintManagerCompat fingerprintManager)
        {
            FingerprintManagerApiDialogFragment frag = new FingerprintManagerApiDialogFragment
                                                       {
                                                           _fingerprintManager = fingerprintManager
                                                       };
            return frag;
        }

        public void Init(bool startScanning = true)
        {
            ScanForFingerprintsInOnResume = startScanning;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            CryptObjectHelper = new CryptoObjectHelper();
            SetStyle(DialogFragmentStyle.Normal, Res.Style.ThemeMaterialLightDialog);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Dialog.SetTitle(Resource.String.sign_in);
            

            View v = inflater.Inflate(Resource.Layout.dialog_scanning_for_fingerprint, container, false);

            _cancelButton = v.FindViewById<Button>(Resource.Id.cancel_button);
            _cancelButton.Click += (sender, args) =>
                                   {
                                       UserCancelledScan = true;
                                       StopListeningForFingerprints();
                                   };

            return v;
        }

        public override void OnResume()
        {
            base.OnResume();
            if (!ScanForFingerprintsInOnResume)
            {
                return;
            }

            UserCancelledScan = false;
            _cancellationSignal = new CancellationSignal();
            _fingerprintManager.Authenticate(CryptObjectHelper.BuildCryptoObject(),
                                             (int) FingerprintAuthenticationFlags.None, /* flags */
                                             _cancellationSignal,
                                             new SimpleAuthCallbacks(this),
                                             null);
        }

        public async override void OnPause()
        {
            base.OnPause();
            
            if (IsScanningForFingerprints)
            {
                 StopListeningForFingerprints(true);
            }
        }

      public void StopListeningForFingerprints(bool butStartListeningAgainInOnResume = false)
        {
            if (_cancellationSignal != null)
            {
                _cancellationSignal.Cancel();
                _cancellationSignal = null;
                Log.Debug(TAG, "StopListeningForFingerprints: _cancellationSignal.Cancel();");
            }
            ScanForFingerprintsInOnResume = butStartListeningAgainInOnResume;
        }

        public override void OnDestroyView()
        {
            // see https://code.google.com/p/android/issues/detail?id=17423
            if (Dialog != null && RetainInstance)
            {
                Dialog.SetDismissMessage(null);
            }
            base.OnDestroyView();
        }

        class SimpleAuthCallbacks : FingerprintManagerCompat.AuthenticationCallback
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            static readonly string TAG = "X:" + typeof (SimpleAuthCallbacks).Name;
            static readonly byte[] SECRET_BYTES = {1, 2, 3, 4, 5, 6, 7, 8, 9};
            readonly FingerprintManagerApiDialogFragment _fragment;
            private Context mainActivity;
            int tiempo = 30;
            bool tiempoCorriendo = false;
            public async void tiempoatras()
            {
                for (int i = 0; i < 30; i++)
                {
                    
                    tiempo = --tiempo;
                    await Task.Delay(1000);
                    if(tiempo == 0)
                    {
                        tiempoCorriendo = false;
                        return;
                    }
                }
            }
            public SimpleAuthCallbacks(FingerprintManagerApiDialogFragment frag)
            {
                _fragment = frag;
            }
            public SimpleAuthCallbacks(Context mainActivity)
            {
                this.mainActivity = mainActivity;
            }
            public async override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
            {
                Log.Debug(TAG, "OnAuthenticationSucceeded");
                if (result.CryptoObject.Cipher != null)
                {
                    try
                    {
                        await Xamarin.Forms.Application.Current.MainPage.Navigation.PushAsync(new HuellaDactilar.MenuPrincipal());
                        ReportSuccess();
                    }
                    catch (BadPaddingException bpe)
                    {
                        Log.Error(TAG, "Failed to encrypt the data with the generated key." + bpe);
                        ReportAuthenticationFailed();
                    }
                    catch (IllegalBlockSizeException ibse)
                    {
                        Log.Error(TAG, "Failed to encrypt the data with the generated key." + ibse);
                        ReportAuthenticationFailed();
                    }
                }
                else
                {
                    Log.Debug(TAG, "Fingerprint authentication succeeded.");
                    ReportSuccess();
                }
            }
            
            void ReportSuccess()
            {
                
                FingerprintManagerApiActivity activity = (FingerprintManagerApiActivity) _fragment.Activity;
                activity.AuthenticationSuccessful();
                _fragment.Dismiss();
            }

            void ReportScanFailure(int errMsgId, string errorMessage)
            {
                FingerprintManagerApiActivity activity = (FingerprintManagerApiActivity) _fragment.Activity;
                activity.ShowError(errorMessage, string.Format("Error message id {0}.", errMsgId));
                _fragment.Dismiss();
            }

            void ReportAuthenticationFailed()
            {
                FingerprintManagerApiActivity activity = (FingerprintManagerApiActivity) _fragment.Activity;
                string msg = _fragment.Resources.GetString(Resource.String.authentication_failed_message);
                activity.ShowError(msg);
                _fragment.Dismiss();
            }

            public override void OnAuthenticationError(int errMsgId, ICharSequence errString)
            {
                bool reportError = (errMsgId == (int)FingerprintState.ErrorCanceled) &&
                                   !_fragment.ScanForFingerprintsInOnResume;
                //SE LLAMA CUANDO SE SUPERA LA CANTINDAD DE INTENTOS (5)
                if (errMsgId == (int)FingerprintState.ErrorLockout)
                {
                    Toast.MakeText(_fragment.Context, "Se excedió la cantidad de intentos, intentelo más tarde.", ToastLength.Short).Show();
                    FingerprintManagerApiActivity act = (FingerprintManagerApiActivity)_fragment.Activity;
                    act.OnBackPressed();
                    return;
                }
                string debugMsg = string.Format("OnAuthenticationError: {0}:`{1}`.", errMsgId, errString);

                if (_fragment.UserCancelledScan)
                {
                    FingerprintManagerApiActivity activity = (FingerprintManagerApiActivity)_fragment.Activity;
                    activity.AuthenticationSuccessful();
                    _fragment.Dismiss();
                }
                else if (reportError)
                {
                    ReportScanFailure(errMsgId, errString.ToString());
                    debugMsg += " Reporting the error.";
                }
                else
                {
                }
                Log.Debug(TAG, debugMsg);

            }
            public override void OnAuthenticationFailed()
            {
                _fragment.Dismiss();
                Toast.MakeText(_fragment.Context, "Error al validar la huella dactilar, vuelva a intentar.", ToastLength.Short).Show();
                _fragment.Show(_fragment.FragmentManager, "fingerprint_auth_fragment");
            }

            public async override void OnAuthenticationHelp(int helpMsgId, ICharSequence helpString)
            {
                //NO RECONOCIÓ LA HUELLA POR QUE SE PUSO MUY RÁPIDO
            }
        }
    }
}