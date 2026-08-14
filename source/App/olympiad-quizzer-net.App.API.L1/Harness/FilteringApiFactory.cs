namespace OlympiadQuizzer.App.Api.L1.Harness;

public sealed class FilteringApiFactory : ApiFactory
{
    public FilteringApiFactory() : base(FixturePath.Resolve("filtering-bank.json")) { }
}
