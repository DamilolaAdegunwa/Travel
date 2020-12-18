using System;

namespace Travel.Core.Domain.Entities.Enums
{
    [Flags]
    public enum BookingTypes
    {
        Terminal,
        Advanced,
        Online,
        All,
        BookOnHold
    }
}