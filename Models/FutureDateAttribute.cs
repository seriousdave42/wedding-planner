using System.ComponentModel.DataAnnotations;
using System;
namespace WeddingPlanner.Models
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime)value <= DateTime.Now)
            {
                return new ValidationResult("Date must be in the future");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}