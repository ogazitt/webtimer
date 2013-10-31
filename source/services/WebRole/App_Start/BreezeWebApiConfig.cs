using Newtonsoft.Json;
using System.Web.Http;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(WebTimer.WebRole.App_Start.BreezeWebApiConfig), "RegisterBreezePreStart")]
namespace WebTimer.WebRole.App_Start {
  ///<summary>
  /// Inserts the Breeze Web API controller route at the front of all Web API routes
  ///</summary>
  ///<remarks>
  /// This class is discovered and run during startup; see
  /// http://blogs.msdn.com/b/davidebb/archive/2010/10/11/light-up-your-nupacks-with-startup-code-and-webactivator.aspx
  ///</remarks>
  public static class BreezeWebApiConfig {

    public static void RegisterBreezePreStart() {
      GlobalConfiguration.Configuration.Routes.MapHttpRoute(
          name: "BreezeApi",
          routeTemplate: "api/{controller}/{action}"
      );
    }
  }
}

public class CustomBreezeConfig : Breeze.WebApi.BreezeConfig
{

    ///<summary> Enable sending of null values to the client. </summary>
    protected override JsonSerializerSettings CreateJsonSerializerSettings()
    {
        var baseSettings = base.CreateJsonSerializerSettings();
        baseSettings.NullValueHandling = NullValueHandling.Include; // SEND NULL VALUES
        return baseSettings;
    }
}