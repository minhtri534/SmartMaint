import { createBrowserRouter } from 'react-router'
import { AccountDetailPage, AccountFormPage, AccountListPage, RolesPage } from '@/features/accounts'
import { AuditLogPage } from '@/features/audit'
import { ChangePasswordPage, LoginPage, RequireAuth } from '@/features/auth'
import {
  CaseDetailPage,
  CaseQueuePage,
  MyRequestsPage,
  NewRequestPage,
  WorkOrdersPage,
} from '@/features/cases'
import { ForbiddenPage, NotFoundPage, RouteErrorPage } from '@/features/errors/pages/ErrorPages'
import { HomePage } from '@/features/home/pages/HomePage'
import { NotificationsPage } from '@/features/notifications'
import { SlaSettingsPage } from '@/features/settings'
import { AppLayout } from '@/shared/layout/AppLayout'
import { AuthLayout } from '@/shared/layout/AuthLayout'
import { paths } from './paths'

/**
 * Route tree. Rules:
 *  - Public pages go under AuthLayout without RequireAuth.
 *  - Signed-in pages go under AppLayout. Restrict by role with <RequireAuth roles={[...]}> on the route element,
 *    using the same roles as the API endpoint's [AuthorizeRoles] and the sidebar entry in navigation.ts.
 */
export const router = createBrowserRouter([
  {
    errorElement: <RouteErrorPage />,
    children: [
      {
        element: <AuthLayout />,
        children: [
          { path: paths.login, element: <LoginPage /> },
          {
            path: paths.changePassword,
            element: (
              <RequireAuth>
                <ChangePasswordPage />
              </RequireAuth>
            ),
          },
        ],
      },
      {
        element: (
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        ),
        children: [
          { path: paths.home, element: <HomePage /> },
          { path: paths.forbidden, element: <ForbiddenPage /> },
          // F3 — normal incident (role lists match navigation.ts and the future CasesController)
          {
            path: paths.myRequests,
            element: (
              <RequireAuth roles={['customer']}>
                <MyRequestsPage />
              </RequireAuth>
            ),
          },
          {
            path: paths.newRequest,
            element: (
              <RequireAuth roles={['customer']}>
                <NewRequestPage />
              </RequireAuth>
            ),
          },
          {
            path: paths.cases,
            element: (
              <RequireAuth roles={['staff', 'manager']}>
                <CaseQueuePage />
              </RequireAuth>
            ),
          },
          {
            path: paths.workOrders,
            element: (
              <RequireAuth roles={['technician']}>
                <WorkOrdersPage />
              </RequireAuth>
            ),
          },
          {
            path: paths.caseDetail,
            element: (
              <RequireAuth roles={['customer', 'staff', 'technician', 'manager']}>
                <CaseDetailPage />
              </RequireAuth>
            ),
          },
          // Core — every role (NotificationsController)
          { path: paths.notifications, element: <NotificationsPage /> },
          // Core — Administrator (AccountsController, AuditLogsController, PriorityLevelsController: Admin)
          ...[
            { path: paths.accounts, element: <AccountListPage /> },
            { path: paths.accountNew, element: <AccountFormPage mode="create" /> },
            { path: paths.accountDetail, element: <AccountDetailPage /> },
            { path: paths.accountEdit, element: <AccountFormPage mode="edit" /> },
            { path: paths.accountChangeRole, element: <AccountFormPage mode="change-role" /> },
            { path: paths.roles, element: <RolesPage /> },
            { path: paths.auditLog, element: <AuditLogPage /> },
            { path: paths.slaSettings, element: <SlaSettingsPage /> },
          ].map((route) => ({
            ...route,
            element: <RequireAuth roles={['admin']}>{route.element}</RequireAuth>,
          })),
        ],
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
