using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TeslaKey.Models
{
    public class KeyResult
    {
        public string Message { get; set; } = null;
        public string Token { get; set; } = null;

        internal static KeyResult Create(string response)
        {
            if (!string.IsNullOrWhiteSpace(response))
            {
                return new KeyResult
                {
                    Message = response
                };
            }
            else
            {
                return new KeyResult
                {
                    Message = $"Error communicating with TESLA"
                };
            }
        }
        internal static KeyResult Error(string message)
        {
            return new KeyResult
            {
                Message = message
            };
        }
        internal static KeyResult ErrorServer()
        {
            return new KeyResult
            {
                Message = "Error handling your request.. you could contact us todo abc"
            };
        }

        internal static KeyResult Code(string access_token)
        {
            if (!string.IsNullOrWhiteSpace(access_token))
            { 
                return new KeyResult
                {
                    Token = access_token
                };
            }
            else
            {
                return new KeyResult
                {
                    Message = $"Error communicating with TESLA"
                };
            }
        }
    }
}
