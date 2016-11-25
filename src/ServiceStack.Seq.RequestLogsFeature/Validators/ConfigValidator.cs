namespace ServiceStack.Seq.RequestLogsFeature.Validators
{
    using System;
    using FluentValidation;

    internal class ConfigValidator : AbstractValidator<SeqRequestLogsFeature>
    {
        public ConfigValidator()
        {
            RuleFor(cs => cs.SeqUrl)
                .NotEmpty()
                .Must(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                .WithMessage("Seq Url is not a valid url");
        }
    }
}