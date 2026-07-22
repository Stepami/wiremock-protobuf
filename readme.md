# WireMock.Net.Google.Protobuf

[![GitHub stars](https://img.shields.io/github/stars/Stepami/wiremock-protobuf?style=flat-square)](https://github.com/Stepami/wiremock-protobuf/stargazers)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Statically typed gRPC mocking for `WireMock.Net` using generated `Google.Protobuf` contracts.**

```csharp
Request.Create()
    .WithBodyAsGoogleProtobuf(new HelloRequest { Name = "Stepan" });

Response.Create()
    .WithBodyAsGoogleProtobuf(
        new HelloReply { Message = "Hello, Stepan!" });
```

No `ProtoDefinition`, message type strings, JSON conversion, or manually constructed binary payloads.

## Why?

The standard `WireMock.Net` protobuf integration requires loading `.proto` definitions and referring to protobuf message types separately from your generated C# contracts.

This package uses the generated `IMessage<T>` types you already have to write less configuration and do no duplicated schema setup:

* compile-time type safety;
* native `Google.Protobuf` serialization;
* exact-message and predicate request matching;

When a contract changes, outdated mocks stop compiling instead of failing at runtime.

## Installation

```bash
dotnet add package WireMock.Grpc.Protobuf
```

The package targets **.NET 8**

## Message Matching

Assuming you have some `greet.proto` file that generates `HelloRequest`, `HelloReply`, and `Greeter.GreeterClient` for instance:
```protobuf
syntax = "proto3";

option csharp_namespace = "Greeting.Contracts";

package greet;

service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply);
}

message HelloRequest {
  string name = 1;
}

message HelloReply {
  string message = 1;
}
```

And assuming you also have the required packages added with protobuf compilation settings to your test project:
```xml
<Project Sdk="Microsoft.NET.Sdk">
    <ItemGroup>
        <PackageReference Include="WireMock.Net" Version="2.13.0" />
        <PackageReference Include="WireMock.Grpc.Protobuf" Version="1.0.0" />
        <PackageReference Include="Grpc.Net.Client" Version="2.80.0" />
        <PackageReference Include="Grpc.Core" Version="2.46.6"/>
        <PackageReference Include="Grpc.Tools" Version="2.82.0">
            <PrivateAssets>all</PrivateAssets>
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>
    </ItemGroup>

    <ItemGroup>
        <Protobuf Include=".\test.proto" GrpcServices="Client" />
    </ItemGroup>
</Project>
```

Start `WireMock.Net` with **HTTP/2** enabled and configure the expected request and response using the `WithBodyAsGoogleProtobuf` extension methods:
```csharp
using Greeting.Contracts;
using Grpc.Net.Client;
using WireMock.Net.Google.Protobuf.Request;
using WireMock.Net.Google.Protobuf.Response;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

using var server = WireMockServer.Start(
    new WireMockServerSettings
    {
        UseHttp2 = true
    });

var request = new HelloRequest
{
    Name = "Stepan"
};

var expectedResponse = new HelloReply
{
    Message = "Hello, Stepan!"
};

server
    .Given(
        Request.Create()
            .UsingPost()
            .WithHttpVersion("2")
            .WithPath("/greet.Greeter/SayHello")
            .WithBodyAsGoogleProtobuf(request))
    .RespondWith(
        Response.Create()
            .WithHeader("Content-Type", "application/grpc")
            .WithTrailingHeader("grpc-status", "0")
            .WithBodyAsGoogleProtobuf(expectedResponse));

using var channel = GrpcChannel.ForAddress(server.Url!);
var client = new Greeter.GreeterClient(channel);

var actualResponse = await client.SayHelloAsync(request);

Assert.Equal(expectedResponse, actualResponse);
```

## Predicate Matching

Match only the fields relevant to your test:

```csharp
Request.Create()
    .WithBodyAsGoogleProtobuf<HelloRequest>(
        request =>
            request.Name.StartsWith("Step") &&
            request.Name.Length >= 5);
```

This keeps request matching strongly typed without comparing the entire protobuf message.

## Important

**Do not call** `WithTransformer()` after `WithBodyAsGoogleProtobuf()`. The response is already a binary gRPC frame, and text transformation may corrupt it.

WireMock.Net must also run with HTTP/2 enabled:

```csharp
new WireMockServerSettings
{
    UseHttp2 = true
};
```

## License

Distributed under the [MIT License](LICENSE).
