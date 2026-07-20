using Google.Protobuf;
using WireMock.Matchers;
using WireMock.Net.Google.Protobuf.Request.Matchers;
using WireMock.RequestBuilders;

namespace WireMock.Net.Google.Protobuf.Request;

/// <summary>
/// <see cref="IRequestBuilder"/> extensions for Google Protobuf
/// </summary>
public static class RequestBuilderExtension
{
    /// <summary>
    /// Expect the exact grpc message body
    /// </summary>
    /// <param name="requestBuilder">The <see cref="IRequestBuilder"/></param>
    /// <param name="message">Expected grpc message body</param>
    /// <param name="matchBehaviour">The match behavior, default is <see cref="MatchBehaviour.AcceptOnMatch"/></param>
    /// <typeparam name="TMessage">Proto compiled implementation of <see cref="IMessage{TMessage}"/></typeparam>
    public static IRequestBuilder WithBodyAsGoogleProtobuf<TMessage>(
        this IRequestBuilder requestBuilder,
        TMessage message,
        MatchBehaviour matchBehaviour = MatchBehaviour.AcceptOnMatch)
        where TMessage : IMessage<TMessage>
    {
        return requestBuilder.Add(
            new RequestGoogleProtobufMatcher(
                new GrpcMessageMatcher<TMessage>(message, matchBehaviour)));
    }

    /// <summary>
    /// Expect the grpc message body that satisfies the predicate
    /// </summary>
    /// <param name="requestBuilder">The <see cref="IRequestBuilder"/></param>
    /// <param name="predicate">Boolean function to test whether the incoming grpc message satisfies a mapping</param>
    /// <param name="matchBehaviour">The match behavior, default is <see cref="MatchBehaviour.AcceptOnMatch"/></param>
    /// <typeparam name="TMessage">Proto compiled implementation of <see cref="IMessage{TMessage}"/></typeparam>
    public static IRequestBuilder WithBodyAsGoogleProtobuf<TMessage>(
        this IRequestBuilder requestBuilder,
        Func<TMessage, bool> predicate,
        MatchBehaviour matchBehaviour = MatchBehaviour.AcceptOnMatch)
        where TMessage : IMessage<TMessage>, new()
    {
        return requestBuilder.Add(
            new RequestGoogleProtobufMatcher(
                new GrpcPredicateMatcher<TMessage>(predicate, matchBehaviour)));
    }
}