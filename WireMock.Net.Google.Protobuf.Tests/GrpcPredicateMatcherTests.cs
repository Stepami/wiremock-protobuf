using WireMock.Matchers;
using WireMock.Net.Google.Protobuf.Request.Matchers;
using WireMock.Net.Google.Protobuf.Response;
using WireMock.Protobuf.Client;

namespace WireMock.Net.Google.Protobuf.Tests;

public class GrpcPredicateMatcherTests
{
    [Test]
    public async Task IsMatch_RepeatedCall_Success()
    {
        // Arrange
        var matcher = new GrpcPredicateMatcher<GetGroupsRequestGrpc>(
            x => x.Segments.SequenceEqual(["1", "2", "3"]),
            MatchBehaviour.AcceptOnMatch);

        var input = new GetGroupsRequestGrpc { Segments = { "1", "2", "3" } };
        var response = ResponseBuilders.Response.Create().WithBodyAsGoogleProtobuf(input);

        // Act
        matcher.IsMatch(response.ResponseMessage.BodyData?.BodyAsBytes);
        var result = matcher.IsMatch(response.ResponseMessage.BodyData?.BodyAsBytes).IsPerfect();

        // Assert
        await Assert.That(result).IsTrue();
    }
}