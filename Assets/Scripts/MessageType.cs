// If this goes over 256 values, it will no longer be able to be stored in 1 byte
public enum MessageType : byte {
    None = 0,
    Disconnect = 1,
    Ping = 2,
    Pong = 3,
}