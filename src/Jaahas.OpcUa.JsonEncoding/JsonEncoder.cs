using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace Jaahas.OpcUa.JsonEncoding {

    /// <summary>
    /// <see cref="IEncoder"/> that encodes data using the OPC UA JSON encoding.
    /// </summary>
    /// <remarks>
    ///   The OPC UA JSON encoding is defined <a href="https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4">here</a>.
    /// </remarks>
    public sealed class JsonEncoder : IEncoder {

        /// <summary>
        /// Flags if the encoder has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The encoding context, if provided.
        /// </summary>
        private readonly IEncodingContext _context;

        /// <summary>
        /// The stream to write to.
        /// </summary>
        private readonly Stream? _stream;

        /// <summary>
        /// When <see langword="true"/>, the <see cref="_stream"/> will be kept open when the 
        /// encoder is disposed.
        /// </summary>
        private readonly bool _keepStreamOpenOnDispose;

        /// <summary>
        /// The JSON writer.
        /// </summary>
        private readonly Utf8JsonWriter _writer;

        /// <summary>
        /// The encoder options.
        /// </summary>
        private readonly JsonEncoderOptions _options;


        /// <summary>
        /// Creates a new <see cref="JsonEncoder"/> instance that writes to the specified 
        /// <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">
        ///   The stream to write to.
        /// </param>
        /// <param name="context">
        ///   The encoding context.
        /// </param>
        /// <param name="options">
        ///   The encoder options.
        /// </param>
        /// <param name="keepStreamOpen">
        ///   <see langword="true"/> to keep the <paramref name="stream"/> open when the encoder 
        ///   is disposed, or <see langword="false"/> to dispose of the <paramref name="stream"/> 
        ///   when the encoder is disposed.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        public JsonEncoder(Stream stream, IEncodingContext? context = null, JsonEncoderOptions? options = null, bool keepStreamOpen = false) {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _context = context ?? new DefaultEncodingContext();
            _options = options ?? new JsonEncoderOptions();
            _keepStreamOpenOnDispose = keepStreamOpen;

            _writer = new Utf8JsonWriter(stream, new JsonWriterOptions {
                Indented = _options.WriteIndented,
                SkipValidation = true
            });
        }


        /// <summary>
        /// Creates a new <see cref="JsonEncoder"/> instance that writes to the specified 
        /// <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">
        ///   The buffer to write to.
        /// </param>
        /// <param name="context">
        ///   The encoding context.
        /// </param>
        /// <param name="options">
        ///   The encoder options.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="buffer"/> is <see langword="null"/>.
        /// </exception>
        public JsonEncoder(IBufferWriter<byte> buffer, IEncodingContext? context = null, JsonEncoderOptions? options = null) {
            _context = context ?? new DefaultEncodingContext();
            _options = options ?? new JsonEncoderOptions();

            _writer = new Utf8JsonWriter(buffer ?? throw new ArgumentNullException(nameof(buffer)), new JsonWriterOptions {
                Indented = _options.WriteIndented,
                SkipValidation = true
            });
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
        /// Writes a null value.
        /// </summary>
        /// <param name="fieldName">
        ///   The field name for the null value.
        /// </param>
        private void WriteNull(string? fieldName = null) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.1

            if (fieldName != null) {
                if (_options.UseReversibleEncoding) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNullValue();
        }


        /// <inheritdoc/>
        public void WriteBoolean(string? fieldName, bool value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.2

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteBooleanValue(value);
        }


        /// <inheritdoc/>
        public void WriteSByte(string? fieldName, sbyte value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.3

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNumberValue(value);
        }


        /// <inheritdoc/>
        public void WriteByte(string? fieldName, byte value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.3

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNumberValue(value);
        }


        /// <inheritdoc/>
        public void WriteInt16(string? fieldName, short value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.3

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNumberValue(value);
        }


        /// <inheritdoc/>
        public void WriteUInt16(string? fieldName, ushort value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.3

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNumberValue(value);
        }


        /// <inheritdoc/>
        public void WriteInt32(string? fieldName, int value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.3

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNumberValue(value);
        }


        /// <inheritdoc/>
        public void WriteUInt32(string? fieldName, uint value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.3

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNumberValue(value);
        }


        /// <inheritdoc/>
        public void WriteInt64(string? fieldName, long value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.3

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            // Int64 is encoded as a string.
            _writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }


        /// <inheritdoc/>
        public void WriteUInt64(string? fieldName, ulong value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.3

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            // UInt64 is encoded as a string.
            _writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }


        /// <inheritdoc/>
        public void WriteFloat(string? fieldName, float value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.4

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNumberValue(value);
        }


        /// <inheritdoc/>
        public void WriteDouble(string? fieldName, double value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.4

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteNumberValue(value);
        }


        /// <inheritdoc/>
        public void WriteString(string? fieldName, string? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.5

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            if (_context.MaxStringLength > 0 && Encoding.UTF8.GetByteCount(value) > _context.MaxStringLength) {
                throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded, string.Format(Resources.Error_MaximumStringLengthExceeded, _context.MaxStringLength));
            }

            _writer.WriteStringValue(value);
        }


        /// <inheritdoc/>
        public void WriteDateTime(string? fieldName, DateTime value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.6

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteStringValue(value);
        }


        /// <inheritdoc/>
        public void WriteGuid(string? fieldName, Guid value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.7

            if (fieldName != null) {
                if (_options.UseReversibleEncoding && value == default) {
                    return;
                }
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteStringValue(value);
        }


        /// <inheritdoc/>
        public void WriteByteString(string? fieldName, byte[]? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.8

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            if (_context.MaxByteStringLength > 0 && value.Length > _context.MaxByteStringLength) {
                throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded, string.Format(Resources.Error_MaximumByteStringLengthExceeded, _context.MaxByteStringLength));
            }

            _writer.WriteBase64StringValue(value);
        }


        /// <inheritdoc/>
        public void WriteXElement(string? fieldName, XElement? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.9

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }
            _writer.WriteStringValue(value.ToString());
        }


        /// <inheritdoc/>
        public void WriteNodeId(string? fieldName, NodeId? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.10

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            _writer.WriteStartObject();

            switch (value.IdType) {
                case IdType.String:
                case IdType.Guid:
                case IdType.Opaque:
                    WriteInt32("IdType", (int) value.IdType);
                    break;
            }

            switch (value.IdType) {
                case IdType.Numeric:
                    WriteUInt32("Id", (uint) value.Identifier);
                    break;
                case IdType.String:
                    WriteString("Id", (string) value.Identifier);
                    break;
                case IdType.Guid:
                    WriteGuid("Id", (Guid) value.Identifier);
                    break;
                case IdType.Opaque:
                    WriteByteString("Id", (byte[]) value.Identifier);
                    break;
            }

            if (value.NamespaceIndex != 0) {
                if (_options.UseReversibleEncoding) {
                    WriteUInt16("Namespace", value.NamespaceIndex);
                }
                else if (_context.NamespaceUris?.Count > value.NamespaceIndex) {
                    WriteString("Namespace", _context.NamespaceUris[value.NamespaceIndex]);
                }
                else {
                    WriteUInt16("Namespace", value.NamespaceIndex);
                }
            }

            _writer.WriteEndObject();
        }


        /// <inheritdoc/>
        public void WriteExpandedNodeId(string? fieldName, ExpandedNodeId? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.11

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            _writer.WriteStartObject();

            switch (value.NodeId.IdType) {
                case IdType.String:
                case IdType.Guid:
                case IdType.Opaque:
                    WriteInt32("IdType", (int) value.NodeId.IdType);
                    break;
            }

            switch (value.NodeId.IdType) {
                case IdType.Numeric:
                    WriteUInt32("Id", (uint) value.NodeId.Identifier);
                    break;
                case IdType.String:
                    WriteString("Id", (string) value.NodeId.Identifier);
                    break;
                case IdType.Guid:
                    WriteGuid("Id", (Guid) value.NodeId.Identifier);
                    break;
                case IdType.Opaque:
                    WriteByteString("Id", (byte[]) value.NodeId.Identifier);
                    break;
            }

            if (value.NodeId.NamespaceIndex != 0) {
                if (value.NodeId.NamespaceIndex != 1) {
                    if (value.NamespaceUri != null) {
                        WriteString("Namespace", value.NamespaceUri);
                    }
                    else if (_context.NamespaceUris?.Count > value.NodeId.NamespaceIndex) {
                        WriteString("Namespace", _context.NamespaceUris[value.NodeId.NamespaceIndex]);
                    }
                }

                WriteUInt16("NamespaceIndex", value.NodeId.NamespaceIndex);
            }

            if (value.ServerIndex != 0) {
                if (_options.UseReversibleEncoding) {
                    WriteUInt32("ServerUri", value.ServerIndex);
                }
                else if (_context.ServerUris?.Count > value.ServerIndex) {
                    WriteString("ServerUri", _context.ServerUris[(int) value.ServerIndex]);
                }
                else {
                    WriteUInt32("ServerUri", value.ServerIndex);
                }
            }

            _writer.WriteEndObject();
        }


        /// <inheritdoc/>
        public void WriteStatusCode(string? fieldName, StatusCode value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.12

            if (_options.UseReversibleEncoding) {
                WriteUInt32(fieldName, value.Value);
            }
            else {
                if (fieldName != null) {
                    if (value.Value == StatusCodes.Good) {
                        return;
                    }

                    _writer.WritePropertyName(fieldName);
                }
                _writer.WriteStartObject();
                WriteUInt32("Code", value.Value);
                WriteString("Symbol", value.GetSymbolName() ?? "");
                _writer.WriteEndObject();
            }
        }


        /// <inheritdoc/>
        public void WriteQualifiedName(string? fieldName, QualifiedName? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.14

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            _writer.WriteStartObject();

            WriteString("Name", value.Name);

            if (value.NamespaceIndex != 0) {
                if (_options.UseReversibleEncoding) {
                    WriteUInt16("Namespace", value.NamespaceIndex);
                }
                else if (_context.NamespaceUris?.Count > value.NamespaceIndex) {
                    WriteString("Namespace", _context.NamespaceUris[value.NamespaceIndex]);
                }
                else {
                    WriteUInt16("Namespace", value.NamespaceIndex);
                }
            }

            _writer.WriteEndObject();
        }


        /// <inheritdoc/>
        public void WriteLocalizedText(string? fieldName, LocalizedText? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.15

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            if (_options.UseReversibleEncoding) {
                _writer.WriteStartObject();
                WriteString("Locale", value.Locale);
                WriteString("Text", value.Text);
                _writer.WriteEndObject();
            }
            else {
                _writer.WriteStringValue(value.Text);
            }
        }


        /// <inheritdoc/>
        public void WriteExtensionObject(string? fieldName, ExtensionObject? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.16

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            if (_options.UseReversibleEncoding) {
                _writer.WriteStartObject();
                WriteExpandedNodeId("TypeId", value.TypeId);
                // Skip Encoding property
                _writer.WritePropertyName("Body");

                switch (value.BodyType) {
                    case BodyType.None:
                        _writer.WriteNullValue();
                        break;
                    case BodyType.ByteString:
                        WriteByteString(null, (byte[]?) value.Body);
                        break;
                    case BodyType.XmlElement:
                        WriteXElement(null, (XElement?) value.Body);
                        break;
                    default:
                        var encodable = (IEncodable?) value.Body;
                        if (encodable == null) {
                            _writer.WriteNullValue();
                        }
                        else {
                            _writer.WriteStartObject();
                            encodable.Encode(this);
                            _writer.WriteEndObject();
                        }
                        break;
                }

                _writer.WriteEndObject();
            }
            else {
                switch (value.BodyType) {
                    case BodyType.None:
                        _writer.WriteNullValue();
                        break;
                    case BodyType.ByteString:
                        WriteByteString(null, (byte[]?) value.Body);
                        break;
                    case BodyType.XmlElement:
                        WriteXElement(null, (XElement?) value.Body);
                        break;
                    default:
                        var encodable = (IEncodable?) value.Body;
                        if (encodable == null) {
                            _writer.WriteNullValue();
                        }
                        else {
                            _writer.WriteStartObject();
                            encodable.Encode(this);
                            _writer.WriteEndObject();
                        }
                        break;
                }
            }
        }


        /// <inheritdoc/>
        void IEncoder.WriteExtensionObject<T>(string? fieldName, T? value) where T : class {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.16

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            var type = typeof(T);
            if (!TypeLibrary.TryGetBinaryEncodingIdFromType(type, out var typeId)) {
                throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_CannotFindTypeIdForType, type.FullName));
            }

            WriteExtensionObject(fieldName, new ExtensionObject(value, typeId));
        }


        /// <inheritdoc/>
        public void WriteDataValue(string? fieldName, DataValue? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.18

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            _writer.WriteStartObject();
            if (!Variant.IsNull(value.Variant)) {
                WriteVariant("Value", value.Variant);
            }
            if (value.StatusCode != StatusCodes.Good) {
                WriteStatusCode("StatusCode", value.StatusCode);
            }
            if (value.SourceTimestamp != default) {
                WriteDateTime("SourceTimestamp", value.SourceTimestamp);
            }
            if (value.SourcePicoseconds != default) {
                WriteUInt16("SourcePicoseconds", value.SourcePicoseconds);
            }
            if (value.ServerTimestamp != default) {
                WriteDateTime("ServerTimestamp", value.ServerTimestamp);
            }
            if (value.ServerPicoseconds != default) {
                WriteUInt16("ServerPicoseconds", value.ServerPicoseconds);
            }
            _writer.WriteEndObject();
        }


        /// <inheritdoc/>
        public void WriteVariant(string? fieldName, Variant value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.17

            if (Variant.IsNull(value)) {
                WriteNull(fieldName);
                return;
            }

            if (!_options.UseReversibleEncoding) {
                WriteVariantBody(fieldName, value);
                return;
            }
            
            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            _writer.WriteStartObject();
            WriteInt32("Type", (int) value.Type);
            WriteVariantBody("Body", value);
            if (value.ArrayDimensions != null && value.ArrayDimensions.Length > 1) {
                // Array dimensions are only included for multi-dimensional arrays.
                WriteInt32Array("Dimensions", value.ArrayDimensions);
            }
            _writer.WriteEndObject();
        }


        /// <summary>
        /// Writes the body of a <see cref="Variant"/> (i.e. the actual variant value).
        /// </summary>
        /// <param name="fieldName">
        ///   The optional field name for the body.
        /// </param>
        /// <param name="value">
        ///   The variant.
        /// </param>
        /// <exception cref="ServiceResultException">
        ///   The variant type is not supported.
        /// </exception>
        private void WriteVariantBody(string? fieldName, Variant value) {
            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            if (value.ArrayDimensions == null) {
                // Value is scalar

                switch (value.Type) {
                    case VariantType.Boolean:
                        WriteBoolean(null, (bool) value.Value!);
                        break;
                    case VariantType.SByte:
                        WriteSByte(null, (sbyte) value.Value!);
                        break;
                    case VariantType.Byte:
                        WriteByte(null, (byte) value.Value!);
                        break;
                    case VariantType.Int16:
                        WriteInt16(null, (short) value.Value!);
                        break;
                    case VariantType.UInt16:
                        WriteUInt16(null, (ushort) value.Value!);
                        break;
                    case VariantType.Int32:
                        WriteInt32(null, (int) value.Value!);
                        break;
                    case VariantType.UInt32:
                        WriteUInt32(null, (uint) value.Value!);
                        break;
                    case VariantType.Int64:
                        WriteInt64(null, (long) value.Value!);
                        break;
                    case VariantType.UInt64:
                        WriteUInt64(null, (ulong) value.Value!);
                        break;
                    case VariantType.Float:
                        WriteFloat(null, (float) value.Value!);
                        break;
                    case VariantType.Double:
                        WriteDouble(null, (double) value.Value!);
                        break;
                    case VariantType.String:
                        WriteString(null, (string) value.Value!);
                        break;
                    case VariantType.DateTime:
                        WriteDateTime(null, (DateTime) value.Value!);
                        break;
                    case VariantType.Guid:
                        WriteGuid(null, (Guid) value.Value!);
                        break;
                    case VariantType.ByteString:
                        WriteByteString(null, (byte[]) value.Value!);
                        break;
                    case VariantType.XmlElement:
                        WriteXElement(null, (XElement) value.Value!);
                        break;
                    case VariantType.NodeId:
                        WriteNodeId(null, (NodeId) value.Value!);
                        break;
                    case VariantType.ExpandedNodeId:
                        WriteExpandedNodeId(null, (ExpandedNodeId) value.Value!);
                        break;
                    case VariantType.StatusCode:
                        WriteStatusCode(null, (StatusCode) value.Value!);
                        break;
                    case VariantType.QualifiedName:
                        WriteQualifiedName(null, (QualifiedName) value.Value!);
                        break;
                    case VariantType.LocalizedText:
                        WriteLocalizedText(null, (LocalizedText) value.Value!);
                        break;
                    case VariantType.ExtensionObject:
                        WriteExtensionObject(null, (ExtensionObject) value.Value!);
                        break;
                    default:
                        throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnsupportedVariantType, value.Type));
                }

                return;
            }

            // Value is an array

            switch (value.Type) {
                case VariantType.Boolean:
                    WriteBooleanArray(null, (bool[]?) value.Value);
                    break;
                case VariantType.SByte:
                    WriteSByteArray(null, (sbyte[]?) value.Value);
                    break;
                case VariantType.Byte:
                    WriteByteArray(null, (byte[]?) value.Value);
                    break;
                case VariantType.Int16:
                    WriteInt16Array(null, (short[]?) value.Value);
                    break;
                case VariantType.UInt16:
                    WriteUInt16Array(null, (ushort[]?) value.Value);
                    break;
                case VariantType.Int32:
                    WriteInt32Array(null, (int[]?) value.Value);
                    break;
                case VariantType.UInt32:
                    WriteUInt32Array(null, (uint[]?) value.Value);
                    break;
                case VariantType.Int64:
                    WriteInt64Array(null, (long[]?) value.Value);
                    break;
                case VariantType.UInt64:
                    WriteUInt64Array(null, (ulong[]?) value.Value);
                    break;
                case VariantType.Float:
                    WriteFloatArray(null, (float[]?) value.Value);
                    break;
                case VariantType.Double:
                    WriteDoubleArray(null, (double[]?) value.Value);
                    break;
                case VariantType.String:
                    WriteStringArray(null, (string[]?) value.Value);
                    break;
                case VariantType.DateTime:
                    WriteDateTimeArray(null, (DateTime[]?) value.Value);
                    break;
                case VariantType.Guid:
                    WriteGuidArray(null, (Guid[]?) value.Value);
                    break;
                case VariantType.ByteString:
                    WriteByteStringArray(null, (byte[]?[]?) value.Value);
                    break;
                case VariantType.XmlElement:
                    WriteXElementArray(null, (XElement[]?) value.Value);
                    break;
                case VariantType.NodeId:
                    WriteNodeIdArray(null, (NodeId[]?) value.Value);
                    break;
                case VariantType.ExpandedNodeId:
                    WriteExpandedNodeIdArray(null, (ExpandedNodeId[]?) value.Value);
                    break;
                case VariantType.StatusCode:
                    WriteStatusCodeArray(null, (StatusCode[]?) value.Value);
                    break;
                case VariantType.QualifiedName:
                    WriteQualifiedNameArray(null, (QualifiedName[]?) value.Value);
                    break;
                case VariantType.LocalizedText:
                    WriteLocalizedTextArray(null, (LocalizedText[]?) value.Value);
                    break;
                case VariantType.ExtensionObject:
                    WriteExtensionObjectArray(null, (ExtensionObject[]?) value.Value!);
                    break;
                default:
                    throw new ServiceResultException(StatusCodes.BadEncodingError, string.Format(Resources.Error_UnsupportedVariantType, value.Type));
            }
        }


        /// <inheritdoc/>
        public void WriteDiagnosticInfo(string? fieldName, DiagnosticInfo? value) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.2.13

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            _writer.WriteStartObject();
            if (value.SymbolicId != -1) {
                WriteInt32("SymbolicId", value.SymbolicId);
            }
            if (value.NamespaceUri != -1) {
                WriteInt32("NamespaceUri", value.NamespaceUri);
            }
            if (value.Locale != -1) {
                WriteInt32("Locale", value.Locale);
            }
            if (value.LocalizedText != -1) {
                WriteInt32("LocalizedText", value.LocalizedText);
            }
            if (value.AdditionalInfo != null) {
                WriteString("AdditionalInfo", value.AdditionalInfo);
            }
            if (value.InnerStatusCode != StatusCodes.Good) {
                WriteStatusCode("InnerStatusCode", value.InnerStatusCode);
            }
            if (value.InnerDiagnosticInfo != null) {
                WriteDiagnosticInfo("InnerDiagnosticInfo", value.InnerDiagnosticInfo);
            }
            _writer.WriteEndObject();
        }


        /// <inheritdoc/>
        void IEncoder.WriteEncodable<T>(string? fieldName, T? value) where T : class {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.6

            if (value == null) {
                WriteNull(fieldName);
                return;
            }

            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }

            _writer.WriteStartObject();
            value.Encode(this);
            _writer.WriteEndObject();
        }


        /// <inheritdoc/>
        public void WriteRequest(IServiceRequest request) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.6

            if (request == null) {
                WriteNull();
                return;
            }

            ((IEncoder) this).WriteEncodable(null, request);
        }


        /// <inheritdoc/>
        public void WriteEnumeration<T>(string? fieldName, T value) where T : struct, IConvertible {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.4

            if (_options.UseReversibleEncoding) {
                WriteInt32(fieldName, Convert.ToInt32(value, CultureInfo.InvariantCulture));
            }
            else {
                WriteString(fieldName, $"{value.ToString()}_{Convert.ToInt32(value)}");
            }
        }


        /// <summary>
        /// Writes an array of values.
        /// </summary>
        /// <typeparam name="T">
        ///   The array element type.
        /// </typeparam>
        /// <param name="fieldName">
        ///   The optional field name to write.
        /// </param>
        /// <param name="values">
        ///   The array of values to write.
        /// </param>
        /// <param name="writeValue">
        ///   A delegate that writes a single value.
        /// </param>
        /// <exception cref="ServiceResultException">
        ///   The number of items in the array exceeds <see cref="IEncodingContext.MaxArrayLength"/>.
        /// </exception>
        private void WriteArray<T>(string? fieldName, Array? values, Action<string?, T> writeValue) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.5

            if (values == null) {
                WriteNull(fieldName);
                return;
            }

            if (_context.MaxArrayLength > 0 && values.Length > _context.MaxArrayLength) {
                throw new ServiceResultException(StatusCodes.BadEncodingLimitsExceeded, string.Format(Resources.Error_MaximumArrayLengthExceeded, _context.MaxArrayLength));
            }
            
            if (fieldName != null) {
                _writer.WritePropertyName(fieldName);
            }
            WriteArrayCore(values, 0, new int[values.Rank], writeValue);
        }


        /// <summary>
        /// Writes an array of values.
        /// </summary>
        /// <typeparam name="T">
        ///   The array element type.
        /// </typeparam>
        /// <param name="values">
        ///   The array of values to write.
        /// </param>
        /// <param name="dimension">
        ///   The current array dimension that is being processed.
        /// </param>
        /// <param name="indices">
        ///   The array indices for the current iteration.
        /// </param>
        /// <param name="writeValue">
        ///   A delegate that writes a single value.
        /// </param>
        private void WriteArrayCore<T>(Array values, int dimension, int[] indices, Action<string?, T> writeValue) {
            // https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.5

            var length = values.GetLength(dimension);

            _writer.WriteStartArray();

            for (var i = 0; i < length; i++) {
                indices[dimension] = i;
                if (dimension + 1 == values.Rank) {
                    var val = (T) values.GetValue(indices)!;
                    writeValue(null, val);
                }
                else {
                    WriteArrayCore(values, dimension + 1, indices, writeValue);
                }
            }

            _writer.WriteEndArray();
        }


        /// <inheritdoc/>
        public void WriteBooleanArray(string? fieldName, bool[]? values) {
            WriteArray<bool>(fieldName, values, WriteBoolean);
        }


        /// <inheritdoc/>
        public void WriteSByteArray(string? fieldName, sbyte[]? values) {
            WriteArray<sbyte>(fieldName, values, WriteSByte);
        }


        /// <inheritdoc/>
        public void WriteByteArray(string? fieldName, byte[]? values) {
            WriteArray<byte>(fieldName, values, WriteByte);
        }


        /// <inheritdoc/>
        public void WriteInt16Array(string? fieldName, short[]? values) {
            WriteArray<short>(fieldName, values, WriteInt16);
        }


        /// <inheritdoc/>
        public void WriteUInt16Array(string? fieldName, ushort[]? values) {
            WriteArray<ushort>(fieldName, values, WriteUInt16);
        }


        /// <inheritdoc/>
        public void WriteInt32Array(string? fieldName, int[]? values) {
            WriteArray<int>(fieldName, values, WriteInt32);
        }


        /// <inheritdoc/>
        public void WriteUInt32Array(string? fieldName, uint[]? values) {
            WriteArray<uint>(fieldName, values, WriteUInt32);
        }


        /// <inheritdoc/>
        public void WriteInt64Array(string? fieldName, long[]? values) {
            WriteArray<long>(fieldName, values, WriteInt64);
        }


        /// <inheritdoc/>
        public void WriteUInt64Array(string? fieldName, ulong[]? values) {
            WriteArray<ulong>(fieldName, values, WriteUInt64);
        }


        /// <inheritdoc/>
        public void WriteFloatArray(string? fieldName, float[]? values) {
            WriteArray<float>(fieldName, values, WriteFloat);
        }


        /// <inheritdoc/>
        public void WriteDoubleArray(string? fieldName, double[]? values) {
            WriteArray<double>(fieldName, values, WriteDouble);
        }


        /// <inheritdoc/>
        public void WriteStringArray(string? fieldName, string?[]? values) {
            WriteArray<string?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteString(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteDateTimeArray(string? fieldName, DateTime[]? values) {
            WriteArray<DateTime>(fieldName, values, WriteDateTime);
        }


        /// <inheritdoc/>
        public void WriteGuidArray(string? fieldName, Guid[]? values) {
            WriteArray<Guid>(fieldName, values, WriteGuid);
        }


        /// <inheritdoc/>
        public void WriteByteStringArray(string? fieldName, byte[]?[]? values) {
            WriteArray<byte[]?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteByteString(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteXElementArray(string? fieldName, XElement?[]? values) {
            WriteArray<XElement?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteXElement(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteNodeIdArray(string? fieldName, NodeId?[]? values) {
            WriteArray<NodeId?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteNodeId(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteExpandedNodeIdArray(string? fieldName, ExpandedNodeId?[]? values) {
            WriteArray<ExpandedNodeId?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteExpandedNodeId(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteStatusCodeArray(string? fieldName, StatusCode[]? values) {
            WriteArray<StatusCode>(fieldName, values, WriteStatusCode);
        }


        /// <inheritdoc/>
        public void WriteQualifiedNameArray(string? fieldName, QualifiedName?[]? values) {
            WriteArray<QualifiedName?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteQualifiedName(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteLocalizedTextArray(string? fieldName, LocalizedText?[]? values) {
            WriteArray<LocalizedText?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteLocalizedText(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteExtensionObjectArray(string? fieldName, ExtensionObject?[]? values) {
            WriteArray<ExtensionObject?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteExtensionObject(null, v);
                }
            });
        }


        /// <inheritdoc/>
        void IEncoder.WriteExtensionObjectArray<T>(string? fieldName, T?[]? values) where T : class {
            WriteArray<T?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    ((IEncoder) this).WriteExtensionObject(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteDataValueArray(string? fieldName, DataValue?[]? values) {
            WriteArray<DataValue?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteDataValue(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteVariantArray(string? fieldName, Variant[]? values) {
            WriteArray<Variant>(fieldName, values, WriteVariant);
        }


        /// <inheritdoc/>
        public void WriteDiagnosticInfoArray(string? fieldName, DiagnosticInfo?[]? values) {
            WriteArray<DiagnosticInfo?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    WriteDiagnosticInfo(null, v);
                }
            });
        }


        /// <inheritdoc/>
        void IEncoder.WriteEncodableArray<T>(string? fieldName, T?[]? values) where T : class {
            WriteArray<T?>(fieldName, values, (_, v) => {
                if (v == null) {
                    WriteNull();
                }
                else {
                    ((IEncoder) this).WriteEncodable(null, v);
                }
            });
        }


        /// <inheritdoc/>
        public void WriteEnumerationArray<T>(string fieldName, T[] values) where T : struct, IConvertible {
            WriteArray<T>(fieldName, values, WriteEnumeration);
        }


        /// <summary>
        /// Flushes the JSON writer to the underlying <see cref="Stream"/>.
        /// </summary>
        public void Flush() {
            ThrowIfDisposed();
            _writer.Flush();
        }


        /// <summary>
        /// Asynchronously flushes the JSON writer to the underlying <see cref="Stream"/>.
        /// </summary>
        /// <param name="cancellationToken">
        ///   The cancellation token for the operation.
        /// </param>
        public Task FlushAsync(CancellationToken cancellationToken = default) {
            ThrowIfDisposed();
            return _writer.FlushAsync(cancellationToken);
        }


        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the encoder has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///   The encoder has been disposed.
        /// </exception>
        private void ThrowIfDisposed() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }


        /// <inheritdoc/>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _writer.Dispose();
            if (!_keepStreamOpenOnDispose) {
                _stream?.Dispose();
            }

            _disposed = true;
        }

    }

}
