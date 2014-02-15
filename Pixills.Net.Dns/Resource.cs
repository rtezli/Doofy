namespace Pixills.Net.Dns
{
    public struct Resource
    {
        public string Name;
        public short TYPE;
        public short CLASS;
        public int TTL;
        public short RDLENGTH;
        public string RDATA;
    }
}
