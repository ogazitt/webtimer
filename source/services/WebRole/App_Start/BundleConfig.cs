using System.Web;
using System.Web.Optimization;

namespace WebTimer.WebRole
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",           // jQuery itself
                        "~/Scripts/jquery-migrate-{version}.js")); // jQuery migrate

            bundles.Add(new ScriptBundle("~/bundles/google-analytics").Include(
                        "~/Scripts/google-analytics.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Scripts/angular.js"));

            bundles.Add(new ScriptBundle("~/bundles/ajaxlogin").Include(
                        "~/app/ajaxlogin.js"));

            bundles.Add(new ScriptBundle("~/bundles/extras").Include(
                        "~/Scripts/jquery.colorPicker.js",
                        "~/Scripts/angular-colorpicker.js",
                        "~/Scripts/date.js"));

            bundles.Add(new ScriptBundle("~/bundles/breeze").Include(
                        "~/Scripts/q.js",
                        "~/Scripts/breeze.debug.js",
                        "~/Scripts/breeze.min.js",
                        "~/Scripts/breeze.savequeuing.js"));

            bundles.Add(new ScriptBundle("~/bundles/highcharts").Include(
                        "~/Scripts/highcharts/highstock.js",
                        "~/Scripts/angular-highcharts.js"));

            bundles.Add(new ScriptBundle("~/bundles/uservoice").Include(
                        "~/Scripts/uservoice.js"));

            bundles.Add(new ScriptBundle("~/bundles/spin").Include(
                        "~/Scripts/spin.js"));

            bundles.Add(new ScriptBundle("~/bundles/controls").Include(
                        "~/Scripts/controls.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                        "~/app/dashboard.js", // must be first
                        "~/app/dashboard.datacontext.js", 
                        "~/app/dashboard.model.js",
                        "~/app/logger.js",
                        "~/app/controllers/day.controller.js",
                        "~/app/controllers/detail.day.controller.js",
                        "~/app/controllers/devices.controller.js",
                        "~/app/controllers/people.controller.js",
                        "~/app/controllers/log.controller.js",
                        "~/app/controllers/header.controller.js",
                        "~/app/controllers/toolbar.controller.js"));


            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/Site.css",
                        "~/Content/jquery-colorPicker.css",
                        "~/Content/btnswitch.css",
                        "~/Content/TodoList.css"));

            bundles.Add(new StyleBundle("~/Content/dashboard").Include(
                        "~/Content/dashboard.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
        }
    }
}