﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp;

namespace BinanceDex.WebSockets
{
    public class WebSocketBase : IDisposable
    {
        protected readonly WebSocket WebSocket;
        private readonly bool keepConnected;
        private readonly CancellationTokenSource cts;

        protected WebSocketBase(string baseUrl, bool keepConnected) : this(baseUrl)
        {
            this.keepConnected = keepConnected;
            this.cts = new CancellationTokenSource();
        }

        protected WebSocketBase(string baseUrl)
        {
            this.WebSocket = new WebSocket(baseUrl) {EmitOnPing = true};
        }

        private Task KeepAlive(CancellationToken token) => Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(25), token);
            if (this.WebSocket.IsAlive)
            {
                this.Send(@"{ ""method"": ""keepAlive"" }");
                await this.KeepAlive(token);
            }
        }, token);

        public void Connect()
        {
            if (this.WebSocket.IsAlive) return;

            this.WebSocket.Connect();
            if (this.keepConnected)
            {
                this.KeepAlive(this.cts.Token);
            }
        }

        protected void Send(string data)
        {
            this.WebSocket.Send(data);
        }


        public void Dispose()
        {
            this.Send(@"{""method"": ""close""}");
            ((IDisposable) this.WebSocket)?.Dispose();
            this.cts?.Dispose();
        }
    }


    public class SubscriptionOptions
    {

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("symbols")]
        public List<string> Symbols { get; set; }
    }
}
