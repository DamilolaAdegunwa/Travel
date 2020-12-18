using Travel.Core.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Travel.WebAPI.ViewModels
{
    public class PayStackResponseModel
    {
        public string email { get; set; }

        public int amount { get; set; }

        public string RefCode { get; set; }

        public string PayStackReference { get; set; }
    }
}
