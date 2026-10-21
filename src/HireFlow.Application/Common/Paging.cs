namespace HireFlow.Application.Common;

// Keeps page and pageSize inside safe limits so nobody can ask for 1 million rows.
public static class Paging
{
    public static int SafePage(int page) => page < 1 ? 1 : page;

    public static int SafePageSize(int pageSize) => pageSize < 1 ? 10 : Math.Min(pageSize, 100);
}
