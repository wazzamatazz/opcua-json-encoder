using System.Text;

using Jaahas.OpcUa.JsonEncoding;

using Workstation.ServiceModel.Ua;

using var ms = new MemoryStream();

var provider = new JsonEncodingProvider(new JsonEncoderOptions() {
    UseReversibleEncoding = false,
    WriteIndented = true
});

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
