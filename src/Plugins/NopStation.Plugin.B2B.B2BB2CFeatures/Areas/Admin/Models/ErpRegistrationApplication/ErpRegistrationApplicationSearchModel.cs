using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication
{
    public record ErpRegistrationApplicationSearchModel : BaseSearchModel
    {
        #region Ctor

        public ErpRegistrationApplicationSearchModel()
        {
            ShowInActiveOption = new List<SelectListItem>();
            ShowIsApprovedOption = new List<SelectListItem>();
        }

        #endregion

        #region Properties
        public string FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Fields.FullRegisteredName")]
        public string FullRegisteredName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Fields.RegistrationNumber")]
        public string RegistrationNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Fields.AccountsEmail")]
        public string AccountsEmail { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Fields.Show")]
        public int ShowInActive { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Fields.ShowIsApproved")]
        public int ShowIsApproved { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Fields.FromCreatedDate")]
        [UIHint("DateNullable")]
        public DateTime? FromDate { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Fields.ToCreatedDate")]
        [UIHint("DateNullable")]
        public DateTime? ToDate { get; set; }

        public IList<SelectListItem> ShowInActiveOption { get; set; }
        public IList<SelectListItem> ShowIsApprovedOption { get; set; }

        #endregion
    }
}