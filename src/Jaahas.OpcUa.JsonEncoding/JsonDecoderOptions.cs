using System.Xml.Linq;

using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace Jaahas.OpcUa.JsonEncoding {

    /// <summary>
    /// Options for <see cref="JsonDecoder"/>.
    /// </summary>
    public class JsonDecoderOptions {

        /// <summary>
        /// A factory for creating <see cref="IDecoder"/> instances for decoding XML-encoded 
        /// extension objects to CLR types.
        /// </summary>
        public Func<IEncodingContext, XElement, IDecoder?>? XmlDecoderFactory { get; set; }

    }

}
