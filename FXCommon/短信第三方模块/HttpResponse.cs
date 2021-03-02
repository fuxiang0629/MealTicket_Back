using System.Net;
using System.Net.Http.Headers;

namespace SMS_SendHandler
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Content { get; set; }

        public HttpResponse(HttpStatusCode statusCode, HttpResponseHeaders headers, string content)
        {
            StatusCode = statusCode;
            Headers = headers;
            Content = content;
        }
    }
}
