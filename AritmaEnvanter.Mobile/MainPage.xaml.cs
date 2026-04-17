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

        // Tünel baypas parametresini her navigasyonda koruyalım
        if (e.Url.Contains("devtunnels.ms") && !e.Url.Contains("X-Free-Tunnel-Bypass=true"))
        {
            e.Cancel = true; // Mevcut navigasyonu iptal et
            string newUrl = e.Url;
            if (newUrl.Contains("?"))
                newUrl += "&X-Free-Tunnel-Bypass=true";
            else
                newUrl += "?X-Free-Tunnel-Bypass=true";

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AritmaWebView.Source = newUrl;
            });
        }
    }

    private async void AritmaWebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[WebView] Navigated to: {e.Url} | Result: {e.Result}");
            
            if (e.Result == WebNavigationResult.Success && AritmaWebView != null)
            {
                // Dev Tunnel "Mavi Ekran"ı geçmek için daha agresif bir deneme
                for (int i = 0; i < 10; i++) // 10 saniye boyunca dene
                {
                    if (AritmaWebView == null) break;

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            // Hem buton metnine, hem ID'ye, hem de sınıfa göre ara
                            await AritmaWebView.EvaluateJavaScriptAsync(@"
                                (function() {
                                    var clicked = false;
                                    var buttons = document.querySelectorAll('button, a, input[type=""button""]');
                                    for (var btn of buttons) {
                                        var text = (btn.innerText || btn.value || '').toLowerCase();
                                        if (text.includes('continue') || text.includes('devam') || 
                                            btn.id === 'continue' || btn.id === 'confirm' ||
                                            btn.classList.contains('btn-primary')) {
                                            btn.click();
                                            clicked = true;
                                        }
                                    }
                                    // Microsoft özel sayfası için ekstra kontrol
                                    var consentBtn = document.getElementById('continue');
                                    if (consentBtn) { consentBtn.click(); clicked = true; }
                                    
                                    return clicked;
                                })()
                            ");
                        }
                        catch (Exception jsEx)
                        {
                            Debug.WriteLine($"[WebView JS Error] {jsEx.Message}");
                        }
                    });
                    
                    await Task.Delay(1000);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[WebView Fatal Error] {ex.Message}");
        }
    }
}
