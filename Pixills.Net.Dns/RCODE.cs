namespace Pixills.Net.Dns
{
    public enum RCODE : byte
    {
        NOERROR = 0,
        FORMAT_ERROR = 1,
        SERVER_FAILURE = 2,
        NAME_ERROR = 3,
        NOT_IMPLEMENTED = 4,
        REFUSED = 5
    }
}
