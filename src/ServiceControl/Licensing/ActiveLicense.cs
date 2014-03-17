﻿namespace Particular.ServiceControl.Licensing
{
    using Particular.Licensing;

    public class ActiveLicense
    {
        public bool IsValid { get; set; }
        public bool HasExpired { get; set; }
        public License Details { get; set; }
    }
}