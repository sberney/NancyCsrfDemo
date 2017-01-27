using System;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Nancy.ViewEngines;
using Nancy.ErrorHandling;

namespace NancyCsrfDemo
{
    public class Main : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Nancy.Security.Csrf.Enable(pipelines);
        }
    }

    public class Module : NancyModule
    {
        public Module() : base("/")
        {
            Get["/success"] = _ =>
            {
                ViewBag["Status"] = "Success";
                return View["Success"];
            };

            // Because we are implementing a Single-Page-Application,
            // we can set a few variables (not implemented) to
            // give the user notice that this initial route is unavailable,
            //   and so send them the application -- which requires CSRF tokens.
            //
            Get["/expected-failure"] = _ =>
            {
                return HttpStatusCode.InternalServerError;    // I work
            };

            // you will see "CSRF is not enabled on this request" instead
            // of the expected "intentional unhandled exception"
            //
            Get["/failure"] = _ =>
            {
                throw new Exception("intentional unhandled exception");  // I don't work
            };
        }
    }

    public class CustomErrorHandler : DefaultViewRenderer, IStatusCodeHandler
    {
        public CustomErrorHandler(IViewFactory factory) : base(factory) { }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return true;    // handles 500 errors too.
            //return statusCode != HttpStatusCode.InternalServerError;  // We can simply not customize 500 errors.
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            context.ViewBag["Status"] = statusCode.ToString();
            context.Response = RenderView(context, "Success");
            context.Response.StatusCode = statusCode;
        }
    }

    public static class Extensions
    {
        public static string CsrfToken(this Nancy.ViewEngines.Razor.HtmlHelpers html)
        {
            return html.RenderContext.GetCsrfToken().Value;
        }
    }
}