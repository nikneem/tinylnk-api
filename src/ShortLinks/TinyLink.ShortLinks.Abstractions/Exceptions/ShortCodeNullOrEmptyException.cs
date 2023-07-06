using TinyLink.ShortLinks.Abstractions.ErrorCodes;

namespace TinyLink.ShortLinks.Abstractions.Exceptions;

public class ShortCodeNullOrEmptyException : UrlShortnerShortLinkException
{
    public ShortCodeNullOrEmptyException() : base(UrlShortnerShortLinksErrorCodes.ShortCodeNullOrEmpty)
    {
    }
}