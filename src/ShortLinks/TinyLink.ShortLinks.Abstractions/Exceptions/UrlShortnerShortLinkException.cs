using TinyLink.Core.Exceptions;
using TinyLink.ShortLinks.Abstractions.ErrorCodes;

namespace TinyLink.ShortLinks.Abstractions.Exceptions;

public class UrlShortnerShortLinkException : UrlShortnerBaseException
{
    public UrlShortnerShortLinkException(UrlShortnerShortLinksErrorCode errorCode, Exception? innerException = null) : base(errorCode, innerException)
    {
    }
}