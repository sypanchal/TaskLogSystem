using System.Web;
using System.Web.Optimization;

namespace TaskLogSystem
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/themejq").Include(
                        "~/Content/Assets/plugins/jQuery/jQuery-2.2.0.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/minjs").Include(
            "~/Content/Assets/bootstrap/js/bootstrap.min.js",
            "~/Content/Assets/plugins/datepicker/bootstrap-datepicker.js",
            "~/Content/Assets/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.all.min.js",
            "~/Content/Assets/plugins/slimScroll/jquery.slimscroll.min.js",
            "~/Content/Assets/plugins/daterangepicker/daterangepicker.js",
            "~/Content/Assets/plugins/fastclick/fastclick.js",
            "~/Content/Assets/dist/js/app.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/customstyle").Include("~/Content/CustomStyle.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/plugins").Include(
                     //"~/Content/Assets/bootstrap/css/bootstrap.min.css",
                     "~/Content/Assets/dist/css/AdminLTE.min.css",
                     "~/Content/Assets/dist/css/skins/_all-skins.min.css",
                     "~/Content/Assets/plugins/iCheck/flat/blue.css",
                     "~/Content/Assets/plugins/morris/morris.css",
                     "~/Content/Assets/plugins/datepicker/datepicker3.css",
                     "~/Content/Assets/plugins/daterangepicker/daterangepicker-bs3.css",
                     "~/Content/Assets/plugins/bootstrap-wysihtml5/bootstrap3-wysihtml5.min.css"));
        }
    }
}