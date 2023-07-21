// ReSharper disable ClassNeverInstantiated.Global
namespace GPConnect.PatientFacingService.Provider.AcceptanceTests.Constants
{
    internal static class HttpConst
    {
        internal static class ContentTypes
        {
            public const string kJson = "application/json";
            public const string kXml = "application/xml";
        }

        internal static class Headers
        {
            public const string kAccept = "Accept";
            public const string kPrefer = "Prefer";
            public const string kIfNoneMatch = "If-None-Match";
            public const string kIfMatch = "If-Match";
            public const string kLocation = "Location";
            public const string kAuthorization = "Authorization";
            // public const string kSspFrom = "Authorization";
            // public const string kSspTo = "X_Request_ID";
            // public const string kSspInteractionId = "Ssp-InteractionId";
            public const string kXCorrelationID = "x-correlation-id";
            public const string kXRequestID = "x-request-id";
            public const string kContentType = "Content-Type";
            public const string kAcceptEncoding = "Accept-Encoding";
            public const string kContentEncoding = "Content-Encoding";
            public const string kTransferEncoding = "Transfer-Encoding";
            public const string kCacheControl = "Cache-Control";
            public const string kExpires = "Expires";
            public const string kPragma = "Pragma";
        }
    }
}
