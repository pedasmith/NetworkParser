using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ExampleParserApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ClearStatus ()
        {
            uiNetworkStatus.Text = "";
        }
        private void SetStatus(string text)
        {
            uiNetworkStatus.Text = text;
        }

        private async void OnGetUrl(object sender, RoutedEventArgs e)
        {
            ClearStatus();
            var uriStatus = Uri.TryCreate(uiUri.Text, UriKind.Absolute, out var uri);
            if (uriStatus == false)
            {
                SetStatus("That's not a valid URI");
                return;
            }

            try
            {
                var socket = new StreamSocket();
                await socket.ConnectAsync(new HostName(uri.Host), uri.Port.ToString());
                var dw = new DataWriter (socket.OutputStream);
                dw.WriteString ($"GET {uri.AbsolutePath} HTTP/1.0\r\n\r\n");
                await dw.StoreAsync();
                await dw.FlushAsync();

                uiResults.Text = "";

                // Now read in the results!
                var dr = new DataReader(socket.InputStream);
                dr.InputStreamOptions = InputStreamOptions.Partial;
                bool keepGoing = true;
                var state = new NetworkParsers.ParseCRLF.SplitState();
                while (keepGoing)
                {
                    uint nbytes = await dr.LoadAsync(1);
                    if (nbytes == 0)
                    {
                        SetStatus("ALL DONE reading");
                        keepGoing = false;
                    }
                    else
                    {
                        SetStatus($"Read {nbytes} bytes");
                        var data = new byte[nbytes];
                        dr.ReadBytes(data); //ISSUE: if I use a single buffer, need to pass in the byte count to the reader
                        state = NetworkParsers.ParseCRLF.SplitCRLF(data, state);
                    }
                }

                foreach (var lineBytes in state.Lines)
                {
                    //ISSUE: decode correctly
                    var lineString = Encoding.UTF8.GetString(lineBytes);
                    uiResults.Text += lineString + "\n";
                }
                dr.Dispose(); // Close it all down. ISSUE: is this closing correct?
            }
            catch (Exception ex)
            {
                SetStatus ($"ERROR: Exception {ex.Message}");
                return;
            }
        }
    }
}
