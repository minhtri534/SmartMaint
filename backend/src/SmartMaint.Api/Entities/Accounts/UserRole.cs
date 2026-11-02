namespace SmartMaint.Api.Entities.Accounts;

/// <summary>
/// The 5 fixed roles (D1). ERD: Roles(ID, Name) — the value is the row ID of the roles table, the API sends the
/// snake_case name ("customer", "staff"...). Every account also has a row in the table of its role (Customers, Staff...).
/// </summary>
public enum UserRole
{
    Customer = 1,
    Staff = 2,
    Technician = 3,
    Manager = 4,
    Admin = 5,
}

/// <summary>
/// NOT IN ERD v5. Account state managed by the Administrator. A temporary lock after failed sign-ins is NOT a status;
/// it is tracked by <see cref="Account.LockedUntil"/> (BR-18).
/// </summary>
public enum UserStatus
{
    Active,
    Deactivated,
}
