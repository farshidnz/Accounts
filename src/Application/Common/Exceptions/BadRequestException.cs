using System;

namespace Accounts.Application.Common.Exceptions;

[Serializable]
public class BadRequestException : Exception
{
    public BadRequestException(string message)
           : base(message)
    {
    }
}