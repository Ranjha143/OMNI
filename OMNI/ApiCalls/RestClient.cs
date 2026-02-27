using RestSharp;

namespace OMNI.ApiCalls
{

    internal class RestApiClient
    {
        // baseUrl , endpoint ,  method , formParams, headers, body, isMultipart
        public static async Task<RestResponse> SendAsync(
        string baseUrl,
        string endpoint,
        Method method,
        Dictionary<string, string>? formParams = null,
        Dictionary<string, string>? headers = null,
        object? body = null,
        bool isMultipart = false)
        {
            try
            {
                var options = new RestClientOptions(baseUrl);
                var client = new RestSharp.RestClient(options);
                var request = new RestRequest(endpoint, method)
                {
                    // Timeout = -1
                };

                // Add JSON body
                if (body != null)
                {
                    request.AddJsonBody(body);
                }

                if (isMultipart)
                    request.AlwaysMultipartFormData = true;

                // Add form parameters
                if (formParams != null)
                {
                    foreach (var param in formParams)
                    {
                        request.AddParameter(param.Key, param.Value);
                    }
                }

                // Add headers
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.AddHeader(header.Key, header.Value);
                    }
                }

                RestResponse response = await client.ExecuteAsync(request);
                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                Console.WriteLine("Request timed out.");
                throw new Exception("Request timed out.", ex);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request failed: {ex.Message}");
                throw new Exception("HTTP request failed.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                throw new Exception("An error occurred while sending the request.", ex);
            }
        }
    }


}
