using System.Text.RegularExpressions;
using Labora.Application.Interfaces;
using Labora.Domain.Exceptions;

namespace Labora.Application.Services;

public class UzbekistanPhoneNumberNormalizer : IPhoneNumberNormalizer
{
    private const string CountryCode = "998";
    private const int SubscriberNumberLength = 9;
    private static readonly Regex FormattingCharacters = new(@"[\s\-()]", RegexOptions.Compiled);

    public string Normalize(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new InvalidPhoneNumberException("Phone number is required.");
        }

        string stripped = FormattingCharacters.Replace(phoneNumber.Trim(), string.Empty);
        string digits = stripped.StartsWith('+') ? stripped[1..] : stripped;

        if (digits.Length == 0 || !digits.All(char.IsAsciiDigit))
        {
            throw new InvalidPhoneNumberException($"Phone number '{phoneNumber}' contains unsupported characters.");
        }

        string subscriberNumber = digits.Length switch
        {
            SubscriberNumberLength => digits,
            SubscriberNumberLength + 3 when digits.StartsWith(CountryCode) => digits[3..],
            _ => throw new InvalidPhoneNumberException($"Phone number '{phoneNumber}' is not a recognized Uzbekistan number.")
        };

        return $"+{CountryCode}{subscriberNumber}";
    }
}
