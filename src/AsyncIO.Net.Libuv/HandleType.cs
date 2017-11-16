namespace AsyncIO.Net.Libuv
{
    public enum HandleType
    {
        Unknown = 0,
        ASYNC,
        CHECK,
        FS_EVENT,
        FS_POLL,
        HANDLE,
        IDLE,
        NAMED_PIPE,
        POLL,
        PREPARE,
        PROCESS,
        STREAM,
        TCP,
        TIMER,
        TTY,
        UDP,
        SIGNAL,
    }
}