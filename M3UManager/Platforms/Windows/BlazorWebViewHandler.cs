using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Web.WebView2.Core;

namespace M3UManager.Platforms.Windows
{
    public class CustomBlazorWebViewHandler : BlazorWebViewHandler
    {
        protected override void ConnectHandler(Microsoft.UI.Xaml.Controls.WebView2 platformView)
        {
            base.ConnectHandler(platformView);
            
            // Handle CoreWebView2Initialized event
            platformView.CoreWebView2Initialized += (sender, args) =>
            {
                if (platformView.CoreWebView2 != null)
                {
                    // Add a web resource requested handler to intercept and allow mixed content
                    platformView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
                    
                    // Enable all necessary settings
                    var settings = platformView.CoreWebView2.Settings;
                    settings.IsScriptEnabled = true;
                    settings.AreDefaultContextMenusEnabled = true;
                    settings.AreDevToolsEnabled = true;
                    settings.IsWebMessageEnabled = true;
                }
            };
        }
    }
}
