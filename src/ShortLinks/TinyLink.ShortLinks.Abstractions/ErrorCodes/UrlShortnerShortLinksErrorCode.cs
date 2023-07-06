using TinyLink.Core.ErrorCodes;

namespace TinyLink.ShortLinks.Abstractions.ErrorCodes;

public abstract class UrlShortnerShortLinksErrorCode : UrlShortnerErrorCode
{
    public override string ErrorNamespace => $"{base.ErrorNamespace}.ShortLinks";
}