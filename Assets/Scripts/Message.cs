using System;

public struct Message 
{
    public byte[] signature;
    public ushort length;
    public MessageType type;
    public byte[] data;

    public Message(byte[] signature, MessageType type, byte[] data)
    {
        this.signature = signature;
        this.type = type;
        this.data = data;
        this.length = (ushort)data.Length;
    }
    public Message(MessageType type, byte[] data)
    {
        this.signature = GetSignature().ToArray();
        this.type = type;
        this.data = data;
        this.length = (ushort)data.Length;
    }
    public Message(MessageType type, byte data)
    {
        this.signature = GetSignature().ToArray();
        this.type = type;
        this.data = new byte[]{data};
        this.length = (ushort)1;
    }
    public Message(MessageType type)
    {
        this.signature = GetSignature().ToArray();
        this.type = type;
        this.data = Array.Empty<byte>();
        this.length = (ushort)0;
    }

    // This defines the protocol for the message
    public byte[] Serialize()
    {
        byte[] serializedMessage = new byte[signature.Length + length + 3];
        
        // Signature
        Buffer.BlockCopy(signature, 0, serializedMessage, 0, signature.Length);
        // Message Length
        Buffer.BlockCopy(BitConverter.GetBytes(length), 0, serializedMessage, signature.Length, 2);
        // Message Type
        serializedMessage[signature.Length + 2] = (byte)type;
        // Data
        Buffer.BlockCopy(data, 0, serializedMessage, signature.Length + 3, data.Length);

        return serializedMessage;
    }

    public override string ToString()
    {
        return $"Message({type}, len={data.Length})";
    }

    public static Span<byte> GetSignature() => new byte[5] {1, 2, 3, 4, 5};

    public static (int msgLen, Message message)? ReadMessage(ReadOnlySpan<byte> buffer)
    {
        int headerLength = 5 + 2 + 1; 
        if (buffer.Length < headerLength)
            return null;

        var signatureSpan = buffer.Slice(0, 5);
        if (!signatureSpan.SequenceEqual(GetSignature()))
            throw new ArgumentException("Invalid message signature!");

        var payloadLenSpan = buffer.Slice(5, 2);
        ushort payloadLength = BitConverter.ToUInt16(payloadLenSpan.ToArray(), 0);
        var messageType = (MessageType)buffer[7];

        var totalLength = headerLength + payloadLength;
        if (totalLength < buffer.Length)
            return null;

        var payload = buffer.Slice(headerLength, payloadLength).ToArray();

        return (totalLength, new Message(messageType, payload));
    }
}