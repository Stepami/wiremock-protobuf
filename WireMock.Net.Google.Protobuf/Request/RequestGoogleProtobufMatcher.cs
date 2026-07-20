using WireMock.Matchers;
using WireMock.Matchers.Request;

namespace WireMock.Net.Google.Protobuf.Request;

/// <summary>
/// The request body GRPC matcher with static typing
/// </summary>
/// <param name="matcher">The object matcher to match on grpc message object</param>
internal sealed class RequestGoogleProtobufMatcher(IObjectMatcher matcher) : IRequestMatcher
{
    /// <inheritdoc />
    public double GetMatchingScore(
        IRequestMessage requestMessage,
        IRequestMatchResult requestMatchResult)
    {
        var matchResult = matcher.IsMatch(requestMessage.BodyAsBytes);
        return requestMatchResult.AddMatchDetail(
            new MatchDetail
            {
                Name = matchResult.Name,
                MatcherType = nameof(RequestGoogleProtobufMatcher),
                Score = matchResult.Score,
                Exception = matchResult.Exception
            });
    }

    /// <inheritdoc />
    public RequestMatcherType Type => RequestMatcherType.ProtoBuf;
}