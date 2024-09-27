using System;

namespace Phoenix
{
    public enum WebsocketState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    public struct WebsocketConfiguration
    {
        public Uri uri;
        public bool binaryMode;
        public Action<IWebsocket> onOpenCallback;
        public Action<IWebsocket, ushort, string> onCloseCallback;
        public Action<IWebsocket, Exception> onErrorCallback;
        public Action<IWebsocket, string> onMessageCallback;
        public Action<IWebsocket, byte[]> onBinaryCallback;
    }

    public interface IWebsocketFactory
    {
        IWebsocket Build(WebsocketConfiguration config);
    }

    public interface IWebsocket
    {
        WebsocketState State { get; }

        void Connect();
        void Send(string data);
        void Send(byte[] data);
        void Close(ushort? code = null, string reason = null);
    }
}
