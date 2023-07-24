using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace EdgeGateway
{
    public class WebSocketClient
    {

        public event EventHandler<MessageEventArgs> ReceiveData;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void Connect(string url,string topic =null)
        {
            using (var ws = new WebSocketSharp.WebSocket(url))
            {
                //ws.OnMessage += (sender, e) =>
                //logger.Info("Received Message: " + e.Data);
                if (ReceiveData != null)
                    ws.OnMessage += ReceiveData;

                ws.OnError += (sender, e) =>
                logger.Error("WebSocketClient Error: " + e.Message);

                ws.OnClose += (sender, e) =>
                {
                    logger.Info("WebSocketClient Closed: " + e.Reason);
                    ws.Connect();
                };

                ws.OnOpen += (sender, e) =>
                {
                    logger.Info("WebSocketClient Connected!");
                    //ws.Send("Hello, Server!");
                    if(topic != null)
                        ws.Send("{\"cmd\":\"sub\",\"topic\":\"all_device_state\"}");
                };

                ws.Connect();

            }
        }

    }
}
