﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Composite.Core.Routing;
using Composite.Core.Types;
using Composite.Data;

namespace Composite.Functions
{
    internal class AttributeBasedRoutedDataUrlMapper : IRoutedDataUrlMapper
    {
        private readonly Guid _pageId;
        private readonly Type _dataType;
        private static readonly MethodInfo _getFilteredDataMethodInfo;
        private readonly IRelativeRouteToPredicateMapper _mapper;

        static AttributeBasedRoutedDataUrlMapper()
        {
            _getFilteredDataMethodInfo = StaticReflection.GetGenericMethodInfo(() => GetFilteredData<IData>(null));
        }

        protected AttributeBasedRoutedDataUrlMapper(Type dataType, Guid pageId, IRelativeRouteToPredicateMapper mapper)
        {
            _pageId = pageId;
            _mapper = mapper;
            _dataType = dataType;
        }



        public static AttributeBasedRoutedDataUrlMapper GetDataUrlMapper(Type dataType, Guid pageId)
        {
            var mapper = AttributeBasedRoutingHelper.GetPredicateMapper(dataType);
            if (mapper == null)
            {
                return null;
            }

            return new AttributeBasedRoutedDataUrlMapper(dataType, pageId, mapper);
        }

        public RoutedDataModel GetRouteDataModel(PageUrlData pageUrlData)
        {
            if (pageUrlData.PageId != _pageId) return null;

            string pathInfo = pageUrlData.PathInfo;
            bool pathIsEmpty = string.IsNullOrEmpty(pathInfo);

            if (pathIsEmpty && !pageUrlData.HasQueryParameters)
            {
                return new RoutedDataModel(GetDataQueryable);
            }

            int totalSegments = _mapper.PathSegmentsCount;

            if (pathIsEmpty != (totalSegments == 0))
            {
                return null;
            }

            string[] segments = !pathIsEmpty
                ? pathInfo.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)
                : new string[0];

            if (segments.Length != totalSegments)
            {
                return null;
            }

            var relativeRoute = new RelativeRoute
            {
                PathSegments = segments,
                QueryString = pageUrlData.QueryParameters
            };

            var filterExpression = GetPredicate(_pageId, relativeRoute);
            if (filterExpression == null)
            {
                return null;
            }

            var dataSet = (IReadOnlyCollection<IData>) _getFilteredDataMethodInfo.MakeGenericMethod(_dataType)
                .Invoke(null, new object[] {filterExpression});

            if (dataSet.Count == 0 || dataSet.Count > 1)
            {
                return null;
            }

            return new RoutedDataModel(dataSet.First());
        }

        private IQueryable GetDataQueryable()
        {
            if (typeof (IPageRelatedData).IsAssignableFrom(_dataType))
            {
                var method = StaticReflection.GetGenericMethodInfo(a => GetFilteredDataQueryable<IPageRelatedData>(Guid.Empty));

                return (IQueryable) method.MakeGenericMethod(_dataType).Invoke(null, new object[] { _pageId });
            }

            return DataFacade.GetData(_dataType);
        }

        private static IQueryable GetFilteredDataQueryable<TDataType>(Guid pageId) where TDataType: class, IPageRelatedData
        {
            return DataFacade.GetData<TDataType>().Where(data => data.PageId == pageId);
        }

        private static IReadOnlyCollection<IData> GetFilteredData<T>(Expression<Func<T, bool>> lambdaExpression)
            where T : class, IData
        {
            return DataFacade.GetData<T>().ToList().AsQueryable().Where(lambdaExpression).Take(2).ToList();
        }

        public PageUrlData BuildItemUrl(IData item)
        {
            var page = PageManager.GetPageById(_pageId);
            if (page == null)
            {
                return null;
            }

            var route = GetRoute(item);
            if (route == null)
            {
                return null;
            }

            string pathInfo = route.PathSegments.Any() ? "/" + string.Join("/", route.PathSegments) : null;

            if (pathInfo == null && (route.QueryString == null || route.QueryString.Count == 0))
            {
                return null;
            }

            return new PageUrlData(page)
            {
                PathInfo = pathInfo,
                QueryParameters = route.QueryString
            };
        }

        private RelativeRoute GetRoute(IData data)
        {
            var @interface = typeof(IRelativeRouteToPredicateMapper<>).MakeGenericType(_dataType);
            return @interface.GetMethod("GetRoute").Invoke(_mapper, new[] { data }) as RelativeRoute;
        }


        private Expression GetPredicate(Guid pageId, RelativeRoute relativeRoute)
        {
            var @interface = typeof (IRelativeRouteToPredicateMapper<>).MakeGenericType(_dataType);
            return @interface.GetMethod("GetPredicate").Invoke(_mapper, new object[] {pageId, relativeRoute}) as Expression;
        }
    }
}