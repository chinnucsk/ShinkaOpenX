using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Shinka.Ads
{
  public class OpenX
  {
    public string Url { get; set; }
    public string QS { get; set; }
    public int TimeOut { get; set; }
    public string ForwardFor { get; set; }
    public string UserAgent { get; set; }
    public string AcceptLang { get; set; }

    public OpenX()
    {

    }

    public AdvertDTO FetchAdvert()
    {
      string httpResult = GetAd();

      AdvertDTO ads = new XmlSerializerHelper<AdvertDTO>().Deserialize(httpResult);

      return ads;
    }

    private string GetAd()
    {
      HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(Url + QS);
      httpRequest.Method = "GET";
      UTF8Encoding utf8 = new UTF8Encoding();

      if (TimeOut > 0)
      {
        httpRequest.Timeout = TimeOut;
      }

      //set the useragent and other headers OpenX expects
      httpRequest.UserAgent = UserAgent;
      httpRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, AcceptLang);
      httpRequest.Headers.Add("X-Forwarded-For", ForwardFor);

      //if you dont want to use HTTP compress change this to false;
      bool compress = true;
      if (compress)
      {
        httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
      }

      HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
      if (httpResponse.ContentEncoding.ToLower().Contains("gzip"))
      {
        compress = true;
      }
      else
      {
        //the server does not support gzip so set compress to false so that the responsestream is not decoded below
        compress = false;
      }

      Stream responseStream = responseStream = httpResponse.GetResponseStream();
      if (compress)
      {
        responseStream = new GZipStream(httpResponse.GetResponseStream(), CompressionMode.Decompress);
      }
      StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
      string result = reader.ReadToEnd();
      responseStream.Close();
      reader.Close();
      httpResponse.Close();
      return result;
    }

    [Serializable, XmlRoot("ads")]
    public class AdvertDTO
    {
      [XmlAttribute("version")]
      public string Version { get; set; }

      [XmlAttribute("count")]
      public int Count { get; set; }

      [XmlElement("ad")]
      public List<OpenXAd> Adverts { get; set; }

      [XmlRoot("ad")]
      public class OpenXAd
      {
        [XmlAttribute("adunit")]
        public int AdUnit { get; set; }

        [XmlAttribute("adid")]
        public int AdId { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        [XmlElement("html")]
        public string HTML { get; set; }

        [XmlElement("creatives")]
        public AdCreativeHolder Creative { get; set; }
      }


      [Serializable, XmlRoot("creatives")]
      public class AdCreativeHolder
      {
        [XmlElement("creative")]
        public List<AdCreative> Creatives { get; set; }
      }

      [XmlRoot("creative")]
      public class AdCreative
      {
        [XmlAttribute("mime")]
        public string Mime { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("alt")]
        public string AltText { get; set; }

        [XmlAttribute("target")]
        public string Target { get; set; }

        [XmlText]
        [XmlElement("media")]
        public string MediaURL { get; set; }

        [XmlElement("tracking")]
        public AdTracking Tracking { get; set; }

        public class AdTracking
        {

          [XmlElement("impression")]
          public string ImpressionsURL { get; set; }

          [XmlElement("inview")]
          public string InviewURL { get; set; }

          [XmlElement("click")]
          public string ClickURL { get; set; }

        }
      }
    }
    
    internal class XmlSerializerHelper<T>
    {
      public Type _type;

      public XmlSerializerHelper()
      {
        _type = typeof(T);
      }

      public XDocument Serialize(T obj)
      {
        XDocument target = new XDocument();
        XmlSerializer s = new XmlSerializer(_type);
        System.Xml.XmlWriter writer = target.CreateWriter();
        s.Serialize(writer, obj);
        writer.Close();
        return target;
      }

      public T Deserialize(string xmlString)
      {
        T result;

        using (TextReader textReader = new StringReader(xmlString))
        {
          XmlSerializer deserializer = new XmlSerializer(_type);
          result = (T)deserializer.Deserialize(textReader);
        }
        return result;
      }
    }
  }
}
