  
**SMARTMAINT**

**SmartMaint Web Application: Screen Catalog**

Smart Corporate Facility Maintenance Management Platform (FA26SE147)

Screen specification, Web only, all roles, including error and exception cases

| Version | Version 1.0 (draft for team review) |
| :---- | :---- |
| **Date** | 21 September 2026 |
| **Team** | Group SmartMaint (SE147) |
| **Screens** | 81 (58 pages, 23 dialogs) |
| **Language** | English (a Vietnamese copy is available) |

**Contents**

[**1\. About this document**](#bookmark=id.bt4cisy2jo51)**3**

[**2\. Decisions and assumptions**](#bookmark=id.g0uc4f92k7wk)**4**

[2.1 Decisions confirmed by the Leader](#bookmark=id.czetj0namf80)4

[2.2 Assumptions that need your confirmation](#bookmark=id.916b5nlgog5g)5

[2.3 Things in existing work that must change](#bookmark=id.2dwrnc3zx3ce)6

[**3\. Glossary (plain words)**](#bookmark=id.6ujxclmplq3z)**7**

[**4\. The five roles**](#bookmark=id.1zxeark8zd7w)**8**

[**5\. Case life cycle**](#bookmark=id.375h1mpe5tj4)**9**

[5.1 Statuses, screens and actions](#bookmark=id.2xmxluv8vg7m)9

[5.2 Notifications](#bookmark=id.j0zzawca10k3)10

[**6\. Screen inventory**](#bookmark=id.qx3enqddq97i)**11**

[**7\. Permission table (Screen × Role)**](#bookmark=id.gibvwov1hri8)**15**

[**8\. Navigation maps**](#bookmark=id.ku3zsh65w2dk)**18**

[8.1 Sign-in and shared screens (all roles)](#bookmark=id.saelj4p2rj8i)19

[8.2 Employee](#bookmark=id.hguq8eifjjyn)20

[8.3 Facility Officer: handling cases](#bookmark=id.qp3ef7ixv6gm)21

[8.4 Facility Officer: assets, preventive maintenance, analytics](#bookmark=id.8k7qwcmyf4hz)22

[8.5 Maintenance Technician](#bookmark=id.ak7iib2tntf8)23

[8.6 Facility Manager](#bookmark=id.g4ewxcaocnz)24

[8.7 Administrator](#bookmark=id.g8ysvsg0me6v)25

[**9\. Rules that apply to every screen**](#bookmark=id.6zsrytz9a7nd)**26**

[**10\. Screen specifications**](#bookmark=id.y1fjmeuqh88r)**28**

[10.1 Account and sign-in (AUTH)](#bookmark=id.vg1bus2he641)28

[10.2 Error pages (ERR)](#bookmark=id.enqshob2i199)36

[10.3 Shared screens (COM)](#bookmark=id.h5halefsl77m)40

[10.4 Employee (EMP)](#bookmark=id.1fk5ammgiu3h)52

[10.5 Facility Officer (OFF)](#bookmark=id.wxr3wt7xeqvf)58

[10.6 Maintenance Technician (TEC)](#bookmark=id.eej0wyvwrtxw)65

[10.7 Facility Manager (MGR)](#bookmark=id.5d9niimym6ku)72

[10.8 Administrator (ADM)](#bookmark=id.1f67ci1l7mev)75

[10.9 Assets and facilities (AST)](#bookmark=id.yk27io58qenj)85

[10.10 Preventive maintenance (PM)](#bookmark=id.p8s3xejr4gp9)95

[10.11 Analytics (ANA)](#bookmark=id.ls2y8x1obnh9)103

[10.12 Dialogs (DLG)](#bookmark=id.dci8p7g6xza)109

**Part A. Overview**

# **1\. About this document**

**Purpose**

This document lists every screen of the SmartMaint web application, for all five roles. It describes not only the normal path (happy path) but also refusals, mistakes, empty lists, loading problems, missing permissions and other exceptions. Use it to write the SRS (Report 3), to design the interface, to code the web app, and to share the same picture with the whole team (Tùng for mobile, Huy for tests and user guide, Hùng for review).

**Scope**

Web application only (React). Mobile screens are not described here, but every web screen ID can be reused as the reference for the matching mobile screen. Screens were derived from PROJECT\_CONTEXT.md and the answers of the Leader on 21 September 2026, not copied from the HTML prototype v5.

**How screens are numbered**

Each screen has a code made of a module prefix and a number, for example [EMP-02](#bookmark=id.v4er16ngh3i9). Prefixes: AUTH sign-in and account, ERR error pages, COM shared screens, EMP Employee, OFF Facility Officer, TEC Technician, MGR Manager, ADM Administrator, AST assets and facilities, PM preventive maintenance, ANA analytics, DLG dialogs (small windows that appear on top of a page). The same code is used in both language versions.

**How each screen is described**

Every screen has the same parts: a summary table (type, who can open it, where it comes from), the purpose, a block layout picture, what is on the screen, what the user can do, situations and error cases, rules, and where the user goes next.

# **2\. Decisions and assumptions**

## **2.1 Decisions confirmed by the Leader**

These points were decided by the Leader in the question rounds before this document was written. The screens follow them.

| No. | Decision | What it changes on screens |
| ----- | :---- | :---- |
| 1 | Only the web app is described. The document is built from the context, not from the HTML prototype v5. | All screens are new. Differences with the prototype are listed in 2.3. |
| 2 | Accounts are created or invited by the Administrator. There is no self-registration. | No Register screen. Added [AUTH-04](#bookmark=id.kdpgutl9nzf5) (forced first-time password change) and [ADM-03](#bookmark=id.l5d1kw4uy1i1). |
| 3 | Account screens beyond login: forgot and reset password, change password in Profile, forced change at first login, session expired and account locked. | [AUTH-02](#bookmark=id.qau4lqkd5b9f), [AUTH-03](#bookmark=id.kzmn6civei1f), [AUTH-04](#bookmark=id.kdpgutl9nzf5), [AUTH-05](#bookmark=id.dy9vq2ywyc10), [AUTH-06](#bookmark=id.nyi20vb1h2mm), [COM-03](#bookmark=id.gpq9qcuy2jzw). |
| 4 | Approve and Assign are two separate steps. There is an On Hold status. | [DLG-01](#bookmark=id.nqyaa9ctr6re) (Approve) and [DLG-04](#bookmark=id.6bnps7bwaz0o) (Assign) are separate; [DLG-10](#bookmark=id.g5i37a8no9xn) for On Hold. A tick "Assign next" keeps it quick. |
| 5 | The Manager can only view. No approval of any cost. | [MGR-01](#bookmark=id.fxg093g2jt85), [OFF-0](#bookmark=id.n0o9xiciqqd7)3 [(when opened by the Manager)](#bookmark=id.n0o9xiciqqd7) and ANA screens are read only. No cost field exists in Case or Work Order; only parts and labor hours. |
| 6 | Case extras: Priority, comments, Employee rating after closing, Employee cancel and "Not fixed". | [COM-08](#bookmark=id.83jeomyy0mjj), [DLG-12](#bookmark=id.ck085r6ldanm), [DLG-13](#bookmark=id.3h2jh6omhwpq), [DLG-14](#bookmark=id.qg8dy46jzlj2); Priority in [DLG-01](#bookmark=id.nqyaa9ctr6re). |
| 7 | Only the Administrator creates, edits and deletes Building, Room and Asset. The Officer views and changes an asset's status. | [AST-01](#bookmark=id.r397yhsl2mmw), [AST-04](#bookmark=id.615v8i8ga3hr), [AST-05](#bookmark=id.xz1mbdpv5xr3) for the Administrator; [DLG-18](#bookmark=id.wwutpqpzg3ck) for the Officer. |
| 8 | Asset extras: QR code, tree view of places, import from CSV or Excel. | [AST-01](#bookmark=id.r397yhsl2mmw), [AST-05](#bookmark=id.xz1mbdpv5xr3), [AST-06](#bookmark=id.9e47xaty55ny), [AST-07](#bookmark=id.z84288y2n6n5). |
| 9 | Preventive maintenance extras: technician checklist, Inspection Records screen, calendar view. | [TEC-05](#bookmark=id.ff0ok48aee2q), [PM-04](#bookmark=id.xzhz6y8fmacz), [PM-05](#bookmark=id.j0baje6voyco), [PM-06](#bookmark=id.dk34aamyou4v). |
| 10 | AI: floating chat window on every screen, with 👍 / 👎 rating for each answer. No full-page AI screen and no "Ask AI" buttons inside cases. | Only [COM-07](#bookmark=id.su5epyb3mo4x) (with waiting, error and refusal states because the AI works in two steps). |
| 11 | Analytics: overview dashboard, technician performance, failure trends and problem assets, export to PDF or Excel. | [ANA-01](#bookmark=id.qschmq5ovp16) to [ANA-04](#bookmark=id.vuzx748pj0j4). "Repair or replace" is not a separate screen. |
| 12 | Administrator extras: maintenance categories, Audit Log, SLA settings. No per-building assignment of Officers or Technicians. | [ADM-06](#bookmark=id.a8n26oa6i98t), [ADM-07](#bookmark=id.6ynr8feqi2ao), [ADM-08](#bookmark=id.382mbmgltcob). All Officers share one queue. |
| 13 | Shared screens: notification center, global search with filters, error pages, personal settings. | [COM-04](#bookmark=id.xs734q1rwfk9), [COM-05](#bookmark=id.k3ei9nwbznrg), [COM-06](#bookmark=id.npycecq01gq2), [ERR-01](#bookmark=id.lmhdh2gy047x) to [ERR-04](#bookmark=id.z17yjcyldi2q). |
| 14 | Officer behaviour: Claim button, workload shown when assigning, reassign after assigning, creating a case for an Employee. | [DLG-22](#bookmark=id.60m6ytailped), [DLG-04](#bookmark=id.6bnps7bwaz0o), [DLG-05](#bookmark=id.pqnz92ls7h1u), [OFF-05](#bookmark=id.gtd6k0343759). |
| 15 | Every role has its own Home page with number tiles and things to do now. | [EMP-01](#bookmark=id.gevp1q7fiais), [OFF-01](#bookmark=id.eui2u2y7lh0t), [TEC-01](#bookmark=id.3lbqglcq4he0), [MGR-01](#bookmark=id.fxg093g2jt85), [ADM-01](#bookmark=id.ekdh78v68oz1). |
| 16 | Two files with the same structure: English (for reports) and Vietnamese (for the team), in plain words, with the block layout of each screen and the summary tables. | This document. |

## **2.2 Assumptions that need your confirmation**

These are choices made by the writer where no answer was given. Please read each line. If something is wrong, tell the Leader and the screens will be changed. Nothing here is final until confirmed.

| No. | Assumption | Why it matters | Screens |
| ----- | :---- | :---- | :---- |
| 1 | The Employee only suggests an urgency (Low, Normal, High, Urgent). The Officer sets the final priority when approving. | Priority is needed for SLA and analytics, but Employees should not decide it. | [EMP-02](#bookmark=id.v4er16ngh3i9), [DLG-01](#bookmark=id.nqyaa9ctr6re), [OFF-05](#bookmark=id.gtd6k0343759) |
| 2 | Preventive plans are created by the Officer. The Manager can view; the Administrator does not use the module. | The context does not say who owns the plans. | [PM-01](#bookmark=id.hubvrybhov1x) to [PM-06](#bookmark=id.dk34aamyou4v) |
| 3 | Work orders from a plan skip the Pending review. They start as Approved (or Assigned when the plan has a default technician). After completion they are verified by the Officer like other cases. | Avoids a useless review step but keeps the verification. | [PM-02](#bookmark=id.u6drodgl2m81), [TEC-05](#bookmark=id.ff0ok48aee2q), [OFF-02](#bookmark=id.x3zeg4xiveou) |
| 4 | A claim ends after 30 minutes without action; one Officer can hold 5 claims at most. Both numbers are editable in [ADM-07](#bookmark=id.6ynr8feqi2ao). | Stops cases from being blocked by someone who left. | [OFF-02](#bookmark=id.x3zeg4xiveou), [DLG-22](#bookmark=id.60m6ytailped), [ADM-07](#bookmark=id.6ynr8feqi2ao) |
| 5 | The Employee can say "Not fixed" within 7 days after Closed, once per case. The Officer decides (reopen or decline). | Gives a way to react to a bad fix without allowing endless reopening. | [DLG-13](#bookmark=id.3h2jh6omhwpq), [DLG-07](#bookmark=id.xisa6ciajrx1), [DLG-08](#bookmark=id.d7csm922kxo8) |
| 6 | A reopened case gets its own status Reopened and goes back to the same Technician (the Officer may change the person). The Technician must accept again. | Makes reopening visible in lists and in the reopen-rate number. | [DLG-07](#bookmark=id.xisa6ciajrx1), [TEC-02](#bookmark=id.8l487t8fl9xe), [COM-08](#bookmark=id.83jeomyy0mjj) |
| 7 | Officers, Managers and Technicians see all buildings. There is one shared Officer queue. | Follows the context (shared queue) and your answer about no per-building assignment. | [OFF-02](#bookmark=id.x3zeg4xiveou), ADM-\* |
| 8 | The Administrator cannot open cases, only see case codes as plain text in the Audit Log. | Protects content of cases; matches the AI rule for Administrator. | [COM-08](#bookmark=id.83jeomyy0mjj), [ADM-08](#bookmark=id.382mbmgltcob), [AST-03](#bookmark=id.j6n8nmgqzl9d) |
| 9 | The Administrator has no analytics screens. The Officer sees a shorter analytics view and cannot see technician rankings. | Keeps performance data with the Manager. | [ANA-01](#bookmark=id.qschmq5ovp16) to [ANA-04](#bookmark=id.vuzx748pj0j4) |
| 10 | Comments have an "Internal note" option that hides the comment from the Employee. | Staff often need to talk freely about a case. | [COM-08](#bookmark=id.83jeomyy0mjj), [TEC-03](#bookmark=id.dkkwhpnr6ifa) |
| 11 | Photos: JPG, PNG or WEBP, 5 MB each. Employee up to 5 per request, Technician up to 10 per work order. | Protects storage and speed. | [EMP-02](#bookmark=id.v4er16ngh3i9), [TEC-03](#bookmark=id.dkkwhpnr6ifa), [DLG-20](#bookmark=id.p1oyfarais6j) |
| 12 | Password rules: at least 8 characters with uppercase, lowercase and a number. 5 wrong tries lock the account for 15 minutes. Reset link lasts 30 minutes. A session lasts 8 hours. | Common safe defaults; change if the school or supervisor asks for something else. | [AUTH-01](#bookmark=id.8yzrz7fdv0n7) to [AUTH-05](#bookmark=id.dy9vq2ywyc10), [COM-03](#bookmark=id.gpq9qcuy2jzw), [ADM-07](#bookmark=id.6ynr8feqi2ao) |
| 13 | Default SLA: Urgent 1 h reply and 4 h resolve, High 4 h and 24 h, Normal 8 h and 72 h, Low 24 h and 7 days; counted in working hours. Editable in [ADM-07](#bookmark=id.6ynr8feqi2ao). While a case is On Hold the resolve timer pauses. | Numbers are examples; the team should confirm them with the supervisor. | [ADM-07](#bookmark=id.6ynr8feqi2ao), [ANA-01](#bookmark=id.qschmq5ovp16), [COM-08](#bookmark=id.83jeomyy0mjj) |
| 14 | Notifications go to the app and by email; the user can turn email off per kind in Settings, except important events. | The context lists the moments but not the channels. | [COM-04](#bookmark=id.xs734q1rwfk9), [COM-05](#bookmark=id.k3ei9nwbznrg) |
| 15 | An asset's place is a Room. Common areas (lobby, corridor) are rooms of type "Common area". | Keeps the tree at three levels. | [AST-01](#bookmark=id.r397yhsl2mmw), [AST-04](#bookmark=id.615v8i8ga3hr), [DLG-15](#bookmark=id.vxxh7jrxg7s3) |
| 16 | Asset statuses: Active, Under repair, Out of service (changed by the Officer) and Retired (only by the Administrator, blocked while open cases exist). | Status is information; it does not stop reports. | [DLG-17](#bookmark=id.24kwc21rauut), [DLG-18](#bookmark=id.wwutpqpzg3ck), [AST-02](#bookmark=id.vfpdk57qamqs) |
| 17 | A Technician cannot reassign or close work. The Technician can reject only before accepting; later they use On Hold or comments. | Keeps assignment power with the Officer. | [TEC-03](#bookmark=id.dkkwhpnr6ifa), [DLG-09](#bookmark=id.3u16ee70zy1w), [DLG-10](#bookmark=id.g5i37a8no9xn) |
| 18 | A failed item in a preventive inspection can create a new repair Case (Pending) automatically when the Technician ticks the box. | Links preventive and corrective work without extra typing. | [TEC-05](#bookmark=id.ff0ok48aee2q), [PM-06](#bookmark=id.dk34aamyou4v) |
| 19 | The AI limit is 20 questions per hour per user and 500 characters per question. | Controls cost of the AI service credit. | [COM-07](#bookmark=id.su5epyb3mo4x) |
| 20 | The UI is available in English and Vietnamese; the user chooses in Settings. | You wrote that Settings should include language. | [COM-04](#bookmark=id.xs734q1rwfk9) |

## **2.3 Things in existing work that must change**

Work that already exists (the code frame, the HTML prototype v5 and the open points in PROJECT\_CONTEXT) must be updated to match this document.

| Item | Existing work | New (this document) | What to update |
| :---- | :---- | :---- | :---- |
| **Sign-up** | The code frame has a Register page. | No self-registration; the Administrator creates accounts. | Remove or hide Register in the code frame; add [AUTH-04](#bookmark=id.kdpgutl9nzf5) and [ADM-03](#bookmark=id.l5d1kw4uy1i1) behaviour. |
| **Approve and assign** | Prototype v5 has one button "Approve & assign". | Two steps: [DLG-01](#bookmark=id.nqyaa9ctr6re) then [DLG-04](#bookmark=id.6bnps7bwaz0o), joined by the tick "Assign next". | Update the Officer screens and the state machine in the SRS. |
| **Manager** | Prototype v5 shows cost approval above the Officer's limit. | Manager is read only; no cost fields. | Remove the cost approval area from the Manager tab. |
| **Technician status** | Prototype v5 uses a Status dropdown (In progress, Completed, On hold). | Buttons plus dialogs ([DLG-09](#bookmark=id.3u16ee70zy1w), [DLG-10](#bookmark=id.g5i37a8no9xn), [DLG-11](#bookmark=id.v914lp4rv88u)), so reasons and checks are required. | Replace the dropdown in [TEC-03](#bookmark=id.dkkwhpnr6ifa). |
| **Officer priority** | Prototype v5 has a Priority dropdown next to the assignment. | Employee suggests; Officer sets the final priority in [DLG-01](#bookmark=id.nqyaa9ctr6re). | Add the urgency field to the Employee form. |
| **Admin user form** | Prototype v5 has an "invite user" form and an RBAC table. | [ADM-03](#bookmark=id.l5d1kw4uy1i1) creates users with a temporary password; [ADM-05](#bookmark=id.fsyqtq4knii4) is a read-only reference. | Rename and adjust. |
| **Status names** | PROJECT\_CONTEXT lists "Status names" as TBD. | Twelve statuses are proposed in section 5.1. | If accepted, write them into the Decisions Log and the SRS. |
| **AI window** | Prototype v4/v5 has a floating AI button that only opens. | [COM-07](#bookmark=id.su5epyb3mo4x) adds waiting, timeout, refusal, limit and rating states because Replicate answers in two steps. | Design these states and note the two-step call in Report 3 and 4\. |

# **3\. Glossary (plain words)**

Short explanations of words used in this document.

| Term | Meaning |
| :---- | :---- |
| **Case (maintenance request)** | One problem that someone reports, followed from the first report until it is closed. |
| **Work Order** | The instruction that gives a Case to a Technician. In version 1, one Case has exactly one Work Order, so screens show both codes side by side. |
| **Asset** | Anything that can be maintained: an air conditioner, a lamp, a lift, a printer, a network switch. |
| **Building, Floor, Room** | The three levels of a place. Every asset sits in a room. |
| **Officer** | Short name for Facility Officer, the person who decides and coordinates. |
| **Technician** | Short name for Maintenance Technician, the person who does the work on site. |
| **Claim (take a case)** | A mark that says "I am working on this case", so two Officers do not handle the same one. |
| **Priority** | How urgent a case is: Low, Normal, High, Urgent. The Employee only suggests; the Officer decides. |
| **SLA** | The target time to reply and to finish a case, set by priority. Missing it marks the case Overdue. |
| **Preventive maintenance** | Regular checks done on a schedule, before anything breaks. |
| **Inspection record** | The saved result of one preventive check, item by item. |
| **Timeline** | The list of everything that happened to a case, in order. |
| **Dialog** | A small window that appears on top of a page to ask for a decision or a short text. |
| **Toast** | A short message that appears for a few seconds and disappears by itself. |
| **Audit Log** | A record of who did what and when. It cannot be edited. |
| **RBAC (permissions by role)** | The rule that each person can only see and do what their role allows. |
| **Session** | The period between signing in and signing out. |
| **AI Assistant** | A chat helper that gives advice. It never decides and never shows more than the user's role may see. |
| **Wireframe (block layout)** | A simple drawing of where things are on a screen, without colours or final looks. |
| **Happy path** | The normal way where nothing goes wrong. |

# **4\. The five roles**

| Role | What this role does | Main screens |
| :---- | :---- | :---- |
| **Employee** | Reports problems, follows progress, answers questions, rates the service. | [EMP-01](#bookmark=id.gevp1q7fiais) to [EMP-04](#bookmark=id.1wkdzvdv2xi8), [COM-08](#bookmark=id.83jeomyy0mjj) |
| **Facility Officer** | Decides on new requests, gives work to Technicians, watches progress, checks finished work and closes cases. Also plans preventive maintenance and reads the assets. | [OFF-01](#bookmark=id.eui2u2y7lh0t) to [OFF-05](#bookmark=id.gtd6k0343759), [COM-08](#bookmark=id.83jeomyy0mjj), [AST-01](#bookmark=id.r397yhsl2mmw) to [AST-03](#bookmark=id.j6n8nmgqzl9d), [AST-06](#bookmark=id.9e47xaty55ny), [PM-01](#bookmark=id.hubvrybhov1x) to [PM-06](#bookmark=id.dk34aamyou4v), [ANA-01](#bookmark=id.qschmq5ovp16), [ANA-03](#bookmark=id.cwmp2fxt8ju0), [ANA-04](#bookmark=id.vuzx748pj0j4) |
| **Maintenance Technician** | Does the repair or inspection on site and records what really happened (cause, parts, hours, photos). | [TEC-01](#bookmark=id.3lbqglcq4he0) to [TEC-05](#bookmark=id.ff0ok48aee2q) |
| **Facility Manager** | Watches numbers and performance across all buildings. Does not approve or assign anything. | [MGR-01](#bookmark=id.fxg093g2jt85), [OFF-0](#bookmark=id.n0o9xiciqqd7)3, [COM-08](#bookmark=id.83jeomyy0mjj) (read only), [ANA-01](#bookmark=id.qschmq5ovp16) to [ANA-04](#bookmark=id.vuzx748pj0j4), [AST-01](#bookmark=id.r397yhsl2mmw) to [AST-03](#bookmark=id.j6n8nmgqzl9d), [PM-01](#bookmark=id.hubvrybhov1x), [PM-03](#bookmark=id.rfx2okd8binq) to [PM-06](#bookmark=id.dk34aamyou4v) (read only) |
| **Administrator** | Creates accounts, keeps places, assets and lists correct, sets SLA and reads the audit log. Does not see case content. | [ADM-01](#bookmark=id.ekdh78v68oz1) to [ADM-08](#bookmark=id.382mbmgltcob), [AST-01](#bookmark=id.r397yhsl2mmw) to [AST-07](#bookmark=id.z84288y2n6n5) |

# **5\. Case life cycle**

## **5.1 Statuses, screens and actions**

A case moves through the statuses below. The table shows what each status means, where it is seen and what can be done next.

| Status | Meaning | Where it is seen | Who can do what next |
| :---- | :---- | :---- | :---- |
| **Pending** | A new request waiting for the Officer's decision. | Employee: [EMP-01](#bookmark=id.gevp1q7fiais), [EMP-03](#bookmark=id.53a1f1joukxn), [COM-08](#bookmark=id.83jeomyy0mjj). Officer: [OFF-01](#bookmark=id.eui2u2y7lh0t), [OFF-02](#bookmark=id.x3zeg4xiveou) (To review), [OFF-03](#bookmark=id.n0o9xiciqqd7). Manager: [OFF-03](#bookmark=id.n0o9xiciqqd7) (read only). | Officer claims it, then: Approve ([DLG-01](#bookmark=id.nqyaa9ctr6re)) gives Approved; Reject ([DLG-02](#bookmark=id.2pg2riu4kig1)) gives Rejected; Need more info ([DLG-03](#bookmark=id.mmdngvj5b2et)) gives Need more info; Cancel ([DLG-12](#bookmark=id.ck085r6ldanm)) gives Cancelled. Employee: edit on [EMP-04](#bookmark=id.1wkdzvdv2xi8) (stays Pending); Cancel ([DLG-12](#bookmark=id.ck085r6ldanm)) gives Cancelled. |
| **Need more info** | The Officer asked the Employee a question. | Employee: [EMP-01](#bookmark=id.gevp1q7fiais) (Needs attention), [EMP-03](#bookmark=id.53a1f1joukxn), [EMP-04](#bookmark=id.1wkdzvdv2xi8). Officer: [OFF-02](#bookmark=id.x3zeg4xiveou) (Waiting for employee), [OFF-03](#bookmark=id.n0o9xiciqqd7). | Employee: Send back on [EMP-04](#bookmark=id.1wkdzvdv2xi8) gives Pending; Cancel gives Cancelled. Officer: Cancel gives Cancelled. |
| **Approved** | Accepted as real work, waiting for a Technician. Also used for new preventive work orders and for assignments a Technician rejected. | Officer: [OFF-01](#bookmark=id.eui2u2y7lh0t), [OFF-02](#bookmark=id.x3zeg4xiveou) (To assign), [OFF-03](#bookmark=id.n0o9xiciqqd7). Employee sees "Approved" on [COM-08](#bookmark=id.83jeomyy0mjj). | Officer: Assign ([DLG-04](#bookmark=id.6bnps7bwaz0o)) gives Assigned; Cancel gives Cancelled. |
| **Assigned** | Given to a Technician who has not answered yet. | Technician: [TEC-01](#bookmark=id.3lbqglcq4he0), [TEC-02](#bookmark=id.8l487t8fl9xe) (New), [TEC-03](#bookmark=id.dkkwhpnr6ifa). Officer: [OFF-03](#bookmark=id.n0o9xiciqqd7). Employee: [COM-08](#bookmark=id.83jeomyy0mjj). | Technician: Accept gives Accepted; Reject ([DLG-09](#bookmark=id.3u16ee70zy1w)) gives Approved. Officer: Reassign ([DLG-05](#bookmark=id.pqnz92ls7h1u)) gives Assigned to another person; Cancel gives Cancelled. |
| **Accepted** | The Technician agreed but has not started yet. | Technician: [TEC-02](#bookmark=id.8l487t8fl9xe) (Accepted), [TEC-03](#bookmark=id.dkkwhpnr6ifa). Officer: [OFF-03](#bookmark=id.n0o9xiciqqd7). Employee: [COM-08](#bookmark=id.83jeomyy0mjj). | Technician: Start work gives In Progress. Officer: Reassign, Cancel. |
| **In Progress** | The Technician is working on site. | Technician: [TEC-02](#bookmark=id.8l487t8fl9xe) (In progress), [TEC-03](#bookmark=id.dkkwhpnr6ifa), [TEC-05](#bookmark=id.ff0ok48aee2q). Officer: [OFF-03](#bookmark=id.n0o9xiciqqd7). Employee: [COM-08](#bookmark=id.83jeomyy0mjj). | Technician: Put on hold ([DLG-10](#bookmark=id.g5i37a8no9xn)) gives On Hold; Mark completed ([DLG-11](#bookmark=id.v914lp4rv88u)) gives Completed. Officer: Reassign, Cancel. |
| **On Hold** | Work is paused. The reason and the expected date are shown. | Technician: [TEC-02](#bookmark=id.8l487t8fl9xe) (On hold), [TEC-03](#bookmark=id.dkkwhpnr6ifa). Officer: [OFF-01](#bookmark=id.eui2u2y7lh0t) (if over 3 days), [OFF-03](#bookmark=id.n0o9xiciqqd7). Manager: [MGR-01](#bookmark=id.fxg093g2jt85). Employee: [COM-08](#bookmark=id.83jeomyy0mjj). | Technician: Resume gives In Progress. Officer: Reassign, Cancel. |
| **Completed** | The Technician says the work is done; the Officer has not checked yet. | Officer: [OFF-01](#bookmark=id.eui2u2y7lh0t), [OFF-04](#bookmark=id.lpy4qqom276w), [COM-08](#bookmark=id.83jeomyy0mjj). Technician: [TEC-04](#bookmark=id.6l0457m09273). Employee: [COM-08](#bookmark=id.83jeomyy0mjj) ("Work done, being checked"). | Officer: Verify ([DLG-06](#bookmark=id.ni3tomkmvn69)) gives Closed; Reopen ([DLG-07](#bookmark=id.xisa6ciajrx1)) gives Reopened. |
| **Closed** | The Officer verified the work. This is the normal end. | Employee: [EMP-03](#bookmark=id.53a1f1joukxn), [COM-08](#bookmark=id.83jeomyy0mjj). Officer: [OFF-03](#bookmark=id.n0o9xiciqqd7). Technician: [TEC-04](#bookmark=id.6l0457m09273). Manager: [OFF-03](#bookmark=id.n0o9xiciqqd7) (read only). | Employee: Rate ([DLG-14](#bookmark=id.qg8dy46jzlj2)); Not fixed ([DLG-13](#bookmark=id.3h2jh6omhwpq)) within 7 days keeps Closed and adds the tag Reopen requested. Officer: Reopen ([DLG-07](#bookmark=id.xisa6ciajrx1)) gives Reopened, or Decline ([DLG-08](#bookmark=id.d7csm922kxo8)) keeps Closed. |
| **Reopened** | Sent back to the Technician after a failed check or a report from the Employee. | Technician: [TEC-01](#bookmark=id.3lbqglcq4he0), [TEC-02](#bookmark=id.8l487t8fl9xe) (New), [TEC-03](#bookmark=id.dkkwhpnr6ifa) (red reason box). Officer: [OFF-03](#bookmark=id.n0o9xiciqqd7). Employee: [COM-08](#bookmark=id.83jeomyy0mjj). | Technician: Accept gives Accepted; Reject ([DLG-09](#bookmark=id.3u16ee70zy1w)) gives Approved. Officer: Reassign, Cancel. |
| **Rejected** | The Officer refused the request. Final. | Employee: [EMP-03](#bookmark=id.53a1f1joukxn), [COM-08](#bookmark=id.83jeomyy0mjj) (reason shown). Officer: [OFF-03](#bookmark=id.n0o9xiciqqd7). Manager: [OFF-03](#bookmark=id.n0o9xiciqqd7) (read only). | None. The Employee may send a new request ([EMP-02](#bookmark=id.v4er16ngh3i9)). |
| **Cancelled** | Stopped by the Employee (before approval) or by an Officer. Final. | Employee: [EMP-03](#bookmark=id.53a1f1joukxn), [COM-08](#bookmark=id.83jeomyy0mjj). Officer: [OFF-03](#bookmark=id.n0o9xiciqqd7). Manager: [OFF-03](#bookmark=id.n0o9xiciqqd7) (read only). | None. |

## **5.2 Notifications**

| No. | Event | Who is notified | Opens |
| ----- | :---- | :---- | :---- |
| 1 | New request submitted, or entered by an Officer | All Officers | [COM-08](#bookmark=id.83jeomyy0mjj) ([OFF-02](#bookmark=id.x3zeg4xiveou)) |
| 2 | Request approved | Employee | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 3 | Request rejected, with the reason | Employee | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 4 | More information needed (and a reminder after 3 days) | Employee | [EMP-04](#bookmark=id.1wkdzvdv2xi8) |
| 5 | Employee sent the answer back | All Officers | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 6 | Work assigned or reassigned | The new Technician; the previous Technician is told the work was taken back | [TEC-03](#bookmark=id.dkkwhpnr6ifa) |
| 7 | Technician accepted (in the app only) | The Officer who assigned it | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 8 | Technician rejected the assignment | All Officers | [OFF-02](#bookmark=id.x3zeg4xiveou) (To assign) |
| 9 | Work put on hold | All Officers, Employee | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 10 | Work marked completed | All Officers | [OFF-04](#bookmark=id.lpy4qqom276w) |
| 11 | Case closed (with an invitation to rate) | Employee | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 12 | Case reopened | Technician, Employee | [TEC-03](#bookmark=id.dkkwhpnr6ifa), [COM-08](#bookmark=id.83jeomyy0mjj) |
| 13 | Employee reports "Not fixed" | All Officers | [OFF-02](#bookmark=id.x3zeg4xiveou) (Reopen requests) |
| 14 | Case cancelled | People involved: Employee, Officer, Technician | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 15 | New comment (internal notes only reach staff) | Other people on the case | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 16 | Case close to its SLA target, or overdue | Officers; Manager when overdue | [COM-08](#bookmark=id.83jeomyy0mjj) |
| 17 | Preventive work order created | Officers, or the default Technician of the plan | [COM-08](#bookmark=id.83jeomyy0mjj), [TEC-05](#bookmark=id.ff0ok48aee2q) |
| 18 | Preventive work overdue | Officers, Manager | [PM-03](#bookmark=id.rfx2okd8binq) |
| 19 | Account created, password reset or changed, account locked (by email) | The user | [AUTH-01](#bookmark=id.8yzrz7fdv0n7) |
| 20 | An AI answer was reported as showing information the user should not see | Administrator (flagged in the Audit Log) | [ADM-08](#bookmark=id.382mbmgltcob) |

# **6\. Screen inventory**

Click a code to jump to the description. Counts: 58 pages and 23 dialogs, 81 in total.

| Code | Screen | Type | Roles | What it is for |
| :---- | :---- | :---- | :---- | :---- |
| **10.1 Account and sign-in (AUTH)** |  |  |  |  |
| [**AUTH-01**](#bookmark=id.8yzrz7fdv0n7) | Login | Page | Everyone (public) | The only public page. |
| [**AUTH-02**](#bookmark=id.qau4lqkd5b9f) | Forgot Password | Page | Everyone (public) | Lets a user ask for a link to set a new password by email. |
| [**AUTH-03**](#bookmark=id.kzmn6civei1f) | Reset Password | Page | Everyone (public) | Lets the user choose a new password after clicking the link in the email. |
| [**AUTH-04**](#bookmark=id.kdpgutl9nzf5) | Force Change Password (first login) | Page | All roles | Makes a new user replace the temporary password that the Administrator gave them. |
| [**AUTH-05**](#bookmark=id.dy9vq2ywyc10) | Session Expired | Page | All roles | Tells the user their sign-in time is over and lets them sign in again without losing their place. |
| [**AUTH-06**](#bookmark=id.nyi20vb1h2mm) | Account Locked or Deactivated | Page | Everyone (public) | Explains why a user cannot use the system and who to contact. |
| **10.2 Error pages (ERR)** |  |  |  |  |
| [**ERR-01**](#bookmark=id.lmhdh2gy047x) | No Permission (403) | Page | All roles | Shown when a signed-in user opens a page or record they are not allowed to see. |
| [**ERR-02**](#bookmark=id.gqj1o5u9y4bl) | Page Not Found (404) | Page | All roles | Shown when the address does not exist or the record was deleted. |
| [**ERR-03**](#bookmark=id.ajd3goevrvqh) | Server Error (500) | Page | All roles | Shown when something breaks on the server that the user cannot fix. |
| [**ERR-04**](#bookmark=id.z17yjcyldi2q) | No Connection (Offline) | Page | All roles | Warns the user that the internet or the server cannot be reached. |
| **10.3 Shared screens (COM)** |  |  |  |  |
| [**COM-01**](#bookmark=id.nxzrfhuvw01m) | App Shell (common layout) | Layout | All roles | The frame around every page after login: side menu, top bar and the floating AI button. |
| [**COM-02**](#bookmark=id.n6cz4ahexs2w) | Profile | Page | All roles | Shows and lets users edit their own basic information. |
| [**COM-03**](#bookmark=id.gpq9qcuy2jzw) | Change Password | Page | All roles | Lets a signed-in user change their password by knowing the old one. |
| [**COM-04**](#bookmark=id.xs734q1rwfk9) | Personal Settings | Page | All roles | Personal choices that change how the app looks and which notifications the user receives. |
| [**COM-05**](#bookmark=id.k3ei9nwbznrg) | Notification Center | Page | All roles | One place to see all notifications and open the related case or record. |
| [**COM-06**](#bookmark=id.npycecq01gq2) | Global Search and Advanced Filters | Page | All roles | Finds cases, assets and users quickly from anywhere. |
| [**COM-07**](#bookmark=id.su5epyb3mo4x) | AI Assistant (floating chat) | Page | All roles | A chat window where any user can ask questions. |
| [**COM-08**](#bookmark=id.83jeomyy0mjj) | Case Detail | Page | Employee, Officer, Manager | The single page that shows everything about one maintenance case: what was reported, who handles it, what was done, the history and the discussion. |
| **10.4 Employee (EMP)** |  |  |  |  |
| [**EMP-01**](#bookmark=id.gevp1q7fiais) | Employee Home | Page | Employee | A short overview for the Employee: what needs attention and the latest requests. |
| [**EMP-02**](#bookmark=id.v4er16ngh3i9) | New Request | Page | Employee | Lets an Employee report a facility problem so that it becomes a new Case. |
| [**EMP-03**](#bookmark=id.53a1f1joukxn) | My Requests | Page | Employee | The Employee's list of all their requests with their current status. |
| [**EMP-04**](#bookmark=id.1wkdzvdv2xi8) | Edit Request / Provide More Information | Page | Employee | Lets the Employee change a request that is not yet approved, or answer the Officer's question when the status is Need more info. |
| **10.5 Facility Officer (OFF)** |  |  |  |  |
| [**OFF-01**](#bookmark=id.eui2u2y7lh0t) | Officer Home | Page | Officer | Shows the Officer what needs a decision today. |
| [**OFF-02**](#bookmark=id.x3zeg4xiveou) | Case Queue | Page | Officer | The shared queue where new work arrives. |
| [**OFF-03**](#bookmark=id.n0o9xiciqqd7) | Case Tracking | Page | Officer, Manager (read only) | Shows every case in every status so the Officer can follow the progress and find delays; the Manager sees the same list in read-only mode. |
| [**OFF-04**](#bookmark=id.lpy4qqom276w) | Verification Queue | Page | Officer | Lists work that the Technician marked Completed. |
| [**OFF-05**](#bookmark=id.gtd6k0343759) | Create Case for an Employee | Page | Officer | For problems reported by phone or in person: the Officer enters the case on behalf of the Employee. |
| **10.6 Maintenance Technician (TEC)** |  |  |  |  |
| [**TEC-01**](#bookmark=id.3lbqglcq4he0) | Technician Home | Page | Technician | Shows the Technician what to do today. |
| [**TEC-02**](#bookmark=id.8l487t8fl9xe) | My Work Orders | Page | Technician | The Technician's list of active work orders. |
| [**TEC-03**](#bookmark=id.dkkwhpnr6ifa) | Work Order: Update and Complete | Page | Technician | Where the Technician records the real work on site: accept, start, note the cause, parts, hours, photos, put on hold, and mark completed. |
| [**TEC-04**](#bookmark=id.6l0457m09273) | Work History | Page | Technician | Lets the Technician look back at finished work orders and find what they did on an asset before. |
| [**TEC-05**](#bookmark=id.ff0ok48aee2q) | Inspection Checklist (preventive) | Page | Technician | For preventive work orders: the Technician goes through the checklist of a plan and records the result for each item. |
| **10.7 Facility Manager (MGR)** |  |  |  |  |
| [**MGR-01**](#bookmark=id.fxg093g2jt85) | Manager Home | Page | Manager | A quick look at how maintenance is performing across all buildings, and what needs attention. |
| **10.8 Administrator (ADM)** |  |  |  |  |
| [**ADM-01**](#bookmark=id.ekdh78v68oz1) | Administrator Home | Page | Admin | Gives the Administrator a quick view of accounts, data quality and recent activity. |
| [**ADM-02**](#bookmark=id.84apprc917i0) | User List | Page | Admin | Shows every account and lets the Administrator search, filter and manage them. |
| [**ADM-03**](#bookmark=id.l5d1kw4uy1i1) | Create / Edit User | Page | Admin | Creates a new account or changes an existing one, including the role. |
| [**ADM-04**](#bookmark=id.czm9bezfxzhx) | User Detail | Page | Admin | Shows one account in full with its recent activity and the management actions. |
| [**ADM-05**](#bookmark=id.fsyqtq4knii4) | Roles and Permissions (reference) | Page | Admin | A read-only table that explains what each of the five roles can do. |
| [**ADM-06**](#bookmark=id.a8n26oa6i98t) | Maintenance Categories | Page | Admin | Manages the lists that people choose from: issue types, asset types and common root causes. |
| [**ADM-07**](#bookmark=id.6ynr8feqi2ao) | SLA and Rules Settings | Page | Admin | Sets the target times for handling cases and a few numbers the system uses in its rules, so they can be changed without changing the software. |
| [**ADM-08**](#bookmark=id.382mbmgltcob) | Audit Log | Page | Admin | A record of who did what and when, including sign-ins, changes and every AI question. |
| **10.9 Assets and facilities (AST)** |  |  |  |  |
| [**AST-01**](#bookmark=id.r397yhsl2mmw) | Facility Tree (Building, Floor, Room) | Page | Officer, Manager, Admin | Shows all places as a tree and lets the Administrator create and edit them. |
| [**AST-02**](#bookmark=id.vfpdk57qamqs) | Asset List | Page | Officer, Manager, Admin | Lists all assets with filters. |
| [**AST-03**](#bookmark=id.j6n8nmgqzl9d) | Asset Detail | Page | Officer, Manager, Admin | Shows everything about one asset: information, its maintenance history, its preventive plans and how often it fails. |
| [**AST-04**](#bookmark=id.615v8i8ga3hr) | Create / Edit Asset | Page | Admin | Adds a new asset or edits an existing one. |
| [**AST-05**](#bookmark=id.xz1mbdpv5xr3) | Import Assets from File | Page | Admin | Adds many assets at once from an Excel or CSV file, in four clear steps. |
| [**AST-06**](#bookmark=id.9e47xaty55ny) | QR Labels (print) | Page | Officer, Admin | Creates printable QR labels that people stick on assets. |
| [**AST-07**](#bookmark=id.z84288y2n6n5) | QR Landing (scan to report) | Page | All roles | Decides where to send a person who scans a QR label on an asset, and explains problems with the label. |
| **10.10 Preventive maintenance (PM)** |  |  |  |  |
| [**PM-01**](#bookmark=id.hubvrybhov1x) | Preventive Plan List | Page | Officer, Manager | Lists the plans that create maintenance work on a schedule, before anything breaks. |
| [**PM-02**](#bookmark=id.u6drodgl2m81) | Create / Edit Preventive Plan | Page | Officer | Defines what to check, how often, on which assets, and who does it. |
| [**PM-03**](#bookmark=id.rfx2okd8binq) | Preventive Plan Detail | Page | Officer, Manager | Shows one plan, what it created so far, and how well it is followed. |
| [**PM-04**](#bookmark=id.xzhz6y8fmacz) | Preventive Calendar | Page | Officer, Manager | Shows upcoming and past preventive work on a calendar so the Officer can balance the workload. |
| [**PM-05**](#bookmark=id.j0baje6voyco) | Inspection Records | Page | Officer, Manager | The history of all inspections that Technicians completed. |
| [**PM-06**](#bookmark=id.dk34aamyou4v) | Inspection Record Detail | Page | Officer, Manager | Shows the full result of one inspection. |
| **10.11 Analytics (ANA)** |  |  |  |  |
| [**ANA-01**](#bookmark=id.qschmq5ovp16) | KPI Overview Dashboard | Page | Officer, Manager | Shows how maintenance is performing so managers can make decisions with numbers. |
| [**ANA-02**](#bookmark=id.h2nhwbchtidg) | Technician Performance | Page | Manager | Helps the Manager see how each Technician is doing and how the workload is shared. |
| [**ANA-03**](#bookmark=id.cwmp2fxt8ju0) | Failure Trends and Problem Assets | Page | Officer, Manager | Shows which assets and which kinds of problems happen most, so preventive work can be planned better. |
| [**ANA-04**](#bookmark=id.vuzx748pj0j4) | Reports and Export | Page | Officer, Manager | Creates reports as PDF or Excel files for meetings and records. |
| **10.12 Dialogs (DLG)** |  |  |  |  |
| [**DLG-01**](#bookmark=id.nqyaa9ctr6re) | Approve Case | Dialog | Officer | The Officer accepts the request as a real maintenance job and sets its final priority. |
| [**DLG-02**](#bookmark=id.2pg2riu4kig1) | Reject Case | Dialog | Officer | The Officer refuses the request and explains why. |
| [**DLG-03**](#bookmark=id.mmdngvj5b2et) | Request More Information | Dialog | Officer | The Officer asks the Employee a question before deciding. |
| [**DLG-04**](#bookmark=id.6bnps7bwaz0o) | Assign Technician | Dialog | Officer | The Officer chooses who will do the work and by when. |
| [**DLG-05**](#bookmark=id.pqnz92ls7h1u) | Reassign Technician | Dialog | Officer | Moves a case that was already given to another Technician, for example when someone is ill or overloaded. |
| [**DLG-06**](#bookmark=id.ni3tomkmvn69) | Verify and Close | Dialog | Officer | The Officer confirms that the work was done well and closes the case. |
| [**DLG-07**](#bookmark=id.xisa6ciajrx1) | Reopen Case | Dialog | Officer | Sends a case back to the Technician because the fix was not good enough or the Employee says it is not fixed. |
| [**DLG-08**](#bookmark=id.d7csm922kxo8) | Decline Reopen Request | Dialog | Officer | The Officer says no to an Employee's "Not fixed" report and explains. |
| [**DLG-09**](#bookmark=id.3u16ee70zy1w) | Reject Assignment | Dialog | Technician | The Technician cannot take a work order and tells the Officer why. |
| [**DLG-10**](#bookmark=id.g5i37a8no9xn) | Put Work on Hold | Dialog | Technician | The Technician pauses work, for example because a part or a permission is missing. |
| [**DLG-11**](#bookmark=id.v914lp4rv88u) | Confirm Completion | Dialog | Technician | A last check before the Technician reports the work as done. |
| [**DLG-12**](#bookmark=id.ck085r6ldanm) | Cancel Request or Case | Dialog | Employee, Officer | Stops a request. The Employee can cancel their own request before it is approved; an Officer can cancel a case at any point before Completed. |
| [**DLG-13**](#bookmark=id.3h2jh6omhwpq) | Report "Not Fixed" | Dialog | Employee | After a case is Closed, the Employee can say the problem is still there. |
| [**DLG-14**](#bookmark=id.qg8dy46jzlj2) | Rate the Service | Dialog | Employee | The Employee gives a rating after the case is closed, which helps the Manager see quality. |
| [**DLG-15**](#bookmark=id.vxxh7jrxg7s3) | Building, Floor or Room Form | Dialog | Admin | Creates or edits a place in the facility tree. |
| [**DLG-16**](#bookmark=id.x23yp780kgp) | User Account Action | Dialog | Admin | Asks the Administrator to confirm a sensitive account action. |
| [**DLG-17**](#bookmark=id.24kwc21rauut) | Confirm Retire or Delete | Dialog | Admin | Protects important data: retiring an asset, deactivating or deleting a place, or deleting a category. |
| [**DLG-18**](#bookmark=id.wwutpqpzg3ck) | Change Asset Status | Dialog | Officer | The Officer records whether an asset is working, being repaired or out of service. |
| [**DLG-19**](#bookmark=id.98dbp5b14xo5) | Unsaved Changes | Dialog | All roles | Prevents losing typed data when the user leaves a form. |
| [**DLG-20**](#bookmark=id.p1oyfarais6j) | Photo Viewer and Upload Problems | Dialog | All roles | Shows photos large and explains problems when adding photos. |
| [**DLG-21**](#bookmark=id.p5kc67ltjc22) | Generate Preventive Work Order Now | Dialog | Officer | Creates the next preventive work order early, without waiting for the schedule. |
| [**DLG-22**](#bookmark=id.60m6ytailped) | Claim Conflict and Release | Dialog | Officer | Handles the moments when two Officers want the same case, or when a claim ends. |
| [**DLG-23**](#bookmark=id.nd9lxpb4ubvz) | Category Item Form | Dialog | Admin | Adds or edits one item in a maintenance category list. |

**Screens by module**

| Module | Count |
| :---- | ----- |
| Account and sign-in (AUTH) | 6 |
| Error pages (ERR) | 4 |
| Shared screens (COM) | 8 |
| Employee (EMP) | 4 |
| Facility Officer (OFF) | 5 |
| Maintenance Technician (TEC) | 5 |
| Facility Manager (MGR) | 1 |
| Administrator (ADM) | 8 |
| Assets and facilities (AST) | 7 |
| Preventive maintenance (PM) | 6 |
| Analytics (ANA) | 4 |
| Dialogs (DLG) | 23 |
| **Total** | **81** |

# **7\. Permission table (Screen × Role)**

Read across a row to see who can use a screen. The letters are explained in section 1\.

**Legend for the permission letters**

* F \= Full: can use everything on this screen that belongs to the role (view, create, change, decide).

* S \= Scoped: can open it, but only part of it: only their own data, or fewer buttons.

* R \= Read only: can look, cannot change anything.

* \- \= No access: the menu does not show it and typing the address leads to [ERR-01](#bookmark=id.lmhdh2gy047x).

* P \= Public: can be opened without signing in.

| Code | Screen | Employee | Officer | Technician | Manager | Admin |
| :---- | :---- | :---: | :---: | :---: | :---: | :---: |
| **10.1 Account and sign-in (AUTH)** |  |  |  |  |  |  |
| [**AUTH-01**](#bookmark=id.8yzrz7fdv0n7) | Login | **P** | **P** | **P** | **P** | **P** |
| [**AUTH-02**](#bookmark=id.qau4lqkd5b9f) | Forgot Password | **P** | **P** | **P** | **P** | **P** |
| [**AUTH-03**](#bookmark=id.kzmn6civei1f) | Reset Password | **P** | **P** | **P** | **P** | **P** |
| [**AUTH-04**](#bookmark=id.kdpgutl9nzf5) | Force Change Password (first login) | **F** | **F** | **F** | **F** | **F** |
| [**AUTH-05**](#bookmark=id.dy9vq2ywyc10) | Session Expired | **F** | **F** | **F** | **F** | **F** |
| [**AUTH-06**](#bookmark=id.nyi20vb1h2mm) | Account Locked or Deactivated | **P** | **P** | **P** | **P** | **P** |
| **10.2 Error pages (ERR)** |  |  |  |  |  |  |
| [**ERR-01**](#bookmark=id.lmhdh2gy047x) | No Permission (403) | **F** | **F** | **F** | **F** | **F** |
| [**ERR-02**](#bookmark=id.gqj1o5u9y4bl) | Page Not Found (404) | **F** | **F** | **F** | **F** | **F** |
| [**ERR-03**](#bookmark=id.ajd3goevrvqh) | Server Error (500) | **F** | **F** | **F** | **F** | **F** |
| [**ERR-04**](#bookmark=id.z17yjcyldi2q) | No Connection (Offline) | **F** | **F** | **F** | **F** | **F** |
| **10.3 Shared screens (COM)** |  |  |  |  |  |  |
| [**COM-01**](#bookmark=id.nxzrfhuvw01m) | App Shell (common layout) | **F** | **F** | **F** | **F** | **F** |
| [**COM-02**](#bookmark=id.n6cz4ahexs2w) | Profile | **F** | **F** | **F** | **F** | **F** |
| [**COM-03**](#bookmark=id.gpq9qcuy2jzw) | Change Password | **F** | **F** | **F** | **F** | **F** |
| [**COM-04**](#bookmark=id.xs734q1rwfk9) | Personal Settings | **F** | **F** | **F** | **F** | **F** |
| [**COM-05**](#bookmark=id.k3ei9nwbznrg) | Notification Center | **F** | **F** | **F** | **F** | **F** |
| [**COM-06**](#bookmark=id.npycecq01gq2) | Global Search and Advanced Filters | **S** | **S** | **S** | **S** | **S** |
| [**COM-07**](#bookmark=id.su5epyb3mo4x) | AI Assistant (floating chat) | **F** | **F** | **F** | **F** | **F** |
| [**COM-08**](#bookmark=id.83jeomyy0mjj) | Case Detail | **S** | **F** | \- | **R** | \- |
| **10.4 Employee (EMP)** |  |  |  |  |  |  |
| [**EMP-01**](#bookmark=id.gevp1q7fiais) | Employee Home | **F** | \- | \- | \- | \- |
| [**EMP-02**](#bookmark=id.v4er16ngh3i9) | New Request | **F** | \- | \- | \- | \- |
| [**EMP-03**](#bookmark=id.53a1f1joukxn) | My Requests | **F** | \- | \- | \- | \- |
| [**EMP-04**](#bookmark=id.1wkdzvdv2xi8) | Edit Request / Provide More Information | **F** | \- | \- | \- | \- |
| **10.5 Facility Officer (OFF)** |  |  |  |  |  |  |
| [**OFF-01**](#bookmark=id.eui2u2y7lh0t) | Officer Home | \- | **F** | \- | \- | \- |
| [**OFF-02**](#bookmark=id.x3zeg4xiveou) | Case Queue | \- | **F** | \- | \- | \- |
| [**OFF-03**](#bookmark=id.n0o9xiciqqd7) | Case Tracking | \- | **F** | \- | R | \- |
| [**OFF-04**](#bookmark=id.lpy4qqom276w) | Verification Queue | \- | **F** | \- | \- | \- |
| [**OFF-05**](#bookmark=id.gtd6k0343759) | Create Case for an Employee | \- | **F** | \- | \- | \- |
| **10.6 Maintenance Technician (TEC)** |  |  |  |  |  |  |
| [**TEC-01**](#bookmark=id.3lbqglcq4he0) | Technician Home | \- | \- | **F** | \- | \- |
| [**TEC-02**](#bookmark=id.8l487t8fl9xe) | My Work Orders | \- | \- | **F** | \- | \- |
| [**TEC-03**](#bookmark=id.dkkwhpnr6ifa) | Work Order: Update and Complete | \- | \- | **F** | \- | \- |
| [**TEC-04**](#bookmark=id.6l0457m09273) | Work History | \- | \- | **F** | \- | \- |
| [**TEC-05**](#bookmark=id.ff0ok48aee2q) | Inspection Checklist (preventive) | \- | \- | **F** | \- | \- |
| **10.7 Facility Manager (MGR)** |  |  |  |  |  |  |
| [**MGR-01**](#bookmark=id.fxg093g2jt85) | Manager Home | \- | \- | \- | **F** | \- |
| **10.8 Administrator (ADM)** |  |  |  |  |  |  |
| [**ADM-01**](#bookmark=id.ekdh78v68oz1) | Administrator Home | \- | \- | \- | \- | **F** |
| [**ADM-02**](#bookmark=id.84apprc917i0) | User List | \- | \- | \- | \- | **F** |
| [**ADM-03**](#bookmark=id.l5d1kw4uy1i1) | Create / Edit User | \- | \- | \- | \- | **F** |
| [**ADM-04**](#bookmark=id.czm9bezfxzhx) | User Detail | \- | \- | \- | \- | **F** |
| [**ADM-05**](#bookmark=id.fsyqtq4knii4) | Roles and Permissions (reference) | \- | \- | \- | \- | **F** |
| [**ADM-06**](#bookmark=id.a8n26oa6i98t) | Maintenance Categories | \- | \- | \- | \- | **F** |
| [**ADM-07**](#bookmark=id.6ynr8feqi2ao) | SLA and Rules Settings | \- | \- | \- | \- | **F** |
| [**ADM-08**](#bookmark=id.382mbmgltcob) | Audit Log | \- | \- | \- | \- | **F** |
| **10.9 Assets and facilities (AST)** |  |  |  |  |  |  |
| [**AST-01**](#bookmark=id.r397yhsl2mmw) | Facility Tree (Building, Floor, Room) | \- | **R** | \- | **R** | **F** |
| [**AST-02**](#bookmark=id.vfpdk57qamqs) | Asset List | \- | **S** | \- | **R** | **F** |
| [**AST-03**](#bookmark=id.j6n8nmgqzl9d) | Asset Detail | \- | **S** | \- | **R** | **S** |
| [**AST-04**](#bookmark=id.615v8i8ga3hr) | Create / Edit Asset | \- | \- | \- | \- | **F** |
| [**AST-05**](#bookmark=id.xz1mbdpv5xr3) | Import Assets from File | \- | \- | \- | \- | **F** |
| [**AST-06**](#bookmark=id.9e47xaty55ny) | QR Labels (print) | \- | **F** | \- | \- | **F** |
| [**AST-07**](#bookmark=id.z84288y2n6n5) | QR Landing (scan to report) | **F** | **F** | **F** | **F** | **F** |
| **10.10 Preventive maintenance (PM)** |  |  |  |  |  |  |
| [**PM-01**](#bookmark=id.hubvrybhov1x) | Preventive Plan List | \- | **F** | \- | **R** | \- |
| [**PM-02**](#bookmark=id.u6drodgl2m81) | Create / Edit Preventive Plan | \- | **F** | \- | \- | \- |
| [**PM-03**](#bookmark=id.rfx2okd8binq) | Preventive Plan Detail | \- | **F** | \- | **R** | \- |
| [**PM-04**](#bookmark=id.xzhz6y8fmacz) | Preventive Calendar | \- | **F** | \- | **R** | \- |
| [**PM-05**](#bookmark=id.j0baje6voyco) | Inspection Records | \- | **F** | \- | **R** | \- |
| [**PM-06**](#bookmark=id.dk34aamyou4v) | Inspection Record Detail | \- | **F** | \- | **R** | \- |
| **10.11 Analytics (ANA)** |  |  |  |  |  |  |
| [**ANA-01**](#bookmark=id.qschmq5ovp16) | KPI Overview Dashboard | \- | **S** | \- | **F** | \- |
| [**ANA-02**](#bookmark=id.h2nhwbchtidg) | Technician Performance | \- | \- | \- | **F** | \- |
| [**ANA-03**](#bookmark=id.cwmp2fxt8ju0) | Failure Trends and Problem Assets | \- | **S** | \- | **F** | \- |
| [**ANA-04**](#bookmark=id.vuzx748pj0j4) | Reports and Export | \- | **S** | \- | **F** | \- |
| **10.12 Dialogs (DLG)** |  |  |  |  |  |  |
| [**DLG-01**](#bookmark=id.nqyaa9ctr6re) | Approve Case | \- | **F** | \- | \- | \- |
| [**DLG-02**](#bookmark=id.2pg2riu4kig1) | Reject Case | \- | **F** | \- | \- | \- |
| [**DLG-03**](#bookmark=id.mmdngvj5b2et) | Request More Information | \- | **F** | \- | \- | \- |
| [**DLG-04**](#bookmark=id.6bnps7bwaz0o) | Assign Technician | \- | **F** | \- | \- | \- |
| [**DLG-05**](#bookmark=id.pqnz92ls7h1u) | Reassign Technician | \- | **F** | \- | \- | \- |
| [**DLG-06**](#bookmark=id.ni3tomkmvn69) | Verify and Close | \- | **F** | \- | \- | \- |
| [**DLG-07**](#bookmark=id.xisa6ciajrx1) | Reopen Case | \- | **F** | \- | \- | \- |
| [**DLG-08**](#bookmark=id.d7csm922kxo8) | Decline Reopen Request | \- | **F** | \- | \- | \- |
| [**DLG-09**](#bookmark=id.3u16ee70zy1w) | Reject Assignment | \- | \- | **F** | \- | \- |
| [**DLG-10**](#bookmark=id.g5i37a8no9xn) | Put Work on Hold | \- | \- | **F** | \- | \- |
| [**DLG-11**](#bookmark=id.v914lp4rv88u) | Confirm Completion | \- | \- | **F** | \- | \- |
| [**DLG-12**](#bookmark=id.ck085r6ldanm) | Cancel Request or Case | **S** | **F** | \- | \- | \- |
| [**DLG-13**](#bookmark=id.3h2jh6omhwpq) | Report "Not Fixed" | **F** | \- | \- | \- | \- |
| [**DLG-14**](#bookmark=id.qg8dy46jzlj2) | Rate the Service | **F** | \- | \- | \- | \- |
| [**DLG-15**](#bookmark=id.vxxh7jrxg7s3) | Building, Floor or Room Form | \- | \- | \- | \- | **F** |
| [**DLG-16**](#bookmark=id.x23yp780kgp) | User Account Action | \- | \- | \- | \- | **F** |
| [**DLG-17**](#bookmark=id.24kwc21rauut) | Confirm Retire or Delete | \- | \- | \- | \- | **F** |
| [**DLG-18**](#bookmark=id.wwutpqpzg3ck) | Change Asset Status | \- | **F** | \- | \- | \- |
| [**DLG-19**](#bookmark=id.98dbp5b14xo5) | Unsaved Changes | **F** | **F** | **F** | **F** | **F** |
| [**DLG-20**](#bookmark=id.p1oyfarais6j) | Photo Viewer and Upload Problems | **F** | **F** | **F** | **F** | **F** |
| [**DLG-21**](#bookmark=id.p5kc67ltjc22) | Generate Preventive Work Order Now | \- | **F** | \- | \- | \- |
| [**DLG-22**](#bookmark=id.60m6ytailped) | Claim Conflict and Release | \- | **F** | \- | \- | \- |
| [**DLG-23**](#bookmark=id.nd9lxpb4ubvz) | Category Item Form | \- | \- | \- | \- | **F** |

# **8\. Navigation maps**

Each map starts from the role's Home page and reads from left to right. Boxes with a dashed border are exception screens. The same screen can appear in several places; it is only expanded once.

## **8.1 Sign-in and shared screens (all roles)**

![Sign-in and shared screens (all roles)][image1]

## **8.2 Employee**

![Employee][image2]

## **8.3 Facility Officer: handling cases**

![Facility Officer: handling cases][image3]

## **8.4 Facility Officer: assets, preventive maintenance, analytics**

![Facility Officer: assets, preventive maintenance, analytics][image4]

## **8.5 Maintenance Technician**

![Maintenance Technician][image5]

## **8.6 Facility Manager**

![Facility Manager][image6]

## **8.7 Administrator**

![Administrator][image7]

# **9\. Rules that apply to every screen**

These rules are the same on every screen, so they are written once here and not repeated in each description.

**Lists (tables)**

Every list has: a search box, filters shown as small chips, sortable columns, 20 rows per page with page numbers and the total count, a loading state (grey rows), an empty state with a friendly sentence, an "empty after filtering" state with Clear filters, and a red banner with Retry when loading fails. Clicking a row opens its detail.

**Forms**

Required fields are marked with a star. Errors appear in red under the field, in words that say what to do (not codes). Checks happen when the user leaves a field and again on submit; the page scrolls to the first error. The submit button is disabled while sending so nothing is sent twice. If sending fails, a red banner appears and everything typed is kept. Successful actions show a green toast for 4 seconds. A form with unsaved changes asks [DLG-19](#bookmark=id.98dbp5b14xo5) before leaving.

**Loading, empty and error**

Each block of a page loads on its own, so one failing block does not break the page. Loading shows grey placeholder shapes, not spinners over the whole page. A page that cannot load at all shows [ERR-03](#bookmark=id.ajd3goevrvqh); a lost connection shows the offline bar ([ERR-04](#bookmark=id.z17yjcyldi2q)).

**Status labels**

A status is always shown with colour, a small icon and text together, never colour alone. The same words and colours are used on every screen.

**Permissions and hidden buttons**

A button that a role must never use is hidden, not greyed out. A button that is normally allowed but not right now (for example wrong status) is greyed out with a short tooltip that says why. Opening an address without permission shows [ERR-01](#bookmark=id.lmhdh2gy047x), and the attempt is logged.

**Two people at the same time**

If another person changes a case while you have it open, a blue bar says so and decision buttons are disabled until the page is refreshed. Claiming a case is decided by who clicks first ([DLG-22](#bookmark=id.60m6ytailped)).

**Photos and files**

JPG, PNG or WEBP, 5 MB each. Employee up to 5 photos per request, Technician up to 10 per work order. Drag and drop or click; a thumbnail with a progress bar is shown; failures show a red mark and Retry. Clicking a photo opens [DLG-20](#bookmark=id.p1oyfarais6j). Imports accept .xlsx and .csv up to 5 MB.

**Dates, times and language**

Times are shown in the user's own time zone. Recent times are relative ("2 hours ago") with the exact time on hover. The user chooses English or Vietnamese in Settings ([COM-04](#bookmark=id.xs734q1rwfk9)); all messages in this document exist in both languages.

**Live refresh**

Lists and the bell refresh by themselves about every 30 seconds. Changes appear with a short highlight; lists never jump while the user is reading (a bar "3 new items. Show" appears instead).

**Audit and safety**

Every sign-in, every change of data, every status change, every export and every AI question is written to the Audit Log. Passwords are never shown or stored in readable form. Case content is not shown to the Administrator.

**Screen size and access**

Designed for desktop (1280 px and wider). Below 1024 px the side menu becomes a hamburger button and tables scroll sideways. Everything can be used with the keyboard; focus is always visible; text and colour contrast are readable; icons have text labels.

**Wording**

Short, kind and clear. Say what happened and what to do next. No technical words or codes in messages. Buttons start with a verb ("Send request", not "OK").

**Part B. Screen specifications**

# **10\. Screen specifications**

Screens are grouped by module. Inside each description, a code such as [EMP-04](#bookmark=id.1wkdzvdv2xi8) is a link to that screen.

## **10.1 Account and sign-in (AUTH)**

### **AUTH-01  Login**

| Type | Page |
| :---- | :---- |
| **Module** | Account and sign-in (AUTH) |
| **Who can open it** | Everyone, without signing in |
| **Where it comes from** | Opens when the user visits the site, when a session ends ([AUTH-05](#bookmark=id.dy9vq2ywyc10)), or after logging out. If a link from a notification or QR code was opened before login, the user is sent to that page after signing in. |

**Purpose**

The only public page. Users sign in with the email and password given by the Administrator.

**Block layout**

![AUTH-01 Login][image8]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Email field** | Required. Must look like an email address. |
| **Password field** | Required. Has an eye icon to show or hide the characters. |
| **Sign in button** | Disabled while the request is being sent, shows a small spinner. |
| **Forgot password link** | Opens [AUTH-02](#bookmark=id.qau4lqkd5b9f). |
| **Error message area** | Shows one clear message above the button when sign-in fails. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Sign in** | Correct details: the user goes to their own Home page ([EMP-01](#bookmark=id.gevp1q7fiais), [OFF-01](#bookmark=id.eui2u2y7lh0t), [TEC-01](#bookmark=id.3lbqglcq4he0), [MGR-01](#bookmark=id.fxg093g2jt85) or [ADM-01](#bookmark=id.ekdh78v68oz1)). First login with a temporary password: goes to [AUTH-04](#bookmark=id.kdpgutl9nzf5) first. |
| **Press Enter** | Same as pressing the Sign in button. |
| **Click Forgot password** | Goes to [AUTH-02](#bookmark=id.qau4lqkd5b9f). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty fields** | Red text under the field: "Please enter your email" or "Please enter your password". Nothing is sent. |
| **Wrong email or password** | "Email or password is incorrect." The message does not say which one is wrong, for safety. |
| **Too many wrong attempts** | After 5 wrong attempts the account is locked for 15 minutes. Message: "Too many attempts. Try again in 15 minutes or contact your Administrator." |
| **Account deactivated** | Goes to [AUTH-06](#bookmark=id.nyi20vb1h2mm). |
| **No connection or server error** | Red banner: "Cannot reach the server. Please try again." The typed email stays in the box. |
| **Already signed in** | Opening this page while signed in sends the user straight to their Home page. |

**Rules**

* There is no Register button. Accounts are created only by the Administrator ([ADM-03](#bookmark=id.l5d1kw4uy1i1)).

* Every sign-in attempt (success or fail) is written to the Audit Log.

* Password is never shown in error messages or saved in the browser by the system.

**Where next**

To [AUTH-02](#bookmark=id.qau4lqkd5b9f), [AUTH-04](#bookmark=id.kdpgutl9nzf5), [AUTH-06](#bookmark=id.nyi20vb1h2mm) or the role Home page.

### **AUTH-02  Forgot Password**

| Type | Page |
| :---- | :---- |
| **Module** | Account and sign-in (AUTH) |
| **Who can open it** | Everyone, without signing in |
| **Where it comes from** | From the Forgot password link on [AUTH-01](#bookmark=id.8yzrz7fdv0n7). |

**Purpose**

Lets a user ask for a link to set a new password by email.

**Block layout**

![AUTH-02 Forgot Password][image9]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Email field** | Required. Must be a valid email format. |
| **Send reset link button** | Sends the email if the address belongs to an active account. |
| **Confirmation message** | After sending, the form is replaced by: "If this email exists, we sent a link. Check your inbox." |
| **Resend button** | Appears on the confirmation view. Disabled for 60 seconds after each send. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Send reset link** | Sends the email and shows the confirmation message. |
| **Back to sign in** | Goes to [AUTH-01](#bookmark=id.8yzrz7fdv0n7). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Invalid email format** | Red text under the field. Nothing is sent. |
| **Email not found or account deactivated** | The same confirmation message is shown (so nobody can guess which emails exist). No email is sent. |
| **Sending too often** | "Please wait a minute before asking again." |
| **Email service failed** | "We could not send the email. Please try again later." |

**Rules**

* The reset link is valid for 30 minutes and can be used once.

* The request is written to the Audit Log.

**Where next**

To [AUTH-01](#bookmark=id.8yzrz7fdv0n7), or the user opens the email link, which leads to [AUTH-03](#bookmark=id.kzmn6civei1f).

### **AUTH-03  Reset Password**

| Type | Page |
| :---- | :---- |
| **Module** | Account and sign-in (AUTH) |
| **Who can open it** | Everyone, without signing in |
| **Where it comes from** | From the link in the reset email. |

**Purpose**

Lets the user choose a new password after clicking the link in the email.

**Block layout**

![AUTH-03 Reset Password][image10]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **New password field** | Required. Show or hide option. |
| **Password rules checklist** | Live checklist: at least 8 characters, one uppercase, one lowercase, one number. Each line turns green when met. |
| **Confirm password field** | Must match the new password. |
| **Save button** | Enabled only when all rules are met and both fields match. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save new password** | Password is changed, the link stops working, all old sessions are ended, and the user is sent to [AUTH-01](#bookmark=id.8yzrz7fdv0n7) with a green message "Password updated. Please sign in." |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Link expired or already used** | The form is replaced by: "This link is no longer valid." with a button "Ask for a new link" (goes to [AUTH-02](#bookmark=id.qau4lqkd5b9f)). |
| **Password too weak** | Unmet rules stay grey or red and the Save button stays disabled. |
| **Passwords do not match** | Red text: "Passwords do not match." |
| **Same as the old password** | "Choose a password you have not used before." |
| **Server error** | Red banner with a Retry button. Typed values are kept. |

**Rules**

* Password change is written to the Audit Log and an email "your password was changed" is sent.

**Where next**

To [AUTH-01](#bookmark=id.8yzrz7fdv0n7) or [AUTH-02](#bookmark=id.qau4lqkd5b9f).

### **AUTH-04  Force Change Password (first login)**

| Type | Page |
| :---- | :---- |
| **Module** | Account and sign-in (AUTH) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Shown right after a successful sign-in when the account is marked "must change password" (new account or password reset by Administrator). |

**Purpose**

Makes a new user replace the temporary password that the Administrator gave them.

**Block layout**

![AUTH-04 Force Change Password (first login)][image11]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Current password field** | Required. The temporary password from the Administrator. |
| **New and confirm fields** | Same rules as [AUTH-03](#bookmark=id.kzmn6civei1f). |
| **Log out link** | Small link at the bottom. The user can leave without changing. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save and continue** | Password changed. The user goes to their Home page. |
| **Log out** | Ends the session and goes to [AUTH-01](#bookmark=id.8yzrz7fdv0n7). The next login will ask again. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Wrong current password** | "Current password is incorrect." |
| **New password same as temporary one** | "Choose a different password." |
| **Weak or mismatched password** | Same messages as [AUTH-03](#bookmark=id.kzmn6civei1f). |
| **Server error** | Red banner with Retry. |

**Rules**

* Until this step is done the user cannot open any other page. Any other address sends them back here.

**Where next**

To the role Home page or [AUTH-01](#bookmark=id.8yzrz7fdv0n7).

### **AUTH-05  Session Expired**

| Type | Page |
| :---- | :---- |
| **Module** | Account and sign-in (AUTH) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Appears over any page when the server says the login is no longer valid (session ended, or the Administrator changed the account). |

**Purpose**

Tells the user their sign-in time is over and lets them sign in again without losing their place.

**Block layout**

![AUTH-05 Session Expired][image12]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Message** | "For your safety you were signed out. Please sign in again." |
| **Sign in again button** | Goes to [AUTH-01](#bookmark=id.8yzrz7fdv0n7). After signing in, the user returns to the page they were on. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Sign in again** | Goes to [AUTH-01](#bookmark=id.8yzrz7fdv0n7) and remembers the last page. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Unsaved form data** | The text typed so far is kept in the browser for that tab and is restored after signing in again, when possible. |
| **Session ends while uploading a photo** | The upload stops and the photo must be added again. |

**Rules**

* A session lasts 8 hours. A quiet warning appears 5 minutes before it ends with a "Stay signed in" button.

**Where next**

To [AUTH-01](#bookmark=id.8yzrz7fdv0n7).

### **AUTH-06  Account Locked or Deactivated**

| Type | Page |
| :---- | :---- |
| **Module** | Account and sign-in (AUTH) |
| **Who can open it** | Everyone, without signing in |
| **Where it comes from** | After sign-in for a deactivated account, or when the Administrator deactivates an account while the user is already signed in. |

**Purpose**

Explains why a user cannot use the system and who to contact.

**Block layout**

![AUTH-06 Account Locked or Deactivated][image13]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Title and message** | "Your account is deactivated. Please contact your Administrator." The Administrator's contact (email) comes from system settings. |
| **Back to sign in button** | Goes to [AUTH-01](#bookmark=id.8yzrz7fdv0n7). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Back to sign in** | Goes to [AUTH-01](#bookmark=id.8yzrz7fdv0n7). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Temporary lock (wrong passwords)** | Shown on [AUTH-01](#bookmark=id.8yzrz7fdv0n7) as a message with the remaining minutes, not on this page. |
| **Deactivated while signed in** | The next action of the user shows this page and ends the session. |

**Rules**

* Deactivated users keep their history (cases, comments) but cannot sign in.

**Where next**

To [AUTH-01](#bookmark=id.8yzrz7fdv0n7).

## **10.2 Error pages (ERR)**

### **ERR-01  No Permission (403)**

| Type | Page |
| :---- | :---- |
| **Module** | Error pages (ERR) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Typing an address by hand, using an old bookmark, or opening a link shared by someone with a different role (for example, an Employee opening another person's case). |

**Purpose**

Shown when a signed-in user opens a page or record they are not allowed to see.

**Block layout**

![ERR-01 No Permission (403)][image14]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Message** | "You do not have permission to open this page." |
| **Go to my Home button** | Goes to the user's own Home page. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Go to my Home** | Home page of the user's role. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Case belongs to someone else** | Same 403 page. The page never confirms if the case exists. |

**Rules**

* Menus never show links the user cannot use, so this page appears only for hand-typed or old links.

* The blocked attempt is written to the Audit Log.

**Where next**

To the role Home page.

### **ERR-02  Page Not Found (404)**

| Type | Page |
| :---- | :---- |
| **Module** | Error pages (ERR) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | A mistyped address, or a link to a record that no longer exists (for example a deleted asset). |

**Purpose**

Shown when the address does not exist or the record was deleted.

**Block layout**

![ERR-02 Page Not Found (404)][image15]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Message** | "We could not find what you are looking for." |
| **Go to my Home button** | Goes to Home. A "Go back" link returns to the previous page. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Go to my Home / Go back** | Navigates as named. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Not signed in** | Unknown addresses first ask the user to sign in ([AUTH-01](#bookmark=id.8yzrz7fdv0n7)), then show this page if needed. |

**Rules**

* Cases and assets are never permanently deleted in normal use, so 404 for a case usually means a wrong code.

**Where next**

To the role Home page.

### **ERR-03  Server Error (500)**

| Type | Page |
| :---- | :---- |
| **Module** | Error pages (ERR) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Any page whose data request fails with a server error, or a page that crashes. |

**Purpose**

Shown when something breaks on the server that the user cannot fix.

**Block layout**

![ERR-03 Server Error (500)][image16]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Message** | "Something went wrong on our side. Your data is safe." |
| **Reference code** | A short code the user can send to the Administrator to find the problem in the logs. |
| **Try again button** | Reloads the same page. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Try again** | Repeats the request. |
| **Go to my Home** | Secondary link. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Small part of a page fails** | Only that block shows "Could not load. Retry", the rest of the page keeps working. |
| **Form submit fails** | A red banner is shown on the form, typed data is kept, no 500 page. |

**Rules**

* The error is recorded with the reference code for the technical team.

**Where next**

Same page again or the role Home page.

### **ERR-04  No Connection (Offline)**

| Type | Page |
| :---- | :---- |
| **Module** | Error pages (ERR) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Appears automatically as a bar at the top of any page when the browser goes offline. |

**Purpose**

Warns the user that the internet or the server cannot be reached.

**Block layout**

![ERR-04 No Connection (Offline)][image17]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Offline bar** | "You are offline. Changes cannot be saved until the connection returns." |
| **Retry now link** | Checks the connection at once. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Retry now** | If the connection is back, the bar disappears with a short "Back online" message. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Submit buttons** | Disabled while offline with the hint "No connection". |
| **Photo upload in progress** | Pauses and continues by itself when the connection returns; if it fails, an error appears on that photo with a Retry link. |

**Rules**

* The web app does not work offline. It only prevents data loss and explains what happened.

**Where next**

Stays on the current page.

## **10.3 Shared screens (COM)**

### **COM-01  App Shell (common layout)**

| Type | Layout |
| :---- | :---- |
| **Module** | Shared screens (COM) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Shown on every page except [AUTH-01](#bookmark=id.8yzrz7fdv0n7) to [AUTH-03](#bookmark=id.kzmn6civei1f) and [AUTH-06](#bookmark=id.nyi20vb1h2mm). |

**Purpose**

The frame around every page after login: side menu, top bar and the floating AI button. It is described once here so other screens do not repeat it.

**Block layout**

![COM-01 App Shell (common layout)][image18]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Side menu** | Employee: Home, New Request, My Requests. Officer: Home, Case Queue, Case Tracking, Verification, Assets, Preventive Maintenance, Analytics. Technician: Home, My Work Orders, Work History. Manager: Home, All Cases, Analytics, Assets, Preventive Maintenance. Admin: Home, Users, Facilities & Assets, Categories, SLA, Audit Log. The current item has an orange bar on the left. |
| **Top bar: search box** | Type and press Enter to open [COM-06](#bookmark=id.npycecq01gq2). Shortcut: press / to focus. |
| **Top bar: bell** | Shows the number of unread notifications. Click opens a small list of the latest 8 with a link "See all" ([COM-05](#bookmark=id.k3ei9nwbznrg)). |
| **Top bar: avatar menu** | Name and role. Items: Profile ([COM-02](#bookmark=id.n6cz4ahexs2w)), Settings ([COM-04](#bookmark=id.xs734q1rwfk9)), Log out. |
| **Floating AI button** | Bottom-right corner on every page. Opens [COM-07](#bookmark=id.su5epyb3mo4x). |
| **Offline bar and toasts** | Yellow bar for [ERR-04](#bookmark=id.z17yjcyldi2q). Short green or red messages (toasts) appear for 4 seconds after actions. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Log out** | Ends the session, goes to [AUTH-01](#bookmark=id.8yzrz7fdv0n7). Ask to confirm only if a form has unsaved changes ([DLG-19](#bookmark=id.98dbp5b14xo5)). |
| **Collapse menu** | Small arrow at the bottom of the menu; the choice is remembered. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Narrow window (under 1024 px)** | The side menu becomes a hamburger button; tables scroll sideways. The web app is designed mainly for desktop. |
| **Unread count over 99** | The bell shows "99+". |

**Rules**

* The menu shows only what the role can use (see the permission table).

* The Administrator has no Case pages in the menu.

**Where next**

Every other screen.

### **COM-02  Profile**

| Type | Page |
| :---- | :---- |
| **Module** | Shared screens (COM) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Avatar menu → Profile. |

**Purpose**

Shows and lets users edit their own basic information.

**Block layout**

![COM-02 Profile][image19]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Avatar** | Upload JPG or PNG up to 2 MB. If none, the initials are shown. |
| **Full name** | Required, 2 to 60 characters. |
| **Phone** | Optional. Digits, spaces, \+ and \- only, up to 15 digits. |
| **Email, role, department, employee code** | Shown but greyed out. Only the Administrator can change them ([ADM-03](#bookmark=id.l5d1kw4uy1i1)). |
| **Change password card** | Button that opens [COM-03](#bookmark=id.gpq9qcuy2jzw). |
| **Last sign-in info** | Date and time of the last sign-in. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save changes** | Saves, shows a green toast "Profile updated". The name in the top bar updates at once. |
| **Upload / Remove avatar** | Changes the picture after a preview. |
| **Cancel** | Restores the saved values. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Invalid value** | Red text under the field, Save disabled. |
| **Unsaved changes and leaving** | [DLG-19](#bookmark=id.98dbp5b14xo5) asks to keep editing or discard. |
| **Picture too big or wrong type** | "Use a JPG or PNG under 2 MB." |
| **Save fails** | Red banner, values kept. |

**Rules**

* Users can edit only their own profile.

* Changes are written to the Audit Log.

**Where next**

To [COM-03](#bookmark=id.gpq9qcuy2jzw) or [COM-04](#bookmark=id.xs734q1rwfk9).

### **COM-03  Change Password**

| Type | Page |
| :---- | :---- |
| **Module** | Shared screens (COM) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Profile → Change password. |

**Purpose**

Lets a signed-in user change their password by knowing the old one.

**Block layout**

![COM-03 Change Password][image20]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Three password fields** | Current, new, confirm. Show or hide option on each. |
| **Rules checklist** | Same live checklist as [AUTH-03](#bookmark=id.kzmn6civei1f). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Update password** | Changes the password, keeps this session, ends all other sessions of the user, sends a confirmation email, shows a green toast. |
| **Cancel** | Back to Profile. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Wrong current password** | "Current password is incorrect." After 5 wrong tries the user is signed out. |
| **Weak, mismatched or reused password** | Same messages as [AUTH-03](#bookmark=id.kzmn6civei1f). |

**Rules**

* The new password must be different from the current one.

**Where next**

Back to [COM-02](#bookmark=id.n6cz4ahexs2w).

### **COM-04  Personal Settings**

| Type | Page |
| :---- | :---- |
| **Module** | Shared screens (COM) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Avatar menu → Settings. |

**Purpose**

Personal choices that change how the app looks and which notifications the user receives.

**Block layout**

![COM-04 Personal Settings][image21]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Theme choice** | Three options with a live preview. Applies at once. |
| **Language choice** | Switches all labels between English and Vietnamese. |
| **Notification table** | One row per kind of notification the role can receive. Two switches per row: "In the app" and "Email". In-app cannot be turned off for important events (assignments, verification). |
| **Save button** | Appears when something changed. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save** | Stores the choices; green toast. |
| **Reset to default** | Restores the default choices after a confirmation. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Save fails** | Red banner, the switches return to their old positions. |
| **Locked switches** | Grey with a small lock and a tooltip "Required for your role". |

**Rules**

* Theme and language are also remembered in the browser so the login page uses them.

**Where next**

Back to the previous page.

### **COM-05  Notification Center**

| Type | Page |
| :---- | :---- |
| **Module** | Shared screens (COM) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Bell icon → See all, or a click on a notification email link. |

**Purpose**

One place to see all notifications and open the related case or record.

**Block layout**

![COM-05 Notification Center][image22]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Tabs** | All, Unread. Optional filter by type (Case, Preventive, Account). |
| **Notification row** | Coloured dot if unread, icon by type, one sentence (for example "CASE-1042 was approved"), relative time. Click opens the related page and marks it as read. |
| **Mark all as read** | Marks every notification as read. |
| **Row menu** | Mark as read or unread, Delete. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Click a notification** | Opens the target (usually [COM-08](#bookmark=id.83jeomyy0mjj)) and marks it read. |
| **Mark all as read** | All dots disappear, bell count becomes 0\. |
| **Delete** | Removes it from the user's list only. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty** | "You are all caught up." with a light illustration. |
| **Loading** | Grey placeholder rows. |
| **Target no longer available** | Opens [ERR-02](#bookmark=id.gqj1o5u9y4bl) (or [ERR-01](#bookmark=id.lmhdh2gy047x) if the user lost access), the notification stays in the list. |
| **Load fails** | "Could not load notifications. Retry." |

**Rules**

* New notifications appear within about 30 seconds without reloading.

* Notifications are kept for 90 days.

* Each role only receives its own events (see the notification table at the end of this document).

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj) or the related page.

### **COM-06  Global Search and Advanced Filters**

| Type | Page |
| :---- | :---- |
| **Module** | Shared screens (COM) |
| **Who can open it** | Employee (Scoped), Officer (Scoped), Technician (Scoped), Manager (Scoped), Admin (Scoped) |
| **Where it comes from** | Type in the top bar search box and press Enter, or choose a suggestion. |

**Purpose**

Finds cases, assets and users quickly from anywhere. Each user only finds what their role may see.

**Block layout**

![COM-06 Global Search and Advanced Filters][image23]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Search box with suggestions** | After 2 characters, shows up to 5 quick suggestions (case code, title, asset code). |
| **Result tabs** | Cases for everyone. Assets for Officer, Manager, Administrator. Users for Administrator only. |
| **Advanced filters** | Status, priority, building, room, asset type, technician (Officer and Manager), requester (Officer and Manager), date range, only overdue, only preventive. |
| **Result table** | Columns depend on the tab. Click a row to open the detail page. |
| **Clear filters** | Resets everything except the search words. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Search** | Shows results grouped by tab with counts. |
| **Apply / clear filters** | Refreshes the table; filter choices appear as small chips above it. |
| **Sort by column** | Click a column title. |
| **Open a result** | Goes to [COM-08](#bookmark=id.83jeomyy0mjj), [AST-03](#bookmark=id.j6n8nmgqzl9d) or [ADM-04](#bookmark=id.czm9bezfxzhx). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No results** | "No results for ..." with tips: check spelling, remove filters. |
| **Search word too short** | "Type at least 2 characters." |
| **Too many results** | Results are paged, 20 per page, with the total count. |
| **Search fails** | "Search is not available now. Retry." |

**Rules**

* Employees only find their own cases. Technicians only find cases assigned to them. Officers and Managers find all cases. The Administrator finds no cases.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj), [AST-03](#bookmark=id.j6n8nmgqzl9d) or [ADM-04](#bookmark=id.czm9bezfxzhx).

### **COM-07  AI Assistant (floating chat)**

| Type | Page |
| :---- | :---- |
| **Module** | Shared screens (COM) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Click the round AI button at the bottom-right corner of any page. |

**Purpose**

A chat window where any user can ask questions. The AI only answers with information that the user's own role is already allowed to see. It gives advice and never makes decisions.

**Block layout**

![COM-07 AI Assistant (floating chat)][image24]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Scope note** | A small grey line, for example: "I can help with your own requests and how to use the app." The text changes by role. |
| **Suggested questions** | Employee: "Where is my request?", "How do I add a photo?". Technician: "How to check a compressor?" for the asset type of my current work order. Officer: "Summarise today's pending cases.". Manager: "Why did response time go up this week?" (based on dashboard numbers). Administrator: "How do I add a new asset type?" |
| **Message list** | Your messages on the right, AI answers on the left. Each AI answer has the small note "AI can make mistakes. Check important information." |
| **Thinking indicator** | Three moving dots "AI is thinking..." while the answer is prepared. The answer is not instant, it may take a few seconds. |
| **Rating buttons** | Thumbs up and thumbs down under each answer. Thumbs down opens a short choice: Wrong, Not helpful, Shows information I should not see, Other. |
| **Input box and Send** | Up to 500 characters with a counter. Enter sends, Shift+Enter makes a new line. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Send a question** | The message appears at once, then the thinking indicator, then the answer. |
| **Click a suggested question** | Sends it as a normal question. |
| **Rate an answer** | Saves the rating and shows "Thanks for your feedback". |
| **New chat** | Clears the window and starts a fresh conversation (the old one stays in the Audit Log). |
| **Cancel while thinking** | A small Stop button stops waiting for the answer. |
| **Close** | Hides the window; the conversation is kept until sign-out. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty (first open)** | Welcome text, scope note and suggested questions. |
| **Waiting for the answer** | Thinking indicator. The chat checks the result again every 2 seconds. |
| **Answer takes too long (over 60 s)** | "This is taking longer than usual." with Keep waiting and Cancel. After 2 minutes: "No answer received. Please try again." with a Retry button. |
| **AI service error** | "The assistant is not available right now." The rest of the app works normally. |
| **Out of scope question** | The AI politely refuses: "I cannot help with that for your role. You can ask about ..." and shows two suggestions. Example: an Employee asks about another person's case. |
| **Too many questions** | Limit of 20 questions per hour per user: "You reached the limit. Try again in N minutes." |
| **Offline** | Send is disabled with the hint "No connection". |
| **Session expired** | [AUTH-05](#bookmark=id.dy9vq2ywyc10) appears. |

**Rules**

* The AI receives only data filtered by the same rules the user's role already has. It can never open more than the user can.

* Every question and every answer is written to the Audit Log (the request and the final answer are saved separately).

* The AI only suggests. It has no button to approve, assign or close anything.

* Because the AI service works in two steps (send request, then collect result), the window must show the waiting state clearly.

**Where next**

Stays on the current page, may show links to [COM-08](#bookmark=id.83jeomyy0mjj) for the user's own cases.

### **COM-08  Case Detail**

| Type | Page |
| :---- | :---- |
| **Module** | Shared screens (COM) |
| **Who can open it** | Employee (Scoped), Officer (Full), Manager (Read only) |
| **Where it comes from** | From My Requests ([EMP-03](#bookmark=id.53a1f1joukxn)), Case Queue ([OFF-02](#bookmark=id.x3zeg4xiveou)), Case Tracking ([OFF-03](#bookmark=id.n0o9xiciqqd7)), Verification Queue ([OFF-04](#bookmark=id.lpy4qqom276w)), Asset detail ([AST-03](#bookmark=id.j6n8nmgqzl9d)), Inspection record ([PM-06](#bookmark=id.dk34aamyou4v)), notifications, search, or AI answers. Technicians use [TEC-03](#bookmark=id.dkkwhpnr6ifa) instead for their own work orders. |

**Purpose**

The single page that shows everything about one maintenance case: what was reported, who handles it, what was done, the history and the discussion. What each role sees and can do depends on its role and the case status.

**Block layout**

![COM-08 Case Detail][image25]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Header** | Case code (like CASE-1042), title, status pill (colour \+ icon \+ text), priority pill, and small tags: Preventive, Reopened, Overdue, Reopen requested, Claimed by (name). |
| **Progress stepper** | Submitted → Approved → Assigned → In Progress → Completed → Closed. Special ends (Rejected, Cancelled) and On Hold are shown as a note on the bar. |
| **Request information** | Requester (and "entered by Officer" if created on behalf), created time, building, floor, room, asset (link to [AST-03](#bookmark=id.j6n8nmgqzl9d) for staff), suggested urgency, description, photos with a viewer ([DLG-20](#bookmark=id.p1oyfarais6j)). |
| **People and location card** | Requester, Officer who handles it, assigned Technician with phone number (visible to Officer and Employee). |
| **SLA card** | First response target and resolve target with a countdown or "overdue by 3h". Visible to Officer and Manager only. |
| **Work report** | Appears once the Technician has entered data: root cause, parts used (name, quantity), labor hours, evidence photos, notes. Hidden from the Employee until the case is Completed or Closed; the Employee sees a short summary and the photos. |
| **Timeline** | Every change in order: who, what, when, plus the reason text for Reject, Need more info, Reassign, Reopen, On Hold. |
| **Comments** | Chat-like thread with a text box, up to 1000 characters, and optional photo. Officer and Technician get the tick box "Internal note (staff only)" which hides the comment from the Employee. |
| **Rating** | After Closed, shows the Employee's stars and comment, or a button "Rate this service" for the Employee. |
| **Action panel** | Buttons change with role and status (see the Actions table). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Employee, status Pending or Need more info: Edit / Add information** | Opens [EMP-04](#bookmark=id.1wkdzvdv2xi8). |
| **Employee, Pending or Need more info: Cancel request** | Opens [DLG-12](#bookmark=id.ck085r6ldanm). |
| **Employee, Closed within 7 days: Not fixed** | Opens [DLG-13](#bookmark=id.3h2jh6omhwpq). |
| **Employee, Closed and not rated: Rate this service** | Opens [DLG-14](#bookmark=id.qg8dy46jzlj2). |
| **Officer, Pending: Claim / Release** | Claiming marks the case "being handled by (name)" so nobody else works on it. See [DLG-22](#bookmark=id.60m6ytailped). |
| **Officer, Pending: Approve** | Opens [DLG-01](#bookmark=id.nqyaa9ctr6re). |
| **Officer, Pending: Reject** | Opens [DLG-02](#bookmark=id.2pg2riu4kig1). |
| **Officer, Pending: Need more info** | Opens [DLG-03](#bookmark=id.mmdngvj5b2et). |
| **Officer, Approved or Reopened: Assign technician** | Opens [DLG-04](#bookmark=id.6bnps7bwaz0o). |
| **Officer, Assigned to On Hold: Reassign technician** | Opens [DLG-05](#bookmark=id.pqnz92ls7h1u). |
| **Officer, Completed: Verify and close** | Opens [DLG-06](#bookmark=id.ni3tomkmvn69). |
| **Officer, Completed or Closed with a reopen request: Reopen** | Opens [DLG-07](#bookmark=id.xisa6ciajrx1). "Decline" opens [DLG-08](#bookmark=id.d7csm922kxo8). |
| **Officer, before Completed: Cancel case** | Opens [DLG-12](#bookmark=id.ck085r6ldanm). |
| **Officer and Technician: Add comment** | Posts to the thread. |
| **Employee: Add comment** | Allowed until the case is Closed, and for 7 days after (to report the fix). |
| **Manager: view only** | No action buttons, no comments. Sees the full page including internal notes. |
| **Print / Export PDF** | Available to Officer and Manager. Produces a one-page summary of the case. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Loading** | Grey placeholder blocks for header, info and timeline. |
| **Case not found or no permission** | [ERR-02](#bookmark=id.gqj1o5u9y4bl) or [ERR-01](#bookmark=id.lmhdh2gy047x). |
| **Someone else changed the case** | Blue bar: "This case was updated by (name). Refresh to see the latest." Action buttons are disabled until refreshed. |
| **Case claimed by another Officer** | Orange bar: "Being handled by (name) since 10:20." Decision buttons are disabled for other Officers, with a link "Ask to release" (adds a comment). |
| **Rejected or Cancelled case** | Read only. The reason is shown in a red or grey box below the header. |
| **On Hold** | Yellow box with the reason and the expected date to resume. |
| **Overdue** | Red "Overdue" tag and red countdown in the SLA card. |
| **Comment fails to send** | The text stays in the box with a red "Not sent. Retry". |
| **Photo cannot be shown** | Grey tile with "Image unavailable". |

**Rules**

* Employees see only their own cases. Technicians see only cases assigned to them (via [TEC-03](#bookmark=id.dkkwhpnr6ifa)). Officers and Managers see all cases. The Administrator cannot open cases.

* Every action needs the right role and the right status; the buttons that do not apply are hidden, not just disabled.

* Actions with a reason (Reject, Need more info, Reassign, Reopen, On Hold, Cancel) require at least 10 characters.

* All actions are written to the Timeline and the Audit Log.

**Where next**

To [EMP-04](#bookmark=id.1wkdzvdv2xi8), [TEC-03](#bookmark=id.dkkwhpnr6ifa), [AST-03](#bookmark=id.j6n8nmgqzl9d), [PM-06](#bookmark=id.dk34aamyou4v) and the dialogs [DLG-01](#bookmark=id.nqyaa9ctr6re) to [DLG-14](#bookmark=id.qg8dy46jzlj2), [DLG-22](#bookmark=id.60m6ytailped).

## **10.4 Employee (EMP)**

### **EMP-01  Employee Home**

| Type | Page |
| :---- | :---- |
| **Module** | Employee (EMP) |
| **Who can open it** | Employee (Full) |
| **Where it comes from** | First page after sign-in, or menu → Home. |

**Purpose**

A short overview for the Employee: what needs attention and the latest requests.

**Block layout**

![EMP-01 Employee Home][image26]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Number tiles** | Open requests, Need my input (Need more info), Waiting for my rating (Closed and not rated), Closed this month. Each tile opens [EMP-03](#bookmark=id.53a1f1joukxn) with the matching filter. |
| **Needs your attention** | Shows requests where the Officer asked for information, and closed requests that can still be rated. Each line has one button (Reply, Rate). |
| **Latest requests** | Five most recently updated: code, title, place, status pill, time. |
| **New request button** | Big orange button. Opens [EMP-02](#bookmark=id.v4er16ngh3i9). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Click a tile** | Opens [EMP-03](#bookmark=id.53a1f1joukxn) filtered. |
| **Click a request** | Opens [COM-08](#bookmark=id.83jeomyy0mjj). |
| **Reply / Rate** | Opens [EMP-04](#bookmark=id.1wkdzvdv2xi8) or [DLG-14](#bookmark=id.qg8dy46jzlj2). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **New user, no request yet** | Friendly text "Report your first issue" and the New request button. |
| **Nothing needs attention** | "You are all caught up." |
| **Loading / load error** | Grey placeholders; each block can show "Could not load. Retry." on its own. |

**Rules**

* Numbers only count the Employee's own requests.

**Where next**

To [EMP-02](#bookmark=id.v4er16ngh3i9), [EMP-03](#bookmark=id.53a1f1joukxn), [EMP-04](#bookmark=id.1wkdzvdv2xi8), [COM-08](#bookmark=id.83jeomyy0mjj), [DLG-14](#bookmark=id.qg8dy46jzlj2).

### **EMP-02  New Request**

| Type | Page |
| :---- | :---- |
| **Module** | Employee (EMP) |
| **Who can open it** | Employee (Full) |
| **Where it comes from** | Menu → New Request, the button on Home, or by scanning the QR label on an asset ([AST-07](#bookmark=id.z84288y2n6n5)), which fills in the place and the asset for the user. |

**Purpose**

Lets an Employee report a facility problem so that it becomes a new Case.

**Block layout**

![EMP-02 New Request][image27]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Building, Floor, Room** | Three linked dropdowns. Choosing a building fills the floors; choosing a floor fills the rooms. Building and Room are required; Floor helps to narrow the list. |
| **Asset** | Optional dropdown with search, listing assets in the chosen room (name \+ code). Option "Not in the list / I am not sure" is available. |
| **Issue type** | Required. Values come from the maintenance categories set by the Administrator (for example Air conditioning, Lighting, Plumbing, Network, Furniture, Other). |
| **Title** | Required, 5 to 100 characters. Example: "Air conditioner not cooling". |
| **Description** | Required, 10 to 1000 characters with a counter. Hint: what is wrong, since when, what you already tried. |
| **Photos** | Optional, up to 5 photos, JPG/PNG/WEBP, 5 MB each. Drag and drop or click. Thumbnails with a small X to remove and a progress bar while uploading. |
| **Suggested urgency** | Low, Normal (default), High, Urgent, with one line of help for each. It is only a suggestion; the Officer sets the final priority. |
| **Contact phone** | Pre-filled from the profile, can be changed for this request. |
| **Duplicate warning** | If the chosen asset already has an open request, a yellow box shows it (code, status) with a link. The user can still continue. |
| **Submit and Cancel buttons** | Submit is disabled while sending. Cancel returns to the previous page (asks [DLG-19](#bookmark=id.98dbp5b14xo5) if the form is not empty). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Submit request** | Creates the Case with status Pending, shows a success view "Request CASE-1050 sent" with the buttons View request and Report another issue. The Officers are notified. |
| **Add / remove photo** | Uploads at once; a failed photo shows a red mark with Retry. |
| **Change the building or room** | Clears the asset choice. |
| **Cancel** | Goes back. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Required field empty** | Red text under each field, the page scrolls to the first one. |
| **Building has no rooms or room has no assets** | Room list shows "No rooms yet, contact Administrator". Asset list shows only "Not in the list". |
| **Photo too big or wrong type** | "Each photo must be JPG, PNG or WEBP under 5 MB." |
| **More than 5 photos** | "You can add up to 5 photos." |
| **QR code asset is retired or not found** | See [AST-07](#bookmark=id.z84288y2n6n5). The form opens without the asset and a yellow note explains why. |
| **Submit fails** | Red banner "Could not send. Your text and photos are kept. Retry." |
| **Sending twice by mistake** | The button is disabled after the first click, so only one Case is created. |

**Rules**

* An Employee can send many requests but each request describes one problem.

* The Case code is created by the system and cannot be edited.

* Photos are stored securely and are visible only to people allowed to see the case.

**Where next**

To the success view, then [COM-08](#bookmark=id.83jeomyy0mjj) or [EMP-02](#bookmark=id.v4er16ngh3i9) again. Cancel goes to the previous page.

### **EMP-03  My Requests**

| Type | Page |
| :---- | :---- |
| **Module** | Employee (EMP) |
| **Who can open it** | Employee (Full) |
| **Where it comes from** | Menu → My Requests, or a tile on Home. |

**Purpose**

The Employee's list of all their requests with their current status.

**Block layout**

![EMP-03 My Requests][image28]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Tabs** | Show a small count. "Need my input" is highlighted red when it is not empty. |
| **Search box** | Finds by code or words in the title. |
| **Filters** | Status, building, created date range, priority. |
| **Table columns** | Code, Title, Place (building · room), Status pill, Priority, Last updated. A small star icon shows if a rating is still possible. |
| **Row buttons** | Reply (Need more info), Rate (Closed), Cancel (Pending). |
| **New request button** | Top right. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Open a request** | Opens [COM-08](#bookmark=id.83jeomyy0mjj). |
| **Reply** | Opens [EMP-04](#bookmark=id.1wkdzvdv2xi8). |
| **Cancel** | Opens [DLG-12](#bookmark=id.ck085r6ldanm). |
| **Rate** | Opens [DLG-14](#bookmark=id.qg8dy46jzlj2). |
| **Sort** | Default: newest update first. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty (no requests)** | "You have not sent any request yet." and the New request button. |
| **Empty after filtering** | "No requests match your filters." with Clear filters. |
| **Loading** | Grey table rows. |
| **Load fails** | Red banner with Retry. |
| **Status changed while viewing** | The row updates within about 30 seconds and briefly highlights. |

**Rules**

* Only the Employee's own requests are listed, including ones an Officer entered on their behalf.

* Twenty rows per page.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj), [EMP-02](#bookmark=id.v4er16ngh3i9), [EMP-04](#bookmark=id.1wkdzvdv2xi8), [DLG-12](#bookmark=id.ck085r6ldanm), [DLG-14](#bookmark=id.qg8dy46jzlj2).

### **EMP-04  Edit Request / Provide More Information**

| Type | Page |
| :---- | :---- |
| **Module** | Employee (EMP) |
| **Who can open it** | Employee (Full) |
| **Where it comes from** | From [COM-08](#bookmark=id.83jeomyy0mjj), [EMP-03](#bookmark=id.53a1f1joukxn), [EMP-01](#bookmark=id.gevp1q7fiais), or from a notification saying more information is needed. |

**Purpose**

Lets the Employee change a request that is not yet approved, or answer the Officer's question when the status is Need more info.

**Block layout**

![EMP-04 Edit Request / Provide More Information][image29]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Officer's question box** | Only when the status is Need more info. Shows who asked, when, and the full question. |
| **Reply box** | Required in Need more info, 10 to 1000 characters. |
| **Request fields** | Same fields and rules as [EMP-02](#bookmark=id.v4er16ngh3i9), filled with the current values. The Case code is shown at the top and cannot be changed. |
| **Photos** | Existing photos can be removed; new ones added up to 5 in total. |
| **Buttons** | "Send back" (Need more info) or "Save changes" (Pending), and Cancel. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Send back** | The status returns to Pending, the reply is added to the Timeline, and the Officer who claimed the case (or all Officers) is notified. |
| **Save changes** | For Pending requests: saves the new values and adds "edited by requester" to the Timeline. |
| **Cancel** | Back to [COM-08](#bookmark=id.83jeomyy0mjj); asks [DLG-19](#bookmark=id.98dbp5b14xo5) if there are unsaved changes. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Case already approved or closed** | The page shows "This request can no longer be edited. Add a comment instead." with a link to [COM-08](#bookmark=id.83jeomyy0mjj). |
| **Officer is reviewing at this moment** | A yellow note: "An Officer is reviewing this request. Your changes will be shown to them." Editing is still allowed. |
| **Missing required fields** | Same red messages as [EMP-02](#bookmark=id.v4er16ngh3i9). |
| **Save fails** | Red banner, all input kept. |

**Rules**

* Only the person who sent the request can use this page. If an Officer entered it on behalf of the Employee, the Employee may reply but cannot edit the Officer's original text.

**Where next**

Back to [COM-08](#bookmark=id.83jeomyy0mjj).

## **10.5 Facility Officer (OFF)**

### **OFF-01  Officer Home**

| Type | Page |
| :---- | :---- |
| **Module** | Facility Officer (OFF) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | First page after sign-in, or menu → Home. |

**Purpose**

Shows the Officer what needs a decision today.

**Block layout**

![OFF-01 Officer Home][image30]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Number tiles** | To review (Pending), To assign (Approved), To verify (Completed), Overdue (past SLA), Reopen requests. Each opens the matching list ([OFF-02](#bookmark=id.x3zeg4xiveou), [OFF-04](#bookmark=id.lpy4qqom276w), [OFF-03](#bookmark=id.n0o9xiciqqd7)). |
| **My claimed cases** | Cases this Officer claimed and has not finished, with a Release button. |
| **Oldest waiting** | Top 5 Pending cases by waiting time, each with a Claim button. |
| **On hold and rejected by technician** | Small list of cases that need the Officer to act: On Hold for more than 3 days, or assignments a Technician rejected. |
| **Preventive due this week** | Plans due or overdue, with a link to [PM-01](#bookmark=id.hubvrybhov1x). |
| **Create case for employee button** | Opens [OFF-05](#bookmark=id.gtd6k0343759). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Click a tile** | Opens the list already filtered. |
| **Claim** | Claims the case and opens [COM-08](#bookmark=id.83jeomyy0mjj). |
| **Release** | Releases a claim ([DLG-22](#bookmark=id.60m6ytailped)). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Nothing waiting** | "Great, nothing is waiting for you." |
| **Loading / load error** | Placeholders; each block shows its own Retry. |

**Rules**

* All Officers share one queue. Numbers count all cases, not only the ones the Officer claimed.

**Where next**

To [OFF-02](#bookmark=id.x3zeg4xiveou), [OFF-03](#bookmark=id.n0o9xiciqqd7), [OFF-04](#bookmark=id.lpy4qqom276w), [OFF-05](#bookmark=id.gtd6k0343759), [PM-01](#bookmark=id.hubvrybhov1x), [COM-08](#bookmark=id.83jeomyy0mjj).

### **OFF-02  Case Queue**

| Type | Page |
| :---- | :---- |
| **Module** | Facility Officer (OFF) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Menu → Case Queue. |

**Purpose**

The shared queue where new work arrives. The Officer picks a case, claims it and decides: approve, reject, ask for information, or assign a technician.

**Block layout**

![OFF-02 Case Queue][image31]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Tabs with counts** | To review \= Pending. To assign \= Approved (including preventive work orders and cases a technician rejected, shown with a tag). Reopen requests \= Employee said "Not fixed". Waiting for employee \= Need more info. |
| **Filters and search** | Code or words, building, issue type, suggested urgency, requester, only unclaimed, only overdue. |
| **List columns** | Code, Title, Place, Requester, Suggested urgency, Waiting time (red when over the first-response target), Claimed by (avatar and name), Tags. |
| **Quick preview panel** | Clicking a row shows the description, photos and asset history on the right without leaving the list, with the buttons Claim and Open. |
| **Sort** | Default: longest waiting first. Also by urgency. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Claim** | Marks the case "being handled by (you)". Other Officers see it as taken. Opens [COM-08](#bookmark=id.83jeomyy0mjj). |
| **Open** | Opens [COM-08](#bookmark=id.83jeomyy0mjj) without claiming (view only). |
| **Approve, Reject, Need more info, Assign** | Available from [COM-08](#bookmark=id.83jeomyy0mjj) after claiming, or from the preview panel: opens [DLG-01](#bookmark=id.nqyaa9ctr6re), [DLG-02](#bookmark=id.2pg2riu4kig1), [DLG-03](#bookmark=id.mmdngvj5b2et), [DLG-04](#bookmark=id.6bnps7bwaz0o). |
| **Reopen or decline (Reopen requests tab)** | Opens [DLG-07](#bookmark=id.xisa6ciajrx1) or [DLG-08](#bookmark=id.d7csm922kxo8). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty tab** | "No cases here." with a short friendly line. |
| **Someone claimed the case just before you** | The Claim button shows "Already taken by (name)" ([DLG-22](#bookmark=id.60m6ytailped)). |
| **New cases arrive while viewing** | A small bar "3 new cases. Show" appears; the list does not jump by itself. |
| **Load fails** | Red banner with Retry. |

**Rules**

* A claim is released by pressing Release, or automatically after 30 minutes without any action on the case.

* An Officer can hold several claims at once (limit of 5).

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj), [DLG-01](#bookmark=id.nqyaa9ctr6re) to [DLG-05](#bookmark=id.pqnz92ls7h1u), [DLG-07](#bookmark=id.xisa6ciajrx1), [DLG-08](#bookmark=id.d7csm922kxo8), [DLG-22](#bookmark=id.60m6ytailped).

### **OFF-03  Case Tracking**

| Type | Page |
| :---- | :---- |
| **Module** | Facility Officer (OFF) |
| **Who can open it** | Officer (Full), Manager (Read only) |
| **Where it comes from** | Officer: menu → Case Tracking, or a tile on Home. Manager: menu → All Cases, or links from Home and Analytics. |

**Purpose**

Shows every case in every status so the Officer can follow the progress and find delays; the Manager sees the same list in read-only mode.

**Block layout**

![OFF-03 Case Tracking][image32]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Tabs** | All, In progress (Assigned, Accepted, In Progress), On hold, Overdue, Completed, Closed, Rejected and Cancelled. |
| **Filters** | Technician, building, room, issue type, priority, date range, tags (Preventive, Reopened). |
| **Table columns** | Code, Title, Place, Priority, Status, Technician, Due date, SLA remaining, Last update. |
| **Row menu (Officer only)** | Open, Reassign technician, Add comment, Cancel case. |
| **Row colours** | A thin red bar on the left for overdue rows, yellow for on hold. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Open a case** | Opens [COM-08](#bookmark=id.83jeomyy0mjj). |
| **Reassign technician** | Opens [DLG-05](#bookmark=id.pqnz92ls7h1u). |
| **Cancel case** | Opens [DLG-12](#bookmark=id.ck085r6ldanm). |
| **Sort and filter** | Standard list behaviour. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty** | "No cases match." |
| **Load fails** | Red banner with Retry. |
| **Many results** | Paged, 20 per page with the total count. |

**Rules**

* The Officer reads what the Technician has entered here; the Officer does not edit the Technician's data. The Manager opens this page in read-only mode: only the Open action (COM-08 read only), no row menu; the Manager sees internal notes but cannot write comments.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj), [DLG-05](#bookmark=id.pqnz92ls7h1u), [DLG-12](#bookmark=id.ck085r6ldanm).

### **OFF-04  Verification Queue**

| Type | Page |
| :---- | :---- |
| **Module** | Facility Officer (OFF) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Menu → Verification, or the To verify tile on Home. |

**Purpose**

Lists work that the Technician marked Completed. The Officer checks the report and photos, then closes the case or reopens it.

**Block layout**

![OFF-04 Verification Queue][image33]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Completed list** | Code, Title, Place, Technician, Completed at, Waiting time, Number of photos. Oldest first. |
| **Work report preview** | Root cause, parts, labor hours, the Technician's note, and the original request side by side. |
| **Evidence photos** | Thumbnails that open the viewer ([DLG-20](#bookmark=id.p1oyfarais6j)). |
| **Buttons** | Verify and close, Reopen, Open full page. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Verify and close** | Opens [DLG-06](#bookmark=id.ni3tomkmvn69). The case becomes Closed and the Employee is notified. |
| **Reopen** | Opens [DLG-07](#bookmark=id.xisa6ciajrx1). The case returns to the Technician. |
| **Open full page** | Opens [COM-08](#bookmark=id.83jeomyy0mjj). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty** | "Nothing waiting for verification." |
| **Report incomplete** | Cannot happen normally because the Technician must fill the required fields. If old data is missing, a yellow note says "Report is incomplete" and the Officer can reopen. |
| **Another Officer verified it** | The row disappears and a toast "Already closed by (name)" is shown. |

**Rules**

* Only an Officer can close a case. A Technician cannot close their own work.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj), [DLG-06](#bookmark=id.ni3tomkmvn69), [DLG-07](#bookmark=id.xisa6ciajrx1), [DLG-20](#bookmark=id.p1oyfarais6j).

### **OFF-05  Create Case for an Employee**

| Type | Page |
| :---- | :---- |
| **Module** | Facility Officer (OFF) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Button on [OFF-01](#bookmark=id.eui2u2y7lh0t) or [OFF-02](#bookmark=id.x3zeg4xiveou). |

**Purpose**

For problems reported by phone or in person: the Officer enters the case on behalf of the Employee.

**Block layout**

![OFF-05 Create Case for an Employee][image34]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Requester** | Required. Search by name, email or employee code; only active Employees are listed. |
| **Reported via** | Required list. |
| **Case fields** | Same fields and rules as [EMP-02](#bookmark=id.v4er16ngh3i9) (place, asset, issue type, title, description, photos). |
| **Priority** | Low, Normal, High, Urgent. Because the Officer is deciding, this sets the final priority. |
| **Approve right away** | Tick box (off by default). When on, the case is created as Approved and goes to the To assign tab. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Create case** | Creates the case with the note "entered by (Officer) on behalf of (Employee)". The Employee is notified. Without "Approve right away" the case is Pending and claimed by this Officer. |
| **Cancel** | Back; asks [DLG-19](#bookmark=id.98dbp5b14xo5) if the form has data. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Requester not found** | "No active employee found." The Officer cannot continue without one. |
| **Missing required fields** | Same red messages as [EMP-02](#bookmark=id.v4er16ngh3i9). |
| **Submit fails** | Red banner, input kept. |

**Rules**

* The Timeline always shows who entered the case and how it was reported.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj) of the new case.

## **10.6 Maintenance Technician (TEC)**

### **TEC-01  Technician Home**

| Type | Page |
| :---- | :---- |
| **Module** | Maintenance Technician (TEC) |
| **Who can open it** | Technician (Full) |
| **Where it comes from** | First page after sign-in, or menu → Home. |

**Purpose**

Shows the Technician what to do today.

**Block layout**

![TEC-01 Technician Home][image35]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Number tiles** | New assignments (waiting to be accepted), In progress, On hold, Due today. Each opens [TEC-02](#bookmark=id.8l487t8fl9xe) with the matching tab. |
| **Needs your action** | New assignments with Accept and Reject buttons, and reopened cases. |
| **Today's inspections** | Preventive work orders due today with a Start inspection button ([TEC-05](#bookmark=id.ff0ok48aee2q)). |
| **My month** | Only the Technician's own numbers: work orders completed, average completion time, how many were reopened. No comparison with other technicians. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Accept** | Status becomes Accepted. |
| **Reject** | Opens [DLG-09](#bookmark=id.3u16ee70zy1w). |
| **Open a work order** | Opens [TEC-03](#bookmark=id.dkkwhpnr6ifa). |
| **Start inspection** | Opens [TEC-05](#bookmark=id.ff0ok48aee2q). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No work** | "No work assigned to you right now." |
| **Loading / load error** | Placeholders and Retry per block. |

**Rules**

* A Technician sees only their own work orders.

**Where next**

To [TEC-02](#bookmark=id.8l487t8fl9xe), [TEC-03](#bookmark=id.dkkwhpnr6ifa), [TEC-05](#bookmark=id.ff0ok48aee2q), [DLG-09](#bookmark=id.3u16ee70zy1w).

### **TEC-02  My Work Orders**

| Type | Page |
| :---- | :---- |
| **Module** | Maintenance Technician (TEC) |
| **Who can open it** | Technician (Full) |
| **Where it comes from** | Menu → My Work Orders. |

**Purpose**

The Technician's list of active work orders.

**Block layout**

![TEC-02 My Work Orders][image36]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Tabs** | New \= Assigned or Reopened (needs Accept or Reject). Accepted \= not started. In progress. On hold. Each has a count. |
| **Table columns** | Work order code (WO-2042) and Case code, Title, Place, Asset, Priority, Due date (red if overdue), Status pill, Tags (Preventive, Reopened). |
| **Row buttons** | Accept and Reject on New. Start work on Accepted. Continue on In progress. |
| **Default order** | Priority first, then due date. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Accept** | Status becomes Accepted; the Officer is informed. |
| **Reject** | Opens [DLG-09](#bookmark=id.3u16ee70zy1w). |
| **Open** | Opens [TEC-03](#bookmark=id.dkkwhpnr6ifa) (or [TEC-05](#bookmark=id.ff0ok48aee2q) for inspections). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty tab** | "Nothing here." |
| **Work order was reassigned away** | The row disappears and a toast "WO-2042 was reassigned by (Officer)" is shown. |
| **Load fails** | Red banner with Retry. |

**Rules**

* Work orders older than the due date get a red "Overdue" tag.

**Where next**

To [TEC-03](#bookmark=id.dkkwhpnr6ifa), [TEC-05](#bookmark=id.ff0ok48aee2q), [DLG-09](#bookmark=id.3u16ee70zy1w).

### **TEC-03  Work Order: Update and Complete**

| Type | Page |
| :---- | :---- |
| **Module** | Maintenance Technician (TEC) |
| **Who can open it** | Technician (Full) |
| **Where it comes from** | From [TEC-01](#bookmark=id.3lbqglcq4he0), [TEC-02](#bookmark=id.8l487t8fl9xe), [TEC-04](#bookmark=id.6l0457m09273), a notification, or a search result. |

**Purpose**

Where the Technician records the real work on site: accept, start, note the cause, parts, hours, photos, put on hold, and mark completed.

**Block layout**

![TEC-03 Work Order: Update and Complete][image37]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Header and status** | Work order code, case code, status pill, priority, due date, and tags. If Reopened, a red box shows the Officer's reason. |
| **Original request** | Title, description, photos, place, requester name, and contact phone. Read only. |
| **Asset card** | Asset name, code, type, status, and the last 3 repairs on the same asset, to help find the cause. |
| **Work status** | The buttons shown depend on the status: Accept / Reject (Assigned, Reopened), Start work (Accepted), Put on hold / Mark completed (In Progress), Resume (On Hold). |
| **Root cause** | Required to complete. A list of common causes plus "Other" with a text box; 3 to 300 characters. |
| **Parts used** | Optional table. Each row: part name, quantity, unit. Buttons Add row and Remove row. |
| **Labor hours** | Required to complete. A number greater than 0 and up to 100, steps of 0.25. |
| **Work notes** | Optional, up to 1000 characters. Describe what was done. |
| **Evidence photos** | At least 1 photo is required to complete, up to 10, each labelled Before, During or After. |
| **Timeline and comments** | Same as [COM-08](#bookmark=id.83jeomyy0mjj), including the Internal note option. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Accept** | Status Accepted. Officer notified. |
| **Reject** | Opens [DLG-09](#bookmark=id.3u16ee70zy1w). The case goes back to the Officer. |
| **Start work** | Status In Progress; start time is stored. |
| **Save progress** | Saves what was typed. Also saved automatically every 30 seconds and when leaving the page. |
| **Put on hold** | Opens [DLG-10](#bookmark=id.g5i37a8no9xn) (reason and expected date). |
| **Resume** | From On Hold back to In Progress. |
| **Mark completed** | Checks the required fields, then opens [DLG-11](#bookmark=id.v914lp4rv88u) to confirm. The case becomes Completed and the Officer is notified. |
| **Add comment / photo** | Posts to the thread. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Missing required fields when completing** | Red text under Root cause, Labor hours or Evidence photos; the page scrolls to the first one. The case stays In Progress. |
| **Photo upload fails** | Red mark on the photo with Retry. Completing is blocked until all photos are uploaded. |
| **Reassigned by the Officer while editing** | A red bar: "This work order was reassigned." The page turns read only. Typed data is kept as a draft note in the Timeline. |
| **Completed or Closed** | Everything is read only. Only comments are allowed while Completed. |
| **Someone else edited the case** | Blue bar "Updated by (name). Refresh." like [COM-08](#bookmark=id.83jeomyy0mjj). |
| **Unsaved changes and leaving** | [DLG-19](#bookmark=id.98dbp5b14xo5) asks; auto-save has usually saved most of it already. |

**Rules**

* A Technician can open only work orders assigned to them.

* Completing needs: root cause, labor hours above 0, and at least one evidence photo.

* Completed does not mean Closed. The Officer must verify.

* The Technician cannot change the original request, the priority, or the technician assignment.

**Where next**

To [TEC-02](#bookmark=id.8l487t8fl9xe), [TEC-04](#bookmark=id.6l0457m09273), [DLG-09](#bookmark=id.3u16ee70zy1w), [DLG-10](#bookmark=id.g5i37a8no9xn), [DLG-11](#bookmark=id.v914lp4rv88u), [DLG-20](#bookmark=id.p1oyfarais6j).

### **TEC-04  Work History**

| Type | Page |
| :---- | :---- |
| **Module** | Maintenance Technician (TEC) |
| **Who can open it** | Technician (Full) |
| **Where it comes from** | Menu → Work History. |

**Purpose**

Lets the Technician look back at finished work orders and find what they did on an asset before.

**Block layout**

![TEC-04 Work History][image38]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Summary tiles** | For the chosen date range: number completed, total labor hours, number reopened. |
| **History table** | Work order and case code, Title, Asset, Completed date, Labor hours, Final status (Completed, Closed, Reopened). |
| **Filters** | Date range (default last 30 days), building, asset type, preventive or repair. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Open a row** | Opens [TEC-03](#bookmark=id.dkkwhpnr6ifa) in read only mode. |
| **Filter and sort** | Standard list behaviour. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty** | "No finished work in this period." |
| **Load fails** | Red banner with Retry. |

**Rules**

* Only the Technician's own history is shown.

**Where next**

To [TEC-03](#bookmark=id.dkkwhpnr6ifa).

### **TEC-05  Inspection Checklist (preventive)**

| Type | Page |
| :---- | :---- |
| **Module** | Maintenance Technician (TEC) |
| **Who can open it** | Technician (Full) |
| **Where it comes from** | From [TEC-01](#bookmark=id.3lbqglcq4he0) or [TEC-02](#bookmark=id.8l487t8fl9xe) for work orders that carry the tag Preventive. |

**Purpose**

For preventive work orders: the Technician goes through the checklist of a plan and records the result for each item.

**Block layout**

![TEC-05 Inspection Checklist (preventive)][image39]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Header** | Plan name, asset with place, due date, frequency. |
| **Checklist items** | Each row: the check to do, a three-way choice Pass, Fail, N/A, a note box (required when Fail), and a photo button (photo required when Fail). |
| **Overall result** | Chosen automatically: Passed if no item failed, Needs attention if any failed. The Technician can add a note. |
| **Report a problem found** | Shown when at least one item failed. A tick box "Create a repair request for the failed items" pre-fills a new Case with the asset, notes and photos; it goes to the Officer queue as Pending. |
| **Progress bar** | "7 of 10 items checked". |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save progress** | Saves the answers; also auto-saves every 30 seconds. |
| **Complete inspection** | Only when every item has an answer. Creates the Inspection Record ([PM-05](#bookmark=id.j0baje6voyco), [PM-06](#bookmark=id.dk34aamyou4v)), moves the work order to Completed, and notifies the Officer. |
| **Put on hold** | Opens [DLG-10](#bookmark=id.g5i37a8no9xn). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Unanswered items** | Those rows are outlined in red; the button stays disabled. |
| **Fail without note or photo** | Red text under that row. |
| **Plan changed after the work order was created** | The checklist shown is the version at the time of creation; a grey note says so. |
| **Save fails** | Red banner, answers kept. |

**Rules**

* Preventive work orders skip the Pending review. They start as Approved (waiting for the Officer to assign) or already Assigned when the plan has a default technician (see [PM-02](#bookmark=id.u6drodgl2m81)).

* After Completed, the Officer verifies like any other case.

**Where next**

To [TEC-02](#bookmark=id.8l487t8fl9xe), [PM-06](#bookmark=id.dk34aamyou4v), or [COM-08](#bookmark=id.83jeomyy0mjj) of the new repair case.

## **10.7 Facility Manager (MGR)**

### **MGR-01  Manager Home**

| Type | Page |
| :---- | :---- |
| **Module** | Facility Manager (MGR) |
| **Who can open it** | Manager (Full) |
| **Where it comes from** | First page after sign-in, or menu → Home. |

**Purpose**

A quick look at how maintenance is performing across all buildings, and what needs attention.

**Block layout**

![MGR-01 Manager Home][image40]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **KPI tiles** | Open cases, Overdue, Average first response time, Average resolution time, Reopen rate, Average rating. Each tile shows the change against last week (up or down arrow with a word, not only colour). |
| **Charts** | Cases per week and cases by building. Hover shows exact numbers. |
| **Needs attention** | Cases overdue for more than 24 hours, cases On Hold for more than 3 days, preventive plans overdue, assets with 3 or more repairs in 30 days. |
| **Links to Analytics** | "See full dashboard" opens [ANA-01](#bookmark=id.qschmq5ovp16). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Click a tile or chart bar** | Opens [ANA-01](#bookmark=id.qschmq5ovp16) or [OFF-03](#bookmark=id.n0o9xiciqqd7) with the filter applied. |
| **Click a line in Needs attention** | Opens [COM-08](#bookmark=id.83jeomyy0mjj), [AST-03](#bookmark=id.j6n8nmgqzl9d) or [PM-03](#bookmark=id.rfx2okd8binq). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No data yet** | Charts show "Not enough data yet". |
| **Nothing needs attention** | "All good." |
| **Loading / load error** | Placeholders; each block has its own Retry. |

**Rules**

* The Manager has read-only access. There is no approval or assignment on this page.

* Numbers use the same definitions as [ANA-01](#bookmark=id.qschmq5ovp16).

**Where next**

To [ANA-01](#bookmark=id.qschmq5ovp16), [ANA-02](#bookmark=id.h2nhwbchtidg), [ANA-03](#bookmark=id.cwmp2fxt8ju0), [OFF-03](#bookmark=id.n0o9xiciqqd7), [COM-08](#bookmark=id.83jeomyy0mjj), [AST-03](#bookmark=id.j6n8nmgqzl9d), [PM-03](#bookmark=id.rfx2okd8binq).

## **10.8 Administrator (ADM)**

### **ADM-01  Administrator Home**

| Type | Page |
| :---- | :---- |
| **Module** | Administrator (ADM) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | First page after sign-in, or menu → Home. |

**Purpose**

Gives the Administrator a quick view of accounts, data quality and recent activity.

**Block layout**

![ADM-01 Administrator Home][image41]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Number tiles** | Active users, locked or deactivated users, users who never signed in, and counts of buildings, rooms and assets. |
| **Users by role** | A small bar for each of the five roles. |
| **Data checks** | Warnings such as: rooms with no assets, assets without a type, issue types never used, Technicians with no specialty. Each line links to the place to fix it. |
| **Recent audit events** | The latest 10 events (sign-ins, role changes, configuration changes) with a link to [ADM-08](#bookmark=id.382mbmgltcob). |
| **Quick links** | Add user, Add asset, Import assets. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Click a tile or a data check** | Opens the related list, filtered. |
| **Quick links** | Opens [ADM-03](#bookmark=id.l5d1kw4uy1i1), [AST-04](#bookmark=id.615v8i8ga3hr) or [AST-05](#bookmark=id.xz1mbdpv5xr3). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No warnings** | "Your data looks complete." |
| **Loading / load error** | Placeholders; each block has its own Retry. |

**Rules**

* The Administrator does not see any cases or case numbers here, to keep case content private.

**Where next**

To [ADM-02](#bookmark=id.84apprc917i0), [ADM-03](#bookmark=id.l5d1kw4uy1i1), [ADM-08](#bookmark=id.382mbmgltcob), [AST-01](#bookmark=id.r397yhsl2mmw), [AST-02](#bookmark=id.vfpdk57qamqs), [AST-04](#bookmark=id.615v8i8ga3hr), [AST-05](#bookmark=id.xz1mbdpv5xr3).

### **ADM-02  User List**

| Type | Page |
| :---- | :---- |
| **Module** | Administrator (ADM) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Menu → Users. |

**Purpose**

Shows every account and lets the Administrator search, filter and manage them.

**Block layout**

![ADM-02 User List][image42]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Search and filters** | By name, email or employee code; role; status (Active, Deactivated, Locked, Not signed in yet); department. |
| **Table columns** | Avatar and name, Email, Role, Department, Status pill, Last sign-in. |
| **Row menu** | View, Edit, Reset password, Unlock, Deactivate or Reactivate. |
| **Add user button** | Top right. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Add user** | Opens [ADM-03](#bookmark=id.l5d1kw4uy1i1) in create mode. |
| **View** | Opens [ADM-04](#bookmark=id.czm9bezfxzhx). |
| **Edit** | Opens [ADM-03](#bookmark=id.l5d1kw4uy1i1) in edit mode. |
| **Reset password, Unlock, Deactivate, Reactivate** | Opens [DLG-16](#bookmark=id.x23yp780kgp) to confirm. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty after filtering** | "No users match." |
| **Load fails** | Red banner with Retry. |
| **Own account row** | Deactivate is hidden, so the Administrator cannot lock themselves out. |

**Rules**

* At least one active Administrator must always exist.

**Where next**

To [ADM-03](#bookmark=id.l5d1kw4uy1i1), [ADM-04](#bookmark=id.czm9bezfxzhx), [DLG-16](#bookmark=id.x23yp780kgp).

### **ADM-03  Create / Edit User**

| Type | Page |
| :---- | :---- |
| **Module** | Administrator (ADM) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Add user on [ADM-02](#bookmark=id.84apprc917i0), or Edit on [ADM-02](#bookmark=id.84apprc917i0) / [ADM-04](#bookmark=id.czm9bezfxzhx). |

**Purpose**

Creates a new account or changes an existing one, including the role.

**Block layout**

![ADM-03 Create / Edit User][image43]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Full name** | Required, 2 to 60 characters. |
| **Email** | Required, valid format, unique in the system. Cannot be changed after creation if the user has history (shown greyed). |
| **Role** | Required. One of Employee, Facility Officer, Maintenance Technician, Facility Manager, Administrator. A short line explains what the role can do. |
| **Department, Employee code, Phone** | Department optional. Employee code optional but unique. Phone optional. |
| **Specialty** | Appears only for the Technician role. One or more issue types (for example Air conditioning, Electrical) to help the Officer choose whom to assign. |
| **Status (edit only)** | Active or Deactivated. |
| **Temporary password (create only)** | The system creates one. Options: "Show it to me once" and "Send it by email to the user" (default on). The user must change it at first sign-in ([AUTH-04](#bookmark=id.kdpgutl9nzf5)). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save** | Create mode: creates the account and shows a result box with the email and the temporary password (if chosen). Edit mode: saves the changes. |
| **Cancel** | Back; asks [DLG-19](#bookmark=id.98dbp5b14xo5) if changed. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Email already used** | "This email already has an account." |
| **Employee code already used** | "This code belongs to another user." |
| **Changing the role of a user with open work** | Blocked with: "This user has 3 active work orders. Reassign them first." Same for a Technician being deactivated. For an Employee, open requests stay but keep working. |
| **Removing the last Administrator** | Blocked: "There must be at least one Administrator." |
| **Email service fails** | The account is still created; the result box says "Email not sent" and shows the temporary password so it can be given by hand. |
| **Save fails** | Red banner, input kept. |

**Rules**

* The temporary password is shown only once and is never stored in readable form.

* Role changes take effect at the user's next request; the user may be asked to sign in again.

* Every change is written to the Audit Log with the old and new values.

**Where next**

To [ADM-02](#bookmark=id.84apprc917i0) or [ADM-04](#bookmark=id.czm9bezfxzhx).

### **ADM-04  User Detail**

| Type | Page |
| :---- | :---- |
| **Module** | Administrator (ADM) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Click a user on [ADM-02](#bookmark=id.84apprc917i0) or in search results. |

**Purpose**

Shows one account in full with its recent activity and the management actions.

**Block layout**

![ADM-04 User Detail][image44]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Account info** | Name, email, role, department, employee code, phone, specialty, created date, last sign-in, password last changed. |
| **Activity summary** | Counts only, for example: requests sent, work orders done. It does not show the content of any case. |
| **Recent audit events** | Last 20 events of this user with a link to [ADM-08](#bookmark=id.382mbmgltcob) filtered to the user. |
| **Action buttons** | Edit, Reset password, Unlock (only if locked), Deactivate or Reactivate, Force sign-out. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Edit** | Opens [ADM-03](#bookmark=id.l5d1kw4uy1i1) in edit mode. |
| **Reset password, Unlock, Deactivate, Reactivate, Force sign-out** | Opens [DLG-16](#bookmark=id.x23yp780kgp). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **User not found** | [ERR-02](#bookmark=id.gqj1o5u9y4bl). |
| **Loading / load error** | Placeholders; Retry. |

**Rules**

* Case content is never shown to the Administrator.

**Where next**

To [ADM-02](#bookmark=id.84apprc917i0), [ADM-03](#bookmark=id.l5d1kw4uy1i1), [ADM-08](#bookmark=id.382mbmgltcob), [DLG-16](#bookmark=id.x23yp780kgp).

### **ADM-05  Roles and Permissions (reference)**

| Type | Page |
| :---- | :---- |
| **Module** | Administrator (ADM) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Menu → Users → Roles tab, or the link on the role dropdown of [ADM-03](#bookmark=id.l5d1kw4uy1i1). |

**Purpose**

A read-only table that explains what each of the five roles can do. The five roles are fixed and cannot be edited.

**Block layout**

![ADM-05 Roles and Permissions (reference)][image45]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Role cards** | Name, one-line description, and the number of active users in that role. |
| **Permission table** | Rows are capabilities (submit request, approve, assign, execute work, verify, view analytics, manage users, and so on), columns are roles, cells are Yes, View only, Own only, or No. |
| **AI access rule** | For each role, one sentence about what the AI may answer (same wording as the AI scope note in [COM-07](#bookmark=id.su5epyb3mo4x)). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **View only** | There are no buttons to change permissions. A note says permissions are set by the system design. |
| **Click the user count** | Opens [ADM-02](#bookmark=id.84apprc917i0) filtered by that role. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Load fails** | Red banner with Retry. |

**Rules**

* The table is generated from the same permission rules that the system uses, so it never becomes out of date.

**Where next**

To [ADM-02](#bookmark=id.84apprc917i0).

### **ADM-06  Maintenance Categories**

| Type | Page |
| :---- | :---- |
| **Module** | Administrator (ADM) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Menu → Categories. |

**Purpose**

Manages the lists that people choose from: issue types, asset types and common root causes.

**Block layout**

![ADM-06 Maintenance Categories][image46]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Three tabs** | Issue types (used in [EMP-02](#bookmark=id.v4er16ngh3i9)), Asset types (used in [AST-04](#bookmark=id.615v8i8ga3hr)), Root causes (used in [TEC-03](#bookmark=id.dkkwhpnr6ifa)). |
| **List** | Name, short description, "Used in N records", status (Active or Inactive), drag handle for order. |
| **Row menu** | Edit, Deactivate or Reactivate, Delete (only if unused). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Add item / Edit** | Opens [DLG-23](#bookmark=id.nd9lxpb4ubvz). |
| **Deactivate / Reactivate** | Inactive items disappear from the choice lists but stay visible on old records. |
| **Delete** | Opens [DLG-17](#bookmark=id.24kwc21rauut). Only unused items can be deleted. |
| **Reorder** | Drag and drop; saved at once. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty tab** | "No items yet. Add the first one." |
| **Duplicate name** | "This name already exists in this list." |
| **Delete an item in use** | Blocked with "Used in 12 records. Deactivate it instead." |
| **Save fails** | Red toast, list returns to its earlier order. |

**Rules**

* There must be at least one active item in Issue types and Asset types.

* Changes are written to the Audit Log.

**Where next**

To [DLG-17](#bookmark=id.24kwc21rauut), [DLG-23](#bookmark=id.nd9lxpb4ubvz).

### **ADM-07  SLA and Rules Settings**

| Type | Page |
| :---- | :---- |
| **Module** | Administrator (ADM) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Menu → SLA. |

**Purpose**

Sets the target times for handling cases and a few numbers the system uses in its rules, so they can be changed without changing the software.

**Block layout**

![ADM-07 SLA and Rules Settings][image47]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **SLA table** | One row per priority (Urgent, High, Normal, Low). Columns: first response target (hours), resolve target (hours), counting mode (Working hours or Round the clock). Default: Urgent 1 h and 4 h, High 4 h and 24 h, Normal 8 h and 72 h, Low 24 h and 7 days. |
| **Working hours** | Days of the week and start and end time (default Monday to Friday, 08:00 to 17:30), and a list of holidays. |
| **Other rules** | Claim time-out (default 30 minutes), maximum claims per Officer (5), reopen window for Employees (7 days), reminder for Need more info (3 days), session length (8 hours), Administrator contact email shown on [AUTH-06](#bookmark=id.nyi20vb1h2mm). |
| **Save and Reset** | Save is active only when something changed. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save** | Applies to new cases and to open cases from the next calculation. Shows a green toast. |
| **Reset to default** | Asks to confirm, then restores the default numbers. |
| **Edit holidays** | Add or remove dates in a small list. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Invalid value** | Red text: numbers must be whole and above 0; resolve time must be longer than first response time. |
| **Unsaved changes and leaving** | [DLG-19](#bookmark=id.98dbp5b14xo5). |
| **Save fails** | Red banner, old values kept. |

**Rules**

* Changes are written to the Audit Log with old and new values.

**Where next**

To [DLG-19](#bookmark=id.98dbp5b14xo5).

### **ADM-08  Audit Log**

| Type | Page |
| :---- | :---- |
| **Module** | Administrator (ADM) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Menu → Audit Log, or links from [ADM-01](#bookmark=id.ekdh78v68oz1) and [ADM-04](#bookmark=id.czm9bezfxzhx). |

**Purpose**

A record of who did what and when, including sign-ins, changes and every AI question. It can be read and searched but never edited or deleted.

**Block layout**

![ADM-08 Audit Log][image48]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Filters** | Date range (default last 7 days), user, role, action type (Sign-in, Case action, User management, Configuration, AI question, Export, Access denied), result (Success or Failed). |
| **Log table** | Time, User, Role, Action (a short sentence), Object (for example CASE-1042 as plain text), Result, and the device address. |
| **Detail drawer** | Opens on the right when a row is clicked. Shows the exact changes (old value → new value). For AI events: the question, the answer, the role scope used, and the user's rating. |
| **Export CSV** | Downloads the filtered rows (up to 50,000). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Filter and search** | Standard list behaviour. |
| **Open a row** | Shows the detail drawer. |
| **Export CSV** | Creates the file; the export itself is written to the log. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No events** | "No events for these filters." |
| **Too many rows to export** | "More than 50,000 rows. Narrow the date range." |
| **Load fails** | Red banner with Retry. |

**Rules**

* Log entries are kept for 12 months.

* Case numbers in the log are plain text; the Administrator cannot open the case from here.

**Where next**

To [ADM-04](#bookmark=id.czm9bezfxzhx).

## **10.9 Assets and facilities (AST)**

### **AST-01  Facility Tree (Building, Floor, Room)**

| Type | Page |
| :---- | :---- |
| **Module** | Assets and facilities (AST) |
| **Who can open it** | Officer (Read only), Manager (Read only), Admin (Full) |
| **Where it comes from** | Menu → Facilities & Assets (Administrator), or the Assets menu (Officer, Manager). |

**Purpose**

Shows all places as a tree and lets the Administrator create and edit them.

**Block layout**

![AST-01 Facility Tree (Building, Floor, Room)][image49]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Tree** | Expand and collapse. Each node shows a small count of assets and open cases (counts of cases are shown only to Officer and Manager). Inactive places are greyed. |
| **Search box** | Finds a place by name and opens the tree to it. |
| **Place detail** | Name, code, type (Room or Common area), floor area, notes, status, and who created it. |
| **Assets in this place** | A small table of the assets there, each opening [AST-03](#bookmark=id.j6n8nmgqzl9d). |
| **Buttons (Administrator only)** | Add building, Add floor, Add room under the selected node, Edit, Deactivate. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Add / Edit place** | Opens [DLG-15](#bookmark=id.vxxh7jrxg7s3). |
| **Deactivate / Delete place** | Opens [DLG-17](#bookmark=id.24kwc21rauut). |
| **Open an asset** | Opens [AST-03](#bookmark=id.j6n8nmgqzl9d). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No places yet** | Administrator sees "Add the first building". Others see "No places yet. Please contact the Administrator." |
| **Deactivate a place with open cases** | Blocked with the number of open cases. |
| **Load fails** | Red banner with Retry. |

**Rules**

* Officer and Manager can only view the tree. The Employee and Technician do not have this screen; they only choose places in forms.

* A place with assets or history is never deleted, only deactivated.

**Where next**

To [AST-02](#bookmark=id.vfpdk57qamqs), [AST-03](#bookmark=id.j6n8nmgqzl9d), [DLG-15](#bookmark=id.vxxh7jrxg7s3), [DLG-17](#bookmark=id.24kwc21rauut).

### **AST-02  Asset List**

| Type | Page |
| :---- | :---- |
| **Module** | Assets and facilities (AST) |
| **Who can open it** | Officer (Scoped), Manager (Read only), Admin (Full) |
| **Where it comes from** | Menu → Assets. |

**Purpose**

Lists all assets with filters. The Administrator manages them, the Officer changes their status, the Manager reads.

**Block layout**

![AST-02 Asset List][image50]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Search** | By name, asset code, serial number. |
| **Filters** | Building, floor, room, asset type, status (Active, Under repair, Out of service, Retired), has open case, warranty ending soon. |
| **Table columns** | Code, Name, Type, Place, Status pill, Open cases, Last maintenance, Next preventive due. |
| **Top buttons (Administrator)** | Add asset, Import from file, Print QR labels for the ticked rows. |
| **Row menu** | Administrator: View, Edit, Retire. Officer: View, Change status, Create case for this asset, Print QR. Manager: View. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Add asset** | Opens [AST-04](#bookmark=id.615v8i8ga3hr). |
| **Import** | Opens [AST-05](#bookmark=id.xz1mbdpv5xr3). |
| **Print QR** | Opens [AST-06](#bookmark=id.9e47xaty55ny) with the ticked assets. |
| **Change status** | Opens [DLG-18](#bookmark=id.wwutpqpzg3ck). |
| **Retire** | Opens [DLG-17](#bookmark=id.24kwc21rauut). |
| **Create case for this asset** | Opens [OFF-05](#bookmark=id.gtd6k0343759) with the asset filled. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty** | Administrator: "No assets yet. Add one or import a file." Others: "No assets found." |
| **Empty after filtering** | "No assets match." with Clear filters. |
| **Load fails** | Red banner with Retry. |

**Rules**

* Retired assets are hidden by default; a switch "Show retired" reveals them.

**Where next**

To [AST-03](#bookmark=id.j6n8nmgqzl9d), [AST-04](#bookmark=id.615v8i8ga3hr), [AST-05](#bookmark=id.xz1mbdpv5xr3), [AST-06](#bookmark=id.9e47xaty55ny), [DLG-17](#bookmark=id.24kwc21rauut), [DLG-18](#bookmark=id.wwutpqpzg3ck), [OFF-05](#bookmark=id.gtd6k0343759).

### **AST-03  Asset Detail**

| Type | Page |
| :---- | :---- |
| **Module** | Assets and facilities (AST) |
| **Who can open it** | Officer (Scoped), Manager (Read only), Admin (Scoped) |
| **Where it comes from** | From [AST-01](#bookmark=id.r397yhsl2mmw), [AST-02](#bookmark=id.vfpdk57qamqs), search, Case Detail, preventive pages or after scanning a QR code as staff. |

**Purpose**

Shows everything about one asset: information, its maintenance history, its preventive plans and how often it fails.

**Block layout**

![AST-03 Asset Detail][image51]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Header** | Asset code, name, status pill, a small QR picture, and a warning tag "Frequent failures" when there were 3 or more repairs in the last 30 days. |
| **Overview tab** | Type, manufacturer, model, serial number, purchase date, warranty end date (red if ended), place with link to [AST-01](#bookmark=id.r397yhsl2mmw), notes, photo. |
| **History tab** | All cases on this asset: code, title, dates, status, Technician, root cause, labor hours. Each row opens [COM-08](#bookmark=id.83jeomyy0mjj). |
| **Preventive tab** | Plans that include this asset, next due date, and the last 5 inspection records. |
| **Statistics tab** | Number of repairs per month (last 12 months), total labor hours, average repair time, top root causes. |
| **Action buttons** | Administrator: Edit, Retire. Officer: Change status, Create case, Print QR. Manager: none. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Edit** | Opens [AST-04](#bookmark=id.615v8i8ga3hr) (Administrator). |
| **Change status** | Opens [DLG-18](#bookmark=id.wwutpqpzg3ck) (Officer). |
| **Retire** | Opens [DLG-17](#bookmark=id.24kwc21rauut) (Administrator). |
| **Print QR** | Opens [AST-06](#bookmark=id.9e47xaty55ny). |
| **Create case** | Opens [OFF-05](#bookmark=id.gtd6k0343759) with the asset filled (Officer). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Not found or retired** | [ERR-02](#bookmark=id.gqj1o5u9y4bl), or a grey banner "This asset is retired" with read-only data. |
| **No history yet** | "No maintenance yet." |
| **Administrator view** | The History and Statistics tabs are hidden because they contain case content; the Administrator sees only Overview and Preventive plan names. |
| **Load fails** | Red banner with Retry. |

**Rules**

* Assets cannot be deleted when they have history; they are retired.

**Where next**

To [AST-01](#bookmark=id.r397yhsl2mmw), [AST-04](#bookmark=id.615v8i8ga3hr), [AST-06](#bookmark=id.9e47xaty55ny), [COM-08](#bookmark=id.83jeomyy0mjj), [PM-03](#bookmark=id.rfx2okd8binq), [PM-06](#bookmark=id.dk34aamyou4v), [DLG-17](#bookmark=id.24kwc21rauut), [DLG-18](#bookmark=id.wwutpqpzg3ck), [OFF-05](#bookmark=id.gtd6k0343759).

### **AST-04  Create / Edit Asset**

| Type | Page |
| :---- | :---- |
| **Module** | Assets and facilities (AST) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Add asset on [AST-02](#bookmark=id.vfpdk57qamqs), or Edit on [AST-02](#bookmark=id.vfpdk57qamqs) / [AST-03](#bookmark=id.j6n8nmgqzl9d). |

**Purpose**

Adds a new asset or edits an existing one.

**Block layout**

![AST-04 Create / Edit Asset][image52]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Asset code** | Required, unique, 3 to 20 characters, letters, numbers and dash. The system suggests the next code (for example AC-0012). |
| **Name** | Required, 2 to 100 characters. |
| **Type** | Required, from Asset types ([ADM-06](#bookmark=id.a8n26oa6i98t)). |
| **Place** | Required: Building, Floor and Room in linked dropdowns. |
| **Manufacturer, Model, Serial number** | Optional text. Serial number must be unique if given. |
| **Purchase date, Warranty end date** | Optional dates. Warranty end must not be before the purchase date. |
| **Status** | Active by default. |
| **Photo and Notes** | One photo up to 5 MB and notes up to 500 characters, both optional. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save** | Creates or updates the asset and opens [AST-03](#bookmark=id.j6n8nmgqzl9d). In create mode, a link "Print QR label" is offered. |
| **Save and add another** | Create mode only. Keeps type and place, clears the rest. |
| **Cancel** | Back; [DLG-19](#bookmark=id.98dbp5b14xo5) if changed. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Code or serial already used** | "This code is already used by (asset name)." |
| **Room has been deactivated** | Room cannot be chosen; a note says why. |
| **Moving an asset with open cases** | A yellow warning lists the open cases but allows the move; the cases keep their old place text in the Timeline. |
| **Save fails** | Red banner, input kept. |

**Rules**

* Changes are written to the Audit Log.

**Where next**

To [AST-03](#bookmark=id.j6n8nmgqzl9d), [AST-06](#bookmark=id.9e47xaty55ny).

### **AST-05  Import Assets from File**

| Type | Page |
| :---- | :---- |
| **Module** | Assets and facilities (AST) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Import button on [AST-02](#bookmark=id.vfpdk57qamqs) or [ADM-01](#bookmark=id.ekdh78v68oz1). |

**Purpose**

Adds many assets at once from an Excel or CSV file, in four clear steps.

**Block layout**

![AST-05 Import Assets from File][image53]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Step 1: Upload** | A link to download the template file, a drop zone, accepted types .xlsx and .csv, up to 5 MB and 1000 rows. |
| **Step 2: Check** | A preview of all rows. Rows with problems are marked red with the reason (for example "Room not found", "Duplicate code", "Type missing", "Date is not valid"). A summary shows how many rows are good and how many have errors. |
| **Step 3: Confirm** | Choose: import only the good rows, or stop and fix the file. A sentence tells what will happen. |
| **Step 4: Result** | "120 assets imported, 5 skipped." A link to download the list of skipped rows with reasons, and a button to open [AST-02](#bookmark=id.vfpdk57qamqs). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Upload a file** | The file is read and checked; nothing is saved yet. |
| **Next / Back** | Moves between steps. |
| **Import** | Saves the good rows. The whole import is one action; if it fails, nothing is saved. |
| **Download error list / template** | Downloads a file. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Wrong file type or too big** | "Please upload an .xlsx or .csv file under 5 MB." |
| **Missing columns** | "Column Name is missing. Please use the template." |
| **More than 1000 rows** | "Too many rows. Please split the file." |
| **All rows have errors** | The Import button is disabled and the error list can be downloaded. |
| **Leaving in the middle** | [DLG-19](#bookmark=id.98dbp5b14xo5): the upload will be lost. |
| **Import fails** | Red banner: "Nothing was saved. Please try again." |

**Rules**

* Existing assets are never overwritten by an import; a duplicate code is an error.

* The import is written to the Audit Log with the file name and the counts.

**Where next**

To [AST-02](#bookmark=id.vfpdk57qamqs).

### **AST-06  QR Labels (print)**

| Type | Page |
| :---- | :---- |
| **Module** | Assets and facilities (AST) |
| **Who can open it** | Officer (Full), Admin (Full) |
| **Where it comes from** | Print QR on [AST-02](#bookmark=id.vfpdk57qamqs) or [AST-03](#bookmark=id.j6n8nmgqzl9d), or after creating an asset. |

**Purpose**

Creates printable QR labels that people stick on assets. Scanning a label opens the report form for that asset.

**Block layout**

![AST-06 QR Labels (print)][image54]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Selected assets** | Up to 100 assets per print. Rows can be removed. |
| **Label size** | Small 30 x 30 mm, Medium 50 x 50 mm, or A4 sheet with 3 x 8 labels. |
| **Preview** | Shows the first label and a page view. Each label has the QR picture, the asset code and the asset name. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Download PDF** | Makes a PDF file. |
| **Print** | Opens the browser print window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No assets selected** | Buttons disabled: "Choose assets first." |
| **Retired asset in the list** | Removed automatically with a note. |
| **PDF creation fails** | Red banner with Retry. |

**Rules**

* The QR holds a web address with the asset code, never personal data.

**Where next**

Back to [AST-02](#bookmark=id.vfpdk57qamqs) or [AST-03](#bookmark=id.j6n8nmgqzl9d).

### **AST-07  QR Landing (scan to report)**

| Type | Page |
| :---- | :---- |
| **Module** | Assets and facilities (AST) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Scanning a QR label with a phone or opening its address in a browser. |

**Purpose**

Decides where to send a person who scans a QR label on an asset, and explains problems with the label.

**Block layout**

![AST-07 QR Landing (scan to report)][image55]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Redirect rules** | Not signed in: [AUTH-01](#bookmark=id.8yzrz7fdv0n7), then back here. Employee: [EMP-02](#bookmark=id.v4er16ngh3i9) with building, room and asset filled in. Officer, Manager, Administrator: [AST-03](#bookmark=id.j6n8nmgqzl9d). Technician: the open work order for this asset ([TEC-03](#bookmark=id.dkkwhpnr6ifa)), or a message that there is none. |
| **Message box** | Appears only when something is wrong (see states). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Continue** | Follows the redirect rule. |
| **Sign in** | Goes to [AUTH-01](#bookmark=id.8yzrz7fdv0n7). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Unknown code** | "This label is not recognised. Please choose the place and asset by hand." with a button to [EMP-02](#bookmark=id.v4er16ngh3i9) (Employee). |
| **Asset retired** | "This asset is no longer in use." Employee can still report a problem for the room. |
| **Technician without work order** | "You have no work order for this asset." with a link to [TEC-02](#bookmark=id.8l487t8fl9xe). |
| **No permission** | [ERR-01](#bookmark=id.lmhdh2gy047x). |

**Rules**

* The address contains only the asset code. Sign-in is always needed, so no one can send requests without an account.

**Where next**

To [AUTH-01](#bookmark=id.8yzrz7fdv0n7), [EMP-02](#bookmark=id.v4er16ngh3i9), [AST-03](#bookmark=id.j6n8nmgqzl9d), [TEC-03](#bookmark=id.dkkwhpnr6ifa), [TEC-02](#bookmark=id.8l487t8fl9xe).

## **10.10 Preventive maintenance (PM)**

### **PM-01  Preventive Plan List**

| Type | Page |
| :---- | :---- |
| **Module** | Preventive maintenance (PM) |
| **Who can open it** | Officer (Full), Manager (Read only) |
| **Where it comes from** | Menu → Preventive Maintenance, or links from Home. |

**Purpose**

Lists the plans that create maintenance work on a schedule, before anything breaks.

**Block layout**

![PM-01 Preventive Plan List][image56]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Tabs** | Active, Paused, Ended, each with a count. |
| **Table columns** | Plan name, Assets (first one and "+3 more"), Frequency, Next due (red if overdue, orange if within 7 days), Last done, Default technician, On-time rate, Status. |
| **Buttons** | New plan and Calendar view (Officer). Manager sees only the Calendar button. |
| **Row menu (Officer)** | Open, Edit, Pause or Resume, Generate work order now, Duplicate. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **New plan** | Opens [PM-02](#bookmark=id.u6drodgl2m81). |
| **Open** | Opens [PM-03](#bookmark=id.rfx2okd8binq). |
| **Calendar** | Opens [PM-04](#bookmark=id.xzhz6y8fmacz). |
| **Generate work order now** | Opens [DLG-21](#bookmark=id.p5kc67ltjc22). |
| **Pause / Resume** | Switches the plan; a confirmation toast appears. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No plans yet** | Officer: "Create your first plan". Manager: "No plans yet." |
| **Empty after filtering** | "No plans match." |
| **Load fails** | Red banner with Retry. |

**Rules**

* Only the Officer can create or change plans. The Manager can view. The Administrator does not use this module.

**Where next**

To [PM-02](#bookmark=id.u6drodgl2m81), [PM-03](#bookmark=id.rfx2okd8binq), [PM-04](#bookmark=id.xzhz6y8fmacz), [DLG-21](#bookmark=id.p5kc67ltjc22).

### **PM-02  Create / Edit Preventive Plan**

| Type | Page |
| :---- | :---- |
| **Module** | Preventive maintenance (PM) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | New plan or Edit on [PM-01](#bookmark=id.hubvrybhov1x) / [PM-03](#bookmark=id.rfx2okd8binq). |

**Purpose**

Defines what to check, how often, on which assets, and who does it.

**Block layout**

![PM-02 Create / Edit Preventive Plan][image57]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Plan name** | Required, 3 to 100 characters. Example: "Monthly air conditioner check". |
| **Assets** | Required, at least one. Search and tick, or "Add all assets of a type in a building". One work order is created per asset each cycle. |
| **Frequency** | Required. Daily, Weekly, Monthly, Every 3 months, Every 6 months, Yearly, or "Every N days / weeks / months". |
| **Start date and End date** | Start date required and not in the past. End date optional (empty means no end). |
| **Lead time** | How many days before the due date the work order is created. Default 3 days. |
| **Priority** | Low, Normal (default), High, Urgent. Used for the work orders that are created. |
| **Default technician** | Optional. If chosen, new work orders start as Assigned to this person. If empty, they wait as Approved in the Officer's To assign tab. |
| **Checklist** | Required, 1 to 30 items. Each item: the check (3 to 200 characters) and a tick "Photo needed". Items can be reordered by dragging. |
| **Instructions and Estimated hours** | Optional text up to 1000 characters and a number of hours. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save plan** | Creates or updates the plan (status Active) and opens [PM-03](#bookmark=id.rfx2okd8binq). Changes apply to work orders created from now on, not to ones already created. |
| **Preview schedule** | Shows the next 5 due dates based on what was typed. |
| **Cancel** | Back; [DLG-19](#bookmark=id.98dbp5b14xo5) if changed. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Missing required fields** | Red text under each field. |
| **No checklist item** | "Add at least one check." |
| **Asset is retired** | Not offered in the list. |
| **Same asset and same frequency already planned** | A yellow note lists the existing plan; saving is still allowed. |
| **Save fails** | Red banner, input kept. |

**Rules**

* Work orders from a plan skip the review step, because the plan itself is the approval.

* If an asset already has an open work order of the same plan, no second one is created.

**Where next**

To [PM-03](#bookmark=id.rfx2okd8binq).

### **PM-03  Preventive Plan Detail**

| Type | Page |
| :---- | :---- |
| **Module** | Preventive maintenance (PM) |
| **Who can open it** | Officer (Full), Manager (Read only) |
| **Where it comes from** | From [PM-01](#bookmark=id.hubvrybhov1x), [PM-04](#bookmark=id.xzhz6y8fmacz), [AST-03](#bookmark=id.j6n8nmgqzl9d), [MGR-01](#bookmark=id.fxg093g2jt85) or a notification. |

**Purpose**

Shows one plan, what it created so far, and how well it is followed.

**Block layout**

![PM-03 Preventive Plan Detail][image58]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Plan info** | All fields from [PM-02](#bookmark=id.u6drodgl2m81) in read view, including the checklist. |
| **Next due dates** | The next 5 dates. |
| **Generated work orders** | History with status. Each row opens [COM-08](#bookmark=id.83jeomyy0mjj) or the inspection record ([PM-06](#bookmark=id.dk34aamyou4v)). |
| **Compliance tiles** | On-time rate (finished before the due date) and Passed rate (inspections with no failed item) for the last 12 months. |
| **Action buttons (Officer)** | Edit, Pause or Resume, Generate now, End plan. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Edit** | Opens [PM-02](#bookmark=id.u6drodgl2m81). |
| **Pause / Resume** | No new work orders are created while paused; existing ones continue. |
| **Generate now** | Opens [DLG-21](#bookmark=id.p5kc67ltjc22). |
| **End plan** | Asks to confirm; the plan becomes Ended and cannot be reactivated (duplicate it instead). |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Plan not found** | [ERR-02](#bookmark=id.gqj1o5u9y4bl). |
| **No work orders yet** | "Nothing generated yet. The first one will be created on (date)." |
| **Paused or Ended plan** | A grey banner at the top explains it and the buttons change accordingly. |

**Rules**

* Manager sees the page without action buttons.

**Where next**

To [PM-02](#bookmark=id.u6drodgl2m81), [PM-06](#bookmark=id.dk34aamyou4v), [COM-08](#bookmark=id.83jeomyy0mjj), [DLG-21](#bookmark=id.p5kc67ltjc22).

### **PM-04  Preventive Calendar**

| Type | Page |
| :---- | :---- |
| **Module** | Preventive maintenance (PM) |
| **Who can open it** | Officer (Full), Manager (Read only) |
| **Where it comes from** | Calendar button on [PM-01](#bookmark=id.hubvrybhov1x), or Home. |

**Purpose**

Shows upcoming and past preventive work on a calendar so the Officer can balance the workload.

**Block layout**

![PM-04 Preventive Calendar][image59]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Calendar** | Month or week view. Each event is a small bar with the plan name and asset, coloured by state: Upcoming (grey), Due soon (orange), Overdue (red), Done (green), each with an icon, not colour alone. |
| **Day panel** | Clicking a day lists all events of that day with the Technician and status. |
| **Filters** | Building, asset type, technician, status. |
| **Today marker** | Today is outlined. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Click an event** | Opens [PM-03](#bookmark=id.rfx2okd8binq) (future) or the work order in [COM-08](#bookmark=id.83jeomyy0mjj) (already generated). |
| **Change view / date** | Month, Week, previous, next, Today. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty period** | "No preventive work in this period." |
| **Busy day** | The cell shows the first 3 events and "+5 more". |
| **Load fails** | Red banner with Retry. |

**Rules**

* The calendar is read only; dates are changed by editing the plan.

**Where next**

To [PM-01](#bookmark=id.hubvrybhov1x), [PM-03](#bookmark=id.rfx2okd8binq), [COM-08](#bookmark=id.83jeomyy0mjj).

### **PM-05  Inspection Records**

| Type | Page |
| :---- | :---- |
| **Module** | Preventive maintenance (PM) |
| **Who can open it** | Officer (Full), Manager (Read only) |
| **Where it comes from** | Menu → Preventive Maintenance → Inspection Records. |

**Purpose**

The history of all inspections that Technicians completed.

**Block layout**

![PM-05 Inspection Records][image60]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Filters** | Date range (default 30 days), asset, building, plan, technician, result (Passed or Needs attention). |
| **Table columns** | Record code, Date, Asset, Plan, Technician, Result pill, Failed items count, Linked repair case (if any). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Open a record** | Opens [PM-06](#bookmark=id.dk34aamyou4v). |
| **Filter and sort** | Standard list behaviour. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Empty** | "No inspection records yet." |
| **Load fails** | Red banner with Retry. |

**Rules**

* Records cannot be edited or deleted after they are created.

**Where next**

To [PM-06](#bookmark=id.dk34aamyou4v).

### **PM-06  Inspection Record Detail**

| Type | Page |
| :---- | :---- |
| **Module** | Preventive maintenance (PM) |
| **Who can open it** | Officer (Full), Manager (Read only) |
| **Where it comes from** | From [PM-05](#bookmark=id.j0baje6voyco), [PM-03](#bookmark=id.rfx2okd8binq), [AST-03](#bookmark=id.j6n8nmgqzl9d). |

**Purpose**

Shows the full result of one inspection.

**Block layout**

![PM-06 Inspection Record Detail][image61]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Header** | Record code, asset with place, plan, inspection date and time, Technician, overall result. |
| **Checklist results** | Every item with its answer. Failed items are highlighted with the note and photos, which open the viewer ([DLG-20](#bookmark=id.p1oyfarais6j)). |
| **Links** | To the work order ([COM-08](#bookmark=id.83jeomyy0mjj)), to the plan ([PM-03](#bookmark=id.rfx2okd8binq)), and to the repair case created from failed items. |
| **Print / PDF** | One-page summary. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Open links** | Goes to [COM-08](#bookmark=id.83jeomyy0mjj), [PM-03](#bookmark=id.rfx2okd8binq). |
| **Print / PDF** | Creates the file. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Not found** | [ERR-02](#bookmark=id.gqj1o5u9y4bl). |
| **Photo cannot be shown** | Grey tile "Image unavailable". |

**Rules**

* Read only for everyone.

**Where next**

To [PM-05](#bookmark=id.j0baje6voyco), [PM-03](#bookmark=id.rfx2okd8binq), [COM-08](#bookmark=id.83jeomyy0mjj).

## **10.11 Analytics (ANA)**

### **ANA-01  KPI Overview Dashboard**

| Type | Page |
| :---- | :---- |
| **Module** | Analytics (ANA) |
| **Who can open it** | Officer (Scoped), Manager (Full) |
| **Where it comes from** | Menu → Analytics. |

**Purpose**

Shows how maintenance is performing so managers can make decisions with numbers. The Officer sees a shorter version.

**Block layout**

![ANA-01 KPI Overview Dashboard][image62]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Filters** | Period: last 7, 30, 90 days or custom range. Building: all or one. Applies to every number on the page. |
| **KPI tiles** | Total cases, Average first response time, Average resolution time, SLA met (%), Reopen rate (%), Average rating (Manager only). Each shows the change from the previous period, with an arrow and a word. |
| **Charts** | Created versus Closed per week (line); Cases by status (bar); Cases by issue type (bar); Resolution time by priority (bar with the SLA target line); Cases by building (Manager only). |
| **Chart hover and data table** | Hover shows exact numbers. A small "Show as table" link shows the same data as a table for accessibility. |
| **Officer view** | Officers see the same filters and all charts except Rating and Cases by building comparison; they cannot open technician rankings. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Change filters** | All widgets refresh together. |
| **Click a chart bar** | Opens [OFF-03](#bookmark=id.n0o9xiciqqd7) with the matching filter. |
| **Export** | Opens [ANA-04](#bookmark=id.vuzx748pj0j4) with this period prefilled. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No data in the period** | Charts show "No cases in this period". |
| **Too little data** | Averages show "Not enough data" when fewer than 5 cases. |
| **Loading / load error** | Each chart loads by itself with its own Retry. |

**Rules**

* Definitions: first response \= from Submitted to the first Officer decision; resolution \= from Approved to Completed; SLA met \= share of cases finished inside the target set in [ADM-07](#bookmark=id.6ynr8feqi2ao).

**Where next**

To [OFF-03](#bookmark=id.n0o9xiciqqd7), [ANA-02](#bookmark=id.h2nhwbchtidg), [ANA-03](#bookmark=id.cwmp2fxt8ju0), [ANA-04](#bookmark=id.vuzx748pj0j4).

### **ANA-02  Technician Performance**

| Type | Page |
| :---- | :---- |
| **Module** | Analytics (ANA) |
| **Who can open it** | Manager (Full) |
| **Where it comes from** | From [ANA-01](#bookmark=id.qschmq5ovp16) or menu → Analytics → Technicians. |

**Purpose**

Helps the Manager see how each Technician is doing and how the workload is shared.

**Block layout**

![ANA-02 Technician Performance][image63]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Table columns** | Technician, Completed, Average completion time, On-time rate, Reopen rate, Average rating, Total labor hours, Open work orders now. |
| **Workload chart** | A bar per Technician showing open work orders now. |
| **Detail panel** | Clicking a Technician shows a small weekly trend and their latest 10 cases (read only). |
| **Sorting and fairness note** | Any column can be sorted. A note reminds that numbers depend on the type of work, and that fewer than 5 cases give unreliable results (shown greyed). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Sort, filter** | Standard behaviour. |
| **Open a case in the panel** | Opens [COM-08](#bookmark=id.83jeomyy0mjj) read only. |
| **Export** | Opens [ANA-04](#bookmark=id.vuzx748pj0j4) with the Technician report selected. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No technicians or no data** | "No data for this period." |
| **Load fails** | Red banner with Retry. |

**Rules**

* Only the Manager can open this page. Technicians never see other Technicians' numbers.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj), [ANA-04](#bookmark=id.vuzx748pj0j4).

### **ANA-03  Failure Trends and Problem Assets**

| Type | Page |
| :---- | :---- |
| **Module** | Analytics (ANA) |
| **Who can open it** | Officer (Scoped), Manager (Full) |
| **Where it comes from** | From [ANA-01](#bookmark=id.qschmq5ovp16), [MGR-01](#bookmark=id.fxg093g2jt85) or menu → Analytics → Failures. |

**Purpose**

Shows which assets and which kinds of problems happen most, so preventive work can be planned better.

**Block layout**

![ANA-03 Failure Trends and Problem Assets][image64]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Top assets** | Ten assets with the most cases: code, name, place, number of cases, total labor hours, average time between failures. Each opens [AST-03](#bookmark=id.j6n8nmgqzl9d). |
| **Trend chart** | Lines per month for each issue type over the last 12 months. |
| **Repeat failures** | Assets with 3 or more cases in 30 days, with the top root cause. A hint says "Consider a preventive plan" with a link to [PM-02](#bookmark=id.u6drodgl2m81) (Officer). |
| **By asset type** | Bar chart of failures per type, useful for deciding on repair or replacement talks. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Open an asset** | Opens [AST-03](#bookmark=id.j6n8nmgqzl9d). |
| **Create plan (Officer)** | Opens [PM-02](#bookmark=id.u6drodgl2m81) with the asset filled. |
| **Export** | Opens [ANA-04](#bookmark=id.vuzx748pj0j4) with the Asset failure report selected. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No repeat failures** | "No repeated failures in this period." |
| **Too little data** | "Not enough data yet." |
| **Load fails** | Red banner with Retry. |

**Rules**

* The page only supports decisions; it does not decide anything. Actions such as repair or replace are made outside the system.

**Where next**

To [AST-03](#bookmark=id.j6n8nmgqzl9d), [PM-02](#bookmark=id.u6drodgl2m81), [ANA-04](#bookmark=id.vuzx748pj0j4).

### **ANA-04  Reports and Export**

| Type | Page |
| :---- | :---- |
| **Module** | Analytics (ANA) |
| **Who can open it** | Officer (Scoped), Manager (Full) |
| **Where it comes from** | Menu → Analytics → Reports, or the Export buttons on other analytics pages. |

**Purpose**

Creates reports as PDF or Excel files for meetings and records.

**Block layout**

![ANA-04 Reports and Export][image65]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Report types** | Case summary; Technician performance (Manager only); Asset failure report; Preventive compliance; Inspection records. The Officer can create Case summary, Asset failure and Preventive compliance. |
| **Options** | Period (required), Building (all or one), Format (PDF or Excel). |
| **Preview** | Shows the first 10 rows and the total count before creating the file. |
| **Recent exports** | The last 20 files with date, type and a Download link, kept for 7 days. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Generate and download** | Shows a progress bar, then the file downloads and appears in the recent list. |
| **Download an old export** | Downloads the same file while it is still kept. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No data in the period** | "No data to export for these options." The button is disabled. |
| **File too large** | "Too many rows. Please choose a shorter period." (limit 20,000 rows) |
| **Creating the file fails** | Red banner with Retry. |
| **Export expired** | The row is greyed with "Expired". |

**Rules**

* Every export is written to the Audit Log.

* Reports contain only what the role can already see on screen.

**Where next**

Stays on the page; files are downloaded.

## **10.12 Dialogs (DLG)**

### **DLG-01  Approve Case**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Approve button on [COM-08](#bookmark=id.83jeomyy0mjj) or [OFF-02](#bookmark=id.x3zeg4xiveou) (status Pending, claimed by this Officer). |

**Purpose**

The Officer accepts the request as a real maintenance job and sets its final priority.

**Block layout**

![DLG-01 Approve Case][image66]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Final priority** | Required. Low, Normal, High, Urgent. Starts with the priority the Employee suggested. A short help line shows the SLA time for each. |
| **Note** | Optional, up to 300 characters. Shown to the Employee in the Timeline. |
| **Assign next** | Tick box, on by default. After approving, [DLG-04](#bookmark=id.6bnps7bwaz0o) opens at once. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Approve** | Status becomes Approved. The Employee is notified. The SLA resolve timer starts. |
| **Cancel** | Closes the window; nothing changes. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Case changed by someone else** | "This case was updated. Please refresh." The dialog closes. |
| **Claim lost (time-out)** | "Your claim expired. Claim the case again." ([DLG-22](#bookmark=id.60m6ytailped)) |
| **Save fails** | Red text inside the dialog; the dialog stays open. |

**Rules**

* Only the Officer who claimed the case can approve it.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj) or [DLG-04](#bookmark=id.6bnps7bwaz0o).

### **DLG-02  Reject Case**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Reject button on [COM-08](#bookmark=id.83jeomyy0mjj) or [OFF-02](#bookmark=id.x3zeg4xiveou) (status Pending). |

**Purpose**

The Officer refuses the request and explains why. The case ends as Rejected.

**Block layout**

![DLG-02 Reject Case][image67]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Reason type** | Required. Duplicate of another case, Not a facility problem, Not enough information, Already solved, Other. |
| **Explanation** | Required, 10 to 500 characters. Written in kind, clear words because the Employee will read it. |
| **Duplicate link** | When the type is Duplicate, a search box lets the Officer pick the other case, which is then shown to the Employee. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Reject** | Status becomes Rejected (final). The Employee is notified with the reason. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Explanation too short** | Red text "Please write at least 10 characters." |
| **Case changed by someone else** | Same message as [DLG-01](#bookmark=id.nqyaa9ctr6re). |

**Rules**

* A rejected case cannot be reopened. The Employee must send a new request.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-03  Request More Information**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Need more info button on [COM-08](#bookmark=id.83jeomyy0mjj) or [OFF-02](#bookmark=id.x3zeg4xiveou) (status Pending). |

**Purpose**

The Officer asks the Employee a question before deciding.

**Block layout**

![DLG-03 Request More Information][image68]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Quick tick list** | Optional. Ticking an item adds a ready sentence to the question box. |
| **Question** | Required, 10 to 500 characters. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Send question** | Status becomes Need more info. The Employee is notified and sees the question on [EMP-04](#bookmark=id.1wkdzvdv2xi8). The claim is released so other Officers can help. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Question too short** | Red text under the box. |
| **Case changed by someone else** | Same message as [DLG-01](#bookmark=id.nqyaa9ctr6re). |

**Rules**

* If the Employee does not answer within 3 days, a reminder is sent (the number can be changed in [ADM-07](#bookmark=id.6ynr8feqi2ao)).

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-04  Assign Technician**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Assign button on [COM-08](#bookmark=id.83jeomyy0mjj) or [OFF-02](#bookmark=id.x3zeg4xiveou) (status Approved or Reopened), or automatically after [DLG-01](#bookmark=id.nqyaa9ctr6re). |

**Purpose**

The Officer chooses who will do the work and by when.

**Block layout**

![DLG-04 Assign Technician][image69]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Technician list** | Only active Technicians. Each line: name, specialties (with a green tick "Matches issue type"), number of open work orders, and the nearest due date. Sorted with matching specialty first, then least busy. |
| **Due date** | Required. Filled from the SLA target of the priority; the Officer can change it but not to a past date. |
| **Note** | Optional, up to 500 characters. Shown to the Technician. |
| **Workload warning** | A yellow line when the chosen Technician already has 8 or more open work orders. Assigning is still allowed. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Assign** | Status becomes Assigned. The Technician is notified. The case leaves the To assign tab. |
| **Cancel** | Closes the window; the case stays Approved. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No technician chosen** | The Assign button is disabled. |
| **No active technicians exist** | "No technician available. Ask the Administrator to add one." |
| **Technician was deactivated meanwhile** | "This technician is no longer available." The list refreshes. |

**Rules**

* One case has one Technician at a time.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-05  Reassign Technician**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Reassign on [COM-08](#bookmark=id.83jeomyy0mjj) or in the row menu of [OFF-03](#bookmark=id.n0o9xiciqqd7) (status Assigned, Accepted, In Progress, On Hold or Reopened). |

**Purpose**

Moves a case that was already given to another Technician, for example when someone is ill or overloaded.

**Block layout**

![DLG-05 Reassign Technician][image70]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Current technician** | Name and the work status so far. |
| **New technician list, due date** | Same list and due date rules as [DLG-04](#bookmark=id.6bnps7bwaz0o). The current Technician cannot be chosen. |
| **Reason** | Required, 10 to 300 characters. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Reassign** | Status returns to Assigned for the new Technician, who must accept. The old Technician is told the work was taken back. Everything the old Technician typed (notes, photos) stays visible to the new one. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Technician is editing right now** | Allowed. The Technician sees the red bar described in [TEC-03](#bookmark=id.dkkwhpnr6ifa). |
| **Reason too short** | Red text under the box. |

**Rules**

* Reassigning does not reset the SLA timers.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-06  Verify and Close**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Verify and close on [COM-08](#bookmark=id.83jeomyy0mjj) or [OFF-04](#bookmark=id.lpy4qqom276w) (status Completed). |

**Purpose**

The Officer confirms that the work was done well and closes the case.

**Block layout**

![DLG-06 Verify and Close][image71]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Summary** | Root cause, labor hours, parts, and the number of evidence photos, for a last look. |
| **Check tick** | Required. Helps avoid closing by habit. |
| **Note** | Optional, up to 300 characters. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Verify and close** | Status becomes Closed (final). The Employee is notified and asked to rate. The asset status returns to Active if it was Under repair because of this case (the Officer is asked to confirm). |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Case changed by someone else** | Same message as [DLG-01](#bookmark=id.nqyaa9ctr6re). |
| **Save fails** | Red text in the dialog. |

**Rules**

* Only an Officer can close a case.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj) or [OFF-04](#bookmark=id.lpy4qqom276w).

### **DLG-07  Reopen Case**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Reopen on [COM-08](#bookmark=id.83jeomyy0mjj) or [OFF-04](#bookmark=id.lpy4qqom276w) (status Completed), or on a Closed case that has a reopen request from the Employee. |

**Purpose**

Sends a case back to the Technician because the fix was not good enough or the Employee says it is not fixed.

**Block layout**

![DLG-07 Reopen Case][image72]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Employee's message** | Shown when the reopen was requested from [DLG-13](#bookmark=id.3h2jh6omhwpq). |
| **Reason** | Required, 10 to 500 characters. The Technician reads it on [TEC-03](#bookmark=id.dkkwhpnr6ifa). |
| **Technician and due date** | The same Technician by default; can be changed. A new due date is required. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Reopen** | Status becomes Reopened (tag "Reopened", counter \+1). The Technician and the Employee are notified. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Reason too short** | Red text. |
| **Case reopened 3 times already** | A yellow note "This case was already reopened 3 times. Consider a different Technician." The action is still allowed. |

**Rules**

* The reopen counter feeds the reopen rate on the dashboards.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-08  Decline Reopen Request**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Decline on [COM-08](#bookmark=id.83jeomyy0mjj) or in the Reopen requests tab of [OFF-02](#bookmark=id.x3zeg4xiveou). |

**Purpose**

The Officer says no to an Employee's "Not fixed" report and explains.

**Block layout**

![DLG-08 Decline Reopen Request][image73]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Employee's message** | Read only. |
| **Explanation** | Required, 10 to 500 characters, suggested wording: "Please send a new request for the new problem." |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Decline** | The case stays Closed, the request tag disappears, the Employee is notified with the explanation and can no longer ask again for this case. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Explanation too short** | Red text. |

**Rules**

* The Employee can still send a brand new request.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-09  Reject Assignment**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Technician (Full) |
| **Where it comes from** | Reject on [TEC-01](#bookmark=id.3lbqglcq4he0), [TEC-02](#bookmark=id.8l487t8fl9xe) or [TEC-03](#bookmark=id.dkkwhpnr6ifa) (status Assigned or Reopened). |

**Purpose**

The Technician cannot take a work order and tells the Officer why.

**Block layout**

![DLG-09 Reject Assignment][image74]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Reason type** | Required. Not my skill, Not available at that time, Wrong place or asset, Too many jobs, Other. |
| **Explanation** | Required, 10 to 300 characters. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Reject** | Status returns to Approved with the tag "Rejected by technician". The Officer is notified and must assign again. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Work order already accepted** | The button is not shown. To stop, the Technician uses Put on hold or asks the Officer in a comment. |
| **Reason too short** | Red text. |

**Rules**

* Rejections are shown to the Officer and to the Manager in reports, so a clear reason matters.

**Where next**

To [TEC-02](#bookmark=id.8l487t8fl9xe).

### **DLG-10  Put Work on Hold**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Technician (Full) |
| **Where it comes from** | Put on hold on [TEC-03](#bookmark=id.dkkwhpnr6ifa) or [TEC-05](#bookmark=id.ff0ok48aee2q) (status In Progress). |

**Purpose**

The Technician pauses work, for example because a part or a permission is missing.

**Block layout**

![DLG-10 Put Work on Hold][image75]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Reason type** | Required. Waiting for parts, Waiting for access to the room, Waiting for a specialist or supplier, Other. |
| **Details** | Required, 10 to 300 characters. For parts: name and quantity needed. |
| **Expected date** | Required, today or later. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Put on hold** | Status becomes On Hold. The Officer and the Employee are notified. The SLA resolve timer is paused until the work continues. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **On hold past the expected date** | The Officer gets a reminder and the case appears in Needs attention lists. |

**Rules**

* The Technician continues with Resume on [TEC-03](#bookmark=id.dkkwhpnr6ifa).

**Where next**

To [TEC-03](#bookmark=id.dkkwhpnr6ifa).

### **DLG-11  Confirm Completion**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Technician (Full) |
| **Where it comes from** | Mark completed on [TEC-03](#bookmark=id.dkkwhpnr6ifa) or Complete inspection on [TEC-05](#bookmark=id.ff0ok48aee2q) after the required fields are valid. |

**Purpose**

A last check before the Technician reports the work as done.

**Block layout**

![DLG-11 Confirm Completion][image76]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Summary** | Root cause, total hours, number of parts and photos. |
| **Warning text** | "After this you cannot edit the report. The Officer will check it." |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Yes, completed** | Status becomes Completed. The Officer is notified. |
| **Go back** | Returns to the form. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Case changed while confirming** | "The case was updated. Please refresh." |

**Rules**

* Only the assigned Technician sees this dialog.

**Where next**

To [TEC-02](#bookmark=id.8l487t8fl9xe).

### **DLG-12  Cancel Request or Case**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Employee (Scoped), Officer (Full) |
| **Where it comes from** | Cancel on [COM-08](#bookmark=id.83jeomyy0mjj), [EMP-03](#bookmark=id.53a1f1joukxn) (Employee), or the row menu of [OFF-03](#bookmark=id.n0o9xiciqqd7) (Officer). |

**Purpose**

Stops a request. The Employee can cancel their own request before it is approved; an Officer can cancel a case at any point before Completed.

**Block layout**

![DLG-12 Cancel Request or Case][image77]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Consequences text** | Employee version: "This request will be closed and cannot be restored." Officer version adds: "The technician (name) will be told." |
| **Reason** | Required for the Officer, 10 to 300 characters. Optional for the Employee. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Yes, cancel** | Status becomes Cancelled (final). Involved people are notified. |
| **Keep it** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Employee cancels after approval** | The button is hidden. The Employee writes a comment to ask the Officer instead. |
| **Case changed while confirming** | The dialog closes with "Please refresh". |

**Rules**

* A cancelled case stays in the history and in reports.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-13  Report "Not Fixed"**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Employee (Full) |
| **Where it comes from** | Not fixed button on [COM-08](#bookmark=id.83jeomyy0mjj) (status Closed, within 7 days, once per case). |

**Purpose**

After a case is Closed, the Employee can say the problem is still there. The Officer decides what to do.

**Block layout**

![DLG-13 Report "Not Fixed"][image78]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **What is still wrong** | Required, 10 to 500 characters. |
| **Photos** | Optional, up to 3 photos, same rules as [EMP-02](#bookmark=id.v4er16ngh3i9). |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Send to Officer** | The case stays Closed but gets the tag "Reopen requested" and appears in the Reopen requests tab of [OFF-02](#bookmark=id.x3zeg4xiveou). Officers are notified. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **More than 7 days after closing** | The button is hidden. A note says "Please send a new request." |
| **Already requested once** | The button shows "Already sent" and is disabled. |

**Rules**

* The window length (7 days) can be changed in [ADM-07](#bookmark=id.6ynr8feqi2ao).

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-14  Rate the Service**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Employee (Full) |
| **Where it comes from** | Rate button on [COM-08](#bookmark=id.83jeomyy0mjj), [EMP-03](#bookmark=id.53a1f1joukxn), [EMP-01](#bookmark=id.gevp1q7fiais), or the notification after closing. |

**Purpose**

The Employee gives a rating after the case is closed, which helps the Manager see quality.

**Block layout**

![DLG-14 Rate the Service][image79]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Stars** | Required, 1 to 5, with words for each (Very poor to Excellent). |
| **Comment** | Optional, up to 500 characters. For 1 or 2 stars, a soft hint asks "What could be better?" |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Send rating** | Saves it; shown on the case. It cannot be changed later. |
| **Not now** | Closes the window; the Rate button stays available. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **No star chosen** | The Send button is disabled. |
| **Save fails** | Red text; the choice stays. |

**Rules**

* Rating is optional and never blocks anything.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-15  Building, Floor or Room Form**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Add or Edit on [AST-01](#bookmark=id.r397yhsl2mmw). |

**Purpose**

Creates or edits a place in the facility tree.

**Block layout**

![DLG-15 Building, Floor or Room Form][image80]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Parent place** | Shown as text and not editable (a building has none, a floor has a building, a room has a floor). |
| **Name** | Required, 1 to 60 characters, unique under the same parent. |
| **Code** | Required, up to 20 characters, unique in the system (for example A-302). |
| **Type and area** | For rooms only: Room or Common area, optional area in square meters. |
| **Notes** | Optional, up to 300 characters. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save** | Saves and updates the tree at once. |
| **Cancel** | Closes the window; [DLG-19](#bookmark=id.98dbp5b14xo5) if changed. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Duplicate name or code** | Red text "This name or code already exists." |
| **Save fails** | Red text in the dialog. |

**Rules**

* Changing a code also changes it on old records automatically.

**Where next**

To [AST-01](#bookmark=id.r397yhsl2mmw).

### **DLG-16  User Account Action**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Row menu on [ADM-02](#bookmark=id.84apprc917i0) or buttons on [ADM-04](#bookmark=id.czm9bezfxzhx). |

**Purpose**

Asks the Administrator to confirm a sensitive account action.

**Block layout**

![DLG-16 User Account Action][image81]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Reset password** | Creates a new temporary password, ends the user's sessions, and requires a change at next sign-in. Options: show it once, send by email. |
| **Unlock** | Removes the temporary lock after too many wrong passwords. |
| **Deactivate** | The user can no longer sign in; history stays. For a Technician with active work orders, the dialog lists them and blocks until they are reassigned. |
| **Reactivate** | The user can sign in again with their old password. |
| **Force sign-out** | Ends all sessions of the user at once. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Confirm** | Performs the action, shows a toast, and writes the Audit Log. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Blocked action** | Red text explains why and what to do first. |
| **Own account** | Deactivate and Force sign-out are not offered. |
| **Email fails after reset** | The temporary password is shown on screen once so it can be given by hand. |

**Rules**

* Every action is written to the Audit Log.

**Where next**

To [ADM-02](#bookmark=id.84apprc917i0) or [ADM-04](#bookmark=id.czm9bezfxzhx).

### **DLG-17  Confirm Retire or Delete**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Retire on [AST-02](#bookmark=id.vfpdk57qamqs) / [AST-03](#bookmark=id.j6n8nmgqzl9d), Deactivate or Delete on [AST-01](#bookmark=id.r397yhsl2mmw), Delete on [ADM-06](#bookmark=id.a8n26oa6i98t). |

**Purpose**

Protects important data: retiring an asset, deactivating or deleting a place, or deleting a category.

**Block layout**

![DLG-17 Confirm Retire or Delete][image82]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **What will happen** | Plain sentence, for example: "The asset will be hidden from forms. Its history is kept." |
| **Blocking list** | If there are open cases or active preventive plans on the asset (or in the place), they are listed, and the Confirm button is disabled. |
| **Type the code** | For retiring an asset or deleting a place, the Administrator types the code to confirm. Not needed for categories. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Confirm** | Performs the action and writes the Audit Log. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Blocked** | The list explains what must be finished first, with links (case codes are plain text). |
| **Code typed wrongly** | The Confirm button stays disabled. |

**Rules**

* Anything with history is retired or deactivated, never really deleted.

**Where next**

To [AST-01](#bookmark=id.r397yhsl2mmw), [AST-02](#bookmark=id.vfpdk57qamqs), [AST-03](#bookmark=id.j6n8nmgqzl9d) or [ADM-06](#bookmark=id.a8n26oa6i98t).

### **DLG-18  Change Asset Status**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Change status on [AST-02](#bookmark=id.vfpdk57qamqs) or [AST-03](#bookmark=id.j6n8nmgqzl9d). |

**Purpose**

The Officer records whether an asset is working, being repaired or out of service.

**Block layout**

![DLG-18 Change Asset Status][image83]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **New status** | Required. Active, Under repair, Out of service. (Retired is only done by the Administrator, see [DLG-17](#bookmark=id.24kwc21rauut).) |
| **Reason** | Required for Out of service, 10 to 300 characters. Optional for the others. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save** | Updates the status pill everywhere and writes the Audit Log. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Setting Active while a case is open** | A yellow note lists the open case and asks "Are you sure?" |

**Rules**

* Status is a label for information; it does not block Employees from reporting problems.

**Where next**

To [AST-02](#bookmark=id.vfpdk57qamqs) or [AST-03](#bookmark=id.j6n8nmgqzl9d).

### **DLG-19  Unsaved Changes**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Appears when the user clicks a menu link, Cancel or Back on a form that was changed but not saved. |

**Purpose**

Prevents losing typed data when the user leaves a form.

**Block layout**

![DLG-19 Unsaved Changes][image84]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Message** | "You have changes that are not saved." |
| **Two buttons** | Keep editing (the default, so Enter does not lose data) and Discard changes. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Keep editing** | Closes the window; the form stays. |
| **Discard changes** | Continues to where the user wanted to go. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Closing the browser tab** | The browser's own warning is shown instead (the system cannot change its wording). |

**Rules**

* Not shown for forms with nothing typed.

**Where next**

To where the user wanted to go, or stays.

### **DLG-20  Photo Viewer and Upload Problems**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Employee (Full), Officer (Full), Technician (Full), Manager (Full), Admin (Full) |
| **Where it comes from** | Click any photo thumbnail. Upload messages appear on the photo area of forms. |

**Purpose**

Shows photos large and explains problems when adding photos.

**Block layout**

![DLG-20 Photo Viewer and Upload Problems][image85]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Large photo** | Fits the window. Zoom by clicking or with the plus and minus buttons. |
| **Caption** | Label (Before, During, After, Request), uploaded by whom and when. |
| **Navigation** | Arrow buttons and the keyboard arrows; Esc closes. |
| **Download button** | Shown to Officer and Manager only. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Next / Previous** | Moves between photos of the same case. |
| **Close** | Returns to the page. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Wrong file type** | "Use JPG, PNG or WEBP." |
| **Too big** | "This photo is over 5 MB." |
| **Too many photos** | "You can add up to N photos here." |
| **Upload interrupted** | The thumbnail shows a red mark and a Retry link. |
| **Photo missing** | Grey tile "Image unavailable". |

**Rules**

* Limits: Employee 5 photos per request, Technician 10 per work order, 5 MB each, JPG, PNG or WEBP.

**Where next**

Back to the page.

### **DLG-21  Generate Preventive Work Order Now**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Generate now on [PM-01](#bookmark=id.hubvrybhov1x) or [PM-03](#bookmark=id.rfx2okd8binq). |

**Purpose**

Creates the next preventive work order early, without waiting for the schedule.

**Block layout**

![DLG-21 Generate Preventive Work Order Now][image86]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Assets** | All assets ticked by default; the Officer can untick some. |
| **Due date** | Required. Starts as today plus the plan's lead time. |
| **Technician** | Starts as the default technician of the plan; can be empty. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Generate** | Creates one work order per ticked asset, tagged Preventive (manual). The next automatic date does not move. |
| **Cancel** | Closes the window. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Asset already has an open work order of this plan** | That asset is greyed with "Already open" and cannot be ticked. |
| **Plan is paused or ended** | The button is not offered. |

**Rules**

* The action is written to the Audit Log.

**Where next**

To [PM-03](#bookmark=id.rfx2okd8binq).

### **DLG-22  Claim Conflict and Release**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Officer (Full) |
| **Where it comes from** | Claim, Release or Ask to release on [OFF-01](#bookmark=id.eui2u2y7lh0t), [OFF-02](#bookmark=id.x3zeg4xiveou) or [COM-08](#bookmark=id.83jeomyy0mjj). |

**Purpose**

Handles the moments when two Officers want the same case, or when a claim ends.

**Block layout**

![DLG-22 Claim Conflict and Release][image87]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Already taken** | "CASE-1042 is being handled by (name) since 10:20." Buttons: View only, Ask to release. |
| **Release my claim** | "Release CASE-1042? Other Officers will be able to take it." Buttons: Release, Keep. |
| **Claim expired** | "Your claim on CASE-1042 ended after 30 minutes without action." Button: Claim again. |
| **Claim limit reached** | "You already handle 5 cases. Finish or release one first." |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **View only** | Opens [COM-08](#bookmark=id.83jeomyy0mjj) without decision buttons. |
| **Ask to release** | Adds a comment for the Officer who holds the claim and notifies them. |
| **Release / Claim again** | Changes the claim and updates every list. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Both click at the same moment** | The system accepts the first click only; the second person sees "Already taken". |

**Rules**

* A claim only means "I am working on it"; it is not an assignment and has no effect on reports.

**Where next**

To [COM-08](#bookmark=id.83jeomyy0mjj).

### **DLG-23  Category Item Form**

| Type | Dialog |
| :---- | :---- |
| **Module** | Dialogs (DLG) |
| **Who can open it** | Admin (Full) |
| **Where it comes from** | Add item or Edit on [ADM-06](#bookmark=id.a8n26oa6i98t). |

**Purpose**

Adds or edits one item in a maintenance category list.

**Block layout**

![DLG-23 Category Item Form][image88]

*Block layout only, not the final design. Orange marks are buttons.*

**What is on the screen**

| Element | Description |
| :---- | :---- |
| **Name** | Required, 2 to 60 characters, unique in the list. |
| **Description** | Optional, up to 200 characters; shown as help in forms. |
| **Active switch** | On by default. |

**What the user can do**

| Action | What happens |
| :---- | :---- |
| **Save** | Saves and updates the list. |
| **Cancel** | Closes the window; [DLG-19](#bookmark=id.98dbp5b14xo5) if changed. |

**Situations and error cases**

| Situation | What the user sees |
| :---- | :---- |
| **Duplicate name** | "This name already exists." |
| **Save fails** | Red text in the dialog. |

**Rules**

* Renaming an item changes its name on old records too.

**Where next**

To [ADM-06](#bookmark=id.a8n26oa6i98t).

*— End of document —*















































































































































































