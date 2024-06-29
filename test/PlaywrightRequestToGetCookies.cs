using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;
using System.Threading.Tasks;
using OpenQA.Selenium.DevTools.V100.Cast;

namespace Airfrance_bot
{
    public class PlaywrightRequestToGetCookies
    {
        public async Task<string> GetCookies(FlightRequestData objFlightData, string DepartureDate, string BrowserType = "Firefox")
        {
            string strBrowser_type = objFlightData.data.browser;
            if (string.IsNullOrEmpty(strBrowser_type)) { strBrowser_type = "Firefox"; }
            string strCookies = string.Empty;         
            if (strBrowser_type.ToLower() == "chrome")
            {
                Logs.LogMessage("Starting Playwright Chrome Request...");
                #region PlaywrightChromium
                using var playwright = await Playwright.CreateAsync();
                var chromium = playwright.Chromium;

                #region proxySettings            
                var proxy = new Proxy
                {
                    Server = "http://us.smartproxy.com:10020",
                    Username = "spyc5m5gbs",
                    Password = "puFNdLvkx6Wcn6h6p8"
                };
                //var proxy = new Proxy
                //{
                //    Server = "http://pr.oxylabs.io:7777",
                //    Username = "customer-engineo_rTGsc-sessid-0652280266-sesstime-10",
                //    Password = "EngiNeoProxy_24+"
                //};
                //var proxy = new Proxy
                //{
                //    Server = "http://us-pr.oxylabs.io:10015",
                //    Username = "customer-engineo_rTGsc",
                //    Password = "EngiNeoProxy_24+"
                //};
                //var proxy = new Proxy
                //{
                //    Server = "http://brd.superproxy.io:22225",
                //    Username = "brd-customer-hl_b11b062e-zone-residential",
                //    Password = "b8nn8wp8f08x"
                //};
                //var userDataDir = $"{Environment.CurrentDirectory}/userdata/"; // Specify the user data directory            
                var browser = await chromium.LaunchAsync(new BrowserTypeLaunchOptions()
                {
                    Headless = false,
                    Channel = "chrome", //msedge
                    SlowMo = 100,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--no-first-run", "--disable-blink-features=AutomationControlled" },
                    IgnoreDefaultArgs = new[] { "--enable-automation" },
                    Proxy = proxy,
                });
                #endregion
                try
                {
                    Random rand = new Random();
                    #region RequestingHomePage_using_PlaywrightWithChrome
                    var customUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";
                    var customViewport = new ViewportSize
                    {
                        Width = 1280,
                        Height = 720
                    };

                    var context = await browser.NewContextAsync(new BrowserNewContextOptions
                    {
                        ViewportSize = customViewport,
                        //UserAgent = customUserAgent,
                        Locale = "en-US",
                        TimezoneId = "America/New_York",
                        BypassCSP = true,
                        JavaScriptEnabled = true,
                    });
                    var page = await context.NewPageAsync();
                    page.SetDefaultTimeout(100000);
                    await page.EvaluateAsync(@"
                // Pass the Chrome Test.
                Object.defineProperty(navigator, 'webdriver', { get: () => undefined });

                // Pass the Plugins Length Test.
                Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3] });

                // Pass the Languages Test.
                Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] });

                // Pass the Navigator Permissions Test.
                const originalQuery = window.navigator.permissions.query;
                window.navigator.permissions.query = (parameters) => (
                    parameters.name === 'notifications' ?
                        Promise.resolve({ state: Notification.permission }) :
                        originalQuery(parameters)
                );

                // Pass the WebGL Vendor and Renderer Test.
                const getParameter = WebGLRenderingContext.prototype.getParameter;
                WebGLRenderingContext.prototype.getParameter = function (parameter) {
                    // UNMASKED_VENDOR_WEBGL
                    if (parameter === 37445) {
                        return 'Intel Inc.';
                    }
                    // UNMASKED_RENDERER_WEBGL
                    if (parameter === 37446) {
                        return 'Intel Iris OpenGL Engine';
                    }
                    return getParameter(parameter);
                };

                // Mock the Chrome object
                window.chrome = {
                    runtime: {},
                };

                // Mock notifications
                const originalNotification = window.Notification;
                window.Notification = function (title, options) {
                    return new originalNotification(title, options);
                };
                window.Notification.permission = originalNotification.permission;
                window.Notification.requestPermission = originalNotification.requestPermission.bind(originalNotification);
            ");
                    string strOriginIataTypeCode = objFlightData.data.originIataType.ToUpper() == "CITY" ? "C" : "A";
                    string strDestinationIataTypeCode = objFlightData.data.originIataType.ToUpper() == "CITY" ? "C" : "A";
                    string _deep_link = "https://wwws.airfrance.us?activeConnection=0&bookingFlow=LEISURE&cabinClass=ECONOMY&pax=1:0:0:0:0:0:0:0&connections=NYC:C:20250120%3EPAR:C";
                    if (rand.Next(1, 3) == 1)
                        _deep_link = "https://wwws.airfrance.us/search/advanced?activeConnection=0&bookingFlow=REWARD&cabinClass=" + objFlightData.data.CabinClass +
                        "&pax=1:0:0:0:0:0:0:0&connections=" + objFlightData.data.originIata +
                        ":" + strOriginIataTypeCode + ":" + Convert.ToDateTime(DepartureDate).ToString("yyyyMMdd") + "%3E" + objFlightData.data.DestinationIata +
                        ":" + strDestinationIataTypeCode + "&hasAnalyticsConsent=null";
                    else
                        _deep_link = "https://wwws.airfrance.us/";
                   
                    Logs.LogMessage("Sending Deep_link Request using Playwright...");
                    try
                    {
                        await page.GotoAsync(_deep_link, new PageGotoOptions { Timeout = 30000 });
                    }
                    catch
                    {
                        var content = await page.ContentAsync();
                        Logs.LogError("Playwright chrome response : " + content);
                    }
                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);      

                    await page.WaitForSelectorAsync("span[data-test=\"bwsfe-booking-flow__tabs--reward\"]", new PageWaitForSelectorOptions { Timeout = 80000 });
                    
                    #endregion

                    #region Checking_page_is_loaded         
                    Logs.LogMessage("Sleeping for few seconds to load page completely.....");                    
                    await page.WaitForLoadStateAsync();
                    await page.WaitForResponseAsync(res => res.Status == 200);
                    var elementInputtext = await page.WaitForSelectorAsync("span[class=\"mdc-button__label\"]", new PageWaitForSelectorOptions
                    {
                        Timeout = 80000 // Set a specific timeout for this call (80 seconds)
                    });
                    Logs.LogMessage("Deep_link Page request completed successfully.....");
                    #endregion

                    #region ClickOnAcceptCookiesButton
                    try
                    {
                        var buttonExists = await page.EvalOnSelectorAsync<bool>("button[id=\"accept_cookies_btn\"]", "el => !!el");
                        if (buttonExists)
                        {
                            Logs.LogMessage("Checking Cookies Accept button and trying to click on it.....");
                            int maxRetries = 3;
                            int retryCount = 0;
                            bool clicked = false;
                            while (!clicked && retryCount < maxRetries)
                            {
                                try
                                {
                                    await page.Locator("button[id=\"accept_cookies_btn\"]").ClickAsync();
                                    clicked = true;
                                }
                                catch (PlaywrightException ex)
                                {
                                    Console.WriteLine($"Click failed: {ex.Message}. Retrying...");
                                    retryCount++;
                                    await Task.Delay(1000); // Wait for a short duration before retrying
                                }
                            }
                            if (clicked)
                                Logs.LogMessage("Successfully clicked on the Cookies Accept button......");
                        }
                    }
                    catch { }
                    #endregion

                    #region MouseMove_ToMimicking_Humanbehaviour
                    Logs.LogMessage("Sleeping for few seconds for mouse movement.....");
                    await Task.Delay(rand.Next(4, 8) * 1000);
                    await page.Mouse.MoveAsync(rand.Next(0, 2), rand.Next(3, 8));
                    await page.Mouse.DownAsync();
                    await page.Mouse.MoveAsync(0, rand.Next(100, 120));
                    await page.Mouse.MoveAsync(rand.Next(100, 120), rand.Next(100, 120));
                    await page.Mouse.MoveAsync(rand.Next(100, 120), 0);
                    await page.Mouse.MoveAsync(0, 0);
                    await page.Mouse.UpAsync();
                    await page.Keyboard.PressAsync("PageDown");
                    await Task.Delay(rand.Next(1, 2) * 1000);
                    await page.Keyboard.PressAsync("PageUp");
                    await Task.Delay(rand.Next(1, 2) * 1000);
                    Logs.LogMessage("Mouse movement completed.....");
                    #endregion

                    #region GetCookies
                    var cookies = await page.Context.CookiesAsync();
                    if (cookies != null)
                    {
                        foreach (var cookie in cookies)
                        {
                            strCookies += $"{cookie.Name}={cookie.Value};";
                        }
                    }
                    Logs.LogMessage($"Cookies received: {strCookies}");
                    await context.CloseAsync();
                    #endregion
                }
                catch (Exception ex)
                {
                    Logs.LogMessage("Exception occured in Chrome Browser" + ex.Message.ToString());                 
                }
                finally
                {                    
                    await browser.CloseAsync();                    
                }
                #endregion
            }
            else if(strBrowser_type.ToLower() == "firefox")
            {
                Logs.LogMessage("Starting Playwright Firefox Request...");
                #region PlaywrightFirefox
                using var playwright = await Playwright.CreateAsync();
                var firefox = playwright.Firefox;

                #region proxySettings            
                var proxy = new Proxy
                {
                    Server = "http://us.smartproxy.com:10020",
                    Username = "spyc5m5gbs",
                    Password = "puFNdLvkx6Wcn6h6p8"
                    //Server = "http://pr.oxylabs.io:7777",
                    //Username = "customer-engineo_rTGsc-sessid-0652280266-sesstime-10",
                    //Password = "EngiNeoProxy_24+"
                    //Server = "http://us-pr.oxylabs.io:10020",
                    //Username = "customer-engineo_rTGsc",
                    //Password = "EngiNeoProxy_24+"
                };
                //var userDataDir = $"{Environment.CurrentDirectory}/userdata/"; // Specify the user data directory            
                var browser = await firefox.LaunchAsync(new BrowserTypeLaunchOptions()
                {
                    Headless = true,                   
                    SlowMo = 100,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--no-first-run", "--disable-blink-features=AutomationControlled" },
                    IgnoreDefaultArgs = new[] { "--enable-automation" },
                    Proxy = proxy,
                });
                #endregion
                try
                {
                    Random rand = new Random();
                    #region RequestingHomePage_using_PlaywrightWithFirefox
                    
                    var customViewport = new ViewportSize
                    {
                        Width = 1280,
                        Height = 720
                    };

                    var context = await browser.NewContextAsync(new BrowserNewContextOptions
                    {
                        ViewportSize = customViewport,
                        //UserAgent = customUserAgent,
                        Locale = "en-US",
                        TimezoneId = "America/New_York",
                        BypassCSP = true,
                        JavaScriptEnabled = true,
                    });
                    var page = await context.NewPageAsync();
                    page.SetDefaultTimeout(100000);
                    // Stealth measures for Firefox
                    await page.EvaluateAsync(@"
                // Pass the Plugins Length Test.
                Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3] });

                // Pass the Languages Test.
                Object.defineProperty(navigator, 'languages', { get: () => ['en-US', 'en'] });

                // Pass the Navigator Permissions Test.
                const originalQuery = window.navigator.permissions.query;
                window.navigator.permissions.query = (parameters) => (
                    parameters.name === 'notifications' ?
                        Promise.resolve({ state: Notification.permission }) :
                        originalQuery(parameters)
                );

                // Pass the WebGL Vendor and Renderer Test.
                const getParameter = WebGLRenderingContext.prototype.getParameter;
                WebGLRenderingContext.prototype.getParameter = function (parameter) {
                    // UNMASKED_VENDOR_WEBGL
                    if (parameter === 37445) {
                        return 'Intel Inc.';
                    }
                    // UNMASKED_RENDERER_WEBGL
                    if (parameter === 37446) {
                        return 'Intel Iris OpenGL Engine';
                    }
                    return getParameter(parameter);
                };

                // Mock the Chrome object
                window.chrome = {
                    runtime: {},
                };

                // Mock notifications
                const originalNotification = window.Notification;
                window.Notification = function (title, options) {
                    return new originalNotification(title, options);
                };
                window.Notification.permission = originalNotification.permission;
                window.Notification.requestPermission = function (callback) {
                    // Simulate a permission granted response
                    const permissionPromise = Promise.resolve('granted');
                    if (callback) {
                        permissionPromise.then(callback);
                    }
                    return permissionPromise;
                };
            ");
                    string strOriginIataTypeCode = objFlightData.data.originIataType.ToUpper() == "CITY" ? "C" : "A";
                    string strDestinationIataTypeCode = objFlightData.data.originIataType.ToUpper() == "CITY" ? "C" : "A";
                    string _deep_link = "https://wwws.airfrance.us?activeConnection=0&bookingFlow=LEISURE&cabinClass=ECONOMY&pax=1:0:0:0:0:0:0:0&connections=NYC:C:20250120%3EPAR:C";
                    if (rand.Next(1, 3) == 1)
                        _deep_link = "https://wwws.airfrance.us/search/advanced?activeConnection=0&bookingFlow=REWARD&cabinClass=" + objFlightData.data.CabinClass +
                        "&pax=1:0:0:0:0:0:0:0&connections=" + objFlightData.data.originIata +
                        ":" + strOriginIataTypeCode + ":" + Convert.ToDateTime(DepartureDate).ToString("yyyyMMdd") + "%3E" + objFlightData.data.DestinationIata +
                        ":" + strDestinationIataTypeCode + "&hasAnalyticsConsent=null";
                    else
                        _deep_link = "https://wwws.airfrance.us/";

                    Logs.LogMessage("Sending Deep_link Request using Playwright...");
                    
                    await page.GotoAsync(_deep_link, new PageGotoOptions { WaitUntil = WaitUntilState.Load });
                    #endregion

                    #region Checking_page_is_loaded         
                    Logs.LogMessage("Sleeping for few seconds to load page completely.....");
                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                    await page.WaitForLoadStateAsync();
                    await page.WaitForResponseAsync(res => res.Status == 200);
                    var elementInputtext = await page.WaitForSelectorAsync("span[class=\"mdc-button__label\"]", new PageWaitForSelectorOptions
                    {
                        Timeout = 80000 // Set a specific timeout for this call (80 seconds)
                    });
                    Logs.LogMessage("Deep_link Page request completed successfully.....");
                    #endregion

                    #region ClickOnAcceptCookiesButton
                    try
                    {
                        var buttonExists = await page.EvalOnSelectorAsync<bool>("button[id=\"accept_cookies_btn\"]", "el => !!el");
                        if (buttonExists)
                        {
                            Logs.LogMessage("Checking Cookies Accept button and trying to click on it.....");
                            int maxRetries = 3;
                            int retryCount = 0;
                            bool clicked = false;
                            while (!clicked && retryCount < maxRetries)
                            {
                                try
                                {
                                    await page.Locator("button[id=\"accept_cookies_btn\"]").ClickAsync();
                                    clicked = true;
                                }
                                catch (PlaywrightException ex)
                                {
                                    Console.WriteLine($"Click failed: {ex.Message}. Retrying...");
                                    retryCount++;
                                    await Task.Delay(1000); // Wait for a short duration before retrying
                                }
                            }
                            if (clicked)
                                Logs.LogMessage("Successfully clicked on the Cookies Accept button......");
                        }
                    }
                    catch { }
                    #endregion

                    #region MouseMove_ToMimicking_Humanbehaviour
                    Logs.LogMessage("Sleeping for few seconds for mouse movement.....");
                    await Task.Delay(rand.Next(4, 8) * 1000);
                    await page.Mouse.MoveAsync(rand.Next(0, 2), rand.Next(3, 8));
                    await page.Mouse.DownAsync();
                    await page.Mouse.MoveAsync(0, rand.Next(100, 120));
                    await page.Mouse.MoveAsync(rand.Next(100, 120), rand.Next(100, 120));
                    await page.Mouse.MoveAsync(rand.Next(100, 120), 0);
                    await page.Mouse.MoveAsync(0, 0);
                    await page.Mouse.UpAsync();
                    await page.Keyboard.PressAsync("PageDown");
                    await Task.Delay(rand.Next(1, 2) * 1000);
                    await page.Keyboard.PressAsync("PageUp");
                    await Task.Delay(rand.Next(1, 2) * 1000);
                    Logs.LogMessage("Mouse movement completed.....");
                    #endregion

                    #region GetCookies
                    var cookies = await page.Context.CookiesAsync();
                    if (cookies != null)
                    {
                        foreach (var cookie in cookies)
                        {
                            strCookies += $"{cookie.Name}={cookie.Value};";
                        }
                    }
                    Logs.LogMessage($"Cookies received: {strCookies}");
                    await context.CloseAsync();
                    #endregion
                }
                catch (Exception ex)
                {
                    Logs.LogMessage("Exception occured" + ex.Message.ToString());
                }
                finally
                {
                    await browser.CloseAsync();
                }
                #endregion
            }
            else
            {
                return "Unsupported Browser Type.";
            }
            return strCookies;
        }

    }
}
