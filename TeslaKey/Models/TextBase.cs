using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TeslaKey.Languages
{
    public abstract class TextBase
    {
        private Dictionary<string, string> texts;
        private static Dictionary<string, TextBase> lookup = LoadLanguages();

        private static Dictionary<string, TextBase> LoadLanguages()
        {
            return new Dictionary<string, TextBase> { { "en", new TextBaseEN() }, { "nl", new TextBaseNL() } };
        }

        public Dictionary<string, string> Texts
        {
            get
            {
                if (texts == null)
                {
                    texts = new Dictionary<string, string>(GetType().GetProperties(BindingFlags.Public).Select(_pd => KV(_pd)));
                }
                return texts;
            }
        }
        private static TextBase Get(string language)
        {
            language = language.ToLower();
            if (language.StartsWith("en"))
            {
                return lookup["en"];
            }
            return lookup["nl"];
        }
        public static TextBase Get(HttpContext context, string language =null)
        {
            language = language?.ToLower();
            if (language != null)
            {
                if (language.StartsWith("en") || language.StartsWith("nl"))
                {
                    return Get(language);
                }
            }
            if (context.Request.Path.Value.ToLower() == "/nl") { language = "nl"; }
            if (context.Request.Path.Value.ToLower() == "/en") { language = "en"; }
            // context.Request.Query.TryGetValue("language", out StringValues languagestrings);
            //  var language = languagestrings.FirstOrDefault();
            return Get((language ?? System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName).ToLower());
        }
        private KeyValuePair<string, string> KV(PropertyInfo pd)
        {
            return new KeyValuePair<string, string>(pd.Name, (string)pd.GetValue(this));
        }

        public abstract string LanguageName { get; }

        public abstract string HtlmTitle { get; }
        public abstract string HtmlHeader { get; }

        public abstract string email { get; }
        public abstract string password { get; }
        public abstract string status { get; }

        public abstract string apiToken { get; }
        public abstract string apiTokenSub { get; }
        public abstract string refreshToken { get; }
        public abstract string loginSub { get; }
        public abstract string github { get; }

        public abstract string commingsoon { get; }

        public abstract string toClipboard { get; }

        public abstract string MsgRequireLogin { get; }
        public abstract string MsgInvalidLogin { get; }
        public abstract string MsgLockedOut { get; }

        public abstract string MsgYourToken { get; }
        public abstract string MsgReady { get; }
        public abstract string MsgRequesting { get; }
        public abstract string MsgGotaProblem { get; }

        public abstract string BtnRequest { get; }

        public abstract string infoheader { get; }
        public abstract string infotext { get; }

        public abstract string HtlmHeaderTitle { get; }
        public abstract string HtmlHeaderDescription { get; }
        public abstract string ServerError { get; }
    }

    public class TextBaseNL : TextBase
    {
        public override string HtlmTitle => "Tesla Token Generator";
        public override string HtmlHeader => "Genereer een API sleutel voor je Tesla Account.";

        public override string LanguageName => "NL";

        public override string email => "Emailadres";
        public override string password => "Wachtwoord";
        public override string status => "Status";

        public override string apiToken => "API sleutel";
        public override string apiTokenSub => "Zichtbaar na inloggen";

        public override string refreshToken => "Ververs sleutel";
        public override string loginSub => "Met je tesla account";
        public override string github => "Bekijk op Github";

        public override string commingsoon => "Nog niet beschikbaar";

        public override string toClipboard => "Kopiëren naar klembord";

        public override string MsgRequireLogin => "Naam en paswoord zijn vereist";
        public override string MsgInvalidLogin => "Toegang geweigerd";
        public override string MsgLockedOut => "Te vaak geprobeerd, wacht een paar minuten en probeer opnieuw";

        public override string MsgYourToken => "Uw toegangs-token:";
        public override string MsgReady => "Klaar voor gebruik";
        public override string MsgRequesting => "Server verzoek word gedaan";
        public override string MsgGotaProblem => "Probleem tegengekomen:";

        public override string BtnRequest => "Genereer sleutel";

        public override string infoheader => "Disclaimer ©2020";
        public override string infotext => "Wij slaan geen van je gegevens op, we gebruiken deze alleen om de API sleutel bij Tesla op te vragen.";

        public override string HtlmHeaderTitle => "Genereer een token voor de Tesla API - TeslaTokenGenerator";
        public override string HtmlHeaderDescription => "Met deze gratis tool kunt u een token genereren om derde partijen toegang te geven tot uw Tesla account, zonder hen uw Tesla gebruikersnaam en wachtwoord te verstrekken.";
        public override string ServerError => "Netwerk/server fout.";
    }
    public class TextBaseEN : TextBase
    {
        public override string HtlmTitle => "Tesla Token Generator";
        public override string HtmlHeader => "Generate an API token for your Tesla Account.";

        public override string LanguageName => "EN";

        public override string email => "Email";
        public override string password => "Password";
        public override string status => "Status";

        public override string apiToken => "API token";
        public override string apiTokenSub => "Will appear after login";

        public override string refreshToken => "Refresh token";
        public override string loginSub => "With your tesla account";
        public override string github => "View on Github";

        public override string commingsoon => "Coming soon..";

        public override string toClipboard => "Copy to clipboard";

        public override string MsgRequireLogin => "name and password are required";
        public override string MsgInvalidLogin => "invalid login";
        public override string MsgLockedOut => "Too many tries, please wait some minutes and try again";

        public override string MsgYourToken => "You access-token:";
        public override string MsgReady => "Ready";
        public override string MsgRequesting => "requesting";
        public override string MsgGotaProblem => "got a problem:";

        public override string BtnRequest => "Generate token";

        public override string infoheader => "Disclaimer ©2020";
        public override string infotext => "We do not store any of your credentials, we only use your credentials to retrieve the API token from Tesla.";

        public override string HtlmHeaderTitle => "Generate a token for the Tesla API - TeslaTokenGenerator";
        public override string HtmlHeaderDescription => "With this free generator you can create a token to access the Tesla API for third party applications, without providing them your username and password.";
        public override string ServerError => "Network/server error.";
    }
}
