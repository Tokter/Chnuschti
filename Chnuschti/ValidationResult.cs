using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chnuschti;

public class ValidationResult
{
    public bool IsValid { get; }
    public string? Error { get; }

    private ValidationResult(bool ok, string? msg = null) { IsValid = ok; Error = msg; }
    public static ValidationResult Valid => new(true);
    public static ValidationResult Invalid(string msg) => new(false, msg);
}

public abstract class ValidationRule
{
    public abstract ValidationResult Validate(object? value);
}
