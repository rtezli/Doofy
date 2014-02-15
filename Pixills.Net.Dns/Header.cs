namespace Pixills.Net.Dns
{
    public struct Header
    {
        public short ID;
        public QR QR;
        public OPCODE OpCode;
        public bool AA;
        public bool TC;
        public bool RD;
        public bool RA;
        public short Z;
        public short RCODE;
        public ushort QDCOUNT;
        public ushort ANCOUNT;
        public ushort NSCOUNT;
        public ushort ARCOUNT;
    }
}
