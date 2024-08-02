# OPC UA JSON Encoding for Workstation.UaClient

This repository defines OPC UA JSON encoder and decoder classes for the [Workstation.UaClient](https://github.com/convertersystems/opc-ua-client) .NET OPC UA client library using System.Text.Json.

The encoder uses [Utf8JsonWriter](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonwriter) for maximum performance. The decoder uses [JsonDocument](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument) to allow properties on OPC UA data types to be decoded from JSON in an order-agnostic way.


# Getting Started

Install the [Jaahas.OpcUa.JsonEncoding](https://www.nuget.org/packages/Jaahas.OpcUa.JsonEncoding) NuGet package.

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


# Building the Solution

The repository uses [Cake](https://cakebuild.net/) for cross-platform build automation. The build script allows for metadata such as a build counter to be specified when called by a continuous integration system such as TeamCity.

A build can be run from the command line using the [build.ps1](/build.ps1) PowerShell script or the [build.sh](/build.sh) Bash script. For documentation about the available build script parameters, see [build.cake](/build.cake).


# Software Bill of Materials

To generate a Software Bill of Materials (SBOM) for the repository in [CycloneDX](https://cyclonedx.org/) XML format, run [build.ps1](./build.ps1) or [build.sh](./build.sh) with the `--target BillOfMaterials` parameter.

The resulting SBOM is written to the `artifacts/bom` folder.
