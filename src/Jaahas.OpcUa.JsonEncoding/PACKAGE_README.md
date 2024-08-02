﻿# About

Jaahas.OpcUa.JsonEncoding provides an [OPC UA JSON encoder and decoder](https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4) for the [Workstation.UaClient](https://github.com/convertersystems/opc-ua-client) .NET OPC UA client library using System.Text.Json. The encoder supports both the reversible and non-reversible forms of the OPC UA JSON encoding.


# Getting Started

The `JsonEncoder` and `JsonDecoder` classes implement the `Workstation.UaClient` `IEncoder` and `IDecoder` interfaces respectively. The `JsonEncodingProvider` class implements `IEncodingProvider`.

Example:

```csharp
var provider = new JsonEncodingProvider(encoderOptions: new JsonEncoderOptions() {
    UseReversibleEncoding = false,
    WriteIndented = true
});

using var ms = new MemoryStream();

using (var encoder = provider.CreateEncoder(ms, context: null, keepStreamOpen: true)) {
    encoder.WriteRequest(new ReadRequest() { 
        MaxAge = 1000,
        NodesToRead = [
            new ReadValueId() {
                NodeId = NodeId.Parse("ns=2;s=Demo.Static.Scalar.UInt32"),
                AttributeId = AttributeIds.Value
            },
            new ReadValueId() {
                NodeId = NodeId.Parse("ns=2;s=Demo.Static.Scalar.String"),
                AttributeId = AttributeIds.Value
            }
        ],
        TimestampsToReturn = TimestampsToReturn.Both,
        RequestHeader = new RequestHeader() { 
            AuditEntryId = "Test",
            RequestHandle = 42
        }
    });
}

Console.WriteLine(Encoding.UTF8.GetString(ms.ToArray()));
```

The above code produces the following output using the [non-reversible form](https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.1) of the OPC UA JSON encoding:

```json
{
  "RequestHeader": {
    "AuthenticationToken": null,
    "Timestamp": "0001-01-01T00:00:00",
    "RequestHandle": 42,
    "ReturnDiagnostics": 0,
    "AuditEntryId": "Test",
    "TimeoutHint": 0,
    "AdditionalHeader": null
  },
  "MaxAge": 1000,
  "TimestampsToReturn": "Both_2",
  "NodesToRead": [
    {
      "NodeId": {
        "IdType": 1,
        "Id": "Demo.Static.Scalar.UInt32",
        "Namespace": 2
      },
      "AttributeId": 13,
      "IndexRange": null,
      "DataEncoding": null
    },
    {
      "NodeId": {
        "IdType": 1,
        "Id": "Demo.Static.Scalar.String",
        "Namespace": 2
      },
      "AttributeId": 13,
      "IndexRange": null,
      "DataEncoding": null
    }
  ]
}
```