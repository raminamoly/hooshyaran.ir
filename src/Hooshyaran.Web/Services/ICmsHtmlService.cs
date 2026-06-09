namespace Hooshyaran.Web.Services;

public interface ICmsHtmlService
{
    string ToSafeHtml(string value);

    string PlainTextToHtml(string value);
}
