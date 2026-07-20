using System.Buffers.Binary;
using Google.Protobuf;
using WireMock.Matchers;

namespace WireMock.Net.Google.Protobuf.Request.Matchers;

/// <summary>
/// The <see cref="IObjectMatcher"/> that tests whether the incoming grpc message satisfies the predicate
/// </summary>
/// <param name="predicate">Boolean function to test whether the incoming grpc message satisfies a mapping</param>
/// <param name="matchBehaviour">The match behavior, default is <see cref="MatchBehaviour.AcceptOnMatch"/></param>
/// <typeparam name="TMessage">Proto compiled implementation of <see cref="IMessage{TMessage}"/></typeparam>
internal sealed class GrpcPredicateMatcher<TMessage>(
    Func<TMessage, bool> predicate,
    MatchBehaviour matchBehaviour) : IObjectMatcher
    where TMessage : IMessage<TMessage>, new()
{
    private static readonly TMessage Empty = new();
    private static readonly MessageParser<TMessage> Parser = new(() => Empty);

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
                IsPredicateMatch(inputBytes));
        }
        catch (Exception e)
        {
            return MatchResult.From(Name, exception: e);
        }
    }

    private bool IsPredicateMatch(byte[] inputBytes)
    {
        const int compressionFlagIndex = 0;
        const int headerLength = 5;
        if (inputBytes[compressionFlagIndex] != 0)
            return false;

        var sizeHeader = new ReadOnlySpan<byte>(inputBytes, 1, 4);
        var length = BinaryPrimitives.ReadUInt32BigEndian(sizeHeader);

        if (inputBytes.Length - headerLength < length)
            return false;

        var inputMessage = Parser.ParseFrom(new ReadOnlySpan<byte>(inputBytes, headerLength, (int)length));
        return predicate.Invoke(inputMessage);
    }

    /// <inheritdoc />
    public object Value => Empty;

    /// <inheritdoc />
    public string GetCSharpCodeArguments() => "NotImplemented";

    /// <inheritdoc />
    public string Name => nameof(GrpcPredicateMatcher<TMessage>);

    /// <inheritdoc />
    public MatchBehaviour MatchBehaviour => matchBehaviour;
}