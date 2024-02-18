using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Accounts.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {

        public ValidationException(string message) : base(message)
        {

        }
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : base($"{string.Join(" ", failures)}")
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        public IDictionary<string, string[]> Errors { get; }
    }
}