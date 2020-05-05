using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DNSRouting.Services
{
  public class CustomRouter : MvcRouteHandler, IRouter
    {
        private IActionContextAccessor _actionContextAccessor;
        private IActionInvokerFactory _actionInvokerFactory;
        private IActionSelector _actionSelector;
        private ILogger _logger;
        private DiagnosticSource _diagnosticSource;


        public CustomRouter(
            IActionInvokerFactory actionInvokerFactory,
            IActionSelector actionSelector,
            DiagnosticSource diagnosticSource,
            ILoggerFactory loggerFactory)
            : this(actionInvokerFactory, actionSelector, diagnosticSource, loggerFactory, actionContextAccessor: null)
        {
        }

        public CustomRouter(IActionInvokerFactory actionInvokerFactory, IActionSelector actionSelector, DiagnosticSource diagnosticSource,
            ILoggerFactory loggerFactory, IActionContextAccessor actionContextAccessor)
            : base(actionInvokerFactory, actionSelector, diagnosticSource,
            loggerFactory, actionContextAccessor)
        {
            _actionContextAccessor = actionContextAccessor;
            _actionInvokerFactory = actionInvokerFactory;
            _actionSelector = actionSelector;
            _diagnosticSource = diagnosticSource;
            _logger = loggerFactory.CreateLogger<MvcRouteHandler>();
        }

        public new Task RouteAsync(RouteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string Host = context.HttpContext.Request.Host.Host;
            if (Host == "localhost")
            {
                // Do nothing, normal routing
            }
            else
            {
                context.RouteData.Values["controller"] = "Alternate";
                context.RouteData.Values.Add("Host", Host); // Add a variable for fun
            }

            //All the next code is copied from base class
            var candidates = _actionSelector.SelectCandidates(context);
            if (candidates == null || candidates.Count == 0)
            {
                return Task.CompletedTask;
            }

            var actionDescriptor = _actionSelector.SelectBestCandidate(context, candidates);
            if (actionDescriptor == null)
            {
                return Task.CompletedTask;
            }

            context.Handler = (c) =>
            {
                var routeData = c.GetRouteData();

                var actionContext = new ActionContext(context.HttpContext, routeData, actionDescriptor);
                if (_actionContextAccessor != null)
                {
                    _actionContextAccessor.ActionContext = actionContext;
                }

                var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
                if (invoker == null)
                {
                    throw new InvalidOperationException();
                }

                return invoker.InvokeAsync();
            };

            return Task.CompletedTask;
        }
    }
}
