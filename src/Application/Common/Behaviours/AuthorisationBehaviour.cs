using Accounts.Application.Common.Exceptions;
using Accounts.Application.Common.Interfaces;
using Accounts.Application.Common.Security;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;

namespace Accounts.Application.Common.Behaviours
{
    public class AuthorisationBehaviour<TMessage> :
                    IFilter<ConsumeContext<TMessage>>
                    where TMessage : class
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public AuthorisationBehaviour(
            ICurrentUserService currentUserService,
            IIdentityService identityService)
        {
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("authorisation-behaviour-filter");
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            var authorizeAttributes = context.Message.GetType().GetCustomAttributes<AuthorizeAttribute>();

            if (authorizeAttributes.Any())
            {
                // Must be authenticated user
                if (_currentUserService.UserId == null)
                {
                    throw new UnauthorizedAccessException();
                }

                // Role-based authorization
                var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

                if (authorizeAttributesWithRoles.Any())
                {
                    foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                    {
                        var authorized = false;
                        foreach (var role in roles)
                        {
                            var isInRole = await _identityService.IsInRoleAsync(_currentUserService.UserId, role.Trim());
                            if (isInRole)
                            {
                                authorized = true;
                                break;
                            }
                        }

                        // Must be a member of at least one role in roles
                        if (!authorized)
                        {
                            throw new ForbiddenAccessException();
                        }
                    }
                }

                // Policy-based authorization
                var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Policy));
                if (authorizeAttributesWithPolicies.Any())
                {
                    foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
                    {
                        var authorized = await _identityService.AuthorizeAsync(_currentUserService.UserId, policy);

                        if (!authorized)
                        {
                            throw new ForbiddenAccessException();
                        }
                    }
                }
            }

            await next.Send(context);
        }
    }
}