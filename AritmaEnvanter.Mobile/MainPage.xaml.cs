using System.Diagnostics;

namespace AritmaEnvanter.Mobile;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Platform-özel WebView ayarlarını (Handler hazır olduğunda) yapalım
        AritmaWebView.HandlerChanged += (s, e) =>
        {
#if ANDROID
            if (AritmaWebView.Handler?.PlatformView is Android.Webkit.WebView nativeWebView)
            {
                nativeWebView.Settings.DomStorageEnabled = true;
                nativeWebView.Settings.JavaScriptEnabled = true;
                nativeWebView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;
                
                var cookieManager = Android.Webkit.CookieManager.Instance;
                if (cookieManager != null)
                {
                    cookieManager.SetAcceptCookie(true);
                    cookieManager.SetAcceptThirdPartyCookies(nativeWebView, true);
                }
            }
#endif
        };
    }

    private void AritmaWebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[WebView] Navigating to: {e.Url}");
    }

    private void AritmaWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[WebView] Navigated to: {e.Url} | Result: {e.Result}");
    }
}
