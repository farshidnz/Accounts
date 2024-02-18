using Accounts.Application.Common.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Accounts.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime Now
        {
            get
            {
                TimeZoneInfo cstZone = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                                TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney") : TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");

                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(UtcNow, cstZone);
                return cstTime;
            }
        }

    
    }
}
