using CosmicWiki.Caching;
using CosmicWiki.Pagination;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.Products.Common;
using CosmicWiki.Product.Application.Features.Products.GetProducts;
using CosmicWiki.Product.Domain.Enums;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;

namespace CosmicWiki.Product.UnitTests.Features.Products.GetProducts;

public class GetProductsQueryHandlerTests
{
    private readonly IProductRepository _productRepositoryMock;
    private readonly ICacheStore _cacheMock;
    private readonly IProductCache _productCacheMock;
    private readonly IOptions<CachingOptions> _cachingOptionsMock;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _productRepositoryMock = Substitute.For<IProductRepository>();
        _cacheMock = Substitute.For<ICacheStore>();
        _productCacheMock = Substitute.For<IProductCache>();
        _cachingOptionsMock = Substitute.For<IOptions<CachingOptions>>();
        
        _cachingOptionsMock.Value.Returns(new CachingOptions
        {
            ProductDetailTtlSeconds = 60,
            ProductListTtlSeconds = 60
        });

        _handler = new GetProductsQueryHandler(
            _productRepositoryMock,
            _cacheMock,
            _productCacheMock,
            _cachingOptionsMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedList_WhenListExistsInCache()
    {
        // Arrange
        var query = new GetProductsQuery(1, 10);
        var expectedCacheKey = "product:list:v1:1:10";
        var cachedResult = new PagedResult<ProductResponse>(new List<ProductResponse>(), 1, 10, 0);

        _productCacheMock.GetListKeyAsync(query.Page, query.PageSize, Arg.Any<CancellationToken>())
            .Returns(expectedCacheKey);

        _cacheMock.GetAsync<PagedResult<ProductResponse>>(expectedCacheKey, Arg.Any<CancellationToken>())
            .Returns(cachedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedResult.Page, result.Page);
        
        await _productRepositoryMock.DidNotReceiveWithAnyArgs()
            .CountAsync(default);
            
        await _productRepositoryMock.DidNotReceiveWithAnyArgs()
            .GetPageAsync(default, default, default);
        
        await _cacheMock.DidNotReceiveWithAnyArgs()
            .SetAsync<PagedResult<ProductResponse>>(default!, default!, default, default);
    }

    [Fact]
    public async Task Handle_ShouldFetchFromDbAndCache_WhenNotInCache()
    {
        // Arrange
        var query = new GetProductsQuery(1, 10);
        var expectedCacheKey = "product:list:v1:1:10";
        var categoryId = Guid.NewGuid();
        var product = ProductEntity.Create("SKU1", "Name1", "slug-1", 100, 90, 10, categoryId, "Desc", ProductStatus.Active);
        var productList = new List<ProductEntity> { product };
        
        _productCacheMock.GetListKeyAsync(query.Page, query.PageSize, Arg.Any<CancellationToken>())
            .Returns(expectedCacheKey);

        _cacheMock.GetAsync<PagedResult<ProductResponse>>(expectedCacheKey, Arg.Any<CancellationToken>())
            .Returns((PagedResult<ProductResponse>?)null);

        _productRepositoryMock.CountAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        _productRepositoryMock.GetPageAsync(0, 10, Arg.Any<CancellationToken>())
            .Returns(productList);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(product.Id, result.Items.First().Id);
        Assert.Equal(1, result.TotalCount);
        
        await _productRepositoryMock.Received(1)
            .CountAsync(Arg.Any<CancellationToken>());
            
        await _productRepositoryMock.Received(1)
            .GetPageAsync(0, 10, Arg.Any<CancellationToken>());
        
        await _cacheMock.Received(1)
            .SetAsync(
                expectedCacheKey,
                Arg.Is<PagedResult<ProductResponse>>(r => r.TotalCount == 1),
                TimeSpan.FromSeconds(60),
                Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public async Task Handle_ShouldThrowArgumentException_WhenPageIsLessThanOne(int page, int pageSize)
    {
        // Arrange
        var query = new GetProductsQuery(page, pageSize);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
