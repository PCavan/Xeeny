﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xeeny.Sockets.WebSockets
{
    public class WebSocket : SocketBase
    {
        System.Net.WebSockets.WebSocket _webSocket;
        Uri _uri;

        public WebSocket(System.Net.WebSockets.WebSocket socket, SocketSettings settings, ILoggerFactory loggerFactory)
            : base(settings, loggerFactory.CreateLogger(nameof(WebSocket)))
        {
            _webSocket = socket;
            SetState();
        }

        public WebSocket(Uri uri, SocketSettings settings, ILoggerFactory loggerFactory)
            : base(settings, loggerFactory.CreateLogger(nameof(WebSocket)))
        {
            _webSocket = new System.Net.WebSockets.ClientWebSocket();
            _uri = uri;

            SetState();
        }

        void SetState()
        {
            var state = _webSocket.State;
            switch(state)
            {
                case WebSocketState.None: State = ConnectionState.None; break;
                case WebSocketState.Connecting: State = ConnectionState.Connecting; break;
                case WebSocketState.Open: State = ConnectionState.Connected; break;
                case WebSocketState.CloseSent: State = ConnectionState.Closing; break;
                case WebSocketState.CloseReceived: State = ConnectionState.Closing; break;
                case WebSocketState.Closed: State = ConnectionState.Closed; break;
                default: throw new NotSupportedException(state.ToString());
            }
        }

        protected override Task OnConnect(CancellationToken ct)
        {
            return ((ClientWebSocket)_webSocket).ConnectAsync(_uri, ct);
        }

        protected override async void OnClose(CancellationToken ct)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session Ended", ct);
            }
            catch { }
            try
            {
                await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Session Ended", ct);
            }
            catch { }
        }

        protected override async Task Send(ArraySegment<byte> segment, CancellationToken ct)
        {
            await _webSocket.SendAsync(segment, WebSocketMessageType.Binary, true, ct)
                               .ConfigureAwait(false);
        }
        
        protected override async Task<int> Receive(ArraySegment<byte> receiveBuffer, CancellationToken ct)
        {
            var result = await _webSocket.ReceiveAsync(receiveBuffer, ct)
                                             .ConfigureAwait(false);
            return result.Count;
        }
    }
}
