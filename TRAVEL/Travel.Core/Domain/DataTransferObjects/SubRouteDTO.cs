namespace Travel.Core.Domain.DataTransferObjects
{
    public class SubRouteDTO
    {
        public int Id { get; set; }
        public int? NameId { get; set; }
        public string Name { get; set; }

        public int RouteId { get; set; }
        public string RouteName { get; set; }
    }

    public class SubRouteViewModel
    {
        public int? SubRouteId { get; set; }
        public int? SubRouteNameId { get; set; }
        public string SubRouteName { get; set; }
        public int RouteId { get; set; }

        public string RouteName { get; set; }
    }
}