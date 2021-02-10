using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TeslaKey.Languages;
using TeslaKey.Models;
using TeslaKey.Singletons;

// https://tesla-api.timdorr.com/api-basics/authentication

namespace TeslaKey.Controllers
{
    [ApiController]
    public class GetTokenController : ControllerBase
    {
        static HttpClientHandler handler_client = new HttpClientHandler
        {
            UseCookies=false,
        };

        static HttpClientHandler handler_client_noredirect = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
        };


        private static readonly string client_secret = "c7257eb71a564034f9419ee651c7d0e5f7aa6bfbd18bafb5c5c033b093bb2fa3";
        private static readonly string client_id = "81527cff06843c8634fdc09e8ac0abefb46ac849f38fe1e431c2ef2106796384";
        private readonly ILockoutHandler _lockout;

        public GetTokenController(ILockoutHandler lockout)
        {
            this._lockout= lockout;
        }
        // POST: api/gettoken
        //   [HttpPost]
        [Route("/gettoken")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<KeyResult> Post([FromForm] Request request)
        {
            var TB = TextBase.Get(HttpContext);
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return KeyResult.Error(TB, TB.MsgRequireLogin);
                }

                if (this._lockout.LockedOut("ip", this.HttpContext.Connection.RemoteIpAddress.ToString()))
                {
                    return KeyResult.Error(TB, TB.MsgLockedOut);
                }
                if (this._lockout.LockedOut("email", request.Email.ToLower()))
                {
                        return KeyResult.Error(TB, TB.MsgLockedOut);
                }
                using (HttpClient client = new HttpClient(handler_client, false))
                using (HttpClient client_noredirect = new HttpClient(handler_client_noredirect, false))
                {
                    // randomizer
                    var rnd = new Random(DateTime.Now.Millisecond);

                    //state,code_verifier en base64
                    var state = new string(Enumerable.Range(0, 10).Select(_n => (char)('0' + rnd.Next(10))).ToArray());
                    var code_verifier = new string(Enumerable.Range(0, 86).Select(_n => (char)('A' + rnd.Next(26))).ToArray());
                    //get hash
                    byte[] hashmessage = SHA256.Create().ComputeHash(new ASCIIEncoding().GetBytes(code_verifier));
                    //hash to hexhash
                    var hash = String.Concat(Array.ConvertAll(hashmessage, x => x.ToString("x2")));
                    //url save base64-encode
                    var base64 = Convert.ToBase64String(new ASCIIEncoding().GetBytes((hash)));
                    base64 = base64.Replace("+", "-").Replace("/", "_").Replace("=", "");

                    // formulier url opbouwen
                    var sb = new StringBuilder();
                    sb.Append(add("client_id", "ownerapi"));
                    sb.Append(add("code_challenge", base64));
                    sb.Append(add("code_challenge_method", "S256"));
                    sb.Append(add("redirect_uri", "https://auth.tesla.com/void/callback"));
                    sb.Append(add("response_type", "code"));
                    sb.Append(add("scope", "openid email offline_access"));
                    sb.Append(add("state", state));

                    var formurl = "https://auth.tesla.com/oauth2/v3/authorize?" + sb.ToString().Substring(0, sb.Length - 1);

                    // step1 get the form-www-page
                    var response = await client.GetAsync(formurl);
                    if (response.IsSuccessStatusCode)
                    {
                        var htmlbody = await response.Content.ReadAsStringAsync();

                        // get cookie
                        response.Headers.TryGetValues("Set-cookie", out IEnumerable<string> cs);
                        var c = cs.Single().Split(";")[0];

                        // load html
                        HtmlWeb web = new HtmlWeb();
                        HtmlDocument document = new HtmlDocument();
                        document.LoadHtml(htmlbody);

                        //parse html
                        var main = document.DocumentNode.Descendants("form").Single();
                        var inputs = main.Descendants("input").ToArray();

                        //build up post-data
                        var dict = new Dictionary<string, string>();
                        // hidden inputs
                        foreach (var input in inputs.Where(_i => _i.Attributes["type"].Value == "hidden"))
                        {
                            dict.Add(input.Attributes["name"].Value, input.Attributes["value"].Value);
                        }
                        // credentials
                        dict.Add("identity", request.Email);
                        dict.Add("credential", request.Password);

                        //step 2 post the form
                        var webrequest = new HttpRequestMessage(HttpMethod.Post, formurl);
                        webrequest.Content = new FormUrlEncodedContent(dict);
                        webrequest.Headers.Add("cookie", c);

                        response = await client_noredirect.SendAsync(webrequest);

                        //redirect status
                        if ((int)response.StatusCode == 302)
                        {
                            // get redirect-location
                            if (response.Headers.TryGetValues("Location", out IEnumerable<string> newurl))
                            {
                                var url = newurl.Single();
                                //get code from url
                                int ind = url.IndexOf("code=");
                                var code = url.Substring(ind + 5).Split('&')[0];

                                //step 3
                                var postdata = new
                                {
                                    grant_type = "authorization_code",
                                    client_id = "ownerapi",
                                    code = code,
                                    code_verifier = code_verifier,
                                    redirect_uri = "https://auth.tesla.com/void/callback"
                                };

                                webrequest = new HttpRequestMessage(HttpMethod.Post, "https://auth.tesla.com/oauth2/v3/token");
                                webrequest.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(postdata), Encoding.UTF8, "application/json");

                                response = await client.SendAsync(webrequest);

                                ///STEP 3 !!!
                                if (response.IsSuccessStatusCode)
                                {
                                    var txt = await response.Content.ReadAsStringAsync();
                                    dynamic data = JObject.Parse(txt);
                                    var access_token = (string)data.access_token;

                                    //step 4 get tokens for jwt-bearer
                                    var postdata2 = new
                                    {
                                        grant_type = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                                        client_id = client_id,
                                        client_secret = client_secret
                                    };
                                    webrequest = new HttpRequestMessage(HttpMethod.Post, "https://owner-api.teslamotors.com/oauth/token");
                                    webrequest.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(postdata2), Encoding.UTF8, "application/json");
                                    webrequest.Headers.Add("Authorization", $"Bearer {access_token}");
                                    response = await client.SendAsync(webrequest);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        txt = await response.Content.ReadAsStringAsync();
                                        data = JObject.Parse(txt);
                                        return KeyResult.Code(TB, (string)data.access_token, (string)data.refresh_token);
                                    }
                                }
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            return KeyResult.Create(TB, TB.MsgInvalidLogin);
                        }
                    }
                }
                return KeyResult.Create(TB, TB.ServerError);
            }
            catch(Exception e)
            {
                return KeyResult.ErrorServer(TB);
            }
        }

        static string add(string name, string value)
        {
            return $"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value.ToString())}&";
        }
    }
}