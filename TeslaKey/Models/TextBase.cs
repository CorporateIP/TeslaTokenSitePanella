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
            if (language.StartsWith("en"))
            {
                return lookup["en"];
            }
            return lookup["nl"];
        }
        public static TextBase Get(HttpContext context)
        {
            context.Request.Query.TryGetValue("language", out StringValues languagestrings);
            var language = languagestrings.FirstOrDefault();
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
    }

    public class TextBaseNL : TextBase
    {
        public override string HtlmTitle => "Tesla - verkijg code";
        public override string HtmlHeader => "Hier voor uw TESLA ontgrendel code";

        public override string LanguageName => "NL";

        public override string email => "E-mail adres";
        public override string password => "Wachtwoord";
        public override string status => "Status";

        public override string MsgRequireLogin => "Naam en paswoord zijn vereist";
        public override string MsgInvalidLogin => "Toegang geweigerd";
        public override string MsgLockedOut => "Te vaak geprobeerd, wacht een paar minuten en probeer opnieuw";

        public override string MsgYourToken => "Uw toegangs-token:";
        public override string MsgReady => "Klaar voor gebruik";
        public override string MsgRequesting => "Server verzoek word gedaan";
        public override string MsgGotaProblem => "Probleem tegengekomen:";

        public override string BtnRequest => "Verzoek code";

        public override string infoheader => "Disclaimer ©2020";
        public override string infotext => @"Deze site slaat geen gegevens van u op, gebruikt geen cookies en is gratis voor gebruik.<br />Het C#-Project kan op <a href=&quote;&quote;>GIT</a> gevonden worden, onder een  <a href=&quote;&quote;>...</a> licentie.<br />";

     }
    public class TextBaseEN : TextBase
    {
        public override string HtlmTitle => "Tesla - get login";
        public override string HtmlHeader => "Get your TESLA unlock code";

        public override string LanguageName => "EN";

        public override string email => "Email";
        public override string password => "Password";
        public override string status => "Status";

        public override string MsgRequireLogin => "name and password are required";
        public override string MsgInvalidLogin => "invalid login";
        public override string MsgLockedOut => "Too many tries, please wait some minutes and try again";

        public override string MsgYourToken => "You access-token:";
        public override string MsgReady => "Ready";
        public override string MsgRequesting => "requesting";
        public override string MsgGotaProblem => "got a problem:";

        public override string BtnRequest => "Get my token";

        public override string infoheader => "Disclaimer ©2020";
        public override string infotext => @"This site doesn't save your credentials, uses no cookies and is free for use.<br />The C#-Project can be found on <a href=&quote;&quote;>GIT</a>, under a  <a href=&quote;&quote;>...</a> license.<br />";
    }
}
