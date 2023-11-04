using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace AethersenseReduxReborn.Misc;

public record Md5Hash: IEquatable<Md5Hash>
{
    protected static Md5Hash Zeroed { get; }

    public required ImmutableArray<byte> Value { get; init; }

    static Md5Hash() { Zeroed = new Md5Hash(new byte[16]); }

    [JsonConstructor]
    public Md5Hash(ImmutableArray<byte> value) { Value = value; }

    [SetsRequiredMembers]
    protected Md5Hash(string hashString)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(hashString));
        Service.PluginLog.Debug("Computed hash {0} for string [{1}]", BitConverter.ToString(hash), hashString);
        Value = ImmutableArray.CreateRange(hash);
    }

    [SetsRequiredMembers]
    protected Md5Hash(IEnumerable<byte> hashBytes)
    {
        var enumerable = hashBytes as byte[] ?? hashBytes.ToArray();
        if (enumerable.Length != 16)
            throw new ArgumentException("Hash must be 16 bytes long", nameof(hashBytes));
        Value = ImmutableArray.CreateRange(enumerable);
    }


#region Overrides

    public override int GetHashCode() => Value.GetHashCode();

    public virtual bool Equals(Md5Hash? other)
    {
        if ((object)other! == this)
            return true;
        return other != null && Value.SequenceEqual(other.Value);
    }

    public override string ToString() => this == Zeroed ? "Zeroed" : BitConverter.ToString(Value.ToArray());

#endregion
}
