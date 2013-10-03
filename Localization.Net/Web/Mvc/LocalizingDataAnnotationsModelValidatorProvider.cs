using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Localization.Net.Web.Mvc
{   

    public class LocalizingDataAnnotationsModelValidator : DataAnnotationsModelValidator
    {        
        public LocalizedValidationAttribute LocalizationInfo { get; set; }

        public LocalizingDataAnnotationsModelValidator(ModelMetadata metadata,
            ControllerContext context, ValidationAttribute attribute) :
            base(metadata, context, attribute)
        {
            LocalizationInfo = GetLocalizationInfo();            
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            try
            {                
                Attribute.Validate(Metadata.Model, "");
            }
            catch (ValidationException ex)
            {
                var msg = ExceptionHelper.LocalizeValidationException(LocalizationHelper.TextManager, ex, Metadata, LocalizationInfo);
                if (!string.IsNullOrEmpty(msg))
                {
                    return new[] { new ModelValidationResult { Message = msg } };
                }
            }
            return base.Validate(container);
        }

        protected string ClientErrorMessage
        {
            get
            {
                var msg = ExceptionHelper.LocalizeValidationException(LocalizationHelper.TextManager, 
                    new ValidationException("", Attribute, "@Value"), Metadata, LocalizationInfo);

                return msg ?? base.ErrorMessage;
            }
        }

        
        protected LocalizedValidationAttribute GetLocalizationInfo()
        {
            if (Metadata.ContainerType != null)
            {
                var attrHolder = !string.IsNullOrEmpty(Metadata.PropertyName) ?
                    (MemberInfo) Metadata.ContainerType.GetProperty(Metadata.PropertyName) : Metadata.ContainerType;

                return attrHolder.GetCustomAttributes(typeof(LocalizedValidationAttribute), true)
                    .Cast<LocalizedValidationAttribute>().FirstOrDefault(x => x.ForAttribute == Attribute.GetType());
            }

            return null;            
        }
    }

    #region Adapters for standard attributes

    public class LocalizingRangeAttributeAdapater : LocalizingDataAnnotationsModelValidator
    {
        public LocalizingRangeAttributeAdapater(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute) :
            base(metadata, context, attribute) {}

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var attr = Attribute as RangeAttribute;
            return new[] { new ModelClientValidationRangeRule(base.ClientErrorMessage, attr.Minimum, attr.Maximum) };
        }
    }

    public class LocalizingRegularExpressionAttributeAdapater : LocalizingDataAnnotationsModelValidator
    {
        public LocalizingRegularExpressionAttributeAdapater(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute) :
            base(metadata, context, attribute) { }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var attr = Attribute as RegularExpressionAttribute;
            return new[] { new ModelClientValidationRegexRule(base.ClientErrorMessage, attr.Pattern) };
        }
    }

    public class LocalizingRequiredAttributeAdapater : LocalizingDataAnnotationsModelValidator
    {
        public LocalizingRequiredAttributeAdapater(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute) :
            base(metadata, context, attribute) { }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var attr = Attribute as RequiredAttribute;
            return new[] { new ModelClientValidationRequiredRule(base.ClientErrorMessage) };
        }
    }

    public class LocalizingStringLengthAttributeAdapater : LocalizingDataAnnotationsModelValidator
    {
        public LocalizingStringLengthAttributeAdapater(ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute) :
            base(metadata, context, attribute) { }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var attr = Attribute as StringLengthAttribute;
            return new[] { new ModelClientValidationStringLengthRule(base.ClientErrorMessage, attr.MinimumLength, attr.MaximumLength) };
        }
    }

    #endregion
}
