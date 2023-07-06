using TinyLink.ShortLinks.Abstractions.ErrorCodes;

namespace TinyLink.ShortLinks.Abstractions.Exceptions;

public class ShortCodeInvalidException : UrlShortnerShortLinkException
{
    public ShortCodeInvalidException() : base(UrlShortnerShortLinksErrorCodes.ShortLinkInvalid)
    {
    }

}