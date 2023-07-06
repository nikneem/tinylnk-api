using TinyLink.Core.Exceptions;
using TinyLink.ShortLinks.Abstractions.ErrorCodes;

namespace TinyLink.ShortLinks.Abstractions.Exceptions;

public class ShortCodeNotFoundException : UrlShortnerBaseException
{
    public ShortCodeNotFoundException(Exception? innerException = null) : base(UrlShortnerShortLinksErrorCodes.ShortLinkNotFound, innerException)
    {
    }
}