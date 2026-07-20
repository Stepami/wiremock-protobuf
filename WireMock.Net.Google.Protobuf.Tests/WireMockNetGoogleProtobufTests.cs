using AutoFixture;
using Grpc.Net.Client;
using WireMock.Logging;
using WireMock.Net.Google.Protobuf.Request;
using WireMock.Net.Google.Protobuf.Response;
using WireMock.Net.TUnit;
using WireMock.Protobuf.Client;
using WireMock.Server;
using WireMock.Settings;

namespace WireMock.Net.Google.Protobuf.Tests;

public class WireMockNetGoogleProtobufTests
{
    private static IWireMockLogger Logger =>
        TestContext.Current?.GetDefaultLogger() is { } logger
            ? new TUnitWireMockLogger(logger)
            : new WireMockNullLogger();

    [Test]
    public async Task WithBodyAsGoogleProtobuf_MessageMatcher_Success()
    {
        var fixture = new Fixture();
        var server = WireMockServer.Start(
            new WireMockServerSettings
            {
                UseHttp2 = true,
                Logger = Logger
            });

        var getGroupsRequestGrpc = new GetGroupsRequestGrpc
        {
            ByGroupType = new()
            {
                GroupType = GroupTypeGrpc.CatalogNavigation,
                EntityIds = { fixture.CreateMany<string>() }
            },
            Segments = { fixture.CreateMany<string>() },
            Platform = "wechat",
            Version = "1.0.0"
        };
        var getGroupsResponseGrpc = new GetGroupsResponseGrpc
        {
            Groups =
            {
                new GroupGrpc
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityIds = { fixture.CreateMany<string>() },
                    EntityType = EntityTypeGrpc.CatalogNavigation,
                    Items = { fixture.CreateMany<ItemV2Grpc>() }
                }
            }
        };

        server
            .Given(
                RequestBuilders.Request.Create()
                    .UsingPost()
                    .WithHttpVersion("2")
                    .WithPath("/test.MyTestService/GetGroups")
                    .WithBodyAsGoogleProtobuf(getGroupsRequestGrpc))
            .RespondWith(
                ResponseBuilders.Response.Create()
                    .WithHeader("Content-Type", "application/grpc")
                    .WithTrailingHeader("grpc-status", "0")
                    .WithBodyAsGoogleProtobuf(getGroupsResponseGrpc));

        var channel = GrpcChannel.ForAddress(server.Url ?? throw new Exception("Grpc channel null"));

        var myTestServiceClient = new MyTestService.MyTestServiceClient(channel);
        var actual = await myTestServiceClient.GetGroupsAsync(getGroupsRequestGrpc);

        server.Stop();

        await Assert.That(actual).IsEqualTo(getGroupsResponseGrpc);
    }

    [Test]
    public async Task WithBodyAsGoogleProtobuf_PredicateMatcher_Success()
    {
        var fixture = new Fixture();
        var server = WireMockServer.Start(
            new WireMockServerSettings
            {
                UseHttp2 = true,
                Logger = Logger
            });

        var getGroupsResponseGrpc = new GetGroupsResponseGrpc
        {
            Groups =
            {
                new GroupGrpc
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityIds = { fixture.CreateMany<string>() },
                    EntityType = EntityTypeGrpc.CatalogNavigation,
                    Items = { fixture.CreateMany<ItemV2Grpc>() }
                }
            }
        };

        server
            .Given(
                RequestBuilders.Request.Create()
                    .UsingPost()
                    .WithHttpVersion("2")
                    .WithPath("/test.MyTestService/GetGroups")
                    .WithBodyAsGoogleProtobuf<GetGroupsRequestGrpc>(x => x is
                    {
                        Platform: "wechat",
                        Version: "1.0.0",
                        ByGroupType.GroupType: GroupTypeGrpc.CatalogNavigation
                    }))
            .RespondWith(
                ResponseBuilders.Response.Create()
                    .WithHeader("Content-Type", "application/grpc")
                    .WithTrailingHeader("grpc-status", "0")
                    .WithBodyAsGoogleProtobuf(getGroupsResponseGrpc));

        var channel = GrpcChannel.ForAddress(server.Url ?? throw new Exception("Grpc channel null"));

        var myTestServiceClient = new MyTestService.MyTestServiceClient(channel);
        var actual = await myTestServiceClient.GetGroupsAsync(
            new GetGroupsRequestGrpc
            {
                ByGroupType = new()
                {
                    GroupType = GroupTypeGrpc.CatalogNavigation,
                    EntityIds = { fixture.CreateMany<string>() }
                },
                Segments = { fixture.CreateMany<string>() },
                Platform = "wechat",
                Version = "1.0.0"
            });

        server.Stop();

        await Assert.That(actual).IsEqualTo(getGroupsResponseGrpc);
    }

    [Test]
    public async Task WithBodyAsGoogleProtobuf_EmptyMessage_Success()
    {
        var server = WireMockServer.Start(
            new WireMockServerSettings
            {
                UseHttp2 = true,
                Logger = Logger
            });

        server
            .Given(
                RequestBuilders.Request.Create()
                    .UsingPost()
                    .WithHttpVersion("2")
                    .WithPath("/test.MyTestService/GetByEmpty")
                    .WithBodyAsGoogleProtobuf(new EmptyRequestGrpc()))
            .RespondWith(
                ResponseBuilders.Response.Create()
                    .WithHeader("Content-Type", "application/grpc")
                    .WithTrailingHeader("grpc-status", "0")
                    .WithBodyAsGoogleProtobuf(new GetEmptyResponseGrpc()));

        var channel = GrpcChannel.ForAddress(server.Url ?? throw new Exception("Grpc channel null"));

        var myTestServiceClient = new MyTestService.MyTestServiceClient(channel);
        var actual = await myTestServiceClient.GetByEmptyAsync(new());

        server.Stop();

        await Assert.That(actual).IsEqualTo(new GetEmptyResponseGrpc());
    }
}