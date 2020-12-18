using System;

namespace Travel.Core.Domain.DataTransferObjects
{
    public class PassordResetDTO
    {
        public string UserNameOrEmail { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }


    public class ChangePassordDTO
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}