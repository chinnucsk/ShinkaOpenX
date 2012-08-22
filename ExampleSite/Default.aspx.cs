using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Shinka.Ads;
using System.Text;

namespace ExampleSite
{
  public partial class Default : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      //in a live environment hosted on the internet timeout of 200ms should suffice
      int timeout = 10000;

      //Mxit bot proxy forwards the users IP address in X-Forwarded-For
      var fwdFor = Request.Headers["X-Forwarded-For"];
      if (string.IsNullOrEmpty(fwdFor))
      {
        fwdFor = Request.UserHostAddress;
      }

      StringBuilder adMarkup = new StringBuilder();

      //Use the UserAgent below - If the Mxit bot proxy is not set to override UserAgents then you will not be served ads.
      var ua = "Mozilla Compatible/5.0 (J2ME/MIDP; Mxit Mobi/6.2.1/1.8.5.168; U;) Opera Mini/3.1";
      var acceptLang = Request.Headers["Accept-Language"];

      //****IMPORTANT**** Get your AdUnit Id from the Shinka Web UI / your Representative - if you serve this Ad Unit you not going to get much revenue
      var shinkaAdId = 219427;

      //Get the following Mxit User data from
      //http://dev.mxit.com/docs/mobi-portal-api#headers

      int userAge = 20; //Request.Headers["X-Mxit-Profile"]
      string userGender = "male"; //Request.Headers["X-Mxit-Profile"]
      string userDevice = "Nokia/X1"; //Request.Headers["X-Device-User-Agent"]

      var data = string.Format("?auid={0}&c.age={1}&c.gen={2}&c.device={3}", shinkaAdId, userAge, userGender, userDevice);


      OpenX.AdvertDTO ads = null;
      try
      {
        OpenX ox = new OpenX()
        {
          Url = "http://ox-d.shinka.sh/ma/1.0/arx",
          QS = data,
          UserAgent = ua,
          AcceptLang = acceptLang,
          ForwardFor = fwdFor,
          TimeOut = timeout
        };

        ads = ox.FetchAdvert();
      }
      catch (Exception ex)
      {
        lblAdMarkup.Text = string.Format("ERROR: {0}", ex.Message);
        return;
      }

      if (ads != null && ads.Adverts != null && ads.Adverts.Count > 0)
      {
        var advert = ads.Adverts[0];

        if (advert.Creative != null && advert.Creative.Creatives != null)
        {
          var creative = advert.Creative.Creatives[0];
          var tracking = creative.Tracking;

          var adHeight = creative.Height;
          var adWidth = creative.Width;

          if (tracking != null)
          {
            if (creative.Mime.ToLower().IndexOf("text") > -1)
            {
              adMarkup.AppendFormat("{0}", creative.MediaURL);
            }
            else
            {
              //You can make the image clickable but Mxit Clients don't play nice with clickable images so its best practice currently to place a text link below the image.
              //else you could just uncomment the line below:
              //adMarkup.AppendFormat("<a href=\"{0}\" onclick=\"window.open(this.href); return false;\"><img src=\"{1}\" border=\"0\" height=\"{2}\" width=\"{3}\"/></a>",  Server.HtmlEncode(Tracking.ClickURL), Server.HtmlEncode(Creative.MediaURL), adHeight, adWidth);
              
              adMarkup.AppendFormat("<img src=\"{0}\" height=\"{1}\" width=\"{2}\"/>", Server.HtmlEncode(creative.MediaURL), adHeight, adWidth);
              adMarkup.AppendFormat("<br/><a href=\"{0}\" onclick=\"window.open(this.href); return false;\">{1}</a>", Server.HtmlEncode(tracking.ClickURL), Server.HtmlEncode(creative.AltText));
            }
            adMarkup.AppendFormat("<br/><img src=\"{0}\" />", tracking.ImpressionsURL);
          }
        }
        lblAdMarkup.Text = adMarkup.ToString();
      }
      else
      {
        lblAdMarkup.Text = "No advert received";
      }
    }
  }
}