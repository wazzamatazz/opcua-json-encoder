using System.Buffers;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace Jaahas.OpcUa.JsonEncoding {

    /// <summary>
    /// <see cref="IDecoder"/> that decodes data using the OPC UA JSON encoding.
    /// </summary>
    /// <remarks>
    ///   The OPC UA JSON encoding is defined <a href=https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4">here</a>.
    /// </remarks>
    public sealed class JsonDecoder : IDecoder {

        /// <summary>
        /// Flags if the decoder has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The encoding context, if provided.
        /// </summary>
        private readonly IEncodingContext _context;

        /// <summary>
        /// The stream to read from.
        /// </summary>
        private readonly Stream? _stream;

        /// <summary>
        /// When <see langword="true"/>, the <see cref="_stream"/> will be kept open when the 
        /// deccoder is disposed.
        /// </summary>
        private readonly bool _keepStreamOpenOnDispose;

        /// <summary>
        /// The JSON document to read from.
        /// </summary>
        private readonly JsonDocument _document;

        /// <summary>
        /// The root element of the JSON document being decoded.
        /// </summary>
        public JsonElement Document => _document.RootElement;

        /// <summary>
        /// Current navigation stack.
        /// </summary>
        private readonly Stack<JsonElement> _stack = new Stack<JsonElement>();

        /// <summary>
        /// The decoder options.
        /// </summary>
        private readonly JsonDecoderOptions _options;


        /// <summary>
        /// Creates a new <see cref="JsonDecoder"/> instance that reads from the specified 
        /// <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The stream to read from.
        /// </param>
        /// <param name="context">
        ///   The encoding context.
        /// </param>
        /// <param name="options">
        ///   The decoder options.
        /// </param>
        /// <param name="keepStreamOpen">
        ///   <see langword="true"/> to keep the <paramref name="stream"/> open when the decoder 
        ///   is disposed, or <see langword="false"/> to dispose of the <paramref name="stream"/> 
        ///   when the decoder is disposed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        ///   The entire contents of the <paramref name="stream"/> are read into a <see cref="JsonDocument"/> 
        ///   when the decoder is created. The <see cref="JsonDocument"/> is disposed when the 
        ///   decoder is disposed.
        /// </remarks>
        public JsonDecoder(Stream stream, IEncodingContext? context = null, JsonDecoderOptions? options = null, bool keepStreamOpen = false) {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _context = context ?? new DefaultEncodingContext();
            _options = options ?? new JsonDecoderOptions();
            
            _keepStreamOpenOnDispose = keepStreamOpen;

            _document = JsonDocument.Parse(stream);
            _stack.Push(_document.RootElement);
        }


        /// <summary>
        /// Creates a new <see cref="JsonDecoder"/> instance that reads from the specified memory 
        /// region.
        /// </summary>
        /// <param name="bytes">
        ///   The memory region to read from.
        /// </param>
        /// <param name="context">
        ///   The encoding context.
        /// </param>
        /// <param name="options">
        ///   The decoder options.
        /// </param>
        public JsonDecoder(ReadOnlyMemory<byte> bytes, IEncodingContext? context = null, JsonDecoderOptions? options = null) {
            _context = context ?? new DefaultEncodingContext();
            _options = options ?? new JsonDecoderOptions();
            _keepStreamOpenOnDispose = true;

            _document = JsonDocument.Parse(bytes);
            _stack.Push(_document.RootElement);
        }


        /// <summary>
        /// Creates a new <see cref="JsonDecoder"/> instance that reads from the specified byte 
        /// sequence.
        /// </summary>
        /// <param name="bytes">
        ///   The byte sequence to read from.
        /// </param>
        /// <param name="context">
        ///   The encoding context.
        /// </param>
        /// <param name="options">
        ///   The decoder options.
        /// </param>
        public JsonDecoder(ReadOnlySequence<byte> bytes, IEncodingContext? context = null, JsonDecoderOptions? options = null) {
            _context = context ?? new DefaultEncodingContext();
            _options = options ?? new JsonDecoderOptions();
            _keepStreamOpenOnDispose = true;

            _document = JsonDocument.Parse(bytes);
            _stack.Push(_document.RootElement);
        }


        /// <inheritdoc/>
        public void PushNamespace(string namespaceUri) {
            // No-op
        }


        /// <inheritdoc/>
        public void PopNamespace() {
            // No-op
        }


        /// <summary>
        /// Checks if the current JSON element has a property with the specified name.
        /// </summary>
        /// <param name="name">
        ///   The property name.
        /// </param>
        /// <param name="valueKind">
        ///   The <see cref="JsonValueKind"/> of the property, if found.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the property was found; otherwise, <see langword="false"/>.
        /// </returns>
        private bool HasProperty(string name, out JsonValueKind valueKind) {
            var currentElement = _stack.Peek();

            if (!currentElement.TryGetProperty(name, out var prop)) {
                valueKind = default;
                return false;
            }

            valueKind = prop.ValueKind;
            return true;
        }


        /// <summary>
        /// Pushes the property with the specified name on the current JSON element onto the stack.
        /// </summary>
        /// <param name="name">
        ///   The property name.
        /// </param>
        /// <param name="property">
        ///   The property.
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the property was found; otherwise, <see langword="false"/>.
        /// </returns>
        private bool TryPushProperty(string name, out JsonElement property) {
            property = default;
            var currentElement = _stack.Peek();

            if (!currentElement.TryGetProperty(name, out property)) {
                return false;
            }

            _stack.Push(property);
            return true;
        }


        /// <summary>
        /// Reads a value from the current JSON element.
        /// </summary>
        /// <typeparam name="T">
        ///   The value type.
        /// </typeparam>
        /// <param name="fieldName">
        ///   The name of the field to read from. If <see langword="null"/>, the value is read 
        ///   directly from the element at the top of the stack.
        /// </param>
        /// <param name="reader">
        ///   A delegate that will read the value from the provided element.
        /// </param>
        /// <returns>
        ///   The value read from the JSON element.
        /// </returns>
        private T? ReadCore<T>(string? fieldName, Func<JsonElement, T> reader) {
            var currentElement = _stack.Peek();

            if (fieldName == null) {
                return reader(currentElement);
            }

            if (!TryPushProperty(fieldName, out var prop)) {
                return default;
            }

            try {
                return prop.ValueKind == JsonValueKind.Null || prop.ValueKind == JsonValueKind.Undefined
                    ? default
                    : reader.Invoke(prop);
            }
            finally {
                _stack.Pop();
            }
        }


        /// <summary>
        /// Reads a one-dimensional array from the current JSON element.
        /// </summary>
        /// <typeparam name="T">
        ///   The array element type.
        /// </typeparam>
        /// <param name="fieldName">
        ///   The name of the field to read from. If <see langword="null"/>, the value is read 
        ///   directly from the element at the top of the stack.
        /// </param>
        /// <param name="reader">
        ///   A delegate that will read the value from the provided element.
        /// </param>
        /// <returns>
        ///   The array read from the JSON element.
        /// </returns>
        /// <exception cref="ServiceResultException">
        ///   The JSON element is not an array.
        /// </exception>
        /// <exception cref="ServiceResultException">
        ///   The number of items in the array exceeds <see cref="IEncodingContext.MaxArrayLength"/>.
        /// </exception>
        private T?[]? ReadOneDimensionalArrayCore<T>(string? fieldName, Func<JsonElement, T> reader) {
            return ReadCore(fieldName, x => {
                if (x.ValueKind == JsonValueKind.Undefined || x.ValueKind == JsonValueKind.Null) {
                    return default;
                }

                if (x.ValueKind != JsonValueKind.Array) {
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnexpectedJsonType, x.ValueKind));
                }

                var result = new List<T>(x.GetArrayLength());

                foreach (var element in x.EnumerateArray()) {
                    if (_context.MaxArrayLength > 0 && result.Count >= _context.MaxArrayLength) {
                        throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded, string.Format(Resources.Error_MaximumArrayLengthExceeded, _context.MaxArrayLength));
                    }

                    _stack.Push(element);
                    try {
                        result.Add(reader.Invoke(element));
                    }
                    finally {
                        _stack.Pop();
                    }
                }

                return result.ToArray();
            });
        }


        /// <summary>
        /// Reads a multi-dimensional array from the current JSON element.
        /// </summary>
        /// <typeparam name="T">
        ///   The array element type.
        /// </typeparam>
        /// <param name="fieldName">
        ///   The name of the field to read from. If <see langword="null"/>, the value is read 
        ///   directly from the element at the top of the stack.
        /// </param>
        /// <param name="dimensions">
        ///   The expected dimensions of the encoded array.
        /// </param>
        /// <param name="reader">
        ///   A delegate that will read the value from the provided element.
        /// </param>
        /// <returns>
        ///   The array read from the JSON element.
        /// </returns>
        /// <exception cref="ServiceResultException">
        ///   The JSON element is not an array.
        /// </exception>
        /// <exception cref="ServiceResultException">
        ///   The number of items in the array exceeds <see cref="IEncodingContext.MaxArrayLength"/>.
        /// </exception>
        /// <exception cref="ServiceResultException">
        ///   The dimensions of the encoded array do not match the expected <paramref name="dimensions"/>.
        /// </exception>
        private Array ReadMultiDimensionalArrayCore<T>(string? fieldName, int[] dimensions, Func<JsonElement, T> reader) {
            if (_context.MaxArrayLength > 0) {
                var expectedItemCount = dimensions.Aggregate((a, b) => a * b);
                if (expectedItemCount > _context.MaxArrayLength) {
                    throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded, string.Format(Resources.Error_MaximumArrayLengthExceeded, _context.MaxArrayLength));
                }
            }

            return ReadCore(fieldName, x => {
                if (x.ValueKind == JsonValueKind.Undefined || x.ValueKind == JsonValueKind.Null) {
                    return default;
                }

                if (x.ValueKind != JsonValueKind.Array) {
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnexpectedJsonType, x.ValueKind));
                }

                var result = Array.CreateInstance(typeof(T), dimensions);
                var itemsRead = 0;
                ReadMultiDimensionalArrayCore(result, x, 0, new int[result.Rank], reader, ref itemsRead);

                return result;
            })!;
        }


        /// <summary>
        /// Recursively reads a multi-dimensional array from the current JSON element.
        /// </summary>
        /// <typeparam name="T">
        ///   The array element type.
        /// </typeparam>
        /// <param name="values">
        ///   The array to write the values to.
        /// </param>
        /// <param name="element">
        ///   The current JSON element.
        /// </param>
        /// <param name="currentDimension">
        ///   The current dimension being read.
        /// </param>
        /// <param name="indices">
        ///   The indices of the current element in the array.
        /// </param>
        /// <param name="reader">
        ///   A delegate that will read a value from the provided element.
        /// </param>
        /// <param name="itemsRead">
        ///   The number of items read so far.
        /// </param>
        /// <exception cref="ServiceResultException">
        ///   The dimensions of the encoded array do not match the expected dimensions.
        /// </exception>
        /// <exception cref="ServiceResultException">
        ///   The number of items in the array exceeds <see cref="IEncodingContext.MaxArrayLength"/>.
        /// </exception>
        private void ReadMultiDimensionalArrayCore<T>(Array values, JsonElement element, int currentDimension, int[] indices, Func<JsonElement, T> reader, ref int itemsRead) {
            if (currentDimension + 1 != values.Rank) {
                // We have not reached the expected last dimension yet; therefore the current
                // element must be an array.
                if (element.ValueKind != JsonValueKind.Array) {
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnexpectedJsonType, element.ValueKind));
                }

                var i = -1;
                foreach (var item in element.EnumerateArray()) {
                    _stack.Push(item);
                    indices[currentDimension] = ++i;
                    try {
                        ReadMultiDimensionalArrayCore(values, item, currentDimension + 1, indices, reader, ref itemsRead);
                    }
                    finally {
                        _stack.Pop();
                    }
                }

                return;
            }

            // We have reached the last dimension; therefore the current element must be a scalar.
            if (element.ValueKind == JsonValueKind.Array) {
                throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnexpectedJsonType, element.ValueKind));
            }

            values.SetValue(reader.Invoke(element), indices);
            ++itemsRead;
        }


        /// <inheritdoc/>
        public bool ReadBoolean(string? fieldName) {
            return ReadCore(fieldName, x => x.GetBoolean());
        }


        /// <inheritdoc/>
        public sbyte ReadSByte(string? fieldName) {
            return ReadCore(fieldName, x => x.GetSByte());
        }


        /// <inheritdoc/>
        public byte ReadByte(string? fieldName) {
            return ReadCore(fieldName, x => x.GetByte());
        }


        /// <inheritdoc/>
        public short ReadInt16(string? fieldName) {
            return ReadCore(fieldName, x => x.GetInt16());
        }


        /// <inheritdoc/>
        public ushort ReadUInt16(string? fieldName) {
            return ReadCore(fieldName, x => x.GetUInt16());
        }


        /// <inheritdoc/>
        public int ReadInt32(string? fieldName) {
            return ReadCore(fieldName, x => x.GetInt32());
        }


        /// <inheritdoc/>
        public uint ReadUInt32(string? fieldName) {
            return ReadCore(fieldName, x => x.GetUInt32());
        }


        /// <inheritdoc/>
        public long ReadInt64(string? fieldName) {
            // Technically 64-bit integers should be encoded as strings according to the OPC UA
            // JSON spec but we will allow them to be specified as numberical values.
            return ReadCore(fieldName, x => x.ValueKind == JsonValueKind.Number
                ? x.GetInt64()
                : long.Parse(x.GetString()!, CultureInfo.InvariantCulture));
        }


        /// <inheritdoc/>
        public ulong ReadUInt64(string? fieldName) {
            // Technically 64-bit integers should be encoded as strings according to the OPC UA
            // JSON spec but we will allow them to be specified as numberical values.
            return ReadCore(fieldName, x => x.ValueKind == JsonValueKind.Number
                ? x.GetUInt64()
                : ulong.Parse(x.GetString()!, CultureInfo.InvariantCulture));
        }


        /// <inheritdoc/>
        public float ReadFloat(string? fieldName) {
            return ReadCore(fieldName, x => x.GetSingle());
        }


        /// <inheritdoc/>
        public double ReadDouble(string? fieldName) {
            return ReadCore(fieldName, x => x.GetDouble());
        }


        /// <inheritdoc/>
        public string? ReadString(string? fieldName) {
            return ReadCore(fieldName, x => {
                var result = x.GetString();
                if (!string.IsNullOrEmpty(result) && _context.MaxStringLength > 0 && System.Text.Encoding.UTF8.GetByteCount(result) > _context.MaxStringLength) {
                    throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded, string.Format(Resources.Error_MaximumStringLengthExceeded, _context.MaxStringLength));
                }
                return result;
            });
        }


        /// <inheritdoc/>
        public DateTime ReadDateTime(string? fieldName) {
            return ReadCore(fieldName, x => x.GetDateTime());
        }


        /// <inheritdoc/>
        public Guid ReadGuid(string? fieldName) {
            return ReadCore(fieldName, x => x.GetGuid());
        }


        /// <inheritdoc/>
        public byte[]? ReadByteString(string? fieldName) {
            return ReadCore(fieldName, x => {
                var result = x.GetBytesFromBase64();
                if (_context.MaxByteStringLength > 0 && result.Length > _context.MaxByteStringLength) {
                    throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded, string.Format(Resources.Error_MaximumByteStringLengthExceeded, _context.MaxByteStringLength));
                }

                return result;
            });
        }


        /// <inheritdoc/>
        public XElement? ReadXElement(string? fieldName) {
            return ReadCore(fieldName, x => XElement.Parse(x.GetString()!));
        }


        /// <inheritdoc/>
        public NodeId ReadNodeId(string? fieldName) {
            return ReadCore(fieldName, x => {
                var idType = ReadInt32("IdType");
                var ns = ReadUInt16("Namespace");

                switch (idType) {
                    case 0:
                        // UInt32
                        return new NodeId(ReadUInt32("Id"), ns);
                    case 1:
                        // String
                        return new NodeId(ReadString("Id")!, ns);
                    case 2:
                        // Guid
                        return new NodeId(ReadGuid("Id"), ns);
                    case 3:
                        // ByteString
                        return new NodeId(ReadByteString("Id")!, ns);
                    default:
                        throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_InvalidNodeIdType, idType));
                }
            })!;
        }


        /// <inheritdoc/>
        public ExpandedNodeId ReadExpandedNodeId(string? fieldName) {
            return ReadCore(fieldName, x => {
                var idType = ReadInt32("IdType");
                var serverIndex = ReadUInt32("ServerUri");

                ushort? nsIndex = null;
                string? nsUri = null;

                if (HasProperty("Namespace", out var valueKind)) {
                    switch (valueKind) {
                        case JsonValueKind.Number:
                            nsIndex = ReadUInt16("Namespace");
                            break;
                        case JsonValueKind.String:
                            nsUri = ReadString("Namespace");
                            break;
                        default:
                            throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnexpectedJsonType, valueKind));
                    }
                }

                switch (idType) {
                    case 0:
                        // UInt32
                        return new ExpandedNodeId(new NodeId(ReadUInt32("Id"), nsIndex ?? 0), nsUri, serverIndex);
                    case 1:
                        // String
                        return new ExpandedNodeId(new NodeId(ReadString("Id")!, nsIndex ?? 0), nsUri, serverIndex);
                    case 2:
                        // Guid
                        return new ExpandedNodeId(new NodeId(ReadGuid("Id"), nsIndex ?? 0), nsUri, serverIndex);
                    case 3:
                        // ByteString
                        return new ExpandedNodeId(new NodeId(ReadByteString("Id")!, nsIndex ?? 0), nsUri, serverIndex);
                    default:
                        throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_InvalidNodeIdType, idType));
                }
            })!;
        }


        /// <inheritdoc/>
        public StatusCode ReadStatusCode(string? fieldName) {
            return ReadCore(fieldName, x => new StatusCode(x.GetUInt32()));
        }


        /// <inheritdoc/>
        public QualifiedName ReadQualifiedName(string? fieldName) {
            return ReadCore(fieldName, x => {
                var name = ReadString("Name")!;
                var ns = ReadUInt16("Uri");
                return new QualifiedName(name, ns);
            })!;
        }


        /// <inheritdoc/>
        public LocalizedText ReadLocalizedText(string? fieldName) {
            return ReadCore(fieldName, x => {
                var locale = ReadString("Locale")!;
                var text = ReadString("Text")!;
                return new LocalizedText(text, locale);
            })!;
        }


        /// <inheritdoc/>
        public ExtensionObject? ReadExtensionObject(string? fieldName) {
            return ReadCore(fieldName, x => {
                var encoding = ReadInt32("Encoding");
                if (encoding < 0 || encoding > 2) {
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnknownEncoding, encoding));
                }
                
                var typeId = ReadNodeId("TypeId");
                var expandedTypeId = NodeId.ToExpandedNodeId(typeId, _context.NamespaceUris);

                if (!TypeLibrary.TryGetTypeFromBinaryEncodingId(expandedTypeId, out var type)) {
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_CannotFindTypeForTypeId, expandedTypeId));
                }

                if (!TryPushProperty("Body", out var body)) {
                    return default;
                }

                try {
                    if (body.ValueKind == JsonValueKind.Null || body.ValueKind == JsonValueKind.Undefined) {
                        return default;
                    }

                    if (encoding == 0) {
                        // Body is a JSON object: decode using this decoder.
                        var encodable = (IEncodable) Activator.CreateInstance(type)!;
                        encodable.Decode(this);
                        return new ExtensionObject(encodable, expandedTypeId);
                    }
                    else if (encoding == 1) {
                        // Body is a ByteString: decode using binary decoder.
                        var encodable = (IEncodable) Activator.CreateInstance(type)!;

                        var bytes = ReadByteString(null);
                        using var stream = JsonEncodingProvider.MemoryStreamProvider.GetStream(bytes!);
                        using var binaryDecoder = new BinaryDecoder(stream, _context, true);

                        encodable.Decode(binaryDecoder);
                        return new ExtensionObject(encodable, expandedTypeId);
                    }
                    else if (encoding == 2) {
                        // Body is an XML element
                        var xml = ReadXElement(null);
                        return new ExtensionObject(xml, expandedTypeId);
                    }
                }
                finally {
                    _stack.Pop();
                }

                // Should never get here
                throw new InvalidOperationException();
            });
        }


        /// <inheritdoc/>
        T? IDecoder.ReadExtensionObject<T>(string? fieldName) where T : class {
            return ReadCore(fieldName, x => {
                var encoding = ReadInt32("Encoding");
                if (encoding < 0 || encoding > 2) {
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnknownEncoding, encoding));
                }

                var typeId = ReadNodeId("TypeId");
                var expandedTypeId = NodeId.ToExpandedNodeId(typeId, _context.NamespaceUris);

                if (!TypeLibrary.TryGetTypeFromBinaryEncodingId(expandedTypeId, out var type)) {
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_CannotFindTypeForTypeId, expandedTypeId));
                }

                if (!TryPushProperty("Body", out var body)) {
                    return default;
                }

                try {
                    if (body.ValueKind == JsonValueKind.Null || body.ValueKind == JsonValueKind.Undefined) {
                        return default;
                    }

                    if (encoding == 0) {
                        // Body is a JSON object: decode using this decoder.
                        var encodable = (IEncodable) Activator.CreateInstance(type)!;
                        encodable.Decode(this);
                        return (T) encodable;
                    }
                    else if (encoding == 1) {
                        // Body is a ByteString: decode using binary decoder.
                        var encodable = (IEncodable) Activator.CreateInstance(type)!;

                        var bytes = ReadByteString(null);
                        using var stream = JsonEncodingProvider.MemoryStreamProvider.GetStream(bytes!);
                        using var binaryDecoder = new BinaryDecoder(stream, _context, true);

                        encodable.Decode(binaryDecoder);
                        return (T) encodable;
                    }
                    else if (encoding == 2) {
                        // Body is an XML element. We can only continue if we can create an XML decoder.
                        var xmlDecoder = _options.XmlDecoderFactory?.Invoke(_context, ReadXElement(null)!);
                        if (xmlDecoder != null) {
                            var encodable = (IEncodable) Activator.CreateInstance(type)!;
                            encodable.Decode(xmlDecoder);
                            return (T) encodable;
                        }

                        throw new ServiceResultException(StatusCodes.BadEncodingError, Resources.Error_DecodingXmlToClrTypeIsNotSupported);
                    }
                }
                finally {
                    _stack.Pop();
                }

                // Should never get here
                throw new InvalidOperationException();
            });
        }


        /// <inheritdoc/>
        public DataValue ReadDataValue(string? fieldName) {
            return ReadCore(fieldName, x => {
                var value = ReadVariant("Value");
                var statusCode = ReadStatusCode("Status");
                var sourceTimestamp = ReadDateTime("SourceTimestamp");
                var sourcePicoseconds = ReadUInt16("SourcePicoseconds");
                var serverTimestamp = ReadDateTime("ServerTimestamp");
                var serverPicoseconds = ReadUInt16("ServerPicoseconds");

                return new DataValue(value, statusCode, sourceTimestamp, sourcePicoseconds, serverTimestamp, serverPicoseconds);
            })!;
        }


        /// <inheritdoc/>
        public Variant ReadVariant(string? fieldName) {
            return ReadCore(fieldName, x => {
                if (!x.TryGetProperty("Body", out var body)) {
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_JsonPropertyNotFound, "Body"));
                }

                var type = (VariantType) ReadInt32("Type");
                var arrayDimensions = ReadInt32Array("Dimensions");

                if (arrayDimensions == null) {
                    // Scalar or one-dimensional array

                    var isArray = body.ValueKind == JsonValueKind.Array;
                    return ReadScalarOrOneDimensionalVariantCore("Body", type, isArray);
                }

                // Multi-dimensional array
                return ReadMultiDimensionalVariantCore("Body", type, arrayDimensions);
            });
        }


        /// <summary>
        /// Reads a scalar or one-dimensional variant array from the current JSON element.
        /// </summary>
        /// <param name="fieldName">
        ///   The name of the field to read from.
        /// </param>
        /// <param name="type">
        ///   The variant type.
        /// </param>
        /// <param name="isArray">
        ///   <see langword="true"/> to read a one-dimensional array, or <see langword="false"/> 
        ///   to read a scalar value.
        /// </param>
        /// <returns>
        ///   The variant value.
        /// </returns>
        /// <exception cref="ServiceResultException">
        ///   <paramref name="type"/> is not supported.
        /// </exception>
        private Variant ReadScalarOrOneDimensionalVariantCore(string? fieldName, VariantType type, bool isArray) {
            return type switch {
                VariantType.Boolean => isArray ? new Variant(ReadBooleanArray(fieldName)) : new Variant(ReadBoolean(fieldName)),
                VariantType.SByte => isArray ? new Variant(ReadSByteArray(fieldName)) : new Variant(ReadSByte(fieldName)),
                VariantType.Byte => isArray ? new Variant(ReadByteArray(fieldName)) : new Variant(ReadByte(fieldName)),
                VariantType.Int16 => isArray ? new Variant(ReadInt16Array(fieldName)) : new Variant(ReadInt16(fieldName)),
                VariantType.UInt16 => isArray ? new Variant(ReadUInt16Array(fieldName)) : new Variant(ReadUInt16(fieldName)),
                VariantType.Int32 => isArray ? new Variant(ReadInt32Array(fieldName)) : new Variant(ReadInt32(fieldName)),
                VariantType.UInt32 => isArray ? new Variant(ReadUInt32Array(fieldName)) : new Variant(ReadUInt32(fieldName)),
                VariantType.Int64 => isArray ? new Variant(ReadInt64Array(fieldName)) : new Variant(ReadInt64(fieldName)),
                VariantType.UInt64 => isArray ? new Variant(ReadUInt64Array(fieldName)) : new Variant(ReadUInt64(fieldName)),
                VariantType.Float => isArray ? new Variant(ReadFloatArray(fieldName)) : new Variant(ReadFloat(fieldName)),
                VariantType.Double => isArray ? new Variant(ReadDoubleArray(fieldName)) : new Variant(ReadDouble(fieldName)),
                VariantType.String => isArray ? new Variant(ReadStringArray(fieldName)) : new Variant(ReadString(fieldName)),
                VariantType.DateTime => isArray ? new Variant(ReadDateTimeArray(fieldName)) : new Variant(ReadDateTime(fieldName)),
                VariantType.Guid => isArray ? new Variant(ReadGuidArray(fieldName)) : new Variant(ReadGuid(fieldName)),
                VariantType.ByteString => isArray ? new Variant(ReadByteStringArray(fieldName)) : new Variant(ReadByteString(fieldName)),
                VariantType.XmlElement => isArray ? new Variant(ReadXElementArray(fieldName)) : new Variant(ReadXElement(fieldName)),
                VariantType.NodeId => isArray ? new Variant(ReadNodeIdArray(fieldName)) : new Variant(ReadNodeId(fieldName)),
                VariantType.ExpandedNodeId => isArray ? new Variant(ReadExpandedNodeIdArray(fieldName)) : new Variant(ReadExpandedNodeId(fieldName)),
                VariantType.StatusCode => isArray ? new Variant(ReadStatusCodeArray(fieldName)) : new Variant(ReadStatusCode(fieldName)),
                VariantType.QualifiedName => isArray ? new Variant(ReadQualifiedNameArray(fieldName)) : new Variant(ReadQualifiedName(fieldName)),
                VariantType.LocalizedText => isArray ? new Variant(ReadLocalizedTextArray(fieldName)) : new Variant(ReadLocalizedText(fieldName)),
                VariantType.ExtensionObject => isArray ? new Variant(ReadExtensionObjectArray(fieldName)) : new Variant(ReadExtensionObject(fieldName)),
                _ => throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnsupportedVariantType, type))
            };
        }


        /// <summary>
        /// Reads a multi-dimensional variant array from the current JSON element.
        /// </summary>
        /// <param name="fieldName">
        ///   The name of the field to read from.
        /// </param>
        /// <param name="type">
        ///   The variant type.
        /// </param>
        /// <param name="dimensions">
        ///   The expected dimensions of the encoded array.
        /// </param>
        /// <returns>
        ///   The variant value.
        /// </returns>
        /// <exception cref="ServiceResultException">
        ///   <paramref name="type"/> is not supported.
        /// </exception>
        private Variant ReadMultiDimensionalVariantCore(string? fieldName, VariantType type, int[] dimensions) {
            var array = type switch {
                VariantType.Boolean => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadBoolean(null)),
                VariantType.SByte => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadSByte(null)),
                VariantType.Byte => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadByte(null)),
                VariantType.Int16 => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadInt16(null)),
                VariantType.UInt16 => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadUInt16(null)),
                VariantType.Int32 => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadInt32(null)),
                VariantType.UInt32 => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadUInt32(null)),
                VariantType.Int64 => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadInt64(null)),
                VariantType.UInt64 => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadUInt64(null)),
                VariantType.Float => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadFloat(null)),
                VariantType.Double => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadDouble(null)),
                VariantType.String => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadString(null)),
                VariantType.DateTime => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadDateTime(null)),
                VariantType.Guid => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadGuid(null)),
                VariantType.ByteString => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadByteString(null)),
                VariantType.XmlElement => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadXElement(null)),
                VariantType.NodeId => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadNodeId(null)),
                VariantType.ExpandedNodeId => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadExpandedNodeId(null)),
                VariantType.StatusCode => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadStatusCode(null)),
                VariantType.QualifiedName => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadQualifiedName(null)),
                VariantType.LocalizedText => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadLocalizedText(null)),
                VariantType.ExtensionObject => ReadMultiDimensionalArrayCore(fieldName, dimensions, _ => ReadExtensionObject(null)),
                _ => throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnsupportedVariantType, type))
            };

            return new Variant(array);
        }


        /// <inheritdoc/>
        public DiagnosticInfo ReadDiagnosticInfo(string? fieldName) {
            return ReadCore(fieldName, x => {
                var symbolicId = ReadInt32("SymbolicId");
                var namespaceUri = ReadInt32("NamespaceUri");
                var locale = ReadInt32("Locale");
                var localizedText = ReadInt32("LocalizedText");
                var additionalInfo = ReadString("AdditionalInfo");
                var innerStatusCode = ReadStatusCode("InnerStatusCode");
                var innerDiagnosticInfo = ReadDiagnosticInfo("InnerDiagnosticInfo");

                return new DiagnosticInfo(symbolicId, namespaceUri, locale, localizedText, additionalInfo, innerStatusCode, innerDiagnosticInfo);
            })!;
        }


        /// <inheritdoc/>
        T IDecoder.ReadEncodable<T>(string? fieldName) {
            return ReadCore(fieldName, x => {
                var encodable = (IEncodable) Activator.CreateInstance(typeof(T))!;
                encodable.Decode(this);
                return (T) encodable;
            })!;
        }


        /// <inheritdoc/>
        public IServiceResponse ReadResponse() {
            return ((IDecoder) this).ReadEncodable<IServiceResponse>(null);
        }


        /// <inheritdoc/>
        public T ReadEnumeration<T>(string? fieldName) where T : struct, IConvertible {
            return ReadCore(fieldName, x => (T) Convert.ChangeType(ReadInt32(null), typeof(T)));
        }


        /// <inheritdoc/>
        public bool[]? ReadBooleanArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadBoolean(null));
        }


        /// <inheritdoc/>
        public sbyte[]? ReadSByteArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadSByte(null));
        }


        /// <inheritdoc/>
        public byte[]? ReadByteArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadByte(null));
        }


        /// <inheritdoc/>
        public short[]? ReadInt16Array(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadInt16(null));
        }


        /// <inheritdoc/>
        public ushort[]? ReadUInt16Array(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadUInt16(null));
        }


        /// <inheritdoc/>
        public int[]? ReadInt32Array(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadInt32(null));
        }


        /// <inheritdoc/>
        public uint[]? ReadUInt32Array(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadUInt32(null));
        }


        /// <inheritdoc/>
        public long[]? ReadInt64Array(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadInt64(null));
        }


        /// <inheritdoc/>
        public ulong[]? ReadUInt64Array(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadUInt64(null));
        }


        /// <inheritdoc/>
        public float[]? ReadFloatArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadFloat(null));
        }


        /// <inheritdoc/>
        public double[]? ReadDoubleArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadDouble(null));
        }


        /// <inheritdoc/>
        public string?[]? ReadStringArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadString(null));
        }


        /// <inheritdoc/>
        public DateTime[]? ReadDateTimeArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadDateTime(null));
        }


        /// <inheritdoc/>
        public Guid[]? ReadGuidArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadGuid(null));
        }


        /// <inheritdoc/>
        public byte[]?[]? ReadByteStringArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadByteString(null));
        }


        /// <inheritdoc/>
        public XElement?[]? ReadXElementArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadXElement(null));
        }


        /// <inheritdoc/>
        public NodeId[]? ReadNodeIdArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadNodeId(null))!;
        }


        /// <inheritdoc/>
        public ExpandedNodeId[]? ReadExpandedNodeIdArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadExpandedNodeId(null))!;
        }


        /// <inheritdoc/>
        public StatusCode[]? ReadStatusCodeArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadStatusCode(null));
        }


        /// <inheritdoc/>
        public QualifiedName[]? ReadQualifiedNameArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadQualifiedName(null))!;
        }


        /// <inheritdoc/>
        public LocalizedText[]? ReadLocalizedTextArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadLocalizedText(null))!;
        }


        /// <inheritdoc/>
        public ExtensionObject?[]? ReadExtensionObjectArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadExtensionObject(null));
        }


        /// <inheritdoc/>
        T?[]? IDecoder.ReadExtensionObjectArray<T>(string? fieldName) where T : class {
            return ReadOneDimensionalArrayCore(fieldName, _ => ((IDecoder) this).ReadExtensionObject<T>(null));
        }


        /// <inheritdoc/>
        public DataValue[]? ReadDataValueArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadDataValue(null))!;
        }


        /// <inheritdoc/>
        public Variant[]? ReadVariantArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadVariant(null))!;
        }


        /// <inheritdoc/>
        public DiagnosticInfo[]? ReadDiagnosticInfoArray(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadDiagnosticInfo(null))!;
        }


        /// <inheritdoc/>
        T[]? IDecoder.ReadEncodableArray<T>(string? fieldName) {
            return ReadOneDimensionalArrayCore(fieldName, _ => ((IDecoder) this).ReadEncodable<T>(null))!;
        }


        /// <inheritdoc/>
        public T[]? ReadEnumerationArray<T>(string fieldName) where T : struct, IConvertible {
            return ReadOneDimensionalArrayCore(fieldName, _ => ReadEnumeration<T>(null))!;
        }


        /// <inheritdoc/>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _document.Dispose();

            if (!_keepStreamOpenOnDispose) {
                _stream?.Dispose();
            }

            _disposed = true;
        }

    }

}
