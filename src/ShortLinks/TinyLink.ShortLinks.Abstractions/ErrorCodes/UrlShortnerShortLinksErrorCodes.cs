namespace TinyLink.ShortLinks.Abstractions.ErrorCodes;

public abstract class UrlShortnerShortLinksErrorCodes : UrlShortnerShortLinksErrorCode
{
    public static readonly UrlShortnerShortLinksErrorCode ShortLinkNotFound = new ShortLinkNotFoundErrorCode();
    public static readonly UrlShortnerShortLinksErrorCode QueryStringInvalid = new QueryStringInvalidErrorCode();
    public static readonly UrlShortnerShortLinksErrorCode ShortCodeCreationFailed= new ShortCodeCreationFailedErrorCode();
    public static readonly UrlShortnerShortLinksErrorCode ShortCodeNullOrEmpty = new ShortCodeNullOrEmptyErrorCode();
    public static readonly UrlShortnerShortLinksErrorCode ShortLinkInvalid = new ShortCodeInvalidErrorCode();
    public static readonly UrlShortnerShortLinksErrorCode ShortCodeNotUnique = new ShortCodeNotUniqueErrorCode();
    public static readonly UrlShortnerShortLinksErrorCode TargetUrlNullOrEmpty = new TargetUrlNullOrEmptyErrorCode();
    public static readonly UrlShortnerShortLinksErrorCode TargetUrlInvalid= new TargetUrlInvalidErrorCode();
}