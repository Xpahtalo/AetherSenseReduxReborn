using System.Collections.Generic;

namespace AethersenseReduxReborn.Filters;

public class FilterList
{
    private readonly List<IFilter> _filters = new();

    public IEnumerable<IFilter> Filters => _filters;


    public FilterList AddFilter(IFilter filter)
    {
        _filters.Add(filter);
        return this;
    }
}
