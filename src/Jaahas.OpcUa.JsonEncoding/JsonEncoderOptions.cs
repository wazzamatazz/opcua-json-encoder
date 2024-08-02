namespace Jaahas.OpcUa.JsonEncoding {

    /// <summary>
    /// Options for <see cref="JsonEncoder"/>.
    /// </summary>
    public class JsonEncoderOptions {

        /// <summary>
        /// Specifies if the encoder should use reversible encoding.
        /// </summary>
        /// <remarks>
        ///   See <a href="https://reference.opcfoundation.org/Core/Part6/v105/docs/5.4.1">here</a> 
        ///   for more information about reversible encoding.
        /// </remarks>
        public bool UseReversibleEncoding { get; set; } = true;

        /// <summary>
        /// Specifies if the encoder should write indented JSON.
        /// </summary>
        public bool WriteIndented { get; set; }

    }

}
