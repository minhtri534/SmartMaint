import { ROLE_LABELS, ROLES, type Role } from '@/shared/types/role'
import { Card, PageHeader } from '@/shared/ui'

/** What each role does and sees (SRS role matrix, Leader decisions 12/10 and 28/10). Read only. */
const ROLE_SCOPE: Record<Role, { does: string; sees: string }> = {
  customer: {
    does: 'Reports problems, follows them, cancels before review, rates the repair.',
    sees: 'Own requests only. Reports in the rented room or the common areas of that building.',
  },
  staff: {
    does: 'Reviews, approves or rejects requests, assigns technicians, verifies and closes work.',
    sees: 'The cases of the buildings assigned to them (primary or backup). No shared queue.',
  },
  technician: {
    does: 'Accepts or declines work, starts, pauses and completes it with the repair record.',
    sees: 'The work assigned to them, and cases they declined (read only).',
  },
  manager: {
    does: 'Approves maintenance plans; follows cases and statistics.',
    sees: 'Everything in the buildings assigned to them, read only for cases.',
  },
  admin: {
    does: 'Creates and manages accounts, SLA hours; reads the audit log.',
    sees: 'No case content (BR-11).',
  },
}

/** ADM-05 Roles and permissions (reference). The five roles are fixed in code; this page only explains them. */
export function RolesPage() {
  return (
    <div className="flex max-w-[860px] flex-col gap-5">
      <PageHeader
        title="Roles"
        description="Five fixed roles. A role is chosen when the account is created."
      />
      <Card padding="none">
        <ul className="flex flex-col">
          {ROLES.map((r) => (
            <li
              key={r}
              className="border-rule grid gap-1 border-b px-5 py-4 last:border-0 md:grid-cols-[180px_minmax(0,1fr)] md:gap-4"
            >
              <div className="font-medium">{ROLE_LABELS[r]}</div>
              <div className="text-sm">
                <p>{ROLE_SCOPE[r].does}</p>
                <p className="text-muted mt-1">{ROLE_SCOPE[r].sees}</p>
              </div>
            </li>
          ))}
        </ul>
      </Card>
    </div>
  )
}
