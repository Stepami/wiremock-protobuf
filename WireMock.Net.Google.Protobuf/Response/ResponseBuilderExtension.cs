using System.Buffers.Binary;
using Google.Protobuf;
using WireMock.ResponseBuilders;

namespace WireMock.Net.Google.Protobuf.Response;

/// <summary>
/// <see cref="IResponseBuilder"/> extensions for Google Protobuf
/// </summary>
public static class ResponseBuilderExtension
{
    /// <summary>
    /// Set the grpc response body from <see cref="IMessage{TMessage}"/> implementation object
    /// </summary>
    /// <param name="responseBuilder">The <see cref="IResponseBuilder"/></param>
    /// <param name="responseMessage">The grpc response message object</param>
    /// <typeparam name="TMessage">Proto compiled implementation of <see cref="IMessage{TMessage}"/></typeparam>
    public static IResponseBuilder WithBodyAsGoogleProtobuf<TMessage>(
        this IResponseBuilder responseBuilder,
        TMessage responseMessage)
        where TMessage : IMessage<TMessage>
    {
        const int compressionFlagIndex = 0;
        const int headerSize = 5;

        var payload = responseMessage.ToByteArray();
        var responseBytes = new byte[headerSize + payload.Length];

        responseBytes[compressionFlagIndex] = 0;
        BinaryPrimitives.WriteUInt32BigEndian(
            responseBytes.AsSpan(1, 4),
            checked((uint)payload.Length));
        payload.CopyTo(responseBytes.AsSpan(headerSize));

        return responseBuilder.WithBody(responseBytes);
    }
}