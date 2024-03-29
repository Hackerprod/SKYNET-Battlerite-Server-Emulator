﻿using System;
using System.Collections.Generic;

namespace SKYNET
{
    public struct Id
    {
        private readonly int value;

        public Id(int value)
        {
            this.value = value;
        }

        public Id<T> Generic<T>()
        {
            return new Id<T>(this.value);
        }
    }

    public struct Id<T> : IEquatable<Id<T>>
    {
        public readonly int value;

        public Id Simple { get { return new Id(this.value); } }

        public Id(int value)
        {
            this.value = value;
        }
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
        public bool Equals(Id<T> other)
        {
            return this.value.Equals(other.value);
        }
        public override bool Equals(object obj)
        {
            return obj is Id<T> && this.Equals((Id<T>)obj);
        }
        public static bool operator ==(Id<T> id0, Id<T> id1)
        {
            return id0.Equals(id1);
        }
        public static bool operator !=(Id<T> id0, Id<T> id1)
        {
            return !(id0 == id1);
        }
    }

    public sealed class IdManager
    {
        private readonly Dictionary<Type, int> lastIds = new Dictionary<Type, int>();

        public Id<T> GetNext<T>()
        {
            int i;
            var type = typeof(T);
            lock (lastIds)
            {
                this.lastIds.TryGetValue(type, out i);
                i++;
                this.lastIds[type] = i;
            }
            return new Id<T>(i);
        }
    }
}
