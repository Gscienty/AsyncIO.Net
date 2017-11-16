namespace AsyncIO.Net.Libuv.Requests
{
    public enum RequestType : int
    {
        Unknown = 0,
        REQ,
        CONNECT,
        WRITE,
        SHUTDOWN,
        UDP_SEND,
        FS,
        WORK,
        GETADDRINFO,
        GETNAMEINFO,
    }
}