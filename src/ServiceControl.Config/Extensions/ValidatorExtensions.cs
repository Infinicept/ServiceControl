﻿namespace ServiceControl.Config.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation;
    using FluentValidation.Internal;
    using FluentValidation.Results;

    static class ValidatorExtensions
    {
        public static ValidationResult Validate<T>(this IValidator validator, T instance, string propertyName)
        {
            return validator.Validate(new ValidationContext<T>(instance, new PropertyChain(), new DefaultValidatorSelector()), propertyName);
        }

        public static ValidationResult Validate<T>(this IValidator validator, ValidationContext<T> context, string propertyName)
        {
            if (!(validator is IEnumerable<IValidationRule> validatorWrapper))
            {
                return validator.Validate(context);
            }

            var failures = validatorWrapper
                .Where(rule =>
                {
                    if (rule is PropertyRule propertyRule)
                    {
                        return propertyRule.PropertyName == propertyName;
                    }

                    return true;
                })
                .SelectMany(x => x.Validate(context))
                .ToList();
            return new ValidationResult(failures);
        }
    }
}