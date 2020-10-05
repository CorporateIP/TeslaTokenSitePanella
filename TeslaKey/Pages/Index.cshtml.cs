using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace TeslaKey.Pages
{
    public class CreateModel : PageModel
    {

        public IActionResult OnGet()
        {
            string language = null;
            if (HttpContext.Request.Path.Value.ToLower() == "/nl") { language = "nl"; }
            else if (HttpContext.Request.Path.Value.ToLower() == "/en") { language = "en"; }
            else { language = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName; }

            if (language != "nl" && language != "en") { language = "en"; }

            if (HttpContext.Request.Path.Value.ToLower() != "/" + language)
            {
                return Redirect("/" + language);
            }
            else
            {
                return Page();
            }
        }
    }
}