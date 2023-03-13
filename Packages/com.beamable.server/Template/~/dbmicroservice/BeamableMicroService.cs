/*
 *  Simple CustomMicroservice example
 */
//#define DB_MICROSERVICE  // I sometimes enable this to see code better in rider


using Core.Server.Common;
using JsonSerializer = System.Text.Json.JsonSerializer;
#if DB_MICROSERVICE
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Text.Json;
using Newtonsoft.Json;

namespace Beamable.Server
{
   [System.AttributeUsage(System.AttributeTargets.Method)]
   public class ClientCallable : System.Attribute
   {
      private string pathName = "";

      public ClientCallable()
      {
      }

      public ClientCallable(string pathnameOverride)
      {
         pathName = pathnameOverride;
      }

      /* PathName:  It maybe that they want to specific a different route to this callable
      *  // steam/purchase/begin
    *  // steam/purchase/complete
    *  // google/purchase/begin
    */
      public string PathName
      {
         set { pathName = value; }
         get { return pathName; }
      }
   }

   public class BeamableMicroService
   {
      public string MicroserviceName => _serviceAttribute.MicroserviceName;
      public string QualifiedName => $"micro_{MicroserviceName}"; // scope everything behind a feature name "micro"

      private bool isStarted = false;
      private int reqId = 0;
      private Dictionary<int, Action<string>> callbacks;
      private EasyWebSocket _webSocket;

      private ServiceMethodCollection _serviceMethods;

      private MicroserviceAttribute _serviceAttribute;

      private IMicroserviceArgs _args;
      private ServiceFactory<MicroService> _serviceFactory;
      protected string CustomerID => _args.CustomerID;
      protected string ProjectName => _args.ProjectName;
      protected string Secret => _args.Secret;
      protected string Host => _args.Host;

      public BeamableMicroService()
      {


      }

      public void Start<TMicroService>(ServiceFactory<TMicroService> factory, IMicroserviceArgs args)
         where TMicroService : MicroService
      {
         _serviceFactory = factory;

         var t = typeof(TMicroService);
         _serviceAttribute = t.GetCustomAttribute<MicroserviceAttribute>();
         if (_serviceAttribute == null)
         {
            throw new Exception($"Cannot create service. Missing [{typeof(MicroserviceAttribute).Name}].");
         }

         _args = args.Copy();
         Console.WriteLine($"CustomerId = {CustomerID}, ProjectName = {ProjectName}, Secret = {Secret}, Host = {Host}.");

         if (isStarted)
            return;

         isStarted = true; // todo: this needs to be smarter
         callbacks = new Dictionary<int, Action<string>>();
//         services = new Dictionary<string, Action<string, string, long, string, Action<object>>>();
//         serviceMethods = new Dictionary<string, Action<long, string, Action<object>>>();

         _serviceMethods = ServiceMethodHelper.Scan<TMicroService>();

         //string serverUri = "wss://thorium-dev.disruptorbeam.com/socket";  // todo: how should this work?
         _webSocket = EasyWebSocket.Create(Host);

         // Call backs
         _webSocket.OnConnect(AuthorizeConnection);
         _webSocket.OnDisconnect(CloseConnection);
         _webSocket.OnMessage(HandleWebsocketMessage);

         // Connect and Run
         Console.Write("Input message ('exit' to exit): \n");
         Console.Write("trying to connect to: " + Host + "\n");
         _webSocket.Connect();

         CancellationTokenSource cancelSource = new CancellationTokenSource();
         cancelSource.Token.WaitHandle.WaitOne();
      }

      RequestContext buildRequestContext(string msg)
      {
         int id = 0;
         string path = "";
         string methodName = "";
         string body = ""; //is there an advantage to keeping it a JsonElement?
         long userID = 0;

         try
         {
            using (JsonDocument document = JsonDocument.Parse(msg))
            {
               id = document.RootElement.GetProperty("id").GetInt32();
               body = document.RootElement.GetProperty("body").ToString();
               JsonElement temp;
               if (document.RootElement.TryGetProperty("path", out temp))
                  path = temp.GetString();
               if (document.RootElement.TryGetProperty("method", out temp))
                  methodName = temp.GetString();
               if (document.RootElement.TryGetProperty("from", out temp))
                  userID = temp.GetInt64();
            }
         }
         catch (Exception e)
         {
            Console.Write("Exception: " +
                          e); // was not printing exceptions without this was just silently moving on so I am doing that
         }

         return new RequestContext(CustomerID, ProjectName, id, userID, path, methodName, body);
      }

      async void HandleWebsocketMessage(EasyWebSocket ws, string msg)
      {
         var ctx = buildRequestContext(msg);

         if (string.IsNullOrEmpty(ctx.Path))
         {
            // this is a platform service callback. Handle the callback.
            var callback = callbacks[ctx.Id];
            if (callback != null)
            {
               callback(ctx.Body);
            }
            else
            {
               throw new Exception($"Received callback message, but no callback was registered. id=[{ctx.Id}]");
            }
         }
         else
         {
            // this is a client request. Handle the service method.

            try
            {
               var argStrings = new List<string>();
               using (var bodyDoc = JsonDocument.Parse(ctx.Body))
               {
                  // extract the payload object.
                  if (bodyDoc.RootElement.TryGetProperty("payload", out var payloadString))
                  {
                     using (var payloadDoc = JsonDocument.Parse(payloadString.ToString()))
                     {
                        foreach (var argJson in payloadDoc.RootElement.EnumerateArray())
                        {
                           argStrings.Add(argJson.ToString());
                        }
                     }
                  }
               }

               var route = ctx.Path.Substring(QualifiedName.Length + 1);

               var service = _serviceFactory.Invoke();
               service.ProvideContext(ctx);

               var result = await _serviceMethods.Handle(service, route, argStrings.ToArray());

               var response = new GatewayResponse
               {
                  id = ctx.Id,
                  status = 200,
                  body = new ClientResponse
                  {
                     payload = result
                  }
               };
               var responseJson = JsonConvert.SerializeObject(response);
               Console.WriteLine($"Responding with [{responseJson}]");
               ws.SendMessage(responseJson);
            }
            catch (TargetInvocationException ex)
            {
               var inner = ex.InnerException;
               Console.WriteLine($"Exception {inner.GetType().Name}: {inner.Message} - {inner.Source} \n {inner.StackTrace}");
               var failResponse = new GatewayResponse
               {
                  id = ctx.Id,
                  status = 500,
                  body = new ClientResponse
                  {
                     payload = ""
                  }
               };
               var failResponseJson = JsonConvert.SerializeObject(failResponse);
               ws.SendMessage(failResponseJson);
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Exception {ex.GetType().Name}: {ex.Message} - {ex.Source}");
               var failResponse = new GatewayResponse
               {
                  id = ctx.Id,
                  status = 500,
                  body = new ClientResponse
                  {
                     payload = ""
                  }
               };
               var failResponseJson = JsonConvert.SerializeObject(failResponse);
               ws.SendMessage(failResponseJson);
            }
         }
      }

      private void request(string method, string path, object body, Action<string> callback)
      {
         reqId = reqId + 1;
         if (callback != null)
         {
            callbacks.Add(reqId, callback);
         }

         var req = new Dictionary<string, object>()
         {
            {"id", reqId},
            {"method", method},
            {"path", path},
            {"body", body}
         };

         string s = JsonSerializer.Serialize(req);
         // Console.WriteLine($"request: {s}");
         _webSocket.SendMessage(s);
      }

      private void CloseConnection(EasyWebSocket ws)
      {
         Console.WriteLine("closing connection...");
      }

      private void AuthorizeConnection(EasyWebSocket ws)
      {
         Console.Write("authorizing connection... \n");
         request("get", "gateway/nonce", null, (string rsp) =>
         {
            string nonce = "";
            //string secret = "7a0ce144-8f3e-477d-bf81-91a2c2c9ea9b";  // todo:  how will this work with lots of customers  needs to be stored in the local Unity project but not compiled into the code or in p4

            using (JsonDocument document = JsonDocument.Parse(rsp))
            {
               nonce = document.RootElement.GetProperty("nonce").ToString();
            }

            string sig = signature(Secret + nonce);
            var req = new Dictionary<string, object>()
            {
               {"cid", CustomerID}, // todo:  how will this work with lots of customers => pull from config file
               {"pid", ProjectName}, // todo:  how will this work with lots of customers => pull from config file
               {"signature", sig}
            };

            var options = new JsonSerializerOptions
            {
               WriteIndented = true
            };
            string s = JsonSerializer.Serialize(req, options);
            request("post", "gateway/auth", req, (string response) =>
            {
               string result = "";
               try
               {
                  using (JsonDocument document = JsonDocument.Parse(response))
                  {
                     result = document.RootElement.GetProperty("result").ToString();
                     if (result == "ok")
                     {
                        Console.Write("authorization: " + result + "\n> ");
                        RegisterServiceProvider();
                     }
                     else
                     {
                        int status = document.RootElement.GetProperty("status").GetInt32();
                        string error = document.RootElement.GetProperty("error").ToString();
                        Console.Write("authorization error: " + status + " " + error + "\n> ");
                     }
                  }
               }
               catch (Exception e)
               {
                  Console.Write("Exception: " + e);
               }
            });
         });
      }

      public string signature(string text)
      {
         System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
         byte[] data = Encoding.UTF8.GetBytes(text);
         byte[] hash = md5.ComputeHash(data);
         return Convert.ToBase64String(hash);
      }

      private void provider(string name)
      {
         //services.Add(name, servicecallback);
         var req = new Dictionary<string, object>()
         {
            {"type", "basic"},
            {"name", name}
         };
         request("post", "gateway/provider", req,
            (string rsp) =>
            {
               Console.Write("platform register service provider callback response: " + rsp + "\n> ");
            });
      }

      private void RegisterServiceProvider()
      {
         Console.Write("RegisterService: " + QualifiedName + " \n> ");
         provider(QualifiedName);
      }

      public void Platform_SetLeaderboardScore(long userID, string boardId, double score)
      {
         var req = new Dictionary<string, object>()
         {
            {"id", userID},
            {"score", score},
         };
         request("put", $"object/leaderboards/{boardId}/entry", req, null);
      }
   }
}
#endif
