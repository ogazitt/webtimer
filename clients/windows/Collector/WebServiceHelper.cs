﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Collector
{
    public class UserAgents
    {
        // user agents for devices
        public const string WinPhone = "Zaplify-WinPhone";
        public const string IOSPhone = "Zaplify-IOSPhone";
        public const string WebTimerWindows = "WebTimer-Windows";
    }

    public class HttpApplicationHeaders
    {
        // custom Http headers used by application
        public const string SpeechEncoding = "X-Zaplify-Speech-Encoding";
        public const string Session = "X-Zaplify-Session";
        public const string RequestedWith = "X-Requested-With";
    }

    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class RegisterUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VerifyPassword { get; set; }
        public string Name { get; set; }
    }

    public enum OperationStatus
    {
        Started = 0,
        Success = 1,
        Failed = 2,
        Retry = 3
    }

    public class WebServiceHelper
    {
        const string authorizationHeader = "Authorization";
        const string authResponseHeader = "Set-Cookie";
        const string authRequestHeader = "Cookie";
        static string authCookie = null;

        private static string baseUrl = null;
        private static string appSettingsBaseUrl = null;
        private static bool triedGettingAppSettingsBaseUrl = false;
        // default URL (which depends on the environment)
        private static string defaultBaseUrl
        {
            get
            {
                return "http://www.webtimer.co";
            }
        }
        
        // getter / setter which cache the Base URL stored in AppSettings 
        private static string AppSettingsBaseUrl
        {
            get
            {
                if (appSettingsBaseUrl == null && !triedGettingAppSettingsBaseUrl)
                {
                    appSettingsBaseUrl = ConfigurationManager.AppSettings["Url"];
                    triedGettingAppSettingsBaseUrl = true;
                }
                return appSettingsBaseUrl;
            }
            set
            {
                ConfigurationManager.AppSettings["Url"] = value;
                appSettingsBaseUrl = value;
                triedGettingAppSettingsBaseUrl = true;
            }
        }

        // BaseUrl for the service
        //   If the BaseUrl was set, use that value
        //   Otherwise if the AppSettings BaseUrl was found, use this one
        //   Otherwise, use the default BaseUrl
        public static string BaseUrl
        {
            get
            {
                return baseUrl ?? (AppSettingsBaseUrl ?? defaultBaseUrl);
            }
            set
            {
                baseUrl = value;
            }
        }

        #region // Web Service calls

        public static void CreateAcccount(RegisterUser user, Delegate del, Delegate netOpInProgressDel)
        {
            InvokeWebServiceRequest(null, BaseUrl + "/accountapi/register", "POST", user, del, netOpInProgressDel, new AsyncCallback(ProcessResponse<string>));
        }

        public static void PostRecords(string userCreds, List<Record> records, Delegate del, Delegate netOpInProgressDel)
        {
            //InvokeWebServiceRequest(user, BaseUrl + "/colapi/collector", "POST", records, del, netOpInProgressDel, new AsyncCallback(ProcessResponse<List<Record>>));
            InvokeWebServiceRequest(userCreds, BaseUrl + "/colapi/collector", "POST", records, del, netOpInProgressDel, new AsyncCallback(ProcessResponse<int>));
        }

        public static void VerifyAccount(User user, Delegate del, Delegate netOpInProgressDel)
        {
            InvokeWebServiceRequest(null, BaseUrl + "/accountapi/validateuser", "POST", user, del, netOpInProgressDel, new AsyncCallback(ProcessResponse<string>));
        }

        public delegate void AccountDelegate(string username);

        #endregion

        #region // Helper methods

        //private static HttpWebResponseWrapper<T> GetWebResponse<T>(IAsyncResult result)
        private static HttpWebResponse GetWebResponse<T>(IAsyncResult result)
        {
            WebServiceState state = result.AsyncState as WebServiceState;
            if (state == null)
            {
                TraceLog.TraceError("Web Service State not found");
                return null;
            }

            var request = state.Request;
            if (request == null)
            {
                TraceLog.TraceError("Web Service Request not found");
                return null;
            }

            HttpWebResponse resp = null;

            // get response and mark request as not in progress
            try
            {
                resp = (HttpWebResponse)request.EndGetResponse(result);
                if (resp == null)
                    return null;
            }
            catch (Exception ex) 
            {
                TraceLog.TraceException("GetWebResponse: EndGetResponse failed", ex);
                return null;
            }

            // put auth cookie in static memory
            if (resp.Headers[authResponseHeader] != null)
            {
                authCookie = resp.Headers[authResponseHeader];
            }

            // create and initialize a new response wrapper
            HttpWebResponse wrapper = resp;
            //HttpWebResponseWrapper<T> wrapper = new HttpWebResponseWrapper<T>(resp);

            // try to get the status code - an exception indicates an error in the payload
            try
            {
                if (wrapper.StatusCode > 0)
                    return wrapper;
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("GetWebResponse: Bad response format received", ex);
                return null;
            }

            return wrapper;
        }

        // Common code for invoking all the web service calls.  
        // GET requests will be served directly from this method,
        // POST/PUT/DELETE requests are served from the InvokeWebServiceRequest_Inner method (which is an async callback)
        private static void InvokeWebServiceRequest(string userCreds, string url, string verb, object obj, Delegate del, Delegate netOpInProgressDel, AsyncCallback callback)
        {
            // signal that a network operation is starting
            if (netOpInProgressDel != null)
                netOpInProgressDel.DynamicInvoke(true, OperationStatus.Started);

            Uri uri = null;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri) == false ||
                uri.Scheme != "http" && uri.Scheme != "https")
            {
                TraceLog.TraceError("InvokeWebServiceRequest: bad URL: " + url);
                return;
            }

			var request = (HttpWebRequest) WebRequest.Create(uri);
            //request.UserAgent = UserAgents.IOSPhone;
            request.Accept = "application/json";
            request.Method = verb == null ? "GET" : verb;

            if (authCookie != null)
            {   // send auth cookie
                request.Headers[authRequestHeader] = authCookie;
            }
            else if (userCreds != null)
            {   // send credentials in authorization header

                // url form encoded
                //string credentials = string.Format("UserName={0}&Password={1}", user.UserName, user.Password);

                // basic auth encoded
                request.Headers[authorizationHeader] = string.Format("Basic {0}", userCreds);
                request.Headers[HttpApplicationHeaders.RequestedWith] = UserAgents.WebTimerWindows;
            }

            // set the session ID header
            var sessionToken = TraceLog.Session;
            if (sessionToken != null)
                request.Headers[HttpApplicationHeaders.Session] = sessionToken;

            // if this is a GET request, we can execute from here
            if (request.Method == "GET")
            {
                // execute the web request and get the response
                try
                {
                    WebServiceState reqState = new WebServiceState()
                    {
                        Request = request,
                        Delegate = del,
                        NetworkOperationInProgressDelegate = netOpInProgressDel
                    };
                    request.BeginGetResponse(callback, reqState);
                }
                catch (Exception ex)
                {
                    TraceLog.TraceException("Exception in BeginGetResponse", ex);

                    // signal that a network operation is done and unsuccessful
                    if (netOpInProgressDel != null)
                    {
                        netOpInProgressDel.DynamicInvoke(false, OperationStatus.Failed);
                    }
                }
            }
            else
            {
                // this is a request that contains a body (PUT, POST, DELETE)
                // need to nest another async call - this time to get the request stream
                try
                {
                    request.BeginGetRequestStream(
                        new AsyncCallback(InvokeWebServiceRequest_Inner),
                        new WebInvokeServiceState()
                        {
                            Request = request,
                            Callback = callback,
                            Delegate = del,
                            NetworkOperationInProgressDelegate = netOpInProgressDel,
                            RequestBody = obj
                        });
                }
                catch (Exception ex)
                {
                    // trace the exception
                    TraceLog.TraceException("Exception in BeginGetResponse", ex);

                    // signal that a network operation is done and unsuccessful
                    if (netOpInProgressDel != null)
                    {
                        netOpInProgressDel.DynamicInvoke(false, OperationStatus.Failed);
                    }
                }
            }
        }

        private static void InvokeWebServiceRequest_Inner(IAsyncResult res)
        {
            WebInvokeServiceState state = res.AsyncState as WebInvokeServiceState;
            if (state == null)
            {
                TraceLog.TraceError("Web Service State not found");
                return;
            }

            var request = state.Request;
            if (request == null)
            {
                TraceLog.TraceError("Web Service Request not found");
                return;
            }

            Stream stream = null;
            try
            {
                // this will throw if the connection can't be established
                stream = request.EndGetRequestStream(res);
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("Can't obtain stream", ex);
                return;
            }

            // serialize a request body if one was passed in (and the verb will take it)
            if (state.RequestBody != null && request.Method != "GET")
            {
#if IOS
                request.UserAgent = UserAgents.IOSPhone;
#else
                //request.UserAgent = UserAgents.WinPhone;
#endif
                // a null request body means that the caller wants to get the stream back and write to it directly
                if (state.RequestBody as Delegate != null)
                {
                    Delegate streamDel = (Delegate)state.RequestBody;
                    // invoke the delegate passed in with the request stream, so that the external caller
                    // can push data into the stream as it becomes available
                    streamDel.DynamicInvoke(stream);
                }
                else
                {
                    // the caller passed the complete object - so just serialize it onto the stream
                    if (state.RequestBody.GetType() == typeof(byte[]))
                    {
                        /*
                        byte[] bytes = (byte[])state.RequestBody;
#if !IOS
                        stream = new GZipStream(stream, CompressionMode.Compress);
                        request.ContentType = "application/x-gzip";
#else
						stream = new MemoryStream();
                        request.ContentType = "application/octet-stream";
#endif
                        stream.Write(bytes, 0, bytes.Length);
                         */
                    }
                    else
                    {
                        request.ContentType = "application/json";
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(state.RequestBody.GetType());
                        ser.WriteObject(stream, state.RequestBody);
                    }
                    // we got all the data into the stream, so flush/close it
                    stream.Flush();
                    stream.Close();
                }
            }

            // complete the invocation (this is not done inline because the InvokeWebServiceRequest_Inner_Complete() method
            // is reused by external callers that want to write to a stream directly and then invoke the operation)
            InvokeWebServiceRequest_Invoke(request, state.Delegate, state.NetworkOperationInProgressDelegate, state.Callback);
        }

        private static void InvokeWebServiceRequest_Invoke(HttpWebRequest request, Delegate del, Delegate netOpInProgressDel, AsyncCallback callback)
        {
            // execute the web request and get the response
            try
            {
                WebServiceState reqState = new WebServiceState()
                {
                    Request = request,
                    Delegate = del,
                    NetworkOperationInProgressDelegate = netOpInProgressDel
                };
                request.BeginGetResponse(callback, reqState);
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("BeginGetResponse failed", ex);

                // signal the operation is done and unsuccessful
                if (netOpInProgressDel != null)
                    netOpInProgressDel.DynamicInvoke(false, OperationStatus.Failed);
            }
        }


        // Common code to process the response from any web service call.  This is invoked from the callback 
        // method for the web service, and passed a Type for deserializing the response body. 
        // This method will also invoke the delegate with the result of the Web Service call
        private static void ProcessResponse<T>(IAsyncResult result)
        {
            WebServiceState state = result.AsyncState as WebServiceState;
            if (state == null)
            {
                TraceLog.TraceError("Web Service State not found");
                return;
            }

            // get the network operation status delegate
            Delegate netOpInProgressDel = state.NetworkOperationInProgressDelegate as Delegate;

            // get the web response and make sure it's not null (failed)
            //HttpWebResponseWrapper<T> resp = GetWebResponse<T>(result);
            HttpWebResponse resp = GetWebResponse<T>(result);
            if (resp == null)
            {
                TraceLog.TraceError("GetWebResponse failed");

                // signal that the network operation completed unsuccessfully
                if (netOpInProgressDel != null)
                {
                    netOpInProgressDel.DynamicInvoke(false, OperationStatus.Failed);
                }
                return;
            }
            else
            {
                OperationStatus status = AsOperationStatus(resp.StatusCode);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {   // using this status code to indicate cookie has expired or is invalid
                    if (authCookie != null)
                    {   // remove cookie and retry with credentials 
                        status = OperationStatus.Retry;
                        authCookie = null;
                    }
                }
                if (resp.StatusCode == HttpStatusCode.Forbidden)
                {   // remove cookie and force authentication on next request
                    authCookie = null;
                }

                if (netOpInProgressDel != null)
                {   // signal the network operation completed and whether it completed successfully
                    netOpInProgressDel.DynamicInvoke(false, status);
                    if (status == OperationStatus.Retry)
                    {   // delegate will retry, exit now
                        TraceLog.TraceInfo("Received a Retry response from Service");
                        return;
                    }
                }
            }

            // get the method-specific delegate
            Delegate del = state.Delegate as Delegate;
            //if (del == null)
            //    return;  // if no delegate was passed, the results can't be processed

            // invoke the delegate with the response body
            try
            {
                T resultObject = (T) new DataContractJsonSerializer(typeof(T)).ReadObject(resp.GetResponseStream());
                //T resultObject = resp.GetBody();
                if (del == null)
                    return;  // if no delegate was passed, the results can't be processed
                del.DynamicInvoke(resultObject);
            }
            catch (Exception ex)
            {
                TraceLog.TraceException("ProcessResponse: exception from GetBody or DynamicInvoke", ex);
                if (del == null)
                    return;  // if no delegate was passed, the results can't be processed
                del.DynamicInvoke(null);
            }
        }

        private static OperationStatus AsOperationStatus(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created || statusCode == HttpStatusCode.Accepted)
            {
                return OperationStatus.Success;
            }
            return OperationStatus.Failed;
        }

        private class WebInvokeServiceState
        {
            public HttpWebRequest Request { get; set; }  // HttpWebRequest for this call
            public AsyncCallback Callback { get; set; }  // callback for the GetResponse
            public Delegate Delegate { get; set; }  // delegate passed in by the caller
            public Delegate NetworkOperationInProgressDelegate { get; set; }  // delegate passed in by the caller
            public object RequestBody { get; set; }  // object to serialize on the request
        }

        private class WebServiceState
        {
            public HttpWebRequest Request { get; set; }  // HttpWebRequest for this call
            public Delegate Delegate { get; set; }  // delegate passed in by the caller
            public Delegate NetworkOperationInProgressDelegate { get; set; }  // delegate passed in by the caller
        }

        #endregion
    }
}
