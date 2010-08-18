﻿using System;
using System.Diagnostics.CodeAnalysis;
using Composite.Logging;


namespace Composite.Implementation
{
#warning RELEASE: Missing documentation
    public class LogDefaultImplementation : LogBase
    {
        public override void LogCritical(string title, Exception exception)
        {
            LoggingService.LogCritical(title, exception);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public override void LogCritical(string title, string messageFormat, params object[] args)
        {
            LoggingService.LogCritical(title, string.Format(messageFormat, args));
        }

        public override void LogError(string title, Exception exception)
        {
            LoggingService.LogError(title, exception);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public override void LogError(string title, string messageFormat, params object[] args)
        {
            LoggingService.LogError(title, string.Format(messageFormat, args));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public override void LogInformation(string title, string messageFormat, params object[] args)
        {
            LoggingService.LogInformation(title, string.Format(messageFormat, args));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public override void LogVerbose(string title, string messageFormat, params object[] args)
        {
            LoggingService.LogVerbose(title, string.Format(messageFormat, args));
        }

        public override void LogWarning(string title, Exception exception)
        {
            LoggingService.LogWarning(title, exception);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public override void LogWarning(string title, string messageFormat, params object[] args)
        {
            LoggingService.LogWarning(title, string.Format(messageFormat, args));
        }
    }
}
