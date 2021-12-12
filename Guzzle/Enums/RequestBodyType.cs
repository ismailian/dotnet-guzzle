namespace Cyberliberty.Guzzle
{
    /// <summary>
    /// This indicates the request body type
    /// </summary>
    public enum RequestBodyType
    {
        /// <summary>
        /// No body.
        /// </summary>
        NONE,

        /// <summary>
        /// Raw content
        /// </summary>
        RAW,

        /// <summary>
        /// Json-based content
        /// </summary>
        JSON,

        /// <summary>
        /// Files
        /// </summary>
        FILES,

        /// <summary>
        /// Parameters
        /// </summary>
        PARAMETERS,

        /// <summary>
        /// Parameters and files.
        /// </summary>
        PARAMETERS_AND_FILES,
    }
}
