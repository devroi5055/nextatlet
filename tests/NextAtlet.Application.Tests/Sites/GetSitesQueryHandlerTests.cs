using NextAtlet.Application.Abstractions.Persistence;
using NextAtlet.Application.Common.Models;
using NextAtlet.Application.Contracts.Sites.Request;
using NextAtlet.Application.Features.Sites;
using NextAtlet.Domain.Entities.Sites;
using NextAtlet.Domain.Enumerations.Individual;
using NextAtlet.Domain.Enumerations.Shared;

namespace NextAtlet.Application.Tests.Sites;

public class GetSitesQueryHandlerTests
{
    private readonly ISiteRepository _sites = Substitute.For<ISiteRepository>();

    private GetSitesQueryHandler BuildHandler() => new(_sites);

    private static Site ASite(string slug = "ada", string displayName = "Ada Athlete") => new()
    {
        Slug              = slug,
        DisplayName       = displayName,
        SiteTypeId        = SiteType.Individual.Id,
        DefaultLocaleId   = Locale.En.Id,
        VisibilityStateId = VisibilityStates.Public.Id,
    };

    [Fact]
    public async Task Handle_MapsEntitiesToResponses_AndCarriesPagingMetadata()
    {
        var filter = new SiteListRequest { Page = 2, PageSize = 10 };
        var site   = ASite();
        _sites.GetPagedAsync(filter, Arg.Any<CancellationToken>())
              .Returns(new PagedResult<Site>([site], Page: 2, PageSize: 10, TotalCount: 25));

        var result = await BuildHandler().Handle(new GetSitesQuery(filter), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var page = result.Value!;
        Assert.Equal(2,  page.Page);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(25, page.TotalCount);
        Assert.Equal(3,  page.TotalPages);   // ceil(25/10)
        Assert.True(page.HasNext);
        Assert.True(page.HasPrevious);

        var dto = Assert.Single(page.Items);
        Assert.Equal(site.Id, dto.Id);
        Assert.Equal("ada", dto.Slug);
        Assert.Equal("Ada Athlete", dto.DisplayName);
        Assert.Equal(Locale.En.Id, dto.DefaultLocale.Id);
        Assert.Equal(VisibilityStates.Public.Id, dto.VisibilityState.Id);
    }

    [Fact]
    public async Task Handle_EmptyPage_IsSuccessWithNoItems()
    {
        var filter = new SiteListRequest();
        _sites.GetPagedAsync(Arg.Any<SiteListRequest>(), Arg.Any<CancellationToken>())
              .Returns(new PagedResult<Site>([], filter.Page, filter.PageSize, 0));

        var result = await BuildHandler().Handle(new GetSitesQuery(filter), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value!.TotalCount);
        Assert.False(result.Value!.HasNext);
    }
}

/// <summary>The URL-bound paging is clamped so a hand-edited query string can't request an unbounded page.</summary>
public class PagedQueryTests
{
    [Fact]
    public void PageSize_AboveMax_IsClampedToMax()
        => Assert.Equal(PagedQuery.MaxPageSize, new SiteListRequest { PageSize = 10_000 }.PageSize);

    [Fact]
    public void PageSize_BelowOne_IsClampedToOne()
        => Assert.Equal(1, new SiteListRequest { PageSize = 0 }.PageSize);

    [Fact]
    public void Page_BelowOne_IsClampedToOne()
        => Assert.Equal(1, new SiteListRequest { Page = -5 }.Page);
}
