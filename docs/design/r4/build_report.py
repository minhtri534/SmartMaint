"""Builds Report 4 (Software Design Document) from the school's Word template.

Keeps the template's cover, table of contents, styles and table colours, and replaces the body of section II with the
SmartMaint content and the PNG diagrams in img/ (rendered by diagrams.mjs; erd-logical.png is Hùng's Logical ERD v5).
Usage (from the repo root): python docs/design/r4/build_report.py
"""
import json
import re
import sys
import zipfile
from pathlib import Path
from xml.sax.saxutils import escape

HERE = Path(__file__).parent
TEMPLATE = HERE / "template" / "Report4_template.docx"
IMG = HERE / "img"
OUT = HERE.parent.parent / "reports" / "Report4_Software_Design_Document_SmartMaint.docx"

PORTRAIT_W = 9360  # text width in dxa (8.5in - 2 x 1in)
LANDSCAPE_W = 12960  # 11in - 2 x 1in
EMU_PER_DXA = 635

# ----- XML helpers ---------------------------------------------------------------------------------------------------


def runs(text, base=""):
    """Plain text with **bold** spans."""
    out = []
    for i, part in enumerate(re.split(r"\*\*", text)):
        if not part:
            continue
        rpr = base + ("<w:b/>" if i % 2 else "")
        out.append(f'<w:r>{f"<w:rPr>{rpr}</w:rPr>" if rpr else ""}<w:t xml:space="preserve">{escape(part)}</w:t></w:r>')
    return "".join(out)


def heading(level, text):
    return f'<w:p><w:pPr><w:pStyle w:val="Heading{level}"/></w:pPr>{runs(text)}</w:p>'


def para(text, align="both"):
    return f'<w:p><w:pPr><w:jc w:val="{align}"/></w:pPr>{runs(text)}</w:p>'


def bullet(text):
    return f'<w:p><w:pPr><w:ind w:left="360" w:hanging="216"/><w:jc w:val="both"/></w:pPr>{runs("- " + text)}</w:p>'


def caption_title(text):
    """Bold italic line above a table, like "Package Descriptions" in the template."""
    rpr = "<w:b/><w:i/><w:sz w:val=\"24\"/>"
    return f'<w:p><w:pPr><w:spacing w:after="60" w:line="240" w:lineRule="auto"/></w:pPr><w:r><w:rPr>{rpr}</w:rPr><w:t xml:space="preserve">{escape(text)}</w:t></w:r></w:p>'


def figure_caption(text):
    return f'<w:p><w:pPr><w:jc w:val="center"/></w:pPr><w:r><w:rPr><w:i/><w:sz w:val="20"/></w:rPr><w:t xml:space="preserve">{escape(text)}</w:t></w:r></w:p>'


def page_break():
    return '<w:p><w:r><w:br w:type="page"/></w:r></w:p>'


HEAD_RPR = '<w:rFonts w:ascii="Calibri" w:eastAsia="Times New Roman" w:hAnsi="Calibri" w:cs="Calibri"/><w:b/><w:bCs/><w:color w:val="000000"/>'
BODY_PPR = '<w:pStyle w:val="comment"/><w:spacing w:before="20" w:after="20" w:line="240" w:lineRule="auto"/><w:ind w:left="0"/>'
BODY_RPR = '<w:i w:val="0"/><w:color w:val="000000"/><w:sz w:val="18"/><w:szCs w:val="18"/>'


def table(headers, rows, fractions, width):
    widths = [int(width * f) for f in fractions]
    widths[-1] = width - sum(widths[:-1])
    borders = "".join(f'<w:{b} w:val="single" w:sz="4" w:space="0" w:color="auto"/>' for b in ("top", "left", "bottom", "right", "insideH", "insideV"))
    out = [f'<w:tbl><w:tblPr><w:tblW w:w="{width}" w:type="dxa"/><w:tblBorders>{borders}</w:tblBorders><w:tblLayout w:type="fixed"/><w:tblLook w:val="0000"/></w:tblPr>']
    out.append("<w:tblGrid>" + "".join(f'<w:gridCol w:w="{w}"/>' for w in widths) + "</w:tblGrid>")
    cells = "".join(
        f'<w:tc><w:tcPr><w:tcW w:w="{w}" w:type="dxa"/><w:shd w:val="clear" w:color="auto" w:fill="FFE8E1"/><w:vAlign w:val="center"/></w:tcPr>'
        f'<w:p><w:pPr><w:spacing w:after="0" w:line="240" w:lineRule="auto"/><w:jc w:val="center"/></w:pPr><w:r><w:rPr>{HEAD_RPR}</w:rPr><w:t xml:space="preserve">{escape(h)}</w:t></w:r></w:p></w:tc>'
        for h, w in zip(headers, widths)
    )
    out.append(f'<w:tr><w:trPr><w:tblHeader/></w:trPr>{cells}</w:tr>')
    for row in rows:
        tcs = []
        for ci, (value, w) in enumerate(zip(row, widths)):
            lines = value if isinstance(value, list) else [value]
            jc = '<w:jc w:val="center"/>' if ci == 0 and len(headers) > 2 else ""
            ps = "".join(f"<w:p><w:pPr>{BODY_PPR}{jc}</w:pPr>{runs(line, BODY_RPR)}</w:p>" for line in lines)
            tcs.append(f'<w:tc><w:tcPr><w:tcW w:w="{w}" w:type="dxa"/></w:tcPr>{ps}</w:tc>')
        out.append("<w:tr>" + "".join(tcs) + "</w:tr>")
    out.append("</w:tbl>")
    return "".join(out) + "<w:p/>"


class Images:
    def __init__(self):
        self.items = []  # (rid, path)
        sizes = json.loads((IMG / "sizes.json").read_text(encoding="utf-8"))
        sizes["erd-logical"] = {"width": 2702, "height": 1551}
        self.sizes = sizes

    def figure(self, name, caption, width_dxa, max_height_in):
        n = len(self.items) + 1
        rid = f"rIdR4img{n}"
        self.items.append((rid, IMG / f"{name}.png"))
        size = self.sizes[name]
        cx = width_dxa * EMU_PER_DXA
        cy = int(cx * size["height"] / size["width"])
        max_cy = int(max_height_in * 914400)
        if cy > max_cy:
            cx = int(cx * max_cy / cy)
            cy = max_cy
        pid = 100 + n
        drawing = (
            f'<w:drawing><wp:inline distT="0" distB="0" distL="0" distR="0"><wp:extent cx="{cx}" cy="{cy}"/>'
            f'<wp:effectExtent l="0" t="0" r="0" b="0"/><wp:docPr id="{pid}" name="Picture {pid}"/>'
            '<wp:cNvGraphicFramePr><a:graphicFrameLocks xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" noChangeAspect="1"/></wp:cNvGraphicFramePr>'
            '<a:graphic xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"><a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture">'
            f'<pic:pic xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture"><pic:nvPicPr><pic:cNvPr id="{pid}" name="{name}.png"/><pic:cNvPicPr/></pic:nvPicPr>'
            f'<pic:blipFill><a:blip r:embed="{rid}"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill>'
            f'<pic:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm><a:prstGeom prst="rect"><a:avLst/></a:prstGeom></pic:spPr></pic:pic>'
            "</a:graphicData></a:graphic></wp:inline></w:drawing>"
        )
        return (
            f'<w:p><w:pPr><w:keepNext/><w:spacing w:before="120" w:after="60"/><w:jc w:val="center"/></w:pPr><w:r><w:rPr><w:noProof/></w:rPr>{drawing}</w:r></w:p>'
            + figure_caption(caption)
        )


# ----- content -------------------------------------------------------------------------------------------------------


def section_one(img, W):
    x = []
    x.append(heading(1, "II. Software Design Document"))
    x.append(heading(2, "1. System Design"))
    x.append(heading(3, "1.1 System Architecture"))
    x.append(para(
        "SmartMaint is a client-server system. The web application and the mobile application are thin clients: they "
        "show screens and call one REST API. All business rules (the case state machine, role-based access control, "
        "data scope per building, SLA deadlines) live in the backend, so every client behaves the same. The backend is "
        "one ASP.NET Core project split into layers. Authentication is our own JWT (Supabase Auth is not used)."))
    x.append(img.figure("architecture", "Figure 1: SmartMaint system architecture", W, 4.2))
    x.append(caption_title("Component Descriptions"))
    x.append(table(["No", "Component", "Description"], [
        ["01", "Users", "Five roles: Customer (tenant of an office tower), Facility Staff, Maintenance Technician, Facility Manager, Administrator. Accounts are created by the Administrator; there is no self sign-up."],
        ["02", "Web application", "React 19 + Vite single-page app written in TypeScript, hosted on Vercel. Server data is cached with TanStack Query; requests go through one axios client that adds the JWT and turns errors (ProblemDetails) into messages. Screens are grouped by feature (auth, cases, ...)."],
        ["03", "Mobile application", "Flutter app for the same five roles (full parity with the web). Built in a later package by the mobile owner; it calls the same REST API."],
        ["04", "SmartMaint.Api", "ASP.NET Core Web API on .NET 10, hosted on Railway. Layers: Controllers (HTTP endpoints, role check with [AuthorizeRoles]) → Modules (business logic per domain: Auth, Cases, Catalog, ...) → Data (EF Core AppDbContext) / Entities. Common holds what every layer uses: JWT and password hashing, the current user, errors as ProblemDetails, FluentValidation, paging."],
        ["05", "PostgreSQL 17", "Relational database. Supabase hosts it when deployed; a Docker container is used on developer machines. The schema is created by EF Core code-first migrations (database/Schema.sql) and seeded from database/Data.sql."],
        ["06", "Supabase Storage", "Object storage for case photos and attachments (planned). Until then photos are stored as image data in the case_images table."],
        ["07", "Replicate", "Hosted LLM API for the AI assistant (planned). Calls are asynchronous: the API creates a prediction and polls for the result. The AI only suggests and is limited to the data the asking role may see."],
    ], [0.07, 0.2, 0.73], W))

    x.append(heading(3, "1.2 Package Diagram"))
    x.append(para("**Backend (SmartMaint.Api).** The code is split by layer, and inside Modules by business domain. A controller only calls the service interface of its module; a service reads and writes through AppDbContext. Controllers never use AppDbContext directly."))
    x.append(img.figure("packages-backend", "Figure 2: Package diagram of the backend (SmartMaint.Api)", W, 4.6))
    x.append(caption_title("Package Descriptions"))
    x.append(table(["No", "Package", "Description"], [
        ["01", "Controllers", "One controller per module (AuthController, CasesController, CatalogController). Maps HTTP routes to service methods and declares which roles may call each endpoint with [AuthorizeRoles]. No business logic."],
        ["02", "Modules.Auth", "Sign in, current user, change password. AuthService checks the password, locks the account after 5 failed attempts and issues the JWT. Has its own DTOs, validators and error codes (auth.*)."],
        ["03", "Modules.Cases", "F3 normal incident handling. CaseService implements every step of a case; CaseTransitions is the state-machine table (who may do which action from which status); CaseMapper builds the API responses; CaseErrors holds the error codes (case.*)."],
        ["04", "Modules.Catalog", "Read-only reference data for the F3 forms: service types, buildings / floors / rooms, assets of a room, technicians. Will be replaced by the full F1 catalogue module."],
        ["05", "Modules (next packages)", "F1 building and asset catalogue, F2 preventive maintenance, F4 emergency, F5 repair or replace / third party, F6 monitoring and statistics, AI assistant. Each gets its own folder with the same structure."],
        ["06", "Data", "AppDbContext (EF Core + Npgsql), one configuration class per table (Configurations/), enum-as-text convention, timestamp interceptor (created_at, updated_at), migrations and the seed runner."],
        ["07", "Entities", "Plain C# classes mapped to tables: User; Building, Floor, Room, Asset, staff / manager building assignments; Case, CaseImage, CaseStatusChange, CaseAssignment, PriorityLevel, CaseCategory. Base classes Entity and AuditableEntity."],
        ["08", "Common", "Security (JwtTokenService, PasswordHasher, CurrentUser, AuthorizeRolesAttribute, PasswordChangeRequiredFilter), Errors (AppException, GlobalExceptionHandler, ProblemFactory), Validation (ValidationFilter runs the FluentValidation validator of each request), Models (PageRequest, PagedResult), Extensions and Configuration (service registration, .env loading)."],
    ], [0.07, 0.2, 0.73], W))
    x.append(para("**Web application (frontend/src).** Code is grouped by feature. A feature only uses shared/ and lib/, never another feature's internals, so each owner can work on a flow alone."))
    x.append(img.figure("packages-web", "Figure 3: Package diagram of the web application", W, 4.4))
    x.append(caption_title("Package Descriptions"))
    x.append(table(["No", "Package", "Description"], [
        ["01", "app", "Application shell: App.tsx (providers), router.tsx (routes and which roles may open them), navigation.ts (side menu per role), paths.ts (URL helpers)."],
        ["02", "features/auth", "Sign-in and change-password pages, AuthProvider (holds the signed-in user and token), RequireAuth (guards routes by role), API calls, hooks, zod schemas and types."],
        ["03", "features/cases", "F3 screens: My requests, New request (customer), Cases queue (staff, manager), My work orders (technician), Case detail for every role, with the action dialogs (approve, reject, assign, hold, complete, close, reopen, rate). The buttons shown come from allowedActions returned by the API."],
        ["04", "shared", "Reusable UI components (Button, Card, Dialog, Table, form fields, Icon, ...), the Role type and formatting helpers. No business logic."],
        ["05", "lib", "api-client.ts (axios instance with the JWT header and session-expired handling), api-error.ts (ProblemDetails → ApiError), token-storage.ts, query-client.ts (TanStack Query defaults), env.ts."],
    ], [0.07, 0.2, 0.73], W))
    x.append(para("**Mobile application.** The Flutter project (mobile/) is created in the next package; its package diagram will be added then."))
    return x


ERD_TABLES = [
    # (table, description, primary key, foreign keys)
    ("Account", "Sign-in account of every person: username, password (hashed), phone number and role.", "ID", "Role → Roles.ID"),
    ("Roles", "The five roles: Customer, Staff, Technician, Manager, Administrator.", "ID", "-"),
    ("Customers", "Tenant (office customer) who reports problems. One row per customer account.", "ID", "ID → Account.ID"),
    ("Staff", "Facility staff member who reviews, assigns and closes cases of the buildings assigned to them.", "ID", "ID → Account.ID"),
    ("Technicians", "Maintenance technician who does the work; stores the hourly rate.", "ID", "ID → Account.ID"),
    ("Manager", "Facility manager who supervises buildings and approves preventive maintenance plans.", "ID", "ID → Account.ID"),
    ("Admins", "Administrator who manages accounts and settings and reads the audit log.", "ID", "ID → Account.ID"),
    ("StaffBuildingAssignments", "Which staff member looks after which building; IsPrimary marks the main staff, the others are backups (a building can have several staff).", "Staff, Building", "Staff → Staff.ID; Building → Buildings.ID"),
    ("ManagerBuildingAssignments", "Which manager supervises which building.", "Staff, Building", "Staff → Manager.ID; Building → Buildings.ID"),
    ("Buildings", "Office tower managed by the company: name and address.", "ID", "-"),
    ("Floors", "Floor of a building.", "ID", "Building → Buildings.ID"),
    ("Rooms", "Room or area on a floor (office unit, toilet, lobby, ...).", "ID", "Floor → Floors.ID"),
    ("Assets", "Equipment installed in a room, with warranty end date and copyright flag (used by F5 repair or replace).", "ID", "RoomID → Rooms.ID; TypeID → AssetType.ID; ManufacturerID → Manufacturers.ID"),
    ("AssetType", "Type of asset (e.g. split air conditioner) inside an asset category.", "ID", "CategoryID → AssetCategory.ID"),
    ("AssetCategory", "Group of asset types (HVAC, electrical, plumbing, ...).", "ID", "-"),
    ("Manufacturers", "Manufacturer or supplier of assets, with contact phone and email.", "ID", "-"),
    ("Cases", "Central table of the system: one problem to handle (normal incident F3, emergency F4, or a work order created by preventive maintenance F2). Holds requester, asset, category, status, priority, staff and technician in charge, submitted / start / due / completed / outsourced times and estimated / actual hours.", "ID", "RequestingCustomer → Customers.ID; Asset → Assets.ID; Category → CaseCategories.ID; TriggerWorkOrder → MaintenanceWorkOrderDetails.ID; Status → CaseStatuses.ID; PriorityLevel → PriorityLevels.ID; Staff → Staff.ID; AssignedTechnician → Technicians.ID"),
    ("CaseStatuses", "The 11 case statuses: Pending, Approved, Assigned, Accepted, In Progress, On Hold, Completed, Closed, Reopened, Rejected, Cancelled.", "ID", "-"),
    ("CaseStatusHistory", "Every status change of a case with its time, so the timeline and SLA can be rebuilt.", "ID", "NewStatus → CaseStatuses.ID; Case → Cases.ID"),
    ("CaseCategories", "Service type chosen when reporting (e.g. Air conditioning - Not cooling).", "ID", "-"),
    ("PriorityLevels", "Urgency of a case (Emergency, High, Normal, Low); decides the SLA deadline.", "ID", "-"),
    ("CaseImages", "Photos attached to a case (when reporting, before and after the repair) with notes.", "ID", "Case → Cases.ID"),
    ("CaseTasks", "Tasks inside a case (from a checklist or added by the technician).", "ID", "Status → TaskStatuses.ID; Case → Cases.ID"),
    ("TaskStatuses", "Status of a case task (to do, done, ...).", "ID", "-"),
    ("TaskImages", "Photos of a case task with notes.", "ID", "Task → CaseTasks.ID"),
    ("TaskNotes", "Text notes written on a case task.", "ID", "Task → CaseTasks.ID"),
    ("TechnicianRejections", "A technician declining an assigned case: time and reason (the manager watches these).", "Technician, Case", "Technician → Technicians.ID; Case → Cases.ID"),
    ("Specialties", "Skill of technicians (electrical, plumbing, HVAC, ...).", "ID", "-"),
    ("TechnicianSpecialty", "Which technician has which specialty.", "Technician, Specialty", "Technician → Technicians.ID; Specialty → Specialties.ID"),
    ("Costs", "Money spent on a case (parts, outsourced service, ...): name, description, amount.", "ID", "Case → Cases.ID"),
    ("MaintenanceTriggers", "Preventive maintenance plan (F2): repeat frequency, staff owner, approving manager, status, last paused / triggered time.", "ID", "FrequencyType → FrequencyType.ID; Status → TriggerStatuses.ID"),
    ("FrequencyType", "Unit of the repeat frequency (day, week, month, ...).", "ID", "-"),
    ("TriggerStatuses", "Status of a maintenance plan (waiting for approval, active, paused, ...).", "ID", "-"),
    ("MaintenanceWorkOrderDetails", "Work order template of a plan: which checklist to run, estimated hours and priority.", "ID", "Trigger → MaintenanceTriggers.ID; Checklist → CaseChecklists.ID; PriorityLevel → PriorityLevels.ID"),
    ("MaintenanceWorkOrderAssets", "Assets covered by a work order and the technician working on each, with start and end time.", "WorkOrderDetail, Asset", "WorkOrderDetail → MaintenanceWorkOrderDetails.ID; Asset → Assets.ID; Technician → Technicians.ID"),
    ("CaseChecklists", "Reusable checklist (e.g. monthly AC check) for a category.", "ID", "Category → CaseCategories.ID"),
    ("ChecklistTasks", "Task lines of a checklist.", "ID", "Checklist → CaseChecklists.ID"),
    ("Notifications", "Message sent to a user (new case, assignment, deadline, ...) and when it was sent.", "ID", "UserID → Account.ID"),
    ("AIConversation", "Conversation of a user with the AI assistant, linked to the screen or record it was opened from (ContextType, ContextID).", "ID", "UserID → Account.ID"),
    ("AIMessage", "One question and the AI reply inside a conversation.", "ID", "ConversationID → AIConversation.ID"),
    ("AuditLog", "Record of an important change: who, which kind of record, which operation, old and new value, time.", "ID", "AdminID → Admins.ID; TargetType → AuditLogTargetType.ID; Operation → AuditLogTargetOperation.ID"),
    ("AuditLogTargetType", "Kind of record an audit entry is about (account, case, asset, ...).", "ID", "-"),
    ("AuditLogTargetOperation", "Operation of an audit entry (create, update, delete, sign in, ...).", "ID", "-"),
]


def section_two(img, W):
    x = []
    x.append(heading(2, "2. Database Design"))
    x.append(para(
        "Figure 4 is the logical ERD v5 of SmartMaint (owner: Hùng, version of 28/10/2026 on the team Drive). It covers all "
        "six flows: accounts and roles, buildings and assets (F1), preventive maintenance plans and work orders (F2), "
        "cases with their status history, tasks, photos and costs (F3, F4, F5), notifications, the AI assistant and the "
        "audit log (F6). The ERD is still being finalised by the team; the descriptions below follow it."))
    x.append(img.figure("erd-logical", "Figure 4: Logical ERD v5 (database/ERD_SmartMaint_v5.drawio)", W, 5.6))
    x.append(para(
        "**Physical database (current state).** Tables are created by EF Core code-first migrations on PostgreSQL 17 and "
        "follow these rules: table and column names in English snake_case; primary key bigint identity plus a display code "
        "built from it (e.g. CS-000123); times in timestamptz (UTC); enumerations stored as text with a CHECK constraint "
        "instead of lookup tables (CaseStatuses, Roles, ...); created_at and updated_at on every table. The tables built so "
        "far are users, buildings, floors, rooms, assets, staff_building_assignments, manager_building_assignments, "
        "priority_levels, case_categories, cases, case_images, case_status_history and case_assignments. The columns that "
        "F3 needs beyond the ERD (room of a case, hold reason and date, repair record, rating, assignment history) are "
        "listed in docs/database/F3_Cot_Bo_Sung_Cho_ERD.md for the team to agree on."))
    x.append(caption_title("Table Descriptions"))
    rows = [[f"{i:02d}", name, [desc, f"- Primary keys: {pk}", f"- Foreign keys: {fk}"]] for i, (name, desc, pk, fk) in enumerate(ERD_TABLES, 1)]
    x.append(table(["No", "Table", "Description"], rows, [0.06, 0.22, 0.72], W))
    return x


def section_auth(img, W, H):
    x = []
    x.append(heading(2, "3. Detailed Design"))
    x.append(para(
        "Each feature below shows the real classes of the code (backend) and the screens that call them (web). Every "
        "request follows the same path: page → API client → controller ([AuthorizeRoles], ValidationFilter) → service → "
        "AppDbContext → entities, and errors come back as ProblemDetails with a code such as auth.invalid_credentials. "
        "Features of the next packages (F1, F2, F4, F5, F6, AI) will be added with the same structure."))
    x.append(heading(3, "3.1 Authentication"))
    x.append(para(
        "Sign in with email and password, get the current user, and change the password (required at the first sign-in "
        "because accounts are created by the Administrator). After 5 wrong passwords the account is locked for 15 "
        "minutes. The token is a JWT carrying the user id and role; every other API reads it through ICurrentUser."))
    x.append(heading(4, "3.1.1 Class Diagram"))
    x.append(img.figure("auth-class", "Figure 5: Class diagram of Authentication", W, H))
    x.append(caption_title("Class Specifications"))
    x.append(table(["No", "Class", "Main methods", "Description"], [
        ["01", "AuthController", "Login, Me, ChangePassword", "REST endpoints /api/auth/login (anonymous), /api/auth/me, /api/auth/change-password. Only calls IAuthService."],
        ["02", "IAuthService / AuthService", "LoginAsync, GetCurrentUserAsync, ChangePasswordAsync", "Business logic of sign-in: finds the user by email, rejects deactivated or locked accounts, verifies the password, counts failures, issues the token."],
        ["03", "IPasswordHasher / PasswordHasher", "Hash, Verify", "Hashes passwords with ASP.NET Core Identity's PBKDF2 hasher; plain passwords are never stored."],
        ["04", "ITokenService / JwtTokenService", "CreateAccessToken", "Creates the signed JWT (user id, role, must-change-password flag) and its expiry time from JwtOptions."],
        ["05", "ICurrentUser / CurrentUser", "Id, Role", "Reads the signed-in user from the JWT claims of the current HTTP request."],
        ["06", "User", "IsLockedOut, RecordFailedLogin, RecordSuccessfulLogin, ChangePassword", "Entity of the users table. Keeps its own rules: lock after the maximum failed attempts, reset the counter on success, clear MustChangePassword after a change."],
        ["07", "AppDbContext", "Users, SaveChangesAsync", "EF Core unit of work over PostgreSQL."],
        ["08", "AuthErrors / AppException", "InvalidCredentials, AccountLocked, ...", "Error codes of the module. GlobalExceptionHandler turns an AppException into ProblemDetails with the right HTTP status (401, 403, 400, 404)."],
        ["09", "LoginRequest, ChangePasswordRequest, LoginResponse, CurrentUserResponse", "-", "Request and response records (DTOs) of the API."],
        ["10", "LoginRequestValidator, ChangePasswordRequestValidator", "-", "FluentValidation rules run by ValidationFilter before the action; failures return 400 with field errors."],
    ], [0.05, 0.25, 0.25, 0.45], W))
    x.append(heading(4, "3.1.2 Sign In"))
    x.append(img.figure("auth-seq-login", "Figure 6: Sequence diagram - Sign in", W, H))
    x.append(heading(4, "3.1.3 Change Password"))
    x.append(para("The new password is checked by ChangePasswordRequestValidator in ValidationFilter in the same way as in Figure 6 (not repeated). Until the password is changed, PasswordChangeRequiredFilter blocks every other endpoint for an account with MustChangePassword."))
    x.append(img.figure("auth-seq-change-password", "Figure 7: Sequence diagram - Change password", W, H))
    return x


def section_f3(img, W, H):
    x = []
    x.append(heading(3, "3.2 F3 - Normal Incident Handling"))
    x.append(para(
        "A customer reports a problem; a staff member of that building reviews it (approve with urgency and cost bearer, "
        "or reject with a reason, e.g. a duplicate) and assigns a technician; the technician accepts, starts work (the SLA "
        "deadline starts here: start time + hours of the urgency), may put it on hold with a reason and an expected date, "
        "and reports it completed with the cause, parts, hours and photos; the staff member verifies and closes it, or "
        "reopens it within 7 days; the customer rates the service. Emergency cases are handed over to F4."))
    x.append(para(
        "Every role sees only its own data: a customer its own cases, staff and managers the cases of their buildings, a "
        "technician the cases assigned to them; the Administrator sees none. The API returns allowedActions with every "
        "case, so the web only draws the buttons the server allows."))
    x.append(heading(4, "3.2.1 Class Diagram"))
    x.append(img.figure("f3-class-services", "Figure 8: Class diagram of F3 - controllers, services and DTOs", W, H))
    x.append(img.figure("f3-class-entities", "Figure 9: Class diagram of F3 - entities", W, H))
    x.append(caption_title("Class Specifications"))
    x.append(table(["No", "Class", "Main methods", "Description"], [
        ["01", "CasesController", "List, Get, Create, Cancel, Approve, Reject, Assign, Accept, Decline, Start, Hold, Resume, Complete, Close, Reopen, Rate", "REST endpoints under /api/cases. Each action declares its roles with [AuthorizeRoles] (e.g. Approve: Staff; Accept: Technician; Rate: Customer)."],
        ["02", "ICaseService / CaseService", "ListAsync, GetAsync, CreateAsync and one method per action; ActAsync, IsAllowed, LoadScopeAsync, Visible", "All F3 logic. Every status change goes through ActAsync: load the case inside the caller's data scope, check the action is allowed, apply the change, set the new status, add one history row, save."],
        ["03", "CaseTransitions / TransitionRule", "Find(action, role, status)", "The state machine as a table: which role may do which action from which statuses and the status it leads to."],
        ["04", "CaseMapper", "ToSummary, ToDetail", "Builds CaseSummaryResponse (lists) and CaseDetailResponse (detail page, with allowedActions)."],
        ["05", "CaseErrors", "NotFound, InvalidTransition, PhotoRequired, ...", "Error codes case.* (404, 409, 400)."],
        ["06", "Request records and validators", "CreateCaseRequestValidator, CompleteWorkRequestValidator, ...", "One record and one FluentValidation validator per action (create, approve, reject, assign, reason, hold, complete, rate)."],
        ["07", "CatalogController / CatalogService", "ServiceTypes, Buildings, Assets, Technicians", "Reference data for the forms; technicians are listed least busy first for the assign dialog."],
        ["08", "Case", "CodeOf(id)", "Entity of the cases table; owns its images, status history and assignments (composition)."],
        ["09", "CaseImage, CaseStatusChange, CaseAssignment", "-", "Photos (report / before / after), every status change with who and why, every assignment with the technician's answer (waiting / accepted / declined)."],
        ["10", "PriorityLevel, CaseCategory, Room, Floor, Building, Asset", "-", "Reference entities: SLA hours per urgency, service types (emergency flag, photo required), location of the case."],
    ], [0.05, 0.25, 0.27, 0.43], W))

    x.append(heading(4, "3.2.2 View Case List"))
    x.append(para("Shown for a staff member; for the other roles only the Visible(...) filter differs (customer: own cases; technician: cases assigned now or before; manager: buildings in ManagerBuildingAssignments)."))
    x.append(img.figure("f3-seq-list", "Figure 10: Sequence diagram - View case list", W, H))
    x.append(heading(4, "3.2.3 Submit Case"))
    x.append(img.figure("f3-seq-submit", "Figure 11: Sequence diagram - Submit case (customer)", W, H))
    x.append(heading(4, "3.2.4 Approve Case"))
    x.append(para("This is the common structure of every status change: ActAsync loads the case inside the caller's scope, IsAllowed checks the state machine (CaseTransitions), the role and the emergency rule, then the action-specific part runs and one history row is written. The other actions follow Figure 12; only the action-specific part differs, as listed in the table after the figure."))
    x.append(img.figure("f3-seq-approve", "Figure 12: Sequence diagram - Approve case (staff), common structure of every status change", W, H))
    x.append(caption_title("Other status changes (same structure as Figure 12)"))
    x.append(table(["No", "Action / endpoint", "Role", "From → To", "Action-specific part"], [
        ["01", "Reject  POST /cases/{id}/reject", "Staff", "Pending → Rejected", "Reason (missing info, out of scope, duplicate) + note saved in the history; StaffId set."],
        ["02", "Cancel  POST /cases/{id}/cancel", "Customer, Staff", "Pending (customer); any open status (staff) → Cancelled", "Reason saved in the history."],
        ["03", "Accept  POST /cases/{id}/accept", "Technician", "Assigned, Reopened → Accepted", "The waiting CaseAssignment becomes Accepted (409 case.no_open_assignment if none)."],
        ["04", "Decline  POST /cases/{id}/decline", "Technician", "Assigned, Reopened → Approved", "Assignment becomes Declined with the reason; the technician is removed so staff can assign someone else."],
        ["05", "Start  POST /cases/{id}/start", "Technician", "Accepted → In Progress", "StartedAt = now; DueAt = now + SLA hours of the urgency (the SLA starts here)."],
        ["06", "Hold  POST /cases/{id}/hold", "Technician", "In Progress → On Hold", "Hold reason and expected resume date; reason 'waiting for third party' sets OutsourcedAt (hand-over to F5)."],
        ["07", "Resume  POST /cases/{id}/resume", "Technician, Staff", "On Hold → In Progress", "Hold reason and date cleared."],
        ["08", "Close  POST /cases/{id}/close", "Staff", "Completed → Closed", "ClosedAt = now (staff verified the repair record and photos)."],
        ["09", "Reopen  POST /cases/{id}/reopen", "Staff", "Completed, Closed (within 7 days) → Reopened", "Same technician gets a new waiting assignment; start, due, completed and closed times are cleared (the SLA starts again)."],
        ["10", "Rate  POST /cases/{id}/rating", "Customer", "Closed (no status change)", "1-5 stars and a comment; only once per case."],
    ], [0.05, 0.25, 0.13, 0.22, 0.35], W))
    x.append(heading(4, "3.2.5 Assign Technician"))
    x.append(img.figure("f3-seq-assign", "Figure 13: Sequence diagram - Assign technician (staff)", W, H))
    x.append(heading(4, "3.2.6 Report Completed Work"))
    x.append(para("Before this step the technician accepted the job and pressed Start (Figure 12 structure, rows 03 and 05 of the table). Completed is not Closed: the staff member still has to verify."))
    x.append(img.figure("f3-seq-complete", "Figure 14: Sequence diagram - Report completed work (technician)", W, H))
    return x


# ----- assemble ------------------------------------------------------------------------------------------------------


def main():
    global OUT
    if len(sys.argv) > 1:
        OUT = Path(sys.argv[1])
    zin = zipfile.ZipFile(TEMPLATE)
    doc = zin.read("word/document.xml").decode("utf-8")
    head, body_tail = doc.split("<w:body>", 1)
    body = body_tail.rsplit("<w:sectPr", 1)[0]
    pieces = [m.group(0) for m in re.finditer(r"<w:p[ >].*?</w:p>|<w:tbl>.*?</w:tbl>", body, re.S)]

    # Cover: project name, group and date.
    cover = pieces[:12]
    title_ppr = '<w:pPr><w:jc w:val="center"/></w:pPr>'
    cover[4] = f'<w:p>{title_ppr}<w:r><w:rPr><w:b/><w:sz w:val="32"/></w:rPr><w:t>SmartMaint - Smart Corporate Facility Maintenance Management Platform</w:t></w:r></w:p>'
    cover[5] = f'<w:p>{title_ppr}<w:r><w:rPr><w:sz w:val="28"/></w:rPr><w:t>Group FA26SE147</w:t></w:r></w:p>'
    cover[10] = re.sub(r"Hanoi, August 2019", "Hanoi, October 2026", cover[10])

    # Record of changes: fill the first empty row.
    roc = pieces[25]
    rows = re.findall(r"<w:tr[ >].*?</w:tr>", roc, re.S)
    values = iter(["28/10/2026", "A", "Quách Minh Trí", "First version: system architecture, package diagrams (backend, web), logical ERD v5 and table descriptions, detailed design of Authentication and F3 Normal incident handling."])
    filled = re.sub(r'(<w:p [^>]*><w:pPr><w:pStyle w:val="Bang"/></w:pPr>)(</w:p>)', lambda m: m.group(1) + runs(next(values)) + m.group(2), rows[1])
    roc = roc.replace(rows[1], filled, 1)

    img = Images()
    portrait = '<w:sectPr><w:pgSz w:w="12240" w:h="15840"/><w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440" w:header="720" w:footer="720" w:gutter="0"/><w:cols w:space="720"/><w:docGrid w:linePitch="360"/></w:sectPr>'
    landscape = '<w:sectPr><w:pgSz w:w="15840" w:h="12240" w:orient="landscape"/><w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440" w:header="720" w:footer="720" w:gutter="0"/><w:cols w:space="720"/><w:docGrid w:linePitch="360"/></w:sectPr>'

    content = []
    content += section_one(img, PORTRAIT_W)
    # Section 2 and 3 hold wide diagrams: landscape pages from here on.
    content.append(f"<w:p><w:pPr>{portrait}</w:pPr></w:p>")
    content += section_two(img, LANDSCAPE_W)
    content.append(page_break())
    content += section_auth(img, LANDSCAPE_W, 5.5)
    content.append(page_break())
    content += section_f3(img, LANDSCAPE_W, 5.5)

    new_body = "".join(cover + pieces[12:25] + [roc] + pieces[26:28] + content) + landscape
    doc_out = head + "<w:body>" + new_body + "</w:body></w:document>"

    rels = zin.read("word/_rels/document.xml.rels").decode("utf-8")
    extra = "".join(f'<Relationship Id="{rid}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="media/r4_{p.name}"/>' for rid, p in img.items)
    rels = rels.replace("</Relationships>", extra + "</Relationships>")
    settings = zin.read("word/settings.xml").decode("utf-8")
    if "<w:updateFields" not in settings:
        settings = re.sub(r"(<w:zoom [^>]*/>)", r'\1<w:updateFields w:val="true"/>', settings, count=1)

    OUT.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED) as zout:
        for item in zin.infolist():
            if item.filename == "word/document.xml":
                zout.writestr(item, doc_out)
            elif item.filename == "word/_rels/document.xml.rels":
                zout.writestr(item, rels)
            elif item.filename == "word/settings.xml":
                zout.writestr(item, settings)
            else:
                zout.writestr(item, zin.read(item.filename))
        for _, p in img.items:
            zout.write(p, f"word/media/r4_{p.name}")
    print(f"{OUT}  ({len(img.items)} figures)")


if __name__ == "__main__":
    main()
