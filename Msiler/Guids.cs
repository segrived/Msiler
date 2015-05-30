// Guids.cs
// MUST match guids.h

using System;

namespace Quart.Msiler
{
    internal static class GuidList
    {
        public const string guidMsilerPkgString = "2e50f4f0-18d1-419e-a204-f1156c910f2b";
        public const string guidMsilerCmdSetString = "04d989fc-bbaa-4e42-aef8-c93d8727da2b";
        public const string guidToolWindowPersistanceString = "0c127690-de92-4d02-a743-634bb922145c";
        public static readonly Guid guidMsilerCmdSet = new Guid(guidMsilerCmdSetString);
    };
}