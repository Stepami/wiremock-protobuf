using System.Buffers.Binary;
using Google.Protobuf;
using WireMock.Matchers;

namespace WireMock.Net.Google.Protobuf.Request.Matchers;

/// <summary>
/// The <see cref="IObjectMatcher"/> that compares two grpc messages and expects the exact match
/// </summary>
/// <param name="messageValue">Expected grpc message body</param>
/// <param name="matchBehaviour">The match behavior, default is <see cref="MatchBehaviour.AcceptOnMatch"/></param>
/// <typeparam name="TMessage">Proto compiled implementation of <see cref="IMessage{TMessage}"/></typeparam>
internal sealed class GrpcMessageMatcher<TMessage>(
    TMessage messageValue,
    MatchBehaviour matchBehaviour) : IObjectMatcher
    where TMessage : IMessage<TMessage>
{
    /// <inheritdoc />
    public MatchResult IsMatch(object? input)
    {
        if (input is not byte[] inputBytes)
            return MatchResult.From(Name);

        try
        {
            return MatchResult.From(
                Name,
                MatchBehaviour,
                IsMessageMatch(inputBytes));
        }
        catch (Exception e)
        {
            return MatchResult.From(Name, exception: e);
        }
    }

    private bool IsMessageMatch(byte[] inputBytes)
    {
        const int compressionFlagIndex = 0;
        const int headerLength = 5;
        if (inputBytes[compressionFlagIndex] != 0)
            return false;

        var sizeHeader = new ReadOnlySpan<byte>(inputBytes, 1, 4);
        var length = BinaryPrimitives.ReadUInt32BigEndian(sizeHeader);

        if (inputBytes.Length - headerLength < length)
            return false;

        var messageBytes = new ReadOnlySpan<byte>(messageValue.ToByteArray());
        return messageBytes.SequenceEqual(new ReadOnlySpan<byte>(inputBytes, headerLength, (int)length));
    }

    /// <inheritdoc />
    public object Value => messageValue;

    /// <inheritdoc />
    public string GetCSharpCodeArguments() => "NotImplemented";

    /// <inheritdoc />
    public string Name => nameof(GrpcMessageMatcher<>);

    /// <inheritdoc />
    public MatchBehaviour MatchBehaviour => matchBehaviour;
}