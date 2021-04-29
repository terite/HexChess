using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Extensions
{
    public static class Extensions 
    {
        public static float Sqr(this int value) => value * value;
        public static float Sqr(this float value) => value * value;
        public static float Sqrt(this float value) => Mathf.Sqrt(value);
        public static int Floor(this float val) => Mathf.FloorToInt(val);
        public static int Ceil(this float val) => Mathf.CeilToInt(val);

        public static void ForEach<T>(this IEnumerable<T> source, System.Action<T> action)
        {
            foreach(T item in source)
                action(item);
        }

        public static HexNeighborDirection OppositeDirection(this HexNeighborDirection direction) =>
            (HexNeighborDirection)(((int)direction + 3) % 6);
    
        public static string IP(this TcpClient client) => $"{((IPEndPoint)client?.Client.RemoteEndPoint).Address}";

        public static Span<byte> Add(this Span<byte> span, byte toAdd)
        {
            byte[] ba = new byte[span.Length + 1];
            Span<byte> target = new Span<byte>(ba);
            span.CopyTo(target.Slice(0, span.Length));
            MemoryMarshal.Write(target.Slice(span.Length, 1), ref toAdd);
            return target;
        }

        public static Span<byte> Add(this Span<byte> span, byte[] toAdd)
        {
            byte[] ba = new byte[span.Length + toAdd.Length];
            Span<byte> add = new Span<byte>(toAdd);
            Span<byte> target = new Span<byte>(ba);
            span.CopyTo(target.Slice(0, span.Length));
            add.CopyTo(target.Slice(span.Length, add.Length));
            return target;
        }
    }
}