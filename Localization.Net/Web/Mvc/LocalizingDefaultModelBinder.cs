using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Localization.Net.Web.Mvc
{
    public class LocalizingDefaultModelBinder : DefaultModelBinder
    {
        public TextManager TextManager { get; protected set; }
        
        public LocalizingDefaultModelBinder(TextManager textManager)
        {
            TextManager = textManager;
        }
        

        protected override void SetProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, object value)
        {            
            base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
            
            var metadata = bindingContext.PropertyMetadata[propertyDescriptor.Name];
            var key = CreateSubPropertyName(bindingContext.ModelName, propertyDescriptor.Name);

            bool hasFormatException = false;

            ModelState state;            
            if (bindingContext.ModelState.TryGetValue(key, out state))
            {                                
                foreach (var err in state.Errors.Where(x => x.Exception != null).ToList())
                {
                    for (var inner = err.Exception; inner != null; inner = inner.InnerException)
                    {
                        if (inner is FormatException)
                        {                            
                            hasFormatException = true;
                            //DefaultModelBinder.BindProperty shouldn't consider this (thanks Reflector)
                            var msg = ExceptionHelper.LocalizeValidationException(TextManager, inner, metadata, 
                                value: state.Value);
                            if (!string.IsNullOrEmpty(msg))
                            {
                                state.Errors.Remove(err);
                                state.Errors.Add(msg);
                            }
                        }                        
                    }
                }
            }


            //Replace required message for non nullable types (again, thanks Reflector)
            var type = propertyDescriptor.PropertyType;
            bool nonNullable = type.IsValueType && Nullable.GetUnderlyingType(type) == null;
            if (nonNullable && value == null && !hasFormatException )
            {                
                //"Simulate" RequiredAttribute on property
                var attr = new RequiredAttribute();
                var msg = ExceptionHelper.LocalizeValidationException(TextManager, new ValidationException("", attr, value), metadata);
                
                if (msg != null)
                {
                    bindingContext.ModelState.Remove(key);
                    bindingContext.ModelState.AddModelError(key, msg);
                }
            }            
        }
        
    }
}
