namespace TinyLink.ShortLinks.Abstractions.ErrorCodes;

public class ShortCodeNullOrEmptyErrorCode : UrlShortnerShortLinksErrorCode
{
    public override string Code => "ShortLinkNullOrEmpty";
}