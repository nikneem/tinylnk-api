using TinyLink.ShortLinks.Abstractions.ErrorCodes;

namespace TinyLink.ShortLinks.Abstractions.Exceptions;


public class TargetUrlNullOrEmptyException : UrlShortnerShortLinkException
{
    public TargetUrlNullOrEmptyException() : base(UrlShortnerShortLinksErrorCodes.TargetUrlNullOrEmpty)
    {
    }
}
