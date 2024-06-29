using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Playwright;
using Airfrance_bot;

using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OpenQA.Selenium.DevTools.V100.Network;
using System.Net.Http.Headers;
//using SeleniumUndetectedChromeDriver;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium;

namespace Budget_us_location_bot
{
    class Program
    {
        static async Task Main(string[] args)
        {

            #region read_request_json
            Logs.LogMessage("Starting application and reading FlightData Request...");
            string request_filepath = Path.Combine(Directory.GetCurrentDirectory(), "request.json");
            string jsonString = File.ReadAllText(request_filepath);
            FlightRequestData objFlightData = JsonConvert.DeserializeObject<FlightRequestData>(jsonString);
            #endregion

            List<string> DepartureDates =
                [
                "2024-08-01",
            "2024-08-02",
            "2024-08-03",
            "2024-08-04",
            "2024-08-05"
        ];

            PlaywrightRequestToGetCookies objPlaywrightRequestToGetCookies = new PlaywrightRequestToGetCookies();
            string strCookies = await GetCookies(objPlaywrightRequestToGetCookies, objFlightData, DepartureDates[0]);
            string strTempCookies = strCookies;
            bool is_all_requests_passed = true;

            if (!string.IsNullOrEmpty(strCookies) && strCookies != "Unsupported Browser Type.")
            {
                foreach (string departure_date in DepartureDates)
                {
                    #region HttpWebRequest

                    #region HeadersSection
                    Random rnd = new Random();
                    string strUserAgent = GetUserAgent();
                    Dictionary<string, string> ObjDic = new Dictionary<string, string>();
                    ObjDic.Add("User-Agent", strUserAgent);
                    ObjDic.Add("Accept-Language", "en-US");
                    ObjDic.Add("Upgrade-Insecure-Requests", "1");
                    ObjDic.Add("AFKL-Travel-Country", "US");
                    ObjDic.Add("language", "en");
                    ObjDic.Add("AFKL-Travel-Language", "en");
                    ObjDic.Add("X-Aviato-Host", "wwws.airfrance.us");
                    ObjDic.Add("x-client-revision", "da871398f5fe60c8f77c47ffaf9e6fa9d85cb57b");
                    ObjDic.Add("x-dtpc", "6$485345456_803h274vPOAESSOFUUOERSRHCKMWHFRRPOLIKUHU-0e0");
                    ObjDic.Add("AFKL-TRAVEL-Host", "AF");
                    ObjDic.Add("x-dtreferer", "https://wwws.airfrance.us/search/advanced?activeConnection=0&bookingFlow=REWARD&cabinClass=ECONOMY&pax=1:0:0:0:0:0:0:0&connections=NYC:C:20250120%3EPAR:C");
                    ObjDic.Add("country", "US");
                    ObjDic.Add("sec-ch-ua-mobile", "?0");
                    ObjDic.Add("sec-ch-ua-platform", "\"Windows\"");
                    ObjDic.Add("sec-ch-ua", "\"Google Chrome\";v=\"125\", \"Chromium\";v=\"125\", \"Not.A/Brand\";v=\"24\"");
                    ObjDic.Add("Sec-Fetch-Site", "same-origin");
                    ObjDic.Add("Sec-Fetch-Mode", "cors");
                    ObjDic.Add("Sec-Fetch-Dest", "empty");
                    ObjDic.Add("Accept-Encoding", "gzip, deflate, br, zstd");

                    string strContentType = "application/json";
                    string strAccept = "application/json;charset=utf-8;hashcash=536358021284214996938a77abcbc6a72f52a6476ccc8941a701398a37ee14e9-459";
                    #endregion

                    #region ProxySettings
                    WebProxy _proxy = new WebProxy("us.smartproxy.com:10020");
                    _proxy.Credentials = new NetworkCredential("spyc5m5gbs", "puFNdLvkx6Wcn6h6p8");
                    //WebProxy _proxy = new WebProxy("pr.oxylabs.io:7777");
                    //_proxy.Credentials = new NetworkCredential("customer-engineo_rTGsc-sessid-0652280266-sesstime-10", "EngiNeoProxy_24+");
                    //WebProxy _proxy = new WebProxy("us-pr.oxylabs.io:10013");
                    //_proxy.Credentials = new NetworkCredential("customer-engineo_rTGsc", "EngiNeoProxy_24+");
                    //_proxy = null;
                    #endregion

                    string strLocation = string.Empty;
                    string strRef = "https://wwws.airfrance.us/search/flights/0";

                    #region FirstHit     
                    string strUrl = "https://wwws.airfrance.us/gql/v1?bookingFlow=LEISURE";
                    string strPostdata = GetPostdata(objFlightData, departure_date);
                    string strFlights_Response = string.Empty;
                    //strFlights_Response = await GetCustomResponseAsync("http://geo.brdtest.com/mygeo.json", WebRequestMethods.Http.Get, strRef, strCookies, _proxy, strPostdata, true, strContentType, strLocation, ObjDic, strAccept, "", 11);
                    string strHost = "wwws.airfrance.us";
                    for (int count = 0; count <= 4; count++)
                    {
                        is_all_requests_passed = true;
                        strCookies = strTempCookies;
                        strFlights_Response = await GetCustomResponseAsync(strUrl, WebRequestMethods.Http.Post, strRef, strCookies, _proxy, strPostdata, true, strContentType, strLocation, ObjDic, strAccept, strHost, 11);
                        if (strFlights_Response.Contains("<<Error>>") && !strFlights_Response.Contains("Timeout of"))
                        {
                            is_all_requests_passed = false;
                            continue;
                        }
                        if (string.IsNullOrEmpty(strFlights_Response) || !strFlights_Response.Contains("\"availableOffers\"") || strFlights_Response.Contains("DataSourceError"))
                        {
                            strCookies = await GetCookies(objPlaywrightRequestToGetCookies, objFlightData, DepartureDates[0]);
                            if (string.IsNullOrEmpty(strCookies))
                            {
                                Logs.LogError("Flights Request Failed.....");
                                is_all_requests_passed = false;
                                break;
                            }
                            strTempCookies = strCookies;
                        }
                        else
                        {
                            Logs.LogMessage("Flight response successful.");
                            break;
                        }
                    }
                    // Save the content to a file
                    var filePath = $"json_response_{objFlightData.data.originIata}_{objFlightData.data.DestinationIata}_{departure_date}.txt";
                    await File.WriteAllTextAsync(filePath, strFlights_Response);
                    Logs.LogMessage(".....Saving API response......");
                    if (!is_all_requests_passed)
                        break;
                    #endregion
                    #endregion
                }
            }
            else
            {
                if (strCookies == "Unsupported Browser Type.")
                    Logs.LogError("Unsupported Browser Type. Please check requested Browser Type.");
                else
                    Logs.LogError("Playwright Request failed: Cookies not received.");
            }
        }

        private static string GetUserAgent()
        {
            List<string> userAgents = new List<string>
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36",
        };
            Random rand = new Random();
            int index = rand.Next(userAgents.Count);
            return userAgents[index];            
        }

        private static async Task<string> GetCookies(PlaywrightRequestToGetCookies objPlaywrightRequestToGetCookies, FlightRequestData objFlightData, string DepartureDate)
        {
            string strCookies = string.Empty;
            for (int i = 0; i < 3; i++)
            {
                strCookies = await objPlaywrightRequestToGetCookies.GetCookies(objFlightData, DepartureDate, objFlightData.data.browser);
                if (!string.IsNullOrEmpty(strCookies) || strCookies == "Unsupported Browser Type.")
                    break;
            }
            return strCookies;
        }

        private static string GetPostdata(FlightRequestData FlightData, string departure_date)
        {
            string strAdultsData = GetAdulsString(FlightData);
            string strPostdata = "{\"operationName\":\"SearchResultAvailableOffersQuery\",\"variables\":{\"activeConnectionIndex\":0," +
                "\"bookingFlow\":\"REWARD\",\"availableOfferRequestBody\":{\"commercialCabins\":[\"" + FlightData.data.CabinClass +
                "\"],\"passengers\":[" + strAdultsData + "],\"requestedConnections\":[{\"origin\":{\"code\":\"" + FlightData.data.originIata +
                "\",\"type\":\"" + FlightData.data.originIataType +
                "\"},\"destination\":{\"code\":\"" + FlightData.data.DestinationIata +
                "\",\"type\":\"" + FlightData.data.DestinationIataType + "\"},\"departureDate\":\"" + departure_date +
                "\"}],\"bookingFlow\":\"REWARD\",\"customer\":{\"selectedTravelCompanions\":[{\"passengerId\":1,\"travelerKey\":0,\"travelerSource\":\"PROFILE\"}]}}," +
                "\"searchStateUuid\":\"6326dc5f-7c4c-4503-808e-2c7ca609cf71\"}," +
                "\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"e4ca2f800f1d846800ba1bddea12b7f6ca4c225a758f59f4e5cf9a9e87d1c7b5\"}}}";
            return strPostdata;
        }

        private static string GetAdulsString(FlightRequestData FlightData)
        {
            
            List<string> repeatedStrings = new List<string>();

            for (int i = 1; i <= FlightData.data.Adults; i++)
            {
                repeatedStrings.Add("{\"type\":\"ADT\",\"id\":" + i + "}");
            }
           
            string concatenatedString = System.String.Join(",", repeatedStrings);
            return concatenatedString;
        }
        
        private static List<string> GetListResult(string pattern, string input)
        {
            List<string> ObjLst = new List<string>();           
            MatchCollection matches = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {                
                foreach (Match match in matches)
                {
                    ObjLst.Add(match.Groups[1].Value);
                }
            }            
            return ObjLst;
        }
        private static string GetStringResult(string pattern, string input)
        {
            string strResult = string.Empty;
            MatchCollection matches = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    strResult = match.Groups[1].Value;
                    break;
                }
            }
            return strResult;
        }
        private static Int64 GetTime(DateTime nDate)
        {
            Int64 retval = 0;
            DateTime st = new DateTime(1970, 1, 1).ToLocalTime();
            TimeSpan t = (nDate - st);
            retval = (Int64)(t.TotalMilliseconds);
            return retval;
        }
        //public static string GetCustomResponse(string strURL, string strMethod, string strRefr, ref string CookieKeyValuePair, WebProxy hWebproxy,
        //   string strPostFormDetails, bool isRedirect, string strContentType, ref string strLocation, Dictionary<string, string> httpRequestHeaderColl, string strAccept, string strHost, int HTTPVersion)
        //{
        //    string result = "";
        //    StreamWriter myWriter = null;
        //    System.String strPost = strPostFormDetails;
        //    string Charset = string.Empty;
        //    try
        //    {
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
        //        #region Req
        //        HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(strURL);
        //        objRequest.Method = strMethod;
        //        objRequest.Proxy = hWebproxy;
        //        //objRequest.ClientCertificates.Clear();
        //        //objRequest.ClientCertificates.Add(new X509Certificate());
        //        objRequest.Credentials = CredentialCache.DefaultCredentials;
        //        objRequest.Timeout = 100000;
        //        if (HTTPVersion == 10)
        //            objRequest.ProtocolVersion = HttpVersion.Version10;
        //        else
        //            objRequest.ProtocolVersion = HttpVersion.Version11;
        //        objRequest.Host = strHost;
        //        objRequest.Accept = strAccept;
        //        if (!string.IsNullOrEmpty(strContentType))
        //            objRequest.ContentType = strContentType;
        //        if (!string.IsNullOrEmpty(strRefr))
        //            objRequest.Referer = strRefr;
        //        if (httpRequestHeaderColl != null)
        //        {
        //            foreach (KeyValuePair<string, string> kvp in httpRequestHeaderColl)
        //            {
        //                if (kvp.Key != "User-Agent")
        //                {
        //                    objRequest.Headers.Add(kvp.Key, kvp.Value);
        //                }
        //                else
        //                {
        //                    objRequest.UserAgent = kvp.Value;
        //                }
        //            }
        //        }
        //        if (strRefr != null)
        //        {
        //            objRequest.Referer = strRefr;
        //        }
        //        if (CookieKeyValuePair != null)
        //        {
        //            objRequest.Headers["Cookie"] = CookieKeyValuePair;
        //        }
        //        if (strPost != null && (strMethod == "POST" || strMethod.ToLower().Contains("put")))
        //        {
        //            try //posting the form values
        //            {
        //                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
        //                byte[] data = encoder.GetBytes(strPost);
        //                objRequest.ContentLength = data.Length;

        //                System.IO.Stream reqstream = objRequest.GetRequestStream();
        //                reqstream.Write(data, 0, data.Length);
        //                reqstream.Close();
        //            }
        //            catch (Exception ex)
        //            {

        //            }

        //        }
        //        //if (strMethod == WebRequestMethods.Http.Post)
        //        //{
        //        //    try
        //        //    {
        //        //        myWriter = new StreamWriter(objRequest.GetRequestStream());
        //        //        strPost = string.Format(strPost);
        //        //        myWriter.Write(strPost);
        //        //    }
        //        //    catch (Exception ex)
        //        //    {

        //        //    }
        //        //    finally
        //        //    {
        //        //        myWriter.Dispose();
        //        //    }
        //        //}
        //        #endregion
        //        #region Resp
        //        HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

        //        Encoding encoding;
        //        Charset = objResponse.CharacterSet;
        //        if (string.IsNullOrEmpty(Charset))
        //        {
        //            encoding = Encoding.Default;
        //        }
        //        else
        //        {
        //            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //            encoding = Encoding.UTF8;
        //        }
        //        Stream _ResponseStream = objResponse.GetResponseStream();
        //        if (objResponse.ContentEncoding != null)
        //        {
        //            if (objResponse.ContentEncoding.ToLower().Contains("gzip"))
        //                _ResponseStream = new GZipStream(_ResponseStream, CompressionMode.Decompress);
        //            else if (objResponse.ContentEncoding.ToLower().Contains("deflate"))
        //                _ResponseStream = new DeflateStream(_ResponseStream, CompressionMode.Decompress);
        //        }
        //        CookieKeyValuePair = objResponse.GetResponseHeader("Set-Cookie").ToString();
        //        strLocation = Convert.ToString(objResponse.GetResponseHeader("Location"));
        //        using (StreamReader sr = new StreamReader(_ResponseStream, encoding))
        //        {
        //            if (sr != null)
        //            {
        //                result += sr.ReadToEnd();
        //                sr.Close();
        //                objResponse.Close();
        //            }
        //        }
        //        #endregion
        //    }
        //    catch (WebException ex)
        //    {
        //        Logs.LogError($"Flights Request Failed.....{ex.Message}");
        //        if (ex.Message.Contains("The proxy tunnel"))
        //            result = ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logs.LogError($"Flights Request Failed.....{ex.Message}");
        //    }
        //    return result;
        //}
        public static async Task<string> GetCustomResponseAsync(string strURL, string strMethod, string strRefr, string CookieKeyValuePair, WebProxy hWebproxy,
           string strPostFormDetails, bool isRedirect, string strContentType, string strLocation, Dictionary<string, string> httpRequestHeaderColl, string strAccept, string strHost, int HTTPVersion)
        {
            string result = "";
            string Charset = string.Empty;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;

                using (HttpClient client = hWebproxy != null ? new HttpClient(new HttpClientHandler { Proxy = hWebproxy }) : new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(100);

                    if (HTTPVersion == 10)
                        client.DefaultRequestVersion = HttpVersion.Version10;
                    else
                        client.DefaultRequestVersion = HttpVersion.Version11;

                    client.DefaultRequestHeaders.Host = strHost;
                    client.DefaultRequestHeaders.Accept.ParseAdd(strAccept);

                    if (!string.IsNullOrEmpty(strContentType))
                        client.DefaultRequestHeaders.Add("ContentType", strContentType);

                    if (!string.IsNullOrEmpty(strRefr))
                        client.DefaultRequestHeaders.Referrer = new Uri(strRefr);

                    if (httpRequestHeaderColl != null)
                    {
                        foreach (KeyValuePair<string, string> kvp in httpRequestHeaderColl)
                        {
                            if (kvp.Key != "User-Agent")
                            {
                                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                            }
                            else
                            {
                                client.DefaultRequestHeaders.UserAgent.ParseAdd(kvp.Value);
                            }
                        }
                    }

                    if (CookieKeyValuePair != null)
                    {
                        client.DefaultRequestHeaders.Add("Cookie", CookieKeyValuePair);
                    }

                    HttpResponseMessage response;

                    if (strMethod == "POST" || strMethod.ToLower().Contains("put"))
                    {
                        HttpContent content = new StringContent(strPostFormDetails, Encoding.UTF8, strContentType);
                        response = await client.PostAsync(strURL, content);
                    }
                    else
                    {
                        response = await client.GetAsync(strURL);
                    }

                    result = await response.Content.ReadAsStringAsync();
                    CookieKeyValuePair = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
                    strLocation = response.Headers.Location?.ToString();
                }
            }
            catch (HttpRequestException ex)
            {
                Logs.LogError($"Flights Request Failed: {ex.Message}");
                result = "<<Error>>" + ex.Message;
            }
            catch (Exception ex)
            {
                Logs.LogError($"Flights Request Failed: {ex.Message}");
                result = "<<Error>>" + ex.Message;
            }
            return result;
        }

    }
}

