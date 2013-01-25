using System.Linq;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;

namespace Contrib.NotEqualId.Filters {
    public interface IFilterProvider : IEventHandler
    {
        void Describe(dynamic describe);
    }

    [OrchardFeature("Contrib.NotEqualId")]
    public class NotEqualIdFilter : IFilterProvider
    {
        private readonly IContentManager _cms;
        private readonly IWorkContextAccessor _work;
        public Localizer T { get; set; }

        public NotEqualIdFilter(IContentManager cms, IWorkContextAccessor work) {
            _cms = cms;
            _work = work;

            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic describe)
        {
            Describe((DescribeFilterContext)describe);
        }

        private void Describe(DescribeFilterContext describe)
        {
            describe.For(
                "CommonPartRecord",
                T("Common Part Record"),
                T("Common Part Record"))

                    .Element(
                        "NotEqualId",
                        T("Not equal Id"),
                        T("Where the content item's id is unequal to the current one"),
                        ApplyFilter,
                        DisplayFilter
                );
        }

        private LocalizedString DisplayFilter(FilterContext context) {
            return T("Content with unequal Id");
        }

        private int TryGetCurrentContentId(int defaultIfNotFound = -1)
        {
            string alias = _work.GetContext().HttpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2);

            var content = _cms
                .Query<AutoroutePart, AutoroutePartRecord>(VersionOptions.Published)
                .Where(r => r.DisplayAlias == alias)
                .Slice(1).FirstOrDefault();

            if (content != null) return content.Id;

            return defaultIfNotFound;
        }

        private void ApplyFilter(FilterContext context) {
            var tryGetCurrentContentId = TryGetCurrentContentId();
            if (tryGetCurrentContentId != -1) {
                context.Query = context.Query.Where(factory => factory.ContentItem(), exp1 => exp1.Not(exp2 => exp2.Eq("Id", tryGetCurrentContentId)));
            }
        }
    }
}