

![][image1]

**CAPSTONE PROJECT REPORT**

**Report 3 – Software Requirement Specification**

**Smart Corporate Facility Maintenance Management Platform**

Project Code: SE147 | Group Name: SmartMaint

\- Ho Chi Minh City, September 2026 \-

**Table of Contents**

[I](#_toc9001)[. Record of Changes	4](#i.-record-of-changes)

[I](#_toc9002)[I. Software Requirement Specification	5](#ii.-software-requirement-specification)

[1](#_toc9003)[. Product Overview	5](#1.-product-overview)

[2](#_toc9004)[. User Requirements	6](#2.-user-requirements)

[2](#_toc9005)[.1 Actors	6](#2.1-actors)

[2](#_toc9006)[.2 Use Cases	6](#2.2-use-cases)

[3](#_toc9007)[. Functional Requirements	16](#3.-functional-requirements)

[3](#_toc9008)[.1 System Functional Overview	16](#3.1-system-functional-overview)

[3](#_toc9009)[.2 Sign-in and Account Recovery	31](#3.2-sign-in-and-account-recovery)

[3](#_toc9010)[.3 Application Shell and System Pages	33](#3.3-application-shell-and-system-pages)

[3](#_toc9011)[.4 Personal Account	35](#3.4-personal-account)

[3](#_toc9012)[.5 Search and AI Assistant	36](#3.5-search-and-ai-assistant)

[3](#_toc9013)[.6 Maintenance Request Submission (Employee)	38](#3.6-maintenance-request-submission-\(employee\))

[3](#_toc9014)[.7 Case Detail (shared)	41](#3.7-case-detail-\(shared\))

[3](#_toc9015)[.8 Case Queue and Coordination (Officer)	46](#3.8-case-queue-and-coordination-\(officer\))

[3](#_toc9016)[.9 Work Order Execution (Technician)	49](#3.9-work-order-execution-\(technician\))

[3](#_toc9017)[.10 Facility Manager Dashboard	51](#3.10-facility-manager-dashboard)

[3](#_toc9018)[.11 Analytics and Reporting	52](#3.11-analytics-and-reporting)

[3](#_toc9019)[.12 User and Access Administration	54](#3.12-user-and-access-administration)

[3](#_toc9020)[.13 System Configuration and Audit	57](#3.13-system-configuration-and-audit)

[3](#_toc9021)[.14 Facility Structure and Asset Directory	59](#3.14-facility-structure-and-asset-directory)

[3](#_toc9022)[.15 Asset Lifecycle Management	61](#3.15-asset-lifecycle-management)

[3](#_toc9023)[.16 Preventive Plan Management	63](#3.16-preventive-plan-management)

[3](#_toc9024)[.17 Shared UI Utilities	66](#3.17-shared-ui-utilities)

[3](#_toc9025)[.18 Non-Screen Functions	67](#3.18-non-screen-functions)

[4](#_toc9026)[. Non-Functional Requirements	69](#4.-non-functional-requirements)

[4](#_toc9027)[.1 External Interfaces	69](#4.1-external-interfaces)

[4](#_toc9028)[.2 Quality Attributes	70](#4.2-quality-attributes)

[5](#_toc9029)[. Requirement Appendix	71](#5.-requirement-appendix)

[5](#_toc9030)[.1 Business Rules	72](#5.1-business-rules)

[5](#_toc9031)[.2 Common Requirements	73](#5.2-common-requirements)

[5](#_toc9032)[.3 Application Messages List	74](#5.3-application-messages-list)

[5](#_toc9033)[.4 Other Requirements	75](#5.4-other-requirements)

# **I. Record of Changes** {#i.-record-of-changes}

| Date | A\*/M/D | In charge | Change Description |
| ----- | ----- | ----- | ----- |
| 24/09/2026 | A | Quach Minh Tri | Product Overview and Context Diagram, Actors, Facility Officer use cases, Non-Screen Functions, Entity Relationship Diagram, Business Rules. |
| 25/09/2026 | A | Nguyen Quang Tung | Use case diagrams and descriptions for Employee and Maintenance Technician (including common use cases). |
| 25/09/2026 | A | Nguyen Hoang Huy | Screen Descriptions \+ Screen Authorization  |
| 27/09/2026 | A | Nguyen Vo Manh Hung | Use case diagrams and descriptions for Facility Manager and Administrator. |
| 28/09/2026 | M | Quach Minh Tri | Merged all parts into one SRS. ERD: added entity Floor (Building \- Floor \- Room). Screen Descriptions and Screen Authorization regenerated from the 81-screen catalog (MGR-02 merged into OFF-03). Use cases reviewed against the catalog and business rules. Added Screens Flow, function details (3.2 to 3.18), Non-Functional Requirements, Common Requirements, Application Messages and Case life cycle. |
| 29/09/2026 | M | Quach Minh Tri | Consistency review with Report 1, Report 2 and the Screen Catalog: added mobile scope (Flutter, same functions for all five roles); ERD extended to 17 entities (IssueType, RootCause, SystemSetting; PreventivePlan M \- M Asset); Administrator sees only metadata of AI events; aligned Case Detail actions, cancel window, notifications and status transitions (5.4.1) with the catalog; fixed section references, general use case references and messages MSG23 to MSG25. |
| 30/9/2026 | M | Nguyen Vo Manh Hung | Updated use case diagrams for overall, manager and admin |

\*A \- Added M \- Modified D \- Deleted

# **II. Software Requirement Specification** {#ii.-software-requirement-specification}

## **1\. Product Overview** {#1.-product-overview}

SmartMaint (Smart Corporate Facility Maintenance Management Platform) is a new web and mobile system for companies that run office buildings with many assets such as meeting rooms, air conditioners, lights, elevators, printers and network devices. Today, facility problems are reported by phone, chat or a simple helpdesk that only records requests. The current way does not help to plan maintenance, to follow the repair work, or to make decisions based on data (which assets fail most often, which technician is effective, whether to repair or replace).

SmartMaint manages the full life cycle of a Maintenance Case: an Employee reports a problem, a Facility Officer reviews it and assigns a Maintenance Technician, the Technician repairs it on site and records the evidence, and the Officer verifies the result and closes the case. Around this core the system keeps asset and facility records (building, floor, room, asset, QR label), runs preventive maintenance plans, gives the Facility Manager dashboards and reports, and lets the Administrator manage accounts and settings. An AI Assistant, provided by the external AI service Replicate, answers questions in every role; its answers are limited to the data that the role is already allowed to see (RBAC-scoped), and it works asynchronously (a prediction is created first and its result is received later by polling or webhook).

The context diagram below shows the boundary of SmartMaint for release 1.0: the five user roles and the external AI service, and the data flows between each of them and the system.

![][image2]

*Figure 1\. Context Diagram \- SmartMaint*

## **2\. User Requirements** {#2.-user-requirements}

### **2.1 Actors** {#2.1-actors}

The system has five human actors. The external AI service (Replicate) is not an actor of any use case; it is described in the Context Diagram and in 4.1 External Interfaces.

| \# | Actor | Description |
| ----- | :---- | :---- |
| 1 | Employee | Any staff member working in the buildings. Reports facility problems with photos, follows the progress of their own requests, answers questions from the Facility Officer, rates the service after closing and can report "Not fixed" within 7 days after a case is closed. |
| 2 | Facility Officer | Coordinates maintenance work in the system (does not repair equipment). Reviews new requests (approve, reject, ask for more information), assigns and reassigns Maintenance Technicians, follows progress, verifies completed work and closes or reopens cases. Also manages preventive maintenance plans, changes asset status and prints QR labels. |
| 3 | Maintenance Technician | Carries out repairs and inspections on site. Accepts or rejects assigned work orders, updates progress, records root cause, spare parts, labor hours and photo evidence, and completes preventive inspection checklists. |
| 4 | Facility Manager | Monitors maintenance performance of all buildings through dashboards, KPIs, technician performance and failure trends, and exports reports. Read only: does not approve, assign or close work. |
| 5 | Administrator | Creates and manages user accounts and roles, the facility tree (building, floor, room), assets and QR labels, maintenance categories and SLA/rule settings, and reads the Audit Log. Cannot open the content of cases. |

### **2.2 Use Cases** {#2.2-use-cases}

Use cases shared by all five roles are listed once with the prefix UC-COM; they appear again in each role diagram so that every diagram is complete. Role-specific use cases use the prefixes UC-EMP, UC-OFF, UC-TEC, UC-MGR and UC-ADM. An extending use case («extend») is started from the detail screen of the use case it extends. The overall use case diagram (Figure 2\) groups the main functions of each role into general use cases with the prefix UC-GEN; the diagram shows only the main functions; each general use case lists in brackets the role use cases that detail it, and the remaining role use cases appear only in the role diagrams (Figures 3 to 8).

#### ***2.2.1 Diagram(s)***

![][image3] 

*Figure 2\. Use Case Diagram \- Overall*

![][image4]

*Figure 3\. Use Case Diagram \- Employee*

![][image5]

*Figure 4\. Use Case Diagram \- Facility Officer (case handling)*

![][image6]

*Figure 5\. Use Case Diagram \- Facility Officer (assets, preventive maintenance, analytics)*

![][image7]

*Figure 6\. Use Case Diagram \- Maintenance Technician*

![][image8]   

*Figure 7\. Use Case Diagram \- Facility Manager*

  ![][image9]

*Figure 8\. Use Case Diagram \- Administrator*

#### ***2.2.2 Descriptions***

| ID | Use Case | Actors | Use Case Description |
| :---: | :---- | :---- | :---- |
| UC-GEN-01 | Create Maintenance Request | Employee | Report a facility problem with location, issue type, description and photos (UC-EMP-01). |
| UC-GEN-02 | Track Maintenance Requests | Employee | Follow the status and details of own requests and act on them: add information, cancel, rate or report "Not fixed" (UC-EMP-02 to UC-EMP-07). |
| UC-GEN-03 | Process Maintenance Request | Facility Officer | Claim a request from the queue and review it: approve, reject, ask for more information, cancel, verify and close, or handle a reopen request; also create a case on behalf of an Employee (UC-OFF-01 to UC-OFF-05, UC-OFF-08 to UC-OFF-10, UC-OFF-12). |
| UC-GEN-04 | Assign Technician | Facility Officer | Extends UC-GEN-03. Assign or reassign a Maintenance Technician to an approved case (UC-OFF-06, UC-OFF-07). |
| UC-GEN-05 | Track Case Progress | Facility Officer | View all cases in every status and filter them by status, due date or Technician (UC-OFF-11). |
| UC-GEN-06 | Plan Preventive Maintenance | Facility Officer | Create and run recurring maintenance plans and follow the preventive calendar and inspection results (UC-OFF-16 to UC-OFF-20). |
| UC-GEN-07 | Receive Assigned Work Orders | Maintenance Technician | View assigned work orders and accept or reject them (UC-TEC-01 to UC-TEC-04). |
| UC-GEN-08 | Update Work Order Progress | Maintenance Technician | Extends UC-GEN-07. Update progress, complete the work with root cause, spare parts and photo evidence, and carry out preventive inspections (UC-TEC-05 to UC-TEC-07, UC-TEC-09 to UC-TEC-11). |
| UC-GEN-09 | View Management Dashboard | Facility Manager | View overall maintenance figures and all cases, assets and plans in read-only mode (UC-MGR-01 to UC-MGR-07). |
| UC-GEN-10 | Analyze Maintenance KPIs | Facility Manager | Extends UC-GEN-09. Analyze failure trends and export reports (UC-MGR-09, UC-MGR-10). |
| UC-GEN-11 | Monitor Maintenance Performance | Facility Manager | Extends UC-GEN-09. Monitor technician performance: completed cases, average handling time and reopened cases (UC-MGR-08). |
| UC-GEN-12 | Manage Users | Administrator | Create, edit, unlock and deactivate accounts and assign roles (UC-ADM-01). |
| UC-GEN-13 | Manage Facilities and Assets | Administrator | Manage the facility tree, assets, asset import and QR codes (UC-ADM-04 to UC-ADM-07). |
| UC-GEN-14 | Manage Maintenance Categories | Administrator | Manage issue types, asset categories and root causes (UC-ADM-02). |
| UC-COM-01 | Log In to System | All roles | Sign in with email and password. A first-time user must change the temporary password; 5 wrong attempts lock the account for 15 minutes. |
| UC-COM-02 | Manage User Profile | All roles | View and update personal information and change the account password. |
| UC-COM-03 | Use AI Assistant | All roles | Ask the AI chatbot about procedures, troubleshooting and own data. Answers are limited to the data the user's role may see (RBAC-scoped). |
| UC-COM-04 | View System Notifications | All roles | Check and open alerts about case and account events in the Notification Center. |
| UC-COM-05 | Global Search | All roles | Search maintenance cases, assets or users across the system, within the user's permission scope. |
| UC-COM-06 | Reset Password | All roles | Request a password reset link by email and set a new password when the password is forgotten. |
| UC-COM-07 | Manage Personal Settings | All roles | Configure personal preferences such as language and email notifications. |
| UC-EMP-01 | Create Maintenance Request | Employee | Create and submit a facility incident report with location (building, floor, room), optional asset, issue type, description, suggested urgency and up to 5 photos; can also start by scanning an asset QR label. |
| UC-EMP-02 | Track Maintenance Requests | Employee | View, filter and sort the list of maintenance requests submitted by the employee. |
| UC-EMP-03 | View Request Details | Employee | View the details, current status and comment timeline of a specific request, and add comments. |
| UC-EMP-04 | Provide Additional Info | Employee | Extends UC-EMP-03. Update information or add photos when a Facility Officer asks for clarification; the request goes back to Pending. |
| UC-EMP-05 | Rate Maintenance Service | Employee | Extends UC-EMP-03. Rate the maintenance result after the case is Closed (optional). |
| UC-EMP-06 | Cancel Maintenance Request | Employee | Extends UC-EMP-03. Cancel an own request before it is approved (status Pending or Need more info). |
| UC-EMP-07 | Report "Not Fixed" | Employee | Extends UC-EMP-03. Report that a closed case is still broken, within 7 days after closure and once per case; the Facility Officer decides to reopen or decline. |
| UC-OFF-01 | Claim Case from Queue | Facility Officer | Take a case from the shared queue (new requests or reopen requests) to work on it. A claim is released automatically after 30 minutes without action; one Officer holds at most 5 claims. |
| UC-OFF-02 | View Case Details | Facility Officer | Open the full information of a case: request data, photos, status history, comments and work records, and add comments or internal notes. |
| UC-OFF-03 | Approve Maintenance Request | Facility Officer | Extends UC-OFF-02. Approve a claimed request and set the final priority; the case becomes Approved and waits for assignment. |
| UC-OFF-04 | Reject Maintenance Request | Facility Officer | Extends UC-OFF-02. Reject an invalid or duplicate request with a reason; the case becomes Rejected. |
| UC-OFF-05 | Request More Information | Facility Officer | Extends UC-OFF-02. Ask the Employee a question; the case becomes Need more info. |
| UC-OFF-06 | Assign Technician | Facility Officer | Extends UC-OFF-02. Choose a Technician (current workload is shown) and a due date for an approved case; the case becomes Assigned. |
| UC-OFF-07 | Reassign Technician | Facility Officer | Extends UC-OFF-02. Change the Technician of a case that is being handled. |
| UC-OFF-08 | Cancel Case | Facility Officer | Extends UC-OFF-02. Cancel a case while it is still open (not Completed, Closed, Rejected or Cancelled); the case becomes Cancelled. |
| UC-OFF-09 | Verify and Close Case | Facility Officer | Extends UC-OFF-02. Check the work reported as completed by the Technician, then close the case (Closed) or send it back (Reopened). |
| UC-OFF-10 | Handle Reopen Request | Facility Officer | Extends UC-OFF-02. Accept (Reopened) or decline an Employee's "Not fixed" report. |
| UC-OFF-11 | Track Case Progress | Facility Officer | View all cases in every status and filter or sort them by status, due date or Technician. |
| UC-OFF-12 | Create Case on Behalf of Employee | Facility Officer | Enter a request for an Employee who reported by phone or in person. |
| UC-OFF-13 | View Asset List and Details | Facility Officer | Browse the facility tree, search assets and view their details and maintenance history. |
| UC-OFF-14 | Change Asset Status | Facility Officer | Mark an asset as Active, Under repair or Out of service. |
| UC-OFF-15 | Print Asset QR Labels | Facility Officer | Generate and print QR labels to stick on assets for quick reporting. |
| UC-OFF-16 | Create / Edit Preventive Plan | Facility Officer | Create or edit A recurring maintenance plan for one or more assets (frequency, checklist, default Technician). |
| UC-OFF-17 | Pause / Resume / End Preventive Plan | Facility Officer | Change the running state of a preventive plan. |
| UC-OFF-18 | Generate Preventive Work Order Now | Facility Officer | Create a preventive work order immediately instead of waiting for the schedule. |
| UC-OFF-19 | View Preventive Calendar | Facility Officer | View upcoming preventive work orders by week or month. |
| UC-OFF-20 | View Inspection Records | Facility Officer | Review the results of completed preventive inspections. |
| UC-OFF-21 | View KPI Dashboard | Facility Officer | View case volume, SLA and reopen-rate figures (short Officer view, without technician rankings). |
| UC-OFF-22 | View Failure Trends | Facility Officer | View assets that fail often and failure trends to plan preventive work. |
| UC-OFF-23 | Export Reports | Facility Officer | Export case and KPI reports to PDF or Excel. |
| UC-TEC-01 | Manage Assigned Work Orders | Maintenance Technician | View and filter new, ongoing and due preventive work orders assigned to the technician. |
| UC-TEC-02 | View Work Order Details | Maintenance Technician | View the original request, the asset details and the maintenance history of the asset. |
| UC-TEC-03 | Accept Work Order | Maintenance Technician | Extends UC-TEC-02. Accept an assigned work order (status becomes Accepted). |
| UC-TEC-04 | Reject Work Order | Maintenance Technician | Extends UC-TEC-02. Decline a work order before accepting it, with a reason; it returns to the Facility Officer (status Approved). |
| UC-TEC-05 | Update Work Order Progress | Maintenance Technician | Extends UC-TEC-02. Start the work, put it on hold with a reason, resume it, or save progress notes. |
| UC-TEC-06 | Discuss Work Order | Maintenance Technician | Extends UC-TEC-02. Add comments or photos to the work order timeline. |
| UC-TEC-07 | Complete Work Order | Maintenance Technician | Extends UC-TEC-02. Enter root cause, labor hours, spare parts and photo evidence, then mark the work as Completed. |
| UC-TEC-08 | View Work History | Maintenance Technician | View, filter and review work orders that the technician has completed. |
| UC-TEC-09 | Perform Routine Inspection | Maintenance Technician | Open the checklist of a scheduled preventive maintenance work order. |
| UC-TEC-10 | Update Inspection Checklist | Maintenance Technician | Extends UC-TEC-09. Mark items Pass, Fail or N/A, add the required note and photo for every failed item and save progress. |
| UC-TEC-11 | Complete Routine Inspection | Maintenance Technician | Extends UC-TEC-09. Submit the checklist; a failed item can automatically create a new repair case. |
| UC-MGR-01 | View Dashboard | Facility Manager | View the overall maintenance figures on the Manager home page and the KPI dashboard. |
| UC-MGR-02 | View All Maintenance Cases | Facility Manager | View the list and progress of all maintenance cases (read only). |
| UC-MGR-03 | View Case Details | Facility Manager | View the details of one maintenance case (read only). |
| UC-MGR-04 | View Asset Details | Facility Manager | View an asset's maintenance history, preventive plans and failure frequency. |
| UC-MGR-05 | View Preventive Maintenance Plan Details | Facility Manager | View preventive maintenance plans, the preventive calendar and inspection records. |
| UC-MGR-06 | View Facility Tree | Facility Manager | View the buildings, floors and rooms of the company. |
| UC-MGR-07 | View Asset List | Facility Manager | View the list of assets. |
| UC-MGR-08 | Analyze Technician Performance | Facility Manager | View and analyze technician indicators: completed cases, average time from Assigned to Completed, number of reopened cases. |
| UC-MGR-09 | Analyze Failure Trends | Facility Manager | Analyze the assets and issue types that fail most often. |
| UC-MGR-10 | Export Reports | Facility Manager | Export reports to Excel or PDF. |
| UC-ADM-01 | Manage Users | Administrator | Create or invite, edit, unlock, deactivate and reactivate user accounts, reset passwords and assign their role. |
| UC-ADM-02 | Configure Maintenance Categories | Administrator | Manage the option lists used in forms: issue types, asset categories and root causes. |
| UC-ADM-03 | View Audit Logs | Administrator | View all recorded activities of every user in the system. |
| UC-ADM-04 | Manage Facility Tree | Administrator | Create, edit and deactivate buildings, floors and rooms. |
| UC-ADM-05 | Manage Assets | Administrator | Create, edit and retire assets; retiring is blocked while the asset has open cases. |
| UC-ADM-06 | Import Assets from File | Administrator | Add many assets at once from an Excel or CSV file. |
| UC-ADM-07 | Generate Asset QR Codes | Administrator | Create QR codes to stick on assets so users can scan them to report issues quickly. |
| UC-ADM-08 | Configure SLA and Rules | Administrator | Set SLA target times per priority and rule values such as the claim timeout, the reopen window and the session length. |
| UC-ADM-09 | View Roles and Permissions | Administrator | View the read-only reference table of what each of the five roles can do. |

## **3\. Functional Requirements** {#3.-functional-requirements}

### **3.1 System Functional Overview** {#3.1-system-functional-overview}

This part gives the functional overview of SmartMaint (web and mobile application, release 1.0): the Screens Flow, the descriptions of all 81 screens, the screen authorization of the five roles, the non-screen functions and the Entity Relationship Diagram. The detailed functions are specified in 3.2 to 3.18. The screens and functions in this part are specified for the web application; the Flutter mobile application provides the same functions for all five roles, with the same business rules and permissions (3.1.3), using phone layouts. Mobile-specific interfaces (camera, QR scanning, push notifications) are described in 4.1.

#### ***3.1.1 Screens Flow***

The Screens Flow is split into eight diagrams: one for sign-in, one for the screens shared by all roles and one for each role area. Normal screens (pages) are rectangles and pop-up screens (dialogs) are ovals, as required by the template; grey dashed boxes are navigation areas or notes, not screens. Every one of the 81 screens (58 pages and 23 dialogs) appears in at least one diagram. The screen codes are the same codes used in 3.1.2, 3.1.3 and 3.2 to 3.18.

![][image10]

![][image11]

*Figure 9\. Screens Flow \- Sign-in and account access (all roles)*

![][image12]

*Figure 10\. Screens Flow \- Shared screens after sign-in (all roles)*

![][image13]

*Figure 11\. Screens Flow \- Employee*

![][image14]

*Figure 12\. Screens Flow \- Facility Officer: handling cases*

![][image15]

*Figure 13\. Screens Flow \- Facility Officer: assets, preventive maintenance, analytics*

![][image16]

*Figure 14\. Screens Flow \- Maintenance Technician*

![][image17]

*Figure 15\. Screens Flow \- Facility Manager*

![][image18]

*Figure 16\. Screens Flow \- Administrator*

#### ***3.1.2 Screen Descriptions***

The table describes all 81 screens (58 pages and 23 dialogs). Screen codes match the Screens Flow and the function details in 3.2 to 3.18.

| \# | Feature | Screen | Description |
| ----- | :---- | :---- | :---- |
| **Account and Sign-in (AUTH)** |  |  |  |
| 1 | Login | AUTH-01 Login | Public page (no sign-in needed). Users sign in with the email and password given by the Administrator. |
| 2 | Forgot Password | AUTH-02 Forgot Password | Lets a user ask for a link to set a new password by email. |
| 3 | Reset Password | AUTH-03 Reset Password | Lets the user choose a new password after clicking the link in the email. |
| 4 | First-time Login | AUTH-04 Force Change Password (first login) | Makes a new user replace the temporary password that the Administrator gave them. |
| 5 | Session Management | AUTH-05 Session Expired | Tells the user their sign-in time is over and lets them sign in again without losing their place. |
| 6 | Account Security | AUTH-06 Account Locked or Deactivated | Explains why a user cannot use the system and who to contact. |
| **Error Pages (ERR)** |  |  |  |
| 7 | Error Handling | ERR-01 No Permission (403) | Shown when a signed-in user opens a page or record they are not allowed to see. |
| 8 | Error Handling | ERR-02 Page Not Found (404) | Shown when the address does not exist or the record was deleted. |
| 9 | Error Handling | ERR-03 Server Error (500) | Shown when something breaks on the server that the user cannot fix. |
| 10 | Error Handling | ERR-04 No Connection (Offline) | Warns the user that the internet or the server cannot be reached. |
| **Shared Screens (COM)** |  |  |  |
| 11 | Global Layout | COM-01 App Shell (common layout) | The frame around every page after login: side menu, top bar and the floating AI button. It is described once here so other screens do not repeat it. |
| 12 | Profile Management | COM-02 Profile | Shows and lets users edit their own basic information. |
| 13 | Profile Management | COM-03 Change Password | Lets a signed-in user change their password by knowing the old one. |
| 14 | Preferences | COM-04 Personal Settings | Personal choices that change how the app looks and which notifications the user receives. |
| 15 | Notifications | COM-05 Notification Center | One place to see all notifications and open the related case or record. |
| 16 | Search | COM-06 Global Search and Advanced Filters | Finds cases, assets and users quickly from anywhere. Each user only finds what their role may see. |
| 17 | AI Assistance | COM-07 AI Assistant (floating chat) | A chat window where any user can ask questions. The AI only answers with information that the user's own role is already allowed to see. It gives advice and never makes decisions. |
| 18 | Case Management | COM-08 Case Detail | The single page that shows everything about one maintenance case: what was reported, who handles it, what was done, the history and the discussion. What each role sees and can do depends on its role and the case status. |
| **Employee (EMP)** |  |  |  |
| 19 | Dashboard | EMP-01 Employee Home | A short overview for the Employee: what needs attention and the latest requests. |
| 20 | Request Creation | EMP-02 New Request | Lets an Employee report a facility problem so that it becomes a new Case. |
| 21 | Request Tracking | EMP-03 My Requests | The Employee's list of all their requests with their current status. |
| 22 | Request Editing | EMP-04 Edit Request / Provide More Information | Lets the Employee change a request that is not yet approved, or answer the Officer's question when the status is Need more info. |
| **Facility Officer (OFF)** |  |  |  |
| 23 | Dashboard | OFF-01 Officer Home | Shows the Officer what needs a decision today. |
| 24 | Queue Management | OFF-02 Case Queue | The shared queue where new work arrives. The Officer picks a case, claims it and decides: approve, reject, ask for information, or assign a technician. |
| 25 | Case Tracking | OFF-03 Case Tracking | Shows every case in every status so the Officer can follow the progress and find delays. The Facility Manager also uses this same list, read-only (the old separate "All Cases" screen), to look at every case and its progress. |
| 26 | Verification | OFF-04 Verification Queue | Lists work that the Technician marked Completed. The Officer checks the report and photos, then closes the case or reopens it. |
| 27 | Case Creation | OFF-05 Create Case for an Employee | For problems reported by phone or in person: the Officer enters the case on behalf of the Employee. |
| **Maintenance Technician (TEC)** |  |  |  |
| 28 | Dashboard | TEC-01 Technician Home | Shows the Technician what to do today. |
| 29 | Work Orders | TEC-02 My Work Orders | The Technician's list of active work orders. |
| 30 | Execution | TEC-03 Work Order: Update and Complete | Where the Technician records the real work on site: accept, start, note the cause, parts, hours, photos, put on hold, and mark completed. |
| 31 | History | TEC-04 Work History | Lets the Technician look back at finished work orders and find what they did on an asset before. |
| 32 | Preventive Maintenance | TEC-05 Inspection Checklist (preventive) | For preventive work orders: the Technician goes through the checklist of a plan and records the result for each item. |
| **Facility Manager (MGR)** |  |  |  |
| 33 | Dashboard | MGR-01 Manager Home | A quick look at how maintenance is performing across all buildings, and what needs attention. |
| **Administrator (ADM)** |  |  |  |
| 34 | Dashboard | ADM-01 Administrator Home | Gives the Administrator a quick view of accounts, data quality and recent activity. |
| 35 | User Management | ADM-02 User List | Shows every account and lets the Administrator search, filter and manage them. |
| 36 | User Management | ADM-03 Create / Edit User | Creates a new account or changes an existing one, including the role. |
| 37 | User Management | ADM-04 User Detail | Shows one account in full with its recent activity and the management actions. |
| 38 | System Configuration | ADM-05 Roles and Permissions (reference) | A read-only table that explains what each of the five roles can do. The five roles are fixed and cannot be edited. |
| 39 | System Configuration | ADM-06 Maintenance Categories | Manages the lists that people choose from: issue types, asset categories and root causes. |
| 40 | System Configuration | ADM-07 SLA and Rules Settings | Sets the target times for handling cases and a few numbers the system uses in its rules, so they can be changed without changing the software. |
| 41 | Audit | ADM-08 Audit Log | A record of who did what and when, including sign-ins, changes and every AI question. It can be read and searched but never edited or deleted. |
| **Assets and Facilities (AST)** |  |  |  |
| 42 | Location Structure | AST-01 Facility Tree (Building, Floor, Room) | Shows all places as a tree and lets the Administrator create and edit them. |
| 43 | Asset Management | AST-02 Asset List | Lists all assets with filters. The Administrator manages them, the Officer changes their status, the Manager reads. |
| 44 | Asset Management | AST-03 Asset Detail | Shows everything about one asset: information, its maintenance history, its preventive plans and how often it fails. |
| 45 | Asset Management | AST-04 Create / Edit Asset | Adds a new asset or edits an existing one. |
| 46 | Asset Management | AST-05 Import Assets from File | Adds many assets at once from an Excel or CSV file, in four clear steps. |
| 47 | Asset Identification | AST-06 QR Labels (print) | Creates printable QR labels that people stick on assets. Scanning a label opens the report form for that asset. |
| 48 | Quick Reporting | AST-07 QR Landing (scan to report) | Decides where to send a person who scans a QR label on an asset, and explains problems with the label. |
| **Preventive Maintenance (PM)** |  |  |  |
| 49 | Schedule Management | PM-01 Preventive Plan List | Lists the plans that create maintenance work on a schedule, before anything breaks. |
| 50 | Schedule Management | PM-02 Create / Edit Preventive Plan | Defines what to check, how often, on which assets, and who does it. |
| 51 | Schedule Management | PM-03 Preventive Plan Detail | Shows one plan, what it created so far, and how well it is followed. |
| 52 | Calendar View | PM-04 Preventive Calendar | Shows upcoming and past preventive work on a calendar so the Officer can balance the workload. |
| 53 | Execution Logs | PM-05 Inspection Records | The history of all inspections that Technicians completed. |
| 54 | Execution Logs | PM-06 Inspection Record Detail | Shows the full result of one inspection. |
| **Analytics and Decision Support (ANA)** |  |  |  |
| 55 | Reports | ANA-01 KPI Overview Dashboard | Shows how maintenance is performing so managers can make decisions with numbers. The Officer sees a shorter version. |
| 56 | Reports | ANA-02 Technician Performance | Helps the Manager see how each Technician is doing and how the workload is shared. |
| 57 | Reports | ANA-03 Failure Trends and Problem Assets | Shows which assets and which kinds of problems happen most, so preventive work can be planned better. |
| 58 | Data Export | ANA-04 Reports and Export | Creates reports as PDF or Excel files for meetings and records. |
| **Dialogs / pop-up screens (DLG)** |  |  |  |
| 59 | Case Actions | DLG-01 Approve Case | The Officer accepts the request as a real maintenance job and sets its final priority. |
| 60 | Case Actions | DLG-02 Reject Case | The Officer refuses the request and explains why. The case ends as Rejected. |
| 61 | Case Actions | DLG-03 Request More Information | The Officer asks the Employee a question before deciding. |
| 62 | Work Assignment | DLG-04 Assign Technician | The Officer chooses who will do the work and by when. |
| 63 | Work Assignment | DLG-05 Reassign Technician | Moves a case that was already given to another Technician, for example when someone is ill or overloaded. |
| 64 | Case Closure | DLG-06 Verify and Close | The Officer confirms that the work was done well and closes the case. |
| 65 | Case Actions | DLG-07 Reopen Case | Sends a case back to the Technician because the fix was not good enough or the Employee says it is not fixed. |
| 66 | Case Actions | DLG-08 Decline Reopen Request | The Officer says no to an Employee's "Not fixed" report and explains. |
| 67 | Work Actions | DLG-09 Reject Assignment | The Technician cannot take a work order and tells the Officer why. |
| 68 | Work Actions | DLG-10 Put Work on Hold | The Technician pauses work, for example because a part or a permission is missing. |
| 69 | Work Actions | DLG-11 Confirm Completion | A last check before the Technician reports the work as done. |
| 70 | Case Actions | DLG-12 Cancel Request or Case | Stops a request. The Employee can cancel their own request before it is approved; an Officer can cancel a case while it is still open (not Completed, Closed, Rejected or Cancelled). |
| 71 | Feedback | DLG-13 Report "Not Fixed" | After a case is Closed, the Employee can say the problem is still there. The Officer decides what to do. |
| 72 | Feedback | DLG-14 Rate the Service | The Employee gives a rating after the case is closed, which helps the Manager see quality. |
| 73 | Master Data | DLG-15 Building, Floor or Room Form | Creates or edits a place in the facility tree. |
| 74 | Account Actions | DLG-16 User Account Action | Asks the Administrator to confirm a sensitive account action. |
| 75 | Data Management | DLG-17 Confirm Retire or Delete | Protects important data: retiring an asset, deactivating or deleting a place, or deleting a category. |
| 76 | Asset Actions | DLG-18 Change Asset Status | The Officer records whether an asset is working, being repaired or out of service. |
| 77 | System Prompt | DLG-19 Unsaved Changes | Prevents losing typed data when the user leaves a form. |
| 78 | Media Viewer | DLG-20 Photo Viewer and Upload Problems | Shows photos large and explains problems when adding photos. |
| 79 | PM Execution | DLG-21 Generate Preventive Work Order Now | Creates the next preventive work order early, without waiting for the schedule. |
| 80 | Queue Actions | DLG-22 Claim Conflict and Release | Handles the moments when two Officers want the same case, or when a claim ends. |
| 81 | Master Data | DLG-23 Category Item Form | Adds or edits one item in a maintenance category list. |

#### ***3.1.3 Screen Authorization***

Legend: X \= full access; X\* \= access with a limited scope or only some actions; R \= read only; P \= public screen, used before signing in; empty \= no access. A button that a role must never use is hidden, not greyed out; opening a screen without permission shows ERR-01 (403) and the attempt is logged.

| Screen | Employee | Officer | Technician | Manager | Admin |
| :---- | ----- | ----- | ----- | ----- | ----- |
| **Account and Sign-in (AUTH)** |  |  |  |  |  |
| AUTH-01 Login | P | P | P | P | P |
| AUTH-02 Forgot Password | P | P | P | P | P |
| AUTH-03 Reset Password | P | P | P | P | P |
| AUTH-04 Force Change Password (first login) | X | X | X | X | X |
| AUTH-05 Session Expired | X | X | X | X | X |
| AUTH-06 Account Locked or Deactivated | P | P | P | P | P |
| **Error Pages (ERR)** |  |  |  |  |  |
| ERR-01 No Permission (403) | X | X | X | X | X |
| ERR-02 Page Not Found (404) | X | X | X | X | X |
| ERR-03 Server Error (500) | X | X | X | X | X |
| ERR-04 No Connection (Offline) | X | X | X | X | X |
| **Shared Screens (COM)** |  |  |  |  |  |
| COM-01 App Shell (common layout) | X | X | X | X | X |
| COM-02 Profile | X | X | X | X | X |
| COM-03 Change Password | X | X | X | X | X |
| COM-04 Personal Settings | X | X | X | X | X |
| COM-05 Notification Center | X | X | X | X | X |
| COM-06 Global Search and Advanced Filters | X\* | X\* | X\* | X\* | X\* |
| COM-07 AI Assistant (floating chat) | X | X | X | X | X |
| COM-08 Case Detail | X\* | X |  | R |  |
| **Employee (EMP)** |  |  |  |  |  |
| EMP-01 Employee Home | X |  |  |  |  |
| EMP-02 New Request | X |  |  |  |  |
| EMP-03 My Requests | X |  |  |  |  |
| EMP-04 Edit Request / Provide More Information | X |  |  |  |  |
| **Facility Officer (OFF)** |  |  |  |  |  |
| OFF-01 Officer Home |  | X |  |  |  |
| OFF-02 Case Queue |  | X |  |  |  |
| OFF-03 Case Tracking |  | X |  | R |  |
| OFF-04 Verification Queue |  | X |  |  |  |
| OFF-05 Create Case for an Employee |  | X |  |  |  |
| **Maintenance Technician (TEC)** |  |  |  |  |  |
| TEC-01 Technician Home |  |  | X |  |  |
| TEC-02 My Work Orders |  |  | X |  |  |
| TEC-03 Work Order: Update and Complete |  |  | X |  |  |
| TEC-04 Work History |  |  | X |  |  |
| TEC-05 Inspection Checklist (preventive) |  |  | X |  |  |
| **Facility Manager (MGR)** |  |  |  |  |  |
| MGR-01 Manager Home |  |  |  | X |  |
| **Administrator (ADM)** |  |  |  |  |  |
| ADM-01 Administrator Home |  |  |  |  | X |
| ADM-02 User List |  |  |  |  | X |
| ADM-03 Create / Edit User |  |  |  |  | X |
| ADM-04 User Detail |  |  |  |  | X |
| ADM-05 Roles and Permissions (reference) |  |  |  |  | X |
| ADM-06 Maintenance Categories |  |  |  |  | X |
| ADM-07 SLA and Rules Settings |  |  |  |  | X |
| ADM-08 Audit Log |  |  |  |  | X |
| **Assets and Facilities (AST)** |  |  |  |  |  |
| AST-01 Facility Tree (Building, Floor, Room) |  | R |  | R | X |
| AST-02 Asset List |  | X\* |  | R | X |
| AST-03 Asset Detail |  | X\* |  | R | X\* |
| AST-04 Create / Edit Asset |  |  |  |  | X |
| AST-05 Import Assets from File |  |  |  |  | X |
| AST-06 QR Labels (print) |  | X |  |  | X |
| AST-07 QR Landing (scan to report) | X | X | X | X | X |
| **Preventive Maintenance (PM)** |  |  |  |  |  |
| PM-01 Preventive Plan List |  | X |  | R |  |
| PM-02 Create / Edit Preventive Plan |  | X |  |  |  |
| PM-03 Preventive Plan Detail |  | X |  | R |  |
| PM-04 Preventive Calendar |  | X |  | R |  |
| PM-05 Inspection Records |  | X |  | R |  |
| PM-06 Inspection Record Detail |  | X |  | R |  |
| **Analytics and Decision Support (ANA)** |  |  |  |  |  |
| ANA-01 KPI Overview Dashboard |  | X\* |  | X |  |
| ANA-02 Technician Performance |  |  |  | X |  |
| ANA-03 Failure Trends and Problem Assets |  | X\* |  | X |  |
| ANA-04 Reports and Export |  | X\* |  | X |  |
| **Dialogs / pop-up screens (DLG)** |  |  |  |  |  |
| DLG-01 Approve Case |  | X |  |  |  |
| DLG-02 Reject Case |  | X |  |  |  |
| DLG-03 Request More Information |  | X |  |  |  |
| DLG-04 Assign Technician |  | X |  |  |  |
| DLG-05 Reassign Technician |  | X |  |  |  |
| DLG-06 Verify and Close |  | X |  |  |  |
| DLG-07 Reopen Case |  | X |  |  |  |
| DLG-08 Decline Reopen Request |  | X |  |  |  |
| DLG-09 Reject Assignment |  |  | X |  |  |
| DLG-10 Put Work on Hold |  |  | X |  |  |
| DLG-11 Confirm Completion |  |  | X |  |  |
| DLG-12 Cancel Request or Case | X\* | X |  |  |  |
| DLG-13 Report "Not Fixed" | X |  |  |  |  |
| DLG-14 Rate the Service | X |  |  |  |  |
| DLG-15 Building, Floor or Room Form |  |  |  |  | X |
| DLG-16 User Account Action |  |  |  |  | X |
| DLG-17 Confirm Retire or Delete |  |  |  |  | X |
| DLG-18 Change Asset Status |  | X |  |  |  |
| DLG-19 Unsaved Changes | X | X | X | X | X |
| DLG-20 Photo Viewer and Upload Problems | X | X | X | X | X |
| DLG-21 Generate Preventive Work Order Now |  | X |  |  |  |
| DLG-22 Claim Conflict and Release |  | X |  |  |  |
| DLG-23 Category Item Form |  |  |  |  | X |

#### ***3.1.4 Non-Screen Functions***

Functions that run in the backend without their own screen (services, background jobs and integrations). Their details are in 3.18.

| \# | Feature | System Function | Description |
| :---- | :---- | :---- | :---- |
| 1 | AI-assisted support (RBAC-scoped) | Create AI Prediction | Receives a question from a user, adds context filtered by the same RBAC scope as the user's role, calls Replicate to create a prediction and returns the prediction id immediately (Replicate is asynchronous and does not return the answer at once). |
| 2 | AI-assisted support (RBAC-scoped) | Poll / Webhook AI Result | Receives the result from Replicate by webhook (or the client polls the status every 2 seconds), stores the answer in AIConversation and writes the Audit Log both when the request is created and when the result arrives. |
| 3 | Case handling | Auto-release Claim | Background job: when an Officer claims a case but does nothing for 30 minutes, the claim is released back to the shared queue. One Officer can hold at most 5 claims. Both numbers are set in ADM-07. |
| 4 | Case handling | SLA Timer & Overdue Check | Follows sla\_target\_at of every open case according to its priority, in working hours; the timer pauses while the case is On Hold. Close to the target the Officers are notified; when overdue the case is flagged as overdue and the Manager is also notified. |
| 5 | Preventive maintenance | Auto-generate Preventive Work Order | When a preventive plan reaches its lead time before the due date (default 3 days), creates a new work order that skips the Pending review and starts as Approved (or Assigned when the plan has a default Technician). |
| 6 | Preventive maintenance | Inspection Failure to New Case | When a Technician ticks a failed checklist item for follow-up, creates a new repair Case (Pending) linked to the inspection record. |
| 7 | Notification | Notification Dispatch | Sends in-app and email notifications at the case milestones listed in 5.4.2 (new request, approve/reject/need info, assign, hold, complete, close, reopen, cancel, SLA warnings) and account events. |
| 8 | Audit | Audit Logging | Writes every sign-in, data change, status change, export and AI question to the Audit Log. Records cannot be edited or deleted. |

#### ***3.1.5 Entity Relationship Diagram***

The ERD uses Chen notation, as in the template sample: rectangles are entities, diamonds are relationships and 1 / M / 0..1 are cardinalities. Compared with the first draft, the entity Floor was added between Building and Room, because the facility tree (AST-01) and the place form (DLG-15) manage floors as their own records (Building 1 \- M Floor, Floor 1 \- M Room). The entities IssueType, RootCause and SystemSetting were also added for the lists and rule values managed by the Administrator (ADM-06, ADM-07), and one preventive plan can now cover several assets (PreventivePlan M \- M Asset).

![][image19]

*Figure 17\. Entity Relationship Diagram \- SmartMaint*

**Entities Description**

| \# | Entity | Description |
| :---- | :---- | :---- |
| 1 | Building | A building of the company; contains floors. |
| 2 | Floor | A floor of a building; groups the rooms on that floor. |
| 3 | Room | A room or common area (lobby, corridor) on a floor; the place where assets are located and problems are reported. |
| 4 | AssetCategory | Type of asset (for example air conditioner, lighting, network), managed by the Administrator. |
| 5 | Asset | Equipment to be maintained; belongs to one room and one category, has a QR code and a status (Active, Under repair, Out of service, Retired). |
| 6 | User | A user account with exactly one of the five roles. |
| 7 | Case | A maintenance request/case with status, priority and SLA target; in release 1.0 one Case is also its Work Order. |
| 8 | CaseComment | A comment on a case; can be marked as an internal note hidden from the Employee. |
| 9 | CaseAttachment | A photo or file attached to a case by the Employee, Officer or Technician. |
| 10 | PreventivePlan | A recurring maintenance plan for one or more assets (frequency, checklist, default Technician, status). |
| 11 | InspectionRecord | The result of one preventive inspection; can create a new Case when an item fails. |
| 12 | AuditLog | Read-only record of user activities (sign-in, changes, status changes, exports, AI question events without the question and answer text). |
| 13 | AIConversation | AI Assistant question and answer history per user, with the Replicate prediction id, status and the user's rating. |
| 14 | Notification | An in-app notification sent to a user, optionally linked to a case. |
| 15 | IssueType | Type of problem chosen in a request; also used as a Technician's specialty; managed by the Administrator. |
| 16 | RootCause | Common root cause chosen by the Technician when completing a work order; managed by the Administrator. |
| 17 | SystemSetting | A configurable rule value (SLA targets, claim time-out, reopen window, session length, working hours) changed by the Administrator in ADM-07; every change is written to the Audit Log. |

### **3.2 Sign-in and Account Recovery** {#3.2-sign-in-and-account-recovery}

#### ***3.2.1 Sign In***

**Function trigger:** The user opens the site, a session ends (AUTH-05), or after signing out.

**Function description:** Public page, open to anyone. Every one of the five roles signs in here with the email and password that the Administrator gave them; there is no self-registration.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image20]

**Function Details:**

The form takes an email and a password and, on success, sends the user to the Home page of their role (or to AUTH-04 first, if this is their first sign-in with a temporary password). Client-side checks catch empty fields before anything is sent; the server checks the credentials and never reveals which of the two was wrong.

* Wrong credentials: generic message "Email or password is incorrect" (never says which field).  
* 5 wrong attempts lock the account for 15 minutes (BR-18); a deactivated account is sent to AUTH-06.  
* A link opened before sign-in (notification, QR code) is remembered and reopened right after sign-in.  
* Every attempt, success or fail, is written to the Audit Log (BR-16).

***Screens covered:** AUTH-01*

#### ***3.2.2 Forgot and Reset Password***

**Function trigger:** Forgot password link on AUTH-01 (AUTH-02); the link in the reset email (AUTH-03).

**Function description:** Public, self-service password recovery in two steps: request a reset link by email, then choose a new password that meets the password policy.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image21]

**Function Details:**

AUTH-02 only asks for an email and always shows the same confirmation message, whether or not that email has an account, so nobody can use the form to discover valid emails. AUTH-03 shows a live checklist against the password rules and only enables Save once both the rules and the confirmation match are satisfied; saving ends every other open session of the account and sends a confirmation email.

* Reset link is valid 30 minutes and single-use (BR-18); an expired or reused link shows "no longer valid" with a link back to AUTH-02.  
* Password policy: at least 8 characters with uppercase, lowercase and a number (BR-18); the new password must differ from the old one.  
* Resend on AUTH-02 is throttled to once every 60 seconds.  
* Both steps are written to the Audit Log.

***Screens covered:** AUTH-02, AUTH-03*

#### ***3.2.3 First-time Password Change and Session Handling***

**Function trigger:** AUTH-04 right after a first sign-in or an Administrator-issued temporary password; AUTH-05 when the server rejects an expired session; AUTH-06 when the account is deactivated.

**Function description:** Three closely related system pages that guard the boundaries of a session: forcing a temporary password to be replaced, telling the user their 8-hour session ended, and explaining that their account no longer has access.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image22]

**Function Details:**

AUTH-04 blocks every other page until the temporary password is replaced with the same rules as AUTH-03; a small link lets the user log out instead of changing it. AUTH-05 appears as an overlay on any page when the token expires (a quiet warning appears 5 minutes beforehand) and returns the user to the exact page they were on after signing in again. AUTH-06 is shown after a failed sign-in on a deactivated account, or immediately if the Administrator deactivates the account of a user who is already signed in; the account's history (cases, comments) is kept even though it can no longer sign in.

* Session length is 8 hours (BR-18), configurable in ADM-07.  
* Unsaved typed data is kept in the browser tab where possible and restored after re-signing in on AUTH-05.  
* A photo upload interrupted by a session timeout must be re-added.

***Screens covered:** AUTH-04, AUTH-05, AUTH-06*

### **3.3 Application Shell and System Pages** {#3.3-application-shell-and-system-pages}

#### ***3.3.1 Application Shell and Navigation***

**Function trigger:** Shown automatically around every page after sign-in (all pages except AUTH-01 to AUTH-03 and AUTH-06).

**Function description:** The common frame — side menu, top bar and floating AI button — described once here so it is not repeated on every other screen; every one of the five roles uses it, with a menu built from their own permitted screens only.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image23]

**Function Details:**

The side menu, search box, notification bell, avatar menu and the floating AI Assistant button are always present; below 1024 px the side menu collapses into a hamburger button and tables scroll sideways, since the web application targets desktop use (the mobile application uses its own phone layout). Menu items never point to a screen the signed-in role cannot open, so a user only reaches a forbidden page (ERR-01) by typing an address by hand or opening an old link.

* The Administrator's menu has no case-related item, matching BR-11 (no case content for the Administrator).  
* Logging out asks to confirm only when a form has unsaved changes (DLG-19).  
* The unread-notification bell shows the exact count up to 99, then "99+".

***Screens covered:** COM-01*

#### ***3.3.2 Error and Offline Pages***

**Function trigger:** ERR-01 to ERR-04 pages appear automatically whenever a request cannot be completed normally: no permission, no matching address, a server failure, or no network connection.

**Function description:** Four system pages, available to all signed-in roles, that explain a failure in plain language instead of showing a technical error, and always offer a way back to the user's own Home page.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image24]

**Function Details:**

No Permission (403) appears for hand-typed addresses or links shared across roles, and never confirms whether the record even exists, so it doubles as the response to "case belongs to someone else". Page Not Found (404) covers both a wrong address and a record that was genuinely removed, though in normal use records are retired or deactivated rather than deleted, so a 404 on a case code usually means it was mistyped. Server Error (500) shows a short reference code the user can give to the Administrator, and a failure in only part of a page (one widget) shows a local "Could not load. Retry" instead of a full-page error. No Connection appears as a top bar the moment the browser goes offline, disables Submit buttons, and resumes a paused photo upload automatically once the connection returns.

* Every blocked (403) attempt is written to the Audit Log.  
* The web and mobile applications do not work offline; ERR-04 only prevents silent data loss.

***Screens covered:** ERR-01, ERR-02, ERR-03, ERR-04*

### **3.4 Personal Account** {#3.4-personal-account}

#### ***3.4.1 Profile and Change Password***

**Function trigger:** Avatar menu → Profile (COM-02); Profile → Change password (COM-03).

**Function description:** Lets every signed-in user view and edit their own basic information and change their own password; role, email, department and employee code stay read-only here because only the Administrator can change them (ADM-03).

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image25]

**Function Details:**

Profile edits name, phone and avatar with simple format checks, and shows the last sign-in time; leaving with unsaved changes triggers DLG-19. Change Password needs the current password plus a new one that meets the same live checklist as AUTH-03; a successful change keeps the current session but ends every other session of the same user and sends a confirmation email.

* 5 wrong attempts at the current password sign the user out.  
* The new password must differ from the current one.  
* All changes are written to the Audit Log (BR-16).

***Screens covered:** COM-02, COM-03*

#### ***3.4.2 Personal Settings and Notification Center***

**Function trigger:** Avatar menu → Settings (COM-04); the notification bell → See all, or a notification email link (COM-05).

**Function description:** Personal preferences (theme, language, per-kind notification switches) and a single inbox of every notification the user's role can receive, each one opening its related record.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image26]

**Function Details:**

Personal Settings lists one row per notification kind the role can receive, with an "in app" and an "email" switch each; important events (assignment, verification) cannot be turned off in either channel. The Notification Center lists all notifications with Unread/All tabs, marks an item read when it is opened, and refreshes within about 30 seconds of a new event without reloading the page.

* Theme and language choices are also remembered so the sign-in page already uses them.  
* Notifications are kept for 90 days; opening a target that no longer applies to the role shows ERR-01/ERR-02 and the item stays in the list.  
* Each role only ever receives its own events (full table in 5.4.2).

***Screens covered:** COM-04, COM-05*

### **3.5 Search and AI Assistant** {#3.5-search-and-ai-assistant}

#### ***3.5.1 Global Search and Advanced Filters***

**Function trigger:** Type in the top bar search box and press Enter, or choose a quick suggestion.

**Function description:** A role-scoped search across cases, assets and users, reached from any page through the app shell; Employee, Officer, Technician and Manager see the Cases tab, Officer/Manager/Administrator see Assets, and only the Administrator sees Users.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image27]

**Function Details:**

Results are grouped in tabs with counts, refined with advanced filters (status, priority, building, technician, date range, etc.), and a result opens the matching detail page (COM-08, or TEC-03 for a Technician, AST-03 or ADM-04). Below 2 typed characters the box only asks for more input; empty results suggest checking spelling or clearing filters; large result sets are paged 20 rows at a time.

* Role scope (same visibility as Case Detail, 3.7.1): Employees find only their own cases, Technicians only their assigned cases, Officers/Managers find all cases, the Administrator finds none.

***Screens covered:** COM-06*

#### ***3.5.2 AI Assistant***

**Function trigger:** Click the floating round AI button in the bottom-right corner of any page.

**Function description:** A chat window, available to every role, that answers questions using only the data the signed-in role is already allowed to see; it advises and never performs an action itself.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image28]

**Function Details:**

The window shows role-specific suggested questions, a thinking indicator while the prediction is generated (it polls every 2 seconds and may take a few seconds to answer), and thumbs up/down rating with a short reason on a thumbs-down. Because it calls an external AI service (Replicate) asynchronously, the UI must clearly show waiting, a "taking longer than usual" state after 60 seconds, and a final timeout with Retry after 2 minutes.

* Out-of-scope questions (e.g. another user's case) are politely refused rather than answered (BR-15).  
* Rate limit: 20 questions per hour per user, 500 characters per question (BR-15).  
* Every question and answer is written to the Audit Log, saved separately from the request/answer generation itself (BR-16).  
* No approve, assign or close button exists inside the chat — advice only.

***Screens covered:** COM-07*

### **3.6 Maintenance Request Submission (Employee)** {#3.6-maintenance-request-submission-(employee)}

#### ***3.6.1 Employee Home***

**Function trigger:** First page after sign-in for an Employee, or menu → Home.

**Function description:** A short dashboard for the Employee showing counts of open requests, items that need their input, ratings still possible, and the latest requests, each linking into My Requests (EMP-03) or Case Detail (COM-08).

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image29]

**Function Details:**

Every number and list on this page is scoped to the signed-in Employee's own requests only; a new user with nothing submitted yet sees an encouraging empty state and the New Request button rather than an empty table.

* "Needs your attention" surfaces Need more info replies and unrated Closed cases in one place.  
* Each block loads and retries independently if its data fails to load.

***Screens covered:** EMP-01*

#### ***3.6.2 New Request***

**Function trigger:** Menu → New Request, the button on EMP-01, or scanning an asset's QR label (AST-07), which pre-fills the place and asset.

**Function description:** The form an Employee uses to report a facility problem, creating a new Case with status Pending; this is the entry point of the whole case lifecycle described in the Case Detail feature below.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image30]

**Function Details:**

Place (Building/Floor/Room, linked dropdowns), issue type, a 5–100 character title and a 10–1000 character description are required; asset, photos (up to 5, JPG/PNG/WEBP, 5 MB each — BR-17) and a suggested urgency are optional or advisory. Submitting is disabled after the first click to avoid a duplicate case, and a duplicate-request warning is shown (non-blocking) when the chosen asset already has an open case.

* The suggested urgency is only a suggestion; the Facility Officer sets the final priority on approval (BR-01).  
* The Case code is generated by the system and cannot be edited.  
* A room with no assets still lets the Employee submit with "Not in the list / not sure".  
* Submit failure keeps the typed text and photos so nothing has to be retyped.

***Screens covered:** EMP-02*

#### ***3.6.3 My Requests, Editing and Providing More Information***

**Function trigger:** Menu → My Requests, or a tile on EMP-01 (EMP-03); COM-08, EMP-03, EMP-01 or a "more information needed" notification (EMP-04).

**Function description:** EMP-03 lists every request the Employee has ever sent, including ones an Officer entered on their behalf, with status, filters and per-row actions; EMP-04 is the same form as New Request, reused either to answer the Officer's question (status Need more info) or to edit a request that is still Pending.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image31]

**Function Details:**

Rows offer Reply, Rate or Cancel depending on status; opening a request goes to Case Detail (COM-08). EMP-04 shows the Officer's question when replying, requires a 10–1000 character reply, and returns the case to Pending once sent; once a case has moved past Pending it can no longer be edited here — the page instead points the Employee to add a comment on COM-08.

* 20 rows per page; "Need my input" is visually highlighted when not empty.  
* If an Officer originally entered the request on the Employee's behalf, the Employee may reply but cannot edit the Officer's original text.  
* A status change on a viewed row is reflected within about 30 seconds without a manual refresh.

***Screens covered:** EMP-03, EMP-04*

### **3.7 Case Detail (shared)** {#3.7-case-detail-(shared)}

#### ***3.7.1 Case Detail View***

**Function trigger:** Opened from My Requests, Case Queue, Case Tracking, Verification Queue, an asset, an inspection record, a notification, search or an AI answer.

**Function description:** The single page that shows everything about one Case — header, progress stepper, request information, work report, timeline, comments and rating — for the Employee who submitted it, the Officer and Manager (read scope) and, indirectly, the Technician (who works from TEC-03 instead). What is visible and which action buttons appear depend on the viewer's role and the case's current status.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image32]

**Function Details:**

The work report (root cause, parts, hours, evidence photos) only appears once the Technician has entered it, and stays hidden from the Employee until the case reaches Completed or Closed. Officers and Technicians can mark a comment "Internal note" so it never reaches the Employee. All of the transition actions (approve, reject, assign, verify, reopen, cancel, rate, and so on) are opened from this page's action panel and are detailed as their own functions below, under Case Approval Workflow, Case Assignment Workflow, Case Verification and Closure and Case Cancellation and Rating.

| Status | Who can act | Available actions from this page |
| :---- | :---- | :---- |
| Pending | Officer (after claiming); Employee | Officer: Approve, Reject, Need more info, Cancel. Employee: Edit (EMP-04), Cancel |
| Need more info | Employee; Officer | Employee: Reply / edit and send back (EMP-04), Cancel. Officer: Cancel |
| Approved / Reopened | Officer; Technician (Reopened) | Approved: Assign technician, Cancel. Reopened: the Technician accepts or rejects again; the Officer can reassign or cancel |
| Assigned / Accepted / In Progress / On Hold | Officer | Reassign technician, Cancel |
| Completed | Officer | Verify and close, Reopen |
| Closed | Employee; Officer | Employee: Report "Not fixed" (within 7 days, once), Rate the service (once). Officer: Reopen or decline a "Not fixed" report |
| Rejected / Cancelled | — | Read only; reason shown below the header |

* Visibility: Employees see only their own cases, Technicians use TEC-03 for their own work orders instead of this page, Officers and Managers see all cases, the Administrator cannot open case content at all (BR-11).  
* A case claimed by another Officer shows an orange bar and disables the decision buttons for everyone else, with "Ask to release" (DLG-22).  
* Reason-based actions (Reject, Need more info, Reassign, Reopen, On Hold, Cancel) all require at least 10 characters of explanation.  
* Every action is written to the Timeline and the Audit Log (BR-16).

***Screens covered:** COM-08*

#### ***3.7.2 Case Approval Workflow***

**Function trigger:** Approve / Reject / Need more info buttons on Case Detail (COM-08) or the Case Queue (OFF-02), only for a Pending case claimed by the acting Officer.

**Function description:** The three decisions a Facility Officer can make on a newly submitted request: accept it as real work and set its final priority (Approve), refuse it with a reason (Reject), or ask the Employee a question first (Need more info).

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image33]

**Function Details:**

Approve sets the final priority (defaulting to the Employee's suggested urgency), starts the SLA resolve timer, and by default opens Assign Technician right away so approval and assignment normally happen in one flow while staying two separate, measurable steps (BR-20). Reject needs a reason type plus a 10–500 character explanation and ends the case permanently — it cannot later be reopened, only resubmitted as a new request. Need more info sends a required question to the Employee, releases the Officer's claim so any Officer can pick it up once answered, and triggers a 3-day reminder if the Employee does not reply.

* Only the Officer who currently claims the case can approve, reject or ask for information (claim rule; see also BR-02).  
* If the case was changed by someone else while the dialog was open, the dialog closes with a refresh message instead of silently saving.  
* A Reject with reason type "Duplicate" links to the other case for the Employee to see.

***Screens covered:** DLG-01, DLG-02, DLG-03*

#### ***3.7.3 Case Assignment Workflow***

**Function trigger:** Assign button (Approved or Reopened case) or Reassign button (Assigned onward) on Case Detail, Case Queue or Case Tracking; the claim dialog opens automatically on a conflict.

**Function description:** How the Facility Officer hands a case to a Maintenance Technician, changes that assignment later if needed, and how the system resolves two Officers wanting the same case at once.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image34]

**Function Details:**

Assign shows only active Technicians, sorted by matching specialty then by lowest current workload, with a due date pre-filled from the SLA target of the priority (editable but never into the past); assigning notifies the Technician and moves the case out of the To assign tab. Reassign requires a 10–300 character reason, keeps everything the previous Technician already entered (notes, photos) visible to the new one, and does not reset the SLA clock. Claim Conflict and Release covers claiming, releasing, an expired claim (after 30 minutes of inactivity) and the 5-claim-per-Officer limit, so only one Officer works a given Pending case at a time.

* One case has exactly one Technician at a time (one Case equals one Work Order, BR-05).  
* A yellow warning (not a block) appears when the chosen Technician already has 8 or more open work orders.  
* All Officers share a single queue and see every building (BR-19); there is no per-building assignment.

***Screens covered:** DLG-04, DLG-05, DLG-22*

#### ***3.7.4 Case Verification and Closure***

**Function trigger:** Verify and close / Reopen on Case Detail or the Verification Queue (OFF-04) for a Completed case; Decline on a case with a reopen request; Not fixed on a Closed case (within 7 days, once).

**Function description:** The quality-control loop that closes finished work or sends it back: only an Officer can close a case, and an Employee's own "not fixed" report is the trigger that lets an Officer reopen it.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image35]

**Function Details:**

Verify and Close requires a confirmation tick, closes the case (it can still be reopened through a "Not fixed" report within 7 days, BR-03), notifies the Employee and asks them to rate it, and offers to return the asset status to Active if it had been marked Under repair for this case. Reopen needs a 10–500 character reason and a new due date, sends the work back to the same Technician by default (BR-04) who must accept it again, and increments a reopen counter that feeds the reopen-rate KPI; a case already reopened 3 times shows a soft warning, not a block. Decline Reopen Request lets the Officer explain, with a required reason, why an Employee's "not fixed" report does not justify reopening — the case stays Closed and the Employee cannot ask again for the same case, only submit a new request.

* A Technician can never close their own work — only an Officer can (BR-13 boundary).  
* "Not fixed" is limited to once per case, within 7 days of closing (BR-03; the window is configurable in ADM-07).  
* If two Officers act on the same verification at once, the second sees "already closed by (name)".

***Screens covered:** DLG-06, DLG-07, DLG-08, DLG-13*

#### ***3.7.5 Case Cancellation and Rating***

**Function trigger:** Cancel on Case Detail, My Requests (Employee) or Case Tracking's row menu (Officer); Rate button on Case Detail, My Requests, Employee Home or the closing notification.

**Function description:** Two end-of-life actions available to the Employee and, for cancellation, also to the Officer: stopping a request before it finishes, and letting the Employee score the service once it is Closed.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image36]

**Function Details:**

An Employee can cancel their own request only while it is Pending or Need more info; once approved, the Cancel button disappears and the Employee must ask the Officer through a comment instead. An Officer can cancel a case while it is still open (not Completed, Closed, Rejected or Cancelled), with a required 10–300 character reason, and the assigned Technician is told. Rating is entirely optional, uses 1–5 stars with a short comment, is only offered after Closed, and cannot be changed once sent.

* Cancelled is a final status and the case is kept in history and reports.  
* Rating never blocks any workflow; it exists only to feed the Manager's average-rating KPI.

***Screens covered:** DLG-12, DLG-14*

### **3.8 Case Queue and Coordination (Officer)** {#3.8-case-queue-and-coordination-(officer)}

#### ***3.8.1 Officer Home and Case Queue***

**Function trigger:** First page after sign-in, or menu → Home (OFF-01); menu → Case Queue (OFF-02).

**Function description:** The Officer's daily landing page and the single shared queue where every new request arrives — all Officers see the same queue and can claim any case in it (BR-19).

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image37]

**Function Details:**

OFF-01 summarises what needs a decision today (to review, to assign, to verify, overdue, reopen requests), the Officer's own claimed cases with a Release option, the oldest waiting cases, cases stuck On Hold or rejected by a Technician, and preventive plans due this week. OFF-02 is the working queue itself, organised in tabs (To review, To assign, Reopen requests, Waiting for employee) with a quick preview panel so an Officer can claim, approve, reject, ask for information or assign without leaving the list.

* Claiming marks a case as being handled by that Officer; it auto-releases after 30 minutes of inactivity and is capped at 5 concurrent claims (BR-02).  
* New cases arriving while the queue is open show a non-disruptive "N new items. Show" bar rather than jumping the list.

***Screens covered:** OFF-01, OFF-02*

#### ***3.8.2 Case Tracking and Verification Queue***

**Function trigger:** Menu → Case Tracking, or menu → All Cases for the Manager (OFF-03); menu → Verification, or the "to verify" tile (OFF-04).

**Function description:** OFF-03 is the full list of every case in every status, used by the Officer to follow progress and find delays, and reused read-only by the Facility Manager (whose Screen Authorization hides the row menu) as the former "All Cases" view. OFF-04 lists work a Technician has marked Completed so an Officer can check it before verifying or reopening.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image38]

**Function Details:**

OFF-03 offers status tabs, rich filters (technician, building, priority, date, tags) and, for the Officer only, a row menu (Open, Reassign, Add comment, Cancel); overdue rows are flagged with a thin red bar. OFF-04's completed list shows a side-by-side preview of the Technician's report against the original request so the Officer can verify without opening the full Case Detail page, and links straight into the Verify and Close / Reopen dialogs described under Case Verification and Closure.

* The Manager's copy of this list is entirely read-only: no approval or assignment action exists there (BR-12).  
* The Officer reads what the Technician entered but never edits it directly.

***Screens covered:** OFF-03, OFF-04*

#### ***3.8.3 Create Case for an Employee***

**Function trigger:** "Create case for employee" button on Officer Home or the Case Queue.

**Function description:** Lets a Facility Officer log a problem reported by phone or in person on behalf of an Employee, using the same request fields as New Request (EMP-02) plus a requester picker.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image39]

**Function Details:**

The Officer searches and selects an active Employee as the requester, records how the problem was reported, fills the same place/asset/issue/description fields as EMP-02, and sets the priority directly since the Officer is the one deciding. An "Approve right away" tick box lets the Officer skip straight to Approved and into the To assign tab instead of creating a Pending case.

* The Timeline always records who entered the case and how it was reported, so provenance is never lost.  
* The Employee is notified even though they did not submit the request themselves.

***Screens covered:** OFF-05*

### **3.9 Work Order Execution (Technician)** {#3.9-work-order-execution-(technician)}

#### ***3.9.1 Technician Home and My Work Orders***

**Function trigger:** First page after sign-in, or menu → Home (TEC-01); menu → My Work Orders (TEC-02).

**Function description:** The Technician's daily overview and full worklist; both are scoped strictly to work orders assigned to the signed-in Technician — nobody sees another Technician's work here.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image40]

**Function Details:**

TEC-01 highlights new assignments waiting for Accept/Reject, today's preventive inspections, and the Technician's own monthly numbers (completed, average time, reopen count) without any comparison to other Technicians. TEC-02 lists every active work order in tabs (New, Accepted, In Progress, On Hold), sorted by priority then due date, with Accept/Reject on new assignments and Start/Continue elsewhere.

* A work order reassigned away while the Technician is viewing it disappears from the list with a toast explaining why.  
* Overdue work orders carry a red tag automatically once past the due date.

***Screens covered:** TEC-01, TEC-02*

#### ***3.9.2 Work Order Update and Completion***

**Function trigger:** Opened from TEC-01, TEC-02, TEC-04, a notification or search; Reject/Hold/Complete dialogs open from the buttons on this page once the required fields are valid.

**Function description:** Where the Technician records the real work done on site — accepting, starting, entering the cause, parts and hours, and marking the job completed — and the three confirmation dialogs that guard those transitions.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image41]

**Function Details:**

The available buttons follow the work order's status: Accept/Reject (Assigned or Reopened), Start work (Accepted), Put on hold/Mark completed (In Progress), Resume (On Hold). Progress is auto-saved every 30 seconds so little is lost if the session ends. Completing is only possible once root cause, labor hours (above 0\) and at least one evidence photo are filled in, and always passes through the Confirm Completion dialog (DLG-11) as a last check, since the report becomes read-only immediately afterward and only the Officer's verification can move the case further.

* A Technician can open only work orders assigned to them.  
* Reject Assignment (DLG-09) is only offered before Accept; after accepting, the only way out is Put on hold (DLG-10) or a comment to the Officer, never a reassignment by the Technician themselves.  
* The Technician can never change the original request text, priority or the assignment itself.  
* Completed does not mean Closed — the Officer still verifies (feeds into Case Verification and Closure).

***Screens covered:** TEC-03, DLG-09, DLG-10, DLG-11*

#### ***3.9.3 Work History***

**Function trigger:** Menu → Work History.

**Function description:** A read-only, filterable log of the Technician's own finished work orders, used to look back at what was done and, in particular, what was previously done on a given asset.

**Function Details:**

Summary tiles total completed count, labor hours and reopen count for the selected date range (default last 30 days); opening a row shows the same Work Order page (TEC-03) in read-only mode.

* Only the signed-in Technician's own history is ever shown.

***Screens covered:** TEC-04*

#### ***3.9.4 Preventive Inspection Execution***

**Function trigger:** From TEC-01 or TEC-02, on a work order tagged Preventive.

**Function description:** How a Technician carries out a scheduled preventive-maintenance checklist and records Pass/Fail/N/A for each item, distinct from a normal repair work order handled on TEC-03.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image42]

**Function Details:**

Every checklist item needs an answer before Complete Inspection is enabled; a Fail requires both a note and a photo. The overall result (Passed / Needs attention) is computed automatically. Completing the checklist creates the Inspection Record (PM-05/PM-06) and moves the work order to Completed for the Officer to verify, exactly like a repair work order.

* If any item is ticked Fail, the Technician can tick "Create a repair request" and a new Pending Case is opened automatically from the failed items (BR-07).  
* Preventive work orders skip the Pending review step entirely — they start Approved, or already Assigned when the plan has a default technician (BR-06).  
* The checklist shown is frozen to the plan version at the time the work order was created, even if the plan is edited afterward.

***Screens covered:** TEC-05*

### **3.10 Facility Manager Dashboard** {#3.10-facility-manager-dashboard}

#### ***3.10.1 Manager Home***

**Function trigger:** First page after sign-in for a Facility Manager, or menu → Home.

**Function description:** A read-only quick look at maintenance performance across every building, with KPI tiles, two charts and a "needs attention" list linking into the deeper Analytics and Case Tracking screens.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image43]

**Function Details:**

Every tile shows the change against the previous week with a word, not colour alone. "Needs attention" surfaces cases overdue more than 24 hours, cases On Hold more than 3 days, overdue preventive plans and assets with 3+ repairs in 30 days, each linking to the relevant detail page (COM-08, AST-03 or PM-03).

* The Manager has no approval or assignment action anywhere on this page — strictly read-only (BR-12).  
* Number definitions match the KPI Overview Dashboard exactly, so the two never disagree.

***Screens covered:** MGR-01*

### **3.11 Analytics and Reporting** {#3.11-analytics-and-reporting}

#### ***3.11.1 KPI Overview and Technician Performance***

**Function trigger:** Menu → Analytics (ANA-01, Officer and Manager); From ANA-01 or menu → Analytics → Technicians (ANA-02, Manager only).

**Function description:** The full performance dashboard (all cases, filterable by period and building) and a Manager-only breakdown of how each Technician is performing and how work is distributed between them.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image44]

**Function Details:**

ANA-01 shows KPI tiles (total cases, average response/resolution time, SLA met %, reopen rate, and average rating for the Manager) plus charts for created-versus-closed, status, issue type and resolution time against the SLA target line; every chart also offers a table view for accessibility. ANA-02 ranks Technicians by completed count, on-time rate, reopen rate, rating and current workload, with a fairness note that results based on fewer than 5 cases are unreliable.

* KPI definitions: first response \= Submitted to first Officer decision; resolution \= Approved to Completed; SLA met \= share finished inside the ADM-07 target.  
* The Officer's copy of ANA-01 omits the Rating tile and the cases-by-building comparison and cannot see ANA-02 at all (see 3.1.3).  
* Technicians never see another Technician's numbers.

***Screens covered:** ANA-01, ANA-02*

#### ***3.11.2 Failure Trends, Problem Assets and Report Export***

**Function trigger:** From ANA-01, MGR-01 or menu → Analytics → Failures (ANA-03); menu → Analytics → Reports, or an Export button on another analytics page (ANA-04).

**Function description:** ANA-03 surfaces which assets and issue types fail most often, to support preventive-maintenance decisions; ANA-04 turns any of these views into a downloadable PDF or Excel report for meetings and records.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image45]

**Function Details:**

ANA-03 lists the top 10 assets by case count and flags assets with 3+ cases in 30 days with a "Consider a preventive plan" hint that, for the Officer, links straight into Create/Edit Preventive Plan pre-filled with that asset. ANA-04 lets the Officer generate Case summary, Asset failure and Preventive compliance reports, and the Manager additionally Technician performance and Inspection records; a preview of the first 10 rows is shown before the file is created, and finished exports stay downloadable for 7 days.

* These screens only support decisions; the system never repairs, replaces or decides anything by itself.  
* Export is capped at 20,000 rows per file; every export is written to the Audit Log.  
* A report can only ever contain data the requesting role could already see on screen.

***Screens covered:** ANA-03, ANA-04*

### **3.12 User and Access Administration** {#3.12-user-and-access-administration}

#### ***3.12.1 Administrator Home***

**Function trigger:** First page after sign-in for an Administrator, or menu → Home.

**Function description:** A quick view of accounts and data quality: active/locked/never-signed-in user counts, users by role, data-quality warnings and the latest audit events, with quick links to add a user or an asset.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image46]

**Function Details:**

Data checks flag things like rooms with no assets, assets without a category, issue types never used or Technicians without a specialty, each linking straight to where it can be fixed; this page deliberately shows no case numbers, protecting case-content privacy for the Administrator role (BR-11).

* No warnings shown: "Your data looks complete."

***Screens covered:** ADM-01*

#### ***3.12.2 User Directory and Account Detail***

**Function trigger:** Menu → Users (ADM-02); clicking a user on ADM-02 or in search results (ADM-04).

**Function description:** ADM-02 is the searchable, filterable list of every account in the system; ADM-04 shows one account in full together with activity counts (not case content) and recent audit events, and both open the account-action dialog for sensitive changes.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image47]

**Function Details:**

ADM-02's row menu opens the confirm-style dialogs for reset password, unlock and deactivate/reactivate, and ADM-04's action buttons offer the same actions plus force sign-out; an Administrator's own row never offers Deactivate, so nobody can lock themselves out. ADM-04's activity summary is counts only (e.g. requests sent, work orders done) — the content of any case is never shown to the Administrator.

* At least one active Administrator must always exist; the last one cannot be deactivated.  
* Case content is never exposed to the Administrator anywhere in this feature (BR-11).

***Screens covered:** ADM-02, ADM-04*

#### ***3.12.3 Create/Edit User and Account Actions***

**Function trigger:** Add user on ADM-02 (create); Edit on ADM-02/ADM-04 (edit); the account-action row menu on ADM-02 or buttons on ADM-04 (DLG-16).

**Function description:** The only place accounts are created in SmartMaint — there is no self-registration (BR-14) — and the confirmation dialog for the sensitive actions an Administrator can take on an existing account.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image48]

**Function Details:**

Creating a user sets name, email (unique), role, optional department/employee code/phone and, for the Technician role only, one or more specialties used later to help Officers assign work; the system generates a temporary password that must be changed at first sign-in (AUTH-04), shown once and/or emailed. DLG-16 handles Reset password (forces a new temporary password and ends sessions), Unlock, Deactivate/Reactivate and Force sign-out; deactivating a Technician or reassigning their role is blocked while they hold active work orders, listed so the Administrator can reassign first.

| Field | Validation / behaviour |
| ----- | ----- |
| Full name | Required, 2–60 characters. |
| Email | Required, valid format, unique; locked after the account has history. |
| Role | Required, one of the five roles; changes take effect at the user's next request. |
| Specialty (Technician only) | One or more issue types, used to rank Assign Technician candidates. |
| Employee code | Optional but unique if given. |
| Temporary password | System-generated; shown once and/or emailed; never stored in readable form. |

* Removing the last Administrator, or changing/deactivating a Technician with open work, is blocked with a clear message naming what must be done first.  
* Every create/edit/action is written to the Audit Log with old and new values (BR-16).

***Screens covered:** ADM-03, DLG-16*

#### ***3.12.4 Roles and Permissions Reference***

**Function trigger:** Menu → Users → Roles tab, or the link on the role dropdown of Create/Edit User.

**Function description:** A read-only reference page, for the Administrator, that explains what each of the five fixed roles can do; there is nothing here to edit since the roles themselves are part of the system design.

**Function Details:**

A permission table (capability rows × role columns, cells Yes / View only / Own only / No) is generated from the same rules the system enforces at runtime, so it can never drift out of date, plus a one-line note per role about what the AI Assistant may answer for that role.

***Screens covered:** ADM-05*

### **3.13 System Configuration and Audit** {#3.13-system-configuration-and-audit}

#### ***3.13.1 Maintenance Categories***

**Function trigger:** Menu → Categories (ADM-06); Add item or Edit on ADM-06 (DLG-23).

**Function description:** Manages the three pick-lists used throughout the system — issue types (used in New Request), asset categories (used in Create/Edit Asset) and root causes (used in Work Order Update) — that only the Administrator can change.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image49]

**Function Details:**

Each list item can be deactivated (it disappears from choice lists but stays visible on old records) or deleted, but only while unused; drag-and-drop reordering saves immediately. DLG-23 is the small add/edit form for one item: a unique name, an optional description shown as help text, and an active switch.

* At least one active item must remain in Issue types and Asset types.  
* Deleting an item still in use is blocked with a count and a suggestion to deactivate instead (routed through DLG-17).

***Screens covered:** ADM-06, DLG-23*

#### ***3.13.2 SLA and Rules Settings***

**Function trigger:** Menu → SLA.

**Function description:** A single settings page where the Administrator adjusts SLA targets and the numeric thresholds used throughout the system's business rules, so operational behaviour can change without changing the software.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image50]

**Function Details:**

Editing here changes BR-08 (SLA targets), BR-02 (claim time-out and per-Officer claim limit), BR-03 (Employee reopen window) and BR-18 (session length), among others; Save is only active once something changed, and every change is written to the Audit Log with old and new values. Changes apply to new cases immediately and to open cases from the next calculation, not retroactively.

| Priority | First response target | Resolve target |
| :---- | :---- | :---- |
| Urgent | 1 hour | 4 hours |
| High | 4 hours | 24 hours |
| Normal | 8 hours | 72 hours |
| Low | 24 hours | 7 days |

* Working hours (default Mon–Fri 08:00–17:30) and holidays are configured here; the resolve timer pauses while a case is On Hold.  
* A resolve target must always be longer than the first-response target for the same priority; invalid values are rejected inline.

***Screens covered:** ADM-07*

#### ***3.13.3 Audit Log***

**Function trigger:** Menu → Audit Log, or links from Administrator Home and User Detail.

**Function description:** The append-only, searchable record of who did what and when across the whole system, including every sign-in, data change, status change, export and AI question (BR-16); it can be filtered and exported but never edited or deleted.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image51]

**Function Details:**

Each row shows time, user, role, a short action sentence, the affected object and the result; clicking a row opens a detail drawer with the exact old/new values, or, for AI events, only the user, the time, the scope used and the user's rating; the question and answer text is never shown (BR-11). CSV export is capped at 50,000 filtered rows per file.

* Log entries are kept for 12 months.  
* Case codes appear only as plain text — the Administrator cannot open the case from here (BR-11).

***Screens covered:** ADM-08*

### **3.14 Facility Structure and Asset Directory** {#3.14-facility-structure-and-asset-directory}

#### ***3.14.1 Facility Tree and Location Form***

**Function trigger:** Menu → Facilities & Assets (Administrator), or Assets (Officer, Manager) (AST-01); Add or Edit on AST-01 (DLG-15).

**Function description:** The tree of buildings, floors and rooms that every other screen's place picker draws from; the Administrator can create, edit and deactivate nodes, while the Officer and Manager can only browse it (BR-09).

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image52]

**Function Details:**

Each node shows a small asset/open-case count (case counts hidden from anyone who cannot see case content) and inactive places are greyed out but not removed. DLG-15 is the shared add/edit form for a building, floor or room: a required unique name and code, an optional room type/area, and notes.

* Deactivating a place with open cases is blocked, with the count shown.  
* A place with assets or history is never deleted, only deactivated (BR-10 principle applied to locations too).  
* Changing a code cascades automatically to old records that reference it.

***Screens covered:** AST-01, DLG-15*

#### ***3.14.2 Asset List and Asset Detail***

**Function trigger:** Menu → Assets (AST-02); opened from AST-01, AST-02, search, Case Detail, preventive pages, or a staff QR scan (AST-03).

**Function description:** The full inventory of assets with filters and role-specific actions, and the detailed page for one asset — information, maintenance history, preventive plans and failure statistics.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image53]

**Function Details:**

The Administrator can add, import, edit and retire assets; the Officer can view, change status and create a case for an asset; the Manager can only view (BR-09). Asset Detail's History and Statistics tabs, which surface case content, are hidden entirely from the Administrator, who sees only Overview and preventive-plan names. A red "Frequent failures" tag appears automatically for 3 or more repairs within 30 days, feeding the same signal shown in Failure Trends (ANA-03).

* Retired assets are hidden from the list by default, revealed with a "Show retired" switch.  
* Assets are never truly deleted once they have history — only retired (BR-10).

***Screens covered:** AST-02, AST-03*

### **3.15 Asset Lifecycle Management** {#3.15-asset-lifecycle-management}

#### ***3.15.1 Create/Edit Asset and Bulk Import***

**Function trigger:** Add asset / Edit on AST-02 or AST-03 (AST-04); Import button on AST-02 or Administrator Home (AST-05).

**Function description:** Administrator-only functions for adding assets one at a time with full validation, or in bulk from an Excel/CSV file, matching BR-09 (only the Administrator creates or edits assets).

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image54]

**Function Details:**

AST-05 runs as a four-step wizard — Upload, Check (a row-by-row preview marking problems such as an unknown room, duplicate code or missing type), Confirm (import only the good rows, or stop and fix the file) and Result — and the import itself is one all-or-nothing action for the rows actually imported: existing assets are never overwritten, and a duplicate code is always an error, never a silent update.

| Field | Validation |
| ----- | ----- |
| Asset code | Required, unique, 3–20 characters (letters, numbers, dash); a next code is suggested. |
| Name / Type | Name required 2–100 characters; Type required, from Maintenance Categories. |
| Place | Required: Building, Floor, Room in linked dropdowns; a deactivated room cannot be chosen. |
| Serial number | Optional, but must be unique if given. |
| Warranty end date | Optional; must not be before the purchase date. |
| Photo / Notes | Optional; photo up to 5 MB, notes up to 500 characters. |

* Moving an asset that has open cases is allowed but shows a warning listing them; those cases keep the old place text in their Timeline.  
* AST-05 caps a file at 5 MB and 1,000 rows; both AST-04 and AST-05 write to the Audit Log.

***Screens covered:** AST-04, AST-05*

#### ***3.15.2 Asset Status and Retirement***

**Function trigger:** Change status on AST-02/AST-03 (DLG-18, Officer); Retire on AST-02/AST-03, Deactivate/Delete on AST-01, or Delete on Maintenance Categories (DLG-17, Administrator).

**Function description:** The two ways an asset's operational state changes day to day: an informational status the Facility Officer sets (Active / Under repair / Out of service), and the Administrator-only, protected action of retiring an asset or removing a location/category.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image55]

**Function Details:**

DLG-18 is purely informational and never blocks an Employee from reporting a new problem on the asset, though setting Active while a case is still open shows a confirmation. DLG-17 is the shared "protect important data" confirmation used across the Asset and Configuration features: it lists any blocking open cases or active preventive plans (which disable Confirm until resolved) and, for retiring an asset or deleting a place, requires typing the code to confirm.

* Only the Administrator can retire an asset, and retiring is blocked while it has open cases (BR-10).  
* Anything with history is retired or deactivated, never truly deleted.  
* Both dialogs write to the Audit Log.

***Screens covered:** DLG-17, DLG-18*

#### ***3.15.3 QR Labels and QR Landing***

**Function trigger:** Print QR on AST-02/AST-03, or after creating an asset (AST-06); scanning a physical QR label, or opening its address directly (AST-07).

**Function description:** Generates printable QR labels for up to 100 assets at a time (AST-06), and the landing address those labels point to, which routes the scanning person to the right screen for their role (AST-07).

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image56]

**Function Details:**

AST-06 offers small/medium label sizes or an A4 sheet and produces a downloadable PDF or a browser print job; a retired asset in the selection is removed automatically with a note. AST-07 always requires sign-in first, then redirects: an Employee lands on New Request with place and asset pre-filled, Officer/Manager/Administrator land on Asset Detail, and a Technician lands on their open work order for that asset if one exists.

* The QR code itself encodes only a web address with the asset code, never personal data.  
* An unrecognised code, a retired asset, or a Technician with no matching work order each show a clear explanation instead of an error.

***Screens covered:** AST-06, AST-07*

### **3.16 Preventive Plan Management** {#3.16-preventive-plan-management}

#### ***3.16.1 Preventive Plan List, Create/Edit and Detail***

**Function trigger:** Menu → Preventive Maintenance (PM-01); New plan / Edit on PM-01 or PM-03 (PM-02); from PM-01, PM-04, AST-03, MGR-01 or a notification (PM-03).

**Function description:** How the Facility Officer defines and manages scheduled preventive-maintenance plans; the Manager can view every plan and its compliance, the Administrator does not use this module at all.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image57]

**Function Details:**

A plan defines a checklist (1–30 items; a note and a photo are required for every failed item), a frequency, a start/end date, a lead time before the due date, a priority and an optional default technician; PM-02 also previews the next 5 due dates before saving. PM-03 shows the plan's generated work-order history and two compliance tiles (on-time rate, passed rate over the last 12 months); ending a plan is permanent (duplicate it instead of reactivating).

| Field | Validation |
| ----- | ----- |
| Assets | Required, at least one; "add all of a type in a building" is offered. |
| Frequency | Required: fixed choices, or every N days/weeks/months. |
| Start date | Required, not in the past; End date optional (empty \= no end). |
| Lead time | Days before due date a work order is created; default 3 days. |
| Default technician | Optional; when set, generated work orders start Assigned instead of Approved. |
| Checklist | Required, 1–30 items, each 3–200 characters. |

* Work orders created from a plan skip the Pending review, because the plan itself is the approval (BR-06).  
* If an asset already has an open work order from the same plan, no duplicate is generated.  
* Only the Officer creates or edits plans; the Manager is view-only (BR-12).

***Screens covered:** PM-01, PM-02, PM-03*

#### ***3.16.2 Preventive Calendar and Manual Generation***

**Function trigger:** Calendar button on PM-01/Home (PM-04); Generate now on PM-01 or PM-03 (DLG-21).

**Function description:** A month/week calendar of upcoming and past preventive work for workload balancing, and the dialog that creates a plan's next work order immediately instead of waiting for its scheduled date.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image58]

**Function Details:**

Calendar events are colour- and icon-coded (upcoming, due soon, overdue, done) and clicking one opens the plan or the already-generated work order; the calendar itself is read-only — dates only change by editing the plan. Generate Now lists the plan's assets (all ticked by default, an asset with an already-open work order of the plan is greyed out and cannot be ticked) and creates one work order per remaining ticked asset, tagged "Preventive (manual)", without moving the plan's normal automatic schedule.

* Unavailable when the plan is paused or ended.  
* Every manual generation is written to the Audit Log.

***Screens covered:** PM-04, DLG-21*

#### ***3.16.3 Inspection Records and Detail***

**Function trigger:** Menu → Preventive Maintenance → Inspection Records (PM-05); from PM-05, PM-03 or AST-03 (PM-06).

**Function description:** A permanent, filterable history of every completed preventive inspection and the full checklist result of one of them, including any repair case it triggered.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image59]

**Function Details:**

PM-05 filters by date, asset, building, plan, technician and result (Passed / Needs attention). PM-06 shows every checklist item's answer, with failed items highlighted alongside their note and photos, and links to the originating work order, the plan and any repair Case that was auto-created from a failed item.

* Records are read-only for everyone and cannot be edited or deleted once created.

***Screens covered:** PM-05, PM-06*

### **3.17 Shared UI Utilities** {#3.17-shared-ui-utilities}

#### ***3.17.1 Photo Viewer and Unsaved-Changes Guard***

**Function trigger:** Clicking any photo thumbnail anywhere in the app (DLG-20); clicking a menu link, Cancel or Back on a form that has unsaved changes (DLG-19).

**Function description:** Two small, reusable dialogs shared by every screen in the application rather than being specific to any one feature: a full-size photo viewer with next/previous navigation, and a guard that stops typed data from being lost by accident.

**Screen layout:** mock-up of the screen (wireframe, the same block layout as the Screen Catalog).

![][image60]

**Function Details:**

The photo viewer supports zoom and keyboard navigation, shows who uploaded each photo and when, and offers a Download button only to the Officer and Manager; the same dialog also surfaces upload problems (wrong type, over 5 MB, too many photos, an interrupted upload). The unsaved-changes guard defaults its focus to "Keep editing" so pressing Enter never discards data by accident, and is skipped entirely for a form where nothing was typed.

* Photo limits: JPG/PNG/WEBP only, 5 MB each; 5 photos per Employee request, 10 per Technician work order (BR-17).  
* Closing the browser tab shows the browser's own native warning instead, which the application cannot restyle.

***Screens covered:** DLG-19, DLG-20*

### **3.18 Non-Screen Functions** {#3.18-non-screen-functions}

#### ***3.18.1 Create AI Prediction***

**Function trigger:** The user sends a question in the AI Assistant (COM-07).

**Function description:** Non-screen function of the feature "AI-assisted support (RBAC-scoped)", run by the backend without user interface. Receives a question from a user, adds context filtered by the same RBAC scope as the user's role, calls Replicate to create a prediction and returns the prediction id immediately (Replicate is asynchronous and does not return the answer at once).

**Function details:** Related business rules: BR-15, BR-16.

#### ***3.18.2 Poll / Webhook AI Result***

**Function trigger:** Replicate calls the webhook when the prediction finishes, or the chat window checks the status every 2 seconds.

**Function description:** Non-screen function of the feature "AI-assisted support (RBAC-scoped)", run by the backend without user interface. Receives the result from Replicate by webhook (or the client polls the status every 2 seconds), stores the answer in AIConversation and writes the Audit Log both when the request is created and when the result arrives.

**Function details:** Related business rules: BR-15, BR-16.

#### ***3.18.3 Auto-release Claim***

**Function trigger:** Background check of claims that had no action for 30 minutes.

**Function description:** Non-screen function of the feature "Case handling", run by the backend without user interface. Background job: when an Officer claims a case but does nothing for 30 minutes, the claim is released back to the shared queue. One Officer can hold at most 5 claims. Both numbers are set in ADM-07.

**Function details:** Related business rules: BR-02.

#### ***3.18.4 SLA Timer & Overdue Check***

**Function trigger:** Background check of all open cases against their SLA target, and every status change.

**Function description:** Non-screen function of the feature "Case handling", run by the backend without user interface. Follows sla\_target\_at of every open case according to its priority, in working hours; the timer pauses while the case is On Hold. Close to the target the Officers are notified; when overdue the case is flagged as overdue and the Manager is also notified.

**Function details:** Related business rules: BR-08.

#### ***3.18.5 Auto-generate Preventive Work Order***

**Function trigger:** A preventive plan reaches its lead time before the due date (default 3 days), or the Officer uses Generate Preventive Work Order Now (DLG-21).

**Function description:** Non-screen function of the feature "Preventive maintenance", run by the backend without user interface. When a preventive plan reaches its lead time before the due date (default 3 days), creates a new work order that skips the Pending review and starts as Approved (or Assigned when the plan has a default Technician).

**Function details:** Related business rules: BR-06.

#### ***3.18.6 Inspection Failure to New Case***

**Function trigger:** The Technician submits an inspection checklist (TEC-05) with a failed item ticked for follow-up.

**Function description:** Non-screen function of the feature "Preventive maintenance", run by the backend without user interface. When a Technician ticks a failed checklist item for follow-up, creates a new repair Case (Pending) linked to the inspection record.

**Function details:** Related business rules: BR-07.

#### ***3.18.7 Notification Dispatch***

**Function trigger:** Any event listed in 5.4.2 Notifications.

**Function description:** Non-screen function of the feature "Notification", run by the backend without user interface. Sends in-app and email notifications at the case milestones listed in 5.4.2 (new request, approve/reject/need info, assign, hold, complete, close, reopen, cancel, SLA warnings) and account events.

**Function details:** Email can be turned off per kind in COM-04, except for important events.

#### ***3.18.8 Audit Logging***

**Function trigger:** Every sign-in, data change, status change, export and AI question.

**Function description:** Non-screen function of the feature "Audit", run by the backend without user interface. Writes every sign-in, data change, status change, export and AI question to the Audit Log. Records cannot be edited or deleted.

**Function details:** Related business rules: BR-11, BR-16.

## **4\. Non-Functional Requirements** {#4.-non-functional-requirements}

### **4.1 External Interfaces** {#4.1-external-interfaces}

SmartMaint communicates with its users through a web application and a mobile application, and with other software through the following interfaces.

| Interface | Description |
| ----- | ----- |
| User interface | Web application built with React \+ Vite for desktop browsers (1280 px and wider; below 1024 px the side menu becomes a hamburger button and tables scroll sideways). Mobile application built with Flutter, with the same functions for all five roles. English and Vietnamese are available and the user chooses in Personal Settings. |
| Backend API | RESTful HTTP/JSON API implemented in .NET (C\#), used by both the web and the mobile application. Every request carries a JWT and is checked against the role permissions (RBAC). |
| Database and file storage | Supabase: managed PostgreSQL for all data and Supabase Storage for photos and attachments. Authentication and authorization are implemented in the backend, not with Supabase Auth. |
| AI service (Replicate) | Asynchronous API: the backend creates a prediction and receives a prediction id at once; the result is received later by webhook or by polling. The question is sent together with context already filtered by the user's RBAC scope. If the service is unavailable, only the AI Assistant shows an error; the rest of the system keeps working. |
| Email | Email notifications (account created, password reset, locked account, and case events the user has not turned off) are sent through an email delivery service. |
| Hosting | Backend API hosted on Railway, web application hosted on Vercel; the mobile application calls the same backend API; all traffic uses HTTPS. |
| Hardware | No special hardware. The mobile camera is used for photos and QR scanning; any office printer can print QR labels. |

### **4.2 Quality Attributes** {#4.2-quality-attributes}

The quality attributes below are the targets for release 1.0, measured on the demo environment with seed data.

#### ***4.2.1 Usability*** 

* Every list has a search box, filters, sortable columns and 20 rows per page with the total count; clicking a row opens its detail.  
* A status is always shown with colour, icon and text together, using the same words and colours on every screen.  
* Messages are short and say what happened and what to do next, without technical codes; buttons start with a verb.  
* Successful actions show a green message for 4 seconds; form errors appear in red under the field and the page scrolls to the first error.  
* Every role starts on its own Home page with number tiles and the tasks to do now.  
* Everything can be used with the keyboard, focus is always visible and icons have text labels.

#### ***4.2.2 Reliability*** 

* Each block of a page loads on its own, so one failing block does not break the whole page; a page that cannot load shows ERR-03 and a lost connection shows the offline bar (ERR-04).  
* Submit buttons are disabled while sending, so an action is never saved twice; if sending fails, everything typed is kept.  
* When two people work on the same case, the second person is warned and decision buttons are disabled until the page is refreshed; claiming a case is decided by the first click.  
* If the AI service is slow or unavailable, the AI Assistant shows a clear message and the core case workflow is not affected.  
* Audit Log records cannot be changed; data with history is deactivated or retired instead of deleted.

#### ***4.2.3 Performance***

* Normal operations (sign in, open a case list, create a case) respond in less than 2 seconds on the demo environment, checked with the browser developer tools (web) and Flutter DevTools (mobile).  
* An AI question is accepted in less than 2 seconds (the prediction id is returned at once); the answer is shown when ready, and after 60 seconds the user is told that it is taking longer.  
* Lists and the notification bell refresh by themselves about every 30 seconds without making the list jump.  
* Photo uploads are limited to 5 MB per file and imports to 5 MB and 1,000 rows per file.

#### ***4.2.4 Security***

* Authentication with JWT; authorization by role (RBAC) for the five roles on every API call and screen.  
* Passwords are stored only as hashes and are never shown; password and lock rules follow BR-18.  
* All traffic uses HTTPS; secrets are not stored in the source code.  
* The AI Assistant only receives data the user's role may already see; answers reported as showing forbidden data are flagged to the Administrator as an Audit Log event without the question and answer text.  
* The Administrator cannot read case content; QR codes contain only a web address with the asset code, never personal data.

#### ***4.2.5 Maintainability***

* The code is organised by domain module (authentication, user, case, asset, preventive maintenance, analytics, AI) with separate controller, service and data-transfer layers.  
* Role checks are implemented once as shared authorization rules, not repeated by hand in each function.  
* SLA targets and rule values (claim timeout, reopen window, session length) are settings in ADM-07, so they can change without changing the software.  
* A common code style is enforced with linters/formatters and every change is reviewed through a GitHub Pull Request.

## **5\. Requirement Appendix** {#5.-requirement-appendix}

This part lists the business rules, the common requirements that apply to every screen, the application messages and other requirements (Case life cycle and notifications).

### **5.1 Business Rules** {#5.1-business-rules}

The business rules below come from the decisions agreed by the team in the Screen Catalog and the project context; numeric defaults (time limits, counts) are proposals to be confirmed with the supervisor and can be changed in ADM-07; the functions in 3.2 to 3.18 refer to them by ID.

| ID | Rule Definition |
| ----- | ----- |
| BR-01 | The Employee only suggests an urgency (Low, Normal, High, Urgent); the Facility Officer sets the final priority when approving. |
| BR-02 | A claimed case with no action for 30 minutes is released automatically to the shared queue; one Officer can hold at most 5 claimed cases. |
| BR-03 | The Employee can report "Not fixed" only within 7 days after a case is Closed, and only once per case. |
| BR-04 | A reopened case goes back to the same Technician (unless the Officer changes the person); the Technician must accept it again. |
| BR-05 | One Case equals one Work Order in release 1.0 (they are not split into two objects). |
| BR-06 | Preventive work orders created from a plan skip the Pending review and start as Approved (or Assigned when the plan has a default Technician); they are verified by the Officer like other cases. |
| BR-07 | A failed item in a preventive inspection can automatically create a new repair Case (Pending) when the Technician ticks the box. |
| BR-08 | Default SLA per priority (first response / resolution): Urgent 1 h / 4 h, High 4 h / 24 h, Normal 8 h / 72 h, Low 24 h / 7 days, counted in working hours; the resolution timer pauses while the case is On Hold. Values are editable in ADM-07. |
| BR-09 | Only the Administrator creates, edits and deactivates buildings, floors, rooms and assets; the Facility Officer can only view assets, change their status and print QR labels. |
| BR-10 | Only the Administrator can retire an asset, and retiring is blocked while the asset has open cases. Anything with history is retired or deactivated, never really deleted. |
| BR-11 | The Administrator cannot open the content of cases or of AI conversations and only sees case codes as plain text in the Audit Log; there are no analytics screens for the Administrator. |
| BR-12 | The Facility Manager is read only in the whole system: no approval, no assignment, no cost approval. |
| BR-13 | A Technician can reject a work order only before accepting it; after that only On Hold or comments are possible. A Technician cannot reassign or close work. |
| BR-14 | Accounts are created or invited only by the Administrator; there is no self-registration. |
| BR-15 | The AI Assistant accepts at most 20 questions per hour per user and 500 characters per question; its answers are limited to the RBAC scope of the user's role. |
| BR-16 | Every sign-in, data change, status change, export and AI question is written to the Audit Log; audit records cannot be edited. |
| BR-17 | Photos must be JPG, PNG or WEBP and at most 5 MB each; an Employee can attach up to 5 photos per request and a Technician up to 10 per work order. |
| BR-18 | Passwords need at least 8 characters with uppercase, lowercase and a number; 5 wrong attempts lock the account for 15 minutes; a reset link is valid for 30 minutes; a session lasts 8 hours. |
| BR-19 | All Officers share one case queue and all Officers, Technicians and Managers see all buildings; there is no per-building assignment of staff. |
| BR-20 | Approve and Assign are two separate steps (Approved, then Assigned) so that waiting time before assignment can be measured. |

### **5.2 Common Requirements** {#5.2-common-requirements}

The requirements below apply to every screen of the web application; the function details in 3.2 to 3.18 only describe what differs from them.

| ID | Requirement |
| ----- | ----- |
| CR-01 | Lists (tables): Every list has: a search box, filters shown as small chips, sortable columns, 20 rows per page with page numbers and the total count, a loading state (grey rows), an empty state with a friendly sentence, an "empty after filtering" state with Clear filters, and a red banner with Retry when loading fails. Clicking a row opens its detail. |
| CR-02 | Forms: Required fields are marked with a star. Errors appear in red under the field, in words that say what to do (not codes). Checks happen when the user leaves a field and again on submit; the page scrolls to the first error. The submit button is disabled while sending so nothing is sent twice. If sending fails, a red banner appears and everything typed is kept. Successful actions show a green toast for 4 seconds. A form with unsaved changes asks DLG-19 before leaving. |
| CR-03 | Loading, empty and error: Each block of a page loads on its own, so one failing block does not break the page. Loading shows grey placeholder shapes, not spinners over the whole page. A page that cannot load at all shows ERR-03; a lost connection shows the offline bar (ERR-04). |
| CR-04 | Status labels: A status is always shown with colour, a small icon and text together, never colour alone. The same words and colours are used on every screen. |
| CR-05 | Permissions and hidden buttons: A button that a role must never use is hidden, not greyed out. A button that is normally allowed but not right now (for example wrong status) is greyed out with a short tooltip that says why. Opening an address without permission shows ERR-01, and the attempt is logged. |
| CR-06 | Two people at the same time: If another person changes a case while you have it open, a blue bar says so and decision buttons are disabled until the page is refreshed. Claiming a case is decided by who clicks first (DLG-22). |
| CR-07 | Photos and files: JPG, PNG or WEBP, 5 MB each. Employee up to 5 photos per request, Technician up to 10 per work order. Drag and drop or click; a thumbnail with a progress bar is shown; failures show a red mark and Retry. Clicking a photo opens DLG-20. Imports accept .xlsx and .csv up to 5 MB. |
| CR-08 | Dates, times and language: Times are shown in the user's own time zone. Recent times are relative ("2 hours ago") with the exact time on hover. The user chooses English or Vietnamese in Settings (COM-04); all messages in this document exist in both languages. |
| CR-09 | Live refresh: Lists and the bell refresh by themselves about every 30 seconds. Changes appear with a short highlight; lists never jump while the user is reading (a bar "3 new items. Show" appears instead). |
| CR-10 | Audit and safety: Every sign-in, every change of data, every status change, every export and every AI question is written to the Audit Log. Passwords are never shown or stored in readable form. Case content is not shown to the Administrator. |
| CR-11 | Screen size and access: The web application is designed for desktop (1280 px and wider) and the Flutter mobile application for phone screens, with the same functions. Below 1024 px the side menu becomes a hamburger button and tables scroll sideways. Everything can be used with the keyboard; focus is always visible; text and colour contrast are readable; icons have text labels. |
| CR-12 | Wording: Short, kind and clear. Say what happened and what to do next. No technical words or codes in messages. Buttons start with a verb ("Send request", not "OK"). |

### **5.3 Application Messages List** {#5.3-application-messages-list}

Messages are shown in the language chosen by the user (English or Vietnamese). Words in braces are filled in by the system.

| \# | Message code | Message Type | Context | Content |
| :---- | :---- | :---- | :---- | :---- |
| 1 | MSG01 | In red, under the text box | A required field is empty | Please enter your {field}. |
| 2 | MSG02 | In line | Wrong email or password when signing in | Email or password is incorrect. |
| 3 | MSG03 | In line | 5 wrong sign-in attempts | Too many attempts. Try again in 15 minutes or contact your Administrator. |
| 4 | MSG04 | Red banner | The server cannot be reached | Cannot reach the server. Please try again. |
| 5 | MSG05 | In line | Password reset link expired or already used | This link is no longer valid. |
| 6 | MSG06 | In red, under the text box | New password and confirmation are different | Passwords do not match. |
| 7 | MSG07 | In red, under the text box | New password is the same as the current password | The new password must differ from the current password. |
| 8 | MSG08 | In red, under the upload box | Photo is too big or has a wrong type | Each photo must be JPG, PNG or WEBP under 5 MB. |
| 9 | MSG09 | In red, under the upload box | More photos than allowed | You can add up to {N} photos. |
| 10 | MSG10 | Red banner | Submitting a request fails | Could not send. Your text and photos are kept. Retry. |
| 11 | MSG11 | Toast message | An action is saved successfully | {Action} successfully. |
| 12 | MSG12 | Blue bar | Someone else changed the open case | This case was updated by {name}. Refresh to see the latest. |
| 13 | MSG13 | Orange bar | The case is claimed by another Officer | Being handled by {name} since {time}. |
| 14 | MSG14 | In line | Two Officers claim the same case at the same moment | Already taken. |
| 15 | MSG15 | In chat | The AI answer takes more than 60 seconds | This is taking longer than usual. |
| 16 | MSG16 | In chat | The AI service is not available | The assistant is not available right now. |
| 17 | MSG17 | In chat | The AI question is outside the role scope | I cannot help with that for your role. You can ask about ... |
| 18 | MSG18 | In chat | The AI limit of 20 questions per hour is reached | You reached the limit. Try again in {N} minutes. |
| 19 | MSG19 | In red, under the upload box | Import file has a wrong type or is too big | Please upload an .xlsx or .csv file under 5 MB. |
| 20 | MSG20 | In line | Import file has more than 1,000 rows | Too many rows. Please split the file. |
| 21 | MSG21 | Hint text | The connection is lost | No connection |
| 22 | MSG22 | In line | A list is empty after filtering | No results match your filters. Clear filters |
| 23 | MSG23 | In line | The case was already closed by another Officer | This case was already closed by {name}. |
| 24 | MSG24 | In line | A list, panel or chart fails to load | Could not load. Retry |
| 25 | MSG25 | Hint text | The data checks find no problem | Your data looks complete. |

### **5.4 Other Requirements** {#5.4-other-requirements}

#### ***5.4.1 Case Status Life Cycle***

The table below lists the twelve statuses of a Maintenance Case used by all screens, business rules and reports.

| \# | Status | Meaning |
| :---- | :---- | :---- |
| 1 | Pending | A new request waiting for the Officer's decision. |
| 2 | Need more info | The Officer asked the Employee a question. |
| 3 | Approved | Accepted as real work, waiting for a Technician. Also used for new preventive work orders and for assignments a Technician rejected. |
| 4 | Assigned | Given to a Technician who has not answered yet. |
| 5 | Accepted | The Technician agreed but has not started yet. |
| 6 | In Progress | The Technician is working on site. |
| 7 | On Hold | Work is paused. The reason and the expected date are shown. |
| 8 | Completed | The Technician says the work is done; the Officer has not checked yet. |
| 9 | Closed | The Officer verified the work. This is the normal end. |
| 10 | Reopened | Sent back to the Technician after a failed check or a report from the Employee. |
| 11 | Rejected | The Officer refused the request. Final. |
| 12 | Cancelled | Stopped by the Employee (before approval) or by an Officer. Final. |

Normal path: Pending, Approved, Assigned, Accepted, In Progress, Completed, Closed. Side paths: Need more info (back to Pending), Rejected, Cancelled, On Hold (back to In Progress), Reopened (after a failed verification or an accepted "Not fixed" report, back to the same Technician).

Who moves each status: Pending: the Officer who claims the case approves it (Approved), rejects it (Rejected), asks for more information (Need more info) or cancels it; the Employee can still edit it (stays Pending) or cancel it. Need more info: the Employee sends the answer back (Pending) or cancels; the Officer can cancel. Approved: the Officer assigns a Technician (Assigned) or cancels. Assigned and Reopened: the Technician accepts (Accepted) or rejects (back to Approved); the Officer can reassign or cancel. Accepted: the Technician starts the work (In Progress). In Progress: the Technician puts the work on hold (On Hold) or reports it done (Completed). On Hold: the Technician continues (In Progress). While a case is Accepted, In Progress or On Hold, the Officer can also reassign or cancel it. Completed: the Officer verifies and closes it (Closed) or reopens it (Reopened). Closed: the Employee can rate the service and, once within 7 days, report "Not fixed"; the case stays Closed with a reopen request until the Officer reopens it (Reopened) or declines the report (stays Closed). Rejected and Cancelled: no further action; the Employee can submit a new request.

#### ***5.4.2 Notifications***

The table below lists when the system sends notifications (non-screen function Notification Dispatch).

| Event | Who is notified | Screen opened |
| :---- | :---- | :---- |
| New request submitted, or entered by an Officer | All Officers; the Employee, when an Officer entered the request for them | COM-08 (OFF-02) |
| Request approved | Employee | COM-08 |
| Request rejected, with the reason | Employee | COM-08 |
| More information needed (and a reminder after 3 days) | Employee | EMP-04 |
| Employee sent the answer back | All Officers | COM-08 |
| Work assigned or reassigned | The new Technician; the previous Technician is told the work was taken back | TEC-03 |
| Technician accepted (in the app only) | The Officer who assigned it | COM-08 |
| Technician rejected the assignment | All Officers | OFF-02 (To assign) |
| Work put on hold | All Officers, Employee | COM-08 |
| Work marked completed | All Officers | OFF-04 |
| Case closed (with an invitation to rate) | Employee | COM-08 |
| Case reopened, or a "Not fixed" report declined | Technician and Employee (reopened); Employee (report declined) | TEC-03, COM-08 |
| Employee reports "Not fixed" | All Officers | OFF-02 (Reopen requests) |
| Case cancelled | People involved: Employee, Officer, Technician | COM-08 (TEC-03 for the Technician) |
| New comment (internal notes only reach staff) | Other people on the case | COM-08 (TEC-03 for the Technician) |
| Case close to its SLA target, or overdue | Officers; Manager when overdue | COM-08 |
| Preventive work order created | Officers, or the default Technician of the plan | COM-08, TEC-05 |
| Preventive work overdue | Officers, Manager | PM-03 |
| Account created, password reset or changed, account locked (by email) | The user | AUTH-01 |
| An AI answer was reported as showing information the user should not see | Administrator (flagged in the Audit Log) | ADM-08 |























































































































