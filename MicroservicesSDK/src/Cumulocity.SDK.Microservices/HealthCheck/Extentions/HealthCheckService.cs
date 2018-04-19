using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cumulocity.SDK.Microservices.HealthCheck.Internal;
using System.Linq;
using System.Timers;
using Cumulocity.SDK.Microservices.Settings;
using Cumulocity.SDK.Microservices.Utils.Extenions;
using Microsoft.AspNetCore.Http;

namespace Cumulocity.SDK.Microservices.HealthCheck.Extentions
{
    public class HealthCheckService : IHealthCheckService
    {
		private readonly HealthCheckBuilder _builder;
        private readonly IReadOnlyList<HealthCheckGroup> _groups;
        private readonly HealthCheckGroup _root;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
	    private readonly System.Timers.Timer _timer;
	    private readonly TimeSpan _interval;

	    private IHttpContextAccessor _httpContextAccessor;
	    private Platform _platform;


		public HealthCheckService(HealthCheckBuilder builder, IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor httpContextAccessor, TimeSpan interval = default(TimeSpan))
        {
            _builder = builder;
            _groups = GetGroups().Where(group => group.GroupName != string.Empty).ToList();
            _root = GetGroup(string.Empty);
            _serviceProvider = serviceProvider;
            _serviceScopeFactory = serviceScopeFactory;
			_timer = new System.Timers.Timer();
	        _interval = interval == default(TimeSpan) ? TimeSpan.FromSeconds(100) : interval;
	        _platform = (Platform)serviceProvider.GetRequiredService(typeof(Platform));
	        _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CompositeHealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var scope = GetServiceScope())
            {
	            var isin = _httpContextAccessor.HttpContext.User.IsInContext();
	            var un =_httpContextAccessor.HttpContext.User.UserName();

	            var scopeServiceProvider = scope.ServiceProvider;
                var groupTasks = _groups.Select(group => new { Group = group, Task = RunGroupAsync(scopeServiceProvider, group, cancellationToken) }).ToList();
                var result = await RunGroupAsync(scopeServiceProvider, _root, cancellationToken).ConfigureAwait(false);

                await Task.WhenAll(groupTasks.Select(x => x.Task));

                foreach (var groupTask in groupTasks)
                {
                    result.Add($"Group({groupTask.Group.GroupName})", groupTask.Task.Result);
                }

	            StartTimer();
                return result;
            }
        }

	    private void StartTimer()
	    {
		    if (_timer.Enabled == false)
		    {
			    _timer.Elapsed += new ElapsedEventHandler(OnTimedEventAsync);
			    _timer.Interval = _interval.TotalMilliseconds;
			    _timer.Enabled = true;
		    }
	    }

	    private async  void OnTimedEventAsync(object sender, ElapsedEventArgs e)
		{
			var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

			using (var scope = GetServiceScope())
			{
				var scopeServiceProvider = scope.ServiceProvider;
				var groupTasks = _groups.Select(group => new { Group = group, Task = RunGroupAsync(scopeServiceProvider, group, timeoutTokenSource.Token) }).ToList();
				var result = await RunGroupAsync(scopeServiceProvider, _root, timeoutTokenSource.Token).ConfigureAwait(false);
				var completed = await Task.WhenAll(groupTasks.Select(x => x.Task));
			}
		}

		public IReadOnlyList<CachedHealthCheck> GetAllChecks()
            => _builder.ChecksByName.Values.ToList().AsReadOnly();

        public CachedHealthCheck GetCheck(string checkName)
            => _builder.ChecksByName[checkName];

        public HealthCheckGroup GetGroup(string groupName)
            => _builder.Groups[groupName];

        public IReadOnlyList<HealthCheckGroup> GetGroups()
            => _builder.Groups.Values.ToList().AsReadOnly();

        private IServiceScope GetServiceScope()
            => _serviceScopeFactory == null ? new UnscopedServiceProvider(_serviceProvider) : _serviceScopeFactory.CreateScope();

        public async ValueTask<IHealthCheckResult> RunCheckAsync(CachedHealthCheck healthCheck, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var scope = GetServiceScope())
            {
                return await RunCheckAsync(scope.ServiceProvider, healthCheck, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Uses the provided service provider and executes the provided check.
        /// </summary>
        public ValueTask<IHealthCheckResult> RunCheckAsync(IServiceProvider serviceProvider, CachedHealthCheck healthCheck, CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.ArgumentNotNull(nameof(serviceProvider), serviceProvider);
            Guard.ArgumentNotNull(nameof(healthCheck), healthCheck);

            return healthCheck.RunAsync(serviceProvider, cancellationToken);
        }

        /// <summary>
        /// Creates a new resolution scope from the default service provider and executes the checks in the given group.
        /// </summary>
        public async Task<CompositeHealthCheckResult> RunGroupAsync(HealthCheckGroup group, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var scope = GetServiceScope())
                return await RunGroupAsync(scope.ServiceProvider, group, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Uses the provided service provider and executes the checks in the given group.
        /// </summary>
        public async Task<CompositeHealthCheckResult> RunGroupAsync(IServiceProvider serviceProvider, HealthCheckGroup group, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new CompositeHealthCheckResult(group.PartiallyHealthyStatus);
            var checkTasks = group.Checks.Select(check => new { Check = check, Task = check.RunAsync(serviceProvider, cancellationToken).AsTask() }).ToList();
            await Task.WhenAll(checkTasks.Select(checkTask => checkTask.Task));

            foreach (var checkTask in checkTasks)
            {
                result.Add(checkTask.Check.Name, checkTask.Task.Result);
            }

            return result;
        }

        private class UnscopedServiceProvider : IServiceScope
        {
            public UnscopedServiceProvider(IServiceProvider serviceProvider)
                => ServiceProvider = serviceProvider;

            public IServiceProvider ServiceProvider { get; }

            public void Dispose() { }
        }
    }
}
