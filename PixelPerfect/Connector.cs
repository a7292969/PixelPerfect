using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfect
{
    class Connector
    {
        public static string Get(string url)
        {
            try
            {
                if (!InternetAvailability.IsInternetAvailable())
                    return "-1";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                            return reader.ReadToEnd();
                    }
                    else
                    {
                        return "-1";
                    }
                }
            }
            catch
            {
                return "-1";
            }
        }

        public static string GetAuth(string url, string token)
        {
            try
            {
                if (!InternetAvailability.IsInternetAvailable())
                    return "-1";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.PreAuthenticate = true;
                request.Headers.Add("AuthenticationToken", token);
                request.Accept = "application/json";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                            return reader.ReadToEnd();
                    }
                    else
                    {
                        return "-1";
                    }
                }
            }
            catch
            {
                return "-1";
            }
        }

        public static async Task<string> GetAsync(string url)
        {
            try
            {
                if (!InternetAvailability.IsInternetAvailable())
                    return "-1";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                            return await reader.ReadToEndAsync();
                    }
                    else
                    {
                        return "-1";
                    }
                }
            }
            catch
            {
                return "-1";
            }
        }

        public static string Post(string url, string data)
        {
            try
            {
                if (!InternetAvailability.IsInternetAvailable())
                    return "-1";

                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentLength = dataBytes.Length;
                request.ContentType = "application/json";
                request.Method = "POST";

                using (Stream requestBody = request.GetRequestStream())
                    requestBody.Write(dataBytes, 0, dataBytes.Length);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (Stream stream = ((HttpWebResponse)e.Response).GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                    Console.WriteLine(reader.ReadToEnd());

                return ((int)((HttpWebResponse)e.Response).StatusCode).ToString();
            }
        }

        public static async Task<string> PostAsync(string url, string data)
        {
            try
            {
                if (!InternetAvailability.IsInternetAvailable())
                    return "-1";

                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentLength = dataBytes.Length;
                request.ContentType = "application/json";
                request.Method = "POST";

                using (Stream requestBody = await request.GetRequestStreamAsync())
                    await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                        return await reader.ReadToEndAsync();
                }
            }
            catch (WebException e)
            {
                using (Stream stream = ((HttpWebResponse)e.Response).GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                    Console.WriteLine(await reader.ReadToEndAsync());


                return ((int)((HttpWebResponse)e.Response).StatusCode).ToString();
            }
        }
    }
}
