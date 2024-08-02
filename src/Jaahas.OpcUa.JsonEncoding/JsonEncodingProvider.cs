using System.Buffers;

using Microsoft.IO;

using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace Jaahas.OpcUa.JsonEncoding {

    /// <summary>
    /// <see cref="IEncodingProvider"/> that uses OPC UA JSON data encoding.
    /// </summary>
    public sealed class JsonEncodingProvider : IEncodingProvider {

        /// <summary>
        /// Recyclable memory stream manager.
        /// </summary>
        internal static RecyclableMemoryStreamManager MemoryStreamProvider { get; } = new RecyclableMemoryStreamManager();

        /// <summary>
        /// The encoder options to use.
        /// </summary>
        private readonly JsonEncoderOptions? _encoderOptions;

        /// <summary>
        /// The decoder options to use.
        /// </summary>
        private readonly JsonDecoderOptions? _decoderOptions;


        /// <summary>
        /// Creates a new <see cref="JsonEncodingProvider"/> instance.
        /// </summary>
        /// <param name="encoderOptions">
        ///   The encoder options.
        /// </param>
        /// <param name="decoderOptions">
        ///   The decoder options.
        /// </param>
        public JsonEncodingProvider(JsonEncoderOptions? encoderOptions = null, JsonDecoderOptions? decoderOptions = null) {
            _encoderOptions = encoderOptions;
            _decoderOptions = decoderOptions;
        }


        /// <inheritdoc/>
        public IEncoder CreateEncoder(Stream stream, IEncodingContext? context, bool keepStreamOpen) {
            return new JsonEncoder(stream, context, _encoderOptions, keepStreamOpen);
        }


        /// <summary>
        /// Creates an encoder instance that writes to the specified buffer writer.
        /// </summary>
        /// <param name="writer">
        ///   The buffer writer to write to.
        /// </param>
        /// <param name="context">
        ///   The encoding context.
        /// </param>
        /// <returns>
        ///   The encoder instance.
        /// </returns>
        public IEncoder CreateEncoder(IBufferWriter<byte> writer, IEncodingContext? context) {
            return new JsonEncoder(writer, context, _encoderOptions);
        }


        /// <inheritdoc/>
        public IDecoder CreateDecoder(Stream stream, IEncodingContext? context, bool keepStreamOpen) {
            return new JsonDecoder(stream, context, _decoderOptions, keepStreamOpen);
        }


        /// <summary>
        /// Creates a new decoder that reads from the specified memory region.
        /// </summary>
        /// <param name="bytes">
        ///   The memory region to read from.
        /// </param>
        /// <param name="context">
        ///   The encoding context.
        /// </param>
        /// <returns>
        ///   The decoder instance.
        /// </returns>
        public IDecoder CreateDecoder(ReadOnlyMemory<byte> bytes, IEncodingContext? context) {
            return new JsonDecoder(bytes, context, _decoderOptions);
        }


        /// <summary>
        /// Creates a new decoder that reads from the specified byte sequence.
        /// </summary>
        /// <param name="bytes">
        ///   The byte sequence to read from.
        /// </param>
        /// <param name="context">
        ///   The encoding context.
        /// </param>
        /// <returns>
        ///   The decoder instance.
        /// </returns>
        public IDecoder CreateDecoder(ReadOnlySequence<byte> bytes, IEncodingContext? context) {
            return new JsonDecoder(bytes, context, _decoderOptions);
        }

    }

}
