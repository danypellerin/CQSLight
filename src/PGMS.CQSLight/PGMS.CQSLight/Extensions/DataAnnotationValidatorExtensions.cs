using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PGMS.CQSLight.Extensions
{
    public static class DataAnnotationValidatorExtensions
    {
        public static bool IsValid(this object obj) => Validate(obj, out List<ValidationResult> validationResults);

        public static bool Validate(this object obj, out List<ValidationResult> results)
        {
            var context = new ValidationContext(obj, null, null);
            results = new List<ValidationResult>();
            return Validator.TryValidateObject(obj, context, results, true);
        }
    }
}