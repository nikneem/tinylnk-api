using TinyLink.ShortLinks.Abstractions.ErrorCodes;

namespace TinyLink.ShortLinks.Abstractions.Exceptions;

public class TargetUrlInvalidException : UrlShortnerShortLinkException
{
    public TargetUrlInvalidException() : base(UrlShortnerShortLinksErrorCodes.TargetUrlInvalid)
    {
    }

}