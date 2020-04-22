using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TeslaKey.Models;

namespace TeslaKey.Controllers
{
    [ApiController]
    public class GetTokenController : ControllerBase
    {
        private static readonly string client_secret = "c7257eb71a564034f9419ee651c7d0e5f7aa6bfbd18bafb5c5c033b093bb2fa3";
        private static readonly string client_id = "81527cff06843c8634fdc09e8ac0abefb46ac849f38fe1e431c2ef2106796384";

        static HttpClient client = new HttpClient();

        static GetTokenController()
        {
            client.BaseAddress = new Uri("https://owner-api.teslamotors.com");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // POST: api/gettoken
        //   [HttpPost]
        [Route("/gettoken")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<KeyResult> Post([FromForm] Request request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return KeyResult.Error("name and password are required");
                }  
                var requestdata = new
                {
                    password= request.Password,
                    email = request.Email,
                    client_secret,
                    client_id,
                    grant_type = "password"
                };

                var webrequest = new HttpRequestMessage(HttpMethod.Post, "oauth/token?grant_type=password");
                webrequest.Content = new StringContent(JsonSerializer.Serialize(requestdata), Encoding.UTF8, "application/json");
                webrequest.Headers.Add("User-Agent", "corporateip");

                var response = await client.SendAsync(webrequest);

                var resultdata = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(resultdata);

                if (response.IsSuccessStatusCode)
                {
                    return KeyResult.Code((string)data.access_token);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return KeyResult.Create("Invalid login");
                }
                else
                {
                    //todo abc log ?
                    return KeyResult.Create((string)data.response);
                }
            }
            catch
            {
                return KeyResult.ErrorServer();
            }
        }
    }
}