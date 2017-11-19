namespace AsyncIO.Net.Libuv
{
    public enum HandleType
    {
        Unknown = 0,
        Async,
        Check,
        FileSystemEvent,
        FileSystemPoll,
        Handle,
        Idle,
        NamedPipe,
        Poll,
        Prepare,
        Process,
        Stream,
        TCP,
        Timer,
        TTY,
        UDP,
        Signal,
    }
}