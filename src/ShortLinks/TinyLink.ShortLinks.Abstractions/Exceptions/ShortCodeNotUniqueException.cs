using TinyLink.Core.Exceptions;
using TinyLink.ShortLinks.Abstractions.ErrorCodes;

namespace TinyLink.ShortLinks.Abstractions.Exceptions;

public class ShortCodeNotUniqueException : UrlShortnerBaseException
{
    public ShortCodeNotUniqueException(Exception? innerException = null) : base(UrlShortnerShortLinksErrorCodes.ShortCodeNotUnique, innerException)
    {
    }

}