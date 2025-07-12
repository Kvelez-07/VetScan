namespace VetScan.ViewModels
{
    public class UserRoleListViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int UserCount { get; set; }

        public string StatusClass => IsActive ? "status-active" : "status-inactive";
        public string StatusText => IsActive ? "Activo" : "Inactivo";
    }
}